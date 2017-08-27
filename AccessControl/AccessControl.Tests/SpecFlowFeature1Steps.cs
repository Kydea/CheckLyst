using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace AccessControl.Tests
{
    [Binding]
    public class SpecFlowFeature1Steps
    {
	    private readonly List<int> _numbers = new List<int>();
	    private int _result;

        [Given(@"I have entered (.*) into the calculator")]
        public void GivenIHaveEnteredIntoTheCalculator(int p0)
        {
	        _numbers.Add(p0);
        }
        
        [When(@"I press add")]
        public void WhenIPressAdd()
        {
	        _result = _numbers.Sum() + 1;
        }
        
        [Then(@"the result should be (.*) on the screen")]
        public void ThenTheResultShouldBeOnTheScreen(int p0)
        {
            Assert.AreEqual(p0, _result, "The sum doesn't match what is expected");
        }
    }
}
