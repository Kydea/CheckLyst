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
    public interface IInfrastructureHelper
    {
        Task<WorkFlows> CreateWorkFlowAsync();
        
        Task<PrototypeTypesRequest> CreatePrototypeTypeAsync(DepartmentsRow Department);
        
        Task<Prototypes> CreatePrototypeAsync(DepartmentsRow Department, PrototypeTypesRequest PrototypeType);
        
        Task<DepartmentsRow> CreateDepartmentAsync();
        
        Task<DepartmentsRow> CreateDepartmentWithPrototypeTypeAsync();
        
        Task<DepartmentsRow> CreateDepartmentWithPrototypeAsync();
    }
}
