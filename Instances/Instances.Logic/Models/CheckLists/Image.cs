using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
    public class Image : Item, ICheckListImage
    {
        public override string TypeName => "Image";

        public override ICheckListItem Clone(ICheckListGroup parent)
        {
            var image = new Image { Group = parent, Key = Key, Target = Target };
            Copy(this, image);
            return image;
        }

        public ICheckListAttachmentBase GetTarget() =>
            Target == null || !GetBranchElements().TryGetValue(Target, out var Item) || !(Item is Image Image) ? this : Image;

        public override dynamic GetViewModel(bool ReportModel)
        {
            var ItemModel = base.GetViewModel(ReportModel);
            if (!ReportModel)
                ItemModel.Key = GetTarget().Key;
            return ItemModel;
        }

        public string Key { get; set; }
        public string Target { get; set; }
    }
}
