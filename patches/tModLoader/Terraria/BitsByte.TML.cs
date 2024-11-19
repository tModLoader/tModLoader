using System;

namespace Terraria;

public partial struct BitsByte
{
	public void Deconstruct(out bool b0)
	{
		b0 = this[0];
	}

	public void Deconstruct(out bool b0, out bool b1)
	{
		b0 = this[0];
		b1 = this[1];
	}

	public void Deconstruct(out bool b0, out bool b1, out bool b2)
	{
		b0 = this[0];
		b1 = this[1];
		b2 = this[2];
	}

	public void Deconstruct(out bool b0, out bool b1, out bool b2, out bool b3)
	{
		b0 = this[0];
		b1 = this[1];
		b2 = this[2];
		b3 = this[3];
	}

	public void Deconstruct(out bool b0, out bool b1, out bool b2, out bool b3, out bool b4)
	{
		b0 = this[0];
		b1 = this[1];
		b2 = this[2];
		b3 = this[3];
		b4 = this[4];
	}

	public void Deconstruct(out bool b0, out bool b1, out bool b2, out bool b3, out bool b4, out bool b5)
	{
		b0 = this[0];
		b1 = this[1];
		b2 = this[2];
		b3 = this[3];
		b4 = this[4];
		b5 = this[5];
	}

	public void Deconstruct(out bool b0, out bool b1, out bool b2, out bool b3, out bool b4, out bool b5, out bool b6)
	{
		b0 = this[0];
		b1 = this[1];
		b2 = this[2];
		b3 = this[3];
		b4 = this[4];
		b5 = this[5];
		b6 = this[6];
	}

	public void Deconstruct(out bool b0, out bool b1, out bool b2, out bool b3, out bool b4, out bool b5, out bool b6, out bool b7)
	{
		b0 = this[0];
		b1 = this[1];
		b2 = this[2];
		b3 = this[3];
		b4 = this[4];
		b5 = this[5];
		b6 = this[6];
		b7 = this[7];
	}

	// Added by TML to ease debugging
	public override string ToString()
	{
		Span<char> characters = stackalloc char[8];

		for (int i = 0; i < 8; i++) {
			characters[i] = this[i] ? '1' : '0';
		}

		return new string(characters);
	}
}
