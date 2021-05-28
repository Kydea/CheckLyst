using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Data;
using CheckListManager.Infrastructure.Logic.Departments;
using CheckListManager.Infrastructure.Logic.PrototypeTypes;
using CheckListManager.Infrastructure.Tests.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Tests
{
    public class InfrastructureHelper : IInfrastructureHelper
    {
        private readonly IDefaultIdentity _defaultIdentity;
        private readonly IDepartmentsService _departmentsService;
        private readonly IPrototypeTypesService _prototypeTypesService;
        private readonly IWorkFlowsRepositoryExtended _workFlowsRepositoryExtended;
        private readonly IPrototypesRepositoryExtended _prototypesRepositoryExtended;

        public InfrastructureHelper(
            IDefaultIdentity defaultIdentity,
            IDepartmentsService departmentsService,
            IPrototypeTypesService prototypeTypesService,
            IWorkFlowsRepositoryExtended workFlowsRepositoryExtended,
            IPrototypesRepositoryExtended prototypesRepositoryExtended)
        {
            _defaultIdentity = defaultIdentity;
            _departmentsService = departmentsService;
            _prototypeTypesService = prototypeTypesService;
            _workFlowsRepositoryExtended = workFlowsRepositoryExtended;
            _prototypesRepositoryExtended = prototypesRepositoryExtended;
        }


        public async Task<DepartmentsRow> CreateDepartmentAsync()
        {
            var Record = new DepartmentsRow
            {
                Name = Guid.NewGuid().ToString(),
                Reason = "Testing",
            };

            var Response = await _departmentsService.AddOrEdit(_defaultIdentity, Record, true);
            Assert.IsTrue(Response.Valid, "Department cannot be created.");
            return Record;
        }

        public async Task<WorkFlows> CreateWorkFlowAsync()
        {
            try
            {
                var WorkFlow = await _workFlowsRepositoryExtended.AddWorkFlowAsync(_defaultIdentity, new WorkFlows
                {
                    Description = Guid.NewGuid().ToString()
                });

                return WorkFlow;
            }
            catch (Exception ex)
            {
                Assert.Fail("Cannot create work flow.", ex);
                return null;
            }
        }

        public async Task<PrototypeTypesRequest> CreatePrototypeTypeAsync(DepartmentsRow Department)
        {
            var WorkFlow = await CreateWorkFlowAsync();

            var PrototypeType = new PrototypeTypesRequest()
            {
                Name = Guid.NewGuid().ToString(),
                ObsolescenceWorkFlow = WorkFlow.ID,
                ApprovationWorkFlow = WorkFlow.ID,
                Department = Department.ID,
                Reason = "Testing",
            };

            var Response = await _prototypeTypesService.Add(_defaultIdentity, PrototypeType);
            Assert.IsTrue(Response.Valid, "Prototype type cannot be created.");
            return PrototypeType;
        }

        public async Task<Prototypes> CreatePrototypeAsync(DepartmentsRow Department, PrototypeTypesRequest PrototypeType)
        {
            var Prototype = new Prototypes
            {
                Description = "Testing",
                Name = Guid.NewGuid().ToString(),
                Department = Department.ID.Value,
                PrototypeType = PrototypeType.ID.Value
            };

            await _prototypesRepositoryExtended.AddProtoptypeAsync(_defaultIdentity, Prototype);
            return Prototype;
        }


        public async Task<DepartmentsRow> CreateDepartmentWithPrototypeTypeAsync()
        {
            var Department = await CreateDepartmentAsync();
            await CreatePrototypeTypeAsync(Department);
            return Department;
        }

        public async Task<DepartmentsRow> CreateDepartmentWithPrototypeAsync()
        {
            var Department = await CreateDepartmentAsync();
            var PrototypeType = await CreatePrototypeTypeAsync(Department);
            await CreatePrototypeAsync(Department, PrototypeType);
            return Department;
        }
    }
}
