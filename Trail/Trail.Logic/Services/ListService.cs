using System.Collections.Generic;
using System.Threading.Tasks;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.Trail;
using CheckListManager.Trail.Data;

namespace CheckListManager.Trail.Logic.Services
{
    public class ListService : IListService
    {
        private readonly ITrailRepository _trailRepository;

        public ListService(ITrailRepository trailRepository)
        {
            _trailRepository = trailRepository;
        }

        public Task<List<TrailRow>> GetDataAsync(long companyId, ListRequest form)
        {
            if (form.Action == null)
                form.Action = string.Empty;

            return _trailRepository.GetRecordsAsync(
                companyId,
                r => new TrailRow
                {
                    ID = r.ID,
                    Date = r.Date,
                    User = r.User,
                    Action = r.Action,
                    CheckListCode = r.CheckListCode,
                    CheckListID = r.CheckListID,
                    Information = r.Information
                },
                r =>
                    r.Company == companyId
                    && (form.From == null || r.Date >= form.From)
                    && (form.To == null || r.Date <= form.WholeTo)
                    && (form.User == string.Empty || r.User != null && r.User.ToUpper().Contains(form.User.ToUpper()))
                    && (form.CheckListCode == string.Empty || r.CheckListCode != null && r.CheckListCode.ToUpper().Contains(form.CheckListCode.ToUpper()))
                    && (form.CheckListID == null || r.CheckListID == form.CheckListID)
                    && (form.Action == string.Empty || r.Action != null && r.Action.ToUpper().Contains(form.Action.ToUpper()))
                    && (form.Information == string.Empty || r.Information != null && r.Information.ToUpper().Contains(form.Information.ToUpper())),
                r => r.Date
            );
        }

        public Task<PagingResponse<List<TrailRow>>> GetDataAsync(long companyId, PagingRequest<ListRequest> form)
        {
            if (form.Data.Action == null)
                form.Data.Action = string.Empty;

            return _trailRepository.GetRecordsAsync(
                companyId,
                r => new TrailRow
                {
                    ID = r.ID,
                    Date = r.Date,
                    User = r.User,
                    Action = r.Action,
                    CheckListCode = r.CheckListCode,
                    CheckListID = r.CheckListID,
                    Information = r.Information,
                    ShortInformation = r.Information != null && r.Information.StartsWith("{") ? "..." : r.Information
                },
                r =>
                    r.Company == companyId
                    && (form.Data.From == null || r.Date >= form.Data.From)
                    && (form.Data.To == null || r.Date <= form.Data.WholeTo)
                    && (form.Data.User == string.Empty || r.User != null && r.User.ToUpper().Contains(form.Data.User.ToUpper()))
                    && (form.Data.CheckListCode == string.Empty || r.CheckListCode != null && r.CheckListCode.ToUpper().Contains(form.Data.CheckListCode.ToUpper()))
                    && (form.Data.CheckListID == null || r.CheckListID == form.Data.CheckListID)
                    && (form.Data.Action == string.Empty || r.Action != null && r.Action.ToUpper().Contains(form.Data.Action.ToUpper()))
                    && (form.Data.Information == string.Empty || r.Information != null && r.Information.ToUpper().Contains(form.Data.Information.ToUpper())),
                r => r.Date,
                form.CurrentPage,
                form.ItemsPerPage
            );
        }
    }
}
