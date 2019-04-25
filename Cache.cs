using System;
using System.IO;
using System.Windows.Forms;

namespace QuickBrake {
	class Cache {
		public static bool CacheModified = false;

		public static void PromptReset() {
			string tempH;
			string tempF;

			if (Exists) {
				GetCache(handbrake: out tempH, saveloc: out tempF);
				Console.WriteLine("HandBrake (CLI) Executable: " + tempH);
				Console.WriteLine("Save Location: " + tempF);
				Console.Write("\nDo you want to change these settings? [y/n]: ");

				if (NeoLib.Prompt() == true) {
					Console.Clear();
					UpdateCache();
				}
			} else {
				UpdateCache();
			}
			
			return;
		}
		
		public static void SaveCache() {
			File.WriteAllLines(CacheFile, new string[] { Program.HandBrakeLocation, Program.FileSaveLocation });
			return;
		}

		public static string CacheFile { get { return AppDomain.CurrentDomain.BaseDirectory + "\\Settings.ini"; } }
		
		public static bool Exists { get { return File.Exists(CacheFile); } }

        public static void GetCache(bool save = true) {
            string h;
            string s;
            GetCache(out h, out s);
            if (save) {
                Program.HandBrakeLocation = h;
                Program.FileSaveLocation = s;
            }
        }

		public static void GetCache(out string handbrake, out string saveloc) {
			if (!Exists) { CreateCache(); }
			string[] lines = File.ReadAllLines(CacheFile);
			if (lines.Length < 2 || lines[0] == "Unset" || lines [1] == "Unset" || lines[0].IsEmptyOrNull() || lines[1].IsEmptyOrNull()) { UpdateCache(); }
			handbrake = lines[0];
			saveloc = lines[1];
		}

		public static void CreateCache() {
			if (!Exists) { File.Create(CacheFile).Dispose(); File.WriteAllLines(CacheFile, new string[]{ "Unset", "Unset" }); }
		}

		public static void UpdateCache() {
			Console.Clear();
			Console.WriteLine("Setting up cache file...");
			SetupHandBrakeLocation();
			SetupSaveLocation();
			SaveCache();
			return;
		}

		static void SetupHandBrakeLocation() {
			Console.WriteLine("Please locate 'HandBrakeCLI.exe'");
			DialogResult result;
			OpenFileDialog dialog = NeoLib.BrowseForFile(out result, title: "Please locate 'HandBrakeCLI.exe'", checkFile: true, checkPath: true, extension: "exe", filter: "HandBrakeCLI.exe|HandBrakeCLI.exe");
			if ((result == DialogResult.OK || result == DialogResult.Yes)) {
				Console.WriteLine("Got path: " + dialog.FileName);
				Program.HandBrakeLocation = dialog.FileName;
			} else {
				Program.HandBrakeLocation = "Unset";
			}
			return;
		}

		static void SetupSaveLocation() {
			Console.Clear();
			//To be properly implemented later
			Program.FileSaveLocation = "%f%";
			return;
		}
	}
}
