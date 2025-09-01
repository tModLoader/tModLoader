using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ModLoaderSimple;

// warning class is not threadsafe
public class TmodFile : IEnumerable<TmodFile.FileEntry>
{
	public class FileEntry
	{
		public string Name { get; }

		// from the start of the file
		public int Offset { get; internal set; }
		public int Length { get; }
		public int CompressedLength { get; }

		// intended to be readonly, but unfortunately no ReadOnlySpan on .NET 4.5
		internal byte[] cachedBytes;

		internal FileEntry(string name, int offset, int length, int compressedLength, byte[] cachedBytes = null)
		{
			Name = name;
			Offset = offset;
			Length = length;
			CompressedLength = compressedLength;
			this.cachedBytes = cachedBytes;
		}

		public bool IsCompressed => Length != CompressedLength;
	}

	public const uint MIN_COMPRESS_SIZE = 1 << 10;//1KB
	public const uint MAX_CACHE_SIZE = 1 << 17;//128KB
	public const float COMPRESSION_TRADEOFF = 0.9f;

	private static string Sanitize(string path) => path.Replace('\\', '/');

	public readonly string path;

	private FileStream fileStream;
	private IDictionary<string, FileEntry> files = new Dictionary<string, FileEntry>();
	private FileEntry[] fileTable;

	private int openCounter;
	private EntryReadStream sharedEntryReadStream;
	private List<EntryReadStream> independentEntryReadStreams = new List<EntryReadStream>();

	public Version TModLoaderVersion { get; private set; }

	public string Name { get; private set; }

	public Version Version { get; private set; }

	public byte[] Hash { get; private set; }

	internal byte[] Signature { get; private set; } = new byte[256];

	// Starting position of the hashable part of the stream.
	private long hashStartPos;
	private bool? hashVerified;

	internal TmodFile(string path, string name = null, Version version = null)
	{
		this.path = path;
		this.Name = name;
		this.Version = version;
	}

	public bool HasFile(string fileName) => files.ContainsKey(Sanitize(fileName));

	public byte[] GetBytes(FileEntry entry)
	{
		if (entry.cachedBytes != null && !entry.IsCompressed)
			return entry.cachedBytes;

		using (var stream = GetStream(entry))
			return ReadBytes(stream, entry.Length);
	}

	public List<string> GetFileNames() => files.Keys.ToList();

	public byte[] GetBytes(string fileName) => files.TryGetValue(Sanitize(fileName), out var entry) ? GetBytes(entry) : null;

	public Stream GetStream(FileEntry entry, bool newFileStream = false)
	{
		Stream stream;
		if (entry.cachedBytes != null) {
			stream = GetMemoryStream(entry.cachedBytes);
		}
		else if (fileStream == null) {
			throw new IOException($"File not open: {path}");
		}
		else if (newFileStream) {
			var ers = new EntryReadStream(this, entry, File.OpenRead(path), false);
			lock (independentEntryReadStreams) { // todo, make this a set? maybe?
				independentEntryReadStreams.Add(ers);
			}
			stream = ers;
		}
		else if (sharedEntryReadStream != null) {
			throw new IOException($"Previous entry read stream not closed: {sharedEntryReadStream.Name}");
		}
		else {
			stream = sharedEntryReadStream = new EntryReadStream(this, entry, fileStream, true);
		}

		if (entry.IsCompressed)
			stream = new DeflateStream(stream, CompressionMode.Decompress);

		return stream;
	}

	internal void OnStreamClosed(EntryReadStream stream)
	{
		if (stream == sharedEntryReadStream) {
			sharedEntryReadStream = null;
		}
		else {
			lock (independentEntryReadStreams) {
				if (!independentEntryReadStreams.Remove(stream))
					throw new IOException($"Closed EntryReadStream not associated with this file. {stream.Name} @ {path}");
			}
		}
	}

	public Stream GetStream(string fileName, bool newFileStream = false)
	{
		if (!files.TryGetValue(Sanitize(fileName), out var entry))
			throw new KeyNotFoundException(fileName);

		return GetStream(entry, newFileStream);
	}

	internal void RemoveFile(string fileName)
	{
		files.Remove(Sanitize(fileName));
		fileTable = null;
	}

	public int Count => fileTable.Length;
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public IEnumerator<FileEntry> GetEnumerator()
	{
		foreach (var entry in fileTable)
			yield return entry;
	}
	
	private class DisposeWrapper : IDisposable
	{
		private readonly Action dispose;
		public DisposeWrapper(Action dispose)
		{
			this.dispose = dispose;
		}

		public void Dispose() => dispose?.Invoke();
	}

	public IDisposable Open()
	{
		if (openCounter++ == 0) {
			if (fileStream != null)
				throw new Exception($"File already opened? {path}");

			try {
				if (Name == null)
					Read();
				else
					Reopen();
			}
			catch {
				try { Close(); } catch {}
				throw;
			}
		}

		return new DisposeWrapper(Close);
	}

