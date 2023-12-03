using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
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

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
			WindowsImplementation();
		}
		else {
			UnixImplementation();
		}
	}

	private void WindowsImplementation()
	{
		// Robocopy is always present.
		ExecuteAndWait(
			"robocopy", @$"""{Source}"" ""{Destination}"" /MIR",
			useShellExecute: true,
			isErrorPredicate: static i => i >= 8
		);
	}

	private static string? rsyncPath;

	private void UnixImplementation()
	{
		// Rsync is not always present, and may need to be installed by the user.
		rsyncPath ??= ExecuteAndWait("command", $"-v rsync", useShellExecute: false, redirectStdOutput: true)
			.StandardOutput
			.ReadToEnd()
			.Trim();

		if (!string.IsNullOrEmpty(rsyncPath)) {
			ExecuteAndWait(rsyncPath, @$"-a --delete ""{Source}"" ""{Destination}""", useShellExecute: false);
		}
		else {
			Log.LogMessage(MessageImportance.High, "\trsync was not found, using a slow fallback...");

			// Is there a better fallback, aside for manually writing enumerations & hashchecks in C#?
			ExecuteAndWait("rm", @$"-rf ""{Destination}""");
			ExecuteAndWait("cp", @$"-R ""{Source}"" ""{Destination}""");
		}
	}

	private Process ExecuteAndWait(string command, string args, bool useShellExecute = true, bool redirectStdOutput = false, Predicate<int>? isErrorPredicate = null)
	{
		var process = Process.Start(new ProcessStartInfo(command, args) {
			UseShellExecute = useShellExecute,
			RedirectStandardOutput = redirectStdOutput,
			WindowStyle = ProcessWindowStyle.Hidden,
		});

		process.WaitForExit();

		bool processErrored = (isErrorPredicate != null)
			? isErrorPredicate.Invoke(process.ExitCode)
			: (process.ExitCode != 0);

		if (processErrored) {
			Log.LogError($"Command '{command}{(!string.IsNullOrEmpty(args) ? $" {args}" : null)}' exited with code {process.ExitCode}.");
		}

		return process;
	}
}
