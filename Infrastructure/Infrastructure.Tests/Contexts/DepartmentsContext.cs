using CheckListManager.Infrastructure.Logic.Departments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Tests.Contexts
{
    public class DepartmentsContext
    {
        public DepartmentsResponse Response { get; set; }

        public DepartmentsRow Record { get; set; }
    }
}
