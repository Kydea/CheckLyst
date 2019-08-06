using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.PDF;
using CheckListManager.Common.Logic.Api.PDF.Sections;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Services;
using CheckListManager.Instances.Logic.Models;
using CheckListManager.Instances.Logic.Models.CheckLists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Instances.Logic.PDF
{
    public class InstancePDF : CheckListPDF
    {
        public InstancePDF(long CompanyID, string CompanyName, string TimeZone, string Repository, InstanceCacheObject Instance, IStorageService StorageService, IConfigurationService ConfigurationService) : base(CompanyID, CompanyName, TimeZone, Repository, Instance.Instance, StorageService, ConfigurationService)
        {
            this.Instance = Instance;
        }

        private InstanceCacheObject Instance { get; }

        public string FormatFormula(string Value)
        {
            return Value?.Replace("<span class='Null'></span>", string.Empty).Replace("<span class='Error'>", "[ #Error: ").Replace("</span>", " ]") ?? string.Empty;
        }

        public override string GetFieldValue(ICheckListField Field)
        {
            return FormatFormula(((Field)Field).RawValue);
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

            return new FirstPage(this, Instance.InstanceCode, Instance.PrototypeDescription, FormatFormula(Instance.Instance.Description), Rows);

        }

        protected override Template GenerateTemplate()
        {
            return new Template(this, $"{Instance.InstanceCode} - {Instance.InstanceState}");
        }

        public override bool IsPrototype() => false;
    }
}
