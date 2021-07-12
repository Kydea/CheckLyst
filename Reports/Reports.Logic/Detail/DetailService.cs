using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Reports.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Queue;
using CheckListManager.Common.Logic.Services;
using CheckListManager.Common.Logic.Trail;
using Newtonsoft.Json;
using System.Data;
using CheckListManager.Common.Data.Exceptions;

namespace CheckListManager.Reports.Logic.Detail
{
    public class DetailService : IDetailService
    {
        private readonly IReportsRepository _reportsRepository;
        private readonly IReportViewerRepository _reportViewerRepository;
        private readonly ISettingService _settingService;
        private readonly IQueueService _queueService;

        public DetailService(
            IReportsRepository reportsRepository,
            IReportViewerRepository reportViewerRepository,
            ISettingService settingService,
            IQueueService queueService
        )
        {
            _reportsRepository = reportsRepository;
            _reportViewerRepository = reportViewerRepository;
            _settingService = settingService;
            _queueService = queueService;
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
            var requireUserReason = await _settingService.RequireUserReasonAsync(identity.CompanyId);

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

            if (requireUserReason && string.IsNullOrEmpty(record.Reason))
            {
                response.Fields["Reason"] = new Field
                {
                    ValidationMessages = new List<ValidationMessage>
                    {
                        new ValidationMessage
                        {
                            Level = ValidationMessage.LevelError,
                            Message = "Reason is a required field." +
                            ""
                        }
                    }
                };
            }

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
                for (var Index = 0; Index < record.Clauses.Count; ++Index)
                {
                    var Clause = record.Clauses[Index];
                    if (string.IsNullOrWhiteSpace(Clause.Label))
                        AddListError(response.ClauseErrors, Index, "Label", "Invalid label.");

                    if (string.IsNullOrWhiteSpace(Clause.ParameterName))
                        AddListError(response.ClauseErrors, Index, "ParameterName", "Invalid parameter name.");

                    if (Clause.ClauseType == "Custom" && string.IsNullOrWhiteSpace(Clause.Clause.Custom))
                        AddListError(response.ClauseErrors, Index, "Custom", "Invalid clause.");

                    //rimuovere prototypes
                    if ((Clause.ParameterType == "Prototypes" || Clause.ClauseType == "Filter") && string.IsNullOrWhiteSpace(Clause.Clause.Column))
                        AddListError(response.ClauseErrors, Index, "Column", "Invalid column.");
                }

            if (!response.ClauseErrors.Any()) response.ClauseErrors = null;

            response.Valid = !response.ValidationMessages.Any() && !response.Fields.Any() && response.ColumnErrors == null && response.ClauseErrors == null;

            if (record.ReportSource == null)
                try
                {
                    await _reportViewerRepository.GetDataTableAsync(identity.CompanyId, record.Query, record.Clauses?.Where(c => c.ClauseType == "parameter").ToDictionary(k => k.ParameterName, v => v.ParameterType));
                }
                catch (DatabaseException DatabaseException)
                {
                    AddQueryError(response, DatabaseException.Message, DatabaseException.Position);
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
                response.Fields["id"] = new Field { Value = report.ID.ToString(), Editable = false };
                response.Fields["code"] = new Field { Value = report.Code, Editable = false };
                response.Fields["description"] = new Field { Value = report.Description };
                response.Fields["reportSource"] = new Field { Value = report.ReportSource };
                response.Fields["query"] = new Field { Value = report.Query };
                response.Fields["online"] = new Field { Value = report.Online ? "true" : "false" };
                response.Fields["restricted"] = new Field { Value = report.Restricted ? "true" : "false" };
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
                        Online = request.Online,
                        Restricted = request.Restricted,
                        ReportColumns = request.Columns?.Select(c => new Common.Data.ReportColumns
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
                        ReportClauses = request.Clauses?.Select(c => new Common.Data.ReportClauses
                        {
                            Label = c.Label,
                            Clause = JsonConvert.SerializeObject(c.Clause),
                            Visible = c.Visible,
                            Editable = c.Editable,
                            ParameterName = c.ParameterName,
                            ParameterType = c.ParameterType,
                            ClauseType = c.ClauseType
                        }).ToList()
                    };
                    if (request.ID.HasValue)
                    {
                        report.ID = request.ID.Value;
                        report = await _reportsRepository.EditReportAsync(identity, report, request.Groups);
                        response.ModifiedOn = report.ModifiedOn;
                        response.ModifiedBy = report.ModifiedBy;
                        await AuditAsync(identity, TrailAction.ReportUpdated, request);
                    }
                    else
                    {
                        await _reportsRepository.AddReportAsync(identity, report, request.Groups);
                        request.ID = response.ID = report.ID;
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

        public async Task<List<ReportSource>> GetReportSourcesAsync() =>
            (await _reportsRepository.GetReportSourcesAsync()).Select(rs => new ReportSource
            {
                Code = rs.Code,
                Query = rs.Query
            }).ToList();

        public async Task<List<string>> GetColumnsAsync(IDefaultIdentity identity, string query, Dictionary<string, string> parameters)
        {
            using (var dataTable = await _reportViewerRepository.GetDataTableAsync(identity.CompanyId, query, parameters))
            {
                var reportColumns = new List<string>(dataTable.Columns.Count);
                foreach (DataColumn column in dataTable.Columns)
                    reportColumns.Add(column.ColumnName);

                return reportColumns;
            }
        }

        private Task AuditAsync(IDefaultIdentity identity, string action, DetailRequest record)
        {
            var information = new
            {
                record.Code,
                record.Description,
                record.ReportSource,
                record.Query,
                record.InstancesRangeRestricted,
                record.InstancesIDAlias,
                record.PrototypesRangeRestricted,
                record.PrototypesIDAlias,
                record.Online,
                record.Restricted,
                ReportGroups = record.Groups,
                ReportColumns = record.Columns,
                ReportClauses = record.Clauses,
                ReasonForChange = record.Reason
            };
            return AuditAsync(identity, action, information);
        }

        private Task AuditAsync(IDefaultIdentity identity, string action, long id, string reason)
        {
            var information = new
            {
                ID = id,
                ReasonForChange = reason
            };
            return AuditAsync(identity, action, information);
        }

        private Task AuditAsync(IDefaultIdentity identity, string action, object information)
        {
            return _queueService.SendObjectAsync(QueueName.Trail, new TrailMessage
            {
                Company = identity.CompanyId,
                User = identity.Code,
                Action = action,
                Date = DateTimeOffset.UtcNow,
                Information = information.ToJson(new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None })
            });
        }
    }
}
