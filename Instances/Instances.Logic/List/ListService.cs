using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Instances.Data;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data.ListItems;

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


        private class DepartmentComparer : IEqualityComparer<Departments>
        {
            public bool Equals(Departments x, Departments y) => x.ID == y.ID;
            public int GetHashCode(Departments x) => x.ID.GetHashCode();
        }

        private class PrototypeTypeComparer : IEqualityComparer<PrototypeTypes>
        {
            public bool Equals(PrototypeTypes x, PrototypeTypes y) => x.ID == y.ID;
            public int GetHashCode(PrototypeTypes x) => x.ID.GetHashCode();
        }

        private class PrototypeComparer : IEqualityComparer<Prototypes>
        {
            public bool Equals(Prototypes x, Prototypes y) => x.ID == y.ID;
            public int GetHashCode(Prototypes x) => x.ID.GetHashCode();
        }

        private class PrototypeVersionComparer : IEqualityComparer<PrototypeVersion>
        {
            public bool Equals(PrototypeVersion x, PrototypeVersion y) => x.Version == y.Version;
            public int GetHashCode(PrototypeVersion x) => x.Version;
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

                var departments = await _commonRepository.GetDepartmentsAsync(identity, "Instances", "View");
                departments = departments.Union(await _commonRepository.GetDepartmentsAsync(identity, "Prototypes", "Preview"), new DepartmentComparer()).ToList();
                response.Fields["Department"] = new Field
                {
                    Options = departments.Select(d => new Option(d.ID.ToString(), d.Name)),
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

                response.Fields["ExceptionDescription"] = new Field { Value = form.ExceptionDescription };
                response.Fields["ExceptionState"] = new Field
                {
                    Options = new Option[] {
                        new Option("", "(all)"),
                        new Option("include", "only with exceptions"),
                        new Option("exclude", "only without exceptions")
                    },
                    Value = form.ExceptionState
                };
            }

            if (form.Source == null || form.Source == "department")
            {
                var prototypeTypes = await _commonRepository.GetPrototypeTypesAsync(identity, department ?? -1, "Instances", "View");
                prototypeTypes = prototypeTypes.Union(await _commonRepository.GetPrototypeTypesAsync(identity, department ?? -1, "Prototypes", "Preview"), new PrototypeTypeComparer()).ToList();
                prototypeType = null;
                response.Fields["PrototypeType"] = new Field
                {
                    Options = prototypeTypes.Select(pt => new Option(pt.ID.ToString(), $"{pt.Name} ({pt.DepartmentName})")).Distinct(),
                    Value = prototypeType?.ToString()
                };
            }

            if (form.Source == null || form.Source == "department" || form.Source == "prototypeType")
            {
                var prototypes = await _commonRepository.GetPrototypesAsync(identity, department ?? -1, prototypeType ?? -1, "Instances", "View");
                var testPrototypes = await _commonRepository.GetPrototypesAsync(identity, department ?? -1, prototypeType ?? -1, "Prototypes", "Preview");
                prototypes = prototypes.Union(testPrototypes, new PrototypeComparer()).ToList();

                response.Fields["Prototype"] = new Field
                {
                    Options = prototypes.Select(p => new Option(p.ID.ToString(), p.Name)).Distinct(),
                    Value = prototype.HasValue && prototypes.Any(p => p.ID == prototype.Value) ? prototype.ToString() : null
                };
            }

            if (form.Source == null || form.Source == "department" || form.Source == "prototypeType" || form.Source == "prototype")
            {
                var versions = await _commonRepository.GetPrototypeVersionsAsync(identity, prototype ?? -1, "Instances", "View");
                var testVersions = (await _commonRepository.GetPrototypeVersionsAsync(identity, prototype ?? -1, "Prototypes", "Preview")).Where(pv => pv.VersionState == VersionState.Draft || pv.VersionState == VersionState.InSimulation);
                versions = versions.Union(testVersions, new PrototypeVersionComparer()).ToList();
                response.Fields["Version"] = new Field
                {
                    Options = versions.Select(p => new Option(p.Version.ToString(), p.Description)).Distinct(),
                    Value = version.HasValue && versions.Any(v => v.Version == version.Value) ? version.Value.ToString() : null
                };
            }

            if (form.Source == null || form.Source == "department" || form.Source == "prototypeType" || form.Source == "prototype" || form.Source == "version")
            {
                var tags = await _commonRepository.GetPrototypeVersionTagsAsync(identity, prototype ?? -1, version, "Instances", "View");
                tags = tags.Union(await _commonRepository.GetPrototypeVersionTagsAsync(identity, prototype ?? -1, version, "Prototypes", "Preview")).ToList();
                response.Fields["TagCode"] = new Field
                {
                    Options = tags.Select(t => new Option(t, t)).Distinct(),
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
            form.Data.Description = form.Data.Description ?? string.Empty;
            form.Data.State = form.Data.State ?? string.Empty;
            form.Data.TagCode = form.Data.TagCode ?? string.Empty;
            form.Data.TagValue = form.Data.TagValue ?? string.Empty;
            form.Data.ExceptionState = form.Data.ExceptionState ?? string.Empty;
            form.Data.ExceptionDescription = form.Data.ExceptionDescription ?? string.Empty;

            return await _instancesRepository.GetInstancesAsync(
                company,
                i => new ListRow
                {
                    Id = i.ID,
                    Code = i.Code,
                    Year = i.Year > 0 ? i.Year.ToString() : i.PrototypeVersions.VersionState == VersionState.Draft ? "Test" : "SE",
                    Number = i.Number,
                    State = i.InstanceState,
                    Description = i.Description,
                    DepartmentName = i.Prototypes.Departments.Name,
                    PrototypeTypeName = i.Prototypes.PrototypeTypes.Name,
                    PrototypeName = i.Prototypes.Name,
                    Version = i.Version,
                    Exceptions = i.InstanceExpressionValues.Where(ev => ev.Exception).Select(ev => new InstanceException { Code = ev.Code, Description = ev.Description }).ToList(),

                    CreatedBy = i.CreatedBy,
                    CreatedOn = i.CreatedOn,
                    ModifiedBy = i.ModifiedBy,
                    ModifiedOn = i.ModifiedOn
                },
                i =>
                    (form.Data.ID == null || i.ID == form.Data.ID)
                    && (form.Data.Description == string.Empty || i.Description != null && i.Description.ToUpper().Contains(form.Data.Description.ToUpper()))
                    && (form.Data.Department == null || i.Prototypes.Department == form.Data.Department)
                    && (form.Data.PrototypeType == null || i.Prototypes.PrototypeType == form.Data.PrototypeType)
                    && (form.Data.Prototype == null || i.Prototype == form.Data.Prototype)
                    && (form.Data.Version == null || i.Version == form.Data.Version)
                    && (form.Data.Year == null || i.Year == form.Data.Year)
                    && (form.Data.Number == null || i.Year == form.Data.Number)
                    && (form.Data.TagValue == string.Empty || i.InstanceTags.Any(it => (form.Data.TagCode == string.Empty || it.Tag == form.Data.TagCode) && it.Value.ToUpper().Contains(form.Data.TagValue.ToUpper())))
                    && (form.Data.State == string.Empty || i.InstanceState == form.Data.State)
                    && (!form.Data.From.HasValue || i.ModifiedOn >= form.Data.From.Value)
                    && (!form.Data.To.HasValue || i.ModifiedOn <= form.Data.WholeTo.Value)
                    && (
                        form.Data.ExceptionState == string.Empty
                        || form.Data.ExceptionState == "include" && i.InstanceExpressionValues.Any(ev => ev.Exception)
                        || form.Data.ExceptionState == "exclude" && !i.InstanceExpressionValues.Any(ev => ev.Exception)
                    )
                    && (form.Data.ExceptionDescription == string.Empty || i.InstanceExpressionValues.Any(ev => ev.Exception && ev.Description.ToUpper().Contains(form.Data.ExceptionDescription.ToUpper()))),
                i => i.ID,
                form.CurrentPage,
                form.ItemsPerPage
            );
        }
    }
}
