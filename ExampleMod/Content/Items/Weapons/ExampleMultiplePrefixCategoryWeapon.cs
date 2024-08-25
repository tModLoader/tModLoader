using ExampleMod.Content.DamageClasses;
using ExampleMod.Content.Tiles.Furniture;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Weapons
{
	public class ExampleMultiplePrefixCategoryWeapon : ModItem
	{
		public override void SetDefaults() {
			Item.DamageType = DamageClass.Ranged;
			Item.width = 40;
			Item.height = 40;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.autoReuse = true;
			Item.damage = 70;
			Item.knockBack = 4;
			Item.crit = 6;
			Item.mana = 6; // Makes the item use mana so it can receive all magic prefixes
			Item.value = Item.buyPrice(gold: 1);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item1;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>(20)
				.AddTile<ExampleWorkbench>()
				.Register();
		}

		// These methods allows us to force this weapon to get melee and magic prefixes despite being a DamageClass.Ranged weapon. Ranged specific prefixes are also excluded.
		public override bool MeleePrefix() {
			return true;
		}

		public override bool MagicPrefix() {
			return true;
		}

		public override bool RangedPrefix() {
			return false;
		}
	}
}
