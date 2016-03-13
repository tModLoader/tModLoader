using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Projectiles.Pets
{
	public class ExamplePet : ModProjectile
	{
		public override void SetDefaults()
		{
			projectile.CloneDefaults(ProjectileID.ZephyrFish);
			projectile.name = "Paper Airplane";
			aiType = ProjectileID.ZephyrFish;
			Main.projFrames[projectile.type] = 4;
			Main.projPet[projectile.type] = true;
		}

		public override bool PreAI()
		{
			Player player = Main.player[projectile.owner];
			player.zephyrfish = false; // Relic from aiType
			return true;
		}

		public override void AI()
		{
			Player player = Main.player[projectile.owner];
			ExamplePlayer modPlayer = player.GetModPlayer<ExamplePlayer>(mod);
			if (player.dead)
			{
				modPlayer.examplePet = false;
			}
			if (modPlayer.examplePet)
			{
				projectile.timeLeft = 2;
			}
		}
	}
}