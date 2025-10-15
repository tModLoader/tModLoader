using Microsoft.Xna.Framework;

namespace Terraria.DataStructures;

partial struct Point16
{
	public Point16(int size)
	{
		X = (short)size;
		Y = (short)size;
	}

	public Point16(short size)
	{
		X = size;
		Y = size;
	}

	public static Point16 operator +(Point16 first, Point16 second)
		=> new(first.X + second.X, first.Y + second.Y);

	public static Point16 operator -(Point16 first, Point16 second)
		=> new(first.X - second.X, first.Y - second.Y);

	public static Point16 operator *(Point16 first, Point16 second)
		=> new(first.X * second.X, first.Y * second.Y);

	public static Point16 operator /(Point16 first, Point16 second)
		=> new(first.X / second.X, first.Y / second.Y);

	public static Point16 operator %(Point16 first, int num)
		=> new(first.X % num, first.Y % num);

	public void Deconstruct(out short x, out short y)
	{
		x = X;
		y = Y;
	}
}
