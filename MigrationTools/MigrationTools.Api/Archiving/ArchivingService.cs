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
using CheckListManager.Prototypes.Data.Archiving;
using CheckListManager.Instances.Logic.Models;
using CheckListManager.Instances.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Prototypes.Api;

namespace CheckListManager.Prototypes.Logic.Archiving
{
    public class ArchivingService : IArchivingService
    {
        private readonly ICommonRepository _commonRepository;
        private readonly IArchivingRepository _archivingRepository;
        private readonly IInstancesRepository _instancesRepository;
        private readonly IInstanceManagerService _instanceManagerService;
        private readonly JsonConverter[] _instanceConverters;

        public ArchivingService(ICommonRepository commonRepository, IArchivingRepository archivingRepository, IInstancesRepository instancesRepository, IInstanceManagerService instanceManagerService)
        {
            _commonRepository = commonRepository;
            _archivingRepository = archivingRepository;
            _instancesRepository = instancesRepository;
            _instanceManagerService = instanceManagerService;
            _instanceConverters = new JsonConverter[] {
                new InstanceJsonConverter(),
                new TypedValueJsonConverter(),
                new ExternalSourceJsonConverter()
            };
        }

        public async Task<Dictionary<long, string>> GetCompaniesAsync()
        {
            var companies = await _commonRepository.GetCompaniesAsync();
            return companies.ToDictionary(k => k.ID, v => v.Name);
        }

        public async Task ExportAsync(DefaultIdentity identity, MigrationTools.Api.Monitor monitor, ArchivingRequest request)
        {
            try
            {
                monitor.Counts.Clear();
                monitor.Errors.Clear();

                var query = _archivingRepository.GetInstances(request.Company)
                    .Take(10)
                    ;
                monitor.Start(query.Count());

                var list = await query
                    .OrderBy(p => p.Prototypes.Company).ThenBy(p => p.Prototype).ThenBy(p => p.Version).ToListAsync();

                foreach (var instance in list)
                {
                    try
                    {
                        if (monitor.Aborted) break;
                        monitor.Inc();
                        var prototype = JsonConvert.DeserializeObject<Prototype>(instance.Data, _instanceConverters);
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
                        }

                        if (!monitor.Counts.TryGetValue(instance.InstanceState, out var count))
                            monitor.Counts[instance.InstanceState] = count = new MigrationTools.Api.Monitor.CountRecord();
                        ++count.ok;
                    }
                    catch (Exception e)
                    {
                        var Key = $"{instance.Prototypes.Company}.{instance.Prototypes.Name}.{instance.Version}.{instance.InstanceState}";
                        if (!monitor.Errors.TryGetValue(Key, out var error))
                            monitor.Errors[Key] = error = new List<string>();
                        error.Add(e.Message);

                        if (!monitor.Counts.TryGetValue(instance.InstanceState, out var count))
                            monitor.Counts[instance.InstanceState] = count = new MigrationTools.Api.Monitor.CountRecord();
                        ++count.ko;
                    }
                }
            }
            finally
            {
                monitor.Stop();
            }
        }
    }
}