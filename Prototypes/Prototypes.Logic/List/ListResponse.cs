using CheckListManager.Common.Logic.DataEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Prototypes.Logic.List
{
    public class ListResponse : FormResponse
    {
        public ListResponse(ListRequest request)
        {
            Name = request.Name;
            Department = request.Department;
            PrototypeType = request.PrototypeType;
            State = request.State;
        }

        public string Name { get; set; }

        public int Department { get; set; }

        public int PrototypeType { get; set; }

        public string State { get; set; }

        public Dictionary<int, string> DepartmentList { get; set; }

        public Dictionary<int, string> PrototypeTypeList { get; set; }

        public List<string> StateList { get; set; }
    }
}
