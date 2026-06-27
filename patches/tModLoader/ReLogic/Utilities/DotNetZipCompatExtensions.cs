namespace System.IO.Compression;

/// <summary>
/// Extension methods for <see cref="ZipArchive"/> and <see cref="ZipArchiveEntry"/> that provide
/// API compatibility with Ionic.Zip (DotNetZip), reducing diffs against vanilla
/// </summary>
internal static class DotNetZipCompatExtensions
{
	public static bool ContainsEntry(this ZipArchive zip, string entryName) => zip.GetEntry(entryName) != null;

	public static void Extract(this ZipArchiveEntry entry, Stream stream)
	{
		using var entryStream = entry.Open();
		entryStream.CopyTo(stream);
	}

	public static bool IsDirectory(this ZipArchiveEntry entry) => entry.FullName.EndsWith('/');
}
