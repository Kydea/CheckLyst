using CheckListManager.Common.Data;
using CheckListManager.Trail.Logic.Services;
using CheckListManager.Trail.Tests.Contexts;
using CheckListManager.Trail.Tests.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using NUnit.Framework;

namespace CheckListManager.Trail.Tests.Steps.Audit_Trail
{
    [Binding]
    public class FilterPanelSteps
    {
        private readonly FilterPanelContext _context;
        private readonly IListService _listService;
        private readonly ICommonRepository _commonRepository;
        private readonly ITrailRepositoryExtended _trailRepositoryExtended;

        public FilterPanelSteps(FilterPanelContext context, 
            IListService listService, 
            ICommonRepository commonRepository, 
            ITrailRepositoryExtended trailRepositoryExtended)
        {
            _context = context;
            _listService = listService;
            _commonRepository = commonRepository;
            _trailRepositoryExtended = trailRepositoryExtended;
        }

        [Given(@"I want the trails of a specific user")]
        public async Task GivenIWantTheTrailsOfUser()
        {
            _context.User = (await _trailRepositoryExtended.GetFirstAsync()).User;
            _context.CheckList = String.Empty;
        }
        
        [Given(@"I want a list of trails identified with a specific checklist code")]
        public async Task GivenIWantAListOfTrailsIdentifiedWithChecklistCode()
        {
            _context.User = String.Empty;
            _context.CheckList = (await _trailRepositoryExtended.GetFirstAsync(r => !String.IsNullOrEmpty(r.CheckList))).CheckList;
        }
        
        [When(@"I send the search request")]
        public async Task WhenISendTheSearchRequest()
        {
            long CompanyId = (await _commonRepository.GetCompaniesAsync()).First().ID;

            _context.Response = (await _listService.GetDataAsync(CompanyId, new Common.Logic.Api.Messages.PagingRequest<ListRequest>
            {
                Data = new ListRequest()
                {
                    User = _context.User,
                    Action = String.Empty,
                    Information = String.Empty,
                    CheckList = _context.CheckList
                }
            })).Data;
        }
        
        [Then(@"the result should be a list of records belonging to the same user")]
        public void ThenTheResultShouldBeAListOfRecordsBelongingToTheSameUser()
        {
            _context.Response.ForEach(trail => Assert.AreEqual(_context.User, trail.User));
        }
        
        [Then(@"the result should be a list of records with the same checklist code")]
        public void ThenTheResultShouldBeAListOfRecordsWithTheSameChecklistCode()
        {
            _context.Response.ForEach(trail => Assert.IsTrue(trail.CheckList.ToUpper().Contains(_context.CheckList.ToUpper())));
        }
    }
}
