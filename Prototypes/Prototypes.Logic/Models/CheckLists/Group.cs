using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models;
using CheckListManager.Common.Logic.Models.CheckLists;
using Newtonsoft.Json;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
{
    public class Group : Item, ICheckListGroup
    {
        public override string TypeName => "Group";

        public virtual Dictionary<string, ICheckListElement> Elements { get; set; }

        public List<ICheckListItem> Items { get; set; } = new List<ICheckListItem>();
        public List<ICheckListIteration> Iterations { get; } = new List<ICheckListIteration>();
        public bool Iterator { get; set; }
        public bool Inverted { get; set; }
        public bool ShowAsTable { get; set; }
        public int InstanceIterations { get; set; }
        public int PrototypeIterations { get; set; }
        public bool DisableAdd { set { AddIterationCondition = value ? Conditions.False : Conditions.True; } } //for compatibility with version < 3.0
        public bool DisableRemove { set { RemoveIterationCondition = value ? Conditions.False : Conditions.True; } } //for compatibility with version < 3.0
        public bool DisableMove { set { MoveIterationCondition = value ? Conditions.False : Conditions.True; } } //for compatibility with version < 3.0
        public Conditions AddIterationCondition { get; set; } = Conditions.True;
        public string AddIterationExpressionCode { get; set; }
        public Conditions RemoveIterationCondition { get; set; } = Conditions.True;
        public string RemoveIterationExpressionCode { get; set; }
        public Conditions MoveIterationCondition { get; set; } = Conditions.True;
        public string MoveIterationExpressionCode { get; set; }
        public Conditions DirectImportCondition { get; set; } = Conditions.False;
        public string DirectImportExpressionCode { get; set; }
        [JsonIgnore]
        public string ModifiedBy { get; }
        [JsonIgnore]
        public DateTimeOffset? ModifiedOn { get; }
        public string DumpExternalSourceName { get; set; }
        private bool InTable { get; set; }

        public override bool Setup()
        {
            var Result = base.Setup();

            foreach (Item Item in Items.Cast<Item>())
            {
                Item.CheckList = CheckList;
                Item.Group = this;
                if ((ShowAsTable || InTable) && Item is Group group) group.InTable = true;
                if (InTable) //disable hidden properties for subgroups of tables
                {
                    ShowAsTable = false;
                    Iterator = false;
                }
                Item.PageIndex = PageIndex;
                Item.IterationItem = Iterator || IterationItem; //the item is contained in an iterator at some level
                Result &= Item.Setup();
            }

            return Result;
        }

        public void Traverse(Func<ICheckListItem, bool> condition, Action<ICheckListItem> action)
        {
            if (condition(this))
                action(this);

            foreach (Iteration iteration in Iterations.Cast<Iteration>())
                iteration.Traverse(condition, action);

            foreach (var item in Items)
                if (item is Group group)
                    group.Traverse(condition, action);
                else if (condition(item))
                    action(item);
        }

        public async Task TraverseAsync(Func<ICheckListItem, bool> condition, Func<ICheckListItem, Task> action)
        {
            if (condition(this))
                await action(this);

            foreach (Iteration iteration in Iterations.Cast<Iteration>())
                await iteration.TraverseAsync(condition, action);

            foreach (var item in Items)
                if (item is Group group)
                    await group.TraverseAsync(condition, action);
                else if (condition(item))
                    await action(item);
        }

        public void AddIterations(int Count)
        {
            for (int Index = 0; Index < Count; ++Index)
            {
                var iteration = new Iteration
                {
                    CheckList = CheckList,
                    Page = Page,
                    Group = this
                };
                iteration.Setup();
                Iterations.Add(iteration);
            }
        }

        public override ICheckListItem Clone(ICheckListGroup parent)
        {
            var group = new Group
            {
                CheckList = CheckList,
                Iterator = Iterator,
                Inverted = Inverted,
                ShowAsTable = ShowAsTable,
                InstanceIterations = InstanceIterations,
                PrototypeIterations = PrototypeIterations,
                AddIterationCondition = AddIterationCondition,
                AddIterationExpressionCode = AddIterationExpressionCode,
                RemoveIterationCondition = RemoveIterationCondition,
                RemoveIterationExpressionCode = RemoveIterationExpressionCode,
                MoveIterationCondition = MoveIterationCondition,
                MoveIterationExpressionCode = MoveIterationExpressionCode,
                DirectImportCondition = DirectImportCondition,
                DirectImportExpressionCode = DirectImportExpressionCode,
                DumpExternalSourceName = DumpExternalSourceName,
                Group = parent
            };
            foreach (var item in Items)
                group.Items.Add(item.Clone(group));

            Copy(this, group);
            return group;
        }

        public override bool BuildDependencies()
        {
            var Result = base.BuildDependencies();

            if (AddIterationCondition == Conditions.Expression)
                Result &= BindExpression(AddIterationExpressionCode, DependencyRelation.Properties.AddIteration, "add-iteration");

            if (RemoveIterationCondition == Conditions.Expression)
                Result &= BindExpression(RemoveIterationExpressionCode, DependencyRelation.Properties.RemoveIteration, "remove-iteration");

            if (MoveIterationCondition == Conditions.Expression)
                Result &= BindExpression(MoveIterationExpressionCode, DependencyRelation.Properties.MoveIteration, "move-iteration");

            if (DirectImportCondition == Conditions.Expression)
                Result &= BindExpression(DirectImportExpressionCode, DependencyRelation.Properties.DirectImport, "direct-import");

            if (DumpExternalSourceName != null)
            {
                var DumpResult = true;
                if (!CheckList.ExternalSources.TryGetValue(DumpExternalSourceName, out _))
                {
                    Errors.Add(new ConfigurationError("export-source", $"External source {DumpExternalSourceName} not found"));
                    DumpResult = false;
                }

                if (CheckList.Elements.TryGetValue(SignatureCode, out var SignatureElement) && SignatureElement is Signature Signature && Signature.Cancelable)
                {
                    Errors.Add(new ConfigurationError("export-source", $"The referenced signature is cancelable and cannot be used to export"));
                    DumpResult = false;
                }

                if (DumpResult)
                    AddDependingRelation(DependencyRelation.Properties.Value, CheckList.ExternalSources[DumpExternalSourceName], DependencyRelation.Properties.Value);

                Result &= DumpResult;
            }

            foreach (Item Item in Items.Cast<Item>())
            {
                if (HiddenCondition != Conditions.True || HiddenCondition == Conditions.Expression)
                    Item.AddDependingRelation(DependencyRelation.Properties.Hidden, this, DependencyRelation.Properties.Hidden);

                if (Item is ICheckListEditableItem && (ValidCondition == Conditions.Default || ValidCondition == Conditions.ExpressionOrDefault || ValidCondition == Conditions.ExpressionAndDefault))
                    AddDependingRelation(DependencyRelation.Properties.Validity, Item, DependencyRelation.Properties.Validity);

                Result &= Item.BuildDependencies();
            }

            return Result;
        }

        public override bool Check()
        {
            var Result = base.Check();

            if (AddIterationCondition == Conditions.Expression)
                Result &= CheckExpression("add-iteration", AddIterationExpressionCode, DataType.Boolean);

            if (RemoveIterationCondition == Conditions.Expression)
                Result &= CheckExpression("remove-iteration", RemoveIterationExpressionCode, DataType.Boolean);

            if (MoveIterationCondition == Conditions.Expression)
                Result &= CheckExpression("move-iteration", MoveIterationExpressionCode, DataType.Boolean);

            if (DirectImportCondition == Conditions.Expression)
                Result &= CheckExpression("direct-import", DirectImportExpressionCode, DataType.Boolean);

            foreach (Item Item in Items.Cast<Item>())
                Result &= Item.Check();
            return Result;
        }

        public void CreateInstanceIterations() { throw new NotImplementedException(); }

        public override string DependencyClassName => "group";

        public override string ToString() => $"group {Code ?? Id.ToString()}";
    }
}
