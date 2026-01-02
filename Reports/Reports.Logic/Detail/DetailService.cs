using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Reports.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Exceptions;
using CheckListManager.Common.Logic.Services;
using CheckListManager.Common.Logic.Trail;
using Newtonsoft.Json;
using System.Data;
using CheckListManager.Common.Data;
using System.Security.Principal;
using CheckListManager.Common.Data.ListItems;
using CheckListManager.Common.Logic.WorkFlows;
using static SAP.Middleware.Connector.RfcAbapClassException;

namespace CheckListManager.Reports.Logic.Detail
{
    public class DetailService : IDetailService
    {
        private readonly IReportsRepository _reportsRepository;
        private readonly IReportViewerRepository _reportViewerRepository;
        private readonly IWorkFlowsService _workFlowsService;
        private readonly ISettingService _settingService;
        private readonly IQueueService _queueService;
        private readonly ICommonRepository _commonRepository;

        public DetailService(
            IReportsRepository reportsRepository,
            IReportViewerRepository reportViewerRepository,
            IWorkFlowsService workFlowsService,
            ISettingService settingService,
            IQueueService queueService,
            ICommonRepository commonRepository
        )
        {
            _reportsRepository = reportsRepository;
            _reportViewerRepository = reportViewerRepository;
            _workFlowsService = workFlowsService;
            _settingService = settingService;
            _queueService = queueService;
            _commonRepository = commonRepository;
        }

        private void AddListError(Dictionary<string, ValidationError> List, int Index, string FieldName, string Message)
        {
            if (!List.TryGetValue($"item{Index}", out var ValidationError))
            {
                ValidationError = new ValidationError { Index = Index };
                List[$"item{Index}"] = ValidationError;
            }
            var Field = new Field();
            Field.ValidationMessages.Add(new ValidationMessage
            {
                Level = ValidationMessage.LevelError,
                Message = Message
            });
            ValidationError.Fields[FieldName] = Field;
        }

        private void AddQueryError(DetailResponse response, string message, int? position)
        {
            response.Fields["Query"] = new Field
            {
                ValidationMessages = new List<ValidationMessage> {
                    new ValidationMessage {
                        Level = ValidationMessage.LevelError,
                        Message = message,
                        Position = position
                    }
                }
            };

            response.Valid = false;
        }

        private async Task<DetailResponse> ValidateAsync(IDefaultIdentity identity, DetailRequest record)
        {
            var response = new DetailResponse();
            if (string.IsNullOrWhiteSpace(record.Code))
                response.Fields["Code"] = new Field
                {
                    ValidationMessages = new List<ValidationMessage> {
                        new ValidationMessage {
                            Level = ValidationMessage.LevelError,
                            Message = "Invalid code."
                        }
                    }
                };

            if (record.Columns == null || !record.Columns.Any())
                response.ValidationMessages.Add(new ValidationMessage
                {
                    Level = ValidationMessage.LevelError,
                    Message = "The report has no column."
                });
            else
                for (var Index = 0; Index < record.Columns.Count; ++Index)
                {
                    var Column = record.Columns[Index];
                    if (string.IsNullOrWhiteSpace(Column.Label))
                        AddListError(response.ColumnErrors, Index, "Label", "Invalid label.");

                    if (string.IsNullOrWhiteSpace(Column.Column))
                        AddListError(response.ColumnErrors, Index, "Column", "Invalid column.");
                }

            if (!response.ColumnErrors.Any()) response.ColumnErrors = null;

            if (record.Clauses != null)
            {
                var ParameterNames = new HashSet<string>();
                for (var Index = 0; Index < record.Clauses.Count; ++Index)
                {
                    var Clause = record.Clauses[Index];
                    if (string.IsNullOrWhiteSpace(Clause.Label))
                        AddListError(response.ClauseErrors, Index, "Label", "Invalid label.");

                    if (string.IsNullOrWhiteSpace(Clause.ParameterName))
                        AddListError(response.ClauseErrors, Index, "ParameterName", "Invalid parameter name.");
                    else if (ParameterNames.Contains(Clause.ParameterName))
                        AddListError(response.ClauseErrors, Index, "ParameterName", $"Parameter name {Clause.ParameterName} is duplicated.");
                    else
                        ParameterNames.Add(Clause.ParameterName);

                    if (Clause.ClauseType == "Custom" && string.IsNullOrWhiteSpace(Clause.Clause.Custom))
                        AddListError(response.ClauseErrors, Index, "Custom", "Invalid clause.");

                    //rimuovere prototypes
                    if ((Clause.ParameterType == "Prototypes" || Clause.ClauseType == "Filter") && string.IsNullOrWhiteSpace(Clause.Clause.Column))
                        AddListError(response.ClauseErrors, Index, "Column", "Invalid column.");
                }
            }

            if (!response.ClauseErrors.Any()) response.ClauseErrors = null;

            response.Valid = !response.ValidationMessages.Any() && !response.Fields.Any() && response.ColumnErrors == null && response.ClauseErrors == null;

            if (record.ReportSource == null)
                try
                {
                    await _reportViewerRepository.GetDataTableAsync(identity.CompanyId, record.Query, record.Clauses?.Where(c => c.ClauseType == "parameter").ToDictionary(k => k.ParameterName, v => v.ParameterType));
                }
                catch (DatabaseException databaseException)
                {
                    AddQueryError(response, databaseException.Message, databaseException.Position);
                }
                catch (Exception Exception)
                {
                    AddQueryError(response, Exception.Message, null);
                }

            return response;
        }

