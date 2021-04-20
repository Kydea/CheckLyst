using CheckListManager.Common.Logic.Models.CheckLists;
using Newtonsoft.Json;
using System;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
{
	public class Attachment : Item, ICheckListAttachment
    {
		public override string TypeName => "Attachment";

		public override bool NewLine => true;

        public override ICheckListItem Clone(ICheckListGroup parent)
        {
            var text = new Attachment { Group = parent, Key = Key, FileType = FileType, Target = Target };
            Copy(this, text);
            return text;
        }

        public string Key { get; set; }
        public string FileType { get; set; } = "images";
        public string Target { get; set; }

        [JsonIgnore]
        public string ModifiedBy { get; }
        [JsonIgnore]
        public DateTimeOffset? ModifiedOn { get; }

    }
}
