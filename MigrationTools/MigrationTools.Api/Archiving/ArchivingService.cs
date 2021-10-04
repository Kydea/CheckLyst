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
using CheckListManager.Instances.Logic.PDF;
using CheckListManager.Instances.Data;
using CheckListManager.Prototypes.Data.Archiving;
using CheckListManager.Instances.Logic.Models;
using CheckListManager.Instances.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Prototypes.Api;
using System.IO;
using CheckListManager.Common.Logic.Services;
using System.Text;
using CheckListManager.Common.Core.Helpers;

namespace CheckListManager.Prototypes.Logic.Archiving
{
    public class ArchivingService : IArchivingService
    {
        private readonly ICommonRepository _commonRepository;
        private readonly IArchivingRepository _archivingRepository;
        private readonly IInstancesRepository _instancesRepository;
        private readonly IInstanceManagerService _instanceManagerService;
        private readonly IConfigurationService _configurationService;
        private readonly IStorageService _storageService;
        private readonly JsonConverter[] _instanceConverters;

        private const string CSV_SEPARATOR = ";";

        public ArchivingService(
            ICommonRepository commonRepository,
            IArchivingRepository archivingRepository,
            IInstancesRepository instancesRepository,
            IInstanceManagerService instanceManagerService,
            IConfigurationService configurationService,
            IStorageService storageService
        )
        {
            _commonRepository = commonRepository;
            _archivingRepository = archivingRepository;
            _instancesRepository = instancesRepository;
            _instanceManagerService = instanceManagerService;
            _configurationService = configurationService;
            _storageService = storageService;
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

        private string EscapeCSVToken(string Value)
        {
            return $"\"{Value?.Replace("\"", "\"\"") ?? string.Empty}\"";
        }

        private async Task ExportInstancesAsync(DefaultIdentity identity, MigrationTools.Api.Monitor monitor, List<Common.Data.Instances> instances)
        {
            var company = await _commonRepository.GetCompanyAsync(monitor.CompanyID);
            var csvFileName = $"Instances.csv";
            monitor.Files.Add(csvFileName);

            //creare file temporaneo?
            using (FileStream FileStream = File.Create(Path.Combine(Folder, csvFileName)))
            {
                byte[] Content = new UTF8Encoding(true).GetBytes(
                    EscapeCSVToken("Code") + CSV_SEPARATOR +
                    EscapeCSVToken("Description") + CSV_SEPARATOR +
                    EscapeCSVToken("Created By") + CSV_SEPARATOR +
                    EscapeCSVToken("Created On") + CSV_SEPARATOR +
                    EscapeCSVToken("Modified By") + CSV_SEPARATOR +
                    EscapeCSVToken("Modified On") + CSV_SEPARATOR +
                    EscapeCSVToken("State") +
                    Environment.NewLine
                );

                FileStream.Write(Content, 0, Content.Length);

                foreach (var instance in instances)
                {
                    try
                    {
                        if (monitor.Aborted) break;
                        monitor.Inc();

                        var cacheObject = await _instanceManagerService.GetCacheObjectForDownloadAsync(company.ID, instance.Code, true);

                        var jsonFileName = $"{cacheObject.InstanceCode}.json";
                        monitor.Files.Add(jsonFileName);
                        //export instance.Data on amazon as jsonFileName

                        var PDF = new InstancePDF(company.ID, company.Name, company.TimeZone, _configurationService.PublicRepositoryUrl, cacheObject, _instancesRepository, _storageService, _configurationService);
                        var MemoryStream = new MemoryStream();
                        await PDF.GenerateAsync(MemoryStream);

                        var pdfFileName = $"{cacheObject.InstanceCode}.pdf";
                        monitor.Files.Add(pdfFileName);
                        //export memory stream on amazon as pdfFileName

                        Content = new UTF8Encoding(true).GetBytes(
                            cacheObject.InstanceCode + CSV_SEPARATOR +
                            EscapeCSVToken(instance.Description) + CSV_SEPARATOR +
                            EscapeCSVToken(instance.CreatedBy) + CSV_SEPARATOR +
                            EscapeCSVToken(instance.CreatedOn.Format(company.TimeZone)) + CSV_SEPARATOR +
                            EscapeCSVToken(instance.ModifiedBy) + CSV_SEPARATOR +
                            EscapeCSVToken(instance.ModifiedOn?.Format(company.TimeZone)) + CSV_SEPARATOR +
                            EscapeCSVToken(instance.InstanceState) + CSV_SEPARATOR +
                            Environment.NewLine
                        );

                        FileStream.Write(Content, 0, Content.Length);

                        if (!monitor.Counts.TryGetValue("Instances - " + instance.InstanceState, out var count))
                            monitor.Counts["Instances - " + instance.InstanceState] = count = new MigrationTools.Api.Monitor.CountRecord();
                        ++count.ok;
                    }
                    catch (Exception e)
                    {
                        var Key = $"{instance.Prototypes.Company}.{instance.Prototypes.Name}.{instance.Version}.{instance.InstanceState}";
                        if (!monitor.Errors.TryGetValue(Key, out var error))
                            monitor.Errors[Key] = error = new List<string>();
                        error.Add(e.Message);

                        if (!monitor.Counts.TryGetValue("Instances - " + instance.InstanceState, out var count))
                            monitor.Counts["Instances - " + instance.InstanceState] = count = new MigrationTools.Api.Monitor.CountRecord();
                        ++count.ko;
                    }
                }

                //export file temporaneo su amazon s3 as csvFileName
            }
        }