        public async Task<DetailResponse> LoadReportAsync(long company, DetailRequest request)
        {
            var response = new DetailResponse();

            try
            {
                var report = await _reportsRepository.GetReportAsync(company, request.ID.Value, null);
                var workFlows = (await _reportsRepository.GetWorkFlowsAsync(company)).Select(wf => new Option(wf.ID.ToString(), wf.Description));
                var lastVersion = await _reportsRepository.GetLastVersionAsync(company, report.Code);

                response.ID = report.ID;
                response.Code = report.Code;
                response.Version = report.Version;
                response.State = report.ReportState;
                response.ProposedState = report.ProposedState;
                response.ProposedDate = report.ProposedDate;
                response.Editable = report.ReportState == "Draft";
                response.Fields["id"] = new Field { Value = report.ID.ToString(), Editable = false };
                response.Fields["code"] = new Field { Value = report.Code, Editable = false };
                response.Fields["description"] = new Field { Value = report.Description, Editable = response.Editable };
                response.Fields["note"] = new Field { Value = report.Note, Editable = response.Editable };
                response.Fields["reportSource"] = new Field { Value = report.ReportSource, Editable = response.Editable };
                response.Fields["workFlow"] = new Field { Value = report.WorkFlow?.ToString(), Options = workFlows, Editable = response.Editable && (lastVersion == null || !lastVersion.WorkFlow.HasValue) };
                response.Fields["query"] = new Field { Value = report.Query, Editable = response.Editable };
                response.Fields["reportState"] = new Field { Value = report.ReportState, Editable = false };
                response.Fields["restricted"] = new Field { Value = report.Restricted ? "true" : "false", Editable = response.Editable };
                response.Fields["version"] = new Field { Value = report.Version.ToString(), Editable = false };
                response.CreatedBy = report.CreatedBy;
                response.CreatedOn = report.CreatedOn;
                response.ModifiedBy = report.ModifiedBy;
                response.ModifiedOn = report.ModifiedOn;

                /*
                response.Fields["instancesRangeRestricted"] = new Field { Value = report.InstancesRangeRestricted.ToString() };
                response.Fields["instancesIDAlias"] = new Field { Value = report.InstancesIDAlias };
                response.Fields["prototypesRangeRestricted"] = new Field { Value = report.PrototypesRangeRestricted.ToString() };
                response.Fields["prototypesIDAlias"] = new Field { Value = report.PrototypesIDAlias };
                */

                response.Groups = report.Groups.Select(g => g.Code).ToList();

                response.Columns = report.ReportColumns.OrderBy(c => c.ID).Select(c => new ReportColumn
                {
                    Label = c.Label,
                    Column = c.Column,
                    Format = c.Format,
                    Visible = c.Visible,
                    GroupColumn = c.GroupColumn,
                    OrderColumn = c.OrderColumn,
                    GroupAggregation = c.GroupAggregation,
                    ValueAggregation = c.ValueAggregation
                }).ToList();

                response.Clauses = report.ReportClauses.OrderBy(c => c.ID).Select(c => new ReportClause
                {
                    Label = c.Label,
                    Clause = JsonConvert.DeserializeObject<ReportClause.ClauseRecord>(c.Clause),
                    Visible = c.Visible,
                    Editable = c.Editable,
                    ParameterName = c.ParameterName,
                    ParameterType = c.ParameterType,
                    ClauseType = c.ClauseType
                }).ToList();
            }
            catch (Exception exception)
            {
                response.ValidationMessages.Add(new ValidationMessage
                {
                    Level = ValidationMessage.LevelError,
                    Message = exception.Message
                });
                response.Valid = false;
            }

            return response;
        }

