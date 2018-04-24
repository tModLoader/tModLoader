using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using ICSharpCode.NRefactory.CSharp;
using Terraria.ModLoader.Properties;
using DiffPatch;
using PatchReviewer;

namespace Terraria.ModLoader.Setup
{
	public class PatchTask : Task
	{
		public readonly string baseDir;
		public readonly string srcDir;
		public readonly string patchDir;
		public readonly ProgramSetting<DateTime> cutoff;
		public readonly CSharpFormattingOptions format;
		public readonly Patcher.Mode mode;
		private int warnings;
		private int failures;
		private int fuzzy;
		private StreamWriter logFile;

		private readonly ConcurrentBag<PatchedFile> results = new ConcurrentBag<PatchedFile>();

		public string FullBaseDir => Path.Combine(Program.baseDir, baseDir);
		public string FullSrcDir => Path.Combine(Program.baseDir, srcDir);
		public string FullPatchDir => Path.Combine(Program.baseDir, patchDir);

		public PatchTask(ITaskInterface taskInterface, string baseDir, string srcDir, string patchDir,
			ProgramSetting<DateTime> cutoff, CSharpFormattingOptions format = null) : base(taskInterface)
		{
			this.baseDir = baseDir;
			this.srcDir = srcDir;
			this.patchDir = patchDir;
			this.format = format;
			this.cutoff = cutoff;
			this.mode = (Patcher.Mode) Settings.Default.PatchMode;
		}

		public override bool StartupWarning()
		{
			return MessageBox.Show(
					"Any changes in /" + srcDir + " that have not been converted to patches will be lost.",
					"Possible loss of data", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning)
				== DialogResult.OK;
		}

		public override void Run()
		{
			taskInterface.SetStatus("Deleting Old Src");

			if (Directory.Exists(FullSrcDir))
				Directory.Delete(FullSrcDir, true);

			var baseFiles = Directory.EnumerateFiles(FullBaseDir, "*", SearchOption.AllDirectories);
			var patchFiles = Directory.EnumerateFiles(FullPatchDir, "*", SearchOption.AllDirectories);

			var removedFileList = Path.Combine(FullPatchDir, DiffTask.RemovedFileList);
			var removedFiles = File.Exists(removedFileList) ? new HashSet<string>(File.ReadAllLines(removedFileList)) : new HashSet<string>();

			var copyItems = new List<WorkItem>();
			var patchItems = new List<WorkItem>();
			var formatItems = new List<WorkItem>();
			
			foreach (var file in baseFiles)
			{
				var relPath = RelPath(FullBaseDir, file);
				if (DiffTask.excluded.Any(relPath.StartsWith) || removedFiles.Contains(relPath))
					continue;

				var srcPath = Path.Combine(FullSrcDir, relPath);
				copyItems.Add(new WorkItem("Copying: " + relPath, () => Copy(file, srcPath)));

				if (format != null && file.EndsWith(".cs"))
					formatItems.Add(new WorkItem("Formatting: " + relPath,
						() => FormatTask.Format(srcPath, format, taskInterface.CancellationToken())));
			}

			foreach (var file in patchFiles) {
				var relPath = RelPath(FullPatchDir, file);
				if (relPath.EndsWith(".patch"))
					patchItems.Add(new WorkItem("Patching: " + relPath, () => Patch(file)));
				else if (relPath != DiffTask.RemovedFileList)
					copyItems.Add(new WorkItem("Copying: " + relPath, () => Copy(file, Path.Combine(FullSrcDir, relPath))));
			}

			taskInterface.SetMaxProgress(copyItems.Count + formatItems.Count + patchItems.Count);
			ExecuteParallel(copyItems, false);
			ExecuteParallel(formatItems, false);

			try
			{
				CreateDirectory(Program.LogDir);
				logFile = new StreamWriter(Path.Combine(Program.LogDir, "patch.log"));
				ExecuteParallel(patchItems, false);
			}
			finally {
				logFile?.Close();
			}

			cutoff.Set(DateTime.Now);

			if (fuzzy > 0)
				taskInterface.Invoke(new Action(() => {
					var w = new ReviewWindow(results);
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
			var patchFile = PatchFile.FromText(File.ReadAllText(patchPath));
			//use srcFile because the base file is already copied and there may have been formatting applied
			var srcFile = Path.Combine(Program.baseDir, patchFile.patchedPath);
			var name = RelPath(FullSrcDir, srcFile);

			if (!File.Exists(srcFile))
			{
				Log("MISSING file " + patchFile.patchedPath + Environment.NewLine);
				failures++;
				return;
			}

			var unpatchedLines = File.ReadAllLines(srcFile);
			var patcher = new Patcher(patchFile.patches, unpatchedLines);
			patcher.Patch(mode);

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
			log.AppendLine($"exact: {exact},\toffset: {offset},\tfuzzy: {fuzzy},\tfailed: {failures}\t{name}");

			foreach (var res in patcher.Results)
				log.AppendLine(res.Summary());

			Log(log.ToString());

			var patchedFile = new PatchedFile {
				title = name,
				rootDir = Program.baseDir,
				patchFile = patchFile,
				patchFilePath = patchPath,
				original = unpatchedLines,
				patched = patcher.ResultLines,
				results = patcher.Results.ToList(),
			};
			
			results.Add(patchedFile);
		}

		private void Log(string text)
		{
			lock (logFile)
				logFile.Write(text);
		}
	}
}