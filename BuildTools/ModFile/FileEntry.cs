namespace tModPackager.ModFile;

public class FileEntry
{
	public string Name { get; }

	// from the start of the file
	public int Offset { get; internal set; }
	public int Length { get; }
	public int CompressedLength { get; }

	// intended to be readonly, but unfortunately no ReadOnlySpan on .NET 4.5
	internal byte[] cachedBytes;

	internal FileEntry(string name, int offset, int length, int compressedLength, byte[] cachedBytes)
	{
		Name = name;
		Offset = offset;
		Length = length;
		CompressedLength = compressedLength;
		this.cachedBytes = cachedBytes;
	}
}