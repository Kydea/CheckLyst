using CheckListManager.Common.Core.Network;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Reports.Data;
using Newtonsoft.Json;
using NLog;
using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckListManager.Reports.Logic.Viewer
{
    public class ReportViewerService : IViewerEditorService
    {
        private readonly IReportsRepository _reportsRepository;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ReportViewerService(IReportsRepository reportsRepository)
        {
            _reportsRepository = reportsRepository;
        }
    }
}