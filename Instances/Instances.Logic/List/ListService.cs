using CheckListManager.Common.Core.Services;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Instances.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Instances.Logic.List
{
    public class ListService : IListService
    {
        private readonly ICommonRepository _commonRepository;
        private readonly IInstancesRepository _instancesRepository;

        public ListService(ICommonRepository commonRepository, IInstancesRepository instancesRepository)
        {
            _commonRepository = commonRepository;
            _instancesRepository = instancesRepository;
        }

        public async Task<ListResponse> BindAsync(long Company, ListRequest Form)
        {
            ListResponse response = new ListResponse();
            long? Department = Form.Department;
            long? PrototypeType = Form.PrototypeType;
            long? Prototype = Form.Prototype;
            int? Version = Form.Version;

            if (Form.Source == null)
            {
                //initial load
                var Departments = await _commonRepository.GetDepartmentsAsync(Company);
                Department = Form.Department.HasValue && Departments.Any(d => d.ID == Form.Department) ? Form.Department.Value : Departments.FirstOrDefault()?.ID;
                response.Fields["Department"] = new Field
                {
                    Options = Departments.Select(d => new Option(d.ID.ToString(), d.Name)),
                    Value = Department?.ToString()
                };
                response.Valid &= Departments.Any();

                response.Fields["Description"] = new Field { Value = Form.Description };
                response.Fields["TagValue"] = new Field { Value = Form.TagValue };

                var States = new List<Option>() { new Option(null, "All") };
                States.AddRange((await _commonRepository.GetInstanceStatesAsync()).Select(x => new Option(x)));
                response.Fields["State"] = new Field
                {
                    Options = States,
                    Value = Form.State
                };
            }

            if (Form.Source == null || Form.Source == "department")
            {
                var PrototypeTypes = await _commonRepository.GetPrototypeTypesAsync(Company, Department ?? -1);
                response.Fields["PrototypeType"] = new Field
                {
                    Options = PrototypeTypes.Select(pt => new Option(pt.ID.ToString(), pt.Description)),
                    Value = Form.PrototypeType?.ToString()
                };
                response.Valid &= PrototypeTypes.Any();
            }

            if (Form.Source == null || Form.Source == "department" || Form.Source == "prototypeType")
            {
                var Prototypes = await _commonRepository.GetPrototypesAsync(Company, PrototypeType ?? -1);
                response.Fields["Prototype"] = new Field
                {
                    Options = Prototypes.Select(p => new Option(p.ID.ToString(), p.Name)),
                    Value = Form.Prototype?.ToString()
                };
                response.Valid &= Prototypes.Any();
            }

            if (Form.Source == null || Form.Source == "department" || Form.Source == "prototypeType" || Form.Source == "prototype")
            {
                var Versions = await _commonRepository.GetPrototypeVersionsAsync(Company, Prototype ?? -1);
                response.Fields["Version"] = new Field
                {
                    Options = Versions.Any() ? new Option[] { new Option("0", "All") }.Union(Versions.Select(p => new Option(p.Version.ToString(), p.Description))) : new Option[0],
                    Value = Form.Version?.ToString()
                };
                response.Valid &= Versions.Any();
            }

            if (Form.Source == null || Form.Source == "department" || Form.Source == "prototypeType" || Form.Source == "prototype" || Form.Source == "version")
            {
                var Tags = await _commonRepository.GetPrototypeVersionTagsAsync(Company, Prototype ?? -1, Version);
                response.Fields["Tags"] = new Field
                {
                    Options = Tags.Any() ? new Option[] { new Option("", "All") }.Union(Tags.Select(t => new Option(t, t))) : new Option[0],
                    Value = Form.Version?.ToString()
                };
                response.Valid &= Tags.Any();
            }

            return response;
        }

        public async Task<PagingResponse<List<ListRow>>> GetDataAsync(long Company, PagingRequest<ListRequest> Form)
            =>
                await _instancesRepository.GetInstancessAsync(
                    Company,
                    i => new ListRow
                    {
                        CreatedBy = i.CreatedBy,
                        CreatedOn = i.CreatedOn,
                        ModifiedBy = i.ModifiedBy,
                        ModifiedOn = i.ModifiedOn
                    },
                    i =>
                        (Form.Data.Description == string.Empty || i.Description != null && i.Description.ToUpper().Contains(Form.Data.Description.ToUpper()))
                        && (Form.Data.Department == null || i.Prototypes.Department == Form.Data.Department)
                        && (Form.Data.PrototypeType == null || i.Prototypes.PrototypeType == Form.Data.PrototypeType)
                        && (Form.Data.Prototype == null || i.Prototype == Form.Data.Prototype)
                        && (Form.Data.Version == 0 || i.Version == Form.Data.Version)
                        && (Form.Data.Year == null || i.Year == Form.Data.Year)
                        && (Form.Data.Number == null || i.Year == Form.Data.Number)
                        && (Form.Data.TagCode == "" || i.InstanceTags.Any(it => it.Tag == Form.Data.TagCode && it.Value.ToUpper().Contains(Form.Data.TagValue) ))
                        && (Form.Data.State == null || i.InstanceState == Form.Data.State),
                    i => i.ID,
                    Form.CurrentPage,
                    Form.ItemsPerPage
                );
    }
}
