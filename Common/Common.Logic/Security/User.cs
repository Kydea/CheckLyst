using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Security
{
    public class User
    {
        public string Code { get; set; }

        public string Description { get; set; }

        public int Company { get; set; }

        public IEnumerable<Role> Roles { get; set; }
}
}
