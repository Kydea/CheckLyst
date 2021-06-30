using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Prototypes.Data;
using CheckListManager.Prototypes.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;
using System;
using System.Threading;
using Newtonsoft.Json;
using CheckListManager.Common.Data.ListItems;
using CheckListManager.Prototypes.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Json;
using System.Diagnostics;
using CheckListManager.Instances.Logic.Services;
using CheckListManager.Instances.Logic.Exceptions;
using CheckListManager.Instances.Data;

namespace CheckListManager.Prototypes.Logic.Analysis
{
    public class AnalysisService : IAnalysisService
    {
        private readonly ICommonRepository _commonRepository;
        private readonly IPrototypesRepository _prototypesRepository;
        private readonly IInstancesRepository _instancesRepository;
        private readonly IInstanceManagerService _instanceManagerService;
        private readonly JsonConverter[] _converters;

        public AnalysisService(CustomJsonConverter[] converters, ICommonRepository commonRepository, IPrototypesRepository prototypesRepository, IInstancesRepository instancesRepository, IInstanceManagerService instanceManagerService)
        {
            _converters = converters;
            _commonRepository = commonRepository;
            _prototypesRepository = prototypesRepository;
            _instancesRepository = instancesRepository;
            _instanceManagerService = instanceManagerService;
        }

        public async Task<AnalysisResponse> PrototypesAsync(DefaultIdentity identity, AnalysisRequest request, CancellationToken cancellationToken)
        {
            var response = new AnalysisResponse();
            var query = _prototypesRepository.GetPrototypeVersions()
                //.Take (20)
                //.Where(p => p.Prototypes.Name == "A8630_Line_Clearance" && p.Version == 1)
                ;

            cancellationToken.ThrowIfCancellationRequested();
            var list = await query
                .Where(p => p.ProposedState != VersionState.Obsolete)
                .OrderBy(p => p.Prototypes.Company).ThenBy(p => p.Prototype).ThenBy(p => p.Version).ToListAsync();

            cancellationToken.ThrowIfCancellationRequested();
            foreach (var prototypeVersion in list)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var prototype = JsonConvert.DeserializeObject<Prototype>(prototypeVersion.Data, _converters);
                    //prototype.InstanceCreation = true; //InstanceCreation = true for complete test
                    prototype.Setup(false);
                    if (!prototype.Valid)
                    {
                        var errors = new List<string>();
                        errors.AddRange(prototype.Errors.Select(e => $"Prototype property {e.Property}: {e.Message}"));

                        foreach (var expression in prototype.Expressions.Values)
                            if (expression.Error != null)
                                errors.Add($"Expression {expression.Code}: {expression.Error} (at row {expression.ErrorPosition?.Row.ToString() ?? "n/a"} and column {expression.ErrorPosition?.Column.ToString() ?? "n/a"})");

                        foreach (Page page in prototype.Pages)
                            if (page.Errors.Any())
                                errors.Add($"{page.ToString()}: {string.Join(" - ", page.Errors.Select(e => $"Property {e.Property}: {e.Message}"))}");

                        foreach (Item item in prototype.Items.Values)
                            if (item.Errors.Any())
                                errors.Add($"{item.ToString()}: {string.Join(" - ", item.Errors.Select(e => $"Property {e.Property}: {e.Message}"))}");

                        foreach (var source in prototype.ExternalSources.Values)
                            if (source.Errors.Any())
                                errors.Add($"Source {source.Code}: {string.Join(" - ", source.Errors.Select(e => $"Prototype property {e.Property}: {e.Message}"))}");

                        throw new Exception(string.Join(Environment.NewLine, errors));
                    }

                    try
                    {
                        if (prototypeVersion.VersionState != VersionState.Obsolete && prototypeVersion.VersionState != VersionState.Draft) _instanceManagerService.CreateInstanceAsync(prototypeVersion);
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Intance creation: {e.InnerException?.Message ?? e.Message}");
                    }

                    if (!response.Counts.TryGetValue(prototypeVersion.VersionState, out var count))
                        response.Counts[prototypeVersion.VersionState] = count = new AnalysisResponse.CountRecord();
                    ++count.OK;
                }
                catch (Exception e)
                {
                    var Key = $"{prototypeVersion.Prototypes.Company}.{prototypeVersion.Prototypes.Name}.{prototypeVersion.Version}.{prototypeVersion.VersionState}";
                    if (!response.Errors.TryGetValue(Key, out var error))
                        response.Errors[Key] = error = new List<string>();
                    error.Add(e.Message);

                    if (!response.Counts.TryGetValue(prototypeVersion.VersionState, out var count))
                            response.Counts[prototypeVersion.VersionState] = count = new AnalysisResponse.CountRecord();
                    ++count.KO;
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            response.End = DateTimeOffset.Now;
            return response;
        }

        public async Task<AnalysisResponse> InstancesAsync(DefaultIdentity identity, AnalysisRequest request, CancellationToken cancellationToken)
        {
            var response = new AnalysisResponse();
            var query = _instancesRepository.GetInstancesAsync()
                //.Take (20)
                //.Where(p => p.Prototypes.Name == "A8630_Line_Clearance" && p.Version == 1)
                ;

            cancellationToken.ThrowIfCancellationRequested();
            var list = await query
                .OrderBy(i => i.Prototypes.Company).ThenBy(p => p.Prototype).ThenBy(p => p.Version).ToListAsync();

            cancellationToken.ThrowIfCancellationRequested();
            foreach (var instance in list)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
                catch (Exception e)
                {
                    var Key = $"{instance.Company}.{instance.Prototypes.Name}.{instance.Version}.{instance.Year}.{instance.Number}";
                    if (!response.Errors.TryGetValue(Key, out var error))
                        response.Errors[Key] = error = new List<string>();
                    error.Add(e.Message);

                    if (!response.Counts.TryGetValue("Instance", out var count))
                        response.Counts["Instance"] = count = new AnalysisResponse.CountRecord();
                    ++count.KO;
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            response.End = DateTimeOffset.Now;
            return response;
        }
    }
}