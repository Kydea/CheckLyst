using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
{
    public class Text : Item, ICheckListText
    {
        public override string TypeName => "Text";
        public override bool NewLine { get; set; } = true;
        public int Height { get; set; }
        public int Width { get; set; }

        public override ICheckListItem Clone(ICheckListGroup parent)
        {
            var text = new Text
            {
                Group = parent,
                Height = Height,
                Width = Width
            };
            Copy(this, text);
            return text;
        }
    }
}
