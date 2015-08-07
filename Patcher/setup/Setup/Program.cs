using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Terraria.ModLoader.Setup
{
    static class Program
    {
		public static string baseDir;
		public static readonly string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
	    public static readonly string libDir = Path.Combine(appDir, "..", "lib");
		public static readonly string toolsDir = Path.Combine(appDir, "..", "tools");
        public static string LogDir { get { return Path.Combine(baseDir, "logs"); } }
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

		public static void RunCmd(string dir, string cmd, string args, Action<string> output, Action<string> error) {
            var start = new ProcessStartInfo {
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
            using (var process = Process.Start(start)) {
                using (var reader = process.StandardOutput)
                    output(reader.ReadToEnd());
                using (var reader = process.StandardError)
                    error(reader.ReadToEnd());
            }
        }
    }
}