	private void Close()
	{
		if (openCounter == 0)
			return;

		if (--openCounter == 0) {
			if (sharedEntryReadStream != null)
				throw new IOException($"Previous entry read stream not closed: {sharedEntryReadStream.Name}");
			if (independentEntryReadStreams.Count != 0)
				throw new IOException($"Shared entry read streams not closed: {string.Join(", ", independentEntryReadStreams.Select(e => e.Name))}");

			fileStream?.Close();
			fileStream = null;
		}
	}

	public bool IsOpen => fileStream != null;

	// Ignore file extensions which don't compress well under deflate to improve build time
	private static bool ShouldCompress(string fileName) =>
		!fileName.EndsWith(".png") &&
		!fileName.EndsWith(".mp3") &&
		!fileName.EndsWith(".ogg");

	private void Read()
	{
		fileStream = File.OpenRead(path);
		var reader = new BinaryReader(fileStream); //intentionally not disposed to leave the stream open. In .NET 4.5+ the 3-arg constructor could be used

		// read header info
		if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "TMOD")
			throw new Exception("Magic Header != \"TMOD\"");

		TModLoaderVersion = new Version(reader.ReadString());
		Hash = reader.ReadBytes(20);
		Signature = reader.ReadBytes(256);
		//currently unused, included to read the entire data-blob as a byte-array without decompressing or waiting to hit end of stream
		int datalen = reader.ReadInt32();

		// Verification.  We postpone hash verification until mod loading or an error
		// occurs during reading.
		hashStartPos = fileStream.Position;
		if (datalen != fileStream.Length - hashStartPos)
			throw new Exception("tModLoader.LoadErrorHashMismatchCorrupted");

		try {
			if (TModLoaderVersion < new Version(0, 11)) {
				Upgrade();
				return;
			}

			// read hashed/signed mod info
			Name = reader.ReadString();
			Version = new Version(reader.ReadString());

			// read file table
			int offset = 0;
			fileTable = new FileEntry[reader.ReadInt32()];
			for (int i = 0; i < fileTable.Length; i++) {
				var f = new FileEntry(
					reader.ReadString(),
					offset,
					reader.ReadInt32(),
					reader.ReadInt32());
				fileTable[i] = f;
				files[f.Name] = f;

				offset += f.CompressedLength;
			}

			int fileStartPos = (int)fileStream.Position;
			foreach (var f in fileTable)
				f.Offset += fileStartPos;
		}
		catch (Exception e) {
			if (!VerifyHash())
				throw new Exception("tModLoader.LoadErrorHashMismatchCorrupted", e);

			// If the hash is fine, let it bubble up like normal.
			throw;
		}
	}

	private void Reopen()
	{
		fileStream = File.OpenRead(path);
		var reader = new BinaryReader(fileStream); //intentionally not disposed to leave the stream open. In .NET 4.5+ the 3-arg constructor could be used

		// read header info
		if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "TMOD")
			throw new Exception("Magic Header != \"TMOD\"");

		reader.ReadString(); //tModLoader version
		if (!reader.ReadBytes(20).SequenceEqual(Hash))
			throw new Exception($"File has been modified, hash. {path}");

		// could also check name and version but hash should suffice
	}

	public void CacheFiles(ISet<string> skip = null)
	{
		fileStream.Seek(fileTable[0].Offset, SeekOrigin.Begin);
		foreach (var f in fileTable) {
			if (f.CompressedLength > MAX_CACHE_SIZE || (skip?.Contains(f.Name) ?? false)) {
				fileStream.Seek(f.CompressedLength, SeekOrigin.Current);
				continue;
			}

			f.cachedBytes = ReadBytes(fileStream, f.CompressedLength);
		}
	}

	// TODO never used
	public void RemoveFromCache(IEnumerable<string> fileNames)
	{
		foreach (var fileName in fileNames)
			files[fileName].cachedBytes = null;
	}

	public void ResetCache()
	{
		foreach (var f in fileTable)
			f.cachedBytes = null;
	}

	private void Upgrade()
	{
		// contents deleted
	}

	internal bool VerifyHash() => hashVerified ??= _VerifyHash();

	private bool _VerifyHash()
	{
		if (hashStartPos == 0)
			return false;

		using var fs = File.OpenRead(path);
		fs.Position = hashStartPos;
		return Hash.SequenceEqual(SHA1.Create().ComputeHash(fs));
	}

	/////////////////////// ADDED FOR COMPATIBILITY, ORIGINALLY FROM BinaryIO.cs

	private static MemoryStream GetMemoryStream(byte[] bytes, bool writeable = false)
	{
		return new MemoryStream(bytes, 0, bytes.Length, writeable, publiclyVisible: true);
	}

	private static byte[] ReadBytes(Stream stream, long len)
	{
		var buf = new byte[len];
		ReadBytes(stream, buf);
		return buf;
	}

	private static void ReadBytes(Stream stream, byte[] buf)
	{
		int r, pos = 0;
		while ((r = stream.Read(buf, pos, buf.Length - pos)) > 0)
			pos += r;

		if (pos != buf.Length)
			throw new IOException($"Stream did not contain enough bytes ({pos}) < ({buf.Length})");
	}
}
