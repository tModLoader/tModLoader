using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.NRefactory.CSharp;
using Terraria.ModLoader.Properties;

namespace Terraria.ModLoader.Setup
{
	public class PatchTask : Task
	{
		private const bool NewEngine = true;

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

		public class PatchResults
		{
			public readonly string relPath;
			public readonly string patchPath;
			public readonly string srcPath;
			public readonly string basePath;
			public readonly string[] beforeLines;
			public readonly string[] afterLines;
			public List<Patcher.Result> results;

			public int exact, offset, fuzzy, failures, warnings;

			public PatchResults(string relPath, string patchPath, string srcPath, string basePath, 
				string[] beforeLines, string[] afterLines, IEnumerable<Patcher.Result> results) {
				this.relPath = relPath;
				this.patchPath = patchPath;
				this.srcPath = srcPath;
				this.basePath = basePath;
				this.beforeLines = beforeLines;
				this.afterLines = afterLines;
				this.results = results.ToList();

				foreach (var result in this.results) {
					if (!result.success) {
						failures++;
						continue;
					}

					if (ErrorLevel(result) > 0)
						warnings++;

					if (result.mode == Patcher.Mode.EXACT) exact++;
					else if (result.mode == Patcher.Mode.OFFSET) offset++;
					else if(result.mode == Patcher.Mode.FUZZY) fuzzy++;
				}
			}

			public string Log() {
				var log = new StringBuilder();
				log.AppendLine($"exact: {exact},\toffset: {offset},\tfuzzy: {fuzzy},\tfailed: {failures}\t{relPath}");

				foreach (var res in results)
					log.AppendLine(Summary(res));

				return log.ToString();
			}

			public string Summary(Patcher.Result r) {
				if (!r.success)
					return $"FAILURE: {r.patch.Header}";

				if (r.mode == Patcher.Mode.OFFSET)
					return (r.offsetWarning ? "WARNING" : "OFFSET") + $": {r.patch.Header} offset {r.offset} lines";

				if (r.mode == Patcher.Mode.FUZZY) {
					int q = (int)(r.fuzzyQuality * 100);
					return $"FUZZY: {r.patch.Header} quality {q}%" +
						(r.offset > 0 ? $" offset {r.offset} lines" : "");
				}

				return $"EXACT: {r.patch.Header}";
			}

			// 0 = exact/offset
			// 1 = good quality fuzzy
			// 2 = warning
			// 3 = bad quality fuzzy
			// 4 = failure
			public int ErrorLevel(Patcher.Result result) {
				if (!result.success)
					return 4;
				if (result.mode == Patcher.Mode.FUZZY && result.fuzzyQuality < 0.5f)
					return 3;
				if (result.offsetWarning || result.mode == Patcher.Mode.FUZZY && result.fuzzyQuality < 0.85f)
					return 2;
				if (result.mode == Patcher.Mode.FUZZY)
					return 1;
				return 0;
			}

			public int ErrorLevel() => results.Select(ErrorLevel).Max();
		}

		private ConcurrentBag<PatchResults> results = new ConcurrentBag<PatchResults>();

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

			foreach (var file in patchFiles)
			{
				var relPath = RelPath(FullPatchDir, file);
				if (relPath.EndsWith(".patch"))
					patchItems.Add(new WorkItem("Patching: " + relPath, () => Patch(relPath)));
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
				taskInterface.Invoke(new Action(() => new PatchResultsForm(results).ShowDialog()));
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
		private void Patch(string relPatchPath)
		{
			var relPath = relPatchPath.Remove(relPatchPath.Length - 6);
			if (!File.Exists(Path.Combine(FullSrcDir, relPath)))
			{
				Log("MISSING file " + Path.Combine(srcDir, relPath) + Environment.NewLine);
				failures++;
				return;
			}

			if (NewEngine) {
				NewPatch(relPath);
				return;
			}

			var patchText = File.ReadAllText(Path.Combine(FullPatchDir, relPatchPath));
			patchText = PreparePatch(patchText);

			CallPatch(patchText, Path.Combine(srcDir, relPath));

			//just a copy of the original if the patch wasn't perfect, delete it, we still have it
			var fileName = Path.GetFileName(relPath);
			var fuzzFile = Path.Combine(FullSrcDir, Path.GetDirectoryName(relPath),
				fileName.Substring(0, Math.Min(fileName.Length, 13)) + "~");
			if (File.Exists(fuzzFile))
				File.Delete(fuzzFile);
		}

		//generates destination hunk offsets and enforces windows line endings
		private static string PreparePatch(string patchText) {
			var lines = patchText.Split('\n');
			int delta = 0;
			for (int i = 0; i < lines.Length; i++) {
				var line = lines[i].TrimEnd();
				if (line.StartsWith("@@")) {
					var m = DiffTask.HunkOffsetRegex.Match(lines[i]);
					var hunkOffset = int.Parse(m.Groups[1].Value) + delta;
					delta += int.Parse(m.Groups[4].Value) - int.Parse(m.Groups[2].Value);
					line = m.Result($"@@ -$1,$2 +{hunkOffset},$4 @@");
				}
				lines[i] = line;
			}
			
			return string.Join(Environment.NewLine, lines);
		}

		private void Log(string text)
		{
			lock (logFile)
			{
				logFile.Write(text);
			}
		}

		private void CallPatch(string patchText, string srcFile)
		{
			var output = new StringBuilder();
			var error = new StringBuilder();
			var log = new StringBuilder();
			Program.RunCmd(Program.toolsDir, Path.Combine(Program.toolsDir, "applydiff.exe"),
				$"-u -N -p0 -d {Program.baseDir} {srcFile}",
				s => { output.Append(s); lock(log) log.Append(s); },
				s => { error.Append(s); lock(log) log.Append(s); },
				patchText
			);

			Log(log.ToString());

			if (error.Length > 0)
				throw new Exception(error.ToString());

			foreach (var line in output.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None))
			{
				if (line.StartsWith("Hunk"))
				{
					if (line.Contains("FAILED")) failures++;
					else if (line.Contains("fuzz")) warnings++;
				}
			}
		}

		public static PatchResults NewPatch(string relPath, string basePath, string srcPath, string patchPath, Patcher.Mode mode) {
			var baseLines = File.ReadAllText(basePath).Split('\n').Select(l => l.TrimEnd('\r')).ToArray();
			var patcher = new Patcher(File.ReadAllText(patchPath), baseLines);
			patcher.Apply(mode);
			var resultLines = patcher.ResultLines();
			File.WriteAllText(srcPath, string.Join(Environment.NewLine, resultLines));

			return new PatchResults(relPath, patchPath, srcPath, basePath, baseLines, resultLines, patcher.Results());
		}

		private void NewPatch(string relPath) {
			var r = NewPatch(relPath, 
				Path.Combine(FullBaseDir, relPath), 
				Path.Combine(FullSrcDir, relPath), 
				Path.Combine(FullPatchDir, relPath + ".patch"), 
				mode);

			results.Add(r);
			Log(r.Log());

			fuzzy += r.fuzzy;
			warnings += r.warnings;
			failures += r.failures;
		}
	}
}