using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Terraria.ModLoader.IO;

public static class BinaryIO
{
	[Obsolete("Use Write7BitEncodedInt", true)]
	public static void WriteVarInt(this BinaryWriter writer, int value) => writer.Write7BitEncodedInt(value);

	[Obsolete("Use Read7BitEncodedInt", true)]
	public static int ReadVarInt(this BinaryReader reader) => reader.Read7BitEncodedInt();

	public static BitsByte ReadBitsByte(this BinaryReader reader) => reader.ReadByte();

	public static void ReadBits(this BinaryReader reader, ref bool b0) => ((BitsByte)reader.ReadByte()).Retrieve(ref b0);
	public static void ReadBits(this BinaryReader reader, ref bool b0, ref bool b1) => ((BitsByte)reader.ReadByte()).Retrieve(ref b0, ref b1);
	public static void ReadBits(this BinaryReader reader, ref bool b0, ref bool b1, ref bool b2) => ((BitsByte)reader.ReadByte()).Retrieve(ref b0, ref b1, ref b2);
	public static void ReadBits(this BinaryReader reader, ref bool b0, ref bool b1, ref bool b2, ref bool b3) => ((BitsByte)reader.ReadByte()).Retrieve(ref b0, ref b1, ref b2, ref b3);
	public static void ReadBits(this BinaryReader reader, ref bool b0, ref bool b1, ref bool b2, ref bool b3, ref bool b4) => ((BitsByte)reader.ReadByte()).Retrieve(ref b0, ref b1, ref b2, ref b3, ref b4);
	public static void ReadBits(this BinaryReader reader, ref bool b0, ref bool b1, ref bool b2, ref bool b3, ref bool b4, ref bool b5) => ((BitsByte)reader.ReadByte()).Retrieve(ref b0, ref b1, ref b2, ref b3, ref b4, ref b5);
	public static void ReadBits(this BinaryReader reader, ref bool b0, ref bool b1, ref bool b2, ref bool b3, ref bool b4, ref bool b5, ref bool b6) => ((BitsByte)reader.ReadByte()).Retrieve(ref b0, ref b1, ref b2, ref b3, ref b4, ref b5, ref b6);
	public static void ReadBits(this BinaryReader reader, ref bool b0, ref bool b1, ref bool b2, ref bool b3, ref bool b4, ref bool b5, ref bool b6, ref bool b7) => ((BitsByte)reader.ReadByte()).Retrieve(ref b0, ref b1, ref b2, ref b3, ref b4, ref b5, ref b6, ref b7);

	/// <summary>
	/// Efficiently writes up to 8 bools as a single byte.
	/// </summary>
	public static void Write(this BinaryWriter writer, bool b1 = false, bool b2 = false, bool b3 = false, bool b4 = false, bool b5 = false, bool b6 = false, bool b7 = false, bool b8 = false) => writer.Write(new BitsByte(b1, b2, b3, b4, b5, b6, b7, b8));

	public static void SafeWrite(this BinaryWriter writer, Action<BinaryWriter> write)
	{
		var ms = new MemoryStream();//memory thrash should be fine here
		write(new BinaryWriter(ms));
		writer.Write7BitEncodedInt((int)ms.Length);
		ms.Position = 0;
		ms.CopyTo(writer.BaseStream);
	}

	public static void SafeRead(this BinaryReader reader, Action<BinaryReader> read)
	{
		int length = reader.Read7BitEncodedInt();
		var ms = reader.ReadBytes(length).ToMemoryStream();
		read(new BinaryReader(ms));
		if (ms.Position != length)
			throw new IOException("Read underflow " + ms.Position + " of " + length + " bytes");
	}

	public static void ReadBytes(this Stream stream, byte[] buf)
	{
		int r, pos = 0;
		while ((r = stream.Read(buf, pos, buf.Length - pos)) > 0)
			pos += r;

		if (pos != buf.Length)
			throw new IOException($"Stream did not contain enough bytes ({pos}) < ({buf.Length})");
	}

	public static byte[] ReadBytes(this Stream stream, int len) => ReadBytes(stream, (long)len);

	public static byte[] ReadBytes(this Stream stream, long len)
	{
		var buf = new byte[len];
		stream.ReadBytes(buf);
		return buf;
	}

	public static MemoryStream ToMemoryStream(this byte[] bytes, bool writeable = false)
	{
		return new MemoryStream(bytes, 0, bytes.Length, writeable, publiclyVisible: true);
	}

	public static ReadOnlySpan<byte> ReadByteSpan(this Stream stream, int len)
	{
		if (stream is MemoryStream ms && ms.TryGetBuffer(out var buf)) {
			var span = buf.AsSpan().Slice((int)ms.Position, len);
			ms.Seek(len, SeekOrigin.Current);
			return span;
		}

		// consider using a [ThreadStatic] buffer for small reads
		return ReadBytes(stream, len);
	}
}
