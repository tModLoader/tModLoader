using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExampleMod.Projectiles.Minions;
using ExampleMod;

namespace ExampleMod.Buffs
{
    public class ExampleOverheadMinionBuff : ModBuff
	{
		public override void SetDefaults() {
			DisplayName.SetDefault("Spirit of light");
			Description.SetDefault("The spirit of light is hovering above your head");
			Main.buffNoSave[Type] = true; //does not save after leaving the game.
			Main.buffNoTimeDisplay[Type] = true; //does not display the buff duration.
		}

			public override void Update(Player player, ref int buffIndex) {
			if (player.ownedProjectileCounts[ModContent.ProjectileType<Projectiles.Minions.ExampleOverheadMinion>()] > 0) {
				player.buffTime[buffIndex] = 18000;
				player.GetModPlayer<ExampleOverheadPlayer>().exampleOverheadMinion = true; //activates the overhead minion variable.
			}
			else {
				player.DelBuff(buffIndex);
				buffIndex--;
			}
	}
}
}