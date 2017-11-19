using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Prototypes.Data;
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

        public async Task<ListResponse> BindAsync(long company, ListRequest form)
        {
            var response = new ListResponse();
            var department = form.Department;

            if (form.Source == null)
            {
                //initial load

                response.Fields["Name"] = new Field { Value = form.Name };

                var departments = await _commonRepository.GetDepartmentsAsync(company);
                department = form.Department.HasValue && departments.Any(d => d.ID == form.Department) ? form.Department.Value : departments.FirstOrDefault()?.ID;
                response.Fields["Department"] = new Field
                {
                    Options = departments.Select(d => new Option(d.ID.ToString(), d.Name)),
                    Value = department?.ToString()
                };
                response.Valid &= departments.Any();

                var states = new List<Option> { new Option(null, "All") };
                states.AddRange((await _commonRepository.GetVersionStatesAsync()).Select(x => new Option(x)));
                response.Fields["State"] = new Field
                {
                    Options = states,
                    Value = form.State
                };
            }

            if (form.Source == null || form.Source == "department")
            {
                var prototypeTypes = await _commonRepository.GetPrototypeTypesAsync(company, department ?? -1);
                response.Fields["PrototypeType"] = new Field
                {
                    Options = prototypeTypes.Select(pt => new Option(pt.ID.ToString(), pt.Description)),
                    Value = form.PrototypeType.HasValue && prototypeTypes.Any(pt => pt.ID == form.PrototypeType) ? form.PrototypeType.Value.ToString() : prototypeTypes.FirstOrDefault()?.ID.ToString()
                };
                response.Valid &= prototypeTypes.Any();

            }

            return response;
        }

        public async Task<PagingResponse<List<ListRow>>> GetDataAsync(long company, PagingRequest<ListRequest> form)
            =>
                await _prototypesRepository.GetPrototypesAsync(
                    company,
                    p => new ListRow
                    {
                        Id = p.Prototype,
                        Name = p.Prototypes.Name,
                        Description = p.Description,
                        State = p.VersionState,
                        Version = p.Version,
                        ValidityFrom = p.ValidFrom,
                        ValidityUntil = p.ValidUntil,
                        CreatedBy = p.CreatedBy,
                        CreatedOn = p.CreatedOn,
                        ModifiedBy = p.ModifiedBy,
                        ModifiedOn = p.ModifiedOn
                    },
                    p =>
                        p.Prototypes.Company == company
                        && p.Prototypes.Department == form.Data.Department
                        && p.Prototypes.PrototypeType == form.Data.PrototypeType
                        && (form.Data.State == "All" || p.VersionState == form.Data.State)
                        && (form.Data.Name == string.Empty || p.Description != null && p.Description.ToUpper().Contains(form.Data.Name.ToUpper())),
                    p => p.Prototype,
                    form.CurrentPage,
                    form.ItemsPerPage
                );
    }
}
