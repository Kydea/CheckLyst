using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.Licence
{
    public class LicenseManagerRequest: LicensePayload
    {
        [Required]
        public string PrivateKey { get; set; }
    }
}
