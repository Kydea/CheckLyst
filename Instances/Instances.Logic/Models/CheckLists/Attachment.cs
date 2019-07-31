using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
    public class Attachment : Item, ICheckListAttachment
    {
        public override string TypeName => "Attachment";

        public override bool NewLine => true;

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

        public override bool IsValidByDefault()
        {
            return !string.IsNullOrEmpty(Key);
        }
        
        public string Key { get; set; }
        public string FileType { get; set; }

        public class FileTypes
        {
            public const string Images = "images";
            public const string Pdf = "pdf";
        }
    }
}
