using System.Collections.Generic;

namespace Matematici.DataEngine
{
    public class Form
    {
        public Dictionary<string, Field> Fields { get; } = new Dictionary<string, Field>();

        public List<ValidationMessage> ValidationMessages { get; } = new List<ValidationMessage>();

        public bool Valid { get; set; } = true;
    }
}
