using System;
using CheckListManager.Instances.Logic.Models.CheckLists;
using Newtonsoft.Json;

namespace CheckListManager.Instances.Logic.Models
{
	public class InstanceCacheObject
	{
		public long CompanyId { get; set; }

		public string Code { get; set; }
		public long PrototypeType { get; set; }
		public long Prototype { get; set; }
		public int Version { get; set; }

		[JsonIgnore]
		public Instance Instance { get; set; }

		public long? Id { get; set; }
		public int? Year { get; set; }
		public int? Number { get; set; }

		public string Description { get; set; }
		public string InstanceState { get; set; }
		public string PrototypeName { get; set; }

		public bool IsDirty { get; set; }

		public string CreatedBy { get; set; }
		public DateTimeOffset CreatedOn { get; set; }
		public string ModifiedBy { get; set; }
		public DateTimeOffset? ModifiedOn { get; set; }

		public void AsChanged(string userCode)
		{
			IsDirty = true;
			ModifiedBy = userCode;
			ModifiedOn = DateTimeOffset.UtcNow;
		}
	}
}