using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Items.Placeable
{
	// This item shows off using 1 class for loading multiple items from the same class. This is an alternate to typical inheritance.
	// CloneNewInstances, Autoload, and a new constructor are necessary to make this work.
	// The real strength of this approach happens when you have many items that vary by small changes, like how these 2 trap items vary only by placeStyle.
	public class ExampleTrap : ModItem
	{
		// Here I define some strings that will be used as the ModItem.Name, the internal name of the ModItem. 
		// We use these in the ExampleMod.Tiles.ExampleTrap.Drop rather than ItemType<Items.Placeable.ExampleTrap>() to retrieve the correct ItemID.
		public const string ExampleTrapA = "ExampleTrapA";
		public const string ExampleTrapB = "ExampleTrapB";

		// CloneNewInstances is needed so that fields in this class are Cloned onto new instances, such as when this item is crafted or hovered over.
		// By default, the game creates new instances rather than clone. By forcing Clone, we can preserve fields per Item added by the mod while sharing the same class.
		public override bool CloneNewInstances => true;
		private int placeStyle;

		public ExampleTrap() { } // An empty constructor is needed for tModLoader to attempt Autoload
		public ExampleTrap(int placeStyle) // This is the real constructor we use in Autoload
		{
			this.placeStyle = placeStyle;
		}

		// We use Autoload to prevent the regular loading of this class and instead load 2 versions of this class that we provide.
		public override bool Autoload(ref string name)
		{
			// We could also call AddItem in ExampleMod.Load, but keeping the code here is a little more organized. (The approach needs the empty constructor above.)
			mod.AddItem(ExampleTrapA, new ExampleTrap(0));
			mod.AddItem(ExampleTrapB, new ExampleTrap(1));
			return false; // returning false prevents the autoload, which is what we want since we loaded the 2 versions we wanted already.
		}

		public override void SetStaticDefaults()
		{
			// In mod.AddItem above, we set ModItem.Name to either ExampleTrapA or ExampleTrapB via the AddItem method. We check this now to know which item we are calling SetStaticDefaults on.
			if (Name == ExampleTrapA)
				DisplayName.SetDefault("Example Trap - Ichor Bullet");
			if (Name == ExampleTrapB)
				DisplayName.SetDefault("Example Trap - Chlorophyte Bullet");
		}

		public override void SetDefaults()
		{
			item.useStyle = 1;
			item.useTurn = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.autoReuse = true;
			item.maxStack = 999;
			item.consumable = true;
			item.createTile = TileType<Tiles.ExampleTrap>();
			// With all the setup above, placeStyle will be either 0 or 1 for the 2 ExampleTrap instances we've loaded.
			item.placeStyle = placeStyle;
			item.width = 12;
			item.height = 12;
			item.value = 10000;
			item.mech = true; // lets you see wires while holding.
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.DartTrap);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
