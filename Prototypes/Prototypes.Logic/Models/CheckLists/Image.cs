using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.CheckLists;
using Newtonsoft.Json;
using System;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
{
	public class Image : Item, ICheckListImage
	{
		public override string TypeName => "Image";

        public bool Cloned { get; set; }

        public ICheckListAttachmentBase GetTarget() => throw new NotImplementedException();

        public override bool BuildDependencies()
        {
            var Result = base.BuildDependencies();

            //this dependency is just for prototype editors as there is no possibile change in the instances
            if(Target != null)
                Result &= BindElement<Image>(Target, DependencyRelation.Properties.Value, DependencyRelation.Properties.Value, "target");

            return Result;
        }

        public override ICheckListItem Clone(ICheckListGroup parent)
        {
            var Image = new Image { Group = parent, Key = Key, Target = Target };
            Copy(this, Image);
            return Image;
        }

        public string ImageName { get; set; }
        public string Key { get; set; }
        public string Target { get; set; }
    }
}
