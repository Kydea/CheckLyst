using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Data.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Prototypes.Logic.List
{
    public class ListService: IListService
    {
        private readonly IInfrastructureRepository _infrastructureRepository;

        public ListService(IInfrastructureRepository infrastructureRepository)
        {
            _infrastructureRepository = infrastructureRepository;
        }

        public async Task<ListResponse> GetResponse(long Company, ListRequest form)
        {
            ListResponse response = new ListResponse(form);
            response.FieldsValues["Department"] = (await _infrastructureRepository.GetDepartmentsAsync(Company, d => new { ID = d.ID, Name = d.Name })).ToDictionary(k => k.ID.ToString(), v => v.Name);
            return response;
        }
    }
}
