using CheckListManager.Common.Core.Helpers;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using CheckListManager.Instances.Logic.Models.CheckLists;
using CheckListManager.Instances.Logic.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace CheckListManager.MigrationTools.Terminal.InstancesAnalysis
{
    public class InstancesAnalysisService : IInstancesAnalysisService
    {
        private ICommonRepository _commonRepository;
        private IInstancesAnalysisRepository _instancesAnalysisRepository;
        private IInstanceManagerService _instanceManagerService;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public InstancesAnalysisService(ICommonRepository commonRepository, IInstancesAnalysisRepository instancesAnalysisRepository, IInstanceManagerService instanceManagerService)
        {
            _commonRepository = commonRepository;
            _instancesAnalysisRepository = instancesAnalysisRepository;
            _instanceManagerService = instanceManagerService;
        }

        public async Task AnalyzeAsync(CancellationToken cancellationToken)
        {
            try
            {
                var list = await _instancesAnalysisRepository.GetInstancesAsync();

                IDefaultIdentity identity = null;
                Companies company = null;
                long currentInstance = -1;

                var state = await _instancesAnalysisRepository.GetStateAsync();
                if (!state.Started) return;

                if (!list.Any())
                    try
                    {
                        await _instancesAnalysisRepository.StopAsync();
                    }
                    catch (Exception Exception)
                    {
                        throw new SystemException($"Error stopping the process", Exception);
                    }

                foreach (var record in list)
                {
                    try
                    {

                        try
                        {
                            state = await _instancesAnalysisRepository.GetStateAsync();
                            if (!state.Started) break;
                        }
                        catch (Exception Exception)
                        {
                            throw new SystemException("Error retrieving state", Exception);
                        }

                        if (company?.ID != record.Company)
                        {
                            company = await _commonRepository.GetCompanyAsync(record.Company);
                            identity = new DefaultIdentityHelper(company.ID);
                        }

                        currentInstance = record.ID;
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

                        try
                        {
                            await _instancesAnalysisRepository.OkAsync(record.ID);
                        }
                        catch (Exception Exception)
                        {
                            throw new SystemException("Error incrementing ok", Exception);
                        }

                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                    catch (SystemException)
                    {
                        throw;
                    }
                    catch (OutOfMemoryException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, $"Migration tool instance error (code {record.Company}.{record.PrototypeName}.{record.Version}.{record.Year}.{record.Number}, state {record.InstanceState})");

                        var errors = new List<string>();
                        if (e is ValidationException validationException)
                            errors.AddRange(validationException.Messages);
                        else
                            errors.Add(e.Message);

                        var error = string.Join("<br />", errors);
                        try
                        {
                            await _instancesAnalysisRepository.ErrorAsync($"{record.Company}.{record.PrototypeName}.{record.Version}.{record.Year}.{record.Number}.{record.InstanceState}", error, currentInstance);
                        }
                        catch (Exception Exception)
                        {
                            throw new SystemException($"Error tracing exception {error} on intance {record.ID}", Exception);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Migration tool system error");
                try
                {
                    await _instancesAnalysisRepository.ErrorAsync("System error", e.Message, -1);
                }
                catch (Exception Exception)
                {
                    Logger.Error(Exception, "Migration tool tracing error");
                }
            }
        }
    }
}