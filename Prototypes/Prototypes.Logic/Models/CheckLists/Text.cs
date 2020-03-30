using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
{
	public class Text : Item, ICheckListText
	{
		public override string TypeName => "Text";

		public override bool NewLine => true;

        public override ICheckListItem Clone(ICheckListGroup parent)
        {
            var text = new Text { Group = parent };
            Copy(this, text);
            return text;
        }

        protected override string DescriptionPropertyName => "content";
    }
}
