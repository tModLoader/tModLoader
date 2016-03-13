using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Items
{
	public class ExamplePickaxe : ModItem
	{
		public override void SetDefaults()
		{
			item.name = "Example Pickaxe";
			item.damage = 20;
			item.melee = true;
			item.width = 40;
			item.height = 40;
			item.toolTip = "This is a modded pickaxe.";
			item.useTime = 10;
			item.useAnimation = 10;
			item.pick = 220;
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