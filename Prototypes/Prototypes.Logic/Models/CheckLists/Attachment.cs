using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.CheckLists;
using Newtonsoft.Json;
using System;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
{
	public class Attachment : Item, ICheckListAttachment
    {
		public override string TypeName => "Attachment";

		public override bool NewLine => true;

        public override bool BuildDependencies()
        {
            var Result =  base.BuildDependencies();

            if (Target != null)
                Result &= BindElement<Attachment>(Target, DependencyRelation.Properties.Value, DependencyRelation.Properties.Value, "target");

            return Result;
        }

        public override ICheckListItem Clone(ICheckListGroup parent)
        {
            var text = new Attachment { Group = parent, Key = Key, FileType = FileType, Target = Target };
            Copy(this, text);
            return text;
        }

        public ICheckListAttachmentBase GetTarget() => throw new NotImplementedException();

        public string Key { get; set; }
        public string FileType { get; set; } = "images";
        public string Target { get; set; }
        public bool RestrictToCamera { get; set; }

        [JsonIgnore]
        public string ModifiedBy { get; }
        [JsonIgnore]
        public DateTimeOffset? ModifiedOn { get; }

    }
}
