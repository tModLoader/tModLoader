using ExampleMod.Content.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable
{
	public class ExampleTorch : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a modded torch.");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
		}

		public override void SetDefaults() {
			Item.flame = true;
			Item.noWet = true;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTurn = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.holdStyle = ItemHoldStyleID.HoldFront;
			Item.autoReuse = true;
			Item.maxStack = 999;
			Item.consumable = true;
			Item.createTile = ModContent.TileType<Tiles.ExampleTorch>();
			Item.width = 10;
			Item.height = 12;
			Item.value = 50;
		}

		public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) { //Overrides the default sorting method of this Item.
			itemGroup = ContentSamples.CreativeHelper.ItemGroup.Torches; //Vanilla usually matches sorting methods with the right type of item, but sometimes, like with torches, it doesn't. Make sure to set whichever items manually if need be. 
		}

		public override void HoldItem(Player player) {
			// Randomly spawn sparkles when the torch is held. Twice bigger chance to spawn them when swinging the torch.
			if (Main.rand.Next(player.itemAnimation > 0 ? 40 : 80) == 0) {
				Dust.NewDust(new Vector2(player.itemLocation.X + 16f * player.direction, player.itemLocation.Y - 14f * player.gravDir), 4, 4, ModContent.DustType<Sparkle>());
			}

			// Create a white (1.0, 1.0, 1.0) light at the torch's approximate position, when the item is held.
			Vector2 position = player.RotatedRelativePoint(new Vector2(player.itemLocation.X + 12f * player.direction + player.velocity.X, player.itemLocation.Y - 14f + player.velocity.Y), true);

			Lighting.AddLight(position, 1f, 1f, 1f);
		}

		public override void PostUpdate() {
			// Create a white (1.0, 1.0, 1.0) light when the item is in world, and isn't underwater.
			if (!Item.wet) {
				Lighting.AddLight(Item.Center, 1f, 1f, 1f);
			}
		}

		public override void AutoLightSelect(ref bool dryTorch, ref bool wetTorch, ref bool glowstick) {
			dryTorch = true; // This makes our item eligible for being selected with smart select at a short distance when not underwater.
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
