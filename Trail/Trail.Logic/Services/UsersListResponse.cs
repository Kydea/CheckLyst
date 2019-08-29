using System;
using System.Collections.Generic;
using System.Linq;
using CheckListManager.Common.Data;
using CheckListManager.Common.Logic.Models.Expressions.LogicOperators;
using Newtonsoft.Json;

namespace CheckListManager.Trail.Logic.Services
{
    public class UsersListResponse
    {
        public long Company { get; set; }
        public long Id { get; set; }
        public DateTimeOffset Date { get; set; } = DateTimeOffset.UtcNow;
        public string Action { get; set; }
        public string User { get; set; }
        public string AffectedUser { get; set; }
        public string Information { get; set; }

        [JsonIgnore]
        public UserInformation Details { get; set; }

        public class UserInformation
        {
            public string Code { get; set; }
            public string Email { get; set; }
            public string NameAndSurname { get; set; }
            
            public bool? Enabled { get; set; }

            public string Reason { get; set; }

            public List<Permission> Functions { get; set; } = new List<Permission>();

        }
        
        public class Permission
        {
            public string FunctionGroup { get; set; }
            public string Function { get; set; }
            public string Department { get; set; }
            public string PrototypeType { get; set; }
            public string Level { get; set; }

            public string DepartmentDescription { get; set; }
            public string PrototypeTypeDescription { get; set; }

            public int Order => FunctionGroup == "Instances"
                ? 2
                : FunctionGroup == "Prototypes"
                    ? 1
                    : 0;

            public string Sorting => $"{Order}{FunctionGroup}";

            public override string ToString()
            {
                var parts = new []
                {
                    Function, DepartmentDescription ?? Department, PrototypeTypeDescription ?? PrototypeType, Level
                };

                return string.Join("/", parts.Where(y => !string.IsNullOrWhiteSpace(y)));
            }

            public void SetValues(List<Departments> departments, List<PrototypeTypes> prototypeTypes)
            {
                if (!string.IsNullOrEmpty(Department) && int.TryParse(Department.Trim(), out var department))
                {
                    var record = departments.FirstOrDefault(d => d.ID == department);
                    if (record != null)
                    {
                        DepartmentDescription = record.Name;
                    }
                }
                
                if (!string.IsNullOrEmpty(PrototypeType) && int.TryParse(PrototypeType.Trim(), out var prototypeType))
                {
                    var record = prototypeTypes.FirstOrDefault(p => p.ID == prototypeType);
                    if (record != null)
                    {
                        PrototypeTypeDescription = record.Name;
                    }
                }
            }
        }
    }
}