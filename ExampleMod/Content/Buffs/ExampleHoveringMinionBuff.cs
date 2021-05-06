using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Content.Buffs
{
	public class ExampleHoveringMinionBuff : ModBuff
	{
		public override void SetDefaults() {
			DisplayName.SetDefault("Purity Wisp"); // Sets the English name of the buff
			Description.SetDefault("The purity wisp will fight for you"); // Sets the English description of the buff
			
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true; // The duration of the buff will not display
		}

		public override void Update(Player player, ref int buffIndex) {
			// If the ExampleHoveringMinion projectile exists, reset the buff time, otherwise remove the buff
			if (player.ownedProjectileCounts[ProjectileType<Projectiles.Minions.ExampleHoveringMinion>()] > 0) {
				player.buffTime[buffIndex] = 18000;
			}
			else {
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}
}