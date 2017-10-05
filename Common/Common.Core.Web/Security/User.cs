using System.Collections.Generic;

namespace CheckListManager.Common.Core.Web.Security
{
    public class User
    {
        public string Code { get; set; }

        public string Description { get; set; }

        public int Company { get; set; }

        public IEnumerable<Role> Roles { get; set; }
}
}
