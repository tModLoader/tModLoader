using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Content.Pets.ExamplePet
{
	public class ExamplePetBuff : ModBuff
	{
		public override void SetDefaults() {
			DisplayName.SetDefault("Paper Airplane");
			Description.SetDefault(@"""Let this pet be an example to you!""");

			Main.buffNoTimeDisplay[Type] = true;
			Main.vanityPet[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex) { // This method gets called every frame your buff is active on your player.
			player.buffTime[buffIndex] = 18000;

			int projType = ProjectileType<ExamplePetProjectile>();

			//If the player is local, and there hasn't been a pet projectile spawned yet - spawn it.
			if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[projType] <= 0) {
				Projectile.NewProjectile(player.Center, Vector2.Zero, projType, 0, 0f, player.whoAmI);
			}
		}
	}
}
