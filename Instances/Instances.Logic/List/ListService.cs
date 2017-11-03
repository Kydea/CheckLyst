using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Instances.Data;

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

        public async Task<ListResponse> BindAsync(long company, ListRequest form)
        {
            var response = new ListResponse();
            var department = form.Department;
            var prototypeType = form.PrototypeType;
            var prototype = form.Prototype;
            var version = form.Version;
            var tag = form.TagCode;

            if (form.Source == null)
            {
                //initial load
                var departments = await _commonRepository.GetDepartmentsAsync(company);
                department = form.Department.HasValue && departments.Any(d => d.ID == form.Department) ? form.Department.Value : departments.FirstOrDefault()?.ID;
                response.Fields["Department"] = new Field
                {
                    Options = departments.Select(d => new Option(d.ID.ToString(), d.Name)),
                    Value = department?.ToString()
                };
                response.Valid &= departments.Any();

                response.Fields["Description"] = new Field { Value = form.Description };
                response.Fields["TagValue"] = new Field { Value = form.TagValue };

                var states = new List<Option> { new Option(null, "All") };
                states.AddRange((await _commonRepository.GetInstanceStatesAsync()).Select(x => new Option(x)));
                response.Fields["State"] = new Field
                {
                    Options = states,
                    Value = form.State
                };
            }

            if (form.Source == null || form.Source == "department")
            {
                var prototypeTypes = await _commonRepository.GetPrototypeTypesAsync(company, department ?? -1);
                prototypeType = form.PrototypeType.HasValue && prototypeTypes.Any(d => d.ID == form.PrototypeType) ? form.PrototypeType.Value : prototypeTypes.FirstOrDefault()?.ID;
                response.Fields["PrototypeType"] = new Field
                {
                    Options = prototypeTypes.Select(pt => new Option(pt.ID.ToString(), pt.Description)),
                    Value = prototypeType?.ToString()
                };
                response.Valid &= prototypeTypes.Any();
            }

            if (form.Source == null || form.Source == "department" || form.Source == "prototypeType")
            {
                var prototypes = await _commonRepository.GetPrototypesAsync(company, prototypeType ?? -1);
                prototype = form.Prototype.HasValue && prototypes.Any(d => d.ID == form.Prototype) ? form.Prototype.Value : prototypes.FirstOrDefault()?.ID;
                response.Fields["Prototype"] = new Field
                {
                    Options = prototypes.Select(p => new Option(p.ID.ToString(), p.Name)),
                    Value = prototype?.ToString()
                };
                response.Valid &= prototypes.Any();
            }

            if (form.Source == null || form.Source == "department" || form.Source == "prototypeType" || form.Source == "prototype")
            {
                var versions = await _commonRepository.GetPrototypeVersionsAsync(company, prototype ?? -1);
                version = form.Version.HasValue && versions.Any(d => d.Version == form.Version) ? form.Version.Value : versions.FirstOrDefault()?.Version;
                response.Fields["Version"] = new Field
                {
                    Options = versions.Any() ? new[] { new Option("0", "All") }.Union(versions.Select(p => new Option(p.Version.ToString(), p.Description))) : new Option[0],
                    Value = version?.ToString()
                };
                response.Valid &= versions.Any();
            }

            if (form.Source == null || form.Source == "department" || form.Source == "prototypeType" || form.Source == "prototype" || form.Source == "version")
            {
                var tags = await _commonRepository.GetPrototypeVersionTagsAsync(company, prototype ?? -1, version);
                tag = form.TagCode != string.Empty && tags.Any(d => d == form.TagCode) ? form.TagCode : tags.FirstOrDefault();
                response.Fields["TagCode"] = new Field
                {
                    Options = tags.Any() ? new[] { new Option("", "All") }.Union(tags.Select(t => new Option(t, t))) : new Option[0],
                    Value = tag,
                };

                response.Fields["TagValue"] = new Field
                {
                    Editable = tags.Any()
                };
            }

            return response;
        }

        public async Task<PagingResponse<List<ListRow>>> GetDataAsync(long company, PagingRequest<ListRequest> form)
            =>
                await _instancesRepository.GetInstancessAsync(
                    company,
                    i => new ListRow
                    {
                        CreatedBy = i.CreatedBy,
                        CreatedOn = i.CreatedOn,
                        ModifiedBy = i.ModifiedBy,
                        ModifiedOn = i.ModifiedOn
                    },
                    i =>
                        (form.Data.Description == string.Empty || i.Description != null && i.Description.ToUpper().Contains(form.Data.Description.ToUpper()))
                        && (form.Data.Department == null || i.Prototypes.Department == form.Data.Department)
                        && (form.Data.PrototypeType == null || i.Prototypes.PrototypeType == form.Data.PrototypeType)
                        && (form.Data.Prototype == null || i.Prototype == form.Data.Prototype)
                        && (form.Data.Version == 0 || i.Version == form.Data.Version)
                        && (form.Data.Year == null || i.Year == form.Data.Year)
                        && (form.Data.Number == null || i.Year == form.Data.Number)
                        && (form.Data.TagCode == string.Empty || i.InstanceTags.Any(it => it.Tag == form.Data.TagCode && it.Value.ToUpper().Contains(form.Data.TagValue) ))
                        && (form.Data.State == "All" || i.InstanceState == form.Data.State),
                    i => i.ID,
                    form.CurrentPage,
                    form.ItemsPerPage
                );
    }
}
