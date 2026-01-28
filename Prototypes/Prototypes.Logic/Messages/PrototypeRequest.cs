using CheckListManager.Common.Core.Helpers;
using CheckListManager.Prototypes.Logic.Models.CheckLists;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CheckListManager.Prototypes.Logic.Messages
{
    public class PrototypeRequest
    {
        public Prototype CheckList { get; private set; }
        public string Base64CheckList { get; set; }

        public void Parse(JsonConverter[] converters)
        {
            var bytes = Convert.FromBase64String(Base64CheckList);
            var decompressed = CompressionHelper.Decompress(bytes, true);
            var json = System.Text.Encoding.UTF8.GetString(decompressed);
            CheckList = json.ToObject<Prototype>(converters);
        }

        public long ID { get; set; }
        public int Version { get; set; }
        public int? LifeCycle { get; set; }
        public DateTimeOffset? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public string State { get; set; }
        public bool StateProposed { get; set; }
        public DateTimeOffset? ValidFrom { get; set; }
        public DateTimeOffset? ValidUntil { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long? Department { get; set; }
        public long? PrototypeType { get; set; }
        public long? InstanceConfirmationWorkFlow { get; set; }
        public List<string> ImagesToDelete { get; set; }
        public long? CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string UserCode { get; set; }
        public string TimeZone { get; set; }

        public bool InstanceCreation { get; set; }

        public string CreationReason { get; set; }
    }
}
