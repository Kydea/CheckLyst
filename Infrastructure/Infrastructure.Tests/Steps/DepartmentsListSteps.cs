using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Common.Logic.WorkFlows;
using CheckListManager.Infrastructure.Data;
using CheckListManager.Infrastructure.Logic.Departments;
using CheckListManager.Infrastructure.Logic.PrototypeTypes;
using CheckListManager.Infrastructure.Tests.Contexts;
using CheckListManager.Infrastructure.Tests.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace CheckListManager.Infrastructure.Tests.Steps
{
    [Binding]
    public class DepartmentsListSteps
    {
        private readonly DepartmentsContext _context;
        private readonly IDefaultIdentity _defaultIdentity;
        private readonly ICommonRepository _commonRepository;
        private readonly IDepartmentsService _departmentsService;
        private readonly IInfrastructureHelper _infrastructureHelper;
        private readonly IPrototypeTypesService _prototypeTypesService;
        private readonly IWorkFlowsRepositoryExtended _workFlowsRepositoryExtended;
        private readonly IPrototypesRepositoryExtended _prototypesRepositoryExtended;

        public DepartmentsListSteps(DepartmentsContext context,
            IDefaultIdentity defaultIdentity,
            ICommonRepository commonRepository,
            IDepartmentsService departmentsService,
            IInfrastructureHelper infrastructureHelper,
            IPrototypeTypesService prototypeTypesService,
            IWorkFlowsRepositoryExtended workFlowsRepositoryExtended,
            IPrototypesRepositoryExtended prototypesRepositoryExtended)
        {
            _context = context;
            _defaultIdentity = defaultIdentity;
            _commonRepository = commonRepository;
            _departmentsService = departmentsService;
            _infrastructureHelper = infrastructureHelper;
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            _context.Record = new DepartmentsRow();
        }

        [AfterScenario]
        public void AfterScenario()
        {
            _context.Record = null;
            _context.Response = null;
        }

        [Given(@"I change the name of the department in '(.*)'")]
        [Given(@"I assign '(.*)' as the name of the department")]
        public void GivenIAssignAsTheNameOfTheDepartment(string Name)
        {
            _context.Record.Name = Name;
            _context.Record.Reason = "Reason";
        }

        [Given(@"I click on a department")]
        public async Task GivenIClickOnADeparment()
        {
            _context.Record = await _infrastructureHelper.CreateDepartmentAsync();
        }

        [Given(@"I click on a department that is used for creating a prototype type")]
        public async Task GivenIClickOnADepartmentThatIsUsedForAPrototypeType()
        {
             _context.Record = await _infrastructureHelper.CreateDepartmentWithPrototypeTypeAsync();
        }

        [Given(@"I click on a department that is used for creating a prototype")]
        public async Task GivenIClickOnADepartmentThatIsUsedForAPrototype()
        {
            _context.Record = await _infrastructureHelper.CreateDepartmentWithPrototypeAsync();
        }

        [When(@"I submit the request to add this department")]
        public async Task WhenISendTheAddRequestAsync() => _context.Response = await _departmentsService.AddOrEdit(_defaultIdentity, _context.Record, true);

        [When(@"I submit the request to edit this department")]
        public async Task WhenISendTheEditRequestAsync() => _context.Response = await _departmentsService.AddOrEdit(_defaultIdentity, _context.Record, false);

        [When(@"I submit the request to delete this department")]
        public async Task WhenISendTheDeleteRequest() => _context.Response = await _departmentsService.Delete(_defaultIdentity, _context.Record);

        [Then(@"the system does not saves the department and returns an error message: '(.*)'")]
        public void ThenTheSystemReturnsAnError(string Message)
            => Assert.IsTrue(_context.Response.Fields.ContainsKey("Name") && _context.Response.Fields["Name"].ValidationMessages.Any(msg => msg.Message == Message));

        [Then(@"the system allows to correctly save the department into the database")]
        public void ThenTheSystemAllowsToCorrectlySaveTheRecordIntoTheDatabase() => Assert.IsTrue(_context.Response.Valid);

        [Then(@"the system correctly delete the department from the database")]
        public void ThenTheSystemCorrectlyDeleteTheRecordFromTheDatabase() => Assert.IsTrue(_context.Response.Valid);

        [Then(@"the system does not allow to add the unnamed department")]
        [Then(@"the system does not allow the department deletion from the database")]
        public void ThenTheSystemDoesNotAllowTheDepartmentDeletionFromTheDatabase()
            => Assert.IsFalse(_context.Response.Valid, "the system must be in an invalid state");
    }
}
