using CheckListManager.Common.Core.Web.Security;
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

        public async Task<ListResponse> BindAsync(DefaultIdentity identity, ListRequest form)
        {
            var response = new ListResponse();
            var department = form.Department;

            if (form.Source == null)
            {
                //initial load

                response.Fields["Name"] = new Field { Value = form.Name };

                var departments = await _commonRepository.GetDepartmentsAsync(identity, "Prototypes", "View");
                response.Fields["Department"] = new Field
                {
                    Options = departments.OrderBy(x => x.Name).Select(d => new Option(d.ID.ToString(), d.Name)),
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
                    Options = prototypeTypes.OrderBy(x => x.Name).Select(pt => new Option(pt.ID.ToString(), $"{pt.Name} ({pt.Departments.Name})")),
                    Value = form.PrototypeType.HasValue && prototypeTypes.Any(pt => pt.ID == form.PrototypeType) ? form.PrototypeType.Value.ToString() : null
                };
            }

            return response;
        }

        public async Task<PagingResponse<List<ListRow>>> GetDataAsync(IDefaultIdentity identity, PagingRequest<ListRequest> form)
        {
            if (form.Data.State == null) form.Data.State = string.Empty;
            return await _prototypesRepository.GetPrototypeVersionsAsync(
                identity.CompanyId,
                p => new ListRow
                {
                    Id = p.Prototype,
                    Name = p.Prototypes.Name,
                    Description = p.Description,
                    State = p.VersionState,
                    Version = p.Version,
                    ValidityFrom = p.ValidFrom,
                    ValidityUntil = p.ValidUntil,
                    Department = p.Prototypes.Department,
                    DepartmentName = p.Prototypes.Departments.Name,
                    PrototypeType = p.Prototypes.PrototypeType,
                    PrototypeTypeName = p.Prototypes.PrototypeTypes.Name,
                    CreatedBy = p.CreatedBy,
                    CreatedOn = p.CreatedOn,
                    ModifiedBy = p.ModifiedBy,
                    ModifiedOn = p.ModifiedOn
                },
                p =>
                    p.Prototypes.Company == identity.CompanyId
                    && (form.Data.ID == null || p.Prototype == form.Data.ID)
                    && (form.Data.Department == null || p.Prototypes.Department == form.Data.Department)
                    && (form.Data.PrototypeType == null || p.Prototypes.PrototypeType == form.Data.PrototypeType)
                    && (form.Data.State == string.Empty || p.VersionState == form.Data.State)
                    && (form.Data.Name == string.Empty || p.Prototypes.Name != null && p.Prototypes.Name.ToUpper().Contains(form.Data.Name.ToUpper()))
                    && (form.Data.Pending == "Include" || form.Data.Pending == "Exclude" && p.ProposedState == null || form.Data.Pending == "Filter" && p.ProposedState != null)
                    && (
                        p.Prototypes.PrototypeTypes.UserFunctions.Any(uf =>
                            uf.Company == identity.CompanyId
                            && uf.User == identity.Code
                            && uf.FunctionGroup == "Prototypes"
                            && uf.Function == "View"
                        ) || p.Prototypes.Departments.UserFunctions.Any(uf =>
                            uf.Company == identity.CompanyId
                            && uf.User == identity.Code
                            && uf.FunctionGroup == "Prototypes"
                            && uf.Function == "View"
                            && uf.PrototypeType == null
                        )
                    ),
                p => p.Prototype,
                form.CurrentPage,
                form.ItemsPerPage
            );
        }

        public async Task<List<SyntheticListRow>> GetSyntheticDataAsync(IDefaultIdentity identity)
        {
            return await _prototypesRepository.GetPrototypesAsync(
                identity.CompanyId,
                p => new SyntheticListRow
                {
                    Name = p.Name,
                    Description = p.Description
                },
                p =>
                    p.Company == identity.CompanyId
                    && (
                        p.PrototypeTypes.UserFunctions.Any(uf =>
                            uf.Company == identity.CompanyId
                            && uf.User == identity.Code
                            && uf.FunctionGroup == "Prototypes"
                            && uf.Function == "View"
                        ) || p.Departments.UserFunctions.Any(uf =>
                            uf.Company == identity.CompanyId
                            && uf.User == identity.Code
                            && uf.FunctionGroup == "Prototypes"
                            && uf.Function == "View"
                            && uf.PrototypeType == null
                        )
                    ),
                p => p.Name
            );
        }
    }
}
