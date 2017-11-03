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

        public async Task<PagingResponse<List<TrailMessage>>> GetDataAsync(long Company, PagingRequest<ListRequest> Form)
            =>
                await _trailRepository.GetRecordsAsync(
                    Company,
                    p => new TrailMessage
                    {
                        Id = p.ID,
                        Date = p.Date,
                        User = p.User,
                        Action = p.Action,
                        CheckList = p.CheckList,
                        Information = p.Information
                    },
                    p =>
                        p.Company == Company
                        && (Form.Data.From == null || p.Date >= Form.Data.From)
                        && (Form.Data.To == null || p.Date <= Form.Data.To)
                        && (Form.Data.User == string.Empty || p.User != null && p.User.ToUpper().Contains(Form.Data.User.ToUpper()))
                        && (Form.Data.CheckList == string.Empty || p.CheckList != null && p.CheckList.ToUpper().Contains(Form.Data.CheckList.ToUpper()))
                        && (Form.Data.Action == string.Empty || p.Action != null && p.Action.ToUpper().Contains(Form.Data.Action.ToUpper()))
                        && (Form.Data.Information == string.Empty || p.Information != null && p.Information.ToUpper().Contains(Form.Data.Information.ToUpper())),
                    p => p.Date,
                    Form.CurrentPage,
                    Form.ItemsPerPage
                );
    }
}
