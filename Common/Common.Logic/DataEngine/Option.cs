using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.DataEngine
{
    public class Option
    {
        public Option (string Value, string Text = null)
        {
            this.Value = Value;
            this.Text = Text ?? Value;
        }

        public string Text { get; set; }

        public string Value { get; set; }
    }
}
