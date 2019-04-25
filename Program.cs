using System;
using System.IO;
using System.Diagnostics;

namespace QuickBrake {
	class Program {
		public static string HandBrakeLocation;
		public static string FileSaveLocation;
		public static string[] args;

		[STAThread]
		public static void Main(string[] args) {
			Program.args = args;

			if (args != null && args.Length > 0 && args[0].ToLower() == "-u") { Installer.Uninstall(); } //Uninstallation takes priority, sadly

			if (!Cache.Exists) { Installer.FirstTimeSetup(); }

			int i = 0;
			foreach (string arg in args) {
				Console.WriteLine(i + "] " + arg);
				i++;
			}

			Cache.GetCache(out HandBrakeLocation, out FileSaveLocation);

			if (args == null || args.Length < 1) {
				if (Cache.CacheModified) { Environment.Exit(0); }
				Console.WriteLine("No valid arguments detected...");
				Cache.PromptReset();
				Console.Clear();
				Console.WriteLine("Cache set-up successfully");
				NeoLib.Wait(1000);
				Environment.Exit(0);
			} else if (args[0].Contains(".")) {
				HandBrake(new FileInfo(args[0]));
			}
		}

		static void HandBrake(FileInfo file) {
			string newArgs = "-i \"" + file + "\" -o \"" + FileSaveLocation.Replace("%f%", file.FullName.Replace(file.Extension, "") + "-2.mp4") + "\"";
			//Console.WriteLine("Shall run: " + hb + " " + newArgs);
			LaunchCommandLineApp(HandBrakeLocation, newArgs);
		}

		static void LaunchCommandLineApp(string exe, string args) {
			ProcessStartInfo startInfo = new ProcessStartInfo {
				CreateNoWindow = false,
				UseShellExecute = false,
				FileName = exe,
				WindowStyle = ProcessWindowStyle.Hidden,
				Arguments = args
			};

			try {
				using (Process exeProcess = Process.Start(startInfo)) {
					exeProcess.WaitForExit();
				}
			} catch { }
		}
	}
}
