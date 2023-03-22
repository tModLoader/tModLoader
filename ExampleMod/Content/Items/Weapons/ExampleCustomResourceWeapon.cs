using System.Collections.Generic;
using ExampleMod.Common.Players;
using ExampleMod.Content.Tiles.Furniture;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Weapons
{
	public class ExampleCustomResourceWeapon : ModItem
	{
		private int exampleResourceCost; // Add our custom resource cost

		public override void SetDefaults() {
			Item.damage = 130;
			Item.DamageType = DamageClass.Magic;
			Item.width = 38;
			Item.height = 38;
			Item.useTime = 16;
			Item.useAnimation = 16;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 6;
			Item.value = Item.buyPrice(gold: 15);
			Item.rare = ItemRarityID.Pink;
			Item.UseSound = SoundID.Item71;
			Item.autoReuse = true;
			Item.shoot = ProjectileID.VortexBeaterRocket;
			Item.shootSpeed = 7;
			Item.crit = 32;
			exampleResourceCost = 5; // Set our custom resource cost to 5
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			tooltips.Add(new TooltipLine(Mod, "Example Resource Cost", $"Uses {exampleResourceCost} example resource"));
		}

		// Make sure you can't use the item if you don't have enough resource
		public override bool CanUseItem(Player player) {
			var exampleResourcePlayer = player.GetModPlayer<ExampleResourcePlayer>();

			if (exampleResourcePlayer.exampleResourceCurrent >= exampleResourceCost) {
				exampleResourcePlayer.exampleResourceCurrent -= exampleResourceCost;
				return true;
			}

			return false;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>(10)
				.AddTile<ExampleWorkbench>()
				.Register();
		}
	}
}
