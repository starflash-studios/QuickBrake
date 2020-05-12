using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace QuickBrake {
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();

            HandBrakeOpenDialog.Filter = "HandBrakeCLI.exe|HandBrakeCLI.exe|Any Executable File (*.exe)|*.exe|Any File (*.*)|*.*";
            if (Properties.Settings.Default.HandBrakeCLI.TryGetFile(out FileInfo HandBrakeCLI)) {
                HandBrakeOpenDialog.SelectedOpenPath = HandBrakeCLI.FullName;
                MediaProcessor.HandBrakeCLI = HandBrakeCLI;
            }
            HandBrakeOpenDialog.OnChange += HandBrakeOpenDialog_OnChange;

            CurrentPath.Filter = MediaFilter;
            CurrentPath.OnChange += CurrentPath_OnChange;

            //Populate Encoder Dropdown
            foreach (Encoder EncoderType in Enum.GetValues(typeof(Encoder))) {
                CurrentEncoder.Items.Add(EncoderType);
            }

            MediaList_SelectionChanged(this, default);

            string[] Args = Environment.GetCommandLineArgs();
            if (Args != null && Args.Length > 1) {
                for (int Index = 1; Index < Args.Length; Index++) {
                    string Arg = Args[Index];
                    if (Arg.TryGetFile(out FileInfo MediaFile)) { MediaList.Items.Add(new MediaProcessor(MediaFile)); }
                }
            }
        }

        public const string MediaFilter = "Any Video File (*.mp4;*.m4v;*.mkv;*.mov;*.mpg;*.wmv;*.avi)|*.mp4;*.m4v;*.mkv;*.mov;*.mpg;*.wmv;*.avi|Any Web Video File (*.flv;*.webm;*.mp4)|*.flv;*.webm;*.mp4|Any File (*.*)|*.*";
        
        #region Process Management
        public async Task ProcessAllFiles() {
            foreach(MediaProcessor MediaProcessor in MediaList.Items) {
                Debug.WriteLine("Will process from processor: " + MediaProcessor.ToLogString());
                Process Process = MediaProcessor.GetProcess();
                //Process.StartInfo.RedirectStandardInput = true;
                Process.StartInfo.UseShellExecute = false;
                Process.StartInfo.CreateNoWindow = true;
                Process.StartInfo.RedirectStandardOutput = true;
                Process.StartInfo.RedirectStandardError = true;

                Process.Start();
                Process.OutputDataReceived += Process_OutputDataReceived;
                Process.BeginOutputReadLine();
                Process.BeginErrorReadLine();

                await Process.WaitForExitAsync();
                //new MediaProcessor(File).GetProcess().Start();
            }
        }

        void Process_OutputDataReceived(object Sender, DataReceivedEventArgs E) => Dispatcher.Invoke(() => UpdateCommandOutputViewText(E.Data), System.Windows.Threading.DispatcherPriority.Normal);

        void UpdateCommandOutputViewText(string Text) {
            CommandOutputView.Text += (CommandOutputView.Text.IsNullOrEmpty() ? "" : "\n") + Text;
            CommandOutputView.ScrollToEnd();
        }
        #endregion

        #region XAML Event Handlers
        public static void HandBrakeOpenDialog_OnChange(string NewPath) {
            Debug.WriteLine("Got HandBrakeCLI Path: " + NewPath);
            FileInfo HandBrakeCLI = new FileInfo(NewPath);
            if (HandBrakeCLI == null || !HandBrakeCLI.Exists) { return; }
            MediaProcessor.HandBrakeCLI = HandBrakeCLI;

            Properties.Settings.Default.HandBrakeCLI = HandBrakeCLI.FullName;
            Properties.Settings.Default.Save();
        }

        async void StartButton_Click(object Sender, RoutedEventArgs E) {
            CommandOutputView.Text = "";
            StartButton.IsEnabled = false;
            await ProcessAllFiles();
            StartButton.IsEnabled = true;
        }

        #region MediaList Buttons (Add, Remove, Clear)

        void AddButton_Click(object Sender, RoutedEventArgs E) {
            FileInfo NewFile = WindowsExtensions.GetOpenFile(Filter: MediaFilter);
            if (NewFile != null) {
                MediaList.Items.Add(new MediaProcessor(NewFile));
                //Dispatcher.Invoke(() => MediaList.Items.Add(NewFile), DispatcherPriority.Normal);
            }
        }

        void RemoveButton_Click(object Sender, RoutedEventArgs E) {
            int SelectedIndex = MediaList.SelectedIndex;
            if (SelectedIndex >= 0) {
                MediaList.Items.RemoveAt(SelectedIndex);
            }
        }

        void ClearButton_Click(object Sender, RoutedEventArgs E) {
            switch(WindowsExtensions.ShowMessage(Title + " ・ Clear All?", "Are you sure you want to clear the list?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question)) {
                case MessageBoxResult.Yes:
                case MessageBoxResult.OK:
                    CommandOutputView.Text = "";
                    MediaList.Items.Clear();
                    break;
                // ReSharper disable once RedundantEmptySwitchSection
                default:
                    break;
            }
        }

        #endregion

        #region Current MediaProcessor Selection Handling

        bool _IgnoreCurrentUpdates;
        void MediaList_SelectionChanged(object Sender, System.Windows.Controls.SelectionChangedEventArgs E) {
            _IgnoreCurrentUpdates = true;

            int SelectedIndex = MediaList.SelectedIndex;
            if (Sender == null || SelectedIndex < 0) {
                CurrentPanel.IsEnabled = false;
                CurrentEncoder.SelectedIndex = -1;
                CurrentPath.SelectedSavePath = "";

                _IgnoreCurrentUpdates = false;
            } else {
                MediaProcessor MediaProcessor = (MediaProcessor)MediaList.Items[SelectedIndex];
                CurrentEncoder.SelectedIndex = (int)MediaProcessor.PreferredEncoder;
                CurrentPath.SelectedSavePath = MediaProcessor.DestinationFile.FullName;
                CurrentPanel.IsEnabled = true;
            }

            _IgnoreCurrentUpdates = false;
        }


        void CurrentPath_OnChange(string NewPath) {
            int ListIndex = MediaList.SelectedIndex;
            if (_IgnoreCurrentUpdates || ListIndex < 0 || NewPath.IsNullOrEmpty()) { return; }

            MediaProcessor MediaProcessor = (MediaProcessor)MediaList.Items[ListIndex];

            if (NewPath.TryGetFile(out FileInfo DestinationFile)) {
                MediaProcessor.DestinationFile = DestinationFile;
                MediaList.Items[ListIndex] = MediaProcessor;

                MediaList.SelectedIndex = ListIndex;
            }
        }

        void CurrentEncoder_SelectionChanged(object Sender, System.Windows.Controls.SelectionChangedEventArgs E) {
            int ListIndex = MediaList.SelectedIndex;
            int EncoderIndex = CurrentEncoder.SelectedIndex;
            if (_IgnoreCurrentUpdates || Sender == null || ListIndex < 0 || EncoderIndex < 0) { return; }

            MediaProcessor MediaProcessor = (MediaProcessor)MediaList.Items[ListIndex];
            MediaProcessor.PreferredEncoder = (Encoder)CurrentEncoder.Items[EncoderIndex];
            MediaList.Items[ListIndex] = MediaProcessor;

            MediaList.SelectedIndex = ListIndex;
        }

        #endregion

        #endregion
    }
}
