using CheckListManager.Common.Logic.Api;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Reports.Logic.Viewer;
using System.Threading.Tasks;
using System.Web.Http;
using CheckListManager.Common.Logic.DataEngine;
using System;
using CheckListManager.Common.Logic.Cache;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CheckListManager.Reports.Api.Controllers
{
	[RoutePrefix("Viewer")]
	public class ViewerController : BaseApiController
	{
		private readonly IViewerEditorService _reportViewerService;

        public ViewerController(IViewerEditorService reportViewerService)
		{
            _reportViewerService = reportViewerService;
        }
    }
}
