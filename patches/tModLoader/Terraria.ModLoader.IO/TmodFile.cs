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

		public bool HasFile(string fileName)
		{
			return files.ContainsKey(fileName.Replace('\\', '/'));
		}

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

		public IEnumerator<KeyValuePair<string, byte[]>> GetEnumerator()
		{
			return files.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		internal void Save()
		{
			var dataStream = new MemoryStream();
			using (var writer = new BinaryWriter(new DeflateStream(dataStream, CompressionMode.Compress)))
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

			using (var writer = new BinaryWriter(File.Create(path)))
			{
				writer.Write(Encoding.ASCII.GetBytes("TMOD"));
				writer.Write(ModLoader.version.ToString());
				writer.Write(hash);
				writer.Write(signature);
				writer.Write(data.Length);
				writer.Write(data);
			}
		}

		internal void Read()
		{
			try
			{
				byte[] data;
				using (var reader = new BinaryReader(File.OpenRead(path)))
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

				using (var reader = new BinaryReader(new DeflateStream(new MemoryStream(data), CompressionMode.Decompress)))
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

		public byte[] GetMainAssembly(bool windows = ModLoader.windows)
		{
			return HasFile("All.dll") ? GetFile("All.dll") : windows ? GetFile("Windows.dll") : GetFile("Mono.dll");
		}

		public byte[] GetMainPDB(bool windows = ModLoader.windows)
		{
			return HasFile("All.pdb") ? GetFile("All.pdb") : windows ? GetFile("Windows.pdb") : GetFile("Mono.pdb");
		}
	}
}
