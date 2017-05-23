using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Buffs
{
	public class ExamplePet : ModBuff
	{
		public override void SetDefaults()
		{
			DisplayName.SetDefault("Paper Airplane");
			Description.SetDefault("\"Let this pet be an example to you!\"");
			Main.buffNoTimeDisplay[Type] = true;
			Main.vanityPet[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			player.buffTime[buffIndex] = 18000;
            player.GetModPlayer<ExamplePlayer>(mod).examplePet = true;
			bool petProjectileNotSpawned = player.ownedProjectileCounts[mod.ProjectileType("ExamplePet")] <= 0;
			if (petProjectileNotSpawned && player.whoAmI == Main.myPlayer)
			{
				Projectile.NewProjectile(player.position.X + (float)(player.width / 2), player.position.Y + (float)(player.height / 2), 0f, 0f, mod.ProjectileType("ExamplePet"), 0, 0f, player.whoAmI, 0f, 0f);
			}
		}
	}
}