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
            _context.CheckListCode = string.Empty;
            _context.CheckListID = null;
        }
        
        [Given(@"I want a list of trails identified with a specific checklist code")]
        public async Task GivenIWantAListOfTrailsIdentifiedWithChecklistCode()
        {
            _context.User = string.Empty;
            _context.CheckListCode = (await _trailRepositoryExtended.GetFirstAsync(r => !string.IsNullOrEmpty(r.CheckListCode))).CheckListCode;
            _context.CheckListID = null;
        }

        [Given(@"I want a list of trails identified with a specific checklist ID")]
        public async Task GivenIWantAListOfTrailsIdentifiedWithChecklistID()
        {
            _context.User = string.Empty;
            _context.CheckListCode = string.Empty;
            _context.CheckListID = (await _trailRepositoryExtended.GetFirstAsync(r => r.CheckListID.HasValue)).CheckListID;
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
                    Action = string.Empty,
                    Information = string.Empty,
                    CheckListCode = _context.CheckListCode,
                    CheckListID = _context.CheckListID
                }
            })).Data;
        }
        
        [Then(@"the result should be a list of records belonging to the same user")]
        public void ThenTheResultShouldBeAListOfRecordsBelongingToTheSameUser()
        {
            _context.Response.ForEach(trail => Assert.AreEqual(_context.User.ToUpper(), trail.User.ToUpper()));
        }
        
        [Then(@"the result should be a list of records with the same checklist code")]
        public void ThenTheResultShouldBeAListOfRecordsWithTheSameChecklistCode()
        {
            _context.Response.ForEach(trail => Assert.IsTrue(trail.CheckListCode.ToUpper().Contains(_context.CheckListCode.ToUpper())));
        }

        [Then(@"the result should be a list of records with the same checklist ud")]
        public void ThenTheResultShouldBeAListOfRecordsWithTheSameChecklistID()
        {
            _context.Response.ForEach(trail => Assert.IsTrue(trail.CheckListID == _context.CheckListID));
        }
    }
}
