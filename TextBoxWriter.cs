using System.IO;
using System.Text;
using System.Windows.Controls;

namespace QuickBrake {
    public class TextBoxWriter : TextWriter {
        readonly TextBox _TextBox;
        public TextBoxWriter(TextBox TextBox) => this._TextBox = TextBox;

        public override void Write(char Value) {
            _TextBox.Text += Value;
        }

        public override void Write(string Value) {
            _TextBox.Text += Value;
        }

        public override Encoding Encoding => Encoding.Unicode;
    }
}
