using ExampleMod.Content.Dusts;
using ExampleMod.Content.Items.Placeable.Furniture;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExampleMod.Content.Tiles.Furniture
{
	public class ExampleChest : ModTile
	{
		public static LocalizedText LockedText { get; private set; }

		public override void SetStaticDefaults() {
			// Properties
			Main.tileSpelunker[Type] = true;
			Main.tileContainer[Type] = true;
			Main.tileShine2[Type] = true;
			Main.tileShine[Type] = 1200;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileOreFinderPriority[Type] = 500;
			TileID.Sets.HasOutlines[Type] = true;
			TileID.Sets.BasicChest[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileID.Sets.AvoidedByNPCs[Type] = true;
			TileID.Sets.InteractibleByNPCs[Type] = true;
			TileID.Sets.IsAContainer[Type] = true;
			TileID.Sets.FriendlyFairyCanLureTo[Type] = true;
			TileID.Sets.GeneralPlacementTiles[Type] = false;

			DustType = ModContent.DustType<Sparkle>();
			AdjTiles = [TileID.Containers];
			VanillaFallbackOnModDeletion = TileID.Containers;

			// Other tiles with just one map entry use CreateMapEntryName() to use the default translationkey, "MapEntry"
			// Since ExampleChest needs multiple, we register our own MapEntry keys
			AddMapEntry(new Color(200, 200, 200), this.GetLocalization("MapEntry0"), MapChestName);
			AddMapEntry(new Color(0, 141, 63), this.GetLocalization("MapEntry1"), MapChestName);

			// Style 1 is ExampleChest when locked. We want that tile style to drop the ExampleChest item as well. Use the Chest Lock item to lock this chest.
			// No item places ExampleChest in the locked style, so the automatically determined item drop is unknown, this is why RegisterItemDrop is necessary in this situation.
			RegisterItemDrop(ModContent.ItemType<Items.Placeable.Furniture.ExampleChest>(), 1);
			// Sometimes mods remove content, such as tile styles, or tiles accidentally get corrupted. We can, if desired, register a fallback item for any tile style that doesn't have an automatically determined item drop. This is done by omitting the tileStyles parameter.
			RegisterItemDrop(ItemID.Chest);

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Origin = new Point16(0, 1);
			TileObjectData.newTile.CoordinateHeights = [16, 18];
			TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(Chest.FindEmptyChest, -1, 0, true);
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(Chest.AfterPlacement_Hook, -1, 0, false);
			TileObjectData.newTile.AnchorInvalidTiles = [
				TileID.MagicalIceBlock,
				TileID.Boulder,
				TileID.BouncyBoulder,
				TileID.LifeCrystalBoulder,
				TileID.RollingCactus
			];
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
			TileObjectData.addTile(Type);

			LockedText = this.GetLocalization("Locked");
		}

		// This example shows using GetItemDrops to manually decide item drops. This example is for a tile with a TileObjectData.
		// This example is commented out because the RegisterItemDrop line in SetStaticDefaults above handles this situation and is the recommended approach, but the code is still useful to learn from if conditional drops need to be implemented.
		/*
		public override IEnumerable<Item> GetItemDrops(int i, int j) {
			Tile tile = Main.tile[i, j];
			int style = TileObjectData.GetTileStyle(tile);
			if (style == 0) {
				yield return new Item(ModContent.ItemType<Items.Placeable.Furniture.ExampleChest>());
			}
			if (style == 1) {
				yield return new Item(ModContent.ItemType<Items.Placeable.Furniture.ExampleChest>());
			}
		}
		*/

		public override ushort GetMapOption(int i, int j) {
			return (ushort)(Main.tile[i, j].TileFrameX / 36);
		}

		public override LocalizedText DefaultContainerName(int frameX, int frameY) {
			int option = frameX / 36;
			return this.GetLocalization("MapEntry" + option);
		}

		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) {
			return true;
		}

		public override bool IsLockedChest(int i, int j) {
			return Main.tile[i, j].TileFrameX / 36 == 1;
		}

		public override bool UnlockChest(int i, int j, ref short frameXAdjustment, ref int dustType, ref bool manual) {
			if (Main.dayTime) {
				Main.NewText(LockedText, Color.Orange);
				return false;
			}

			DustType = dustType;
			return true;
		}

		public override bool LockChest(int i, int j, ref short frameXAdjustment, ref bool manual) {
			int style = TileObjectData.GetTileStyle(Main.tile[i, j]);
			// We need to return true only if the tile style is the unlocked variant of a chest that supports locking.
			if (style == 0) {
				// We can check other conditions as well, such as how biome chests can't be locked until Plantera is defeated
				return true;
			}
			return false;
		}

		public static string MapChestName(string name, int i, int j) {
			int left = i;
			int top = j;
			Tile tile = Main.tile[i, j];
			if (tile.TileFrameX % 36 != 0) {
				left--;
			}

			if (tile.TileFrameY != 0) {
				top--;
			}

			int chest = Chest.FindChest(left, top);
			if (chest < 0) {
				return Language.GetTextValue("LegacyChestType.0");
			}

			if (Main.chest[chest].name == "") {
				return name;
			}

			return name + ": " + Main.chest[chest].name;
		}

		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = 1;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			// We override KillMultiTile to handle additional logic other than the item drop. In this case, unregistering the Chest from the world
			Chest.DestroyChest(i, j);
		}

		public override bool RightClick(int i, int j) {
			Player player = Main.LocalPlayer;
			Tile tile = Main.tile[i, j];
			Main.mouseRightRelease = false;
			int left = i;
			int top = j;
			if (tile.TileFrameX % 36 != 0) {
				left--;
			}

			if (tile.TileFrameY != 0) {
				top--;
			}

			player.CloseSign();
			player.SetTalkNPC(-1);
			Main.npcChatCornerItem = 0;
			Main.npcChatText = "";
			if (Main.editChest) {
				SoundEngine.PlaySound(SoundID.MenuTick);
				Main.editChest = false;
				Main.npcChatText = string.Empty;
			}

			if (player.editedChestName) {
				NetMessage.SendData(MessageID.SyncPlayerChest, text: NetworkText.FromLiteral(Main.chest[player.chest].name), number: player.chest, number2: 1f);
				player.editedChestName = false;
			}

			bool isLocked = Chest.IsLocked(left, top);
			if (Main.netMode == NetmodeID.MultiplayerClient && !isLocked) {
				if (left == player.chestX && top == player.chestY && player.chest != -1) {
					player.chest = -1;
					Recipe.FindRecipes();
					SoundEngine.PlaySound(SoundID.MenuClose);
				}
				else {
					NetMessage.SendData(MessageID.RequestChestOpen, number: left, number2: top);
					Main.stackSplit = 600;
				}
			}
			else {
				if (isLocked) {
					// Make sure to change the code in UnlockChest if you don't want the chest to only unlock at night.
					int key = ModContent.ItemType<ExampleChestKey>();
					if (player.HasItemInInventoryOrOpenVoidBag(key) && Chest.Unlock(left, top) && player.ConsumeItem(key, includeVoidBag: true)) {
						if (Main.netMode == NetmodeID.MultiplayerClient) {
							// The 1 value of the number2 parameter is for unlocking chests.
							NetMessage.SendData(MessageID.LockAndUnlock, number2: 1f, number3: left, number4: top);
						}
					}
				}
				else {
					int chest = Chest.FindChest(left, top);
					if (chest != -1) {
						Main.stackSplit = 600;
						if (chest == player.chest) {
							player.chest = -1;
							SoundEngine.PlaySound(SoundID.MenuClose);
						}
						else {
							SoundEngine.PlaySound(player.chest < 0 ? SoundID.MenuOpen : SoundID.MenuTick);
							player.OpenChest(left, top, chest);
						}

						Recipe.FindRecipes();
					}
				}
			}

			return true;
		}

		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;
			Tile tile = Main.tile[i, j];
			int left = i;
			int top = j;
			if (tile.TileFrameX % 36 != 0) {
				left--;
			}

			if (tile.TileFrameY != 0) {
				top--;
			}

			int chest = Chest.FindChest(left, top);
			player.cursorItemIconID = -1;
			if (chest < 0) {
				player.cursorItemIconText = Language.GetTextValue("LegacyChestType.0");
			}
			else {
				string defaultName = TileLoader.DefaultContainerName(tile.TileType, tile.TileFrameX, tile.TileFrameY); // This gets the ContainerName text for the currently selected language
				player.cursorItemIconText = Main.chest[chest].name.Length > 0 ? Main.chest[chest].name : defaultName;
				if (player.cursorItemIconText == defaultName) {
					player.cursorItemIconID = ModContent.ItemType<Items.Placeable.Furniture.ExampleChest>();
					if (Main.tile[left, top].TileFrameX / 36 == 1) {
						player.cursorItemIconID = ModContent.ItemType<ExampleChestKey>();
					}

					player.cursorItemIconText = "";
				}
			}

			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
		}

		public override void MouseOverFar(int i, int j) {
			MouseOver(i, j);
			Player player = Main.LocalPlayer;
			if (player.cursorItemIconText == "") {
				player.cursorItemIconEnabled = false;
				player.cursorItemIconID = ItemID.None;
			}
		}
	}
}