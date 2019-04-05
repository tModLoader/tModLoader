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
	public class PatchTask : Task
	{
		public readonly string baseDir;
		public readonly string patchedDir;
		public readonly string patchDir;
		public readonly ProgramSetting<DateTime> cutoff;
		private Patcher.Mode mode;
		private int warnings;
		private int failures;
		private int fuzzy;
		private StreamWriter logFile;

		private readonly ConcurrentBag<PatchedFile> results = new ConcurrentBag<PatchedFile>();

		public PatchTask(ITaskInterface taskInterface, string baseDir, string patchedDir, string patchDir, ProgramSetting<DateTime> cutoff) : base(taskInterface)
		{
			this.baseDir = baseDir;
			this.patchedDir = patchedDir;
			this.patchDir = patchDir;
			this.cutoff = cutoff;
		}

		public override bool StartupWarning()
		{
			return MessageBox.Show(
					"Any changes in /" + patchedDir + " that have not been converted to patches will be lost.",
					"Possible loss of data", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning)
				== DialogResult.OK;
		}

		public override void Run()
		{
			mode = (Patcher.Mode) Settings.Default.PatchMode;
			taskInterface.SetStatus("Deleting Old Src");

			if (Directory.Exists(patchedDir))
				Directory.Delete(patchedDir, true);

			var baseFiles = Directory.EnumerateFiles(baseDir, "*", SearchOption.AllDirectories);
			var patchFiles = Directory.EnumerateFiles(patchDir, "*", SearchOption.AllDirectories);

			var noCopy = new HashSet<string>();
			var removedFileList = Path.Combine(patchDir, DiffTask.RemovedFileList);
			if(File.Exists(removedFileList))
				foreach (var line in File.ReadAllLines(removedFileList))
					noCopy.Add(line);

			var items = new List<WorkItem>();
			foreach (var file in patchFiles) {
				var relPath = RelPath(patchDir, file);
				if (relPath.EndsWith(".patch")) {
					items.Add(new WorkItem("Patching: " + relPath, () => Patch(file)));
					noCopy.Add(relPath.Substring(0, relPath.Length - 6));
				}
				else if (relPath != DiffTask.RemovedFileList) {
					items.Add(new WorkItem("Copying: " + relPath, () => Copy(file, Path.Combine(patchedDir, relPath))));
				}
			}

			foreach (var file in baseFiles)
			{
				var relPath = RelPath(baseDir, file);
				if (DiffTask.excluded.Any(relPath.StartsWith) || noCopy.Contains(relPath))
					continue;

				var srcPath = Path.Combine(patchedDir, relPath);
				items.Add(new WorkItem("Copying: " + relPath, () => Copy(file, srcPath)));
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

			if (fuzzy > 0)
				taskInterface.Invoke(new Action(() => {
					var w = new ReviewWindow(results);
					w.AutoHeaders = true;
					ElementHost.EnableModelessKeyboardInterop(w);
					w.ShowDialog();
				}));
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

		private void Patch(string patchPath)
		{
			var pf = new PatchedFile {
				patchFilePath = patchPath,
				patchFile = PatchFile.FromText(File.ReadAllText(patchPath)),
				rootDir = ""
			};
			pf.title = RelPath(baseDir, pf.BasePath);

			if (!File.Exists(pf.BasePath))
			{
				Log($"MISSING file {pf.patchFile.basePath}\n");
				failures++;
				return;
			}

			pf.original = File.ReadAllLines(pf.BasePath);

			var patcher = new Patcher(pf.patchFile.patches, pf.original);
			patcher.Patch(mode);

			pf.patched = patcher.ResultLines;
			pf.results = patcher.Results.ToList();
			results.Add(pf);
			
			CreateParentDirectory(pf.PatchedPath);
			File.WriteAllLines(pf.PatchedPath, pf.patched);

			int exact = 0, offset = 0;
			foreach (var result in patcher.Results) {
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
			log.AppendLine($"{pf.title},\texact: {exact},\toffset: {offset},\tfuzzy: {fuzzy},\tfailed: {failures}");

			foreach (var res in patcher.Results)
				log.AppendLine(res.Summary());

			Log(log.ToString());
		}

		private void Log(string text)
		{
			lock (logFile)
				logFile.Write(text);
		}
	}
}