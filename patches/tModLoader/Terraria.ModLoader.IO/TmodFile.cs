using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ionic.Zlib;
using Terraria.Localization;

namespace Terraria.ModLoader.IO
{
	// warning class is not threadsafe
	public class TmodFile : IEnumerable<string>
	{
		private class FileEntry
		{
			public string name;
			public int offset; // from the start of the file
			public int size;
			public int compressedSize;
			public byte[] cachedBytes;
		}

		public const uint MIN_COMPRESS_SIZE = 1<<10;//1KB
		public const uint MAX_CACHE_SIZE = 1<<17;//128KB

		private static string Sanitize(string path) => path.Replace('\\', '/');

		public readonly string path;
		
		private FileStream fileStream;
		private IDictionary<string, FileEntry> files = new Dictionary<string, FileEntry>();
		private FileEntry[] fileTable;

		public Version tModLoaderVersion
		{
			get; private set;
		}

		public string name { get; internal set; }

		public Version version
		{
			get; internal set;
		}

		public byte[] hash
		{
			get; private set;
		}

		internal byte[] signature { get; private set; } = new byte[256];

		private bool? validModBrowserSignature;
		internal bool ValidModBrowserSignature
		{
			get
			{
				if (!validModBrowserSignature.HasValue)
					validModBrowserSignature = ModLoader.IsSignedBy(this, ModLoader.modBrowserPublicKey);

				return validModBrowserSignature.Value;
			}
		}

		internal TmodFile(string path)
		{
			this.path = path;
		}

		public bool HasFile(string fileName) => files.ContainsKey(Sanitize(fileName));

		[Obsolete("Use GetStream or GetBytes instead", true)]
		public byte[] GetFile(string fileName) => GetBytes(fileName);

		public byte[] GetBytes(string fileName)
		{
			if (!files.TryGetValue(Sanitize(fileName), out var entry))
				return null;

			if (entry.cachedBytes != null && entry.compressedSize == entry.size)
				return entry.cachedBytes;
			
			using (var stream = GetStream(entry))
				return stream.ReadBytes(entry.size);
		}

		private EntryReadStream lastEntryReadStream;
		private Stream GetStream(FileEntry entry)
		{
			Stream stream;
			if (entry.cachedBytes != null) 
			{
				stream = new MemoryStream(entry.cachedBytes);
			}
			else 
			{
				if (fileStream == null)
					throw new IOException("File not open: "+path);
				if (lastEntryReadStream != null && !lastEntryReadStream.IsDisposed)
					throw new IOException($"Previous entry read stream not closed: {lastEntryReadStream.name}");

				stream = lastEntryReadStream = new EntryReadStream(fileStream, entry.offset, entry.compressedSize, entry.name);
			}

			if (entry.compressedSize != entry.size)
				stream = new DeflateStream(stream, CompressionMode.Decompress);

			return stream;
		}

		public Stream GetStream(string fileName)
		{
			if (!files.TryGetValue(Sanitize(fileName), out var entry))
				throw new KeyNotFoundException(fileName);

			return GetStream(entry);
		}

