using System.Diagnostics;
using System.Text;

namespace Terraria.ModLoader.Setup.Core;

public static class RunCmd
{
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

		while (!process.HasExited) {
			if (cancel.IsCancellationRequested) {
				process.Kill();
				throw new OperationCanceledException(cancel);
			}
			process.WaitForExit(100);

			output?.Invoke(process.StandardOutput.ReadToEnd());
			error?.Invoke(process.StandardError.ReadToEnd());
		}

		return process.ExitCode;
	}
}