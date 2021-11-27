using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
{
    public class Link : Item, ICheckListLink
    {
        public override string TypeName => "Link";
        private Formula UrlFormula { get; } = new Formula();

        public override bool NewLine => true;

        public string Url { get; set; }

        public override bool BuildDependencies()
        {
            var Result = base.BuildDependencies();

            UrlFormula.BuildDependencies(Url, null);
            foreach (var Key in UrlFormula.ValueKeys)
                if (CheckList.Expressions.TryGetValue(Key, out var Expression))
                    AddDependingRelation(DependencyRelation.Properties.Formula, Expression, DependencyRelation.Properties.Value);

            return Result;
        }

        public override ICheckListItem Clone(ICheckListGroup parent)
        {
            var text = new Link
            {
                Group = parent,
                Url = Url,
            };
            Copy(this, text);
            return text;
        }
    }
}
