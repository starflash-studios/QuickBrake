using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

using QuickBrake.Properties;

namespace QuickBrake {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Cache : Window {
        private static Settings settings;
        internal static Settings Settings { get => settings; set => settings = value; }
        public static readonly string[] Encoders = new string[] { "Auto", "x264", "x264_10bit", "x265", "x265_10bit", "x265_12bit", "MPEG2", "MPEG4", "VP8", "VP9", "Theora" };

        string oldHb;
        string oldSaveLoc;
        bool oldSaveL;
        string oldPre;
        string oldSuf;
        bool oldAutoS;
        bool oldAudioQ;
        string oldEncoder;

        public Cache() {
            InitializeComponent();
            //MainWindow m = new MainWindow();
            //m.Show(this);
        }

        public void OnShow() {
            Start();
        }

        public void Start() {
            Settings = Settings.Default;
            LoadEncoders();
            LoadSettings();
            UpdateOld();
            UpdateWarnings();
            Debug.WriteLine("Old: " + oldAudioQ + " settings: " + settings.AudioQueue + " DefSettings: " + Settings.Default.AudioQueue + " Check: " + HbAudioQueue.IsChecked.ToBool());
        }

        public void LoadEncoders() {
            foreach(string encoder in Encoders) {
                HbEncoderCombo.Items.Add(encoder);
            }
            HbEncoderCombo.SelectedIndex = Array.IndexOf(Encoders, settings.Encoder);
        }

        public void LoadSettings() {
            HbBrowseText.Text = settings.HandBrakeCLI;
            OutputBrowseText.Text = settings.SaveLocation;
            Debug.WriteLine(">>> Location: " + settings.SaveLocation + " | " + (!settings.SaveLocation.IsEmptyOrNull() && File.Exists(settings.SaveLocation)));
            OutputBrowseLocal.IsChecked = settings.SaveLocal;
            OutputPrefix.Text = settings.Prefix;
            OutputSuffix.Text = settings.Suffix;
            HbAutoStart.IsChecked = settings.AutoStart;
            HbAudioQueue.IsChecked = settings.AudioQueue;
        }

        public void UpdateWarnings() {
            bool hB = !settings.HandBrakeCLI.IsEmptyOrNull();
            bool output = false;
            if (!File.Exists(settings.HandBrakeCLI)) { hB = false; }
            if (settings.SaveLocal) {
                output = true;
            } else {
                if (!settings.SaveLocation.IsEmptyOrNull() || Directory.Exists(settings.SaveLocation)) {
                    output = true;
                }
            }
            HbBrowseWarning.Visibility = hB ? Visibility.Hidden : Visibility.Visible;
            OutputBrowseWarning.Visibility = output ? Visibility.Hidden : Visibility.Visible;

            SaveButton.IsEnabled = hB || output;
            SaveUpdated.Visibility = SettingsUpdated() ? Visibility.Visible : Visibility.Hidden;
        }

        public bool SettingsUpdated() {
            return (HbBrowseText.Text == oldHb) && (OutputBrowseText.Text == oldSaveLoc) && (OutputBrowseLocal.IsChecked == oldSaveL) && (OutputPrefix.Text == oldPre) && (OutputSuffix.Text == oldSuf) && (HbAutoStart.IsChecked == oldAutoS) && (HbAudioQueue.IsChecked == oldAudioQ) && (Encoders[HbEncoderCombo.SelectedIndex] == oldEncoder);
        }

        public void UpdateOld() {
            oldHb = settings.HandBrakeCLI;
            oldSaveLoc = settings.SaveLocation;
            oldSaveL = settings.SaveLocal;
            oldPre = settings.Prefix;
            oldSuf = settings.Suffix;
            oldAutoS = settings.AutoStart;
            oldAudioQ = settings.AudioQueue;
            oldEncoder = settings.Encoder;
        }

