using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
{
	public class Attachment : Item, ICheckListAttachment
    {
		public override string TypeName => "Attachment";

		public override bool NewLine => true;

        public override ICheckListItem Clone(ICheckListGroup parent)
        {
            var text = new Attachment { Group = parent };
            Copy(this, text);
            return text;
        }
    }
}
