using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckListManager.WebApplication.Models.Help
{
    public class HelpPage
    {
        public bool ReadOnly { get; set; }
        public string Title { get; set; } = "#?#";
        public string RecordsName { get; set; } = "#?#";
        public string P1 { get; set; }
        public Field[] Buttons { get; set; }
        public Field[] Filters { get; set; }
        public Field[] Columns { get; set; }
        public Field[] DetailFields { get; set; }
        public Tab[] DetailTabs { get; set; }
        public bool IncludeLogColumns { get; set; } = true;
        public bool IncludeLogDetailFields { get; set; } = true;
    }
}