        private async Task ExportTrailRecordsAsync(DefaultIdentity identity, MigrationTools.Api.Monitor monitor, IQueryable<Records> trailRecords)
        {
            var Key = "Audit trail records";

            var company = await _commonRepository.GetCompanyAsync(monitor.CompanyID);
            MigrationTools.Api.Monitor.CountRecord count = monitor.Counts[Key] = new MigrationTools.Api.Monitor.CountRecord();

            var MinDate = await trailRecords.MinAsync(x => x.Date);
            var MaxDate = await trailRecords.MaxAsync(x => x.Date);

            var Month = new DateTimeOffset(MinDate.Year, MinDate.Month, 1, 0, 0, 0, MinDate.Offset);
            while (Month <= MaxDate)
                try
                {
                    var NextMonth = Month.AddMonths(1);
                    var MonthRecords = await trailRecords.Where(x => x.Date >= Month && x.Date < NextMonth).OrderBy(x => x.ID).ToListAsync();

                    var FileName = $"AuditTrail{Month:-yyyy-MM}.csv";
                    monitor.Files.Add(FileName);

                    //creare file temporaneo?
                    using (FileStream FileStream = File.Create(Path.Combine(Folder, FileName)))
                    {
                        byte[] Content = new UTF8Encoding(true).GetBytes(
                            EscapeCSVToken("Id") + CSV_SEPARATOR +
                            EscapeCSVToken("Date") + CSV_SEPARATOR +
                            EscapeCSVToken("User") + CSV_SEPARATOR +
                            EscapeCSVToken("Checklist ID") + CSV_SEPARATOR +
                            EscapeCSVToken("Checklist code") + CSV_SEPARATOR +
                            EscapeCSVToken("Action") + CSV_SEPARATOR +
                            EscapeCSVToken("Detail") +
                            Environment.NewLine
                        );

                        FileStream.Write(Content, 0, Content.Length);

                        foreach (var Record in MonthRecords)
                        {
                            if (monitor.Aborted) break;
                            monitor.Inc();
                            ++count.ok;

                            Content = new UTF8Encoding(true).GetBytes(
                                Record.ID + CSV_SEPARATOR +
                                EscapeCSVToken(Record.Date.Format(company.TimeZone)) + CSV_SEPARATOR +
                                EscapeCSVToken(Record.User) + CSV_SEPARATOR +
                                EscapeCSVToken(Record.CheckListID.ToString()) + CSV_SEPARATOR +
                                EscapeCSVToken(Record.CheckListCode) + CSV_SEPARATOR +
                                EscapeCSVToken(Record.Action) + CSV_SEPARATOR +
                                EscapeCSVToken(Record.Information) +
                                Environment.NewLine
                            );

                            FileStream.Write(Content, 0, Content.Length);
                        }
                    }
                    //export file temporaneo su amazon s3 as csvFileName

                    Month = Month.AddMonths(1);
                }
                catch (Exception e)
                {
                    if (!monitor.Errors.TryGetValue(Key, out var error))
                        monitor.Errors[Key] = error = new List<string>();
                    error.Add(e.Message);

                    ++count.ko;
                }
        }

        private void ExportSummary(DefaultIdentity identity, MigrationTools.Api.Monitor monitor)
        {
            //creare file temporaneo?
            var summaryFileName = "Summary.txt";
            monitor.Files.Add(summaryFileName);
            using (FileStream FileStream = File.Create(Path.Combine(Folder, summaryFileName)))
            {
                byte[] Content = new UTF8Encoding(true).GetBytes(
                    string.Join(Environment.NewLine, monitor.Files.OrderBy(f => f))
                );

                FileStream.Write(Content, 0, Content.Length);
            }

            //export file temporaneo su amazon s3 as csvFileName
        }

        public async Task ExportAsync(DefaultIdentity identity, MigrationTools.Api.Monitor monitor, ArchivingRequest request)
        {
            try
            {
                if (monitor.Running) throw new InvalidOperationException($"The process is already running for the company {monitor.CompanyID} - {monitor.CompanyDescription}");
                monitor.Counts.Clear();
                monitor.Errors.Clear();
                monitor.Files.Clear();

                var intancesQuery = _archivingRepository.GetInstances(request.Company)
                    .Where(x => request.Instances ? true : false)
                    .Take(10)
                    ;

                var trailRecordsQuery = _archivingRepository.GetTrailRecords(request.Company)
                    .Where(x => request.AuditTrail ? true : false)
                    .Take(10)
                    ;

                monitor.Start(intancesQuery.Count() + trailRecordsQuery.Count());

                await ExportInstancesAsync(identity, monitor, await intancesQuery.OrderBy(i => i.Prototypes.Company).ThenBy(i => i.Prototype).ThenBy(i => i.Version).ThenBy(i => i.Year).ThenBy(i => i.Number).ThenBy(i => i.ID).ToListAsync());
                await ExportTrailRecordsAsync(identity, monitor, trailRecordsQuery);
                ExportSummary(identity, monitor);
            }
            finally
            {
                monitor.Stop();
            }
        }
    }
}