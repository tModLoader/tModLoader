using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items.Accessories
{
	// This file is showcasing inheritance to implement an accessory "type" that you can only have one of equipped
	// It also shows how you can interact with inherited methods
	// Additionally, it takes advantage of ValueTuple to make code more compact

	// First, we create an abstract class that all our exclusive accessories will be based on
	// This class won't be autoloaded by tModLoader, meaning it won't "exist" in the game, and we don't need to provide it a texture
	// Further down below will be the actual items (Green/Yellow Exclusive Accessory)
	public abstract class ExclusiveAccessory : ModItem
	{
		public override void SetDefaults() {
			item.width = 30;
			item.height = 32;
			item.accessory = true;
			item.value = Item.sellPrice(gold: 10);
			item.rare = ItemRarityID.Green;
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.SunStone, 1);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		public override bool CanEquipAccessory(Player player, int slot) {
			// To prevent the accessory from being equipped, we need to return false if there is one already in another slot
			// Therefore we go through each accessory slot ignoring vanity slots using FindDifferentEquippedExclusiveAccessory()
			// which we declared in this class below
			if (slot < 10) // This allows the accessory to equip in vanity slots with no reservations
			{
				// Here we use named ValueTuples and retrieve the index of the item, since this is what we need here
				int index = FindDifferentEquippedExclusiveAccessory().index;
				if (index != -1) {
					return slot == index;
				}
			}
			// Here we want to respect individual items having custom conditions for equipability
			return base.CanEquipAccessory(player, slot);
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			// Here we want to add a tooltip to the item if it can be swapped with another one of its kind
			// Therefore we retrieve the accessory from the ValueTuple, because the index isn't needed here
			Item accessory = FindDifferentEquippedExclusiveAccessory().accessory;
			if (accessory != null) {
				tooltips.Add(new TooltipLine(mod, "Swap", "Right click to swap with '" + accessory.Name + "'!") {
					overrideColor = Color.OrangeRed
				});
			}
		}

		public override bool CanRightClick() {
			// An intricacy of vanilla is that it directly swaps the items on right click if the items are the same and just their prefixes differ,
			// even in vanity slots. For this, FindDifferentEquippedExclusiveAccessory() doesn't find these items
			// That means, if for whatever reason you have Green equipped, Yellow in a vanity slot, and then right click a Yellow item in your inventory
			// that has a different prefix than the vanity Yellow, it will swap with the vanity Yellow instead of the equipped Green
			// Therefore we need to reimplement this behavior by doing the following check

			// Check vanity accessory slots for the same item type equipped and return false (so vanilla handles it)
			int maxAccessoryIndex = 5 + Main.LocalPlayer.extraAccessorySlots;
			for (int i = 13; i < 13 + maxAccessoryIndex; i++) {
				if (Main.LocalPlayer.armor[i].type == item.type) return false;
			}

			// Only allow right clicking if there is a different ExclusiveAccessory equipped
			if (FindDifferentEquippedExclusiveAccessory().accessory != null) {
				return true;
			}
			// If this hook returns true, the item is consumed (just like crates and boss bags)
			return base.CanRightClick();
		}

		public override void RightClick(Player player) {
			// Here we implement the "swapping" when right clicked to equip this item inplace of another one
			// Because we need both index and accessory, we "unpack" this ValueTuple like this:
			var (index, accessory) = FindDifferentEquippedExclusiveAccessory();
			if (accessory != null) {
				Main.LocalPlayer.QuickSpawnClonedItem(accessory);
				// We need to use index instead of accessory because we directly want to alter the equipped accessory
				Main.LocalPlayer.armor[index] = item.Clone();
			}
		}

		// We make our own method for compacting the code because we will need to check equipped accessories often
		// This method returns a named ValueTuple, indicated by the (Type name1, Type name2, ...) as the return type
		// This allows us to return more than one value from a method
		protected (int index, Item accessory) FindDifferentEquippedExclusiveAccessory() {
			int maxAccessoryIndex = 5 + Main.LocalPlayer.extraAccessorySlots;
			for (int i = 3; i < 3 + maxAccessoryIndex; i++) {
				Item otherAccessory = Main.LocalPlayer.armor[i];
				// IsAir makes sure we don't check for "empty" slots
				// IsTheSameAs() compares two items and returns true if their types match
				// "is ExclusiveAccessory" is a way of performing pattern matching
				// Here, inheritance helps us determine if the given item is indeed one of our ExclusiveAccessory ones
				if (!otherAccessory.IsAir &&
					!item.IsTheSameAs(otherAccessory) &&
					otherAccessory.modItem is ExclusiveAccessory) {
					// If we find an item that matches these criteria, return both the index and the item itself
					// The second argument is just for convenience, technically we don't need it since we can get the item from just i
					return (i, otherAccessory);
				}
			}
			// If no item is found, we return default values for index and item, always check one of them with this default when you call this method!
			return (-1, null);
		}
	}

	// Here we add our accessories, note that they inherit from ExclusiveAccessory, and not ModItem

	public class GreenExclusiveAccessory : ExclusiveAccessory
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("Increases melee and ranged damage by 50%");
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			// 50% melee and ranged damage increase
			player.meleeDamage += 0.5f;
			player.rangedDamage += 0.5f;
		}

		public override void RightClick(Player player) {
			// This is an example of working with methods in the inheritance chain (you don't need this RightClick() override for your accessory otherwise)

			// Here, before the parent's code is executed, we retrieve the name of the previously equipped item
			// We know guaranteed that there will be an item to be replaced, since otherwise this hook wouldn't run (condition in CanRightClick())
			string previousItemName = "";

			Item accessory = FindDifferentEquippedExclusiveAccessory().accessory;
			if (accessory != null) {
				previousItemName = accessory.Name;
			}

			// In order to preserve its expected behavior (right click swaps this and a different currently equipped accessory)
			// we need to call the parent method via base.Method(arguments)
			// Removing this line will cause this item to just vanish when right clicked
			base.RightClick(player);

			// Here we add additional things that happen on right clicking this item
			Main.NewText("I just equipped " + item.Name + " in place of " + previousItemName + "!");
			Main.PlaySound(SoundID.MaxMana, (int)player.position.X, (int)player.position.Y);
		}
	}

	public class YellowExclusiveAccessory : ExclusiveAccessory
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("Increases melee damage by 100% at day, and ranged damage at night");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			// Not calling base.SetDefaults() will override everything
			// Here we inherit all the properties from our abstract item and just change the rarity
			item.rare = ItemRarityID.Yellow;
		}

		public override void AddRecipes() {
			// because we don't call base.AddRecipes(), we erase the previously defined recipe and can now make a different one
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.SunStone, 1);
			recipe.AddIngredient(ItemID.MoonStone, 1);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			if (Main.dayTime) {
				// 100% melee damage decrease
				player.meleeDamage += 1f;
			}
			else {
				// 100% ranged damage decrease
				player.rangedDamage += 1f;
			}
		}
	}
}
