using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;
using System;
using System.Threading;
using Newtonsoft.Json;
using CheckListManager.Common.Data.ListItems;
using CheckListManager.Common.Logic.Json;
using System.Diagnostics;
using CheckListManager.Instances.Logic.Services;
using CheckListManager.Instances.Logic.Exceptions;
using CheckListManager.Instances.Data;
using CheckListManager.Prototypes.Data;
using CheckListManager.Instances.Data.InstancesAnalysis;
using CheckListManager.Instances.Logic.Models;
using CheckListManager.Instances.Logic.Models.CheckLists;
using CheckListManager.MigrationTools.Api.PrototypesAnalysis;
using CheckListManager.Prototypes.Api;
using CheckListManager.Common.Core.Helpers;
using NLog;

namespace CheckListManager.Instances.Logic.InstancesAnalysis
{
    public class InstancesAnalysisService : IInstancesAnalysisService
    {
        private readonly ICommonRepository _commonRepository;
        private readonly IInstancesAnalysisRepository _instancesAnalysisRepository;
        private readonly IPrototypesRepository _prototypesRepository;
        private readonly IInstancesRepository _instancesRepository;
        private readonly IInstanceManagerService _instanceManagerService;
        private readonly JsonConverter[] _prototypeConverters;
        private readonly JsonConverter[] _instanceConverters;

        public InstancesAnalysisService(CustomJsonConverter[] converters, ICommonRepository commonRepository, IInstancesAnalysisRepository instancesAnalysisRepository, IPrototypesRepository prototypesRepository, IInstancesRepository instancesRepository, IInstanceManagerService instanceManagerService)
        {
            _commonRepository = commonRepository;
            _instancesAnalysisRepository = instancesAnalysisRepository;
            _prototypesRepository = prototypesRepository;
            _instancesRepository = instancesRepository;
            _instanceManagerService = instanceManagerService;
            _prototypeConverters = new JsonConverter[] {
                new PrototypeJsonConverter(),
                new TypedValueJsonConverter(),
                new ExternalSourceJsonConverter()
            };
            _instanceConverters = new JsonConverter[] {
                new InstanceJsonConverter(),
                new TypedValueJsonConverter(),
                new ExternalSourceJsonConverter()
            };
        }

        public async Task ContinueAsync()
        {
            await _instancesAnalysisRepository.ContinueAsync();
        }

        public async Task RestartAsync()
        {
            await _instancesAnalysisRepository.RestartAsync();
        }

        public async Task StopAsync()
        {
            await _instancesAnalysisRepository.StopAsync();
        }

        public async Task<dynamic> UpdateAsync()
        {
            var state = await _instancesAnalysisRepository.UpdateAsync();
            return state.Dump();
        }
    }
}