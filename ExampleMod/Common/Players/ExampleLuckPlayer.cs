using ExampleMod.Content.Items;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	public class ExampleLuckPlayer : ModPlayer
	{
		public bool ExampleTorch;
		public override void ModifyLuck(ref float luck) {
			luck = 999999f;
		}
	}
}
