using System.Collections.Generic;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
    public abstract class Item : Element, ICheckListItem
	{
        protected static void Copy(ICheckListItem source, ICheckListItem destination)
        {
            Element.Copy(source, destination);
            destination.Page = source.Page;
            destination.Description = source.Description;
            destination.NewLine = source.NewLine;
            destination.DescriptionFormula = source.DescriptionFormula == null ? null : new Formula(source.DescriptionFormula.RawValue);
            destination.IterationItem = true;
            destination.IteratorModelItem = false;
        }

        public bool IterationItem { get; set; }

        public bool IteratorModelItem { get; set; }

        [JsonIgnore]
        public Iteration Iteration { get; set; }

        public abstract string TypeName { get; }

        [JsonIgnore]
        public Group Group { get; set; }

        [JsonIgnore]
        public Item Previous { get; set; }

        [JsonIgnore]
        public Page Page { get; set; }

        [JsonIgnore]
        public Formula DescriptionFormula { get; set; }

        public string Description
        {
            get => DescriptionFormula?.RawValue;
	        set => DescriptionFormula = value == null ? null : new Formula(value);
        }

        public virtual bool NewLine { get; set; }

        public virtual string CssClass => TypeName;

	    public int Id { get; set; }

        protected override bool IsReadOnlyByDefault()
        {
            return Previous != null && !Previous.Valid;
        }

        public override bool IsReadOnly(uint clientId)
        {
            return
				base.IsReadOnly(clientId) || Group != null
				&& Group.IsReadOnly(clientId) || Page != null
				&& (Page.IsLocked(clientId) || Page.IsReadOnly(clientId));
        }

        protected override Dictionary<string, Element> GetBranchElements()
        {
            return Iteration?.Elements ?? base.GetBranchElements();
        }

        public override string GetSignatureInformation()
        {
            return base.GetSignatureInformation() ?? Group?.GetSignatureInformation() ?? Page?.GetSignatureInformation();
        }

        public abstract Item Clone(Group parent);

        public virtual void Remove()
        {
            ((Instance)CheckList).Items.Remove(Id);
        }

        public override void Setup()
        {
            base.Setup();
            CheckList.AddItem(this);
        }

        public override void Update()
        {
            var parent = Group;
            while (parent != null)
	            if (parent is Iteration iteration)
	            {
		            RuntimeValues["IterationIndex"] = iteration.Index;
		            parent = null;
	            }
	            else
		            parent = parent.Group;

	        DescriptionFormula?.Update(CheckList.Expressions, RuntimeValues);
        }

        public Dictionary<string, object> RuntimeValues { get; } = new Dictionary<string, object>();

        public override int GetHashCode()
        {
	        // ReSharper disable once NonReadonlyMemberInGetHashCode
            return Id;
        }

        public override dynamic GetModel(uint clientId)
        {
            var result = base.GetModel(clientId);
            result.Id = Id;
            result.CSSClass = CssClass;
            result.Description = DescriptionFormula?.Value ?? Description;
            result.NewLine = NewLine;
            result.TypeName = TypeName;
            result.IterationItem = IterationItem;
            result.IteratorModelItem = IteratorModelItem;
            return result;
        }
    }
}
