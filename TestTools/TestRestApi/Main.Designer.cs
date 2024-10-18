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
            TabControl = new TabControl();
            JsonTabPage = new TabPage();
            JsonTextBox = new TextBox();
            CursorLabel = new Label();
            ParametersTabPage = new TabPage();
            ParametersDataGridView = new DataGridView();
            ParameterNameColumn = new DataGridViewTextBoxColumn();
            ParameterValueColumn = new DataGridViewTextBoxColumn();
            LogTabPage = new TabPage();
            LogTextBox = new TextBox();
            CommandsPanel = new Panel();
            LoadButton = new Button();
            SaveButton = new Button();
            FormatButton = new Button();
            AbortButton = new Button();
            StartButton = new Button();
            TabControl.SuspendLayout();
            JsonTabPage.SuspendLayout();
            ParametersTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)ParametersDataGridView).BeginInit();
            LogTabPage.SuspendLayout();
            CommandsPanel.SuspendLayout();
            SuspendLayout();
            // 
            // TabControl
            // 
            TabControl.Controls.Add(JsonTabPage);
            TabControl.Controls.Add(ParametersTabPage);
            TabControl.Controls.Add(LogTabPage);
            TabControl.Dock = DockStyle.Fill;
            TabControl.Location = new Point(0, 0);
            TabControl.Name = "TabControl";
            TabControl.SelectedIndex = 0;
            TabControl.Size = new Size(1731, 763);
            TabControl.TabIndex = 0;
            // 
            // JsonTabPage
            // 
            JsonTabPage.Controls.Add(JsonTextBox);
            JsonTabPage.Controls.Add(CursorLabel);
            JsonTabPage.Location = new Point(4, 24);
            JsonTabPage.Name = "JsonTabPage";
            JsonTabPage.Padding = new Padding(3);
            JsonTabPage.Size = new Size(1723, 735);
            JsonTabPage.TabIndex = 0;
            JsonTabPage.Text = "Json";
            JsonTabPage.UseVisualStyleBackColor = true;
            // 
            // JsonTextBox
            // 
            JsonTextBox.Dock = DockStyle.Fill;
            JsonTextBox.Location = new Point(3, 3);
            JsonTextBox.Multiline = true;
            JsonTextBox.Name = "JsonTextBox";
            JsonTextBox.ScrollBars = ScrollBars.Both;
            JsonTextBox.Size = new Size(1717, 706);
            JsonTextBox.TabIndex = 0;
            JsonTextBox.KeyUp += ConfigurationTextBox_KeyUp;
            JsonTextBox.MouseUp += ConfigurationTextBox_MouseUp;
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
            // ParametersTabPage
            // 
            ParametersTabPage.Controls.Add(ParametersDataGridView);
            ParametersTabPage.Location = new Point(4, 24);
            ParametersTabPage.Name = "ParametersTabPage";
            ParametersTabPage.Padding = new Padding(3);
            ParametersTabPage.Size = new Size(1723, 735);
            ParametersTabPage.TabIndex = 2;
            ParametersTabPage.Text = "Parameters";
            ParametersTabPage.UseVisualStyleBackColor = true;
            // 
            // ParametersDataGridView
            // 
            ParametersDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            ParametersDataGridView.Columns.AddRange(new DataGridViewColumn[] { ParameterNameColumn, ParameterValueColumn });
            ParametersDataGridView.Dock = DockStyle.Fill;
            ParametersDataGridView.Location = new Point(3, 3);
            ParametersDataGridView.Name = "ParametersDataGridView";
            ParametersDataGridView.Size = new Size(1717, 729);
            ParametersDataGridView.TabIndex = 0;
            // 
            // ParameterNameColumn
            // 
            ParameterNameColumn.HeaderText = "Name";
            ParameterNameColumn.Name = "ParameterNameColumn";
            ParameterNameColumn.Width = 200;
            // 
            // ParameterValueColumn
            // 
            ParameterValueColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            ParameterValueColumn.HeaderText = "Value";
            ParameterValueColumn.Name = "ParameterValueColumn";
            // 
            // LogTabPage
            // 
            LogTabPage.Controls.Add(LogTextBox);
            LogTabPage.Location = new Point(4, 24);
            LogTabPage.Name = "LogTabPage";
            LogTabPage.Padding = new Padding(3);
            LogTabPage.Size = new Size(1723, 735);
            LogTabPage.TabIndex = 1;
            LogTabPage.Text = "Log";
            LogTabPage.UseVisualStyleBackColor = true;
            // 
            // LogTextBox
            // 
            LogTextBox.BackColor = Color.White;
            LogTextBox.Dock = DockStyle.Fill;
            LogTextBox.Location = new Point(3, 3);
            LogTextBox.Multiline = true;
            LogTextBox.Name = "LogTextBox";
            LogTextBox.ReadOnly = true;
            LogTextBox.Size = new Size(1717, 729);
            LogTextBox.TabIndex = 0;
            // 
            // CommandsPanel
            // 
            CommandsPanel.Controls.Add(LoadButton);
            CommandsPanel.Controls.Add(SaveButton);
            CommandsPanel.Controls.Add(FormatButton);
            CommandsPanel.Controls.Add(AbortButton);
            CommandsPanel.Controls.Add(StartButton);
            CommandsPanel.Dock = DockStyle.Bottom;
            CommandsPanel.Location = new Point(0, 763);
            CommandsPanel.Name = "CommandsPanel";
            CommandsPanel.Size = new Size(1731, 35);
            CommandsPanel.TabIndex = 1;
            // 
            // LoadButton
            // 
            LoadButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            LoadButton.Location = new Point(1644, 6);
            LoadButton.Name = "LoadButton";
            LoadButton.Size = new Size(75, 23);
            LoadButton.TabIndex = 4;
            LoadButton.Text = "Load";
            LoadButton.UseVisualStyleBackColor = true;
            LoadButton.Click += LoadButton_Click;
            // 
            // SaveButton
            // 
            SaveButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            SaveButton.Location = new Point(1563, 6);
            SaveButton.Name = "SaveButton";
            SaveButton.Size = new Size(75, 23);
            SaveButton.TabIndex = 3;
            SaveButton.Text = "Save";
            SaveButton.UseVisualStyleBackColor = true;
            SaveButton.Click += SaveButton_Click;
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
            AbortButton.Visible = false;
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
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1731, 798);
            Controls.Add(TabControl);
            Controls.Add(CommandsPanel);
            Name = "Main";
            Text = "Rest API";
            WindowState = FormWindowState.Maximized;
            TabControl.ResumeLayout(false);
            JsonTabPage.ResumeLayout(false);
            JsonTabPage.PerformLayout();
            ParametersTabPage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)ParametersDataGridView).EndInit();
            LogTabPage.ResumeLayout(false);
            LogTabPage.PerformLayout();
            CommandsPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TabControl TabControl;
        private TabPage JsonTabPage;
        private TabPage LogTabPage;
        private Panel CommandsPanel;
        private Button AbortButton;
        private Button StartButton;
        private Button FormatButton;
        private Label CursorLabel;
        private TabPage ParametersTabPage;
        private DataGridView ParametersDataGridView;
        private DataGridViewTextBoxColumn ParameterNameColumn;
        private DataGridViewTextBoxColumn ParameterValueColumn;
        private Button LoadButton;
        private Button SaveButton;
        private TextBox JsonTextBox;
        public TextBox LogTextBox;
    }
}
