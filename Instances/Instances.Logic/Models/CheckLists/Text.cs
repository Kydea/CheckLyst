using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
    public class Text : Item, ICheckListText
    {
        public override string TypeName => "Text";

        public override bool NewLine { get; set; } = true;

        public int Height { get; set; }
        public int Width { get; set; }

        public override dynamic GetViewModel(bool ReportModel)
        {
            var ItemModel = base.GetViewModel(ReportModel);
            if (!ReportModel)
            {
                ItemModel.Height = Height;
                ItemModel.Width = Width;
            }
            return ItemModel;
        }

        public override ICheckListItem Clone(ICheckListGroup parent)
        {
            var text = new Text
            {
                Group = parent,
                Width = Width,
                Height = Height
            };
            Copy(this, text);
            return text;
        }
    }
}
