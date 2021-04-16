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
        public string ElementType { get; set; }
        public string ElementCode { get; set; }

        public override bool BuildDependencies()
        {
            var Result = base.BuildDependencies();

            //these dependencies are just for the prototype editing convenience as the code cannot change
            switch (ElementType)
            {
                case null:
                    break;
                case "Image":
                    Result &= BindElement<Image>(ElementCode, DependencyRelation.Properties.Value, DependencyRelation.Properties.Code, "link");
                    break;
                case "Attachment":
                    Result &= BindElement<Attachment>(ElementCode, DependencyRelation.Properties.Value, DependencyRelation.Properties.Code, "link");
                    break;
                default:
                    Errors.Add(new ConfigurationError("link", $"Invalid element type {ElementType}"));
                    Result = false;
                    break;
            }

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
                ElementType = ElementType,
                ElementCode = ElementCode
            };
            Copy(this, text);
            return text;
        }
    }
}
