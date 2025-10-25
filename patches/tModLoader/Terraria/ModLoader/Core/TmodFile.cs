using Ionic.Zlib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.UI;

namespace Terraria.ModLoader.Core;

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
			return stream.ReadBytes(entry.Length);
	}

	public List<string> GetFileNames() => files.Keys.ToList();

	public byte[] GetBytes(string fileName) => files.TryGetValue(Sanitize(fileName), out var entry) ? GetBytes(entry) : null;

	public Stream GetStream(FileEntry entry, bool newFileStream = false)
	{
		Stream stream;
		if (entry.cachedBytes != null) {
			stream = entry.cachedBytes.ToMemoryStream();
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

	/// <summary>
	/// Adds a (fileName -> content) entry to the compressed payload
	/// This method is not threadsafe with reads, but is threadsafe with multiple concurrent AddFile calls
	/// </summary>
	/// <param name="fileName">The internal filepath, will be slash sanitised automatically</param>
	/// <param name="data">The file content to add. WARNING, data is kept as a shallow copy, so modifications to the passed byte array will affect file content</param>
	internal void AddFile(string fileName, byte[] data)
	{
		fileName = Sanitize(fileName);
		int size = data.Length;

		if (size > MIN_COMPRESS_SIZE && ShouldCompress(fileName)) {
			using (var ms = new MemoryStream(data.Length)) {
				using (var ds = new DeflateStream(ms, CompressionMode.Compress))
					ds.Write(data, 0, data.Length);

				var compressed = ms.ToArray();
				if (compressed.Length < size * COMPRESSION_TRADEOFF)
					data = compressed;
			}
		}

		lock (files)
			files[fileName] = new FileEntry(fileName, -1, size, data.Length, data);

		fileTable = null;
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

	internal void Save()
	{
		if (fileStream != null)
			throw new IOException($"File already open: {path}");

		// write the general TMOD header and data blob
		// TMOD ascii identifier
		// tModLoader version
		// hash
		// signature
		// data length
		// signed data
		Directory.CreateDirectory(Path.GetDirectoryName(path));
		using (fileStream = File.Create(path))
		using (var writer = new BinaryWriter(fileStream)) {
			writer.Write(Encoding.ASCII.GetBytes("TMOD"));
			writer.Write((TModLoaderVersion = BuildInfo.tMLVersion).ToString());

			int hashPos = (int)fileStream.Position;
			writer.Write(new byte[20 + 256 + 4]); //hash, sig, data length

			int dataPos = (int)fileStream.Position;
			writer.Write(Name);
			writer.Write(Version.ToString());

			// write file table
			// file count
			// file-entries:
			//   filename
			//   uncompressed file size
			//   compressed file size (stored size)
			fileTable = files.Values.ToArray();
			writer.Write(fileTable.Length);

			foreach (var f in fileTable) {
				if (f.CompressedLength != f.cachedBytes.Length)
					throw new Exception($"CompressedLength ({f.CompressedLength}) != cachedBytes.Length ({f.cachedBytes.Length}): {f.Name}");

				writer.Write(f.Name);
				writer.Write(f.Length);
				writer.Write(f.CompressedLength);
			}

			// write compressed files and update offsets
			int offset = (int)fileStream.Position; // offset starts at end of file table
			foreach (var f in fileTable) {
				writer.Write(f.cachedBytes);

				f.Offset = offset;
				offset += f.CompressedLength;
			}

			// update hash
			fileStream.Position = dataPos;
			Hash = SHA1.Create().ComputeHash(fileStream);

			fileStream.Position = hashPos;
			writer.Write(Hash);

			//skip signature
			fileStream.Seek(256, SeekOrigin.Current);

			// write data length
			writer.Write((int)(fileStream.Length - dataPos));
		}
		fileStream = null;
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
			throw new Exception(Language.GetTextValue("tModLoader.LoadErrorHashMismatchCorrupted"));

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
				throw new Exception(Language.GetTextValue("tModLoader.LoadErrorHashMismatchCorrupted"), e);

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

			f.cachedBytes = fileStream.ReadBytes(f.CompressedLength);
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
		Interface.loadMods.SubProgressText = $"Upgrading: {Path.GetFileName(path)}";
		Logging.tML.InfoFormat("Upgrading: {0}", Path.GetFileName(path));

		using (var deflateStream = new DeflateStream(fileStream, CompressionMode.Decompress, true))
		using (var reader = new BinaryReader(deflateStream)) {
			Name = reader.ReadString();
			Version = new Version(reader.ReadString());

			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
				AddFile(reader.ReadString(), reader.ReadBytes(reader.ReadInt32()));
		}

		// update buildVersion
		var info = BuildProperties.ReadModFile(this);
		info.buildVersion = TModLoaderVersion;
		// TODO should be turn this into .info? Generally files starting with . are ignored, at least on Windows (and are much harder to accidentally delete or even manually create)
		AddFile("Info", info.ToBytes());

		// make a backup
		fileStream.Seek(0, SeekOrigin.Begin);
		var backupFolder = Path.Combine(Path.GetDirectoryName(path), "UpgradeBackup");
		Directory.CreateDirectory(backupFolder);
		using (var backupStream = File.OpenWrite(Path.Combine(backupFolder, Path.GetFileName(path))))
			fileStream.CopyTo(backupStream);

		// close stream before upgrade
		Close();
		// write to the new format (also updates the file offset table)
		Save();
		// clear all the file contents from AddFile
		ResetCache();
		// Save closes the file so re-open it
		Open();
		// Read contract fulfilled
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
}
