using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Infrastructure.Data;
using CheckListManager.Infrastructure.Logic.Departments;
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
    public class DepartmentsListSteps
    {
        private readonly DepartmentsContext _context;
        private readonly IDefaultIdentity _defaultIdentity;
        private readonly ICommonRepository _commonRepository;
        private readonly IDepartmentsService _departmentsService;

        public DepartmentsListSteps(DepartmentsContext context,
            IDefaultIdentity defaultIdentity,
            ICommonRepository commonRepository,
            IDepartmentsService departmentsService)
        {
            _context = context;
            _defaultIdentity = defaultIdentity;
            _commonRepository = commonRepository;
            _departmentsService = departmentsService;
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
            string Name = Guid.NewGuid().ToString();

            // Create the department
            GivenIAssignAsTheNameOfTheDepartment(Name);
            await WhenISendTheAddRequestAsync();

            // "Click" on the department
            List<DepartmentsRow> data = (await _departmentsService.GetDataAsync(_defaultIdentity, new PagingRequest<DepartmentsRequest>
            {
                Data = new DepartmentsRequest()
                {
                    Department = Name
                }
            })).Data;

            // If you click on a department: Count > 0 (it's a must)
            Assert.NotZero(data.Count, "Clicking on a department implies that the number of departments is greater than 0.");

            _context.Record.ID = data.First().ID;
        }

        [Given(@"I click on a department that is used for creating a prototype type")]
        public async Task GivenIClickOnADepartmentThatIsUsedForAPrototypeType()
        {
            List<Departments> data = (await _commonRepository.GetDepartmentsAsync(_defaultIdentity, null, null))
                .Where(d => d.PrototypeTypes.Any()).ToList();

            // If you click on a department: Count > 0 (it's a must)
            Assert.NotZero(data.Count, "Clicking on a department implies that the number of departments is greater than 0.");

            _context.Record.ID = data.First().ID;
        }

        [Given(@"I click on a department that is used for creating a prototype")]
        public async Task GivenIClickOnADepartmentThatIsUsedForAPrototype()
        {
            List<Departments> data = (await _commonRepository.GetDepartmentsAsync(_defaultIdentity, null, null))
                .Where(d => d.Prototypes.Any()).ToList();

            // If you click on a department: Count > 0 (it's a must)
            Assert.NotZero(data.Count, "Clicking on a department implies that the number of departments is greater than 0.");

            _context.Record.ID = data.First().ID;
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
            => Assert.IsFalse(_context.Response.Valid);
    }
}
