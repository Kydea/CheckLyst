namespace TestRestApi
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tabControl1 = new TabControl();
            JsonTabPage = new TabPage();
            ConfigurationTextBox = new TextBox();
            OutputTabPage = new TabPage();
            textBox2 = new TextBox();
            CommandsPanel = new Panel();
            FormatButton = new Button();
            AbortButton = new Button();
            StartButton = new Button();
            CursorLabel = new Label();
            tabControl1.SuspendLayout();
            JsonTabPage.SuspendLayout();
            OutputTabPage.SuspendLayout();
            CommandsPanel.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(JsonTabPage);
            tabControl1.Controls.Add(OutputTabPage);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1731, 763);
            tabControl1.TabIndex = 0;
            // 
            // JsonTabPage
            // 
            JsonTabPage.Controls.Add(ConfigurationTextBox);
            JsonTabPage.Controls.Add(CursorLabel);
            JsonTabPage.Location = new Point(4, 24);
            JsonTabPage.Name = "JsonTabPage";
            JsonTabPage.Padding = new Padding(3);
            JsonTabPage.Size = new Size(1723, 735);
            JsonTabPage.TabIndex = 0;
            JsonTabPage.Text = "Json";
            JsonTabPage.UseVisualStyleBackColor = true;
            // 
            // ConfigurationTextBox
            // 
            ConfigurationTextBox.Dock = DockStyle.Fill;
            ConfigurationTextBox.Location = new Point(3, 3);
            ConfigurationTextBox.Multiline = true;
            ConfigurationTextBox.Name = "ConfigurationTextBox";
            ConfigurationTextBox.ScrollBars = ScrollBars.Both;
            ConfigurationTextBox.Size = new Size(1717, 706);
            ConfigurationTextBox.TabIndex = 0;
            ConfigurationTextBox.KeyUp += ConfigurationTextBox_KeyUp;
            ConfigurationTextBox.MouseUp += ConfigurationTextBox_MouseUp;
            // 
            // OutputTabPage
            // 
            OutputTabPage.Controls.Add(textBox2);
            OutputTabPage.Location = new Point(4, 24);
            OutputTabPage.Name = "OutputTabPage";
            OutputTabPage.Padding = new Padding(3);
            OutputTabPage.Size = new Size(1723, 735);
            OutputTabPage.TabIndex = 1;
            OutputTabPage.Text = "Output";
            OutputTabPage.UseVisualStyleBackColor = true;
            // 
            // textBox2
            // 
            textBox2.Dock = DockStyle.Fill;
            textBox2.Enabled = false;
            textBox2.Location = new Point(3, 3);
            textBox2.Multiline = true;
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(1717, 729);
            textBox2.TabIndex = 0;
            // 
            // CommandsPanel
            // 
            CommandsPanel.Controls.Add(FormatButton);
            CommandsPanel.Controls.Add(AbortButton);
            CommandsPanel.Controls.Add(StartButton);
            CommandsPanel.Dock = DockStyle.Bottom;
            CommandsPanel.Location = new Point(0, 763);
            CommandsPanel.Name = "CommandsPanel";
            CommandsPanel.Size = new Size(1731, 35);
            CommandsPanel.TabIndex = 1;
            // 
            // FormatButton
            // 
            FormatButton.Location = new Point(12, 6);
            FormatButton.Name = "FormatButton";
            FormatButton.Size = new Size(75, 23);
            FormatButton.TabIndex = 2;
            FormatButton.Text = "Format";
            FormatButton.UseVisualStyleBackColor = true;
            FormatButton.Click += FormatButton_Click;
            // 
            // AbortButton
            // 
            AbortButton.Enabled = false;
            AbortButton.Location = new Point(174, 6);
            AbortButton.Name = "AbortButton";
            AbortButton.Size = new Size(75, 23);
            AbortButton.TabIndex = 1;
            AbortButton.Text = "Abort";
            AbortButton.UseVisualStyleBackColor = true;
            // 
            // StartButton
            // 
            StartButton.Location = new Point(93, 6);
            StartButton.Name = "StartButton";
            StartButton.Size = new Size(75, 23);
            StartButton.TabIndex = 0;
            StartButton.Text = "Start";
            StartButton.UseVisualStyleBackColor = true;
            StartButton.Click += StartButton_Click;
            // 
            // CursorLabel
            // 
            CursorLabel.Dock = DockStyle.Bottom;
            CursorLabel.Location = new Point(3, 709);
            CursorLabel.Name = "CursorLabel";
            CursorLabel.Size = new Size(1717, 23);
            CursorLabel.TabIndex = 1;
            CursorLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1731, 798);
            Controls.Add(tabControl1);
            Controls.Add(CommandsPanel);
            Name = "Main";
            Text = "Rest API";
            tabControl1.ResumeLayout(false);
            JsonTabPage.ResumeLayout(false);
            JsonTabPage.PerformLayout();
            OutputTabPage.ResumeLayout(false);
            OutputTabPage.PerformLayout();
            CommandsPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl1;
        private TabPage JsonTabPage;
        private TabPage OutputTabPage;
        private TextBox ConfigurationTextBox;
        private Panel CommandsPanel;
        private Button AbortButton;
        private Button StartButton;
        private TextBox textBox2;
        private Button FormatButton;
        private Label CursorLabel;
    }
}
