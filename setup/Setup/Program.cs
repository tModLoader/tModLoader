using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Terraria.ModLoader.Setup
{
    static class Program
    {
		public static string baseDir;
		public static readonly string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
	    public static readonly string libDir = Path.Combine(appDir, "..", "lib");
		public static readonly string toolsDir = Path.Combine(appDir, "..", "tools");
        public static string LogDir => Path.Combine(baseDir, "logs");
        public static ProgramSetting<bool> SuppressWarnings = new ProgramSetting<bool>("SuppressWarnings");

		/// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

			AppDomain.CurrentDomain.AssemblyResolve += (sender, resArgs) => {
				var path = Path.Combine(libDir, new AssemblyName(resArgs.Name).Name);
				path = new[] {".exe", ".dll"}.Select(ext => path+ext).SingleOrDefault(File.Exists);
				return path != null ? Assembly.LoadFrom(path) : null;
			};

			if (args.Length == 1 && args[0] == "--steamdir") {
			    Console.WriteLine(Settings.SteamDir.Get());
			    return;
			}

            LoadBaseDir(args);
            if (baseDir == null)
                return;

            Application.Run(new MainForm());
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
                CancellationToken cancel = default(CancellationToken)) {

            if (output == null) output = _ => {};
            if (error == null) error = _ => {};

            using (var process = new Process()) {
                process.StartInfo = new ProcessStartInfo {
                    FileName = cmd,
                    Arguments = args,
                    WorkingDirectory = dir,
                    UseShellExecute = false,
                    StandardOutputEncoding = Encoding.UTF8,
                    RedirectStandardOutput = true,
                    StandardErrorEncoding = Encoding.UTF8,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                if (!process.Start())
                    throw new Exception("Failed to start process: \""+cmd+" "+args+"\"");

                while (!process.HasExited) {
                    if (cancel.IsCancellationRequested) {
                        process.Kill();
                        throw new OperationCanceledException(cancel);
                    }
                    process.WaitForExit(100);

                    output(process.StandardOutput.ReadToEnd());
                    error(process.StandardError.ReadToEnd());
                }

                return process.ExitCode;
            }
        }
    }
}
