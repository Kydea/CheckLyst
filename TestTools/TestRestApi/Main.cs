using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Data.Common;
using System.Drawing.Text;
using System.IO;
using System.Text;
using TestRestApi.Properties;
using Formatting = Newtonsoft.Json.Formatting;

namespace TestRestApi
{
    public partial class Main : Form
    {

        public static Main? Instance { get; private set; }

        public Main()
        {
            InitializeComponent();
            Instance = this;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadSettings(Settings.Default.SaveFile);
        }

        private void FormatButton_Click(object sender, EventArgs e)
        {
            try
            {
                Parse();
            }
            catch (Exception Exception)
            {
                MessageBox.Show(Exception.Message);
            }
        }

        private RestApiModel Parse()
        {
            try
            {
                var value = JsonTextBox.Text;
                using var stringWriter = new StringWriter();
                using var jsonWriter = new JsonTextWriter(stringWriter);
                if (string.IsNullOrWhiteSpace(value))
                    JsonTextBox.Text = string.Empty;
                else
                {
                    jsonWriter.Formatting = Formatting.Indented;
                    jsonWriter.IndentChar = ' ';
                    jsonWriter.Indentation = 4;

                    var serializer = new JsonSerializer
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    };
                    serializer.Serialize(jsonWriter, JsonConvert.DeserializeObject(value));
                    JsonTextBox.Text = stringWriter.ToString();
                }
                return JsonConvert.DeserializeObject<RestApiModel>(JsonTextBox.Text);
            }
            catch (JsonReaderException JsonReaderException)
            {
                var line = JsonReaderException.LineNumber - 1;
                var column = JsonReaderException.LinePosition;
                if (line >= 0 && line < JsonTextBox.Lines.Length)
                {
                    int lineStartIndex = JsonTextBox.GetFirstCharIndexFromLine(line);
                    int targetPosition = lineStartIndex + column;
                    if (column <= JsonTextBox.Lines[line].Length)
                        JsonTextBox.SelectionStart = targetPosition;
                }

                var message = JsonReaderException.Message;
                var caretReferencePosition = message.LastIndexOf(", line ");
                if (caretReferencePosition > 0) message = $"{message[..caretReferencePosition]} [{line + 1}, {column + 1}]";
                JsonTextBox.Focus();
                throw new Exception(message);
            }
        }

        private async void StartButton_Click(object sender, EventArgs e)
        {
            try
            {
                LogTextBox.Clear();
                Settings.Default.SaveFile = SaveSettings();
                Settings.Default.Save();
                var model = Parse();
                model.Validate();

                ResultDataGridView.Columns.Clear();
                ResultDataGridView.Rows.Clear();

                var columnIndex = 0;
                foreach (var column in model.Columns)
                    ResultDataGridView.Columns.Add($"column{columnIndex++}", column);

                var rows = await new RestApiService(model, []).RunAsync();
                foreach (var row in rows)
                    ResultDataGridView.Rows.Add(row);
            }
            catch (Exception Exception)
            {
                MessageBox.Show(Exception.Message);
                if (LogTextBox.Text != string.Empty)
                    TabControl.SelectedIndex = 2;
            }
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
            int cursorPosition = JsonTextBox.SelectionStart;
            int line = JsonTextBox.GetLineFromCharIndex(cursorPosition);
            int column = cursorPosition - JsonTextBox.GetFirstCharIndexOfCurrentLine();
            CursorLabel.Text = $"Riga: {line + 1}, Colonna: {column + 1}";
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                CheckPathExists = true,
                CheckWriteAccess = true,
                RestoreDirectory = true
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                File.WriteAllText(dialog.FileName, SaveSettings());
                Settings.Default.SaveFile = SaveSettings();
                Settings.Default.Save();
            }
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                CheckPathExists = true,
                CheckFileExists = true,
                RestoreDirectory = true
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
                LoadSettings(File.ReadAllText(dialog.FileName));
        }

        private string SaveSettings()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(JsonTextBox.Lines.Length.ToString());
            stringBuilder.AppendLine(JsonTextBox.Text);
            for (var rowIndex = 0; rowIndex < ParametersDataGridView.RowCount - 1; ++rowIndex)
            {
                stringBuilder.AppendLine((string)ParametersDataGridView[0, rowIndex].Value);
                stringBuilder.AppendLine((string)ParametersDataGridView[1, rowIndex].Value);
            }
            return stringBuilder.ToString();
        }

        private void LoadSettings(string value)
        {
            JsonTextBox.Clear();
            ParametersDataGridView.Rows.Clear();
            var tokens = value.Split(Environment.NewLine);
            if (tokens.Length == 0 || !int.TryParse(tokens[0], out var jsonLines)) return;

            for (var lineIndex = 1; lineIndex < jsonLines; ++lineIndex)
                JsonTextBox.AppendText(tokens[lineIndex] + Environment.NewLine);
            if (!string.IsNullOrWhiteSpace(tokens[jsonLines]))
                JsonTextBox.AppendText(tokens[jsonLines] + Environment.NewLine);
            for (var parameterIndex = jsonLines + 1; parameterIndex < tokens.Length - 1; parameterIndex += 2)
                ParametersDataGridView.Rows.Add(tokens[parameterIndex], tokens[parameterIndex +  1]);
        }
    }
}
