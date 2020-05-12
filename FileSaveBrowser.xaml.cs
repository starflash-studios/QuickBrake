using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

using Ookii.Dialogs.Wpf;

namespace QuickBrake {
    public partial class FileSaveBrowser {
        public delegate void DlgOnChange(string NewPath);
        public DlgOnChange OnChange;

        public FileSaveBrowser() {
            InitializeComponent();

        }
        public static DirectoryInfo ExecutingLocation() => ExecutingApplication().Directory;

        public static FileInfo ExecutingApplication() => new FileInfo(Assembly.GetExecutingAssembly().Location);

        public string Filter = "Any File (*.*)|*.*";

        void BrowseButton_Click(object Sender, RoutedEventArgs E) {
            VistaSaveFileDialog SaveDialog = new VistaSaveFileDialog {
                AddExtension = true,
                Filter = Filter,
                FilterIndex = 0,
                FileName = SelectedSavePath,
                InitialDirectory = ExecutingLocation().FullName,
                OverwritePrompt = true,
                Title = "Pick a save location",
                ValidateNames = true
            };

            switch (SaveDialog.ShowDialog()) {
                case true:
                    PathTextBox.Text = SaveDialog.FileName;
                    OnChange?.Invoke(PathTextBox.Text);
                    break;
            }
        }

        #region Dependency Properties

        public string SelectedSavePath {
            get => (string)GetValue(SelectedSavePathProperty);
            set {
                SetValue(SelectedSavePathProperty, value);
                OnChange?.Invoke(value);
            }
        }

        public static readonly DependencyProperty SelectedSavePathProperty =
            DependencyProperty.Register(
                "SelectedSavePath",
                typeof(string),
                typeof(FileSaveBrowser),
                new FrameworkPropertyMetadata(SelectedSavePathChanged) {
                    BindsTwoWayByDefault = true,
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

        static void SelectedSavePathChanged(DependencyObject D, DependencyPropertyChangedEventArgs E) {
            ((FileSaveBrowser)D).PathTextBox.Text = E.NewValue.ToString();
        }

        #endregion

        void PathTextBox_LostKeyboardFocus(object Sender, KeyboardFocusChangedEventArgs E) {
            SelectedSavePath = PathTextBox.Text;
            OnChange?.Invoke(PathTextBox.Text);
        }
    }
}
