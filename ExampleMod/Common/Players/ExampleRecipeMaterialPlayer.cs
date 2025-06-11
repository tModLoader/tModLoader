using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	// This class showcases how to use items in the chest player stands on (if exists)
	// for crafting, even if it is not opened by the player
	// One use of this is allowing items in your custom bank to be used for crafting
	public class ExampleRecipeMaterialPlayer : ModPlayer
	{
		private int _chestIndexNearby = -1;

		// Nearby chest finding
		public override void PostUpdateMiscEffects() {
			if (Main.netMode == NetmodeID.Server) {
				// We don't need to do any recipe stuff on the server
				return;
			}

			int oldChestIndex = _chestIndexNearby;

			// Gets leg position in tile coord for further chest searching
			var legPosition = Player.Bottom - new Vector2(0f, 20f);
			var legPositionInTile = legPosition.ToTileCoordinates();

			_chestIndexNearby = -1;
			// Find a possible chest nearby
			for (int x = -1; x <= 1; x++) {
				var pos = new Point(legPositionInTile.X + x, legPositionInTile.Y);
				if (!WorldGen.InWorld(pos.X, pos.Y)) {
					continue;
				}

				var tile = Main.tile[pos];

				// Dressers are excluded to make search code simpler
				if (!tile.HasTile || !TileID.Sets.IsAContainer[tile.TileType] || tile.TileType is TileID.Dressers) {
					continue;
				}

				// Gets the left-top position for the chest
				if (tile.TileFrameX % 36 != 0) {
					pos.X--;
				}

				if (tile.TileFrameY != 0) {
					pos.Y--;
				}

				int chestIndex = Chest.FindChest(pos.X, pos.Y);
				if (chestIndex > -1 && !Chest.IsLocked(pos.X, pos.Y)) {
					Chest chest = Main.chest[chestIndex];
					// Unopened chests in multiplayer have not initialized the items inside of them, so we check for safety if the first item is not null (assuming that all others won't be null either)
					// Ideally, we would want to write custom netcode to request chest contents, see how a mod like Recipe Browser handles this: https://github.com/JavidPack/RecipeBrowser/blob/1.4/RecipeBrowser.cs, look for usage of packets
					if (chest.item[0] != null) {
						_chestIndexNearby = chestIndex;
						break;
					}
				}
			}

			// If the nearby chest changed, call FindRecipes to refresh available recipes
			// Since FindRecipes takes a long time to run, we should try to avoid calling it frequently
			if (oldChestIndex != _chestIndexNearby) {
				Recipe.FindRecipes();
			}
		}

		// Use items in the chest for crafting
		public override IEnumerable<Item> AddMaterialsForCrafting(out ItemConsumedCallback itemConsumedCallback) {
			// Ensure there is a chest nearby that is not opened by the player, and wasn't destroyed last tick
			if (_chestIndexNearby is -1 || Player.chest == _chestIndexNearby || Main.chest[_chestIndexNearby] is not Chest chest)
				return base.AddMaterialsForCrafting(out itemConsumedCallback);

			// onUsedForCrafting invokes when the item is consumed, can be used to send packets in multiplayer mode
			// If there is no need for this, just set it to null
			itemConsumedCallback = (_, index) => {
				if (Main.netMode is NetmodeID.MultiplayerClient) {
					// Sync chest data
					NetMessage.SendData(MessageID.SyncChestItem, number: _chestIndexNearby, number2: index);
				}
			};

			// Returns the items in the chest to use them for crafting
			// The returned list should not be a cloned version of items otherwise items will not be consumed
			return chest.item;
		}
	}
}