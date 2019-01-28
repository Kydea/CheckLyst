using Matematici.DataSource;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Matematici.ActionResults
{
    public class PDFResult : ActionResult
    {
        public PrototypeFile PrototypeFile { get; }
        public bool Download { get; }

        public PDFResult(PrototypeFile PrototypeFile, bool Download)
        {
            this.PrototypeFile = PrototypeFile;
            this.Download = Download;
        }

        public override void ExecuteResult(ControllerContext Context)
        {
            try
            {
                Context.HttpContext.Response.Clear();
                Context.HttpContext.Response.ContentType = "application/pdf";

                Context.HttpContext.Response.AddHeader("content-disposition", $"{(Download ? "attachment;" : string.Empty)}filename={PrototypeFile.Name}.pdf");

                PrototypeFile.GeneratePDF(Context.HttpContext.Response.OutputStream);
            }
            catch
            {
                Context.HttpContext.Response.Clear();
                Context.HttpContext.Response.ContentType = "text/html";
                throw;
            }
        }
    }
}
