using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ionic.Zlib;

namespace Terraria.ModLoader.IO
{
	public class TmodFile : IEnumerable<KeyValuePair<string, byte[]>>
	{
		public readonly string path;
		private readonly IDictionary<string, byte[]> files = new Dictionary<string, byte[]>();

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

		private Exception readException;

		internal TmodFile(string path)
		{
			this.path = path;
		}

		public bool HasFile(string fileName) => files.ContainsKey(fileName.Replace('\\', '/'));

		public byte[] GetFile(string fileName)
		{
			byte[] data;
			files.TryGetValue(fileName.Replace('\\', '/'), out data);
			return data;
		}

		internal void AddFile(string fileName, byte[] data)
		{
			byte[] dataCopy = new byte[data.Length];
			data.CopyTo(dataCopy, 0);
			files[fileName.Replace('\\', '/')] = dataCopy;
		}

		internal void RemoveFile(string fileName)
		{
			files.Remove(fileName.Replace('\\', '/'));
		}

		public IEnumerator<KeyValuePair<string, byte[]>> GetEnumerator() => files.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		public int FileCount => files.Count;

		internal void Save()
		{
			using (var dataStream = new MemoryStream())
			{
				using (var writerStream = new DeflateStream(dataStream, CompressionMode.Compress))
				using (var writer = new BinaryWriter(writerStream))
				{
					writer.Write(name);
					writer.Write(version.ToString());

					writer.Write(files.Count);
					foreach (var entry in files)
					{
						writer.Write(entry.Key);
						writer.Write(entry.Value.Length);
						writer.Write(entry.Value);
					}
				}
				var data = dataStream.ToArray();
				hash = SHA1.Create().ComputeHash(data);

				using (var fileStream = File.Create(path))
				using (var fileWriter = new BinaryWriter(fileStream))
				{
					fileWriter.Write(Encoding.ASCII.GetBytes("TMOD"));
					fileWriter.Write(ModLoader.version.ToString());
					fileWriter.Write(hash);
					fileWriter.Write(signature);
					fileWriter.Write(data.Length);
					fileWriter.Write(data);
				}
			}
		}

		internal void Read()
		{
			try
			{
				byte[] data;
				using (var fileStream = File.OpenRead(path))
				using (var reader = new BinaryReader(fileStream))
				{
					if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "TMOD")
						throw new Exception("Magic Header != \"TMOD\"");

					tModLoaderVersion = new Version(reader.ReadString());
					hash = reader.ReadBytes(20);
					signature = reader.ReadBytes(256);
					data = reader.ReadBytes(reader.ReadInt32());
					var verifyHash = SHA1.Create().ComputeHash(data);
					if (!verifyHash.SequenceEqual(hash))
						throw new Exception("Hash mismatch, data blob has been modified or corrupted");
				}

				using (var memoryStream = new MemoryStream(data))
				using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress))
				using (var reader = new BinaryReader(deflateStream))
				{
					name = reader.ReadString();
					version = new Version(reader.ReadString());

					int count = reader.ReadInt32();
					for (int i = 0; i < count; i++)
						AddFile(reader.ReadString(), reader.ReadBytes(reader.ReadInt32()));
				}
			}
			catch (Exception e)
			{
				readException = e;
			}
		}

		internal Exception ValidMod()
		{
			if (readException != null)
				return readException;

			if (!HasFile("Info"))
				return new Exception("Missing Info file");

			if (!HasFile("All.dll") && !(HasFile("Windows.dll") && HasFile("Mono.dll")))
				return new Exception("Missing All.dll or Windows.dll and Mono.dll");

			return null;
		}

		public byte[] GetMainAssembly(bool? windows = null)
		{
			bool isWindows = windows.GetValueOrDefault(ModLoader.windows);
			return HasFile("All.dll") ? GetFile("All.dll") : isWindows ? GetFile("Windows.dll") : GetFile("Mono.dll");
		}

		public byte[] GetMainPDB(bool? windows = null)
		{
			bool isWindows = windows.GetValueOrDefault(ModLoader.windows);
			return HasFile("All.pdb") ? GetFile("All.pdb") : isWindows ? GetFile("Windows.pdb") : GetFile("Mono.pdb");
		}
	}
}
