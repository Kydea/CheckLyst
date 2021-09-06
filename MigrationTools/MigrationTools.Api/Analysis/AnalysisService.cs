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
using CheckListManager.Prototypes.Logic.Models.CheckLists;
using CheckListManager.Prototypes.Data.Analysis;
using CheckListManager.Instances.Logic.Models;
using CheckListManager.Instances.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.MigrationTools.Api.Analysis;

namespace CheckListManager.Prototypes.Logic.Analysis
{
    public class AnalysisService : IAnalysisService
    {
        private readonly ICommonRepository _commonRepository;
        private readonly IAnalysisRepository _analysisRepository;
        private readonly IPrototypesRepository _prototypesRepository;
        private readonly IInstancesRepository _instancesRepository;
        private readonly IInstanceManagerService _instanceManagerService;
        private readonly JsonConverter[] _converters;
        private readonly JsonConverter[] _instanceConverters;

        public AnalysisService(CustomJsonConverter[] converters, ICommonRepository commonRepository, IAnalysisRepository analysisRepository, IPrototypesRepository prototypesRepository, IInstancesRepository instancesRepository, IInstanceManagerService instanceManagerService)
        {
            _converters = converters;
            _commonRepository = commonRepository;
            _analysisRepository = analysisRepository;
            _prototypesRepository = prototypesRepository;
            _instancesRepository = instancesRepository;
            _instanceManagerService = instanceManagerService;
            _instanceConverters = new JsonConverter[] {
                new InstanceJsonConverter(),
                new TypedValueJsonConverter(),
                new ExternalSourceJsonConverter()
            };
        }

        public async Task PrototypesAsync(DefaultIdentity identity, AnalysisRequest request)
        {
            try
            {
                MigrationTools.Api.Monitor.Counts.Clear();
                MigrationTools.Api.Monitor.Errors.Clear();

                var query = _analysisRepository.GetPrototypeVersions()
                    //.Take(10)
                    //.Where(p => p.Prototypes.Name == "A8630_Line_Clearance" && p.Version == 1)
                    ;
                MigrationTools.Api.Monitor.Start(query.Count());

                var list = await query
                    .OrderBy(p => p.Prototypes.Company).ThenBy(p => p.Prototype).ThenBy(p => p.Version).ToListAsync();

                foreach (var prototypeVersion in list)
                {
                    try
                    {
                        if (MigrationTools.Api.Monitor.Aborted) break;
                        MigrationTools.Api.Monitor.Inc();
                        var prototype = JsonConvert.DeserializeObject<Prototype>(prototypeVersion.Data, _converters);
                        prototype.InstanceCreation = true; //true: only errors on instance creation, false: include errors specific of the prototype editing process
                        prototype.Setup(false);
                        if (!prototype.Valid)
                        {
                            var errors = new List<string>();
                            errors.AddRange(prototype.Errors.Select(e => $"Prototype property {e.Property}: {e.Message}"));

                            foreach (var expression in prototype.Expressions.Values)
                                if (expression.Error != null)
                                {
                                    string position = expression.ErrorPosition == null ? string.Empty : $" (at row {expression.ErrorPosition.Row + 1} and column {expression.ErrorPosition.Column + 1})";
                                    errors.Add($"Expression {expression.Code}: {expression.Error}{position}");
                                }

                            foreach (Models.CheckLists.Page page in prototype.Pages)
                                if (page.Errors.Any())
                                    errors.Add($"{page.ToString()}: {string.Join(" - ", page.Errors.Select(e => $"Property {e.Property}: {e.Message}"))}");

                            foreach (Models.CheckLists.Item item in prototype.Items.Values)
                                if (item.Errors.Any())
                                    errors.Add($"{item.ToString()}: {string.Join(" - ", item.Errors.Select(e => $"Property {e.Property}: {e.Message}"))}");

                            foreach (var source in prototype.ExternalSources.Values)
                                if (source.Errors.Any())
                                    errors.Add($"Source {source.Code}: {string.Join(" - ", source.Errors.Select(e => $"Parameter {e.Property}: {e.Message}"))}");

                            throw new ValidationException(errors);
                        }

                        try
                        {
                            if (prototypeVersion.VersionState != VersionState.Obsolete) CreateInstanceAsync(prototypeVersion);
                        }
                        catch (Exception e)
                        {
                            throw new Exception($"Intance creation: {e.InnerException?.Message ?? e.Message}");
                        }

                        if (!MigrationTools.Api.Monitor.Counts.TryGetValue(prototypeVersion.VersionState, out var count))
                            MigrationTools.Api.Monitor.Counts[prototypeVersion.VersionState] = count = new MigrationTools.Api.Monitor.CountRecord();
                        ++count.ok;
                    }
                    catch (Exception e)
                    {
                        var Key = $"{prototypeVersion.Prototypes.Company}.{prototypeVersion.Prototypes.Name}.{prototypeVersion.Version}.{prototypeVersion.VersionState}";
                        if (!MigrationTools.Api.Monitor.Errors.TryGetValue(Key, out var error))
                            MigrationTools.Api.Monitor.Errors[Key] = error = new List<string>();
                        if (e is ValidationException validationException)
                            error.AddRange(validationException.Messages);
                        else
                            error.Add(e.Message);

                        if (!MigrationTools.Api.Monitor.Counts.TryGetValue(prototypeVersion.VersionState, out var count))
                            MigrationTools.Api.Monitor.Counts[prototypeVersion.VersionState] = count = new MigrationTools.Api.Monitor.CountRecord();
                        ++count.ko;
                    }
                }
            }
            finally
            {
                MigrationTools.Api.Monitor.Stop();
            }
        }

