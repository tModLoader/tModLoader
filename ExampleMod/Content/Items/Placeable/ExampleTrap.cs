using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Placeable
{
	// This item shows off using 1 class to load multiple items. This is an alternate to typical inheritance.
	// Read the comments in this example carefully, as there are many parts necessary to make this approach work.
	// The real strength of this approach is when you have many items that vary by small changes, like how these 2 trap items vary only by placeStyle.
	public class ExampleTrap : ModItem
	{
		// This inner class is an ILoadable, the game will automatically call the Load method when loading this mod.
		// Using this class, we manually call AddContent with 2 instances of the ExampleTrap class. This adds them to the game.
		public class ExampleTrapLoader : ILoadable
		{
			public void Load(Mod mod) {
				mod.AddContent(new ExampleTrap(0));
				mod.AddContent(new ExampleTrap(1));
			}

			public void Unload() {
			}
		}

		// CloneNewInstances is needed so that fields in this class are Cloned onto new instances, such as when this item is crafted or hovered over.
		// By default, the game creates new instances rather than clone. By forcing Clone, we can preserve fields per Item added by the mod while sharing the same class.
		protected override bool CloneNewInstances => true;
		private readonly int placeStyle;

		// The internal name of each ModItem must be unique. This code ensures that each of the 2 ExampleTrap instances added have a unique name.
		// In the localization files, these internal names are used as keys for DisplayName and Tooltip, rather than the classname.
		public override string Name => GetInternalNameFromStyle(placeStyle);

		// This helper method converts from the custom instanced data to the internal name. In this example the placeStyle value is the only custom data.
		// This method is called by the Name property and
		public static string GetInternalNameFromStyle(int style) {
			// Here we define some strings that will be used as the ModItem.Name, the internal name of the ModItem.
			// Every ModItem must have a unique internal name, so this step is necessary.
			// We use these in the ExampleMod.Content.Tiles.ExampleTrap.GetItemDrops rather than ModContent.ItemType<Items.Placeable.ExampleTrap>() to retrieve the correct ItemID.
			if (style == 0) {
				return "ExampleTrapIchorBullet";
			}
			if (style == 1) {
				return "ExampleTrapChlorophyteBullet";
			}
			throw new Exception("Invalid style");
		}

		// Content loaded multiple times must have a non-default constructor. This is where unique data is passed in to be used later. This also prevents the game from attempting to add this ModItem to the game automatically.
		public ExampleTrap(int placeStyle) {
			this.placeStyle = placeStyle;
		}

		public override void SetDefaults() {
			// With all the setup above, placeStyle will be either 0 or 1 for the 2 ExampleTrap instances we've loaded.
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.ExampleTrap>(), placeStyle);

			Item.width = 12;
			Item.height = 12;
			Item.value = 10000;
			Item.mech = true; // lets you see wires while holding.
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.DartTrap)
				.Register();
		}
	}
}
