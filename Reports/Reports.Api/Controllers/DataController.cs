using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Reports.Logic.Data;
using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.Common.Logic.DataEngine;
using System;
using CheckListManager.Common.Logic.Cache;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CheckListManager.Reports.Api.Controllers
{
	[RoutePrefix("Data")]
	public class DataController : BaseApiController
	{
		private readonly IDataService _dataService;
        private readonly IRedisCache _redisCache;

        public DataController(IDataService dataService, IRedisCache redisCache)
		{
            _dataService = dataService;
            _redisCache = redisCache;
        }
    }
}
