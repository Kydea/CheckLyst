using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Matematici.ViewModels.Membership
{
    public class Login
    {
        public string Code { get; set; }

        public string Description { get; set; }

        public string Password { get; set; }

        public string ReturnUrl { get; set; }

        public int Company { get; set; }
    }
}
