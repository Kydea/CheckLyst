using System.Threading.Tasks;
using CheckListManager.Common.Logic.Cache;
using CheckListManager.Common.Logic.Json;
using CheckListManager.Instances.Logic.Models;
using CheckListManager.Instances.Logic.Models.CheckLists;
using CheckListManager.Instances.Logic.Services;
using CheckListManager.Instances.Tests.Contexts;
using Newtonsoft.Json;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace CheckListManager.Instances.Tests.Steps.InstanceCache
{
	[Binding]
	public class InstanceCacheSteps
	{
		private readonly InstanceCacheContext _context;
		private readonly IInstanceCacheService _instanceCache;
		private readonly CheckListJsonConverter _converter;
		private readonly IRedisCache _redisCache;

		public InstanceCacheSteps(InstanceCacheContext context, IInstanceCacheService instanceCache, CheckListJsonConverter converter, IRedisCache redisCache)
		{
			_context = context;
			_instanceCache = instanceCache;
			_converter = converter;
			_redisCache = redisCache;
		}

		[Given(@"I am logged in as '(.*)'")]
		public void GivenIAmLoggedInAs(string userCode)
		{
			_context.UserCode = userCode;
		}

		[Given(@"I want to create an instance based on a prototype")]
		public void GivenIHaveWantToCreateAnInstanceBasedOnAPrototype()
		{
			_context.Code = "MyTestInstance";
			_context.Object = new InstanceCacheObject
			{
				Code = _context.Code,
				Instance = JsonConvert.DeserializeObject<Instance>(_context.PrototypeSource, _converter)
			};
		}

		[When(@"I create the instance")]
		public async Task WhenICreateTheInstance()
		{
			await _instanceCache.SetAsync(_context.Code, _context.Object);
		}

		[Then(@"I have the instance stored in redis")]
		public async void ThenIHaveTheInstanceStoredInRedis()
		{
			Assert.True(await _redisCache.SortedSetContainsAsync("Instances", _context.Code, "Instance"), "Instance not found under instances");
		}

		[Then(@"I have the instance in my instances list")]
		public void ThenIHaveTheInstanceInMyInstancesList()
		{
			ScenarioContext.Current.Pending();
		}

		[Then(@"I do not have the instance in the list when I am logged as '(.*)'")]
		public void ThenIDoNotHaveTheInstanceInTheListWhenIAmLoggedAs(string userCode)
		{
			ScenarioContext.Current.Pending();
		}
	}
}
