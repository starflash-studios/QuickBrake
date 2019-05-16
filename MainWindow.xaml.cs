using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

using Microsoft.Win32;

namespace QuickBrake {
    /// <summary>
    /// Interaction logic for Startup.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public MainWindow() {
            InitializeComponent();
            CheckFirstTime();
        }

        void CheckFirstTime() {
            //Do check here
        }

        void DragAreaEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effects = DragDropEffects.Link;
        }

        void DragAreaDrop(object sender, DragEventArgs e) {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            Debug.WriteLine("Got Files: ");
            files.WriteDebugLines();
            /*for (int f = 0; f < files.Length; f++) {
                files[f] = files[f].Nominal();
                Debug.WriteLine(files[f]);
            }*/

            try { ToProcessor(files); } catch { }
        }

        private void DragArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog() {
                Title = "Select Video Files...",
                Filter = "Video File (*.mp4; *.mkv)|*.mp4;*.mkv",
                Multiselect = true,
                ValidateNames = true,
                CheckPathExists = true,
                CheckFileExists = true
            };

            if (ofd.ShowDialog().ToBool()) {
                Debug.WriteLine("Got Files: ");
                ofd.FileNames.WriteDebugLines();
                /*for (int f = 0; f < ofd.FileNames.Length; f++) {
                    ofd.FileNames[f] = ofd.FileNames[f].Nominal();
                    Debug.WriteLine(ofd.FileNames[f]);
                }*/

                try { ToProcessor(ofd.FileNames); } catch { }
            }
        }

        public void ShowProcessor(string[] args) {
            this.Hide();
            List<string> newArgs = args.ToList();
            newArgs.Insert(0, Assembly.GetEntryAssembly().Location);

            Processor p = new Processor();
            p.Top = Top;
            p.Left = Left;
            p.Show();

            object[] parsed = new object[1];
            parsed[0] = newArgs;
            Type t = p.GetType();
            try {
                MethodInfo method = t.GetMethod("OnShow");
                method.Invoke(p, parsed);
            } catch { } // No OnShow() method
        }

        public void ToProcessor(string[] args) {
            this.Hide();
            List<string> newArgs = args.ToList();
            newArgs.Insert(0, System.Reflection.Assembly.GetEntryAssembly().Location);
            Processor p = new Processor();
            p.Show(this);
            p.Start(newArgs);
        }

        private void CacheButton_Click(object sender, RoutedEventArgs e) {
            this.Hide();
            Cache c = new Cache();
            c.Show(this);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            Process.Start("taskkill", "/f /im HandBrakeCLI.exe");
            Process.Start("taskkill", "/f /im QuickBrake.exe");
            Environment.Exit(0);
        }
    }
}
