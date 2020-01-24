using CheckListManager.Common.Logic.Models.CheckLists;
using Newtonsoft.Json;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
{
	public class Image : Item, ICheckListImage
	{
		public override string TypeName => "Image";

        public bool Cloned { get; set; }

        public override ICheckListItem Clone(ICheckListGroup parent)
        {
            var Image = new Image { Group = parent, Key = Key };
            Copy(this, Image);
            return Image;
        }

        public string Key { get; set; }
    }
}
