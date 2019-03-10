using System;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;

namespace QuickBrake {
	class Program {
		static string hb;
		static string saveLoc;
		static string cdir;
		static string cfile;
		static bool cModified = false;

		static void Main(string[] args) {
			int i = 0;
			foreach(string arg in args) {
				Console.WriteLine(i + "] " + arg);
				i++;
			}

			GetCache(out hb, out saveLoc, out cdir, out cfile);
			if (string.IsNullOrEmpty(hb) || hb == "Unset") { CacheCheckOne(); }
			if (string.IsNullOrEmpty(saveLoc) || saveLoc == "Unset") { CacheCheckTwo(); }
			if (cModified) { SaveCache(); }

			if (args.Length == 0) {
				if (cModified) { Environment.Exit(0); }
				Console.WriteLine("No valid arguements detected...");
				string tempF;
				string tempH;
				GetCache(out tempH, out tempF, out var temp, out var tempb);
				Console.WriteLine("HandBrake (CLI) Executable: " + tempH);
				Console.WriteLine("Save Location: " + tempF);
				Console.WriteLine("\nDo you want to change these settings? ");
				if (Prompt() == true) {
					Console.Clear();
					CacheCheckOne();
					CacheCheckTwo();
					SaveCache();
				}
				Environment.Exit(0);
			}

			if (args[0].Contains(".")) {
				HandBrake(new FileInfo(args[0]));
			}
		}

		static bool Prompt() => Console.ReadLine().ToLower()[0] == "y"[0];
		static bool Prompt(out string c) { c = Console.ReadLine(); return c.ToLower()[0] == "y"[0]; }

		static void CacheCheckOne() {
			Console.WriteLine("HandBrake location not set in cache...");
			try {
				using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Compatibility Assistant\Store\")) {
					if (key != null) {
						string[] keys = key.GetValueNames();
						foreach (string k in keys) {
							if (k.ToLower().Contains("handbrake.exe")) {
								hb = k;
							}
						}

						Console.WriteLine("Is this the executable location: " + hb + "? ");
						if (Prompt() == false) { FixHandBrake(); }
						key.Close();
						key.Dispose();
					} else {
						FixHandBrake();
					}
				}
			} catch { FixHandBrake(); }
			cModified = true;


			Console.Clear();
			return;
		}

		static void CacheCheckTwo() {
			Console.WriteLine("Default save location not set in cache...");
			Console.WriteLine("Would you like to save files in the same location as the input? ");
			if (Prompt() == true) { saveLoc = "%f%"; } else { FixFile(); }
			cModified = true;

			Console.Clear();
			return;
		}

		static void FixFile() {
			Console.WriteLine("Please input where you would like your files to be saved (ie c:/foo)");
			string c = Console.ReadLine();
			if (c.Contains(":") && (c.Contains("/") || c.Contains(@"\"))) { saveLoc = c; } else { FixFile(); }
			return;
		}

		static void FixHandBrake() {
			Console.WriteLine("Please give the exact location to the executable (ie c:/foo/bar.exe)");
			string c = Console.ReadLine();
			if (c.EndsWith(".exe") || c.EndsWith(".exe\"")) { hb = c; } else { FixHandBrake(); }
			return;
		}

		static void SaveCache() {
			File.WriteAllLines(cfile, new string[] { hb, saveLoc });
			return;
		}

		static void GetCache(out string handbrake, out string saveloc, out string cacheDir, out string cacheFile) {
			cacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "QuickBrake");
			cacheFile = cacheDir + "/Default.ini";

			if (!Directory.Exists(cacheDir)) { Directory.CreateDirectory(cacheDir); }
			if (!File.Exists(cacheFile)) {
				File.Create(cacheFile).Dispose();
				File.WriteAllLines(cacheFile, new string[]{ "Unset", "Unset" });
			}

			handbrake = File.ReadAllLines(cacheFile)[0];
			saveloc = File.ReadAllLines(cacheFile)[1];
		}

		static void HandBrake(FileInfo file) {
			string newArgs = "-i \"" + file + "\" -o \"" + saveLoc.Replace("%f%", file.FullName.Replace(file.Extension, "") + "-2.mp4") + "\"";
			//Console.WriteLine("Shall run: " + hb + " " + newArgs);
			LaunchCommandLineApp(hb, newArgs);
		}

		static void LaunchCommandLineApp(string exe, string args) {
			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.CreateNoWindow = false;
			startInfo.UseShellExecute = false;
			startInfo.FileName = exe;
			startInfo.WindowStyle = ProcessWindowStyle.Hidden;
			startInfo.Arguments = args;

			try {
				using (Process exeProcess = Process.Start(startInfo)) {
					exeProcess.WaitForExit();
				}
			} catch { }
		}
	}
}
