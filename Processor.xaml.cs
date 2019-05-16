using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

using QuickBrake.Properties;

namespace QuickBrake {
    /// <summary>
    /// Interaction logic for Processor.xaml
    /// </summary>
    public partial class Processor : Window {
        public List<string> files;
        public List<bool> complete;
        public Process process;
        public int currentFile = 0;
        public int totalComplete = 0;
        public bool running;
        public bool previewPlaying;
        public bool justOpened = true;
        public SoundPlayer AudioPlayer;
        
        public Processor() {
            InitializeComponent();
        }

        public void OnShow() {
            justOpened = true;
            currentFile = 0;
            totalComplete = 0;
            if (files == null) {
                MainWindow m = new MainWindow();
                m.Show(this);
            }
            //Start(null);
        }

        public void OnShow(List<string> arguments = null) {
            justOpened = true;
            currentFile = 0;
            if (files == null) {
                MainWindow m = new MainWindow();
                m.Show(this);
            }
            Start(arguments);
        }

        public void Start(List<string> arguments = null) {
            if (arguments == null) { arguments = Environment.GetCommandLineArgs().ToList(); }
            arguments.RemoveAt(0);
            files = arguments;
            complete = new List<bool>();
            for (int f = 0; f < files.Count; f++) {
                complete.Add(false);
            }

            MediaPlaceholder.Visibility = Visibility.Visible;
            DisplayFiles();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            UpdateUI();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private async Task UpdateUI() {
            Debug.WriteLine("<><><> Starting UI Function <><><>");
            while (true) {
                try {
                    if (MediaPreview.NaturalDuration.HasTimeSpan ) { MediaPreviewProgress.Maximum = MediaPreview.NaturalDuration.TimeSpan.TotalMilliseconds; }
                    if (previewPlaying) { MediaPreviewProgress.Value = MediaPreview.Position.TotalMilliseconds; }
                    if (running) { StartButton.Content = "Cancel"; }

                    MediaPlay.Source = previewPlaying ? Properties.Resources.Pause.BitmapImage() : Properties.Resources.Play.BitmapImage();
                    MediaPreview.Stretch = Stretch.UniformToFill;

                    await Task.Delay(200);
                } catch(Exception e) { Debug.WriteLine("<><><> UI Froze: " + e + " <><><>"); }
            }
        }

        public void Display(string prefix, string value) {
            try {
                if (Terminal.Text.IsEmptyOrNull()) {
                    Terminal.AppendText(prefix + " " + value);
                } else {
                    Terminal.AppendText(Environment.NewLine + prefix + " " + value);
                }
            } catch { } //probably owned by seperate thread
        }

        public void Display(string value) {
            try {
                if (Terminal.Text.IsEmptyOrNull()) {
                    Terminal.AppendText(value);
                } else {
                    Terminal.AppendText(Environment.NewLine + value);
                }
            } catch { } //probably owned by seperate thread
        }

        public void DisplayFiles() {
            DataTable dt = new DataTable();
            dt.Columns.Add("c1", typeof(bool));
            dt.Columns.Add("c2");
            for (int i = 0; i < files.Count; i++) {
                Debug.WriteLine("Got File: " + files[i] + " [" + complete[i] + "]");
                //dt.Rows.Add(complete[i], files[i].Truncate(60, "\\"));
                dt.Rows.Add(complete[i], files[i]);
                
            }
            
            GlobalList.DataContext = dt;
            try { GlobalList.ScrollIntoView(GlobalList.Items.GetItemAt(currentFile)); } catch { }

            GlobalProgress.Minimum = 0;
            GlobalProgress.Maximum = files.Count * 100;
            MediaProgress.Minimum = 0;
            MediaProgress.Maximum = 100;
            MediaPreviewProgress.Minimum = 0;

            if (totalComplete >= files.Count) {
                GlobalProgress.Value = files.Count * 100;
                GlobalProgressText.Text = "[" + files.Count + " / " + files.Count + "]";
                MediaProgress.Value = 100;
            } else {
                GlobalProgress.Value = currentFile * 100;
                GlobalProgressText.Text = "[" + totalComplete + " / " + files.Count + "]";
            }

            StartButton.Content = running ? "Cancel" : "Start";
            if (!running) {
                Debug.WriteLine("Attempting to scrape: " + files[currentFile]);
                //ScrapeMetadata(new FileInfo(files[currentFile]));
                //Debug.WriteLine("AutoStart: " + Settings.Default.AutoStart + " && justOpened: " + justOpened);
                if (Settings.Default.AutoStart && justOpened) {
                    justOpened = false;
                    ProcessFile();
                } else {
                    Display(">>>", "Awaiting Start Command...");
                }
            }
        }

        public void ProcessFile() {
            if (running) { return; }
            running = true;
            try {
                FileInfo file = new FileInfo(files[currentFile]);
                //ScrapeMetadata(file);
                HandBrake(file);
            } catch { } //End of queue
        }

        public void ScrapeMetadata(FileInfo file, bool autoplay = false) { //Supports more concise Metadata
            Dispatcher.Invoke(() => {
                Uri uri = new Uri(file.FullName);
                if (uri == MediaPreview.Source) { return; }
                MediaPreview.Source = uri;
                MediaPreview.LoadedBehavior = System.Windows.Controls.MediaState.Manual;
                previewPlaying = autoplay;
                try { MediaPreview.Stop(); } catch { }
                if (autoplay) {
                    MediaPreview.Play();
                }
                MediaName.Content = file.Name.ReverseSubstring(file.Name.LastIndexOf("."));
                MediaFormat.Content = file.Extension.TrimStart("."[0]).SentenceCase();

            });
        }

        private void PlayWav(Stream stream, bool loop = false) {
            if (AudioPlayer != null) {
                AudioPlayer.Stop();
                AudioPlayer.Dispose();
                AudioPlayer = null;
            }

            if (stream == null) { return; }

            AudioPlayer = new SoundPlayer(stream);
            if (loop) { AudioPlayer.PlayLooping(); } else { AudioPlayer.Play(); }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (running) {
                Process.Start("taskkill", "/f /im HandBrakeCLI.exe").WaitForExit(3000);
                try { File.Delete(GetCurrentFile()); } catch { }
            }
            Process.Start("taskkill", "/f /im QuickBrake.exe");
            Environment.Exit(0);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e) {
            if (running) {
                Process.Start("taskkill", "/f /im HandBrakeCLI.exe");
                try { process.Close(); } catch { }
                try { File.Delete(GetCurrentFile()); } catch { }
                StartButton.Content = "Start";
                DisplayFiles();
                //NextFile(false);
            } else {
                if (totalComplete >= files.Count) {
                    totalComplete = 0;
                    currentFile = 0;

                    complete = new List<bool>();
                    for (int f = 0; f < files.Count; f++) {
                        complete.Add(false);
                    }

                    justOpened = true;
                    DisplayFiles();
                }
                Display(">>>", "Starting...");
                Debug.WriteLine("TryStart");
                ProcessFile();
            }
        }

        public string GetCurrentFile() {
            if (Settings.Default.SaveLocal) { return GetLocalFile(new FileInfo(files[currentFile])); }
            return GetLocationalFile(new FileInfo(files[currentFile]));
        }

        public string GetLocalFile(FileInfo file) {
            return (file.DirectoryName + "\\" + Settings.Default.Prefix + file.Name.TrimEnd(file.Extension.ToCharArray()) + Settings.Default.Suffix + ".mp4");
        }

        public string GetLocationalFile(FileInfo file) {
            return (Settings.Default.SaveLocation + "\\" + Settings.Default.Prefix + file.Name.TrimEnd(file.Extension.ToCharArray()) + Settings.Default.Suffix + ".mp4");
        }

        public async void HandBrake(FileInfo file) {
            string enc = "";
            if (Settings.Default.Encoder != "Auto") {
                enc = "-e " + Settings.Default.Encoder.ToLower();
                Debug.WriteLine("Running with encoder: " + Settings.Default.Encoder.ToLower());
            }
            string args = enc + " -i \"" + file + "\" -o " + GetCurrentFile().Nominal();

            Debug.WriteLine("Running with: " + args);
            await LaunchCommandLineApp(Settings.Default.HandBrakeCLI, args);
            return;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task LaunchCommandLineApp(string exe, string args) {
            ProcessStartInfo startInfo = new ProcessStartInfo {
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = exe,
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = args,
                RedirectStandardOutput = true
            };

            try {
                Process proc = Process.Start(startInfo);
                process = proc;
                proc.EnableRaisingEvents = true;
                proc.Exited += Proc_Exited;
                proc.OutputDataReceived += OutputDataReceived;
                proc.BeginOutputReadLine();
            } catch { }
            return;
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        private void Proc_Exited(object sender, EventArgs e) {
            try {
                process.Exited -= Proc_Exited;
                process.Kill();
            } catch { }
            totalComplete++;
            complete[currentFile] = true;
            NextFile(true);
            return;
        }

        public void OutputDataReceived(object sender, DataReceivedEventArgs e) {
            Dispatcher.Invoke(() => {
                try {
                    Terminal.Text = e.Data;
                    foreach (Match match in Regex.Matches(e.Data, @"(\d+.\d+) %")) {
                        float v = 0;
                        if (float.TryParse(match.Groups[1].Value, out v)) {
                            MediaProgress.Value = v;
                            GlobalProgress.Value = currentFile * 100 + v;
                        }
                    }
                    Debug.WriteLine("<<< " + e.Data);
                } catch {
                    //NextFile();
                }
            });
        }

        public void NextFile(bool process = true) {
            running = false;
            Debug.WriteLine("Completed: " + currentFile);
            Dispatcher.Invoke(() => {
                Display(">>>", "Finished");
                if (currentFile > 0) { complete[currentFile] = true; }
                DisplayFiles();
            });

            if (totalComplete >= files.Count) {
                if (Settings.Default.AudioQueue) { PlayWav(Properties.Resources.Event, false); }
                //Debug.WriteLine("<<<PlayPingSound>>>");
                Dispatcher.Invoke(() => {
                    StartButton.Content = "Start Again";
                });
                return;
            } else {
                currentFile++;
                if (process) { ProcessFile(); }
            }
        }

        private void MediaControl_Click(object sender, RoutedEventArgs e) {
            if (MediaPreview.Source != null) {
                MediaPlaceholder.Visibility = Visibility.Hidden;
                if (previewPlaying) {
                    MediaPreview.Pause();
                } else {
                    MediaPreview.Play();
                }

                previewPlaying = !previewPlaying;
            }
        }

        private void MediaPreviewProgress_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e) {
            previewPlaying = false;
            MediaPreview.Pause();
        }

        private void MediaPreviewProgress_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
            MediaPreview.Position = new TimeSpan(0, 0, 0, 0, (int)MediaPreviewProgress.Value);
            MediaPreview.Play();
            previewPlaying = true;
        }

        private void GlobalList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            try {
                FileInfo file = new FileInfo(files[GlobalList.SelectedIndex]);
                ScrapeMetadata(file, true);
            } catch { }
        }
    }
}
