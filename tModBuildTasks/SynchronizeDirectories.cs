using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Framework;

namespace tModLoader.BuildTasks;

/// <summary>
/// Copies contents from one folder to another, additionally removing entries in the destination that aren't present in the source.
/// </summary>
public class SynchronizeDirectories : TaskBase
{
	[Required]
	public string Source { get; set; } = string.Empty;

	[Required]
	public string Destination { get; set; } = string.Empty;

	protected override void Run()
	{
		Source = Path.GetFullPath(Source);
		Destination = Path.GetFullPath(Destination);

		var source = new DirectoryInfo(Source);
		var destination = new DirectoryInfo(Destination);

		if (!source.Exists) {
			Log.LogError($"Source directory '{Source}' doesn't exist!");
			return;
		}

		destination.Create();

		// Delete files that shouldn't be there.
		Parallel.ForEach(destination.EnumerateFiles("*", SearchOption.AllDirectories), file => {
			string relativePath = IOUtils.SubstringToRelativePath(file.FullName, Destination);
			string sourcePath = Path.Combine(Source, relativePath);

			if (!File.Exists(sourcePath)) {
				try {
					RunIOActionWIthRetries(file.Delete);
				}
				catch (FileNotFoundException) { }
			}
		});

		// Delete directories that shouldn't be there.
		Parallel.ForEach(destination.EnumerateDirectories("*", SearchOption.AllDirectories), directory => {
			string relativePath = IOUtils.SubstringToRelativePath(directory.FullName, Destination);
			string sourcePath = Path.Combine(Source, relativePath);

			if (!Directory.Exists(sourcePath)) {
				try {
					RunIOActionWIthRetries(() => directory.Delete(recursive: true));
				}
				catch (DirectoryNotFoundException) { }
			}
		});

		// Copy files that should be there.
		Parallel.ForEach(source.EnumerateFiles("*", SearchOption.AllDirectories), sourceFile => {
			string relativePath = IOUtils.SubstringToRelativePath(sourceFile.FullName, Source);
			var destinationFile = new FileInfo(Path.Combine(Destination, relativePath));

			if (!IOUtils.AreFilesSeeminglyTheSame(sourceFile, destinationFile)) {
				RunIOActionWIthRetries(() => {
					Directory.CreateDirectory(destinationFile.DirectoryName);
					sourceFile.CopyTo(destinationFile.FullName, overwrite: true);
				});
			}
		});
	}

	// Runs an action with a fixed amount of retries for the case of IOExceptions occuring from third-party short-lived file locks.
	private void RunIOActionWIthRetries(Action action)
	{
		const int MaxAttempts = 5;
		const int InitialDelayMs = 100;
		const int PerStepDelayMultiplier = 2;

		int delay = InitialDelayMs;

		for (int attempt = 1; ; attempt++) {
			try {
				action();
				break;
			}
			catch (IOException e) when (e is not DirectoryNotFoundException or FileNotFoundException) {
				if (attempt <= MaxAttempts) {
					Thread.Sleep(delay);
					delay *= PerStepDelayMultiplier;
					continue;
				}

				throw new IOException($"Failed to synchronize '{Destination}' due to {e.GetType().Name}: {e.Message}", e);
			}
		}
	}
}
