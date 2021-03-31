using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Buffs
{
	public class ExampleHoveringMinion : ModBuff
	{
		public override void SetDefaults() {
			//set the name and description of the buff
			DisplayName.SetDefault("Purity Wisp");
			Description.SetDefault("The purity wisp will fight for you");
			
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true; //the duration of the buff will not display
		}

		public override void Update(Player player, ref int buffIndex) {
			ExamplePlayer modPlayer = player.GetModPlayer<ExamplePlayer>();
		
			if (player.ownedProjectileCounts[ModContent.ProjectileType<Projectiles.Minions.ExampleHoveringMinion>()] > 0) { //if the ExampleHoveringMinion projectile is owned by the player
				modPlayer.exampleHoveringMinion = true; 
			}
			
			if (!modPlayer.exampleHoveringMinion) {
				player.DelBuff(buffIndex); //if the exampleHoveringMinion was despawned, the buff is canceled
				buffIndex--;
			}
			else {
				player.buffTime[buffIndex] = 18000; //keeps the buff active while the minion exists
			}
		}
	}
}
