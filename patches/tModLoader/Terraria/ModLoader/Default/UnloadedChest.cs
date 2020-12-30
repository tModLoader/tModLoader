using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;


namespace Terraria.ModLoader.Default
{
	public class UnloadedChest : ModTile
	{
		public override string Name { get; }

		public override string Texture => "ModLoader/UnloadedChest";

		public UnloadedChest(string name = null) {
			Name = name ?? base.Name;
		}

		public override void SetDefaults() {
			// Properties
			Main.tileSpelunker[Type] = true;
			Main.tileContainer[Type] = true;
			Main.tileShine2[Type] = true;
			Main.tileShine[Type] = 1200;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileOreFinderPriority[Type] = 500;
			TileID.Sets.BasicChest[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;

			adjTiles = new int[] { TileID.Containers };

			// Names
			ContainerName.SetDefault("UnloadedChest");

			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Unloaded Chest");
			AddMapEntry(new Color(200, 200, 200), name, MapChestName);

			name = CreateMapEntryName(Name + "_Locked"); // With multiple map entries, you need unique translation keys.
			name.SetDefault("Locked Unloaded Chest");
			AddMapEntry(new Color(0, 141, 63), name, MapChestName);

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Origin = new Point16(0, 0);
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
			TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(Chest.FindEmptyChest, -1, 0, true);
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(Chest.AfterPlacement_Hook, -1, 0, false);
			TileObjectData.newTile.AnchorInvalidTiles = new int[] { TileID.MagicalIceBlock };
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
			TileObjectData.addTile(Type);
		}

		public override ushort GetMapOption(int i, int j) => (ushort)(Main.tile[i, j].frameX / 36);

		public override bool HasSmartInteract() => true;

		public static string MapChestName(string name, int i, int j) {
			int left = i;
			int top = j;
			Tile tile = Main.tile[i, j];
			if (tile.frameX % 36 != 0) {
				left--;
			}

			if (tile.frameY != 0) {
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

		public override void NumDust(int i, int j, bool fail, ref int num) => num = 1;

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Chest.DestroyChest(i, j);
		}

		public override bool RightClick(int i, int j) {
			Player player = Main.LocalPlayer;
			Tile tile = Main.tile[i, j];
			Main.mouseRightRelease = false;
			int left = i;
			int top = j;
			if (tile.frameX % 36 != 0) {
				left--;
			}

			if (tile.frameY != 0) {
				top--;
			}

			if (player.sign >= 0) {
				SoundEngine.PlaySound(SoundID.MenuClose);
				player.sign = -1;
				Main.editSign = false;
				Main.npcChatText = "";
			}

			if (Main.editChest) {
				SoundEngine.PlaySound(SoundID.MenuTick);
				Main.editChest = false;
				Main.npcChatText = "";
			}

			if (player.editedChestName) {
				NetMessage.SendData(MessageID.SyncPlayerChest, -1, -1, NetworkText.FromLiteral(Main.chest[player.chest].name), player.chest, 1f);
				player.editedChestName = false;
			}

			int chest = Chest.FindChest(left, top);
			if (chest >= 0) {
				Main.stackSplit = 600;
				if (chest == player.chest) {
					player.chest = -1;
					SoundEngine.PlaySound(SoundID.MenuClose);
				}
				else {
					player.chest = chest;
					Main.playerInventory = true;
					Main.recBigList = false;
					player.chestX = left;
					player.chestY = top;
					SoundEngine.PlaySound(player.chest < 0 ? SoundID.MenuOpen : SoundID.MenuTick);
				}

				Recipe.FindRecipes();
			}
			
			return true;
		}

		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;
			Tile tile = Main.tile[i, j];
			int left = i;
			int top = j;
			if (tile.frameX % 36 != 0) {
				left--;
			}

			if (tile.frameY != 0) {
				top--;
			}

			if (tile != null && tile.type == Type) {
				int PosID = top * Main.maxTilesX + left;
				ModContent.GetInstance<UnloadedTilesWorld>().chestCoordsToChestInfos.TryGetValue(PosID, out int frameID);
				var infos = ModContent.GetInstance<UnloadedTilesWorld>().chestInfos;
				if (frameID >= 0 && frameID < infos.Count) { // This only works in SP
					var info = infos[frameID];
					if (info != null) {
						player.cursorItemIconEnabled = true;
						player.cursorItemIconID = -1;
						player.cursorItemIconText = $"{info.modName}: {info.name}";
					}
				}
				if (Main.tile[left, top].frameX / 36 == 1) {
					player.cursorItemIconID = Terraria.ID.ItemID.BoneKey;
				}
			}

			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
		}

		public override void MouseOverFar(int i, int j) {
			MouseOver(i, j);
			Player player = Main.LocalPlayer;
			var tile = Main.tile[i, j];
			if(tile != null && tile.type == Type) {
				int PosID = j * Main.maxTilesX + i;
				ModContent.GetInstance<UnloadedTilesWorld>().chestCoordsToChestInfos.TryGetValue(PosID, out int frameID);
				var infos = ModContent.GetInstance<UnloadedTilesWorld>().chestInfos;
				if (frameID >= 0 && frameID < infos.Count) { // This only works in SP
					var info = infos[frameID];
					if (info != null) {
						player.cursorItemIconEnabled = true;
						player.cursorItemIconID = -1;
						player.cursorItemIconText = $"{info.modName}: {info.name}";
					}
				}
			}
		}
	}
}