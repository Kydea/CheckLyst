using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Instances.Data;
using CheckListManager.Common.Core.Web.Security;

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

        public async Task<ListResponse> BindAsync(DefaultIdentity identity, ListRequest form)
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

                var departments = await _commonRepository.GetDepartmentsAsync(identity, "Instances");
                department = department.HasValue && departments.Any(d => d.ID == department.Value) ? department.Value : departments.FirstOrDefault()?.ID;
                response.Fields["Department"] = new Field
                {
                    Options = departments.OrderBy(x => x.Name).Select(d => new Option(d.ID.ToString(), d.Name)),
                    Value = department?.ToString()
                };

                response.Fields["Description"] = new Field { Value = form.Description };
                response.Fields["TagValue"] = new Field { Value = form.TagValue };

                var states = await _commonRepository.GetInstanceStatesAsync();
                response.Fields["State"] = new Field
                {
                    Options = states.Where(x => x != InstanceState.Draft).Select(x => new Option(x)),
                    Value = form.State
                };
            }

            if (form.Source == null || form.Source == "department")
            {
                var prototypeTypes = await _commonRepository.GetPrototypeTypesAsync(identity, department ?? -1, "Instances");
                prototypeType = prototypeType.HasValue && prototypeTypes.Any(d => d.ID == prototypeType.Value) ? form.PrototypeType.Value : prototypeTypes.FirstOrDefault()?.ID;
                response.Fields["PrototypeType"] = new Field
                {
                    Options = prototypeTypes.OrderBy(x => x.Name).Select(pt => new Option(pt.ID.ToString(), pt.Name)),
                    Value = prototypeType?.ToString()
                };
            }

            if (form.Source == null || form.Source == "department" || form.Source == "prototypeType")
            {
                var prototypes = await _commonRepository.GetPrototypesAsync(identity, prototypeType ?? -1, "Instances");
                response.Fields["Prototype"] = new Field
                {
                    Options = prototypes.OrderBy(x => x.Name).Select(p => new Option(p.ID.ToString(), p.Name)),
                    Value = prototype.HasValue && prototypes.Any(p => p.ID == prototype.Value) ? prototype.ToString() : null
                };
            }

            if (form.Source == null || form.Source == "department" || form.Source == "prototypeType" || form.Source == "prototype")
            {
                var versions = await _commonRepository.GetPrototypeVersionsAsync(identity, prototype ?? -1, "Instances");
                response.Fields["Version"] = new Field
                {
                    Options = versions.OrderBy(x => x.Description).Select(p => new Option(p.Version.ToString(), p.Description)),
                    Value = version.HasValue && versions.Any(v => v.Version == version.Value) ? version.Value.ToString() : null
                };
            }

            if (form.Source == null || form.Source == "department" || form.Source == "prototypeType" || form.Source == "prototype" || form.Source == "version")
            {
                var tags = await _commonRepository.GetPrototypeVersionTagsAsync(identity, prototype ?? -1, version, "Instances");
                response.Fields["TagCode"] = new Field
                {
                    Options = tags.OrderBy(x => x).Select(t => new Option(t, t)),
                    Value = !string.IsNullOrEmpty(tag) && tags.Contains(tag) ? tag : null
                };

                response.Fields["TagValue"] = new Field
                {
                    Editable = tags.Any()
                };
            }

            return response;
        }

        public async Task<PagingResponse<List<ListRow>>> GetDataAsync(long company, PagingRequest<ListRequest> form)
        {
            form.Data.State = form.Data.State ?? string.Empty;
            form.Data.TagCode = form.Data.TagCode ?? string.Empty;
            form.Data.TagValue = form.Data.TagValue ?? string.Empty;
            return await _instancesRepository.GetInstancesAsync(
                company,
                i => new ListRow
                {
                    Id = i.ID,
                    Code = i.Code,
                    Year = i.Year == 0 ? "SE" : i.Year.ToString(),
                    Number = i.Number,
                    State = i.InstanceState,
                    Description = i.Description,
                    DepartmentName = i.Prototypes.Departments.Name,
                    PrototypeTypeName = i.Prototypes.PrototypeTypes.Name,
                    PrototypeName = i.Prototypes.Name,
                    Version = i.Version,
                    
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
                    && (form.Data.Version == null || i.Version == form.Data.Version)
                    && (form.Data.Year == null || i.Year == form.Data.Year)
                    && (form.Data.Number == null || i.Year == form.Data.Number)
                    && (form.Data.TagValue == string.Empty || i.InstanceTags.Any(it => (form.Data.TagCode == string.Empty || it.Tag == form.Data.TagCode) && it.Value.ToUpper().Contains(form.Data.TagValue.ToUpper())))
                    && (form.Data.State == string.Empty || i.InstanceState == form.Data.State),
                i => i.ID,
                form.CurrentPage,
                form.ItemsPerPage
            );
        }
    }
}
