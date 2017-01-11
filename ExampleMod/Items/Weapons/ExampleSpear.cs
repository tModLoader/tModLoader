using ExampleMod.Projectiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items.Weapons
{
	public class ExampleSpear : ModItem
	{
		public override void SetDefaults()
		{
			item.name = "Example Spear";
			item.toolTip = "An example spear";
			item.damage = 40;
			item.useStyle = 5;
			item.useAnimation = 18;
			item.useTime = 24;
			item.shootSpeed = 3.7f;
			item.knockBack = 6.5f;
			item.width = 32;
			item.height = 32;
			item.scale = 1f;
			item.rare = 5;
			item.UseSound = SoundID.Item1;
			item.shoot = mod.ProjectileType<ExampleSpearProjectile>();
			item.value = 1000;
			item.noMelee = true; // Important
			item.noUseGraphic = true; // Important
			item.melee = true;
			item.autoReuse = true; // Most spears dont autoReuse, but it's possible
		}

		public override bool CanUseItem(Player player)
		{
			return player.ownedProjectileCounts[item.shoot] < 1; // This is to ensure the spear doesn't bug out when using autoReuse = true
		}
	}
}
