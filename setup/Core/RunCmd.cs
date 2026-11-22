using System.Diagnostics;
using System.Text;

namespace Terraria.ModLoader.Setup.Core;

public static class RunCmd
{
	/// <summary>
	/// Runs a process to completion. <paramref name="output"/> and <paramref name="error"/> are each invoked
	/// once, with everything the process wrote, so a caller can assign rather than append.
	/// </summary>
	public static int Run(
		string dir,
		string cmd,
		string args,
		Action<string>? output = null,
		Action<string>? error = null,
		string? input = null,
		CancellationToken cancel = default)
	{
		using var process = new Process();
		process.StartInfo = new ProcessStartInfo {
			FileName = cmd,
			Arguments = args,
			WorkingDirectory = dir,
			UseShellExecute = false,
			RedirectStandardInput = input != null,
			CreateNoWindow = true,
		};

		if (output != null) {
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
		}

		if (error != null) {
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
		}

		if (!process.Start())
			throw new Exception($"Failed to start process: \"{cmd} {args}\"");

		if (input != null) {
			var w = new StreamWriter(process.StandardInput.BaseStream, new UTF8Encoding(false));
			w.Write(input);
			w.Close();
		}

		// Reading only ends when a stream is closed, so it is done on the thread pool. Waiting here instead
		// would block until the process finished, ignoring cancellation, and would deadlock a process which
		// filled the error pipe while the output pipe was still open.
		Task<string>? outputTask = output != null ? process.StandardOutput.ReadToEndAsync(cancel) : null;
		Task<string>? errorTask = error != null ? process.StandardError.ReadToEndAsync(cancel) : null;

		while (!process.HasExited) {
			if (cancel.IsCancellationRequested) {
				process.Kill(entireProcessTree: true);
				throw new OperationCanceledException(cancel);
			}

			process.WaitForExit(100);
		}

		output?.Invoke(outputTask!.Result);
		error?.Invoke(errorTask!.Result);

		return process.ExitCode;
	}
}