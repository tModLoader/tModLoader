using Mono.Cecil.Cil;
using System.IO;

namespace Terraria.ModLoader.Core
{
	internal static class CecilDebugHeaderFile
	{
		public static ImageDebugHeader Read(BinaryReader reader) {
			var entries = new ImageDebugHeaderEntry[reader.ReadByte()];
			for (int i = 0; i < entries.Length; ++i) {
				var directory = new ImageDebugDirectory {
					Characteristics = reader.ReadInt32(),
					TimeDateStamp = reader.ReadInt32(),
					MajorVersion = reader.ReadInt16(),
					MinorVersion = reader.ReadInt16(),
					Type = (ImageDebugType)reader.ReadInt32(),
					SizeOfData = reader.ReadInt32()
				};
				entries[i] = new ImageDebugHeaderEntry(directory, reader.ReadBytes(directory.SizeOfData));
			}
			return new ImageDebugHeader(entries);
		}

		public static void Write(BinaryWriter writer, ImageDebugHeader header) {
			writer.Write((byte)header.Entries.Length);
			foreach (var entry in header.Entries) {
				var directory = entry.Directory;
				writer.Write(directory.Characteristics);
				writer.Write(directory.TimeDateStamp);
				writer.Write(directory.MajorVersion);
				writer.Write(directory.MinorVersion);
				writer.Write((int)directory.Type);
				writer.Write(directory.SizeOfData);
				writer.Write(entry.Data);
			}
		}

		public static byte[] GetBytes(this ImageDebugHeader header) {
			using (var ms = new MemoryStream()) {
				Write(new BinaryWriter(ms), header);
				return ms.ToArray();
			}
		}

		public static ImageDebugHeader Read(byte[] bytes) {
			using (var ms = new MemoryStream(bytes))
				return Read(new BinaryReader(ms));
		}
	}
}
