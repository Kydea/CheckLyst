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
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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

        public async Task StartAsync(MigrationTools.Api.Monitor monitor, InstancesAnalysisRequest request)
        {
            try
            {
                monitor.Counts.Clear();
                monitor.Errors.Clear();

                var list = await _instancesAnalysisRepository.GetInstances("OK");
                var okCount = list.Count();

                list = await _instancesAnalysisRepository.GetInstances("KO");
                var koCount = list.Count();

                list = await _instancesAnalysisRepository.GetInstances(null)
                    //.Take(10)
                    //.Where(p => p.Prototypes.Name == "A8630_Line_Clearance" && p.Version == 1)
                    //.Where(i => i.Prototype == 82)
                    ;
                monitor.Start(okCount + koCount + list.Count());

                monitor.Counts.Add("Instances", new MigrationTools.Api.Monitor.CountRecord {
                    ok = okCount,
                    ko = koCount
                });

                monitor.Inc(okCount + koCount);

                IDefaultIdentity identity = null;
                Companies company = null;

                foreach (var record in list)
                {
                    try
                    {
                        if (monitor.Aborted) break;
                        monitor.Inc();

                        if (company?.ID != record.Company)
                        {
                            company = await _commonRepository.GetCompanyAsync(record.Company);
                            identity = new DefaultIdentityHelper(company.ID);
                        }

                        var cacheObject = await _instanceManagerService.GetCacheObjectForDownloadAsync(record.Company, record.Code, true);
                        var errors = new List<string>();
                        foreach (Item item in cacheObject.Instance.Items.Values)
                            if (
                                item is Signature signature 
                                && item.WorseCountDownToValue > signature.SignedOn
                            )
                                errors.Add($"Item: {item.TypeName} {item.Id} {item.Code ?? "-"}, firma: {signature.SignedOn.Format(company.TimeZone)}, count down: {item.WorseCountDownToValue?.Format(company.TimeZone)}");

                        if (errors.Any())
                            throw new ValidationException(errors);

                        ++monitor.Counts["Instances"].ok;

                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                    catch (OutOfMemoryException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        var Key = $"{record.Company}.{record.PrototypeName}.{record.Version}.{record.Year}.{record.Number}.{record.InstanceState}";
                        Logger.Error(e, $"Migration tool instance error ({Key})");

                        if (!monitor.Errors.TryGetValue(Key, out var error))
                            monitor.Errors[Key] = error = new List<string>();
                        if (e is ValidationException validationException)
                            error.AddRange(validationException.Messages);
                        else
                            error.Add(e.Message);

                        if (!monitor.Counts.TryGetValue("Instances", out var count))
                            monitor.Counts["Instances"] = count = new MigrationTools.Api.Monitor.CountRecord();

                        ++monitor.Counts["Instances"].ko;
                    }
                }
            }
            catch(Exception e)
            {
                Logger.Error(e, "Migration tool system error");
                var Key = $"System";
                if (!monitor.Errors.TryGetValue(Key, out var error))
                    monitor.Errors[Key] = error = new List<string>();
                else
                    error.Add(e.Message);
            }
            finally
            {
                monitor.Stop();
            }
        }
    }
}