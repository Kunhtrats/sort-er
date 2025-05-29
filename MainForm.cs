using System;
using System.Collections.Concurrent;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
namespace Sort_er
{
    public partial class MainForm : Form
    {
        private string selectedDirectory = "";
        private bool includeSubdirectories = true;
        private bool useUppercase = true;
        private bool simulateMode = false;
        private int totalFiles = 0;
        private int processedFiles = 0;
        private int createdFolders = 0;
        private int skippedFiles = 0;
        private int duplicateFiles = 0;

        private CancellationTokenSource cancellationTokenSource;
        private readonly object lockObject = new object();

        private TextBox directoryTextBox;
        private ProgressBar progressBar;
        private Label statusLabel;
        private Label totalFilesLabel;
        private Label processedFilesLabel;
        private Label createdFoldersLabel;
        private Label skippedFilesLabel;
        private Label duplicateFilesLabel;
        private RichTextBox logTextBox;
        private Button cancelButton;
        private CheckBox simulateModeCheckBox;

        private Dictionary<string, string> customExtensionFolders = new Dictionary<string, string>
        {
            { "jpg", "Images" }, { "jpeg", "Images" }, { "png", "Images" }, { "gif", "Images" }, { "bmp", "Images" },
            { "mp4", "Videos" }, { "avi", "Videos" }, { "mkv", "Videos" }, { "mov", "Videos" },
            { "mp3", "Audio" }, { "wav", "Audio" }, { "flac", "Audio" }, { "m4a", "Audio" },
            { "pdf", "Documents" }, { "doc", "Documents" }, { "docx", "Documents" }, { "txt", "Documents" },
            { "exe", "Executables" }, { "msi", "Executables" }, { "bat", "Executables" },
            { "zip", "Archives" }, { "rar", "Archives" }, { "7z", "Archives" }, { "tar", "Archives" }
        };

        private readonly HashSet<string> excludedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "thumbs.db", "desktop.ini", ".ds_store", "hiberfil.sys", "pagefile.sys", "swapfile.sys"
        };

        public MainForm()
        {
            InitializeComponent();
            LoadSettings();
            selectedDirectory = string.IsNullOrEmpty(selectedDirectory) ? Environment.CurrentDirectory : selectedDirectory;
            directoryTextBox.Text = selectedDirectory;
            Log("Application started. Select a directory and click on 'Organize files'.");
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveSettings();
            CancelOperation();
            base.OnFormClosing(e);
        }

