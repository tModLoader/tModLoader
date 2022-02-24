using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

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
			// Any mod that changes statLifeMax to be greater than 500 is broken and needs to fix their code.
			// This check also prevents this item from being used before vanilla health upgrades are maxed out.
			return player.statLifeMax == 500 && player.GetModPlayer<ExampleLifeFruitPlayer>().exampleLifeFruits < MaxExampleLifeFruits;
		}

		public override bool? UseItem(Player player) {
			// Do not do this: player.statLifeMax += 2;
			player.statLifeMax2 += LifePerFruit;
			player.statLife += LifePerFruit;
			if (Main.myPlayer == player.whoAmI) {
				// This spawns the green numbers showing the heal value and informs other clients as well.
				player.HealEffect(LifePerFruit);
			}

			// This is very important. This is what makes it permanent.
			player.GetModPlayer<ExampleLifeFruitPlayer>().exampleLifeFruits++;
			// This handles the 2 achievements related to using any life increasing item or getting to exactly 500 hp and 200 mp.
			// Ignored since our item is only useable after this achievement is reached
			// AchievementsHelper.HandleSpecialEvent(player, 2);
			//TODO re-add this when ModAchievement is merged?
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

		public override void ResetEffects() {
			// Increasing health in the ResetEffects hook in particular is important so it shows up properly in the player select menu
			// and so that life regeneration properly scales with the bonus health
			Player.statLifeMax2 += exampleLifeFruits * ExampleLifeFruit.LifePerFruit;
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
}
