using System;
using System.Collections.Generic;
using System.IO;
using Ionic.Zlib;

namespace Terraria.ModLoader.IO
{
	internal class TmodFile
	{
		private IList<string> fileNames = new List<string>();
		private IDictionary<string, byte[]> files = new Dictionary<string, byte[]>();
		public readonly string Name;

		public bool InvalidFile
		{
			get;
			internal set;
		}

		internal TmodFile(string name)
		{
			this.Name = name;
			this.InvalidFile = false;
		}

		internal bool HasFile(string fileName)
		{
			return files.ContainsKey(fileName);
		}

		internal void AddFile(string fileName, byte[] data)
		{
			if (!HasFile(fileName))
			{
				fileNames.Add(fileName);
			}
			byte[] dataCopy = new byte[data.Length];
			data.CopyTo(dataCopy, 0);
			files[fileName] = dataCopy;
		}

		internal void RemoveFile(string fileName)
		{
			if (HasFile(fileName))
			{
				fileNames.Remove(fileName);
				files.Remove(fileName);
			}
		}

		internal byte[] GetFile(string fileName)
		{
			if (HasFile(fileName))
			{
				return files[fileName];
			}
			return null;
		}

		internal void Save()
		{
			using (FileStream fileStream = File.Create(Name))
			{
				using (DeflateStream compress = new DeflateStream(fileStream, CompressionMode.Compress))
				{
					using (BinaryWriter writer = new BinaryWriter(compress))
					{
						writer.Write((byte)fileNames.Count);
						foreach (string fileName in fileNames)
						{
							writer.Write(fileName);
							byte[] data = files[fileName];
							writer.Write(data.Length);
							writer.Write(data);
						}
					}
				}
			}
		}

		internal void Read()
		{
			try
			{
				using (FileStream fileStream = File.OpenRead(Name))
				{
					using (DeflateStream decompress = new DeflateStream(fileStream, CompressionMode.Decompress))
					{
						using (BinaryReader reader = new BinaryReader(decompress))
						{
							int count = reader.ReadByte();
							for (int k = 0; k < count; k++)
							{
								AddFile(reader.ReadString(), reader.ReadBytes(reader.ReadInt32()));
							}
						}
					}
				}
			}
			catch
			{
				InvalidFile = true;
			}
		}

		internal bool ValidMod()
		{
			return !InvalidFile && HasFile("Resources") && ((HasFile("Windows") && HasFile("Other")) || HasFile("All"));
		}
	}
}