        private void LoadSettings()
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Sort-er");
                selectedDirectory = key.GetValue("LastDirectory", Environment.CurrentDirectory).ToString();
                includeSubdirectories = bool.Parse(key.GetValue("IncludeSubdirectories", "True").ToString());
                useUppercase = bool.Parse(key.GetValue("UseUppercase", "True").ToString());
                simulateMode = bool.Parse(key.GetValue("SimulateMode", "False").ToString());
            }
            catch
            {

                selectedDirectory = Environment.CurrentDirectory;
                includeSubdirectories = true;
                useUppercase = true;
                simulateMode = false;
            }
        }

        private void SaveSettings()
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Sort-er");
                key.SetValue("LastDirectory", selectedDirectory);
                key.SetValue("IncludeSubdirectories", includeSubdirectories);
                key.SetValue("UseUppercase", useUppercase);
                key.SetValue("SimulateMode", simulateMode);
            }
            catch
            {
            }
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.SelectedPath = selectedDirectory;
                folderDialog.Description = "Select directory to organize";

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedDirectory = folderDialog.SelectedPath;
                    directoryTextBox.Text = selectedDirectory;
                    Log($"Selected directory: {selectedDirectory}");

                    Task.Run(() => AnalyzeDirectory());
                }
            }
        }

        private async void AnalyzeDirectory()
        {
            try
            {
                var files = await GetFilesAsync(selectedDirectory, includeSubdirectories);
                var fileCount = files.Where(f => !IsExcludedFile(f)).Count();

                Invoke(new Action(() =>
                {
                    Log($"Directory contains {fileCount} files to organize.");
                }));
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    Log($"Error analyzing directory: {ex.Message}");
                }));
            }
        }

        private async void OrganizeButton_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            if (!simulateMode)
            {
                var result = MessageBox.Show(
                    "This operation will move files to organized folders. Do you want to continue?",
                    "Confirm Organization",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes) return;
            }

            await StartOrganization();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            CancelOperation();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(selectedDirectory) || !Directory.Exists(selectedDirectory))
            {
                MessageBox.Show("Please select a valid directory.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            try
            {
                var testFile = Path.Combine(selectedDirectory, Guid.NewGuid().ToString() + ".tmp");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
            }
            catch
            {
                MessageBox.Show("No write permissions in selected directory.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private async Task StartOrganization()
        {
            ResetCounters();
            cancellationTokenSource = new CancellationTokenSource();
            EnableControls(false);

            try
            {
                await OrganizeFilesAsync(cancellationTokenSource.Token);

                if (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    UpdateStatus("Organization completed!");
                    Log("File organization successfully completed");

                    var mode = simulateMode ? "simulated" : "organized";
                    MessageBox.Show(
                        $"{processedFiles} files have been {mode} into {createdFolders} folders.\n" +
                        $"Skipped: {skippedFiles}, Duplicates handled: {duplicateFiles}",
                        "Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (OperationCanceledException)
            {
                UpdateStatus("Operation cancelled");
                Log("Organization cancelled by user");
            }
            catch (Exception ex)
            {
                UpdateStatus("Error during organization");
                Log($"ERROR: {ex.Message}");
                MessageBox.Show($"An error occurred: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                EnableControls(true);
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
            }
        }

        private void ResetCounters()
        {
            totalFiles = 0;
            processedFiles = 0;
            createdFolders = 0;
            skippedFiles = 0;
            duplicateFiles = 0;
            UpdateStats();
            progressBar.Value = 0;
        }

        private void CancelOperation()
        {
            cancellationTokenSource?.Cancel();
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

            if (cancelButton != null)
                cancelButton.Enabled = !enable;
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

            if (logTextBox.Lines.Length > 1000)
            {
                var lines = logTextBox.Lines.Skip(200).ToArray();
                logTextBox.Lines = lines;
                logTextBox.ScrollToCaret();
            }
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
            progressBar.Value = Math.Min(Math.Max(value, 0), 100);
        }

        private void UpdateStats()
        {
            if (totalFilesLabel.InvokeRequired)
            {
                totalFilesLabel.Invoke(new Action(UpdateStats));
                return;
            }

            totalFilesLabel.Text = totalFiles.ToString("N0");
            processedFilesLabel.Text = processedFiles.ToString("N0");
            createdFoldersLabel.Text = createdFolders.ToString("N0");

            if (skippedFilesLabel != null)
                skippedFilesLabel.Text = skippedFiles.ToString("N0");
            if (duplicateFilesLabel != null)
                duplicateFilesLabel.Text = duplicateFiles.ToString("N0");
        }

        private async Task<List<string>> GetFilesAsync(string directory, bool includeSubdirs)
        {
            return await Task.Run(() =>
            {
                var searchOption = includeSubdirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                var files = Directory.GetFiles(directory, "*.*", searchOption).ToList();

                string exePath = Application.ExecutablePath;
                files.RemoveAll(file =>
                    file.Equals(exePath, StringComparison.OrdinalIgnoreCase) ||
                    IsExcludedFile(file));

                return files;
            });
        }

        private bool IsExcludedFile(string filePath)
        {
            string fileName = Path.GetFileName(filePath).ToLowerInvariant();
            return excludedFiles.Contains(fileName) || fileName.StartsWith("~");
        }

        private string GetFolderName(string extension)
        {
            string normalizedExt = extension.ToLowerInvariant();

            if (customExtensionFolders.ContainsKey(normalizedExt))
            {
                return useUppercase ?
                    customExtensionFolders[normalizedExt].ToUpperInvariant() :
                    customExtensionFolders[normalizedExt];
            }

            return useUppercase ? extension.ToUpperInvariant() : extension.ToLowerInvariant();
        }

        private async Task OrganizeFilesAsync(CancellationToken cancellationToken)
        {
            UpdateStatus("Analyzing files...");
            Log($"Initiating {(simulateMode ? "simulation" : "organization")} in: {selectedDirectory}");

            var allFiles = await GetFilesAsync(selectedDirectory, includeSubdirectories);
            totalFiles = allFiles.Count;
            UpdateStats();
            Log($"Found {totalFiles} files to process");

            if (totalFiles == 0)
            {
                Log("No files found to organize");
                return;
            }

            var filesByExtension = new ConcurrentDictionary<string, ConcurrentBag<string>>();

            await Task.Run(() =>
            {
                Parallel.ForEach(allFiles, new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                }, filePath =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string extension = Path.GetExtension(filePath).TrimStart('.');
                    if (string.IsNullOrEmpty(extension))
                        extension = "NO_EXTENSION";

                    string folderName = GetFolderName(extension);

                    filesByExtension.AddOrUpdate(folderName,
                        new ConcurrentBag<string> { filePath },
                        (key, existing) => { existing.Add(filePath); return existing; });
                });
            }, cancellationToken);

            processedFiles = 0;
            createdFolders = 0;

            foreach (var kvp in filesByExtension)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string folderName = kvp.Key;
                var fileList = kvp.Value.ToList();
                string folderPath = Path.Combine(selectedDirectory, folderName);

                if (!simulateMode && !Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    createdFolders++;
                    UpdateStats();
                    Log($"Folder created: {folderName}");
                }
                else if (simulateMode)
                {
                    createdFolders++;
                    UpdateStats();
                    Log($"[SIMULATION] Would create folder: {folderName}");
                }

                await ProcessFilesInFolder(fileList, folderPath, folderName, cancellationToken);
            }

            UpdateStatus($"{(simulateMode ? "Simulation" : "Organization")} completed!");
            Log($"File {(simulateMode ? "simulation" : "organization")} successfully completed");
        }

        private async Task ProcessFilesInFolder(List<string> files, string folderPath, string folderName, CancellationToken cancellationToken)
        {
            foreach (string filePath in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await ProcessSingleFile(filePath, folderPath, folderName);

                    if (processedFiles % 10 == 0 || processedFiles == totalFiles)
                    {
                        int progress = totalFiles > 0 ? (int)((double)processedFiles / totalFiles * 100) : 0;
                        UpdateProgress(progress);
                        UpdateStatus($"Processing... {processedFiles}/{totalFiles}");

                        if (processedFiles % 50 == 0 || processedFiles == totalFiles)
                        {
                            Log($"Processed {processedFiles}/{totalFiles} files");
                        }
                    }

                    if (processedFiles % 100 == 0)
                    {
                        await Task.Delay(1, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    string fileName = Path.GetFileName(filePath);
                    Log($"Error processing {fileName}: {ex.Message}");
                    skippedFiles++;
                }

                UpdateStats();
            }
        }

        private async Task ProcessSingleFile(string filePath, string folderPath, string folderName)
        {
            string fileName = Path.GetFileName(filePath);
            string destination = Path.Combine(folderPath, fileName);

            if (simulateMode)
            {
                Log($"[SIMULATION] Would move: {fileName} → {folderName}");
                processedFiles++;
                return;
            }

            if (File.Exists(destination))
            {
                destination = await GetUniqueFileName(folderPath, fileName);
                duplicateFiles++;
            }

            if (!File.Exists(filePath))
            {
                Log($"Source file no longer exists: {fileName}");
                skippedFiles++;
                return;
            }

            await Task.Run(() => File.Move(filePath, destination));
            processedFiles++;
        }

        private async Task<string> GetUniqueFileName(string folderPath, string fileName)
        {
            return await Task.Run(() =>
            {
                string baseName = Path.GetFileNameWithoutExtension(fileName);
                string extension = Path.GetExtension(fileName);
                int counter = 1;

                string newFileName;
                do
                {
                    newFileName = $"{baseName}_{counter}{extension}";
                    counter++;
                } while (File.Exists(Path.Combine(folderPath, newFileName)));

                return Path.Combine(folderPath, newFileName);
            });
        }
    }
}