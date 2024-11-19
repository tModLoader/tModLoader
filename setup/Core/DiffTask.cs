using DiffPatch;
using Terraria.ModLoader.Setup.Core.Abstractions;
using Terraria.ModLoader.Setup.Core.Utilities;

namespace Terraria.ModLoader.Setup.Core
{
	public sealed class DiffTask : SetupOperation
	{
		public const string RemovedFileList = "removed_files.list";

		private static readonly string[] Extensions = [
			".cs", ".csproj", ".ico", ".resx", ".png", "App.config", ".json", ".targets", ".txt", ".bat", ".sh",
		];

		private readonly DiffTaskParameters parameters;

		public DiffTask(DiffTaskParameters parameters)
		{
			this.parameters = parameters with {
				BaseDir = PathUtils.WithUnixSeparators(parameters.BaseDir),
				PatchedDir = PathUtils.WithUnixSeparators(parameters.PatchedDir),
				PatchDir = PathUtils.WithUnixSeparators(parameters.PatchDir),
			};
		}

		private static bool IsDiffable(string relPath) => Extensions.Any(relPath.EndsWith);

		public override async Task Run(IProgress progress, CancellationToken cancellationToken = default)
		{
			using var taskProgress = progress.StartTask($"Generating patches for {parameters.PatchedDir}...");
			var items = new List<WorkItem>();

			foreach ((string file, string relPath) in PatchTask.EnumerateSrcFiles(parameters.PatchedDir))
			{
				if (File.GetLastWriteTime(file) < parameters.Cutoff.Get())
					continue;

				if (!File.Exists(Path.Combine(parameters.BaseDir, relPath)))
					items.Add(new WorkItem("Copying: " + relPath, () => Copy(file, Path.Combine(parameters.PatchDir, relPath))));
				else if (IsDiffable(relPath))
					items.Add(new WorkItem("Diffing: " + relPath, () => Diff(relPath)));
			}

			await ExecuteParallel(items, taskProgress, cancellationToken: cancellationToken);

			taskProgress.ReportStatus("Deleting Unnecessary Patches");
			foreach ((string file, string relPath) in EnumerateFiles(parameters.PatchDir)) {
				var targetPath = relPath.EndsWith(".patch") ? relPath.Substring(0, relPath.Length - 6) : relPath;
				if (!File.Exists(Path.Combine(parameters.PatchedDir, targetPath)))
					DeleteFile(file);
			}

			DeleteEmptyDirs(parameters.PatchDir);

			taskProgress.ReportStatus("Noting Removed Files");
			var removedFiles = PatchTask.EnumerateSrcFiles(parameters.BaseDir)
				.Select(f => f.relPath)
				.Where(path => !File.Exists(Path.Combine(parameters.PatchedDir, path)))
				.Order()
				.ToArray();

			string removedFileList = Path.Combine(parameters.PatchDir, RemovedFileList);
			if (removedFiles.Length > 0)
				await File.WriteAllLinesAsync(removedFileList, removedFiles, cancellationToken);
			else
				DeleteFile(removedFileList);

			parameters.Cutoff.Set(DateTime.Now);
		}

		private void Diff(string relPath)
		{
			var patchFile = Differ.DiffFiles(new LineMatchedDiffer(),
				PathUtils.UnixJoin(parameters.BaseDir, relPath),
				PathUtils.UnixJoin(parameters.PatchedDir, relPath));

			string patchPath = Path.Combine(parameters.PatchDir, relPath + ".patch");
			if (!patchFile.IsEmpty) {
				CreateParentDirectory(patchPath);
				File.WriteAllText(patchPath, patchFile.ToString(true));
			}
			else {
				DeleteFile(patchPath);
			}
		}
	}
}
