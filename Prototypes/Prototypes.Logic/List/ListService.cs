using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Prototypes.Data;
using CheckListManager.Prototypes.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CheckListManager.Prototypes.Logic.List
{
    public class ListService : IListService
    {
        private readonly ICommonRepository _commonRepository;
        private readonly IPrototypesRepository _prototypesRepository;

        public ListService(ICommonRepository commonRepository, IPrototypesRepository prototypesRepository)
        {
            _commonRepository = commonRepository;
            _prototypesRepository = prototypesRepository;
        }

        public async Task<ListResponse> BindAsync(DefaultIdentity identity, PrototypeFilters form)
        {
            var response = new ListResponse();
            var department = form.Department;

            if (form.Source == null)
            {
                //initial load

                response.Fields["Name"] = new Field { Value = form.Name };
                response.Fields["ID"] = new Field { Value = form.ID.ToString() };

                var departments = await _commonRepository.GetDepartmentsAsync(identity, "Prototypes", "View");
                response.Fields["Department"] = new Field
                {
                    Options = departments.Select(d => new Option(d.ID.ToString(), d.Name)),
                    Value = department?.ToString()
                };

                var states = (await _commonRepository.GetVersionStatesAsync()).Select(x => new Option(x));
                response.Fields["State"] = new Field
                {
                    Options = states,
                    Value = form.State ?? string.Empty
                };
            }

            if (form.Source == null || form.Source == "department")
            {
                var prototypeTypes = await _commonRepository.GetPrototypeTypesAsync(identity, department ?? -1, "Prototypes", "View");
                response.Fields["PrototypeType"] = new Field
                {
                    Options = prototypeTypes.Select(pt => new Option(pt.ID.ToString(), $"{pt.Name} ({pt.DepartmentName})")),
                    Value = form.PrototypeType.HasValue && prototypeTypes.Any(pt => pt.ID == form.PrototypeType) ? form.PrototypeType.Value.ToString() : null
                };
            }

            return response;
        }

        public async Task<PagingResponse<List<PrototypeRecord>>> GetDataAsync(IDefaultIdentity identity, PagingRequest<PrototypeFilters> form) =>
            await _prototypesRepository.GetPrototypeVersionsAsync(identity, form.Data, form.CurrentPage, form.ItemsPerPage);

        public async Task<List<SyntheticRecord>> GetSyntheticDataAsync(IDefaultIdentity identity) =>
            await _prototypesRepository.GetPrototypesAsync(identity);
    }
}