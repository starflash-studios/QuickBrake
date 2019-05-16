using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using SharpShell.Attributes;
using SharpShell.SharpContextMenu;

namespace QuickBrake {
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".mp4")]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".mkv")]
    public class ConvertExtension : SharpContextMenu {
        
        protected override bool CanShowMenu() {
            return true;
        }

        protected override ContextMenuStrip CreateMenu() {
            var menu = new ContextMenuStrip();

            var itemCountLines = new ToolStripMenuItem {
                Text = "QuickBrake",
                Image = Properties.Resources.QuickBrake
            };

            itemCountLines.Click += (sender, args) => Convert();
            menu.Items.Add(itemCountLines);

            return menu;
        }

        private void Convert() {
            List<string> newArgs = SelectedItemPaths.ToList();
            newArgs.Insert(0, System.Reflection.Assembly.GetEntryAssembly().Location);
            Processor p = new Processor();
            p.Show();
            p.Start(newArgs);
        }
    }
}
