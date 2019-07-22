using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

// PR NOTE, REMOVE THIS IF IT'S MERGED: some of the comments in here are taken from ExampleDamageItem and SixColorShield
namespace ExampleMod.Items.Accessories
{
	// This file is showcasing inheritance to implement an accessory "type" that you can only have one of equipped
	// It also shows two different ways on how you can interact with inherited methods
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

		public sealed override bool CanEquipAccessory(Player player, int slot) {
			// To prevent the accessory from being equipped, we need to return false if there is one already in another slot
			// Therefore we go through each accessory slot ignoring vanity slots
			if (slot < 10) // This allows the accessory to equip in vanity slots with no reservations
			{
				int maxAccessoryIndex = 5 + player.extraAccessorySlots;
				for (int i = 3; i < 3 + maxAccessoryIndex; i++) {
					// We need "slot != i" because we don't care what is currently in the slot we will be replacing
					// "is ExclusiveAccessory" is a way of performing pattern matching
					// Here, inheritance helps us determine if the given item is indeed one of our ExclusiveAccessory ones
					if (slot != i && player.armor[i].modItem is ExclusiveAccessory) {
						return false;
					}
				}
			}
			// Here we want to respect individual items having custom conditions for equipability
			return SafeCanEquipAccessory(player, slot);
		}

		// Inheriting accessories should override this to further restrict the equipability if necessary
		public virtual bool SafeCanEquipAccessory(Player player, int slot) {
			return true;
		}
	}

	// Here we add our accessories, note that they inherit from ExclusiveAccessory, and not ModItem

	public class GreenExclusiveAccessory : ExclusiveAccessory
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("You can't equip this when 'Yellow Exclusive Accessory' is already equipped!"
				+ "\nIncreases melee and ranged damage by 50%");
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			// 50% melee and ranged damage increase
			player.meleeDamage += 0.5f;
			player.rangedDamage += 0.5f;
		}
	}

	public class YellowExclusiveAccessory : ExclusiveAccessory
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("You can't equip this when 'Green Exclusive Accessory' is already equipped!"
				+ "\nIncreases melee damage by 100% at day, and ranged damage at night");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			// Not calling base.SetDefaults() will override everything
			// Here we inherit all the properties from our abstract item and just change the rarity
			item.rare = ItemRarityID.Yellow;
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
