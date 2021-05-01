using CheckListManager.Common.Logic.DataEngine;
using CheckListManager.Infrastructure.Data.ListItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Infrastructure.Logic.Companies
{
    public class CompaniesResponse : Form
    {
        public List<SettingValue> Settings { get; set; }
    }
}
