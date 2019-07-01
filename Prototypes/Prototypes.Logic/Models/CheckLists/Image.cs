using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
{
	public class Image : Item, ICheckListImage
	{
		public override string TypeName => "Image";

        public override ICheckListItem Clone(ICheckListGroup parent)
        {
            var text = new Image { Group = parent };
            Copy(this, text);
            return text;
        }

        public string Key { get; set; }
    }
}
