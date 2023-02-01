using Terraria;
using Terraria.ID;
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

		// Here I define some strings that will be used as the ModItem.Name, the internal name of the ModItem. 
		// We use these in the ExampleMod.Content.Tiles.ExampleTrap.GetItemDrops rather than ModContent.ItemType<Items.Placeable.ExampleTrap>() to retrieve the correct ItemID.
		public const string ExampleTrapA = "ExampleTrapA";
		public const string ExampleTrapB = "ExampleTrapB";

		// CloneNewInstances is needed so that fields in this class are Cloned onto new instances, such as when this item is crafted or hovered over.
		// By default, the game creates new instances rather than clone. By forcing Clone, we can preserve fields per Item added by the mod while sharing the same class.
		protected override bool CloneNewInstances => true;
		private readonly int placeStyle;

		// The internal name of each piece of content must be unique. This code ensures that each of the 2 ExampleTrap instances added have a unique name.
		public override string Name => placeStyle switch {
			0 => ExampleTrapA,
			1 => ExampleTrapB,
			_ => throw new System.NotImplementedException(),
		};

		// Content loaded multiple times must have a non-default constructor. This is where unique data is passed in to be used later. This also prevents the game from attempting to add this ModItem to the game automatically.
		public ExampleTrap(int placeStyle) {
			this.placeStyle = placeStyle;
		}

		public override void SetStaticDefaults() {
			// Since this code will run for 2 separate items, we need to check which item's SetStaticDefaults method is running and run the appropriate code.
			if (Name == ExampleTrapA)
				DisplayName.SetDefault("Example Trap - Ichor Bullet");
			if (Name == ExampleTrapB)
				DisplayName.SetDefault("Example Trap - Chlorophyte Bullet");
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
