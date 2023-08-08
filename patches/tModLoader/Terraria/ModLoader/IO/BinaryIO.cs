using System;
using System.IO;

namespace Terraria.ModLoader.IO;

public static class BinaryIO
{
	[Obsolete("Use Write7BitEncodedInt", true)]
	public static void WriteVarInt(this BinaryWriter writer, int value) => writer.Write7BitEncodedInt(value);

	[Obsolete("Use Read7BitEncodedInt", true)]
	public static int ReadVarInt(this BinaryReader reader) => reader.Read7BitEncodedInt();

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
