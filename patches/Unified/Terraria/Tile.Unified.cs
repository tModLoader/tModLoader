namespace Terraria;

partial struct Tile
{
	internal ushort SlowGetHeaderS()
	{
		int s = 0;
		{
			s = TileDataPacking.Pack(color(), s, 0, 5);
			s = TileDataPacking.SetBit(active(), s, 5);
			s = TileDataPacking.SetBit(inActive(), s, 6);
			s = TileDataPacking.SetBit(wire(), s, 7);
			s = TileDataPacking.SetBit(wire2(), s, 8);
			s = TileDataPacking.SetBit(wire3(), s, 9);
			s = TileDataPacking.SetBit(halfBrick(), s, 10);
			s = TileDataPacking.SetBit(actuator(), s, 11);
			s = TileDataPacking.Pack(slope(), s, 12, 3);
			s = TileDataPacking.SetBit(fullbrightWall(), s, 15);
		}
		return (ushort)s;
	}

	internal void SlowSetHeaderS(ushort s)
	{
		int v = s;

		color((byte)TileDataPacking.Unpack(v, 0, 5));
		active(TileDataPacking.GetBit(v, 5));
		inActive(TileDataPacking.GetBit(v, 6));
		wire(TileDataPacking.GetBit(v, 7));
		wire2(TileDataPacking.GetBit(v, 8));
		wire3(TileDataPacking.GetBit(v, 9));
		halfBrick(TileDataPacking.GetBit(v, 10));
		actuator(TileDataPacking.GetBit(v, 11));
		slope((byte)TileDataPacking.Unpack(v, 12, 3));
		fullbrightWall(TileDataPacking.GetBit(v, 15));
	}

	internal byte SlowGetHeaderB1()
	{
		int b = 0;
		{
			b = TileDataPacking.Pack(wallColor(), b, 0, 5);
			b = TileDataPacking.Pack(liquidType(), b, 5, 2);
			b = TileDataPacking.SetBit(wire4(), b, 7);
		}
		return (byte)b;
	}

	internal void SlowSetHeaderB1(byte b)
	{
		int v = b;

		wallColor((byte)TileDataPacking.Unpack(v, 0, 5));
		liquidType((byte)TileDataPacking.Unpack(v, 5, 2));
		wire4(TileDataPacking.GetBit(v, 7));
	}

	internal byte SlowGetHeaderB2()
	{
		int b = 0;
		{
			b = TileDataPacking.Pack(wallFrameX(), b, 0, 4);
			b = TileDataPacking.Pack(frameNumber(), b, 4, 2);
			b = TileDataPacking.Pack(wallFrameNumber(), b, 6, 2);
		}
		return (byte)b;
	}

	internal void SlowSetHeaderB2(byte b)
	{
		int v = b;

		wallFrameX((byte)TileDataPacking.Unpack(v, 0, 4));
		frameNumber((byte)TileDataPacking.Unpack(v, 4, 2));
		wallFrameNumber((byte)TileDataPacking.Unpack(v, 6, 2));
	}

	internal byte SlowGetHeaderB3()
	{
		int b = 0;
		{
			b = TileDataPacking.Pack(wallFrameY(), b, 0, 3);
			b = TileDataPacking.SetBit(checkingLiquid(), b, 3);
			b = TileDataPacking.SetBit(skipLiquid(), b, 4);
			b = TileDataPacking.SetBit(invisibleBlock(), b, 5);
			b = TileDataPacking.SetBit(invisibleWall(), b, 6);
			b = TileDataPacking.SetBit(fullbrightBlock(), b, 7);
		}
		return (byte)b;
	}

	internal void SlowSetHeaderB3(byte b)
	{
		int v = b;

		wallFrameY((byte)TileDataPacking.Unpack(v, 0, 3));
		checkingLiquid(TileDataPacking.GetBit(v, 3));
		skipLiquid(TileDataPacking.GetBit(v, 4));
		invisibleBlock(TileDataPacking.GetBit(v, 5));
		invisibleWall(TileDataPacking.GetBit(v, 6));
		fullbrightBlock(TileDataPacking.GetBit(v, 7));
	}
}
