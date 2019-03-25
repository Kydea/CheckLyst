using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.Json;
using CheckListManager.Instances.Logic.List;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CheckListManager.Instances.Api.Controllers
{
    [RoutePrefix("Test")]
	[AllowAnonymous]
    public class TestController : BaseApiController
    {
        private readonly IListService _listService;

        public TestController(IListService listService)
        {
            _listService = listService;
        }

        [HttpGet]
        [Route("Get")]
		public IHttpActionResult Get()
        {
	        var model = new OuterModel
	        {
		        StringProperty = "string",
		        StringDictionary1 = new Dictionary<string, string>
		        {
			        {"key", "value"},
			        {"kEY", "vALUE"},
			        {"Key", "Value"},
			        {"KeY", "ValuE"},
			        {"MyKey", "MyValue"}
				},
		        StringDictionary2 = new Dictionary<string, string>
		        {
			        {"key", "value"},
			        {"kEY", "vALUE"},
			        {"Key", "Value"},
			        {"KeY", "ValuE"},
			        {"MyKey", "MyValue"}
				},
		        InnerModel = new InnerModel
		        {
			        StringProperty = "string",
			        StringDictionary1 = new PascalCaseDictionary<string, string>
					{
				        {"key", "value1"},
				        {"kEY", "vALUE2"},
				        {"Key", "Value3"},
				        {"KeY", "ValuE4"},
				        {"MyKey", "MyValue"}
			        },
			        StringDictionary2 = new PascalCaseDictionary<string, string>
					{
				        {"key", "value1"},
				        {"kEY", "vALUE2"},
				        {"Key", "Value3"},
				        {"KeY", "ValuE4"},
				        {"MyKey", "MyValue"}
					},
		        }
	        };
			return Ok(model);
        }

        [HttpPost]
        [Route("Post")]
		public IHttpActionResult Post([FromBody] OuterModel model)
        {
	        return Ok(model);
        }
	}


	public class OuterModel
	{
		public string StringProperty { get; set; }
		public Dictionary<string, string> StringDictionary1 { get; set; }

		public Dictionary<string, string> StringDictionary2 { get; set; }

		public InnerModel InnerModel { get; set; }
	}

	public class InnerModel
	{
		public string StringProperty { get; set; }
		public PascalCaseDictionary<string, string> StringDictionary1 { get; set; }

		public PascalCaseDictionary<string, string> StringDictionary2 { get; set; }
	}
}
