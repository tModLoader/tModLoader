using System;
using System.Collections.Generic;
using System.IO;
using Ionic.Zlib;

namespace Installer
{
	public class ZipFile
	{
		public readonly string name;
		private IDictionary<string, byte[]> files = new Dictionary<string, byte[]>();

		public ZipFile(string name)
		{
			this.name = name;
		}

		public byte[] this[string file]
		{
			get
			{
				return this.files[file];
			}
			set
			{
				this.files[file] = value;
			}
		}

		public bool HasFile(string file)
		{
			return this.files.ContainsKey(file);
		}

		public void Write()
		{
			this.Write(null, default(DoWorkArgs), default(ProgressChangedArgs));
		}

		internal void Write(Task task, DoWorkArgs progress, ProgressChangedArgs args)
		{
			using (FileStream fileStream = File.Create(this.name))
			{
				using (DeflateStream compress = new DeflateStream(fileStream, CompressionMode.Compress))
				{
					using (BinaryWriter writer = new BinaryWriter(compress))
					{
						writer.Write((byte)this.files.Count);
						foreach (string file in this.files.Keys)
						{
							if (task != null)
							{
								args.message = "Saving " + file + "...";
								task.ReportProgress(progress.background, -1, args);
							}
							writer.Write(file);
							writer.Write(this.files[file].Length);
							writer.Write(this.files[file]);
						}
						if (task != null)
						{
							args.message = "Done";
							task.ReportProgress(progress.background, -1, args);
						}
					}
				}
			}
		}

		public static ZipFile Read(string name)
		{
			return Read(name, null, default(DoWorkArgs), default(ProgressChangedArgs));
		}

		internal static ZipFile Read(string name, Task task, DoWorkArgs progress, ProgressChangedArgs args)
		{
			ZipFile zip = new ZipFile(name);
			using (FileStream fileStream = File.OpenRead(name))
			{
				using (DeflateStream decompress = new DeflateStream(fileStream, CompressionMode.Decompress))
				{
					using (BinaryReader reader = new BinaryReader(decompress))
					{
						int count = reader.ReadByte();
						for (int k = 0; k < count; k++)
						{
							string fileName = reader.ReadString();
							if (task != null)
							{
								args.message = "Reading " + fileName + "...";
								task.ReportProgress(progress.background, -1, args);
							}
							byte[] buffer = reader.ReadBytes(reader.ReadInt32());
							zip[fileName] = buffer;
						}
					}
				}
			}
			return zip;
		}
	}
}
