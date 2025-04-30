namespace Sort_er
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.Text = "sort-er";
            this.Size = new System.Drawing.Size(700, 550);
            this.BackColor = System.Drawing.Color.FromArgb(245, 245, 245);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);

            System.Windows.Forms.Panel mainPanel = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                Padding = new System.Windows.Forms.Padding(20)
            };
            this.Controls.Add(mainPanel);

            System.Windows.Forms.Label titleLabel = new System.Windows.Forms.Label
            {
                Text = "srt-er",
                Font = new System.Drawing.Font("Segoe UI", 18, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(44, 62, 80),
                Dock = System.Windows.Forms.DockStyle.Top,
                Height = 50,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            mainPanel.Controls.Add(titleLabel);

            System.Windows.Forms.GroupBox dirPanel = new System.Windows.Forms.GroupBox
            {
                Text = "Directory selection",
                Dock = System.Windows.Forms.DockStyle.Top,
                Height = 80,
                Padding = new System.Windows.Forms.Padding(10),
                Font = new System.Drawing.Font("Segoe UI", 10)
            };
            mainPanel.Controls.Add(dirPanel);

            directoryTextBox = new System.Windows.Forms.TextBox
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                Font = new System.Drawing.Font("Segoe UI", 10)
            };
            dirPanel.Controls.Add(directoryTextBox);

            System.Windows.Forms.Button browseButton = new System.Windows.Forms.Button
            {
                Text = "Browse",
                Dock = System.Windows.Forms.DockStyle.Right,
                Width = 100,
                BackColor = System.Drawing.Color.FromArgb(52, 152, 219),
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold),
                Cursor = System.Windows.Forms.Cursors.Hand,
                Name = "browseButton"
            };
            browseButton.Click += BrowseButton_Click;
            dirPanel.Controls.Add(browseButton);

            System.Windows.Forms.GroupBox optionsPanel = new System.Windows.Forms.GroupBox
            {
                Text = "Options",
                Dock = System.Windows.Forms.DockStyle.Top,
                Height = 90,
                Padding = new System.Windows.Forms.Padding(10),
                Font = new System.Drawing.Font("Segoe UI", 10),
                Top = dirPanel.Bottom + 10
            };
            mainPanel.Controls.Add(optionsPanel);

            System.Windows.Forms.CheckBox subdirsCheckbox = new System.Windows.Forms.CheckBox
            {
                Text = "Include subdirectories",
                Checked = true,
                Location = new System.Drawing.Point(10, 25),
                AutoSize = true
            };
            subdirsCheckbox.CheckedChanged += (s, e) => { includeSubdirectories = subdirsCheckbox.Checked; };
            optionsPanel.Controls.Add(subdirsCheckbox);

            System.Windows.Forms.CheckBox uppercaseCheckbox = new System.Windows.Forms.CheckBox
            {
                Text = "Folder names in capital letters",
                Checked = true,
                Location = new System.Drawing.Point(10, 50),
                AutoSize = true
            };
            uppercaseCheckbox.CheckedChanged += (s, e) => { useUppercase = uppercaseCheckbox.Checked; };
            optionsPanel.Controls.Add(uppercaseCheckbox);

            System.Windows.Forms.Panel buttonPanel = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Top,
                Height = 50,
                Top = optionsPanel.Bottom + 10
            };
            mainPanel.Controls.Add(buttonPanel);

            System.Windows.Forms.Button organizeButton = new System.Windows.Forms.Button
            {
                Text = "Organize files",
                Dock = System.Windows.Forms.DockStyle.Fill,
                BackColor = System.Drawing.Color.FromArgb(46, 204, 113),
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold),
                Cursor = System.Windows.Forms.Cursors.Hand,
                Name = "organizeButton"
            };
            organizeButton.Click += OrganizeButton_Click;
            buttonPanel.Controls.Add(organizeButton);

            System.Windows.Forms.GroupBox progressPanel = new System.Windows.Forms.GroupBox
            {
                Text = "Progress",
                Dock = System.Windows.Forms.DockStyle.Top,
                Height = 80,
                Padding = new System.Windows.Forms.Padding(10),
                Font = new System.Drawing.Font("Segoe UI", 10),
                Top = buttonPanel.Bottom + 10
            };
            mainPanel.Controls.Add(progressPanel);

            progressBar = new System.Windows.Forms.ProgressBar
            {
                Dock = System.Windows.Forms.DockStyle.Top,
                Height = 25
            };
            progressPanel.Controls.Add(progressBar);

            statusLabel = new System.Windows.Forms.Label
            {
                Text = "Waiting for operation...",
                Dock = System.Windows.Forms.DockStyle.Bottom,
                Height = 20
            };
            progressPanel.Controls.Add(statusLabel);

            System.Windows.Forms.GroupBox statsPanel = new System.Windows.Forms.GroupBox
            {
                Text = "Statistics",
                Dock = System.Windows.Forms.DockStyle.Top,
                Height = 100,
                Padding = new System.Windows.Forms.Padding(10),
                Font = new System.Drawing.Font("Segoe UI", 10),
                Top = progressPanel.Bottom + 10
            };
            mainPanel.Controls.Add(statsPanel);

            System.Windows.Forms.TableLayoutPanel statsGrid = new System.Windows.Forms.TableLayoutPanel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 2
            };
            statsPanel.Controls.Add(statsGrid);

            statsGrid.Controls.Add(new System.Windows.Forms.Label { Text = "Total files:", TextAlign = System.Drawing.ContentAlignment.MiddleLeft }, 0, 0);
            totalFilesLabel = new System.Windows.Forms.Label { Text = "0", Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            statsGrid.Controls.Add(totalFilesLabel, 1, 0);

            statsGrid.Controls.Add(new System.Windows.Forms.Label { Text = "Processed files:", TextAlign = System.Drawing.ContentAlignment.MiddleLeft }, 0, 1);
            processedFilesLabel = new System.Windows.Forms.Label { Text = "0", Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            statsGrid.Controls.Add(processedFilesLabel, 1, 1);

            statsGrid.Controls.Add(new System.Windows.Forms.Label { Text = "Folders created:", TextAlign = System.Drawing.ContentAlignment.MiddleLeft }, 0, 2);
            createdFoldersLabel = new System.Windows.Forms.Label { Text = "0", Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            statsGrid.Controls.Add(createdFoldersLabel, 1, 2);

            System.Windows.Forms.GroupBox logPanel = new System.Windows.Forms.GroupBox
            {
                Text = "Activity Log",
                Dock = System.Windows.Forms.DockStyle.Fill,
                Padding = new System.Windows.Forms.Padding(10),
                Font = new System.Drawing.Font("Segoe UI", 10),
                Top = statsPanel.Bottom + 10
            };
            mainPanel.Controls.Add(logPanel);

            logTextBox = new System.Windows.Forms.RichTextBox
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                BackColor = System.Drawing.Color.FromArgb(250, 250, 250),
                Font = new System.Drawing.Font("Console", 9),
                ReadOnly = true
            };
            logPanel.Controls.Add(logTextBox);

            mainPanel.Controls.SetChildIndex(logPanel, 0);
            mainPanel.Controls.SetChildIndex(statsPanel, 0);
            mainPanel.Controls.SetChildIndex(progressPanel, 0);
            mainPanel.Controls.SetChildIndex(buttonPanel, 0);
            mainPanel.Controls.SetChildIndex(optionsPanel, 0);
            mainPanel.Controls.SetChildIndex(dirPanel, 0);
            mainPanel.Controls.SetChildIndex(titleLabel, 0);

            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        }

        #endregion
    }
}