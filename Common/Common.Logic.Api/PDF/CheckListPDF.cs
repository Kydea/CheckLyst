using CheckListManager.Common.Logic.Api.PDF.Helper;
using CheckListManager.Common.Logic.Api.PDF.Sections;
using CheckListManager.Common.Logic.Models.CheckLists;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using PdfSharp.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Api.PDF
{
    public abstract class CheckListPDF : PDFBase
    {
        public long CompanyID { get; }
        public string CompanyName { get; }
        public string TimeZone { get; }
        public string Repository { get; }
        public ICheckList Model { get; }

        public string PrototypeDescription = "", InstanceCode = "";

        public CheckListPDF(long CompanyID, string CompanyName, string TimeZone, string Repository, ICheckList Model) : base("CheckList")
        {
            this.CompanyID = CompanyID;
            this.CompanyName = CompanyName;
            this.TimeZone = TimeZone;
            this.Repository = Repository;
            this.Model = Model;
        }

        protected abstract FirstPage GenerateFirstPage();

        protected abstract Template GenerateTemplate();

        public string GetFieldValue(Item Item) => GetFieldValue((ICheckListField)Item.Element);

        public abstract string GetFieldValue(ICheckListField Field);

        public abstract bool IsPrototype();

        protected override void OnGenerate()
        {
            GenerateFirstPage().Render();
            AddSection();

            var Header = Model.Header;
            if (Header != null && Header is ICheckListGroup)
                new Group(this, Header).Render();

            foreach (var Element in Model.Pages)
            {
                Container.AddPageBreak();
                new Page(this, Element).Render();
            }
        }

        protected override void OnNewSection() => GenerateTemplate();

        public virtual void RenderGroup(ICheckListGroup Group)
        {
		}
    }
}