        public InstanceCacheObject CreateInstanceAsync(PrototypeVersions prototypeVersion)
        {
            try
            {
                var instance = JsonConvert.DeserializeObject<Instance>(prototypeVersion.Data, _instanceConverters);
                if (prototypeVersion.VersionState == VersionState.InSimulation)
                    AddSimulationNote(instance);

                var cacheObject = new InstanceCacheObject
                {
                    CompanyId = prototypeVersion.Prototypes.Company,
                    Instance = instance,
                    Code = $"{prototypeVersion.Prototypes.Name:X}-{prototypeVersion.Version:X}-{Guid.NewGuid()}",
                    Prototype = prototypeVersion.Prototype,
                    PrototypeName = prototypeVersion.Prototypes.Name,
                    PrototypeDescription = prototypeVersion.Description,
                    Version = prototypeVersion.Version,
                    VersionState = prototypeVersion.VersionState,
                    InstanceState = InstanceState.Draft,
                    CreatedBy = "CheckListManager",
                    CreatedOn = DateTimeOffset.UtcNow,
                    NeedToCache = true
                };

                cacheObject.Instance.Setup(cacheObject, ".", "Atlantic Standard Time", true);
                cacheObject.Instance.UpdateAll(cacheObject);
                cacheObject.Instance.UpdateReadOnly("CheckListManager", cacheObject.InstanceState, false);

                return cacheObject;
            }
            catch (InstanceException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new InstanceException("Unable to create the instance", e);
            }
        }

        private void AddSimulationNote(Instance instance)
        {
            var SimulationPage = new Instances.Logic.Models.CheckLists.Page()
            {
                Description = "Simulation notes",
                Groups = new List<ICheckListGroup>
                {
                    new Instances.Logic.Models.CheckLists.Group
                    {
                        Description = "Simulation notes",
                        Items = new List<ICheckListItem>
                        {
                            new Instances.Logic.Models.CheckLists.Field
                            {
                                Description = "Reason",
                                LinesCount = 5
                            },
                            new Instances.Logic.Models.CheckLists.Field
                            {
                                Description = "Result",
                                LinesCount = 5
                            }

                        }
                    }
                }
            };
            instance.Pages.Add(SimulationPage);
        }
    }
}