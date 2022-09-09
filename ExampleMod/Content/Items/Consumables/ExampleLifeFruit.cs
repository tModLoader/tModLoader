using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.GameContent.UI.ResourceSets;

namespace ExampleMod.Content.Items.Consumables
{
	// This file showcases how to create an item that increases the player's maximum health on use.
	// Within your ModPlayer, you need to save/load a count of usages. You also need to sync the data to other players.
	// The overlay used to display the custom life fruit can be found in Common/UI/ResourceDisplay/VanillaHealthOverlay.cs
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

		public override bool ConsumeItem(Player player) {
			// Prevent consuming the item when the player has used the max amount
			// This hook works in tandem with UseItem to make the item still "usable" once the player has reached the max amount
			return player.GetModPlayer<ExampleLifeFruitPlayer>().exampleLifeFruits < MaxExampleLifeFruits;
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

		public override void ModifyMaxStats(out StatModifier health, out StatModifier mana) {
			health = StatModifier.Default;
			health.Base = exampleLifeFruits * ExampleLifeFruit.LifePerFruit;
			// Alternatively:  health = StatModifier.Default with { Base = exampleLifeFruits * ExampleLifeFruit.LifePerFruit };
			mana = StatModifier.Default;
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
