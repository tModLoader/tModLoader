using System;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Buffs
{
	public class ExamplePet : ModBuff
	{
		public override void SetDefaults()
		{
			Main.buffName[Type] = "Paper Airplane";
			Main.buffTip[Type] = "\"Let this pet be an example to you!\"";
			Main.buffNoTimeDisplay[Type] = true;
			Main.vanityPet[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			player.buffTime[buffIndex] = 18000;
			ExamplePlayer modPlayer = (ExamplePlayer)player.GetModPlayer(mod, "ExamplePlayer");
			modPlayer.examplePet = true;
			bool petProjectileNotSpawned = true;
			if (player.ownedProjectileCounts[mod.ProjectileType("ExamplePet")] > 0)
			{
				petProjectileNotSpawned = false;
			}
			if (petProjectileNotSpawned && player.whoAmI == Main.myPlayer)
			{
				Projectile.NewProjectile(player.position.X + (float)(player.width / 2), player.position.Y + (float)(player.height / 2), 0f, 0f, mod.ProjectileType("ExamplePet"), 0, 0f, player.whoAmI, 0f, 0f);
			}
		}
	}
}