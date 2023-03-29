using System;
using System.Collections.Generic;
using System.IO;

namespace Terraria.ModLoader.IO;

public class BitReader
{
	private byte[] bytes;
	public int MaxBits { get; private set; }
	public int BitsRead { get; private set; }

	public BitReader(BinaryReader reader)
	{
		MaxBits = reader.Read7BitEncodedInt();

		int byteCount = MaxBits / 8;
		if (MaxBits % 8 != 0)
			byteCount++;

		bytes = reader.ReadBytes(byteCount);
	}

	public bool ReadBit()
	{
		if (BitsRead >= MaxBits) {
			throw new IOException("Read overflow while reading compressed bits, more info below");
		}

		return (bytes[BitsRead / 8] & (1 << BitsRead++ % 8)) != 0;
	}
}
