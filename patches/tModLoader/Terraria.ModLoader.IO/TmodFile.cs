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
	public class TmodFile : IEnumerable<KeyValuePair<string, byte[]>>
	{
		public enum LoadedState
		{
			None,
			Integrity,
			Info,
			Code,
			Assets,
			Streaming
		}

		public readonly string path;

		private LoadedState state;
		private IDictionary<string, byte[]> files = new Dictionary<string, byte[]>();

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
			files.TryGetValue(fileName.Replace('\\', '/'), out var data);
			return data;
		}

		/// <summary>
		/// Adds a (fileName -> content) entry to the compressed payload
		/// </summary>
		/// <param name="fileName">The internal filepath, will be slash sanitised automatically</param>
		/// <param name="data">The file content to add. WARNING, data is kept as a shallow copy, so modifications to the passed byte array will affect file content</param>
		internal void AddFile(string fileName, byte[] data)
		{
			files[fileName.Replace('\\', '/')] = data;
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
					foreach (var entry in files.OrderBy(e => GetFileState(e.Key)))
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

		internal delegate void ReadStreamingAsset(string path, int len, BinaryReader reader);
		internal void Read(LoadedState desiredState, ReadStreamingAsset streamingHandler = null)
		{
			if (desiredState <= state)
				return;
			
			using (var fileStream = File.OpenRead(path))
			using (var hReader = new BinaryReader(fileStream))
			{
				if (Encoding.ASCII.GetString(hReader.ReadBytes(4)) != "TMOD")
					throw new Exception("Magic Header != \"TMOD\"");

				tModLoaderVersion = new Version(hReader.ReadString());
				hash = hReader.ReadBytes(20);
				signature = hReader.ReadBytes(256);
				//currently unused, included to read the entire data-blob as a byte-array without decompressing or waiting to hit end of stream
				int datalen = hReader.ReadInt32();

				if (state < LoadedState.Integrity)
				{
					long pos = fileStream.Position;
					var verifyHash = SHA1.Create().ComputeHash(fileStream);
					if (!verifyHash.SequenceEqual(hash))
						throw new Exception(Language.GetTextValue("tModLoader.LoadErrorHashMismatchCorrupted"));

					state = LoadedState.Integrity;
					if (desiredState == LoadedState.Integrity)
						return;

					fileStream.Position = pos;
				}

				bool filesAreLoadOrdered = tModLoaderVersion >= new Version(0, 10, 1, 2);

				using (var deflateStream = new DeflateStream(fileStream, CompressionMode.Decompress))
				using (var reader = new BinaryReader(deflateStream))
				{
					name = reader.ReadString();
					version = new Version(reader.ReadString());

					int count = reader.ReadInt32();
					for (int i = 0; i < count; i++)
					{
						string fileName = reader.ReadString();
						LoadedState fileState = GetFileState(fileName);
						if (filesAreLoadOrdered && fileState > desiredState)
							break;

						int len = reader.ReadInt32();
						if (fileState == LoadedState.Streaming && desiredState >= LoadedState.Streaming)
						{
							var end = deflateStream.TotalOut + len;
							streamingHandler(fileName, len, reader);
							if (deflateStream.TotalOut < end)
								reader.ReadBytes((int) (end - deflateStream.TotalOut));
							else if (deflateStream.TotalOut > end)
								throw new IOException(
									$"Read too many bytes ({deflateStream.Position - end - len}>{len}) while loading streaming asset: {fileName}");
						}
						else
						{
							byte[] content = reader.ReadBytes(len);
							if (fileState > state && fileState <= desiredState)
								AddFile(fileName, content);
						}
					}
				}
			}
			
			if (desiredState >= LoadedState.Info && !HasFile("Info"))
				throw new Exception("Missing Info file");

			if (desiredState >= LoadedState.Code && !HasFile("All.dll") && !(HasFile("Windows.dll") && HasFile("Mono.dll")))
				throw new Exception("Missing All.dll or Windows.dll and Mono.dll");

			state = desiredState;
			if (state > LoadedState.Assets)
				state = LoadedState.Assets;
		}

		private static LoadedState GetFileState(string fileName)
		{
			if (fileName == "Info" || fileName == "icon.png")
				return LoadedState.Info;

			if (fileName.EndsWith(".dll") || fileName.EndsWith(".pdb"))
				return LoadedState.Code;

			if (fileName.EndsWith(".png") || fileName.EndsWith(".rawimg") ||
					fileName.EndsWith(".mp3") || fileName.EndsWith(".wav") ||
					fileName.EndsWith(".xnb") ||
					fileName.StartsWith("Streaming/"))
				return LoadedState.Streaming;

			return LoadedState.Assets;
		}

		internal void UnloadAssets()
		{
			files = files
				.Where(file => GetFileState(file.Key) < LoadedState.Assets)
				.ToDictionary(file => file.Key, file => file.Value);

			state = LoadedState.Code;
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
