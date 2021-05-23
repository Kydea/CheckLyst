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
                            Message = "Invalid code"
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
                            Message = "Reason is a required field"
                        }
                    }
                };
            }

            response.Valid = response.Fields.Count == 0;
            return response;
        }

        public async Task<DetailResponse> LoadReportAsync(long company, DetailRequest request)
        {
            var response = new DetailResponse();

            try
            {
                var report = await _reportsRepository.GetReportAsync(company, request.Code);
                response.Fields["id"] = new Field { Value = report.ID.ToString(), Editable = false };
                response.Fields["code"] = new Field { Value = report.Code, Editable = false };
                response.Fields["description"] = new Field { Value = report.Description };
                response.Fields["reportSource"] = new Field { Value = report.ReportSource };
                response.Fields["query"] = new Field { Value = report.Query };
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

                response.Columns = report.ReportColumns.Select(c => new ReportColumn
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

                response.Clauses = report.ReportClauses.Select(c => new ReportClause
                {
                    Label = c.Label,
                    Clause = c.Clause,
                    Visible = c.Visible,
                    Editable = c.Editable
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
                            Clause = c.Clause,
                            Visible = c.Visible,
                            Editable = c.Editable
                        }).ToList()
                    };
                    if (request.ID.HasValue)
                    {
                        await _reportsRepository.EditReportAsync(identity, report);
                        await AuditAsync(identity, TrailAction.ReportUpdated, request);
                    }
                    else
                    {
                        await _reportsRepository.AddReportAsync(identity, report);
                        request.ID = request.ID;
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
                await _reportsRepository.DeleteReportAsync(identity, request.Code);
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

        public async Task<List<ReportSource>> GetReportSourcesAsync() =>
            (await _reportsRepository.GetReportSourcesAsync()).Select(rs => new ReportSource
            {
                Code = rs.Code,
                Query = rs.Query
            }).ToList();

        public async Task<List<string>> GetColumnsAsync(IDefaultIdentity identity, string query)
        {
            using (var dataTable = await _reportViewerRepository.GetDataTable(identity.CompanyId, query))
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
