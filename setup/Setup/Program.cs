using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Terraria.ModLoader.Properties;

namespace Terraria.ModLoader.Setup
{
	static class Program
	{
		public static string baseDir;
		public static readonly string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		public static readonly string libDir = Path.Combine(appDir, "..", "lib");
		public static readonly string toolsDir = Path.Combine(appDir, "..", "tools");
		public static string LogDir => Path.Combine(baseDir, "logs");
		public static string ReferencesDir => Path.Combine(baseDir, "references");
		public static string TMLBinDir => Path.Combine(baseDir, "src", "tModLoader", "bin", "x86");

		public static string SteamDir => Settings.Default.SteamDir;
		public static string TerrariaPath => Path.Combine(SteamDir, "Terraria.exe");
		public static string TerrariaServerPath => Path.Combine(SteamDir, "TerrariaServer.exe");

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args) {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			AppDomain.CurrentDomain.AssemblyResolve += (sender, resArgs) => {
				var assemblyName = new AssemblyName(resArgs.Name);
				var name = assemblyName.Name;
				if (name == "Mono.Cecil" && assemblyName.Version < new Version(0, 10)) //multiple cecil versions need to be loaded separately
					name += "_0.9.6.0";

				return ResolveAssemblyFrom(libDir, name) ?? ResolveAssemblyFrom(ReferencesDir, name);
			};

			if (args.Length == 1 && args[0] == "--steamdir") {
				Console.WriteLine(SteamDir);
				return;
			}

			LoadBaseDir(args);
			if (baseDir == null)
				return;

			Application.Run(new MainForm());
		}

		private static Assembly ResolveAssemblyFrom(string libDir, string name)
		{
			var path = Path.Combine(libDir, name);
			path = new[] {".exe", ".dll"}.Select(ext => path+ext).SingleOrDefault(File.Exists);
			return path != null ? Assembly.LoadFrom(path) : null;
		}

		private static void LoadBaseDir(string[] args) {
			if (args.Length > 0 && Directory.Exists(args[0])) {
				baseDir = args[0];
				return;
			}

			var dialog = new FolderBrowserDialog {
				SelectedPath = Directory.GetCurrentDirectory(),
				Description = "Select tModLoader root directory"
			};

			if (dialog.ShowDialog() != DialogResult.OK)
				return;

			baseDir = dialog.SelectedPath;
		}

		public static int RunCmd(string dir, string cmd, string args, 
				Action<string> output = null, 
				Action<string> error = null,
				string input = null,
				CancellationToken cancel = default(CancellationToken)) {

			using (var process = new Process()) {
				process.StartInfo = new ProcessStartInfo {
					FileName = cmd,
					Arguments = args,
					WorkingDirectory = dir,
					UseShellExecute = false,
					RedirectStandardInput = input != null,
					CreateNoWindow = true
				};

				if (output != null) {
					process.StartInfo.RedirectStandardOutput = true;
					process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
				}

				if (error != null) {
					process.StartInfo.RedirectStandardError = true;
					process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
				}

				if (!process.Start())
					throw new Exception($"Failed to start process: \"{cmd} {args}\"");

				if (input != null) {
					var w = new StreamWriter(process.StandardInput.BaseStream, new UTF8Encoding(false));
					w.Write(input);
					w.Close();
				}

				while (!process.HasExited) {
					if (cancel.IsCancellationRequested) {
						process.Kill();
						throw new OperationCanceledException(cancel);
					}
					process.WaitForExit(100);

					output?.Invoke(process.StandardOutput.ReadToEnd());
					error?.Invoke(process.StandardError.ReadToEnd());
				}

				return process.ExitCode;
			}
		}

		 public static bool SelectTerrariaDialog() {
			while (true) {
				var dialog = new OpenFileDialog {
					InitialDirectory = Path.GetFullPath(Directory.Exists(SteamDir) ? SteamDir : baseDir),
					Filter = "Terraria|Terraria.exe",
					Title = "Select Terraria.exe"
				};

				if (dialog.ShowDialog() != DialogResult.OK)
					return false;

				string err = null;
				if (Path.GetFileName(dialog.FileName) != "Terraria.exe")
					err = "File must be named Terraria.exe";
				else if (!File.Exists(Path.Combine(Path.GetDirectoryName(dialog.FileName), "TerrariaServer.exe")))
					err = "TerrariaServer.exe does not exist in the same directory";

				if (err != null) {
					if (MessageBox.Show(err, "Invalid Selection", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Cancel)
						return false;
				}
				else {
					Settings.Default.SteamDir = Path.GetDirectoryName(dialog.FileName);
					Settings.Default.Save();
					return true;
				}
			}
		}
	}
}
