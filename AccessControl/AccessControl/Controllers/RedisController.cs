using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.Common.Logic.Cache;

namespace CheckListManager.AccessControl.Controllers
{
	[RoutePrefix("redis")]
	public class RedisController : ApiController
	{
		private readonly IRedisCache _cache;

		public RedisController(IRedisCache cache)
		{
			_cache = cache;
		}

		[HttpGet]
		[Route("{key}/{regionName?}")]
		public async Task<HttpResponseMessage> Get(string key, string regionName)
		{
			try
			{
				var value = await _cache.GetAsync<string>(key, regionName);
				return Request.CreateResponse(HttpStatusCode.OK, value ?? "(NULL)");
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error");
			}
		}

		[HttpPost]
		[Route("{key}/{regionName?}")]
		public async Task<HttpResponseMessage> Set(string key, string regionName, [FromBody]string value)
		{
			try
			{
				await _cache.SetAsync(key, value, regionName);
				return Request.CreateResponse(HttpStatusCode.OK, "OK");
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error");
			}
		}
	}
}
