using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

// PR NOTE, REMOVE THIS IF IT'S MERGED: some of the comments in here are taken from ExampleDamageItem
namespace ExampleMod.Items.Accessories
{
	// This file is showcasing inheritance to implement an accessory "type" that you can only have one of equipped
	// (similar to wings in vanilla, but you can't swap them directly)
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

		// By making the override sealed, we prevent derived classes from further overriding the method and enforcing the use of SafeUpdateAccessory()
		public sealed override void UpdateAccessory(Player player, bool hideVisual) {
			SafeUpdateAccessory(player, hideVisual);
			player.GetModPlayer<ExamplePlayer>().exclusiveAccessory = true;
		}

		public override bool CanEquipAccessory(Player player, int slot) {
			// Here we return false if the player has one of our accessories equipped, true otherwise
			return !player.GetModPlayer<ExamplePlayer>().exclusiveAccessory;
		}

		// Custom accessories should override this to do things
		public virtual void SafeUpdateAccessory(Player player, bool hideVisual) {

		}
	}

	public class GreenExclusiveAccessory : ExclusiveAccessory
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("You can't equip this when 'Yellow Exclusive Accessory' is already equipped!"
				+ "\nIncreases damage by 50%");
		}

		public override void SafeUpdateAccessory(Player player, bool hideVisual) {
			// 50% damage increase
			player.allDamage += 0.5f;
		}
	}

	public class YellowExclusiveAccessory : ExclusiveAccessory
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("You can't equip this when 'Green Exclusive Accessory' is already equipped!"
				+ "\nIncreases melee damage by 100%");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			// Not calling base.SetDefaults() will override everything
			// Here we inherit all the properties from our abstract item and just change the rarity
			item.rare = ItemRarityID.Yellow;
		}

		public override void SafeUpdateAccessory(Player player, bool hideVisual) {
			// 100% melee damage decrease
			player.meleeDamage += 1f;
		}
	}
}
