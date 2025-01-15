using CheckListManager.Instances.Data.Models;
using CheckListManager.Instances.Logic.Models.CheckLists;
using System.Collections.Generic;

namespace CheckListManager.Instances.Logic.Messages
{
	public class LifeCycleResponse
    {
		public List<LifeCycleRecord> Records { get; set; }
	}
}
