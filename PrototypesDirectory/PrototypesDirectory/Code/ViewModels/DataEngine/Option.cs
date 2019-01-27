namespace Matematici.DataEngine
{
    public class Option
    {
        public Option (string value, string text = null)
        {
            Value = value;
            Text = text ?? value;
        }

        public string Text { get; set; }

        public string Value { get; set; }
    }
}
