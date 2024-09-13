using System.Collections.Concurrent;
using System.Text;
using DiffPatch;
using Microsoft.Extensions.DependencyInjection;
using Terraria.ModLoader.Setup.Core.Abstractions;
using Terraria.ModLoader.Setup.Core.Utilities;

namespace Terraria.ModLoader.Setup.Core
{
	public class PatchTask : SetupOperation
	{
		private static readonly string[] NonSourceDirs = ["bin", "obj", ".vs"];

		public static IEnumerable<(string file, string relPath)> EnumerateSrcFiles(string dir) =>
			EnumerateFiles(dir).Where(f => !f.relPath.Split('/').Any(NonSourceDirs.Contains));

		private readonly IUserPrompt userPrompt;
		private readonly IPatchReviewer? patchReviewer;
		private readonly ProgramSettings programSettings;
		private readonly PatchTaskParameters parameters;

		private int warnings;
		private int failures;
		private int fuzzy;
		private StreamWriter? logFile;
		private readonly SemaphoreSlim logSemaphore = new(1, 1);

		public PatchTask(PatchTaskParameters parameters, IServiceProvider serviceProvider)
		{
			userPrompt = serviceProvider.GetRequiredService<IUserPrompt>();
			patchReviewer = serviceProvider.GetService<IPatchReviewer>();
			programSettings = serviceProvider.GetRequiredService<ProgramSettings>();
			this.parameters = parameters with {
				BaseDir = PathUtils.WithUnixSeparators(parameters.BaseDir),
				PatchedDir = PathUtils.WithUnixSeparators(parameters.PatchedDir),
				PatchDir = PathUtils.WithUnixSeparators(parameters.PatchDir),
			};
		}

		public override bool StartupWarning()
		{
			if (programSettings.NoPrompts) {
				return true;
			}

			return userPrompt.Prompt(
				"Possible loss of data",
				"Any changes in /" + parameters.PatchedDir + " that have not been converted to patches will be lost.",
				PromptOptions.OKCancel,
				PromptSeverity.Warning);
		}

		public override async Task Run(IProgress progress, CancellationToken cancellationToken = default)
		{
			using var taskProgress = progress.StartTask($"Patching {parameters.PatchedDir}...");

			string removedFileList = Path.Combine(parameters.PatchDir, DiffTask.RemovedFileList);
			HashSet<string> noCopy = File.Exists(removedFileList)
				? [..File.ReadAllLines(removedFileList).Select(PathUtils.WithUnixSeparators)]
				: [];

			var items = new List<WorkItem>();
			var newFiles = new ConcurrentDictionary<string, object?>();
			ConcurrentBag<FilePatcher> results = [];

			foreach ((string file, string relPath) in EnumerateFiles(parameters.PatchDir)) {
				if (relPath.EndsWith(".patch")) {
					items.Add(new WorkItem("Patching: " + relPath, async ct => {
						FilePatcher filePatcher = await Patch(file, ct);
						results.Add(filePatcher);
						newFiles.TryAdd(PathUtils.WithUnixSeparators(filePatcher.PatchedPath), null);
					}));
					noCopy.Add(relPath.Substring(0, relPath.Length - 6));
				}
				else if (relPath != DiffTask.RemovedFileList) {
					string destination = PathUtils.UnixJoin(parameters.PatchedDir, relPath);

					items.Add(new WorkItem("Copying: " + relPath, () => Copy(file, destination)));
					newFiles.TryAdd(destination, null);
				}
			}

			foreach ((string file, string relPath) in EnumerateSrcFiles(parameters.BaseDir)) {
				if (!noCopy.Contains(relPath)) {
					string destination = PathUtils.UnixJoin(parameters.PatchedDir, relPath);

					items.Add(new WorkItem("Copying: " + relPath, () => Copy(file, destination)));
					newFiles.TryAdd(destination, null);
				}
			}

			try
			{
				CreateDirectory(ProgramSettings.LogsDir);
				logFile = new StreamWriter(Path.Combine(ProgramSettings.LogsDir, "patch.log"));

				await ExecuteParallel(items, taskProgress, cancellationToken: cancellationToken);
			}
			finally {
				logFile?.Close();
			}

			parameters.Cutoff.Set(DateTime.Now);

			//Remove files and directories that weren't in patches and original src.

			taskProgress.ReportStatus("Deleting Old Src Files");

			foreach ((var file, _) in EnumerateSrcFiles(parameters.PatchedDir))
				if (!newFiles.ContainsKey(file))
					File.Delete(file);

			taskProgress.ReportStatus("Deleting Old Src's Empty Directories");
			DeleteEmptyDirs(parameters.PatchedDir);

			taskProgress.ReportStatus("Old Src Removed");

			//Show patch reviewer if there were any fuzzy patches.

			if (fuzzy > 0 || programSettings.PatchMode == Patcher.Mode.FUZZY && failures > 0)
				patchReviewer?.Show(results, commonBasePath: parameters.BaseDir + '/');
		}

		public override bool Failed() => failures > 0;

		public override bool Warnings() => warnings > 0;

		public override void FinishedPrompt()
		{
			if ((patchReviewer != null && fuzzy > 0) || (!Failed() && !Warnings()))
				return;

			userPrompt.Inform(
				"Patch Results",
				$"Patches applied with {failures} failures and {warnings} warnings.\nSee /logs/patch.log for details",
				Failed() ? PromptSeverity.Error : PromptSeverity.Warning);
		}

		private async Task<FilePatcher> Patch(string patchPath, CancellationToken cancellationToken = default)
		{
			var patcher = FilePatcher.FromPatchFile(patchPath);
			patcher.Patch(programSettings.PatchMode);
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
			log.AppendLine(
				$"{patcher.patchFile.basePath},\texact: {exact},\toffset: {offset},\tfuzzy: {fuzzy},\tfailed: {failures}");

			foreach (var res in patcher.results)
				log.AppendLine(res.Summary());

			await logSemaphore.WaitAsync(cancellationToken);
			try {
				await logFile!.WriteAsync(log.ToString());
			}
			finally {
				logSemaphore.Release();
			}

			return patcher;
		}
	}
}