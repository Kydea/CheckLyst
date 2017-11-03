namespace CheckListManager.Common.Logic.Models.CheckLists
{
    public class Text : Item, ICheckListText
	{
        public override string TypeName => "Text";

	    public override bool NewLine => true;

	    public override Item Clone(Group parent)
        {
	        var text = new Text { Group = parent };
	        Copy(this, text);
            return text;
        }
    }
}
