using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Instances.Data;
using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data.ListItems;
using CheckListManager.Instances.Data.Models;
using CheckListManager.Instances.Logic.Messages;
using CheckListManager.Common.Logic.Models.Expressions.Functions;

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

        public async Task<ListResponse> BindAsync(IDefaultIdentity identity, InstanceFilters form)
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

                response.Fields["ID"] = new Field { Value = form.ID.ToString() };
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

                response.Fields["LifeCycleFrom"] = new Field { Value = form.LifeCycleFrom.ToString() };
                response.Fields["LifeCycleTo"] = new Field { Value = form.LifeCycleTo.ToString() };
            }

            if (form.Source == null || form.Source == "department")
            {
                var prototypeTypes = await _commonRepository.GetPrototypeTypesAsync(identity, department ?? -1, "Instances", "View");
                prototypeTypes = prototypeTypes.Union(await _commonRepository.GetPrototypeTypesAsync(identity, department ?? -1, "Prototypes", "Preview"), new PrototypeTypeComparer()).ToList();
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

        public async Task<PagingResponse<List<InstanceRecord>>> GetDataAsync(IDefaultIdentity identity, PagingRequest<InstanceFilters> form) =>
            await _instancesRepository.GetInstancesAsync(identity, form.Data, form.CurrentPage, form.ItemsPerPage);

        public async Task<OverviewResponse> OverviewAsync(IDefaultIdentity identity, InstanceFilters filters) =>
            new OverviewResponse
            {
                Records = await _instancesRepository.OverviewAsync(identity, filters)
            };

        public async Task<LifeCycleResponse> LifeCycleAsync(IDefaultIdentity identity, InstanceFilters filters) =>
            new LifeCycleResponse
            {
                Records = await _instancesRepository.LifeCycleAsync(identity, filters)
            };

        public async Task<TasksResponse> GetTasksAsync(IDefaultIdentity identity, InstanceFilters filters) =>
            new TasksResponse
            {
                Records = await _instancesRepository.GetTasksAsync(identity, filters)
            };

        public async Task<long> CountTasksAsync(IDefaultIdentity identity) =>
            await _instancesRepository.CountTasksAsync(identity);
            
        public async Task<List<InstanceRecord>> ExportAsync(InstanceFilters filters) =>
            await _instancesRepository.ExportAsync(filters);
    }
}
