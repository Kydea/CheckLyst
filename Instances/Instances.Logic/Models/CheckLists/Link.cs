using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
    public class Link : Item, ICheckListLink
    {
        protected static void Copy(Link source, Link destination)
        {
            Item.Copy(source, destination);

            destination.UrlFormula.IsSet = ((Link)source.FindIteratorModelItem()).UrlFormula.IsSet;
        }

        public override string TypeName => "Link";

        public override bool NewLine => true;

        public string Url { get; set; }
        public Formula UrlFormula { get; } = new Formula();


        protected override void UpdateFormula()
        {
            base.UpdateFormula();
            if (IterationItem)
                UrlFormula.Evaluate(CheckList, GetRuntimeValues(), Instance.DecimalSeparator, Instance.TimeZone, Iteration?.Indexes, ((Link)FindIteratorModelItem()).UrlFormula);
            else
                UrlFormula.Evaluate(CheckList, GetRuntimeValues(), Instance.DecimalSeparator, Instance.TimeZone, Iteration?.Indexes);
        }

        public override void BuildDependencies()
        {
            UrlFormula.BuildDependencies(Url, Iteration == null ? null : ((Link)FindIteratorModelItem()).UrlFormula);

            if (!IterationItem)
            {
                foreach (var Key in UrlFormula.ValueKeys)
                    if (CheckList.Expressions.TryGetValue(Key, out var Expression))
                        AddDependingRelation(DependencyRelation.Properties.Formula, Expression, DependencyRelation.Properties.Value);

                base.BuildDependencies();
            }
        }

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

        public override dynamic GetViewModel(bool ReportModel)
        {
            var ItemModel = base.GetViewModel(ReportModel);
            ItemModel.Url = UrlFormula.Value;
            return ItemModel;
        }
    }
}
