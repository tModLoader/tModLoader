using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Dusts
{
	public class ExampleSolution : ModDust
	{
		public override void SetDefaults()
		{
			updateType = 110;
		}
	}
}