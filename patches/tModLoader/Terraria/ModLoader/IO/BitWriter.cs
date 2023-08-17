using System.Collections.Generic;
using System.IO;

namespace Terraria.ModLoader.IO;

public class BitWriter
{
	private List<byte> bytes = new();
	private byte cur;
	private int i;

	public void WriteBit(bool b)
	{
		if (b) {
			cur |= (byte)(1 << i);
		}

		if (++i == 8) {
			bytes.Add(cur);
			cur = 0;
			i = 0;
		}
	}

	public void Flush(BinaryWriter w)
	{
		w.Write7BitEncodedInt(bytes.Count * 8 + i);

		if (i > 0) {
			bytes.Add(cur);
		}

		foreach (var b in bytes) {
			w.Write(b);
		}
	}
}
