using System.Collections.Generic;
using CheckListManager.Common.Logic.Dependency;
using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;

namespace CheckListManager.Instances.Logic.Models.CheckLists
{
    public abstract class Item : Element, ICheckListItem
    {
        public virtual string CssClass => TypeName;

        [JsonIgnore]
        public Formula DescriptionFormula { get; private set; }
        public string Description {
            get => DescriptionFormula?.RawValue;
            set => DescriptionFormula = string.IsNullOrEmpty(value) ? null : new Formula(value);
        }
        public string DescriptionValue { get => DescriptionFormula?.Value; }
        public ICheckListGroup Group { get; set; }
        public int Id { get; set; }
        public ICheckListIteration Iteration { get; set; }
        public bool IterationItem { get; set; }
        public bool IteratorModelItem { get; set; }
        public virtual bool NewLine { get; set; }
        public ICheckListPage Page { get; set; }
        public ICheckListItem Previous { get; set; }
        public Dictionary<string, object> RuntimeValues { get; } = new Dictionary<string, object>();
        public abstract string TypeName { get; }
        public int FontSize { get; set; }
        public string Color { get; set; }
        public bool Removed { get; set; }

        public override void Setup()
        {
            if (!IterationItem)
                base.Setup();
            CheckList.AddItem(this);
        }

        public override void Update(string DecimalSeparator, string TimeZone)
        {
            if (Iteration != null)
                RuntimeValues["IterationIndex"] = Iteration.Index;

            DescriptionFormula?.Update(CheckList, RuntimeValues, DecimalSeparator, TimeZone, Iteration?.Indexes);
        }

        public override bool Hidden => (Group?.Hidden ?? false) || base.Hidden;

        protected override bool IsReadOnlyByDefault()
            => Previous != null && !Previous.Valid;

        public override void UpdateReadOnly(string userCode, bool readOnlyInstance)
        {
            if (Removed || readOnlyInstance || Group != null && Group.ReadOnly || Page != null && Page.ReadOnly)
                ReadOnly = true;
            else
                base.UpdateReadOnly(userCode, readOnlyInstance);
        }

        public override string GetSignatureInformation()
            => base.GetSignatureInformation() ?? Group?.GetSignatureInformation() ?? Page?.GetSignatureInformation();

        protected override Dictionary<string, ICheckListElement> GetBranchElements()
            => Iteration?.Elements ?? base.GetBranchElements();

        public virtual void Remove()
        {
            Removed = true;
        }

        public abstract ICheckListItem Clone(ICheckListGroup parent);

        protected static void Copy(Item source, Item destination)
        {
            Element.Copy(source, destination);
            destination.Page = source.Page;
            destination.Description = source.Description;
            destination.NewLine = source.NewLine;
            destination.DescriptionFormula = source.DescriptionFormula == null ? null : new Formula(source.DescriptionFormula.RawValue);
            destination.IterationItem = true;
            destination.IteratorModelItem = false;
            destination.Color = source.Color;
            destination.FontSize = source.FontSize;
        }

        public override DependencyNodeKey DependencyNodeKey => base.DependencyNodeKey ?? new DependencyNodeKey("item", Id);
    }
}
