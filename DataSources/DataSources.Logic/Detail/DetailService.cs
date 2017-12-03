using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.DataSources.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckListManager.DataSources.Logic.Detail
{
    public class DetailService : IDetailService
    {
        private readonly IDataSourcesRepository _dataSourcesRepository;

        public DetailService(IDataSourcesRepository dataSourcesRepository)
        {
            _dataSourcesRepository = dataSourcesRepository;
        }

        private DetailResponse Validate(DetailRequest record)
        {
            var response = new DetailResponse();
            if (string.IsNullOrWhiteSpace(record.Code))
                response.Fields["Code"] = new Field
                {
                    ValidationMessages = new List<ValidationMessage> {
                        new ValidationMessage {
                            Level = ValidationMessage.LevelError,
                            Message = "Invalid code"
                        }
                    }
                };

            response.Valid = response.Fields.Count() == 0;
            return response;
        }

        public async Task<DetailResponse> AddAsync(DefaultIdentity identity, DetailRequest record)
        {
            var response = Validate(record);
            if (response.Valid)
                try
                {
                    await _dataSourcesRepository.AddDataSourceAsync(identity, new Common.Data.DataSources
                    {
                        Code = record.Code,
                        Description = record.Description
                    });
                }
                catch (Exception Exception)
                {
                    response.ValidationMessages.Add(new ValidationMessage
                    {
                        Level = ValidationMessage.LevelError,
                        Message = Exception.Message
                    });
                    response.Valid = false;
                }

            return response;
        }

        public async Task<DetailResponse> EditAsync(DefaultIdentity identity, DetailRequest record)
        {
            var response = Validate(record);
            if (response.Valid)
                try
                {
                    await _dataSourcesRepository.EditDataSourceAsync(identity, new Common.Data.DataSources
                    {
                        Code = record.Code,
                        Description = record.Description
                    });
                }
                catch (Exception Exception)
                {
                    response.ValidationMessages.Add(new ValidationMessage
                    {
                        Level = ValidationMessage.LevelError,
                        Message = Exception.Message
                    });
                    response.Valid = false;
                }

            return response;
        }

        public async Task<DetailResponse> DeleteAsync(DefaultIdentity identity, string code)
        {
            var response = new DetailResponse();
            try
            {
                await _dataSourcesRepository.DeleteDataSourceAsync(identity, code);
            }
            catch (Exception Exception)
            {
                response.ValidationMessages.Add(new ValidationMessage
                {
                    Level = ValidationMessage.LevelError,
                    Message = Exception.Message
                });
                response.Valid = false;
            }

            return response;
        }

    }
}
