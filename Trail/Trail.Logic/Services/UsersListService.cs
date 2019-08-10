using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.Trail;
using CheckListManager.Trail.Data;
using Newtonsoft.Json;

namespace CheckListManager.Trail.Logic.Services
{
    public class UsersListService : IUsersListService
    {
        private readonly ITrailRepository _trailRepository;

        public UsersListService(ITrailRepository trailRepository)
        {
            _trailRepository = trailRepository;
        }

        public async Task<List<UsersListResponse>> GetDataAsync(long company, UsersListRequest form)
        {
            var records = await _trailRepository.GetRecordsAsync(
                company,
                r => r,
                r =>
                    r.Company == company
                    && (form.From == null || r.Date >= form.From)
                    && (form.To == null || r.Date <= form.WholeTo)
                    && (form.ActionBy == string.Empty || r.User != null && r.User.ToUpper().Contains(form.ActionBy.ToUpper()))
                    && r.Action != null && (r.Action == TrailAction.UserCreated || r.Action == TrailAction.UserEdited || r.Action == TrailAction.UserDeleted),
            r => r.Date
            );

            var list = new List<UsersListResponse>();
            foreach (var record in records)
            {
                if (!string.IsNullOrEmpty(record.Information))
                {
                    UsersListResponse.UserInformation user;
                    try
                    {
                        user = JsonConvert.DeserializeObject<UsersListResponse.UserInformation>(record.Information);
                    }
                    catch (Exception)
                    {
                        user = new UsersListResponse.UserInformation
                        {
                            Code = !record.Information.Contains("{") ? record.Information : ""
                        };
                    }
                    
                    if ((string.IsNullOrEmpty(user.Code) && string.IsNullOrEmpty(form.User) ||
                         user.Code != null && user.Code.ToUpper().Contains(form.User.ToUpper()))
                        && (string.IsNullOrEmpty(user.Email) && string.IsNullOrEmpty(form.Email) ||
                            user.Email != null && user.Email.ToUpper().Contains(form.Email.ToUpper()))
                        && (string.IsNullOrEmpty(user.NameAndSurname) && string.IsNullOrEmpty(form.Name) ||
                            user.NameAndSurname != null && user.NameAndSurname.ToUpper().Contains(form.Name.ToUpper())))
                    {
                        list.Add(new UsersListResponse
                        {
                            Id = record.ID,
                            // ReSharper disable once PossibleInvalidOperationException
                            Company = record.Company.Value,
                            Date = record.Date,
                            Action = record.Action,
                            ActionBy = record.User,
                            User = user.Code,
                            Information = record.Information
                        });
                    }
                }
            }

            return list;
        }

        public async Task<PagingResponse<List<UsersListResponse>>> GetDataAsync(long company, PagingRequest<UsersListRequest> form)
        {
            var data = await GetDataAsync(company, form.Data);
            var count = data.Count;

            return new PagingResponse<List<UsersListResponse>>
            {
                Data = data.Skip(form.Skip).Take(form.ItemsPerPage).ToList(),
                Count = count,
                ItemsPerPage = form.ItemsPerPage,
                CurrentPage = form.CurrentPage
            };
        }
    }
}