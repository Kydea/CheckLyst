using System.Collections.Generic;

namespace CheckListManager.Common.Logic.DataEngine
{
    public class Form
    {
        public Dictionary<string, Field> Fields { get; } = new Dictionary<string, Field>();

        public List<ValidationMessage> ValidationMessages { get; } = new List<ValidationMessage>();

        public bool Valid { get; set; } = true;
    }
}
