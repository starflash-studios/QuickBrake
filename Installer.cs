using System;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace QuickBrake {
	class Installer {

		public static int setupCount = 0;
		public static string backupInstallFolder = "";
		public static string installFolder = "";
		
		public static void FirstTimeSetup() {
			if (setupCount > 0) { return; }
			
			Console.Write("This program has not been set-up yet, do you wish to do so? [y/n]: ");
			if (!NeoLib.Prompt()) {
				Cache.GetCache();
				Program.Main(Program.args);
				return;
			}

			Console.Write("\nPlease ensure, if you have the required permissions, that this program is run in administrator mode for full functionality.\nContinue? [y/n]: ");
			if (!NeoLib.Prompt()) { Environment.Exit(0); }

			Console.WriteLine("Initialising first time set-up...");
			NeoLib.Wait(1000);
			Console.Clear();
			Console.Write("This will install to the current location, is this ok? [y/n]: ");
			installFolder = Directory.GetCurrentDirectory() + "\\";
			if (NeoLib.Prompt()) { Install(); } else { Environment.Exit(0); }
		}

		public static void Install() {
			Console.WriteLine("Please download and extract the HandBrakeCLI.exe file from here: https://handbrake.fr/downloads2.php and place it somewhere (preferably the install folder) ");
			NeoLib.Wait(3000);
			Process.Start("https://handbrake.fr/downloads2.php");
			Console.Write("\n\nPress enter once complete...");
            NeoLib.Wait();
            Console.WriteLine("Locate where you extracted the 'HandBrakeCLI.exe' file");
            Console.Clear();
			Cache.PromptReset();
			Cache.SaveCache();
			Console.WriteLine("Installation has finished, have a nice day :)");
			NeoLib.Wait(2000);
            CreateShortcut();
            Environment.Exit(0);
		}

        static void CreateShortcut() {
            string trueFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Microsoft\\Windows\\Start Menu\\Programs";
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string shortcutLocation = Path.Combine(folder, "QuickBrake.lnk");
            IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
            IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutLocation);

            shortcut.Description = "QuickBrake Media Compressor";                           // The description of the shortcut
            shortcut.IconLocation = Directory.GetCurrentDirectory() + "\\QuickBrake.exe";   // The icon of the shortcut
            shortcut.TargetPath = Directory.GetCurrentDirectory() + "\\QuickBrake.exe";     // The path of the file that will launch when the shortcut is run
            shortcut.Save();

            try { File.Copy(Path.Combine(folder, "QuickBrake.lnk"), Path.Combine(trueFolder, "QuickBrake.lnk")); } catch { Console.WriteLine("User is not elevated, start menu shortcut not created"); } //Program already has a shortcut
        }

		public static void Uninstall() {
			Console.Clear();
			Console.WriteLine("Very well... I can see that i am not wanted here *sniff*");

			try {
				if (File.Exists(Program.HandBrakeLocation)) {
					Console.Write("\nDelete: " + Program.HandBrakeLocation + " [y/n] ");
					if (NeoLib.Prompt()) { File.Delete(Program.HandBrakeLocation); }
				} else if (File.Exists(Directory.GetCurrentDirectory() + "\\HandBrakeCLI.exe")) {
					Console.Write("\nDelete: " + Directory.GetCurrentDirectory() + "\\HandBrakeCLI.exe" + " [y/n] ");
					if (NeoLib.Prompt()) { File.Delete(Directory.GetCurrentDirectory() + "\\HandBrakeCLI.exe"); }
				}
			} catch { } //HandBrakeCLI.exe already removed

			try {
				Console.Write("\nDelete cache? [y/n]: ");
				if (NeoLib.Prompt()) { File.Delete(Cache.CacheFile); }
			} catch { } //Cache already removed

			Console.WriteLine("We are sorry to see you go...");
			NeoLib.Wait(1000);

			Process.Start("cmd.exe", "/C choice /C Y /N /D Y /T 1 & Del \"" + Application.ExecutablePath + "\"", null, null, null);
			Environment.Exit(0);
		}

		public static void CancelInstall() {
			Console.WriteLine("Installation process cancelled...");
			NeoLib.Wait(2000);
			Environment.Exit(0);
		}
	}
}
