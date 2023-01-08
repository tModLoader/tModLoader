using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace ExampleMod.Common.Players
{
	// This class showcases how to use items in the chest player stands on (if exists)
	// for crafting (consuming items from piggy bank), even if it is not opened by the player
	// One use of this is allowing items in your custom bank to be used for crafting
	public class ExampleRecipeMaterialPlayer : ModPlayer
	{
		private int _chestIndexNearby = -1;

		// Nearby chest finding
		public override void PostUpdateMiscEffects() {
			int oldChestIndex = _chestIndexNearby;

			// Gets leg position in tile coord for further chest searching
			var legPosition = Player.Bottom - new Vector2(0f, 20f);
			var legPositionInTile = legPosition.ToTileCoordinates();

			_chestIndexNearby = -1;
			// Find a possible chest nearby
			for (int x = -1; x <= 1; x++) {
				var tile = Main.tile[legPositionInTile.X + x, legPositionInTile.Y];

				// Dressers are excluded to make search code simpler
				if (!tile.HasTile || !TileID.Sets.IsAContainer[tile.TileType] || tile.TileType is TileID.Dressers) {
					continue;
				}

				// Gets the left-top position for the chest
				int left = legPositionInTile.X + x;
				int top = legPositionInTile.Y;

				if (tile.TileFrameX % 36 != 0) {
					left--;
				}

				if (tile.TileFrameY != 0) {
					top--;
				}

				int chest = Chest.FindChest(left, top);
				if (chest > 0) {
					_chestIndexNearby = chest;
					break;
				}
			}

			// If the nearby chest changed, call FindRecipes to refresh available recipes
			// Since FindRecipes takes a long time to run, we should try to avoid calling it frequently
			if (oldChestIndex != _chestIndexNearby) {
				Recipe.FindRecipes();
			}
		}

		// Use items in the chest for crafting
		public override List<Item> AddMaterialsForCrafting(out PlayerLoader.ActionOnUsedForCrafting onUsedForCrafting) {
			// Ensure there is a chest nearby that is not opened by the player
			if (_chestIndexNearby is -1 || Player.chest == _chestIndexNearby)
				return base.AddMaterialsForCrafting(out onUsedForCrafting);

			// onUsedForCrafting invokes when the item is consumed, can be used to send packets in multiplayer mode
			// If there is no need for this, just set it to null
			onUsedForCrafting = (_, index) => {
				if (Main.netMode is NetmodeID.MultiplayerClient) {
					// Sync chest data
					NetMessage.SendData(MessageID.SyncChestItem, number: _chestIndexNearby, number2: index);
				}
			};

			// Returns the items in the chest to use them for crafting
			// The returned list should not be a cloned version of items otherwise items will not be consumed
			return Main.chest[_chestIndexNearby].item.ToList();
		}
	}
}