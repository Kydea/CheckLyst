using CheckListManager.Common.Logic.Dependency;
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
                FileType = FileType,
                Target = Target
            };
            Copy(this, attachment);
            return attachment;
        }

        public override bool IsValidByDefault() => !string.IsNullOrEmpty(Key);
        
        public string Key { get; set; }
        public string FileType { get; set; }
        public string Target { get; set; }

        public class FileTypes
        {
            public const string Images = "images";
            public const string Pdf = "pdf";
        }

        public override void BuildDependencies()
        {
            base.BuildDependencies();

            if (Target != null)
                BindElement<Attachment>(Target, DependencyRelation.Properties.Value, DependencyRelation.Properties.Value);
        }

        public override dynamic GetViewModel()
        {
            var ItemModel = base.GetViewModel();
            var Target = this.Target == null || !GetBranchElements().TryGetValue(this.Target, out var Item) || !(Item is Attachment Attachment) ? null : Attachment;
            ItemModel.Key = Target?.Key ?? Key;
            ItemModel.FileType = Target?.FileType ?? FileType;
            return ItemModel;
        }
    }
}
