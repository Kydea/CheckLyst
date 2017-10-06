using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.DataEngine;
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

        public ListService(ICommonRepository commonRepository)
        {
            _commonRepository = commonRepository;
        }

        public async Task<ListResponse> GetResponse(long Company, ListRequest Form)
        {
            ListResponse response = new ListResponse();
            long? Department = Form.Department;
            if (Form.Source == null)
            {
                var Departments = await _commonRepository.GetDepartmentsAsync(Company);
                //initial load
                response.Fields["Department"] = new Field
                {
                    Options = Departments.Select(d => new Option(d.ID.ToString(), d.Name))
                };
                Department = Departments.FirstOrDefault()?.ID;

                var States = new List<Option>() { new Option(null, "All") };
                States.AddRange((await _commonRepository.GetVersionStatesAsync()).Select(x => new Option(x)));
                response.Fields["State"] = new Field
                {
                    Options = States
                };

            }

            if (Form.Source == null || Form.Source == "Department")
            {
                response.Fields["PrototypeType"] = new Field
                {
                    Options = (await _commonRepository.GetPrototypeTypesAsync(Department ?? -1)).Select(d => new Option(d.ID.ToString(), d.Description)) //TODO
                };
            }
            return response;
        }
    }
}
