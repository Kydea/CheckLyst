using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.CheckLists;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
{
    public class InstanceLink : Item, ICheckListInstanceLink
    {
        public override string TypeName => "InstanceLink";

        public override ICheckListItem Clone(ICheckListGroup parent)
        {
            var CheckList = new InstanceLink
            {
                Group = parent,
                PrototypeName = PrototypeName,
                PrototypeID = PrototypeID,
                Actions = Actions,
                Parameters = Parameters?.Select(p => new InstanceLinkParameter
                {
                    Name = p.Name,
                    ElementCode = p.ElementCode,
                    ExpressionCode = p.ExpressionCode
                }).ToList()
            };
            Copy(this, CheckList);
            return CheckList;
        }

        public override bool BuildDependencies()
        {
            var Result = base.BuildDependencies();
            if (string.IsNullOrEmpty(PrototypeName))
            {
                Errors.Add(new ConfigurationError("prototype", $"Specify a prototype"));
                Result = false;
            }

            if (Parameters != null)
                foreach (var Parameter in Parameters)
                {
                    Parameter.BuildDependencies(CheckList, this);
                    if (Parameter.Error != null)
                    {
                        Errors.Add(new ConfigurationError("parameter", Parameter.Error));
                        Result = false;
                    }
                }

            return Result;
        }

        [JsonIgnore]
        public string ModifiedBy { get; }
        [JsonIgnore]
        public DateTimeOffset? ModifiedOn { get; }

        public string PrototypeName { get; set; }
        public long? PrototypeID { get; set; }
        public InstanceLinkActions Actions { get; set; }
        public InstanceLinkRecord Record { get; set; }
        public List<InstanceLinkParameter> Parameters { get; set; }
    }
}
