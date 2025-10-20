using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Armor
{
	// The AutoloadEquip attribute automatically attaches an equip texture to this item.
	// Providing the EquipType.Body value here will result in TML expecting a X_Body.png file to be placed next to the item's main texture.
	[AutoloadEquip(EquipType.Body)]
	public class ExampleBreastplate : ModItem
	{
		public static readonly int MaxManaIncrease = 20;
		public static readonly int MaxLifeIncrease = 20;
		public static readonly int MaxMinionIncrease = 1;

		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MaxManaIncrease, MaxMinionIncrease);

		public override void SetDefaults() {
			Item.width = 18; // Width of the item
			Item.height = 18; // Height of the item
			Item.value = Item.sellPrice(gold: 1); // How many coins the item is worth
			Item.rare = ItemRarityID.Green; // The rarity of the item
			Item.defense = 6; // The amount of defense the item will give when equipped
		}

		public override void UpdateEquip(Player player) {
			player.buffImmune[BuffID.OnFire] = true; // Make the player immune to Fire
			player.statManaMax2 += MaxManaIncrease; // Increase how many mana points the player can have by 20
			player.maxMinions += MaxMinionIncrease; // Increase how many minions the player can have by one

			// Some effects are applied in ExampleBreastplatePlayer below.
			player.GetModPlayer<ExampleBreastplatePlayer>().exampleBreastplate = true;
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe().AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}

	public class ExampleBreastplatePlayer : ModPlayer
	{
		public bool exampleBreastplate = false;

		public override void ResetEffects() {
			if (exampleBreastplate) {
				// Player.statLifeMax2 needs to be modified in ResetEffects due to the order that player life max code is executed.
				Player.statLifeMax2 += ExampleBreastplate.MaxLifeIncrease;
			}
			exampleBreastplate = false;
		}
	}
}
