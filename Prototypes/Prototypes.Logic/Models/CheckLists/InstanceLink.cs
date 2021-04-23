using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.CheckLists;
using Newtonsoft.Json;
using System;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
{
    public class InstanceLink : Item, ICheckListInstanceLink
    {
        public override string TypeName => "InstanceLink";

        public override ICheckListItem Clone(ICheckListGroup parent)
        {
            var CheckList = new InstanceLink { Group = parent, PrototypeName = PrototypeName, Actions = Actions };
            Copy(this, CheckList);
            return CheckList;
        }

        public override bool BuildDependencies()
        {
            var Result = base.BuildDependencies();
            if (PrototypeName == null)
            {
                Errors.Add(new ConfigurationError("prototype", $"Specify a prototype"));
                Result = false;
            }
            return Result;
        }

        [JsonIgnore]
        public string ModifiedBy { get; }
        [JsonIgnore]
        public DateTimeOffset? ModifiedOn { get; }

        public string PrototypeName { get; set; }
        public InstanceLinkActions Actions { get; set; }
    }
}
