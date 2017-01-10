using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items.Weapons
{
	public class ExampleJavelin : ModItem
	{
		public override void SetDefaults()
		{
			item.name = "Example Javelin";
			item.shoot = mod.ProjectileType<Projectiles.ExampleJavelinProjectile>();
			item.shootSpeed = 10f;
			item.damage = 29;
			item.knockBack = 5f;
			item.thrown = true;
			item.useStyle = 1;
			item.UseSound = SoundID.Item1;
			item.useAnimation = 25;
			item.useTime = 25;
			item.width = 30;
			item.height = 30;
			item.maxStack = 999;
			item.consumable = true;
			item.noUseGraphic = true;
			item.noMelee = true;
			item.autoReuse = true;
			item.value = 50;
			item.rare = 1;
		}
	}
}
