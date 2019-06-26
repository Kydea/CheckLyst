using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
{
    public class Link : Item, ICheckListLink
    {
        public override string TypeName => "Link";

        public override bool NewLine => true;

        public string Url { get; set; }

        public override ICheckListItem Clone(ICheckListGroup parent)
        {
            var text = new Link
            {
                Group = parent,
                Url = Url
            };
            Copy(this, text);
            return text;
        }
    }
}
