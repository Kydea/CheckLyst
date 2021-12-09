using System.Collections.Generic;

namespace CheckListManager.Common.Logic.Api.Instances
{
    public class InvalidateCacheRequest
    {
        public InvalidateCacheRequest(long prototype, int version, List<string> instances, bool onlyDeletion)
        {
            Prototype = prototype;
            Version = version;
            OnlyDeletion = onlyDeletion;
            Instances = instances ?? new List<string>();
        }

        public long Prototype { get; }
        public int Version { get; }
        public List<string> Instances { get; }
        public bool OnlyDeletion { get; }
    }
}