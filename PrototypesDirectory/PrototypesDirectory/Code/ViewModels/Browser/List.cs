using Matematici.DataEngine;
using Matematici.DataSource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Matematici.ViewModels.Browser
{
    public class List : Form
    {
        public List(int Company)
        {
            this.Company = Company;
        }

        private int Company { get; }

        public void Bind(PagingRequest<Request> Request)
        {
            if (Request.Data.Source == null)
            {
                //initial load

                Fields["name"] = new Field { Value = Request.Data.Name };

                var departments = Engine.Instance.Companies[Company].Departments;
                Fields["department"] = new Field
                {
                    Options = departments.OrderBy(x => x).Select(d => new Option(d, d)),
                    Value = Request.Data.Department
                };
            }

            if (Request.Data.Source == null || Request.Data.Source == "department")
            {
                var prototypeTypes = string.IsNullOrEmpty(Request.Data.Department) ? new List<string>() : Engine.Instance.Companies[Company].PrototypeTypes[Request.Data.Department];
                Fields["prototypeType"] = new Field
                {
                    Options = prototypeTypes.OrderBy(x => x).Select(pt => new Option(pt, pt)),
                    Value = !string.IsNullOrEmpty(Request.Data.PrototypeType) && prototypeTypes.Any(pt => pt == Request.Data.PrototypeType) ? Request.Data.PrototypeType : null
                };

            }
        }

    }
}
