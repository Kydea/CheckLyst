using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.DataSources.Logic.Detail;
using CheckListManager.DataSources.Logic.List;
using CheckListManager.DataSources.Tests.Contexts;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace CheckListManager.DataSources.Tests.Steps
{
    [Binding]
    public class DataSourcesListSteps
    {
        private readonly DataSourcesContext _context;
        private readonly IListService _listService;
        private readonly IDetailService _detailService;
        private readonly IDefaultIdentity _defaultIdentity;

        public DataSourcesListSteps(
            DataSourcesContext context,
            IListService listService,
            IDetailService detailService,
            IDefaultIdentity defaultIdentity)
        {
            _context = context;
            _listService = listService;
            _detailService = detailService;
            _defaultIdentity = defaultIdentity;
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            _context.Request = new DetailRequest();
        }

        [AfterScenario]
        public void AfterScenario()
        {
            _context.Request = null;
        }

        #region GIVEN
        [Given(@"I assign '(.*)' as the code of the Data Source")]
        public void GivenIAssignAsTheCodeOfTheDataSource(string Code)
        {
            _context.Request.Code = Code;
        }

        [Given(@"I assign '(.*)' as the class of the Data Source")]
        public void GivenIAssignAsTheClassOfTheDataSource(string Class)
        {
            _context.Request.Class = Class;
        }

        [Given(@"I assign '(.*)' as the description of the Data Source")]
        public void GivenIAssignAsTheDescriptionOfTheDataSource(string Description)
        {
            _context.Request.Description = Description;
        }

        [Given(@"I click on a Data Source")]
        public async Task GivenIClickOnADataSource()
        {
            List<ListRow> data = (await _listService.GetDataAsync(_defaultIdentity.CompanyId, new PagingRequest<ListRequest>
            {
                Data = new ListRequest
                {
                    Code = String.Empty,
                    Description = String.Empty
                }
            })).Data;

            Assert.NotZero(data.Count(), "At least one element must exist in the list.");

            var element = data.First();

            _context.Request.Code = element.Code;
            _context.Request.Class = element.Class;
            _context.Request.Description = element.Description;
        }
        #endregion

        #region WHEN
        [When(@"I submit the request to add the Data Source")]
        public async Task WhenISubmitTheRequestToAddTheDataSource() 
            => _context.Response = await _detailService.AddAsync(_defaultIdentity, _context.Request);

        [When(@"I submit the request to edit the Data Source")]
        public async Task WhenISubmitTheRequestToEditTheDataSource() 
            => _context.Response = await _detailService.EditAsync(_defaultIdentity, _context.Request);

        [When(@"I submit the request to delete the Data Source")]
        public async Task WhenISubmitTheRequestToDeleteTheDataSource()
            => _context.Response = await _detailService.DeleteAsync(_defaultIdentity, _context.Request);
        #endregion

        #region THEN
        [Then(@"the system does not add the Data Source and returns an error message: '(.*)'")]
        public void ThenTheSystemDoesNotAddTheDataSourceAndReturnsAnErrorMessage(string Message)
        {
            Assert.IsFalse(_context.Response.Valid, "The response must be invalid.");
            Assert.IsTrue(_context.Response.Fields.ContainsKey("Code") && _context.Response.Fields["Code"].ValidationMessages.Any(msg => msg.Message == Message));
        }

        [Then(@"the system correctly adds the Data Source")]
        [Then(@"the system correctly deletes the Data Source")]
        [Then(@"the system correctly saves the changes of the Data Source")]
        public void ThenTheSystemCorrectlySavesTheDataSource() 
            => Assert.IsTrue(_context.Response.Valid);
        #endregion
    }
}