        private void Save_Click(object sender, RoutedEventArgs e) {
            Settings.Default.HandBrakeCLI = settings.HandBrakeCLI;
            Settings.Default.SaveLocation = settings.SaveLocation;
            Settings.Default.SaveLocal = settings.SaveLocal;
            Settings.Default.Prefix = settings.Prefix;
            Settings.Default.Suffix = settings.Suffix;
            Settings.Default.AutoStart = settings.AutoStart;
            Settings.Default.AudioQueue = settings.AudioQueue;
            Settings.Default.Encoder = settings.Encoder;
            Settings.Default.Save();
            UpdateOld();
            UpdateWarnings();
        }

        private void OutputBrowseButton_Click(object sender, RoutedEventArgs e) {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog {
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory,
                IsFolderPicker = true,
                EnsurePathExists = true,
                Title = "Output Location"
        };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok) {
                settings.SaveLocation = dialog.FileName;
                OutputBrowseText.Text = dialog.FileName;
            }

            UpdateWarnings();
        }

        private void OutputBrowseLocal_Changed(object sender, RoutedEventArgs e) {
            bool c = (OutputBrowseLocal.IsChecked).ToBool();
            OutputBrowseButton.IsEnabled = !c;
            OutputBrowseText.IsEnabled = !c;
            settings.SaveLocal = c;
            UpdateWarnings();
        }

        private void OutputPrefix_TextChanged(object sender, TextChangedEventArgs e) {
            OutputPrefix.Text = OutputPrefix.Text.EncodeWindows(); //Safe Characters Only
            settings.Prefix = OutputPrefix.Text;
            UpdateWarnings();
        }

        private void OutputSuffix_TextChanged(object sender, TextChangedEventArgs e) {
            OutputSuffix.Text = OutputSuffix.Text.EncodeWindows(); //Safe Characters Only
            settings.Suffix = OutputSuffix.Text;
            UpdateWarnings();
        }

        private void HbDownload_Click(object sender, RoutedEventArgs e) {
            Process.Start("https://handbrake.fr/downloads2.php");
        }

        public static bool IsValid() {
            bool path = Directory.Exists(settings.SaveLocation) || settings.SaveLocal;
            bool exe = File.Exists(settings.HandBrakeCLI);

            return path && exe;
        }

        private void HbBrowseButton_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog {
                InitialDirectory = Directory.GetCurrentDirectory(),
                Title = "Locate \"HandBrakeCLI.exe\" file",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "exe",
                Filter = "Executable Files (*.exe)|*.exe",
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (ofd.ShowDialog().ToBool()) {
                HbBrowseText.Text = ofd.FileName;
                settings.HandBrakeCLI = ofd.FileName;
            }

            UpdateWarnings();
        }

        private void OutputSuffix_KeyUp(object sender, System.Windows.Input.KeyEventArgs e) {
            OutputSuffix.Text = OutputSuffix.Text.EncodeWindows();
        }

        private void OutputPrefix_KeyUp(object sender, System.Windows.Input.KeyEventArgs e) {
            OutputPrefix.Text = OutputPrefix.Text.EncodeWindows();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) {
            MainWindow m = new MainWindow();
            m.Show(this);
        }

        public void ToProcessor() {
            Processor p = new Processor();
            p.Show(this);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            Process.Start("taskkill", "/f /im HandBrakeCLI.exe");
            Process.Start("taskkill", "/f /im QuickBrake.exe");
            Environment.Exit(0);
        }

        private void HbAutoStart_Changed(object sender, RoutedEventArgs e) {
            settings.AutoStart = HbAutoStart.IsChecked.ToBool();
            UpdateWarnings();
        }

        private void Window_Update(object sender, RoutedEventArgs e) {
            UpdateWarnings();
        }
        
        private void HbAudioQueue_Changed(object sender, RoutedEventArgs e) {
            settings.AudioQueue = HbAudioQueue.IsChecked.ToBool();
            UpdateWarnings();
        }

        private void HbEncoderCombo_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            settings.Encoder = Encoders[HbEncoderCombo.SelectedIndex];
        }
    }

    
}