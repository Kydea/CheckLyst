using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.PDF;
using CheckListManager.Common.Logic.Api.PDF.Sections;
using CheckListManager.Common.Logic.Models;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Services;
using CheckListManager.Instances.Data;
using CheckListManager.Instances.Logic.Models;
using CheckListManager.Instances.Logic.Models.CheckLists;
using CheckListManager.Instances.Logic.Services;

namespace CheckListManager.Instances.Logic.PDF
{
    public class InstancePDF : CheckListPDF
    {
        public InstancePDF(long CompanyID, string CompanyName, string TimeZone, string Repository, InstanceCacheObject Instance, IInstancesRepository InstancesRepository, IStorageService StorageService, IConfigurationService ConfigurationService) : base(CompanyID, CompanyName, TimeZone, Repository, Instance.Instance, StorageService, ConfigurationService)
        {
            this.Instance = Instance;
            this.InstancesRepository = InstancesRepository;
        }

        private InstanceCacheObject Instance { get; }
        private IInstancesRepository InstancesRepository { get; }

        public string FormatFormula(string Value)
        {
            return Value?.Replace("<span class='Null'></span>", string.Empty).Replace("<span class='Error'>", "[ #Error: ").Replace("</span>", " ]") ?? string.Empty;
        }

        public override string GetFieldValue(ICheckListField CheckListField)
        {
            var Field = (Field)CheckListField;
            if (Field.ValueTypeName == "DateTime")
                return string.IsNullOrEmpty(Field.Value) ? string.Empty : Field.Value + DateTimeOffsetHelper.FormatOffset(Field.LocalTimeZone ? Field.Offset : 0);
            else
                return FormatFormula(Field.Value);
        }

        public override string GetItemDescription(ICheckListItem CheckListItem)
        {
            var Item = (Models.CheckLists.Item)CheckListItem;
            return Item.IterationItem ?
                FormatFormula(Item.FindIteratorModelItem().DescriptionFormula?.Value ?? Item.DescriptionFormula?.Value) :
                FormatFormula(Item.DescriptionFormula?.Value);
        }

        protected override FirstPage GenerateFirstPage()
        {
            var Rows = new List<Tuple<string, string>> {
                new Tuple<string, string> ("Plant", CompanyName),
                new Tuple<string, string> ("Department", Instance.DepartmentName),
                new Tuple<string, string> ("Prototype type", Instance.PrototypeTypeName),
                new Tuple<string, string> ("Prototype name", Instance.PrototypeName),
                new Tuple<string, string> ("Prototype version", Instance.Version.ToString()),
                new Tuple<string, string> ("Instance code", Instance.InstanceCode),
                new Tuple<string, string> ("Instance state", Instance.InstanceState),
                new Tuple<string, string> ("Instance creation", Instance.CreatedOn.ToLocalTime(TimeZone).ToString("yyyy-MM-dd HH:mm UTC zzz")),
                new Tuple<string, string> ("Instance confirmation", Instance.InstanceState == InstanceState.Confirmed ? Instance.ModifiedOn?.ToLocalTime(TimeZone).ToString("yyyy-MM-dd HH:mm UTC zzz") : "N/A")
            };

            var Exceptions = new StringBuilder();
            foreach (var Expression in Instance.Instance.Expressions.Values)
                if (
                    Expression.Exception
                    && (
                        Expression.Value.Type == DataType.Boolean && Expression.Value.BooleanValue
                        || Expression.Value.Type == DataType.Date && Expression.Value.DateTimeValue <= DateTimeOffset.UtcNow
                        || Expression.Value.Type == DataType.DateTime && Expression.Value.DateTimeValue <= DateTimeOffset.UtcNow
                    )
                )
                {
                    Exceptions.AppendLine(Expression.Description);
                    Exceptions.AppendLine();
                }

            return new FirstPage(this, Instance.InstanceCode, Instance.PrototypeDescription, FormatFormula(Instance.Instance.DescriptionFormula.Value), Rows, Exceptions.Length > 0 ? Exceptions.ToString() : null)
            {
                ColumnsWidths = new float[] { 23.66f, 100 - 23.66f }
            };
        }

        protected override Task<Template> GenerateTemplateAsync()
        {
            var template = new Template(this, $"{Instance.InstanceCode} - {Instance.InstanceState}");
            return Task.FromResult(template);
        }

        protected override async Task OnGenerateAsync()
        {
            await base.OnGenerateAsync();

            if (Instance.InstanceState == InstanceState.Confirmed || Instance.InstanceState == InstanceState.Completed || Instance.InstanceState == InstanceState.Deleted)
            {
                var Record = await InstancesRepository.GetAsync(Instance.CompanyId, Instance.ID.Value);

                var Signatures = Record
                    .InstancesHistory
                    .LastOrDefault(h => h.State == InstanceState.Completed && h.WorkFlowApplication.HasValue)?
                    .WorkFlowApplications?
                    .WorkFlowApplicationSteps
                    .OrderBy(h => h.Step)
                    .ThenBy(h => h.ID)
                    .ToList();

                GenerateSignatureBook(Signatures);
            }
        }

        public override bool IsPrototype() => false;
    }
}
