using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Items
{
	public class ExampleHamaxe : ModItem
	{
		public override void SetDefaults()
		{
			item.name = "Example Hamaxe";
			item.damage = 25;
			item.melee = true;
			item.width = 40;
			item.height = 40;
			item.toolTip = "This is a modded hamaxe.";
			item.useTime = 15;
			item.useAnimation = 15;
			item.axe = 30;
			item.hammer = 100;
			item.useStyle = 1;
			item.knockBack = 6;
			item.value = 10000;
			item.rare = 2;
			item.useSound = 1;
			item.autoReuse = true;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleItem", 10);
			recipe.AddTile(null, "ExampleWorkbench");
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		public override void MeleeEffects(Player player, Rectangle hitbox)
		{
			if (Main.rand.Next(10) == 0)
			{
				int dust = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, mod.DustType("Sparkle"));
			}
		}
	}
}