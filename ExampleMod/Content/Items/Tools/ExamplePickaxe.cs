using ExampleMod.Content.Dusts;
using ExampleMod.Content.Tiles.Furniture;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Content.Items.Tools
{
	public class ExamplePickaxe : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a modded pickaxe.");
		}

		public override void SetDefaults() {
			item.damage = 20;
			item.melee = true;
			item.width = 40;
			item.height = 40;
			item.useTime = 10;
			item.useAnimation = 10;
			item.useStyle = ItemUseStyleID.Swing;
			item.knockBack = 6;
			item.value = Item.buyPrice(gold: 1);
			item.rare = ItemRarityID.Green;
			item.UseSound = SoundID.Item1;
			item.autoReuse = true;

			item.pick = 220; // How strong the pickaxe is, see https://terraria.gamepedia.com/Pickaxe_power for a list of common values
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(Mod);
			recipe.AddIngredient(ItemType<ExampleItem>(), 10);
			recipe.AddTile(TileType<ExampleWorkbench>());
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		public override void MeleeEffects(Player player, Rectangle hitbox) {
			if (Main.rand.NextBool(10)) {
				Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustType<Sparkle>());
			}
		}
	}
}