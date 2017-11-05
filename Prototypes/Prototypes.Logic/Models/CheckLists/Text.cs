using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
{
	public class Text : Item, ICheckListText
	{
		public override string TypeName => "Text";

		public override bool NewLine => true;
	}
}
