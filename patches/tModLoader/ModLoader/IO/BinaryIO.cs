using System;
using System.IO;

namespace Terraria.ModLoader.IO
{
	public static class BinaryIO
	{
		public static void WriteItem(this BinaryWriter writer, Item item, bool readStack = false, bool readFavorite = false) =>
			ItemIO.Send(item, writer, readStack, readFavorite);

		public static Item ReadItem(this BinaryReader reader, bool readStack = false, bool readFavorite = false) =>
			ItemIO.Receive(reader, readStack, readFavorite);

		//copied from BinaryWriter.Read7BitEncodedInt
		public static void WriteVarInt(this BinaryWriter writer, int value) {
			// Write out an int 7 bits at a time.  The high bit of the byte,
			// when on, tells reader to continue reading more bytes.
			uint v = (uint)value;   // support negative numbers
			while (v >= 0x80) {
				writer.Write((byte)(v | 0x80));
				v >>= 7;
			}
			writer.Write((byte)v);
		}

		//copied from BinaryReader.Read7BitEncodedInt
		public static int ReadVarInt(this BinaryReader reader) {
			// Read out an Int32 7 bits at a time.  The high bit
			// of the byte when on means to continue reading more bytes.
			int count = 0;
			int shift = 0;
			byte b;
			do {
				// Check for a corrupted stream.  Read a max of 5 bytes.
				if (shift == 5 * 7)  // 5 bytes max per Int32, shift += 7
					throw new FormatException("variable length int with more than 32 bits");

				// ReadByte handles end of stream cases for us.
				b = reader.ReadByte();
				count |= (b & 0x7F) << shift;
				shift += 7;
			} while ((b & 0x80) != 0);
			return count;
		}

		public static void SafeWrite(this BinaryWriter writer, Action<BinaryWriter> write) {
			var ms = new MemoryStream();//memory thrash should be fine here
			write(new BinaryWriter(ms));
			writer.WriteVarInt((int)ms.Length);
			ms.Position = 0;
			ms.CopyTo(writer.BaseStream);
		}

		public static void SafeRead(this BinaryReader reader, Action<BinaryReader> read) {
			int length = reader.ReadVarInt();
			var ms = new MemoryStream(reader.ReadBytes(length));
			read(new BinaryReader(ms));
			if (ms.Position != length)
				throw new IOException("Read underflow " + ms.Position + " of " + length + " bytes");
		}

		public static void ReadBytes(this Stream stream, byte[] buf) {
			int r, pos = 0;
			while ((r = stream.Read(buf, pos, buf.Length - pos)) > 0)
				pos += r;

			if (pos != buf.Length)
				throw new IOException($"Stream did not contain enough bytes ({pos}) < ({buf.Length})");
		}

		public static byte[] ReadBytes(this Stream stream, int len) => ReadBytes(stream, (long)len);

		public static byte[] ReadBytes(this Stream stream, long len) {
			var buf = new byte[len];
			stream.ReadBytes(buf);
			return buf;
		}
	}
}
