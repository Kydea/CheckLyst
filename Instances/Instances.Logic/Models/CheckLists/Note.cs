using CheckListManager.Common.Logic.Models.CheckLists;
using System.Dynamic;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
    public class Note : Item, ICheckListNote
    {
        public override string TypeName => "Note";
        public override dynamic GetViewModel(bool ReportModel) => new System.NotImplementedException();
        public override ICheckListItem Clone(ICheckListGroup parent)
        {
            var note = new Note
            {
                Group = parent
            };
            Copy(this, note);
            return note;
        }
    }
}
