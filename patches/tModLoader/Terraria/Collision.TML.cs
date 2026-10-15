using System;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terraria;

public partial class Collision
{
	public static bool GetModdedWets(Vector2 Position, int Width, int Height, out bool[] liquidsIn)
	{
		liquidsIn = new bool[LiquidLoader.LiquidCount];
		WetCollision(Position, Width, Height, out bool[] liquidsInNorm);
		LavaCollision(Position, Width, Height, out bool[] liquidsInAlt);
		Array.Copy(liquidsInNorm, liquidsIn, LiquidLoader.LiquidCount);
		for (int i = LiquidID.Count; i < LiquidLoader.LiquidCount; i++) {
			if (LiquidLoader.GetLiquid(i).UsesLavaCollisionForWet) {
				liquidsIn[i] = liquidsInAlt[i];
			}
		}
		return liquidsIn.Contains(true);
	}
}
