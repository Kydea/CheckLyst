using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Prototypes.Data.Models;
using CheckListManager.Prototypes.Logic.List;
using CheckListManager.Prototypes.Tests.Contexts;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace CheckListManager.Prototypes.Tests.Steps
{
    [Binding]
    public class PrototypesListSteps
    {
        private readonly IDefaultIdentity _defaultIdentity;
        private readonly PrototypesListContext _context;
        private readonly IListService _listService;

        public PrototypesListSteps( 
            IDefaultIdentity defaultIdentity,
            PrototypesListContext context,
            IListService listService)
        {
            _defaultIdentity = defaultIdentity;
            _listService = listService;
            _context = context;
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            _context.Request = new PrototypeFilters()
            {
                State = String.Empty,
                Name = String.Empty,
                Pending = "Include", // || Exclude || Filter
            };
        }

        [AfterScenario]
        public void AfterScenario()
        {
            _context.Request = null;
            _context.Response = null;
        }

        [Given(@"I use a user that has not the grants for accessing the prototypes list")]
        public void GivenIUseAUserThatHasNotTheGrantsForAccessingThePrototypesList()
        {
            // Changing User's code will allow us to break the relationship on how grants are retrieved
            var identity = ((DefaultIdentityHelper)_defaultIdentity);
            identity.Code = Guid.NewGuid().ToString();
        }
        
        [When(@"I access the page")]
        public async Task WhenIAccessThePage()
        {
            _context.Response = (await _listService.GetDataAsync(_defaultIdentity, new PagingRequest<PrototypeFilters>
            {
                Data = _context.Request
            }));
        }
        
        [Then(@"the system returns an error message: ""(.*)""")]
        public void ThenTheSystemReturnsAnErrorMessage(string p0)
        {
            Assert.IsTrue(_context.Response.Count == 0 && _context.Response.Data.Count == 0);
        }
    }
}
