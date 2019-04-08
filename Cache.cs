using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace QuickBrake {
	class Cache {
		public static bool CacheModified = false;
		public static string CacheDirectory = "";
		public static string CacheFile = "";

		public static void Start() {
			CacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "QuickBrake");
			CacheFile = CacheDirectory + "\\Default.ini";
			if (!Directory.Exists(CacheDirectory)) { Directory.CreateDirectory(CacheDirectory); }
		}

		public static void Setup() {
			if (IsPortable) {
				Program.HandBrakeLocation = Directory.GetCurrentDirectory() + "\\HandBrakeCLI.exe";
				Program.FileSaveLocation = "%f%";
			} else {
				string tempH;
				string tempF;

				GetCache(handbrake: out tempH, saveloc: out tempF);
				Console.WriteLine("HandBrake (CLI) Executable: " + tempH);
				Console.WriteLine("Save Location: " + tempF);
				Console.Write("\nDo you want to change these settings? [y/n]: ");

				if (NeoLib.Prompt() == true) {
					Console.Clear();
					Process();
				}
				Environment.Exit(0);
			}
		}

		public static void Process() {
			Console.WriteLine("Updating Cache in: " + CacheFile);
			if (Program.HandBrakeLocation.IsEmptyOrNull() || Program.HandBrakeLocation == "Unset") { CacheCheckOne(); }
			if (Program.FileSaveLocation.IsEmptyOrNull() || Program.FileSaveLocation == "Unset") { CacheCheckTwo(); }
			if (CacheModified) { SaveCache(); }
		}

		public static void CacheCheckOne() {
			Console.WriteLine("HandBrake location not set in cache...");
			DialogResult result = DialogResult.None;
			OpenFileDialog dialog = NeoLib.BrowseForFile(out result, title: "Please locate 'HandBrakeCLI.exe'", checkFile: true, checkPath: true, extension: "exe", filter: "HandBrakeCLI.exe|HandBrakeCLI.exe");
			if ((result == DialogResult.OK || result == DialogResult.Yes)) {
				Console.WriteLine("Got path: " + dialog.FileName);
				Program.HandBrakeLocation = dialog.FileName;
			} else {
				Program.HandBrakeLocation = "Unset";
			}
			CacheModified = true;
			return;
		}

		public static void CacheCheckTwo() {
			Console.WriteLine("Default save location not set in cache...");
			Console.Write("\nWould you like to save files in the same location as the input? [y/n]: ");
			if (NeoLib.Prompt() == true) { Program.FileSaveLocation = "%f%"; } else { FixFile(); }
			CacheModified = true;
			return;
		}

		public static void FixFile() {
			Console.WriteLine("Please input where you would like your files to be saved (ie c:/foo).");
			Console.WriteLine("[Hint: %f% will be replaced with the file's location during runtime]");
			string c = Console.ReadLine();
			if (c.Contains(":") && (c.Contains("/") || c.Contains(@"\"))) { Program.FileSaveLocation = c; } else { FixFile(); }
			return;
		}

		public static void SaveCache() {
			File.WriteAllLines(CacheFile, new string[] { Program.HandBrakeLocation, Program.FileSaveLocation });
			return;
		}

		public static bool Set { get { return File.Exists(CacheFile) ? (!File.ReadAllText(CacheFile).IsEmptyOrNull()) : false; } set { Process(); } }
		public static bool Exists { get { return File.Exists(CacheFile); } }

		public static void GetCache(out string handbrake, out string saveloc) {
			if (IsPortable) {
				handbrake = Directory.GetCurrentDirectory() + "\\HandBrakeCLI.exe";
				saveloc = "%f%";
				Program.HandBrakeLocation = handbrake;
				Program.FileSaveLocation = saveloc;
				return;
			} else {
				if (!Directory.Exists(CacheDirectory)) {
					Directory.CreateDirectory(CacheDirectory);
					Installer.FirstTimeSetup();
				}
				if (!File.Exists(CacheFile)) {
					File.Create(CacheFile).Dispose();
					File.WriteAllLines(CacheFile, new string[] { "Unset", "Unset" });
					Installer.FirstTimeSetup();
				}

				handbrake = File.ReadAllLines(CacheFile)[0];
				saveloc = File.ReadAllLines(CacheFile)[1];

				Program.HandBrakeLocation = handbrake;
				Program.FileSaveLocation = saveloc;
				return;
			}
		}

		public static void GetCache() {
			string h;
			string s;
			GetCache(out h, out s);
			return;
		}

		public static bool IsPortable {
			get {
				return File.Exists(Directory.GetCurrentDirectory() + "\\Portable.ini");
			} set {
				if (value) {
					File.Create(Directory.GetCurrentDirectory() + "\\Portable.ini");
				} else {
					File.Delete(Directory.GetCurrentDirectory() + "\\Portable.ini");
				}
			}
		}
	}
}
