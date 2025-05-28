using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Sort_er
{
    public partial class MainForm : Form
    {
        private string selectedDirectory = "";
        private bool includeSubdirectories = true;
        private bool useUppercase = true;
        private int totalFiles = 0;
        private int processedFiles = 0;
        private int createdFolders = 0;

        private TextBox directoryTextBox;
        private ProgressBar progressBar;
        private Label statusLabel;
        private Label totalFilesLabel;
        private Label processedFilesLabel;
        private Label createdFoldersLabel;
        private RichTextBox logTextBox;

        public MainForm()
        {
            InitializeComponent();
            selectedDirectory = Environment.CurrentDirectory;
            directoryTextBox.Text = selectedDirectory;
            Log("Application started. Select a directory and click on 'Organize files'.");
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.SelectedPath = selectedDirectory;
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedDirectory = folderDialog.SelectedPath;
                    directoryTextBox.Text = selectedDirectory;
                    Log($"Selected directory: {selectedDirectory}");
                }
            }
        }

        private async void OrganizeButton_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(selectedDirectory))
            {
                MessageBox.Show("Please select a valid directory.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            totalFiles = 0;
            processedFiles = 0;
            createdFolders = 0;
            totalFilesLabel.Text = "0";
            processedFilesLabel.Text = "0";
            createdFoldersLabel.Text = "0";
            progressBar.Value = 0;

            EnableControls(false);

            await Task.Run(() => OrganizeFiles());

            EnableControls(true);
        }

        private void EnableControls(bool enable)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool>(EnableControls), enable);
                return;
            }

            directoryTextBox.Enabled = enable;
            foreach (Control control in this.Controls.Find("browseButton", true))
                control.Enabled = enable;
            foreach (Control control in this.Controls.Find("organizeButton", true))
                control.Enabled = enable;
        }

        private void Log(string message)
        {
            if (logTextBox.InvokeRequired)
            {
                logTextBox.Invoke(new Action<string>(Log), message);
                return;
            }

            string timestamp = DateTime.Now.ToString("[HH:mm:ss]");
            logTextBox.AppendText($"{timestamp} {message}\r\n");
            logTextBox.ScrollToCaret();
        }

        private void UpdateStatus(string message)
        {
            if (statusLabel.InvokeRequired)
            {
                statusLabel.Invoke(new Action<string>(UpdateStatus), message);
                return;
            }
            statusLabel.Text = message;
        }

        private void UpdateProgress(int value)
        {
            if (progressBar.InvokeRequired)
            {
                progressBar.Invoke(new Action<int>(UpdateProgress), value);
                return;
            }
            progressBar.Value = Math.Min(value, 100);
        }

        private void UpdateStats()
        {
            if (totalFilesLabel.InvokeRequired)
            {
                totalFilesLabel.Invoke(new Action(UpdateStats));
                return;
            }
            totalFilesLabel.Text = totalFiles.ToString();
            processedFilesLabel.Text = processedFiles.ToString();
            createdFoldersLabel.Text = createdFolders.ToString();
        }

        private void OrganizeFiles()
        {
            try
            {
                UpdateStatus("Analyzing files...");
                Log($"Initiating organization in: {selectedDirectory}");

                Dictionary<string, List<string>> filesByExtension = new Dictionary<string, List<string>>();
                List<string> allFiles = new List<string>();

                if (includeSubdirectories)
                {
                    allFiles.AddRange(Directory.GetFiles(selectedDirectory, "*.*", SearchOption.AllDirectories));
                }
                else
                {
                    allFiles.AddRange(Directory.GetFiles(selectedDirectory));
                }

                string exePath = Application.ExecutablePath;
                allFiles.RemoveAll(file => file.Equals(exePath, StringComparison.OrdinalIgnoreCase));

                totalFiles = allFiles.Count;
                UpdateStats();
                Log($"Found {totalFiles} files to process");

                foreach (string filePath in allFiles)
                {
                    string extension = Path.GetExtension(filePath).TrimStart('.');

                    if (useUppercase)
                        extension = extension.ToUpper();
                    else
                        extension = extension.ToLower();

                    if (string.IsNullOrEmpty(extension))
                        extension = useUppercase ? "NO_EXTENSION" : "NO_extension";

                    if (!filesByExtension.ContainsKey(extension))
                        filesByExtension[extension] = new List<string>();

                    filesByExtension[extension].Add(filePath);
                }

                processedFiles = 0;
                createdFolders = 0;

                foreach (var kvp in filesByExtension)
                {
                    string extension = kvp.Key;
                    List<string> fileList = kvp.Value;
                    string folderPath = Path.Combine(selectedDirectory, extension);

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                        createdFolders++;
                        UpdateStats();
                        Log($"Folder created: {extension}");
                    }

                    foreach (string filePath in fileList)
                    {
                        string fileName = Path.GetFileName(filePath);
                        string destination = Path.Combine(folderPath, fileName);

                        if (File.Exists(destination))
                        {
                            string baseName = Path.GetFileNameWithoutExtension(fileName);
                            string ext = Path.GetExtension(fileName);
                            int counter = 1;
                            while (File.Exists(Path.Combine(folderPath, $"{baseName}_{counter}{ext}")))
                            {
                                counter++;
                            }
                            destination = Path.Combine(folderPath, $"{baseName}_{counter}{ext}");
                        }

                        try
                        {
                            File.Move(filePath, destination);
                            processedFiles++;
                            UpdateStats();

                            int progress = (int)((double)processedFiles / totalFiles * 100);
                            UpdateProgress(progress);

                            if (processedFiles % 10 == 0 || processedFiles == totalFiles)
                            {
                                UpdateStatus($"Processing... {processedFiles}/{totalFiles}");
                            }

                            if (processedFiles % 20 == 0 || processedFiles == 1 || processedFiles == totalFiles)
                            {
                                Log($"Processed {processedFiles}/{totalFiles} files");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log($"Error when moving {fileName}: {ex.Message}");
                        }
                    }
                }

                UpdateStatus("Organization completed!");
                Log("File organization successfully completed");
                MessageBox.Show($"{processedFiles} files have been organized into {createdFolders} folders.", "Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                UpdateStatus("Error during organization");
                Log($"ERROR: {ex.Message}");
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}