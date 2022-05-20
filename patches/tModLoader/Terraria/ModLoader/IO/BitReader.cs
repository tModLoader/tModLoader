using System;
using System.Collections.Generic;
using System.IO;

namespace Terraria.ModLoader.IO
{
	public class BitReader
	{
		private byte[] bytes;
		private int i;

		public BitReader(BinaryReader reader) {
			bytes = reader.ReadBytes(reader.ReadVarInt());
		}

		public bool ReadBit() {
			return (bytes[i / 8] & (1 << i++ % 8)) != 0;
		}
	}
}
