using System;
using System.IO;
using Microsoft.Win32;
using Microsoft.VisualBasic.FileIO;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Security.Permissions;
using System.Windows.Forms;
using System.Diagnostics;

namespace QuickBrake {
	class Installer {

		public static int setupCount = 0;
		public static string backupInstallFolder = "";
		public static string installFolder = "";
		
		[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
		public static void FirstTimeSetup() {
			if (setupCount > 0) { return; }
			backupInstallFolder = Path.GetFullPath(SpecialDirectories.ProgramFiles + "\\StarflashStudios\\QuickBrake Compressor\\");

			Console.Write("This program has not been set-up yet, do you wish to do so? [y/n]: ");
			if (!NeoLib.Prompt()) {
				Cache.GetCache();
				Program.Main(Program.args);
				return;
			}

			Console.WriteLine("Initialising first time set-up...");
			NeoLib.Wait(1000);
			Console.Clear();
			if (!NeoLib.IsElevated) {
				Console.WriteLine("Pick the program's installation location");
				GetInstallDirectory();
				return;
			}
			Console.Write("Install to: " + backupInstallFolder + "? [y/n]: ");
			if (NeoLib.Prompt()) {
				installFolder = backupInstallFolder;
				Install();
			} else {
				Console.WriteLine("Pick the program's installation location");
				GetInstallDirectory();
			}
		}

		public static void Install() {
			if (!Directory.Exists(installFolder)) { Directory.CreateDirectory(installFolder); }
			if (NeoLib.IsElevated) { InstallRegistry(); }
			if (File.Exists(installFolder + "QuickBrake.exe")) { File.Delete(installFolder + "QuickBrake.exe"); }
			File.Copy(Application.ExecutablePath, installFolder + "QuickBrake.exe");
			Console.WriteLine("Please download the HandBrakeCLI.exe file from here: https://handbrake.fr/downloads2.php and place it somewhere (preferably the install folder) ");
			NeoLib.Wait(3000);
			Process.Start("https://handbrake.fr/downloads2.php");
			Console.Write("\n\nPress enter once complete");
			NeoLib.Prompt();
			Console.Clear();
			Cache.CacheFile = installFolder + "Settings.ini";
			Cache.PromptReset();
			Cache.SaveCache();
			Console.WriteLine("Installation has finished, have a nice day :)");
			NeoLib.Wait(2000);
			Environment.Exit(0);
		}

		public static void Uninstall() {
			Console.Clear();
			Console.WriteLine("Very well... I can see that i am not wanted here *sniff*");
			try { Registry.ClassesRoot.DeleteSubKeyTree("StarflashStudios.QuickBrakeCompressor.1"); } catch {
				Console.WriteLine("\nAttempted to remove registry keys, but was unable");
				Console.WriteLine("If this program was installed without elevated privileges, ignore this.");
				Console.WriteLine("Otherwise, you may have to manually delete the SubKey 'StarflashStudios.QuickBrakeCompressor.1' and it's children found in 'Computer\\HKEY_CLASSES_ROOT'.");
			}

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
		
		public static void GetInstallDirectory(bool auth = true, bool cfd = true) {
			Console.Clear();
			if (cfd) {
				CommonOpenFileDialog dialog = new CommonOpenFileDialog {
					Title = "Pick the program's installation location",
					IsFolderPicker = true,
					Multiselect = false,
					EnsurePathExists = true,
					EnsureFileExists = false,
					AllowNonFileSystemItems = false,
				};
				if (auth) {
					dialog.InitialDirectory = SpecialDirectories.ProgramFiles + "\\StarflashStudios\\QuickBrake Compressor\\";
				} else {
					dialog.InitialDirectory = SpecialDirectories.MyDocuments;
				}
				CommonFileDialogResult result = dialog.ShowDialog();
				if (result == CommonFileDialogResult.Ok) {
					Console.Write("Got: " + dialog.FileName + "\\ -- is this correct? [y/n]: ");
					if (NeoLib.Prompt()) {
						installFolder = dialog.FileName + "\\";
						Install();
					} else {
						GetInstallDirectory(auth, cfd);
					}
				} else if (result == CommonFileDialogResult.Cancel) {
					CancelInstall();
				} else {
					GetInstallDirectory(auth, cfd);
				}
			} else {
				FolderBrowserDialog browse = new FolderBrowserDialog();
				DialogResult result = browse.ShowDialog();
				switch (result) {
					case DialogResult.OK:
						installFolder = browse.SelectedPath;
						break;
					case DialogResult.Yes:
						installFolder = browse.SelectedPath;
						break;
					case DialogResult.Cancel:
						CancelInstall();
						break;
					case DialogResult.Abort:
						CancelInstall();
						break;
					default:
						GetInstallDirectory(auth, cfd);
						break;
				}
			}			
		}

		public static void CancelInstall() {
			Console.WriteLine("Installation process cancelled...");
			NeoLib.Wait(2000);
			Environment.Exit(0);
		}

		public static void InstallRegistry() {
			RegistryKey BaseKey = Registry.ClassesRoot.CreateSubKey("StarflashStudios.QuickBrakeCompressor.1");
			BaseKey.SetValue("", "QuickBrake Video Compressor");
			BaseKey.SetValue("FriendlyTypeName", "@QuickBrake, -120");
			RegistryKey VerKey = BaseKey.CreateSubKey("CurVer");
			VerKey.SetValue("", "StarflashStudios.QuickBrakeCompressor.1");
			VerKey.Close();
			RegistryKey IconKey = BaseKey.CreateSubKey("DefaultIcon");
			IconKey.SetValue("", "QuickBrake, 0");
			IconKey.Close();
			RegistryKey Shellkey = BaseKey.CreateSubKey("shell");
			RegistryKey PlayKey = Shellkey.CreateSubKey("play");
			RegistryKey CommandKey = PlayKey.CreateSubKey("command");
			CommandKey.SetValue("", "\"%ProgramFiles%\\StarflashStudios\\QuickBrake Compressor\\Quickbrake.exe\" \"%1\"");
			CommandKey.Close();
			PlayKey.Close();
			Shellkey.Close();
			BaseKey.Close();
		}
	}
}
