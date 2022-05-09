using ExampleMod.Content.Projectiles;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Weapons
{
	public class ExampleWhip : ModItem
	{
		public override void SetStaticDefaults() {
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults() {
			// Call this method to quickly set the whip's properties.
			// Mouse over the method to see the parameters
			Item.DefaultToWhip(ModContent.ProjectileType<ExampleWhipProjectile>(), 20, 2, 4); 

			Item.shootSpeed = 4;
			Item.rare = ItemRarityID.Green;
			Item.channel = true; // This is used for the charging functionality. Remove it if your whip shouldn't be chargeable.
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}
