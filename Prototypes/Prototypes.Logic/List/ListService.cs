using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Prototypes.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task<ListResponse> BindAsync(long Company, ListRequest Form)
        {
            ListResponse response = new ListResponse();
            long? Department = Form.Department;

            if (Form.Source == null)
            {
                //initial load

                response.Fields["Name"] = new Field { Value = Form.Name };

                var Departments = await _commonRepository.GetDepartmentsAsync(Company);
                Department = Form.Department.HasValue && Departments.Any(d => d.ID == Form.Department) ? Form.Department.Value : Departments.FirstOrDefault()?.ID;
                response.Fields["Department"] = new Field
                {
                    Options = Departments.Select(d => new Option(d.ID.ToString(), d.Name)),
                    Value = Department?.ToString()
                };
                response.Valid &= Departments.Any();

                var States = new List<Option>() { new Option(null, "All") };
                States.AddRange((await _commonRepository.GetVersionStatesAsync()).Select(x => new Option(x)));
                response.Fields["State"] = new Field
                {
                    Options = States,
                    Value = Form.State
                };
            }

            if (Form.Source == null || Form.Source == "department")
            {
                var PrototypeTypes = await _commonRepository.GetPrototypeTypesAsync(Department ?? -1);
                response.Fields["PrototypeType"] = new Field
                {
                    Options = PrototypeTypes.Select(d => new Option(d.ID.ToString(), d.Description)),
                    Value = Form.PrototypeType?.ToString()
                };
                response.Valid &= PrototypeTypes.Any();

            }

            return response;
        }

        public async Task<PagingResponse<List<ListRow>>> GetDataAsync(long Company, PagingRequest<ListRequest> Form)
            =>
                await _prototypesRepository.GetPrototypesAsync(
                    Company,
                    p => new ListRow
                    {
                        ID = p.Prototype,
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
                        p.Prototypes.Company == Company
                        && p.Prototypes.Department == Form.Data.Department
                        && p.Prototypes.PrototypeType == Form.Data.PrototypeType
                        && (Form.Data.State == "All" || p.VersionState == Form.Data.State)
                        && (Form.Data.Name == string.Empty || p.Description != null && p.Description.ToUpper().Contains(Form.Data.Name.ToUpper())),
                    p => p.Prototype,
                    Form.CurrentPage,
                    Form.ItemsPerPage
                );
    }
}
