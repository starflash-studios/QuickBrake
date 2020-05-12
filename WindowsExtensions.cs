using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Ookii.Dialogs.Wpf;

namespace QuickBrake {
    public static class WindowsExtensions {
        public static FileInfo GetOpenFile(string Title = "Pick a file", string Filter = "Any File (*.*)|*.*", int FilterIndex = 0, DirectoryInfo InitialDirectory = default) {
            FileInfo[] Files = GetOpenFiles(Title, false, Filter, FilterIndex, InitialDirectory).ToArray();
            return Files.Length > 0 ? Files[0] : null;
        }

        public static IEnumerable<FileInfo> GetOpenFiles(string Title = "Pick a file", bool Multiselect = false, string Filter = "Any File (*.*)|*.*", int FilterIndex = 0, DirectoryInfo InitialDirectory = default) {
            VistaOpenFileDialog OpenFileDialog = new VistaOpenFileDialog {
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = Filter,
                FilterIndex = FilterIndex,
                InitialDirectory = (InitialDirectory ?? new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop))).FullName,
                Title = Title,
                Multiselect = Multiselect
            };

            if (OpenFileDialog.ShowDialog() == true) {
                foreach (string FileName in OpenFileDialog.FileNames) {
                    FileInfo Return = null;
                    try {
                        Return = new FileInfo(FileName);
                    } catch (ArgumentException Exc) {
                        Debug.WriteLine($"An exception was caught when generating a file from the path '{Exc}'");
                    }
                    if (Return != null) {
                        yield return Return;
                    }
                }
            }
        }

        public static MessageBoxResult ShowMessage(string Title, string Text, MessageBoxButton Buttons = MessageBoxButton.OK, MessageBoxImage Icon = MessageBoxImage.Information) => MessageBox.Show(Text, Title, Buttons, Icon);

    }
}
