using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Matematici.Binders
{
    public class ModelBinder : DefaultModelBinder
    {
        protected override object CreateModel(ControllerContext controllerContext, ModelBindingContext bindingContext, Type modelType)
        {
			return base.CreateModel(controllerContext, bindingContext, modelType);
		}

		public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			return base.BindModel(controllerContext, bindingContext);
		}
	}
}
