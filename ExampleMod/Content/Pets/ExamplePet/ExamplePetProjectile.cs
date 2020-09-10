using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Pets.ExamplePet
{
	public class ExamplePetProjectile : ModProjectile
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Paper Airplane");

			Main.projFrames[projectile.type] = 4;
			Main.projPet[projectile.type] = true;
		}

		public override void SetDefaults() {
			projectile.CloneDefaults(ProjectileID.ZephyrFish); // Copy the stats of the Zephyr Fish

			aiType = ProjectileID.ZephyrFish; // Copy the AI of the Zephyr Fish.
		}

		public override bool PreAI() {
			Player player = Main.player[projectile.owner];

			player.zephyrfish = false; // Relic from aiType

			return true;
		}

		public override void AI() {
			Player player = Main.player[projectile.owner];

			//Keep the projectile from disappearing as long as the player isn't dead and has the pet buff.
			if (!player.dead && player.HasBuff(ModContent.BuffType<ExamplePetBuff>())) {
				projectile.timeLeft = 2;
			}
		}
	}
}
