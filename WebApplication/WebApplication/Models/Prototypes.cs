using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.WebApplication.Models
{
    public class Prototype
    {
        public enum Actions { Open, Add, Clone }

        public long? Id { get; set; }

        public Actions Action { get; set; }
    }
}