using CheckListManager.Common.Logic.Models.CheckLists;
using CheckListManager.Common.Logic.Models.Expressions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace CheckListManager.Prototypes.Logic.Models.CheckLists
{
	public class Iteration : Group, ICheckListIteration
	{
		public override string TypeName => "Iteration";

        [JsonIgnore]
        public int Index { get; set; }

        [JsonIgnore]
        public List<int> Indexes { get; set; }

        [JsonIgnore]
        public TypedValue ReccurencyValue { get; set; }

        public override void Setup()
        {
            foreach (var Item in Group.Items)
            {
                var Clone = Item.Clone(this);
                Clone.Iteration = this;
                Items.Add(Clone);
            }
        }

    }
}
