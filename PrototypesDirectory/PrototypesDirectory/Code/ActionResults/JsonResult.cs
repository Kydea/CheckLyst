using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Matematici.ActionResults
{
    public class JsonResult : System.Web.Mvc.JsonResult
    {
        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (JsonRequestBehavior == JsonRequestBehavior.DenyGet && String.Equals(context.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Get not allowed");

            HttpResponseBase response = context.HttpContext.Response;
            response.ContentType = !string.IsNullOrEmpty(ContentType) ? ContentType : "application/json";

            if (ContentEncoding != null) response.ContentEncoding = ContentEncoding;
            if (Data != null)
                try
                {
                    var camelCaseFormatter = new JsonSerializerSettings();
                    camelCaseFormatter.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    response.Write(JsonConvert.SerializeObject(Data, camelCaseFormatter));
                }
                catch(Exception Exception)
                {
                    response.Write(JsonConvert.SerializeObject(new { Error = Exception.Message }));
                }
        }
    }
}
