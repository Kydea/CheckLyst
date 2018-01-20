using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Api.PDF;
using CheckListManager.Common.Logic.Api.PDF.Sections;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Prototypes.Logic.Messages;
using CheckListManager.Prototypes.Logic.Models.CheckLists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Prototypes.Logic.PDF
{
    public class PrototypePDF : CheckListPDF
    {
        public PrototypePDF(long CompanyID, string CompanyName, string TimeZone, string Repository, PrototypeResponse Prototype) : base(CompanyID, CompanyName, TimeZone, Repository, Prototype.CheckList)
        {
            this.Prototype = Prototype;
        }

        private PrototypeResponse Prototype { get; }

        protected override FirstPage GenerateFirstPage()
        {
            return new FirstPage(this, $"{Prototype.Name}",  $"Version {Prototype.Version}", Prototype.Description, new List<Tuple<string, string>> {
                new Tuple<string, string>("Department", Prototype.DepartmentName),
                new Tuple<string, string>("Prototype type", Prototype.PrototypeTypeName),
                new Tuple<string, string>("State", Prototype.State),
                new Tuple<string, string>("Valid from", Prototype.ValidFrom?.ToLocalTime(TimeZone).ToString("dd/MM/yyyy HH:mm")),
                new Tuple<string, string>("Valid until", Prototype.ValidUntil?.ToLocalTime(TimeZone).ToString("dd/MM/yyyy HH:mm"))
            });
        }

        protected override Template GenerateTemplate() => new Template(this, $"{Prototype.DepartmentName}.{Prototype.Version}\n{Prototype.Description}");

        public override string GetFieldValue(ICheckListField Field) => "_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _";

        public override bool IsPrototype() => false;
    }
}
