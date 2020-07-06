using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Tiles
{
	public class ScoreBoardGlobalNPC : GlobalNPC
	{
		public override void NPCLoot(NPC npc) {
			if (npc.lastInteraction == 255) {
				//Main.NewText("Accidental Death, score unchanged");
				return;
			}
			int TEScoreBoardType = TileEntityType<TEScoreBoard>();
			foreach (TileEntity current in TileEntity.ByID.Values) {
				if (current.type == TEScoreBoardType) {
					//QuickBox is a neat tool for visualizing things while modding.
					//Dust.QuickBox(npc.position, npc.position + new Vector2(npc.width, npc.height), 1, Color.White, null);
					var scoreboard = current as TEScoreBoard;
					if (scoreboard.GetPlayArea().Intersects(npc.getRect())) {
						Player scoringPlayer = Main.player[npc.lastInteraction];
						int score = 0;
						// Using HalfVector2 and ReinterpretCast.UIntAsFloat is a way to pack a Vector2 into a single float variable.
						HalfVector2 halfVector = new HalfVector2((current.Position.X + 1) * 16, (current.Position.Y + 1) * 16);
						Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileType<Projectiles.ScorePoint>(), 0, 0, Main.myPlayer, ReLogic.Utilities.ReinterpretCast.UIntAsFloat(halfVector.PackedValue), npc.lastInteraction);
						scoreboard.scores.TryGetValue(scoringPlayer.name, out score);
						scoreboard.scores[scoringPlayer.name] = score + 1;
						if (Main.dedServ) {
							NetworkText text = NetworkText.FromFormattable("{0}: {1}", scoringPlayer.name, scoreboard.scores[scoringPlayer.name]);
							NetMessage.BroadcastChatMessage(text, Color.White);
						}
						else {
							Main.NewText(scoringPlayer.name + ": " + scoreboard.scores[scoringPlayer.name]);
						}
						scoreboard.scoresChanged = true;
					}
				}
			}
		}
	}

	// TODO, reset scores option
	public class TEScoreBoard : ModTileEntity
	{
		// Half the width in Tile Coordinates.
		internal const int range = 50;
		internal Dictionary<string, int> scores = new Dictionary<string, int>();
		internal bool scoresChanged;
		internal const int drawBorderWidth = 5;


		/// <summary>
		/// Returns a rectangle representing the play area in World coordinates.
		/// </summary>
		public Rectangle GetPlayArea() {
			return new Rectangle((Position.X + 1) * 16 - range * 16, (Position.Y + 1) * 16 - range * 16, range * 16 * 2, range * 16 * 2);
		}

		public override void Update() {
			if (scoresChanged) {
				// Sending 86 aka, TileEntitySharing, triggers NetSend. Think of it like manually calling sync.
				NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
				scoresChanged = false;
			}
		}

		public override void NetReceive(BinaryReader reader, bool lightReceive) {
			scores.Clear();
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++) {
				string name = reader.ReadString();
				int score = reader.ReadInt32();
				scores[name] = score;
			}
		}

		public override void NetSend(BinaryWriter writer, bool lightSend) {
			writer.Write(scores.Keys.Count);
			foreach (var item in scores) {
				writer.Write(item.Key);
				writer.Write(item.Value);
			}
		}

		public override TagCompound Save() {
			return new TagCompound
			{
				{"scoreNames", scores.Keys.ToList()},
				{"scoreValues", scores.Values.ToList()}
			};
		}

		public override void Load(TagCompound tag) {
			var names = tag.Get<List<string>>("scoreNames");
			var values = tag.Get<List<int>>("scoreValues");
			scores = names.Zip(values, (k, v) => new { Key = k, Value = v }).ToDictionary(x => x.Key, x => x.Value);
		}

		public override bool ValidTile(int i, int j) {
			Tile tile = Main.tile[i, j];
			return tile.active() && tile.type == TileType<ScoreBoard>() && tile.frameX == 0 && tile.frameY == 0;
		}

		public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction) {
			//Main.NewText("i " + i + " j " + j + " t " + type + " s " + style + " d " + direction);
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				NetMessage.SendTileSquare(Main.myPlayer, i, j, 3);
				NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type, 0f, 0, 0, 0);
				return -1;
			}
			return Place(i, j);
		}
	}

	public class ScoreBoard : ModTile
	{
		// TODO, outline sprites.
		public override void SetDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.FramesOnKillWall[Type] = true; // Necessary since we have a placement that uses AnchorWall

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			// We set processedCoordinates to true so our Hook_AfterPlacement gets top left coordinates, regardless of Origin.
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(GetInstance<TEScoreBoard>().Hook_AfterPlacement, -1, 0, true);
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.StyleMultiplier = 5;
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.addAlternate(0);
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Origin = Point16.Zero;
			TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, 2, 0);
			TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
			TileObjectData.addAlternate(1);
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Origin = new Point16(0, 0);
			TileObjectData.newAlternate.AnchorLeft = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, 2, 0);
			TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
			TileObjectData.addAlternate(2);
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Origin = new Point16(1, 0);
			TileObjectData.newAlternate.AnchorRight = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, 2, 0);
			TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
			TileObjectData.addAlternate(3);
			TileObjectData.newTile.Origin = new Point16(0, 1);
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Origin = Point16.Zero;
			TileObjectData.newAlternate.AnchorWall = true;
			TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
			TileObjectData.addAlternate(4);
			TileObjectData.addTile(Type);
			ModTranslation name = CreateMapEntryName();
			name.SetDefault("ScoreBoard");
			AddMapEntry(new Color(26, 127, 206), name);
			disableSmartCursor = true; //?
									//TODO	Main.highlightMaskTexture[Type] = mod.GetTexture("Tiles/ScoreBoard_Outline");
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			Item.NewItem(i * 16, j * 16, 32, 48, ItemType<Items.Placeable.ScoreBoard>());
			GetInstance<TEScoreBoard>().Kill(i, j);
		}

		public override bool NewRightClick(int i, int j) {
			Tile tile = Main.tile[i, j];
			int left = i - tile.frameX % 36 / 18;
			int top = j - tile.frameY / 18;

			int index = GetInstance<TEScoreBoard>().Find(left, top);
			if (index == -1) {
				return false;
			}
			Main.NewText("Scores:");
			TEScoreBoard tEScoreBoard = (TEScoreBoard)TileEntity.ByID[index];
			foreach (var item in tEScoreBoard.scores) {
				Main.NewText(item.Key + ": " + item.Value);
			}
			return true;
		}

		public override void MouseOver(int i, int j) {
			MouseOverBoth(i, j);
		}

		public override void MouseOverFar(int i, int j) {
			MouseOverBoth(i, j);
		}

		public void MouseOverBoth(int i, int j) {
			Tile tile = Main.tile[i, j];
			int left = i - tile.frameX % 36 / 18;
			int top = j - tile.frameY % 36 / 18;
			Main.signBubble = true;
			Main.signX = left * 16 + 16;
			Main.signY = top * 16;
		}
	}
}