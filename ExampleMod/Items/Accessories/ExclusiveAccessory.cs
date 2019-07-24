using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items.Accessories
{
	// This file is showcasing inheritance to implement an accessory "type" that you can only have one of equipped
	// It also shows how you can interact with inherited methods
	// Additionally, it takes advantage of delegates to make code more compact

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
			bool canEquipAccessory = true;
			if (slot < 10) // This allows the accessory to equip in vanity slots with no reservations
			{
				// "(int i, Item foundItem) => { //code }" is a so called lambda. Here we pass a chunk of code into the method 
				// that will be executed if it successfully found a different ExclusiveAccessory
				FindDifferentEquippedExclusiveAccessory((int i, Item foundItem) => {
					// If an item is found and slot is the same as its index in armor[], we allow it to be replaced
					canEquipAccessory = slot == i;
				});
			}
			// Here we want to respect individual items having custom conditions for equipability
			return canEquipAccessory;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			// Here we want to add a tooltip to the item if it can't be equipped because another item of this type is already equipped
			FindDifferentEquippedExclusiveAccessory((int i, Item foundItem) => {
				tooltips.Add(new TooltipLine(mod, "AlreadyEquipped", "You can't equip this when '" + foundItem.Name + "' is already equipped!") {
					overrideColor = Color.OrangeRed
				});
			});
		}

		public override bool CanRightClick() {
			// Only allow right clicking if there is a different ExclusiveAccessory equipped
			bool canRightClick = false;
			FindDifferentEquippedExclusiveAccessory((int i, Item foundItem) => canRightClick = true);
			return canRightClick;
		}

		public override void RightClick(Player player) {
			// Here we want to implement the "swapping" when right clicked to equip this item inplace of another one
			FindDifferentEquippedExclusiveAccessory((int i, Item foundItem) => {
				Main.LocalPlayer.QuickSpawnClonedItem(foundItem);
				// We need to use i instead of foundItem because we directly want to alter the equipped accessory
				Main.LocalPlayer.armor[i] = item.Clone();
			});
		}

		// We make our own method for compacting the code because we will need to check equipped accessories often
		// This method also has a delegate as an argument, which allows us to pass entire methods instead of just variables
		// To understand what an Action delegate is, see here: https://www.tutorialsteacher.com/csharp/csharp-action-delegate
		private void FindDifferentEquippedExclusiveAccessory(Action<int, Item> whenFound) {
			int maxAccessoryIndex = 5 + Main.LocalPlayer.extraAccessorySlots;
			for (int i = 3; i < 3 + maxAccessoryIndex; i++) {
				// IsAir makes sure we don't check for "empty" slots
				// IsTheSameAs() compares two items and returns true if their types match
				// "is ExclusiveAccessory" is a way of performing pattern matching
				// Here, inheritance helps us determine if the given item is indeed one of our ExclusiveAccessory ones
				if (!Main.LocalPlayer.armor[i].IsAir &&
					!item.IsTheSameAs(Main.LocalPlayer.armor[i]) &&
					Main.LocalPlayer.armor[i].modItem is ExclusiveAccessory) {
					// If we find an item that matches these criteria, execute the code inside it
					// The second argument is just for convenience, technically we don't need it since we can get the item from just i
					whenFound.Invoke(i, Main.LocalPlayer.armor[i]);
				}
			}
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
			// In order to preserve its expected behavior (right click swaps this and the currently equipped accessory)
			// we need to call the parent method via base.Method(arguments)
			// You can try to remove this line and see if you can swap this item with another one
			base.RightClick(player);

			// Here we add additional things that happen on right clicking this item
			// Beware that this hook is only called if CanRightClick() returns true (which we defined as only returning true when we can swap items)
			Main.NewText("I just equipped " + item.Name + "!");
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
