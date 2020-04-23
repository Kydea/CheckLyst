using System;
using CheckListManager.Instances.Logic.Models.CheckLists;
using Newtonsoft.Json;

namespace CheckListManager.Instances.Logic.Models
{
	public class InstanceCacheObject
	{
        #region versioning

        //history
        /*
            2.0: zip compression
        */
        private const decimal ModelVersion = 2.0m;
        public decimal CacheObjectVersion { get; set; }
        public bool CheckVersion(decimal? ReferenceModelVersion) => CacheObjectVersion >= (ReferenceModelVersion ?? ModelVersion);
        public void SetModelVersion() { CacheObjectVersion = ModelVersion; }
        
        #endregion

        public long CompanyId { get; set; }

		public string Code { get; set; }
        public long Department { get; set; }
        public string DepartmentName { get; set; }
        public long PrototypeType { get; set; }
        public string PrototypeTypeName { get; set; }
        public long Prototype { get; set; }
        public string PrototypeName { get; set; }
        public string PrototypeDescription { get; set; }
        public int Version { get; set; }
        public string VersionState { get; set; }

        [JsonIgnore]
		public Instance Instance { get; set; }

		public long? Id { get; set; }
		public int? Year { get; set; }
		public int? Number { get; set; }

		public string Description { get; set; }
		public string InstanceState { get; set; }

        [JsonIgnore]
        public bool NeedToCache { get; set; }
		public bool NeedToSave { get; set; }

		public string CreatedBy { get; set; }
		public DateTimeOffset CreatedOn { get; set; }
		public string ModifiedBy { get; set; }
		public DateTimeOffset? ModifiedOn { get; set; }

        [JsonIgnore]
        public string YearCode => VersionState == Common.Data.VersionState.Draft ? "Test" : Year == 0 ? "SE" : (Year?.ToString() ?? "?");

        [JsonIgnore]
        public string InstanceCode => $"{PrototypeName}.{Version}.{(YearCode)}.{Number?.ToString() ?? "?"}";
    }
}