        public async Task<DetailResponse> SaveReportAsync(IDefaultIdentity identity, DetailRequest request)
        {
            var response = await ValidateAsync(identity, request);
            if (response.Valid)
                try
                {
                    var report = new Common.Data.Reports
                    {
                        Code = request.Code,
                        Description = request.Description,
                        ReportSource = request.ReportSource,
                        Query = request.Query,
                        InstancesRangeRestricted = request.InstancesRangeRestricted,
                        InstancesIDAlias = request.InstancesIDAlias,
                        PrototypesRangeRestricted = request.PrototypesRangeRestricted,
                        PrototypesIDAlias = request.PrototypesIDAlias,
                        Note = request.Note,
                        WorkFlow = request.WorkFlow,
                        Restricted = request.Restricted,
                        ReportColumns = request.Columns?.Select(c => new ReportColumns
                        {
                            Label = c.Label,
                            Column = c.Column,
                            Visible = c.Visible,
                            Format = c.Format,
                            GroupColumn = c.GroupColumn,
                            OrderColumn = c.OrderColumn,
                            ValueAggregation = c.ValueAggregation,
                            GroupAggregation = c.GroupAggregation
                        }).ToList(),
                        ReportClauses = request.Clauses?.Select(c => new ReportClauses
                        {
                            Label = c.Label,
                            Clause = JsonConvert.SerializeObject(c.Clause),
                            Visible = c.Visible,
                            Editable = c.Editable,
                            ParameterName = c.ParameterName,
                            ParameterType = c.ParameterType,
                            ClauseType = c.ClauseType,
                        }).ToList()
                    };

                    var lastVersion = await _reportsRepository.GetLastVersionAsync(identity.CompanyId, request.Code);
                    if (request.NewVersion)
                    {
                        if (request.Version != lastVersion.Version + 1) throw new Exception("The report version is not the last one.");
                        request.ID = null;
                    }

                    if (lastVersion.WorkFlow.HasValue)
                        request.WorkFlow = lastVersion.WorkFlow;

                    if (request.ID.HasValue)
                    {
                        report.ID = request.ID.Value;
                        report = await _reportsRepository.EditReportAsync(identity, report, request.Groups);
                        if (report.ReportState != ReportState.Draft)
                            throw new Exception("The report is not in state draft");
                        response.ModifiedOn = report.ModifiedOn;
                        response.ModifiedBy = report.ModifiedBy;
                        await AuditAsync(identity, TrailAction.ReportUpdated, request);
                    }
                    else
                    {
                        await _reportsRepository.AddReportAsync(identity, report, request.Groups, lastVersion == null ? 1 : lastVersion.Version + 1);
                        request.ID = response.ID = report.ID;
                        if (request.NewVersion) lastVersion.ReportState = ReportState.Obsolete;
                        await _reportsRepository.SaveChangesAsync();
                        await AuditAsync(identity, TrailAction.ReportCreated, request);
                    }
                }
                catch (Exception Exception)
                {
                    response.ValidationMessages.Add(new ValidationMessage
                    {
                        Level = ValidationMessage.LevelError,
                        Message = Exception.Message
                    });
                    response.Valid = false;
                }

            return response;
        }

