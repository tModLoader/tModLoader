using System;
using System.IO;
using System.Linq;

namespace Terraria.ModLoader.IO
{
	public class BigEndianWriter : BinaryWriter
	{
		public BigEndianWriter(Stream output) : base(output) { }

		private void WriteBigEndian(byte[] bytes) {
			if (BitConverter.IsLittleEndian)
				bytes = bytes.Reverse().ToArray();

			Write(bytes);
		}

		public override void Write(short value) { WriteBigEndian(BitConverter.GetBytes(value)); }
		public override void Write(ushort value) { WriteBigEndian(BitConverter.GetBytes(value)); }
		public override void Write(int value) { WriteBigEndian(BitConverter.GetBytes(value)); }
		public override void Write(uint value) { WriteBigEndian(BitConverter.GetBytes(value)); }
		public override void Write(long value) { WriteBigEndian(BitConverter.GetBytes(value)); }
		public override void Write(ulong value) { WriteBigEndian(BitConverter.GetBytes(value)); }
		public override void Write(float value) { WriteBigEndian(BitConverter.GetBytes(value)); }
		public override void Write(double value) { WriteBigEndian(BitConverter.GetBytes(value)); }
	}

	public class BigEndianReader : BinaryReader
	{
		public BigEndianReader(Stream input) : base(input) { }

		private byte[] ReadBigEndian(int len) =>
			BitConverter.IsLittleEndian ? ReadBytes(len).Reverse().ToArray() : ReadBytes(len);

		public override short ReadInt16() => BitConverter.ToInt16(ReadBigEndian(2), 0);
		public override ushort ReadUInt16() => BitConverter.ToUInt16(ReadBigEndian(2), 0);
		public override int ReadInt32() => BitConverter.ToInt32(ReadBigEndian(4), 0);
		public override uint ReadUInt32() => BitConverter.ToUInt32(ReadBigEndian(4), 0);
		public override long ReadInt64() => BitConverter.ToInt64(ReadBigEndian(8), 0);
		public override ulong ReadUInt64() => BitConverter.ToUInt64(ReadBigEndian(8), 0);
		public override float ReadSingle() => BitConverter.ToSingle(ReadBigEndian(4), 0);
		public override double ReadDouble() => BitConverter.ToDouble(ReadBigEndian(8), 0);
	}
}
