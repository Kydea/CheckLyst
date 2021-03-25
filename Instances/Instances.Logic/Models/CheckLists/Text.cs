using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
	public class Text : Item, ICheckListText
	{
		public override string TypeName => "Text";

		public override bool NewLine => true;

        public int Height { get; set; }
        public int Width { get; set; }

        public override ICheckListItem Clone(ICheckListGroup parent)
		{
            var text = new Text {
                Group = parent,
                Width = Width,
                Height = Height
            };
			Copy(this, text);
			return text;
		}
	}
}