        public async Task<DetailResponse> DeleteReportAsync(IDefaultIdentity identity, DetailRequest request)
        {
            var response = new DetailResponse();
            try
            {
                await _reportsRepository.DeleteReportAsync(identity, request.ID.Value);
                await AuditAsync(identity, TrailAction.ReportDeleted, request.ID.Value, request.Reason);
            }
            catch (Exception Exception)
            {
                response.ValidationMessages.Add(new ValidationMessage
                {
                    Level = ValidationMessage.LevelError,
                    Message = Exception.Message
                });
                response.Valid = false;
            }

            return response;
        }

        public async Task<DetailResponse> VerifyReportAsync(IDefaultIdentity identity, DetailRequest request) =>
            await ValidateAsync(identity, request);

        public async Task<DetailResponse> SetStateAsync(IDefaultIdentity identity, DetailRequest request)
        {
            var report = await _reportsRepository.GetReportAsync(identity.CompanyId, request.ID.Value, null);
            if (request.State == ReportState.Obsolete && report.ReportState == ReportState.Confirmed && report.WorkFlow == null)
                report.ReportState = ReportState.Obsolete;
            else if (request.State == ReportState.Obsolete && report.ReportState == ReportState.Confirmed && report.WorkFlow != null)
            {
                report.ProposedState = ReportState.Obsolete;
                report.ProposedDate = DateTimeOffset.UtcNow;

                await _workFlowsService.CreateWorkFlowApplicationAsync(identity, new CreateRequest
                {
                    WorkFlow = report.WorkFlow.Value,
                    Report = report.ID,
                    Action = "Report – state to Obsolete"
                });

            }
            else if (request.State == ReportState.Completed && report.ReportState == ReportState.Draft && report.WorkFlow == null)
                report.ReportState = ReportState.Confirmed;
            else if (request.State == ReportState.Completed && report.ReportState == ReportState.Draft && report.WorkFlow != null)
            {
                report.ReportState = ReportState.Completed;
                report.ProposedState = ReportState.Confirmed;
                report.ProposedDate = DateTimeOffset.UtcNow;

                await _workFlowsService.CreateWorkFlowApplicationAsync(identity, new CreateRequest
                {
                    WorkFlow = report.WorkFlow.Value,
                    Report = report.ID,
                    Action = "Report – state to Confirmed"
                });

            }
            else 
                throw new InvalidOperationException($"State change from {report.ReportState} to {request.State} not allowed");


            report.ModifiedBy = identity.Code;
            report.ModifiedOn = DateTimeOffset.UtcNow;

            await _reportsRepository.SaveChangesAsync();
            await AuditAsync(identity, TrailAction.ReportUpdated, request);
            return await LoadReportAsync(identity.CompanyId, request);
        }

        public async Task<List<ReportSource>> GetReportSourcesAsync() =>
            (await _reportsRepository.GetReportSourcesAsync()).Select(rs => new ReportSource
            {
                Code = rs.Code,
                Query = rs.Query
            }).ToList();

        public async Task<List<string>> GetColumnsAsync(IDefaultIdentity identity, string query, Dictionary<string, string> parameters)
        {
            using (var dataTable = await _reportViewerRepository.GetDataTableAsync(identity.CompanyId, query, parameters))
                return dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
        }

        private Task AuditAsync(IDefaultIdentity identity, string action, DetailRequest record)
        {
            var information = new
            {
                record.ID,
                record.Code,
                record.Description,
                record.ReportSource,
                record.Query,
                //record.InstancesRangeRestricted,
                //record.InstancesIDAlias,
                //record.PrototypesRangeRestricted,
                //record.PrototypesIDAlias,
                record.Note,
                record.WorkFlow,
                record.Restricted,
                ReportGroups = record.Groups,
                ReportColumns = record.Columns,
                ReportClauses = record.Clauses,
                ReasonForChange = record.Reason
            };
            return _commonRepository.AuditAsync(identity, action, information);
        }

        private Task AuditAsync(IDefaultIdentity identity, string action, long id, string reason)
        {
            var information = new
            {
                ID = id,
                ReasonForChange = reason
            };
            return _commonRepository.AuditAsync(identity, action, information);
        }
    }
}
