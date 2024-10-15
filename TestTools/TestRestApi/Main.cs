using Newtonsoft.Json;
using System.Data.Common;
using System.IO;
using Formatting = Newtonsoft.Json.Formatting;

namespace TestRestApi
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void FormatButton_Click(object sender, EventArgs e)
        {
            try
            {
                var value = ConfigurationTextBox.Text;
                using (var stringWriter = new StringWriter())
                using (var jsonWriter = new JsonTextWriter(stringWriter))
                {
                    if (string.IsNullOrWhiteSpace(value))
                        ConfigurationTextBox.Text = string.Empty;
                    else
                    {
                        jsonWriter.Formatting = Formatting.Indented;
                        jsonWriter.IndentChar = ' ';
                        jsonWriter.Indentation = 4;

                        var serializer = new JsonSerializer();
                        serializer.Serialize(jsonWriter, JsonConvert.DeserializeObject(value));
                        ConfigurationTextBox.Text = stringWriter.ToString();
                    }
                }
            }
            catch (JsonReaderException JsonReaderException)
            {
                var line = JsonReaderException.LineNumber - 1;
                var column = JsonReaderException.LinePosition;
                if (line >= 0 && line < ConfigurationTextBox.Lines.Length)
                {
                    int lineStartIndex = ConfigurationTextBox.GetFirstCharIndexFromLine(line);
                    int targetPosition = lineStartIndex + column;
                    if (column <= ConfigurationTextBox.Lines[line].Length)
                        ConfigurationTextBox.SelectionStart = targetPosition;
                }

                var message = JsonReaderException.Message;
                var caretReferencePosition = message.LastIndexOf(", line ");
                if (caretReferencePosition > 0) message = $"{message.Substring(0, caretReferencePosition)} [{line + 1}, {column + 1}]";
                MessageBox.Show(message);
                ConfigurationTextBox.Focus();
            }
            catch (Exception Exception)
            {
                MessageBox.Show(Exception.Message);
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            var model = JsonConvert.DeserializeObject<RestApiModel>(ConfigurationTextBox.Text);

        }

        private void ConfigurationTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            ShowCursorPosition();
        }

        private void ConfigurationTextBox_MouseUp(object sender, MouseEventArgs e)
        {
            ShowCursorPosition();
        }

        private void ShowCursorPosition()
        {
            int cursorPosition = ConfigurationTextBox.SelectionStart;
            int line = ConfigurationTextBox.GetLineFromCharIndex(cursorPosition);
            int column = cursorPosition - ConfigurationTextBox.GetFirstCharIndexOfCurrentLine();
            CursorLabel.Text = $"Riga: {line + 1}, Colonna: {column + 1}";
        }
    }
}
