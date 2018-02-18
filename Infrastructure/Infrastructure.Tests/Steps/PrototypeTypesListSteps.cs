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
        private readonly IPrototypeTypesService _prototypeTypesService;

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
            PrototypeTypesResponse response = (await _prototypeTypesService.BindAsync(_defaultIdentity, _context.Request));

            Assert.IsTrue(response.Fields.ContainsKey("Department") && response.Fields["Department"].Options.Any(), "Missing dipartments: cannot select a department from the list.");
            Assert.IsTrue(long.TryParse(response.Fields["Department"].Options.First().Value, out long Department));

            Assert.IsTrue(response.Fields.ContainsKey("ApprovationWorkFlow") && response.Fields["ApprovationWorkFlow"].Options.Any());
            Assert.IsTrue(long.TryParse(response.Fields["ApprovationWorkFlow"].Options.First().Value, out long ApprovationWorkFlow));

            Assert.IsTrue(response.Fields.ContainsKey("ApprovationWorkFlow") && response.Fields["ApprovationWorkFlow"].Options.Any());
            Assert.IsTrue(long.TryParse(response.Fields["ApprovationWorkFlow"].Options.First().Value, out long ObsolescenceWorkFlow));

            _context.Request.Department = Department;
            _context.Request.ApprovationWorkFlow = ApprovationWorkFlow;
            _context.Request.ObsolescenceWorkFlow = ObsolescenceWorkFlow;
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
            string Name = Guid.NewGuid().ToString();

            // Add a prototype type
            GivenIAssignAsTheNameOfThePrototype(Name);
            await WhenISendTheAddRequestAsync();

            // Get the prototype type
            _context.Request.Name = Name;

            List<PrototypeTypesRow> data = (await _prototypeTypesService.GetDataAsync(_defaultIdentity, new PagingRequest<PrototypeTypesRequest>
            {
                Data = _context.Request
            })).Data;

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
