using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

using Ookii.Dialogs.Wpf;

namespace QuickBrake {
    public partial class FileOpenBrowser {
        public delegate void DlgOnChange(string NewPath);
        public DlgOnChange OnChange;

        public FileOpenBrowser() {
            InitializeComponent();
        }

        public static DirectoryInfo ExecutingLocation() => ExecutingApplication().Directory;

        public static FileInfo ExecutingApplication() => new FileInfo(Assembly.GetExecutingAssembly().Location);

        public string Filter = "Any File (*.*)|*.*";

        void BrowseButton_Click(object Sender, RoutedEventArgs E) {
            VistaOpenFileDialog OpenDialog = new VistaOpenFileDialog {
                AddExtension = true,
                Filter = Filter,
                FilterIndex = 0,
                FileName = SelectedOpenPath,
                InitialDirectory = ExecutingLocation().FullName,
                Title = "Pick a file",
                ValidateNames = true
            };

            switch (OpenDialog.ShowDialog()) {
                case true:
                    PathTextBox.Text = OpenDialog.FileName;
                    OnChange?.Invoke(PathTextBox.Text);
                    break;
            }
        }

        #region Dependency Properties

        public string SelectedOpenPath {
            get => (string)GetValue(SelectedOpenPathProperty);
            set {
                SetValue(SelectedOpenPathProperty, value);
                OnChange?.Invoke(value);
            }
        }

        public static readonly DependencyProperty SelectedOpenPathProperty =
            DependencyProperty.Register(
                "SelectedOpenPath",
                typeof(string),
                typeof(FileOpenBrowser),
                new FrameworkPropertyMetadata(SelectedOpenPathChanged) {
                    BindsTwoWayByDefault = true,
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

        static void SelectedOpenPathChanged(DependencyObject D, DependencyPropertyChangedEventArgs E) {
            ((FileOpenBrowser)D).PathTextBox.Text = E.NewValue.ToString();
        }

        #endregion


        void PathTextBox_LostKeyboardFocus(object Sender, KeyboardFocusChangedEventArgs E) {
            SelectedOpenPath = PathTextBox.Text;
            OnChange?.Invoke(PathTextBox.Text);
        }
    }
}
