using System.Runtime.Serialization;

namespace CheckListManager.Common.Core.Web.Security
{
    public interface IDefaultIdentity
    {
        long CompanyId { get; }
        string Code { get; }
        string Description { get; }

	    bool HasPermission(string functionGroup, string function = "", long? department = null, long? prototypeType = null, string tag = null);
    }
}
