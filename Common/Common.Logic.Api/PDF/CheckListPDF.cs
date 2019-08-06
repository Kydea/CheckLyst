using System.Threading.Tasks;
using CheckListManager.Common.Logic.Api.PDF.Sections;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Services;

namespace CheckListManager.Common.Logic.Api.PDF
{
    public abstract class CheckListPDF : PDFBase
    {
        public long CompanyID { get; }
        public string CompanyName { get; }
        public string TimeZone { get; }
        public string Repository { get; }
        public ICheckList Model { get; }

        public IStorageService StorageService { get; }
        public IConfigurationService ConfigurationService { get; }

        public CheckListPDF(long CompanyID, string CompanyName, string TimeZone, string Repository, ICheckList Model, IStorageService StorageService, IConfigurationService ConfigurationService) : base("CheckList")
        {
            this.CompanyID = CompanyID;
            this.CompanyName = CompanyName;
            this.TimeZone = TimeZone;
            this.Repository = Repository;
            this.Model = Model;
            this.StorageService = StorageService;
            this.ConfigurationService = ConfigurationService;
        }

        protected abstract FirstPage GenerateFirstPage();

        protected abstract Task<Template> GenerateTemplateAsync();

        public abstract string GetFieldValue(ICheckListField Field);

        public abstract bool IsPrototype();

        protected override async Task OnGenerateAsync()
        {
            await GenerateFirstPage().RenderAsync();
            await AddSectionAsync();

            var Header = Model.Header;
            if (Header != null && Header is ICheckListGroup)
                await new Group(this, Header).RenderAsync();

            foreach (var Element in Model.Pages)
            {
                Container.AddPageBreak();
                await new Page(this, Element).RenderAsync();
            }
        }

        protected override Task OnNewSectionAsync() => GenerateTemplateAsync();

        public virtual void RenderGroup(ICheckListGroup Group)
        {
		}
    }
}
