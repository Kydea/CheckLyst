using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Api.PDF.Sections;

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
            PdfWriter.PageEvent = GenerateTemplate();
            Document.SetMargins(40, 40, 120, 120);

            GenerateFirstPage().Render();

            //Header
            var Header = Model.Header;
            if (Header != null && Header is ICheckListGroup)
                new Group(this, Header).Render();

            //Pages
            foreach (var Element in Model.Pages)
            {
                Document.NewPage();
                CurrentRenderingY = 0;
                new Page(this, Element).Render();
            }
        }

        public virtual void RenderGroup(ICheckListGroup Group)
        {

        }
    }
}