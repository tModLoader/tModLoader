using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Buffs
{
	public class CarBuff : ModBuff
	{
		public override void SetDefaults()
		{
			Main.buffNoTimeDisplay[Type] = true;
			Main.buffName[this.Type] = "Car";
			Main.buffTip[this.Type] = "Leather seats, 4 cupholders";
		}

		public override void Update(Player player, ref int buffIndex)
		{
			player.mount.SetMount(mod.MountType("Car"), player);
			player.buffTime[buffIndex] = 10;
		}
	}
}
