using System;
using System.IO;

namespace Terraria.ModLoader.IO;

public static class BinaryIO
{
	[Obsolete("Use Write7BitEncodedInt", true)]
	public static void WriteVarInt(this BinaryWriter writer, int value) => writer.Write7BitEncodedInt(value);

	[Obsolete("Use Read7BitEncodedInt", true)]
	public static int ReadVarInt(this BinaryReader reader) => reader.Read7BitEncodedInt();

	public static BitsByte ReadBitsByte(this BinaryReader reader) => reader.ReadByte();

	/// <inheritdoc cref="ReadFlags(BinaryReader, out bool, out bool, out bool, out bool, out bool, out bool, out bool, out bool)"/>
	public static void ReadFlags(this BinaryReader reader, out bool b0)
	{
		b0 = false;
		((BitsByte)reader.ReadByte()).Retrieve(ref b0);
	}
	/// <inheritdoc cref="ReadFlags(BinaryReader, out bool, out bool, out bool, out bool, out bool, out bool, out bool, out bool)"/>
	public static void ReadFlags(this BinaryReader reader, out bool b0, out bool b1)
	{
		b0 = b1 = false;
		((BitsByte)reader.ReadByte()).Retrieve(ref b0, ref b1);
	}
	/// <inheritdoc cref="ReadFlags(BinaryReader, out bool, out bool, out bool, out bool, out bool, out bool, out bool, out bool)"/>
	public static void ReadFlags(this BinaryReader reader, out bool b0, out bool b1, out bool b2)
	{
		b0 = b1 = b2 = false;
		((BitsByte)reader.ReadByte()).Retrieve(ref b0, ref b1, ref b2);
	}
	/// <inheritdoc cref="ReadFlags(BinaryReader, out bool, out bool, out bool, out bool, out bool, out bool, out bool, out bool)"/>
	public static void ReadFlags(this BinaryReader reader, out bool b0, out bool b1, out bool b2, out bool b3)
	{
		b0 = b1 = b2 = b3 = false;
		((BitsByte)reader.ReadByte()).Retrieve(ref b0, ref b1, ref b2, ref b3);
	}
	/// <inheritdoc cref="ReadFlags(BinaryReader, out bool, out bool, out bool, out bool, out bool, out bool, out bool, out bool)"/>
	public static void ReadFlags(this BinaryReader reader, out bool b0, out bool b1, out bool b2, out bool b3, out bool b4)
	{
		b0 = b1 = b2 = b3 = b4 = false;
		((BitsByte)reader.ReadByte()).Retrieve(ref b0, ref b1, ref b2, ref b3, ref b4);
	}
	/// <inheritdoc cref="ReadFlags(BinaryReader, out bool, out bool, out bool, out bool, out bool, out bool, out bool, out bool)"/>
	public static void ReadFlags(this BinaryReader reader, out bool b0, out bool b1, out bool b2, out bool b3, out bool b4, out bool b5)
	{
		b0 = b1 = b2 = b3 = b4 = b5 = false;
		((BitsByte)reader.ReadByte()).Retrieve(ref b0, ref b1, ref b2, ref b3, ref b4, ref b5);
	}
	/// <inheritdoc cref="ReadFlags(BinaryReader, out bool, out bool, out bool, out bool, out bool, out bool, out bool, out bool)"/>
	public static void ReadFlags(this BinaryReader reader, out bool b0, out bool b1, out bool b2, out bool b3, out bool b4, out bool b5, out bool b6)
	{
		b0 = b1 = b2 = b3 = b4 = b5 = b6 = false;
		((BitsByte)reader.ReadByte()).Retrieve(ref b0, ref b1, ref b2, ref b3, ref b4, ref b5, ref b6);
	}
	/// <summary>
	/// Reads up to 8 <see langword="bool"/>s sent as a single <see langword="byte"/> using <c>BinaryReader.WriteFlags</c>. This is more efficient than using <see cref="BinaryReader.ReadBoolean"/>.
	/// </summary>
	public static void ReadFlags(this BinaryReader reader, out bool b0, out bool b1, out bool b2, out bool b3, out bool b4, out bool b5, out bool b6, out bool b7)
	{
		b0 = b1 = b2 = b3 = b4 = b5 = b6 = b7 = false;
		((BitsByte)reader.ReadByte()).Retrieve(ref b0, ref b1, ref b2, ref b3, ref b4, ref b5, ref b6, ref b7);
	}

	/// <summary>
	/// Efficiently writes up to 8 <see langword="bool"/>s as a single <see langword="byte"/>. To read, use <c>BinaryReader.ReadFlags</c>. This is more efficient than using <see cref="BinaryWriter.Write(bool)"/>.
	/// </summary>
	public static void WriteFlags(this BinaryWriter writer, bool b1 = false, bool b2 = false, bool b3 = false, bool b4 = false, bool b5 = false, bool b6 = false, bool b7 = false, bool b8 = false) => writer.Write(new BitsByte(b1, b2, b3, b4, b5, b6, b7, b8));

	/// <inheritdoc cref=" WriteFlags(BinaryWriter, bool, bool, bool, bool, bool, bool, bool, bool)"/>
	public static void Write(this BinaryWriter writer, bool b1 = false, bool b2 = false, bool b3 = false, bool b4 = false, bool b5 = false, bool b6 = false, bool b7 = false, bool b8 = false) => writer.Write(new BitsByte(b1, b2, b3, b4, b5, b6, b7, b8));

	// TODO: public static void Write(this BinaryWriter writer, params bool[] booleans)
	// TODO: public static bool[] ReadBooleans(this BinaryReader reader)

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
