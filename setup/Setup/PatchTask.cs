using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Terraria.ModLoader.Properties;
using DiffPatch;
using PatchReviewer;

namespace Terraria.ModLoader.Setup
{
	public class PatchTask : SetupOperation
	{
		private static string[] nonSourceDirs = { "bin", "obj", ".vs" };
		public static IEnumerable<(string file, string relPath)> EnumerateSrcFiles(string dir) =>
			EnumerateFiles(dir).Where(f => !f.relPath.Split('/', '\\').Any(nonSourceDirs.Contains));

		public readonly string baseDir;
		public readonly string patchedDir;
		public readonly string patchDir;
		public readonly ProgramSetting<DateTime> cutoff;
		private Patcher.Mode mode;
		private int warnings;
		private int failures;
		private int fuzzy;
		private StreamWriter logFile;

		private readonly ConcurrentBag<FilePatcher> results = new ConcurrentBag<FilePatcher>();

		public PatchTask(ITaskInterface taskInterface, string baseDir, string patchedDir, string patchDir, ProgramSetting<DateTime> cutoff) : base(taskInterface)
		{
			this.baseDir = PreparePath(baseDir);
			this.patchedDir = PreparePath(patchedDir);
			this.patchDir = PreparePath(patchDir);
			this.cutoff = cutoff;
		}

		public override bool StartupWarning()
		{
#if AUTO
			return true;
#endif
			return MessageBox.Show(
					"Any changes in /" + patchedDir + " that have not been converted to patches will be lost.",
					"Possible loss of data", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning)
				== DialogResult.OK;
		}

		public override void Run()
		{
			Program.UpdateTargetsFile(); //Update branch information

			mode = (Patcher.Mode) Settings.Default.PatchMode;

			string removedFileList = Path.Combine(patchDir, DiffTask.RemovedFileList);
			var noCopy = File.Exists(removedFileList) ? new HashSet<string>(File.ReadAllLines(removedFileList)) : new HashSet<string>();

			var items = new List<WorkItem>();
			var newFiles = new HashSet<string>();

			foreach (var (file, relPath) in EnumerateFiles(patchDir)) {
				if (relPath.EndsWith(".patch")) {
					items.Add(new WorkItem("Patching: " + relPath, () => newFiles.Add(PreparePath(Patch(file).PatchedPath))));
					noCopy.Add(relPath.Substring(0, relPath.Length - 6));
				}
				else if (relPath != DiffTask.RemovedFileList) {
					string destination = Path.Combine(patchedDir, relPath);

					items.Add(new WorkItem("Copying: " + relPath, () => Copy(file, destination)));
					newFiles.Add(destination);
				}
			}

			foreach (var (file, relPath) in EnumerateSrcFiles(baseDir)) {
				if (!noCopy.Contains(relPath)) {
					string destination = Path.Combine(patchedDir, relPath);

					items.Add(new WorkItem("Copying: " + relPath, () => Copy(file, destination)));
					newFiles.Add(destination);
				}
			}

			try
			{
				CreateDirectory(Program.logsDir);
				logFile = new StreamWriter(Path.Combine(Program.logsDir, "patch.log"));

				taskInterface.SetMaxProgress(items.Count);
				ExecuteParallel(items);
			}
			finally {
				logFile?.Close();
			}

			cutoff.Set(DateTime.Now);

			//Remove files and directories that weren't in patches and original src.

			taskInterface.SetStatus("Deleting Old Src");

			foreach (var (file, relPath) in EnumerateSrcFiles(patchedDir))
				if (!newFiles.Contains(file))
					File.Delete(file);

			DeleteEmptyDirs(patchedDir);

			//Show patch reviewer if there were any fuzzy patches.

			if (fuzzy > 0 || mode == Patcher.Mode.FUZZY && failures > 0)
				taskInterface.Invoke(new Action(() => ShowReviewWindow(results)));
		}

		private void ShowReviewWindow(IEnumerable<FilePatcher> results) {
			var w = new ReviewWindow(results, commonBasePath: baseDir+'/') {
				AutoHeaders = true,
			};
			ElementHost.EnableModelessKeyboardInterop(w);
			w.ShowDialog();
		}

		public override bool Failed() => failures > 0;
		public override bool Warnings() => warnings > 0;

		public override void FinishedDialog() {
			if (fuzzy > 0)
				return;

			MessageBox.Show(
				$"Patches applied with {failures} failures and {warnings} warnings.\nSee /logs/patch.log for details",
				"Patch Results", MessageBoxButtons.OK, Failed() ? MessageBoxIcon.Error : MessageBoxIcon.Warning);
		}

		private FilePatcher Patch(string patchPath)
		{
			var patcher = FilePatcher.FromPatchFile(patchPath);
			patcher.Patch(mode);
			results.Add(patcher);
			CreateParentDirectory(patcher.PatchedPath);
			patcher.Save();

			int exact = 0, offset = 0;
			foreach (var result in patcher.results) {
				if (!result.success) {
					failures++;
					continue;
				}

				if (result.mode == Patcher.Mode.FUZZY || result.offsetWarning) warnings++;
				if (result.mode == Patcher.Mode.EXACT) exact++;
				else if (result.mode == Patcher.Mode.OFFSET) offset++;
				else if (result.mode == Patcher.Mode.FUZZY) fuzzy++;
			}
			
			var log = new StringBuilder();
			log.AppendLine($"{patcher.patchFile.basePath},\texact: {exact},\toffset: {offset},\tfuzzy: {fuzzy},\tfailed: {failures}");

			foreach (var res in patcher.results)
				log.AppendLine(res.Summary());

			Log(log.ToString());

			return patcher;
		}

		private void Log(string text)
		{
			lock (logFile)
				logFile.Write(text);
		}
	}
}