using CheckListManager.Common.Logic.Models.CheckLists;
using System;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
    public class Attachment : Item, ICheckListAttachment
    {
        public override string TypeName => "Attachment";
        public override bool NewLine => true;
        public string ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedOn { get; set; }

        public override ICheckListItem Clone(ICheckListGroup parent)
        {
            var attachment = new Attachment
            {
                Group = parent,
                FileType = FileType
            };
            Copy(this, attachment);
            return attachment;
        }

        public override bool IsValidByDefault() => !string.IsNullOrEmpty(Key);
        
        public string Key { get; set; }
        public string FileType { get; set; }

        public class FileTypes
        {
            public const string Images = "images";
            public const string Pdf = "pdf";
        }

        public override dynamic GetViewModel()
        {
            var ItemModel = base.GetViewModel();
            ItemModel.Key = Key;
            ItemModel.FileType = FileType;
            return ItemModel;
        }
    }
}
