using System;
using TechTalk.SpecFlow;

namespace CheckListManager.Reports.Tests.Features
{
    [Binding]
    public class ReportsListSteps
    {
        [Given(@"I assign '(.*)' as the code of the report")]
        public void GivenIAssignAsTheCodeOfTheReport(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"I submit the request to add the report")]
        public void WhenISubmitTheRequestToAddTheReport()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"the system does not save the report and returns an error message '(.*)' on the '(.*)' field")]
        public void ThenTheSystemDoesNotSaveTheReportAndReturnsAnErrorMessageOnTheField(string p0, string p1)
        {
            ScenarioContext.Current.Pending();
        }
    }
}
