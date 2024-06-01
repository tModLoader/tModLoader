using System;
using System.Buffers.Binary;
using System.IO;

namespace Terraria.ModLoader.IO;

public class BigEndianWriter : BinaryWriter
{
	public BigEndianWriter(Stream output) : base(output) { }

	public override void Write(short value) { Span<byte> buf = stackalloc byte[2]; BinaryPrimitives.WriteInt16BigEndian(buf, value); OutStream.Write(buf); }
	public override void Write(ushort value) { Span<byte> buf = stackalloc byte[2]; BinaryPrimitives.WriteUInt16BigEndian(buf, value); OutStream.Write(buf); }
	public override void Write(int value) { Span<byte> buf = stackalloc byte[4]; BinaryPrimitives.WriteInt32BigEndian(buf, value); OutStream.Write(buf); }
	public override void Write(uint value) { Span<byte> buf = stackalloc byte[4]; BinaryPrimitives.WriteUInt32BigEndian(buf, value); OutStream.Write(buf); }
	public override void Write(long value) { Span<byte> buf = stackalloc byte[8]; BinaryPrimitives.WriteInt64BigEndian(buf, value); OutStream.Write(buf); }
	public override void Write(ulong value) { Span<byte> buf = stackalloc byte[8]; BinaryPrimitives.WriteUInt64BigEndian(buf, value); OutStream.Write(buf); }
	public override void Write(float value) { Span<byte> buf = stackalloc byte[4]; BinaryPrimitives.WriteSingleBigEndian(buf, value); OutStream.Write(buf); }
	public override void Write(double value) { Span<byte> buf = stackalloc byte[8]; BinaryPrimitives.WriteDoubleBigEndian(buf, value); OutStream.Write(buf); }
}

public class BigEndianReader : BinaryReader
{
	public BigEndianReader(Stream input) : base(input) {}

	public override short ReadInt16() => BinaryPrimitives.ReadInt16BigEndian(BaseStream.ReadByteSpan(2));
	public override ushort ReadUInt16() => BinaryPrimitives.ReadUInt16BigEndian(BaseStream.ReadByteSpan(2));
	public override int ReadInt32() => BinaryPrimitives.ReadInt32BigEndian(BaseStream.ReadByteSpan(4));
	public override uint ReadUInt32() => BinaryPrimitives.ReadUInt32BigEndian(BaseStream.ReadByteSpan(4));
	public override long ReadInt64() => BinaryPrimitives.ReadInt64BigEndian(BaseStream.ReadByteSpan(8));
	public override ulong ReadUInt64() => BinaryPrimitives.ReadUInt64BigEndian(BaseStream.ReadByteSpan(8));
	public override float ReadSingle() => BinaryPrimitives.ReadSingleBigEndian(BaseStream.ReadByteSpan(4));
	public override double ReadDouble() => BinaryPrimitives.ReadDoubleBigEndian(BaseStream.ReadByteSpan(8));
}
