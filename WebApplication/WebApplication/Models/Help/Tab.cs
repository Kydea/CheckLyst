using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.WebApplication.Models
{
    public class Tab
    {
        public Tab(string Name)
        {
            this.Name = Name;
        }

        public string Name { get; }
        public string Description { get; set; }
        public bool IncludeLogFields { get; set; }
        public Field[] Fields { get; set; }
    }
}