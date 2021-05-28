using CheckListManager.Common.Core.Web.Security;
using CheckListManager.WorkFlows.Logic.Detail;
using CheckListManager.WorkFlows.Logic.List;
using CheckListManager.WorkFlows.Tests.Contexts;
using CheckListManager.WorkFlows.Tests.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace CheckListManager.WorkFlows.Tests.Steps
{
    [Binding]
    public class WorkFlowsListSteps
    {
        private readonly IListService _listService;
        private readonly WorkFlowsListContext _context;
        private readonly IDetailService _detailService;
        private readonly IDefaultIdentity _defaultIdentity;
        private readonly IWorkFlowsRepositoryExtended _workFlowsRepositoryExtended;
        private readonly IPrototypeTypesRepositoryExtended _prototypeTypesRepositoryExtended;

        public WorkFlowsListSteps(
            IListService listService,
            WorkFlowsListContext context,
            IDetailService detailService,
            IDefaultIdentity defaultIdentity,
            IWorkFlowsRepositoryExtended workFlowsRepositoryExtended,
            IPrototypeTypesRepositoryExtended prototypeTypesRepositoryExtended)
        {
            _context = context;
            _listService = listService;
            _detailService = detailService;
            _defaultIdentity = defaultIdentity;
            _workFlowsRepositoryExtended = workFlowsRepositoryExtended;
            _prototypeTypesRepositoryExtended = prototypeTypesRepositoryExtended;
        }

        [BeforeScenario]
        public void BeforeScenario() 
            => _context.Request = new DetailRequest();

        public void AfterScenario()
        {
            _context.Request = null;
            _context.Response = null;
        }

        private async Task<DetailRequest> CreateWorkFlow()
        {
            var Record = new DetailRequest
            {
                Description = Guid.NewGuid().ToString(),
                Reason = "Testing",
                Steps = new Step[] {
                    new Step()
                    {
                        Reason = "Testing",
                        Signatures = new List<string[]>().ToArray()
                    }
                }
            };

            var Response = await _detailService.AddAsync(_defaultIdentity, Record);
            Assert.IsTrue(Response.Valid, "Impossibile creare un work flow.");
            return Record;
        }

        [Given(@"I assign '(.*)' as the description of the workflow")]
        public void GivenIAssignAsTheDescriptionOfTheWorkflow(string Description)
        {
            _context.Request.Description = Description;
            _context.Request.Reason = "Unit test";
        }

        [Given(@"I add the '(.*)' signature in the workflows' steps")]
        public void GivenIAddTheSignatureInTheWorkflowsSteps(string signature)
        {
            List<Step> steps;

            if (_context.Request.Steps == null)
                steps = new List<Step>();
            else
                steps = new List<Step>(_context.Request.Steps);

            if (!steps.Any())
                steps.Add(new Step() {
                    Reason = "No reason",
                    Signatures = new List<string[]>()
                    {
                        new string[] { signature }
                    }.ToArray()
                });
            else
            {
                Step currStep = steps[0];

                var signatures = new List<string[]>(currStep.Signatures);
                var user = new List<string>(signatures[0])
                {
                    signature
                };

                signatures[0] = user.ToArray();
                currStep.Signatures = signatures.ToArray();
            }

            _context.Request.Steps = steps.ToArray();
        }
        
        [Given(@"I click on a workflow")]
        public async Task GivenIClickOnAWorkflow()
        {
            _context.Request = await CreateWorkFlow();
        }

        [Given(@"I click on a workflow that has been used for a prototype type")]
        public async Task GivenIClickOnAWorkflowThatHasBeenUsedForAPrototypeType()
        {
            var workFlow = await CreateWorkFlow();

            _prototypeTypesRepositoryExtended.Add(_defaultIdentity.CompanyId, new Common.Data.PrototypeTypes()
            {
                Company = _defaultIdentity.CompanyId,
                ApprovationWorkFlow = workFlow.ID,
            });
        }

        [Given(@"I do not add any signature to the workflow")]
        [Given(@"I delete the steps from the workflow")]
        public void GivenIDeleteTheStepsFromTheWorkflow()
        {
            _context.Request.Steps = null;
        }

        [When(@"I submit the request to add the workflow")]
        public async Task WhenISubmitTheRequestToAddTheWorkflow() => _context.Response = await _detailService.AddAsync(_defaultIdentity, _context.Request);

        [When(@"I submit the request to edit the workflow")]
        public async Task WhenISubmitTheRequestToEditTheWorkflow() => _context.Response = await _detailService.EditAsync(_defaultIdentity, _context.Request);

        [When(@"I submit the delete request to delete the workflow")]
        public async Task WhenISubmitTheDeleteRequestToDeleteTheWorkflow() => _context.Response = await _detailService.DeleteAsync(_defaultIdentity, _context.Request);
        
        [Then(@"the system does not save the workflow and returns an error message: '(.*)'")]
        public void ThenTheSystemDoesNotSavesTheWorkflowAndReturnsAnErrorMessage(string Message)
        {
            Assert.IsFalse(_context.Response.Valid, "The response must be validated as 'False' but it is 'True'.");
            Assert.IsTrue(_context.Response.ValidationMessages.Any(msg => msg.Message == msg.Message));
        }

        [Then(@"the system does not edit the workflow and returns an error message '(.*)' on the '(.*)' field")]
        [Then(@"the system does not save the workflow and returns an error message '(.*)' on the '(.*)' field")]
        public void ThenTheSystemDoesNotSaveTheWorkflowAndReturnsAnErrorMessageOnTheField(string Message, string Field)
        {
            Assert.IsFalse(_context.Response.Valid, "The response must be validated as 'False', but it is 'True'.");
            Assert.IsTrue(_context.Response.Fields.ContainsKey(Field) && _context.Response.Fields[Field].ValidationMessages.Any(msg => msg.Message == Message));
        }

        [Then(@"the system does not save the workflow and returns an error")]
        [Then(@"the system does not delete the workflow and returns an error")]
        public void ThenTheSystemDoesNotSavesTheWorkflowAndReturnsAnError() => Assert.IsFalse(_context.Response.Valid);

        [Then(@"the system should correctly save workflow changes")]
        [Then(@"the system should correctly save the workflow into the database")]
        [Then(@"the system should correctly delete the workflow from the database")]
        public void ThenTheSystemShouldCorrectlySaveTheWorkflowIntoTheDatabase() => Assert.IsTrue(_context.Response.Valid);
    }
}
