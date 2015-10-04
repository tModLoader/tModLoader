using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Projectiles
{
	//WIP - need to add buff support first
	public class PurityWisp : ModProjectile
	{
		public override void SetDefaults()
		{
			projectile.name = "Purity Wisp";
			projectile.width = 24;
			projectile.height = 32;
			Main.projFrames[projectile.type] = 3;
			projectile.friendly = true;
			Main.projPet[projectile.type] = true;
			projectile.minion = true;
			projectile.minionSlots = 1;
			projectile.penetrate = -1;
			projectile.timeLeft = 18000;
			projectile.tileCollide = false;
			projectile.ignoreWater = true;
		}
	}
}