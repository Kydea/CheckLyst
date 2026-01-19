using CheckListManager.Common.Logic.Models.CheckLists;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
{
    public class Note : Item, ICheckListNote
    {
        public override string TypeName => "Note";

        public override ICheckListItem Clone(ICheckListGroup parent)
        {
            var note = new Note
            {
                Group = parent,
            };
            Copy(this, note);
            return note;
        }
    }
}
