using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
    public class Image : Item, ICheckListImage
    {
        public override string TypeName => "Image";

        public override ICheckListItem Clone(ICheckListGroup parent)
        {
            var image = new Image { Group = parent, Key = Key };
            Copy(this, image);
            return image;
        }

        public string Key { get; set; }
    }
}
