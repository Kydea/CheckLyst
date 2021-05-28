using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
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
        private readonly IInfrastructureHelper _infrastructureHelper;
        private readonly IPrototypeTypesService _prototypeTypesService;

        public PrototypeTypesListSteps(PrototypeTypesContext context, 
            IDefaultIdentity defaultIdentity, 
            ICommonRepository commonRepository,
            IInfrastructureHelper infrastructureHelper,
            IPrototypeTypesService prototypeTypesService)
        {
            _context = context;
            _defaultIdentity = defaultIdentity;
            _commonRepository = commonRepository;
            _infrastructureHelper = infrastructureHelper;
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

        public void DebugContext()
        {
            if (_context.Response == null)
                return;

            _context.Response.ValidationMessages.ForEach(e => Console.WriteLine("Errore[{0}]: {1}", e.Level, e.Message));
            
            foreach (var Field in _context.Response.Fields)
                Field.Value.ValidationMessages.ForEach(e => Console.WriteLine("Errore[{0}]: {1}", e.Level, e.Message));
        }

        [Given(@"I don't choose any department from the list")]
        public void GivenIDonTChooseAnyDepartmentFromTheList()
        {
            _context.Request.Department = null;
        }
        
        [Given(@"I choose a department from the list")]
        public async Task GivenIChooseADepartmentFromTheList()
        {
            //PrototypeTypesResponse response = (await _prototypeTypesService.BindAsync(_defaultIdentity, _context.Request));

            //Assert.IsTrue(response.Fields.ContainsKey("Department") && response.Fields["Department"].Options.Any(), "Missing dipartments: cannot select a department from the list.");
            //Assert.IsTrue(long.TryParse(response.Fields["Department"].Options.First().Value, out long Department));

            //Assert.IsTrue(response.Fields.ContainsKey("ApprovationWorkFlow") && response.Fields["ApprovationWorkFlow"].Options.Any());
            //Assert.IsTrue(long.TryParse(response.Fields["ApprovationWorkFlow"].Options.First().Value, out long ApprovationWorkFlow));

            //Assert.IsTrue(response.Fields.ContainsKey("ApprovationWorkFlow") && response.Fields["ApprovationWorkFlow"].Options.Any());
            //Assert.IsTrue(long.TryParse(response.Fields["ApprovationWorkFlow"].Options.First().Value, out long ObsolescenceWorkFlow));

            //_context.Request.Department = Department;
            //_context.Request.ApprovationWorkFlow = ApprovationWorkFlow;
            //_context.Request.ObsolescenceWorkFlow = ObsolescenceWorkFlow;


            var Department = await _infrastructureHelper.CreateDepartmentAsync();
            var WorkFlow = await _infrastructureHelper.CreateWorkFlowAsync();
            _context.Request.ObsolescenceWorkFlow = WorkFlow.ID;
            _context.Request.ApprovationWorkFlow = WorkFlow.ID;
            _context.Request.Department = Department.ID;
        }
        
        [Given(@"I change the name of the prototype in '(.*)'")]
        [Given(@"I assign '(.*)' as the name of the prototype")]
        public void GivenIAssignAsTheNameOfThePrototype(string Name)
        {
            _context.Request.Name = Name;
            _context.Request.Reason = "Reason";
        }

        [Given(@"I click on a prototype type")]
        public async Task GivenIClickOnAPrototypeType()
        {
            _context.Request.Name = Guid.NewGuid().ToString();
            _context.Request.Reason = "Testing";

            var Response = await _prototypeTypesService.Add(_defaultIdentity, _context.Request);

            Assert.IsTrue(Response.Valid, "Prototype type cannot be created.");
        }
        
        [Given(@"I click on a prototype type that is used for a prototype")]
        public async Task GivenIClickOnAPrototypeTypeThatIsUsedForAPrototype()
        {
            var Department = await _infrastructureHelper.CreateDepartmentAsync();
            var PrototypeType = await _infrastructureHelper.CreatePrototypeTypeAsync(Department);
            var Prototype = await _infrastructureHelper.CreatePrototypeAsync(Department, PrototypeType);

            _context.Request.ID = Prototype.ID;
        }

        [When(@"I send the add request")]
        public async Task WhenISendTheAddRequestAsync() => _context.Response = await _prototypeTypesService.Add(_defaultIdentity, _context.Request);

        [When(@"I send the edit request")]
        public async Task WhenISendTheEditRequestAsync() => _context.Response = await _prototypeTypesService.Edit(_defaultIdentity, _context.Request);

        [When(@"I send the delete request")]
        public async Task WhenISendTheDeleteRequest() => _context.Response = await _prototypeTypesService.Delete(_defaultIdentity, _context.Request);

        [Then(@"the system returns an error message: '(.*)'")]
        public void ThenTheSystemReturnsAnError(string Message)
            => Assert.IsTrue(_context.Response.Fields.ContainsKey("Name") && _context.Response.Fields["Name"].ValidationMessages.Any(msg => msg.Message == Message));

        [Then(@"the system allows to correctly save the record into the database")]
        public void ThenTheSystemAllowsToCorrectlySaveTheRecordIntoTheDatabase()
        {
            DebugContext();

            Assert.IsTrue(!_context.Response.Fields.Any());
            Assert.IsTrue(!_context.Response.ValidationMessages.Any());
            Assert.IsTrue(_context.Response.Valid);
        }

        [Then(@"the system correctly delete the record from the database")]
        public void ThenTheSystemCorrectlyDeleteTheRecordFromTheDatabaseAsync()
        {
            DebugContext();

            Assert.IsTrue(!_context.Response.Fields.Any());
            Assert.IsTrue(!_context.Response.ValidationMessages.Any());
            Assert.IsTrue(_context.Response.Valid);
        }

        [Then(@"the system does not allow to add the unnamed prototype type")]
        [Then(@"the system does not allow the prototype type deletion from the database")]
        public void ThenTheSystemDoesNotAllowToAddTheUnnamedPrototypeType()
        {
            Assert.IsFalse(_context.Response.Valid);
        }
    }
}