		/// <summary>
		/// Adds a (fileName -> content) entry to the compressed payload
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
					if (compressed.Length < size)
						data = compressed;
				}
			}

			files[fileName] = new FileEntry {
				name = fileName,
				size = size,
				compressedSize = data.Length,
				cachedBytes = data,
			};

			fileTable = null;
		}

		internal void RemoveFile(string fileName) {
			files.Remove(Sanitize(fileName));
			fileTable = null;
		}

		public delegate void EntryIterator(string name, int len, Func<Stream> getStream);
		public void ForEach(EntryIterator iterator)
		{
			foreach (var f in fileTable)
				iterator(f.name, f.size, () => GetStream(f));
		}

		public int Count => fileTable.Length;
		public IEnumerator<string> GetEnumerator() => fileTable?.Select(s => s.name)?.GetEnumerator() ?? files.Keys.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		internal void Save()
		{
			// invalidate the read handle
			Close();

			// write the general TMOD header and data blob
			// TMOD ascii identifier
			// tModLoader version
			// hash
			// signature
			// data length
			// signed data
			using (fileStream = File.Create(path))
			using (var writer = new BinaryWriter(fileStream))
			{
				writer.Write(Encoding.ASCII.GetBytes("TMOD"));
				writer.Write((tModLoaderVersion = ModLoader.version).ToString());
				
				int hashPos = (int) fileStream.Position;
				writer.Write(new byte[20+256+4]); //hash, sig, data length

				int dataPos = (int) fileStream.Position;
				writer.Write(name);
				writer.Write(version.ToString());
				
				// write file table
				// file count
				// file-entries:
				//   filename
				//   uncompressed file size
				//   compressed file size (stored size)
				fileTable = files.Values.ToArray();
				writer.Write(fileTable.Length);

				foreach (var f in fileTable)
				{
					if (f.compressedSize != f.cachedBytes.Length)
						throw new Exception($"compressedSize ({f.compressedSize}) != cachedBytes.Length ({f.cachedBytes.Length}): {f.name}");
					
					writer.Write(f.name);
					writer.Write(f.size);
					writer.Write(f.compressedSize);
				}
				
				// write compressed files and update offsets
				int offset = (int)fileStream.Position; // offset starts at end of file table
				foreach (var f in fileTable) {
					writer.Write(f.cachedBytes);
					
					f.offset = offset;
					offset += f.compressedSize;
				}

				// update hash
				fileStream.Position = dataPos;
				hash = SHA1.Create().ComputeHash(fileStream);

				fileStream.Position = hashPos;
				writer.Write(hash);
				
				//skip signature
				fileStream.Seek(256, SeekOrigin.Current);
				
				// write data length
				writer.Write((int)(fileStream.Length - dataPos));
			}
			fileStream = null;
		}

		private static bool ShouldCompress(string fileName) => !fileName.EndsWith(".png");

		internal void Read()
		{
			if (fileStream != null)
				throw new Exception("File has already been read");
			
			fileStream = File.OpenRead(path);
			var reader = new BinaryReader(fileStream); //intentionally not disposed to leave the stream open. In .NET 4.5+ the 3-arg constructor could be used
			
			// read header info
			if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "TMOD")
				throw new Exception("Magic Header != \"TMOD\"");

			tModLoaderVersion = new Version(reader.ReadString());
			hash = reader.ReadBytes(20);
			signature = reader.ReadBytes(256);
			//currently unused, included to read the entire data-blob as a byte-array without decompressing or waiting to hit end of stream
			int datalen = reader.ReadInt32();

			// verify integrity
			long pos = fileStream.Position;
			var verifyHash = SHA1.Create().ComputeHash(fileStream);
			if (!verifyHash.SequenceEqual(hash))
				throw new Exception(Language.GetTextValue("tModLoader.LoadErrorHashMismatchCorrupted"));

			fileStream.Position = pos;

			if (tModLoaderVersion < new Version(0, 11)) {
				Upgrade();
				return;
			}
				
			// read hashed/signed mod info
			name = reader.ReadString();
			version = new Version(reader.ReadString());
				
			// read file table
			int offset = 0;
			fileTable = new FileEntry[reader.ReadInt32()];
			for (int i = 0; i < fileTable.Length; i++)
			{
				var f = new FileEntry {
					name = reader.ReadString(),
					size = reader.ReadInt32(),
					compressedSize = reader.ReadInt32(),
					offset = offset
				};
				fileTable[i] = f;
				files[f.name] = f;

				offset += f.compressedSize;
			}

			int fileStartPos = (int)fileStream.Position;
			foreach (var f in fileTable)
				f.offset += fileStartPos;
		}

		public void CacheFiles(ISet<string> skip = null)
		{
			fileStream.Seek(fileTable[0].offset, SeekOrigin.Begin);
			foreach (var f in fileTable)
			{
				if (f.compressedSize > MAX_CACHE_SIZE || (skip?.Contains(f.name) ?? false)) {
					fileStream.Seek(f.compressedSize, SeekOrigin.Current);
					continue;
				}
				
				f.cachedBytes = fileStream.ReadBytes(f.compressedSize);
			}
		}

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

		private class DisposeWrapper : IDisposable
		{
			private Action dispose;
			public DisposeWrapper(Action dispose)
			{
				this.dispose = dispose;
			}
			public void Dispose() => dispose?.Invoke();
		}
		public IDisposable EnsureOpen()
		{
			if (fileStream != null)
				return new DisposeWrapper(null);

			fileStream = File.OpenRead(path);
			return new DisposeWrapper(Close);
		}

		public void Close()
		{
			fileStream?.Close();
			fileStream = null;
		}
		
		// load Code and Info files into the cache
		// unfortunately this will recompress files individually which were previously decompressed, but that'll have to do
		private void Upgrade()
		{
			Interface.loadMods.SubProgressText = $"Upgrading: {Path.GetFileName(path)}";
			Logging.tML.InfoFormat("Upgrading: {0}", Path.GetFileName(path));

			using (var deflateStream = new DeflateStream(fileStream, CompressionMode.Decompress))
			using (var reader = new BinaryReader(deflateStream))
			{
				name = reader.ReadString();
				version = new Version(reader.ReadString());

				int count = reader.ReadInt32();
				for (int i = 0; i < count; i++)
					AddFile(reader.ReadString(), reader.ReadBytes(reader.ReadInt32()));
			}

			// update buildVersion
			var info = BuildProperties.ReadModFile(this);
			info.buildVersion = tModLoaderVersion;
			AddFile("Info", info.ToBytes());
			
			// write to the new format (also updates the file offset table)
			Save();
			// clear all the file contents from AddFile
			ResetCache();
			// Save closes the file so re-open it
			fileStream = File.OpenRead(path);
			// Read contract fulfilled
		}

		public void VerifyCoreFiles()
		{
			if (!HasFile("Info"))
				throw new Exception("Missing Info file");

			if (!HasFile("All.dll") && !(HasFile("Windows.dll") && HasFile("Mono.dll")))
				throw new Exception("Missing All.dll or Windows.dll and Mono.dll");
		}

		public byte[] GetMainAssembly(bool? windows = null)
		{
			bool isWindows = windows.GetValueOrDefault(ModLoader.windows);
			return HasFile("All.dll") ? GetBytes("All.dll") : isWindows ? GetBytes("Windows.dll") : GetBytes("Mono.dll");
		}

		public byte[] GetMainPDB(bool? windows = null)
		{
			bool isWindows = windows.GetValueOrDefault(ModLoader.windows);
			return HasFile("All.pdb") ? GetBytes("All.pdb") : isWindows ? GetBytes("Windows.pdb") : GetBytes("Mono.pdb");
		}
	}
}
