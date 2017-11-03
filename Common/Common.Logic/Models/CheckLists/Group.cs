using System.Collections.Generic;
using System.Linq;

namespace CheckListManager.Common.Logic.Models.CheckLists
{
    public class Group : Item, IElementsBranch, ICheckListGroup
	{
        public override string TypeName => "Group";

	    public override string CssClass => string.Empty;

	    public List<Item> Items { get; } = new List<Item>();

        public List<Iteration> Iterations { get; } = new List<Iteration>();

        public bool Iterator { get; set; }

        private bool _valid;
        public override bool IsValidByDefault()
        {
            return _valid;
        }

        public override bool IsReadOnly(uint clientId)
        {
            return Page == null && (CheckList as Instance)?.State == "In progress" || base.IsReadOnly(clientId);
        }

        protected void SetupItems()
        {
            Item previous = null;

            foreach (var item in Items)
            {
                item.CheckList = CheckList;
                item.Page = Page;
                item.Group = this;
                item.Previous = previous;
                item.IteratorModelItem = Iterator || IteratorModelItem;
                item.Iteration = item.Iteration ?? Iteration;
                item.Setup();
                previous = item;
            }
        }

        public override void Setup()
        {
            base.Setup();
            SetupItems();

            if (Iterator && CheckList is Instance)
			{
				foreach (var iteration in Iterations)
				{
					iteration.CheckList = CheckList;
					iteration.Page = Page;
					iteration.Group = this;
					iteration.Setup();
				}
				Elements = new Dictionary<string, Element>();
			}
			else
                Elements = Items.Where(item => item.Code != null).ToDictionary(item => item.Code, item => (Element)item);
        }

        public override void Update()
        {
            base.Update();

            _valid = true;
            foreach (var item in Items)
            {
                item.Update();
                _valid = _valid && item.Valid;
            }

            var index = 1;
            if (Iterator)
            {
                _valid = true;
                foreach (var iteration in Iterations)
                {
                    iteration.Index = index++;
                    iteration.Update();
                    _valid = _valid && iteration.Valid;
                }
            }
        }

        public void UpdateValues(uint clientId)
        {
            foreach (var item in Items)
                if (item is Field field)
                    field.UpdateValues(clientId);

            if (Iterator)
                foreach (var iteration in Iterations)
                    iteration.UpdateValues(clientId);
        }

        public virtual Dictionary<string, Element> Elements { get; private set; }

        public void AddIteration()
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

        public void Remove(Iteration iteration)
        {
            Iterations.Remove(iteration);
            iteration.Remove();
        }

        public void Move(Iteration iteration, int step)
        {
            var index = Iterations.IndexOf(iteration);
            Iterations.RemoveAt(index);
            Iterations.Insert(index + step, iteration);
        }

        public override Item Clone(Group parent)
        {
            var group = new Group
            {
                CheckList = CheckList,
                Iterator = Iterator,
                Group = parent
            };
            foreach (var item in Items)
                group.Items.Add(item.Clone(group));

            Copy(this, group);
            return group;
        }

        public override void Remove()
        {
            foreach (var item in Items)
                item.Remove();

            if (Group.Iterator)
                foreach (var iteration in Iterations)
                    iteration.Remove();

            base.Remove();
        }

        public override dynamic GetModel(uint clientId)
        {
            var result = base.GetModel(clientId);
            result.Iterator = Iterator;
            if (Iterator)
                result.Iterations = Iterations.Select(iteration => iteration.GetModel(clientId));
            else
                result.Items = Items.Select(item => item.GetModel(clientId));
            return result;
        }

	}
}
