using System;

namespace tModPorter;

public record ProgressUpdate
{
	public record Warning(string Message) : ProgressUpdate() {
		public override string ToString() => $"Warning: ${Message}";
	}

	public record Error(string Filename, Exception Exception) : ProgressUpdate()
	{
		public override string ToString() => $"{Filename}: {Exception}";
	}

	public record ProjectLoading(string Operation, TimeSpan ElapsedTime, string PathAndFramework) : ProgressUpdate() {
		public override string ToString() => $"{Operation,-15} {ElapsedTime,-15:m\\:ss\\.fff} {PathAndFramework}";
	}

	/// <summary>
	/// May be triggered multiple times for the same file across passes.
	/// </summary>
	public record FileUpdated(string Filename) : ProgressUpdate() {
		public override string ToString() => $"Updated: {Filename}";
	}

	public record Progress(int Pass, int CurrentFile, int FileCount) : ProgressUpdate();

	public record Complete(int PassCount, int FilesChanged, int FileCount, TimeSpan ElapsedTime) : ProgressUpdate() {
		public override string ToString() => $"Complete! Changed {FilesChanged}/{FileCount} files in {ElapsedTime:m\\:ss}, {PassCount} passes";
	}
}
