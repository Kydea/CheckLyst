using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Infrastructure.Data;
using CheckListManager.Infrastructure.Logic.PrototypeTypes;
using CheckListManager.Infrastructure.Tests.Contexts;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace CheckListManager.Infrastructure.Tests.Steps
{
    [Binding]
    public class PrototypeTypesListSteps
    {
        private readonly PrototypeTypesContext _context;
        private readonly IDefaultIdentity _defaultIdentity;
        private readonly ICommonRepository _commonRepository;
        private readonly IPrototypeTypesService _prototypeTypesService;
        private readonly IInfrastructureRepository _infrastructureRepository;

        public PrototypeTypesListSteps(PrototypeTypesContext context, 
            IDefaultIdentity defaultIdentity, 
            ICommonRepository commonRepository,
            IPrototypeTypesService prototypeTypesService)
        {
            _context = context;
            _defaultIdentity = defaultIdentity;
            _commonRepository = commonRepository;
            _prototypeTypesService = prototypeTypesService;
        }
        
        [BeforeScenario]
        public void BeforeScenario()
        {
            _context.Request = new PrototypeTypesRequest();
        }

        [AfterScenario]
        public void AfterScenario()
        {
            _context.Request = null;
            _context.Records = null;
            _context.Response = null;
        }

        [Given(@"I don't choose any department from the list")]
        public void GivenIDonTChooseAnyDepartmentFromTheList()
        {
            _context.Request.Department = null;
        }
        
        [Given(@"I choose a department from the list")]
        public async Task GivenIChooseADepartmentFromTheList()
        {
            PrototypeTypesResponse response = (await _prototypeTypesService.BindAsync(_defaultIdentity, _context.Request));

            // Almost a department needs to be selected
            Assert.IsTrue(response.Fields.ContainsKey("Department") && response.Fields["Department"].Options.Count() > 0, "Missing dipartments: cannot select a department from the list.");
            Assert.IsTrue(long.TryParse(response.Fields["Department"].Options.First().Value, out long Department));

            _context.Request.Department = Department;
        }
        
        [Given(@"I change the name of the prototype in '(.*)'")]
        [Given(@"I assign '(.*)' as the name of the prototype")]
        public void GivenIAssignAsTheNameOfThePrototype(string Name)
        {
            _context.Request.Name = Name;
        }

        [Given(@"I click on a prototype type")]
        public async Task GivenIClickOnAPrototypeType()
        {
            // The clicked prototype is the first one, if Name is null the "click" fails.
            _context.Request.Name = String.Empty;

            List<PrototypeTypesRow> data = (await _prototypeTypesService.GetDataAsync(_defaultIdentity.CompanyId, new PagingRequest<PrototypeTypesRequest>
            {
                Data = _context.Request
            })).Data;

            // If you click on a prototype: Count > 0 (it's a must)
            Assert.NotZero(data.Count, "Clicking on a prototype type implies that the number of prototypes types is greater than 0.");

            _context.Request.ID = data.First().ID;
        }
        
        [Given(@"I click on a prototype type that is used for a prototype")]
        public async Task GivenIClickOnAPrototypeTypeThatIsUsedForAPrototype()
        {
            List<PrototypeTypes> data = (await _commonRepository.GetPrototypeTypesAsync(_defaultIdentity, _context.Request.Department.Value, null))
                .Where(pt => pt.Prototypes.Count != 0).ToList();

            // If you click on a prototype: Count > 0 (it's a must)
            Assert.NotZero(data.Count, "Clicking on a prototype type implies that the number of prototypes types is greater than 0.");

            _context.Request.ID = data.First().ID;
        }

        [When(@"I send the add request")]
        public async Task WhenISendTheAddRequestAsync() => _context.Response = await _prototypeTypesService.Add(_defaultIdentity, _context.Request);

        [When(@"I send the edit request")]
        public async Task WhenISendTheEditRequestAsync() => _context.Response = await _prototypeTypesService.Edit(_defaultIdentity, _context.Request);

        [When(@"I send the delete request")]
        public async Task WhenISendTheDeleteRequest() => _context.Response = await _prototypeTypesService.Delete(_defaultIdentity, _context.Request.ID.Value);

        [Then(@"the system returns an error message: '(.*)'")]
        public void ThenTheSystemReturnsAnError(string Message)
            => Assert.IsTrue(_context.Response.Fields.ContainsKey("Name") && _context.Response.Fields["Name"].ValidationMessages.Any(msg => msg.Message == Message));

        [Then(@"the system allows to correctly save the record into the database")]
        public void ThenTheSystemAllowsToCorrectlySaveTheRecordIntoTheDatabase() => Assert.IsTrue(_context.Response.Valid);

        [Then(@"the system correctly delete the record from the database")]
        public void ThenTheSystemCorrectlyDeleteTheRecordFromTheDatabase() => Assert.IsTrue(_context.Response.Valid);

        [Then(@"the system does not allow to add the unnamed prototype type")]
        [Then(@"the system does not allow the prototype type deletion from the database")]
        public void ThenTheSystemDoesNotAllowThePrototypeTypeDeletionFromTheDatabase()
            => Assert.IsFalse(_context.Response.Valid);
    }
}
