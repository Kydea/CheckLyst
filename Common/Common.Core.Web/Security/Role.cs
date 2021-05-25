namespace CheckListManager.Common.Core.Web.Security
{
    public class Role
    {
        public string Code { get; set; }

        public long? Department { get; set; }

        public long? PrototypeType { get; set; }

        public string Level { get; set; }

	    public override string ToString()
	    {
		    return $"{Code}-{PrototypeType}";
	    }
    }
}
