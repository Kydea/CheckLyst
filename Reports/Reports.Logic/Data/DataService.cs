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

namespace CheckListManager.Reports.Logic.Data
{
    public class DataService : IDataService
    {
        private readonly IReportsRepository _reportsRepository;
        private readonly IServiceClient _client;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DataService(IReportsRepository reportsRepository, IServiceClient client)
        {
            _reportsRepository = reportsRepository;
            _client = client;
        }
    }
}