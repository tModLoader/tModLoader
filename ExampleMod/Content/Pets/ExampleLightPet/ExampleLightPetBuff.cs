using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ExampleMod.Content.Pets.ExampleLightPet
{
	public class ExampleLightPetBuff : ModBuff
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Annoying Light");
			Description.SetDefault("Ugh, soooo annoying");

			Main.buffNoTimeDisplay[Type] = true;
			Main.lightPet[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex) {
			player.buffTime[buffIndex] = 18000;

			int projType = ModContent.ProjectileType<ExampleLightPetProjectile>();

			// If the player is local, and there hasn't been a pet projectile spawned yet - spawn it.
			if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[projType] <= 0) {
				var entitySource = player.GetSource_Buff(buffIndex);

				Projectile.NewProjectile(entitySource, player.Center, Vector2.Zero, projType, 0, 0f, player.whoAmI);
			}
		}
	}
}