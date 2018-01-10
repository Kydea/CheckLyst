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
        public async Task<List<TrailMessage>> GetDataAsync(long companyId, ListRequest form)
            =>
                await _trailRepository.GetRecordsAsync(
                    companyId,
                    r => new TrailMessage
                    {
                        Id = r.ID,
                        Date = r.Date,
                        User = r.User,
                        Action = r.Action,
                        CheckList = r.CheckList,
                        Information = r.Information
                    },
                    r =>
                        r.Company == companyId
                        && (form.From == null || r.Date >= form.From)
                        && (form.To == null || r.Date <= form.To)
                        && (form.User == string.Empty || r.User != null && r.User.ToUpper().Contains(form.User.ToUpper()))
                        && (form.CheckList == string.Empty || r.CheckList != null && r.CheckList.ToUpper().Contains(form.CheckList.ToUpper()))
                        && (form.Action == string.Empty || r.Action != null && r.Action.ToUpper().Contains(form.Action.ToUpper()))
                        && (form.Information == string.Empty || r.Information != null && r.Information.ToUpper().Contains(form.Information.ToUpper())),
                    r => r.Date
                );

        public async Task<PagingResponse<List<TrailMessage>>> GetDataAsync(long companyId, PagingRequest<ListRequest> form)
            =>
                await _trailRepository.GetRecordsAsync(
                    companyId,
                    p => new TrailMessage
                    {
                        Id = p.ID,
                        Date = p.Date,
                        User = p.User,
                        Action = p.Action,
                        CheckList = p.CheckList,
                        Information = p.Information,
                        ShortInformation = p.Information != null && p.Information.StartsWith("{") ? "..." : p.Information
                    },
                    p =>
                        p.Company == companyId
                        && (form.Data.From == null || p.Date >= form.Data.From)
                        && (form.Data.To == null || p.Date <= form.Data.To)
                        && (form.Data.User == string.Empty || p.User != null && p.User.ToUpper().Contains(form.Data.User.ToUpper()))
                        && (form.Data.CheckList == string.Empty || p.CheckList != null && p.CheckList.ToUpper().Contains(form.Data.CheckList.ToUpper()))
                        && (form.Data.Action == string.Empty || p.Action != null && p.Action.ToUpper().Contains(form.Data.Action.ToUpper()))
                        && (form.Data.Information == string.Empty || p.Information != null && p.Information.ToUpper().Contains(form.Data.Information.ToUpper())),
                    p => p.Date,
                    form.CurrentPage,
                    form.ItemsPerPage
                );
    }
}
