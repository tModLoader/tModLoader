using System;
using System.Buffers;
using System.IO;
using System.Numerics;

namespace tModLoader.BuildTasks;

internal static class IOUtils
{
	/// <summary>
	/// netstandard2.0 lacks Path.GetRelativePath. <br/>
	/// This is a very simple replacement that assumes that 'path' derives from 'relativeTo'.
	/// </summary>
	public static string SubstringToRelativePath(string path, string relativeTo)
	{
		if (!path.StartsWith(relativeTo)) {
			throw new ArgumentException($"Path '{path}' must derive from '{relativeTo}'.");
		}

		bool endsWithSeparator = relativeTo[relativeTo.Length - 1] is '/' or '\\';

		return path.Substring(relativeTo.Length + (endsWithSeparator ? 0 : 1));
	}

	/// <summary>
	/// Returns true if both files exist and have the same length and write time. Does not check file contents.
	/// </summary>
	public static bool AreFilesSeeminglyTheSame(FileInfo fileA, FileInfo fileB)
	{
		return fileA.Exists && fileB.Exists
			&& fileA.Length == fileB.Length
			&& fileA.LastWriteTimeUtc == fileB.LastWriteTimeUtc;
	}
}
