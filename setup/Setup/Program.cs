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
		public static readonly string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		public static readonly string logsDir = Path.Combine("setup", "logs");

		public static string SteamDir => Settings.Default.SteamDir;
		public static string TMLDevSteamDir => Settings.Default.TMLDevSteamDir;
		public static string TerrariaPath => Path.Combine(SteamDir, "Terraria.exe");
		public static string TerrariaServerPath => Path.Combine(SteamDir, "TerrariaServer.exe");

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args) {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			/*if (args.Length == 1 && args[0] == "--steamdir") {
				Console.WriteLine(SteamDir);
				return;
			}*/
#if AUTO
			Settings.Default.TMLDevSteamDir = Settings.Default.SteamDir = @".\1423\Windows";
#else
			CreateTMLSteamDirIfNecessary();
#endif

			UpdateTargetsFile();

#if AUTO
			Console.WriteLine("Automatic setup start");
			new AutoSetup().DoAuto();
			Console.WriteLine("Automatic setup finished");
			return;
#endif
			Application.Run(new MainForm());
		}

		private static Assembly ResolveAssemblyFrom(string libDir, string name)
		{
			var path = Path.Combine(libDir, name);
			path = new[] {".exe", ".dll"}.Select(ext => path+ext).SingleOrDefault(File.Exists);
			return path != null ? Assembly.LoadFrom(path) : null;
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
					InitialDirectory = Path.GetFullPath(Directory.Exists(SteamDir) ? SteamDir : "."),
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
					Settings.Default.TMLDevSteamDir = "";
					Settings.Default.Save();

					CreateTMLSteamDirIfNecessary();
					UpdateTargetsFile();

					return true;
				}
			}
		}

		public static bool SelectTmlDirectoryDialog() {
			while (true) {
				var dialog = new OpenFileDialog {
					InitialDirectory = Path.GetFullPath(Directory.Exists(SteamDir) ? SteamDir : "."),
					ValidateNames = false,
					CheckFileExists = false,
					CheckPathExists = true,
					FileName = "Folder Selection.",
				};

				if (dialog.ShowDialog() != DialogResult.OK)
					return false;

				Settings.Default.TMLDevSteamDir = Path.GetDirectoryName(dialog.FileName);
				Settings.Default.Save();

				UpdateTargetsFile();
				
				return true;
			}
		}

		private static void CreateTMLSteamDirIfNecessary() {
			if (Directory.Exists(TMLDevSteamDir))
				return;
			
			Settings.Default.TMLDevSteamDir = Path.GetFullPath(Path.Combine(Settings.Default.SteamDir, "..", "tModLoaderDev"));
			Settings.Default.Save();

			try {
				Directory.CreateDirectory(TMLDevSteamDir);
			}
			catch (Exception e) {
				Console.WriteLine($"{e.GetType().Name}: {e.Message}");
			}
		}

		private static readonly string targetsFilePath = Path.Combine("src", "WorkspaceInfo.targets");

		internal static void UpdateTargetsFile() {
			SetupOperation.CreateParentDirectory(targetsFilePath);

			string gitsha = "";
			RunCmd("", "git", "rev-parse HEAD", s => gitsha = s.Trim());

			string branch = "";
			RunCmd("", "git", "rev-parse --abbrev-ref HEAD", s => branch = s.Trim());

			string GITHUB_HEAD_REF = Environment.GetEnvironmentVariable("GITHUB_HEAD_REF");
			if (!string.IsNullOrWhiteSpace(GITHUB_HEAD_REF)) {
				Console.WriteLine($"GITHUB_HEAD_REF found: {GITHUB_HEAD_REF}");
				branch = GITHUB_HEAD_REF;
			}
			string HEAD_SHA = Environment.GetEnvironmentVariable("HEAD_SHA");
			if (!string.IsNullOrWhiteSpace(HEAD_SHA)) {
				Console.WriteLine($"HEAD_SHA found: {HEAD_SHA}");
				gitsha = HEAD_SHA;
			}

			string targetsText =
$@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""14.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <!-- This file will always be overwritten, do not edit it manually. -->
  <PropertyGroup>
	<BranchName>{branch}</BranchName>
	<CommitSHA>{gitsha}</CommitSHA>
	<TerrariaSteamPath>{SteamDir}</TerrariaSteamPath>
    <tModLoaderSteamPath>{TMLDevSteamDir}</tModLoaderSteamPath>
  </PropertyGroup>
</Project>";

			if (File.Exists(targetsFilePath) && targetsText == File.ReadAllText(targetsFilePath))
				return;

			File.WriteAllText(targetsFilePath, targetsText);
		}
	}
}
