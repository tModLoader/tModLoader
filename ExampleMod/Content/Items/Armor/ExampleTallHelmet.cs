using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Armor
{
	// This class shows an example of making a tall helmet using ArmorIDs.Head.Sets.IsTallHat.
	// Tall helmets like the Wizard Hat are drawn with slightly different logic, so be sure to consult an existing Head animation sprite.
	// If the drawing logic of IsTallHat is insufficient, modders can make a custom PlayerDrawLayer to manually draw instead.
	[AutoloadEquip(EquipType.Head)]
	public class ExampleTallHelmet : ModItem
	{
		public override void SetStaticDefaults() {
			ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true;
			// Setting IsTallHat is the only special thing this item does.
			ArmorIDs.Head.Sets.IsTallHat[Item.headSlot] = true;
		}

		public override void SetDefaults() {
			Item.width = 18;
			Item.height = 24;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Green;
			Item.defense = 5;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}
