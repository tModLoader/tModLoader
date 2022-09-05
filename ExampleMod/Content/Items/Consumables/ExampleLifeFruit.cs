using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.GameContent.UI.ResourceSets;

namespace ExampleMod.Content.Items.Consumables
{
	// Making an item like Life Fruit (That goes above 500) involves a lot of code, as there are many things to consider.
	// (An alternate that approaches 500 can simply follow vanilla code, however.):
	// You can't make player.statLifeMax more than 500 (it won't save), so you'll have to maintain your extra life within your mod.
	// Within your ModPlayer, you need to save/load a count of usages. You also need to sync the data to other players.
	internal class ExampleLifeFruit : ModItem
	{
		public const int MaxExampleLifeFruits = 10;
		public const int LifePerFruit = 10;

		public override string Texture => "Terraria/Images/Item_" + ItemID.LifeFruit;

		public override void SetStaticDefaults() {
			Tooltip.SetDefault($"Permanently increases maximum life by {LifePerFruit}\nUp to {MaxExampleLifeFruits} can be used");

			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 10;
		}

		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.LifeFruit);
			Item.color = Color.Purple;
		}

		public override bool CanUseItem(Player player) {
			// This check prevents this item from being used before vanilla health upgrades are maxed out.
			return player.ConsumedLifeCrystals == Player.LifeCrystalMax && player.ConsumedLifeFruit == Player.LifeFruitMax;
		}

		public override bool? UseItem(Player player) {
			// Moving the exampleLifeFruits check from CanUseItem to here allows this example fruit to still "be used" like Life Crystals can be
			// when at the max allowed, but it will just play the animation and not affect the player's max life
			if (player.GetModPlayer<ExampleLifeFruitPlayer>().exampleLifeFruits < MaxExampleLifeFruits) {
				// This method handles permanently increasing the player's max health and displaying the green heal text
				player.UseHealthMaxIncreasingItem(LifePerFruit);

				// This field tracks how many of the example fruit have been consumed
				player.GetModPlayer<ExampleLifeFruitPlayer>().exampleLifeFruits++;
				// This handles the 2 achievements related to using any life increasing item or getting at or above 500 hp and 200 mp.
				// Ignored since our item is only useable after this achievement is reached
				// AchievementsHelper.HandleSpecialEvent(player, 2);
				//TODO re-add this when ModAchievement is merged?
			}
			return true;
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}

	public class ExampleLifeFruitPlayer : ModPlayer
	{
		public int exampleLifeFruits;

		public override void ModifyMaxStats(ref int lifeMax, ref int manaMax) {
			// "lifeMax" is added as a shortcut for "player.statLifeMax"
			// Modifying "lifeMax" here allows the changes to appear in the player select menu as well
			lifeMax += exampleLifeFruits * ExampleLifeFruit.LifePerFruit;
		}

		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
			ModPacket packet = Mod.GetPacket();
			packet.Write((byte)ExampleMod.MessageType.ExamplePlayerSyncPlayer);
			packet.Write((byte)Player.whoAmI);
			packet.Write(exampleLifeFruits);
			packet.Send(toWho, fromWho);
		}

		// NOTE: The tag instance provided here is always empty by default.
		// Read https://github.com/tModLoader/tModLoader/wiki/Saving-and-loading-using-TagCompound to better understand Saving and Loading data.
		public override void SaveData(TagCompound tag) {
			tag["exampleLifeFruits"] = exampleLifeFruits;
		}

		public override void LoadData(TagCompound tag) {
			exampleLifeFruits = (int) tag["exampleLifeFruits"];
		}
	}

	public class ExampleLifeFruitSystem : ModSystem
	{
		public override void ModifyStatSnapshot(Player player, ref PlayerStatsSnapshot snapshot) {
			// Use this hook to modify the snapshot of the player's maximum health and mana before drawing the UIs
			// tModLoader will automatically cap the amount of hearts/stars at 20 after this hook is called, so you don't have to modify the snapshot
			//   with your increased life/mana max from ModPlayer.ModifyMaxStats()
		}
	}
}
