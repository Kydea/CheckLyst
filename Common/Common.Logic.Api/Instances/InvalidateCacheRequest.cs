using System.Collections.Generic;

namespace CheckListManager.Common.Logic.Api.Instances
{
    public class InvalidateCacheRequest
    {
        public InvalidateCacheRequest(long prototype, int version, List<string> instances)
        {
            Prototype = prototype;
            Version = version;
            Instances = instances ?? new List<string>();
        }

        public long Prototype { get; }
        public int Version { get; }
        public List<string> Instances { get; }
    }
}