using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using System.IO;
using Terraria.ModLoader.IO;
using System.Linq;

namespace ExampleMod.Tiles
{
	public class ScoreBoardGlobalNPC : GlobalNPC
	{
		public override void NPCLoot(NPC npc)
		{
			if (npc.lastInteraction == 255)
			{
				//Main.NewText("Accidental Death, score unchanged");
				return;
			}
			int ID = mod.GetTileEntity<TEScoreBoard>().type;
			foreach (TileEntity current in TileEntity.ByID.Values)
			{
				if (current.type == ID)
				{
					var scoreboard = current as TEScoreBoard;
					int x = (int)Math.Abs(npc.Center.X / 16 - current.Position.X);
					int y = (int)Math.Abs(npc.Center.Y / 16 - current.Position.Y);
					if (x < TEScoreBoard.range && y < TEScoreBoard.range)
					{
						//Main.NewText("In range, score + 1");

						Player p = Main.player[npc.lastInteraction];
						int score = 0;
						scoreboard.scores.TryGetValue(p.name, out score);
						scoreboard.scores[p.name] = score + 1;
						if (Main.dedServ)
						{
							NetMessage.SendData(25, -1, -1, p.name + ": " + scoreboard.scores[p.name], 255, 255,255,255, 0);
						}
						else
						{
							Main.NewText(p.name + ": " + scoreboard.scores[p.name]);
						}
						scoreboard.scoresChanged = true;
					}
				}
			}
		}
	}

	// TODO, reset scores option, draw competition rectangle. 
	// TODO: NetSend not sent along with CompressedTileBlock
	public class TEScoreBoard : ModTileEntity
	{
		internal const int range = 100;
		internal Dictionary<string, int> scores = new Dictionary<string, int>();
		internal bool scoresChanged = false;

		public override void Update()
		{
			if (scoresChanged)
			{
				// Sending 86 aka, TileEntitySharing, triggers NetSend. Think of it like manually calling sync.
				NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, "", ID, Position.X, Position.Y);
				scoresChanged = false;
			}
		}

		public override void NetReceive(BinaryReader reader)
		{
			scores.Clear();
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string name = reader.ReadString();
				int score = reader.ReadInt32();
				scores[name] = score;
			}
		}

		public override void NetSend(BinaryWriter writer)
		{
			writer.Write(scores.Keys.Count);
			foreach (var item in scores)
			{
				writer.Write(item.Key);
				writer.Write(item.Value);
			}
		}

		public override TagCompound Save()
		{
			return new TagCompound
			{
				{"scoreNames", scores.Keys.ToList()},
				{"scoreValues", scores.Values.ToList()}
			};
		}

		public override void Load(TagCompound tag)
		{
			var names = tag.Get<List<string>>("scoreNames");
			var values = tag.Get<List<int>>("scoreValues");
			scores = names.Zip(values, (k, v) => new { Key = k, Value = v }).ToDictionary(x => x.Key, x => x.Value);
		}

		public override bool ValidTile(int i, int j)
		{
			Tile tile = Main.tile[i, j];
			return tile.active() && tile.type == mod.TileType<ScoreBoard>() && tile.frameX == 0 && tile.frameY == 0;
		}

		public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction)
		{
			//Main.NewText("i " + i + " j " + j + " t " + type + " s " + style + " d " + direction);
			if (Main.netMode == 1)
			{
				NetMessage.SendTileSquare(Main.myPlayer, i, j, 3);
				NetMessage.SendData(87, -1, -1, "", i, j, Type, 0f, 0, 0, 0);
				return -1;
			}
			return Place(i, j);
		}
	}

	public class ScoreBoard : ModTile
	{
		// TODO, outline sprites.
		public override void SetDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			// We set processedCoordinates to true so our Hook_AfterPlacement gets top left coordinates, regardless of Origin.
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(mod.GetTileEntity<TEScoreBoard>().Hook_AfterPlacement, -1, 0, true);
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
			AddMapEntry(new Color(26, 127, 206), "ScoreBoard");
			disableSmartCursor = true; //?
									   //TODO	Main.highlightMaskTexture[Type] = mod.GetTexture("Tiles/ScoreBoard_Outline");
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(i * 16, j * 16, 32, 48, mod.ItemType<Items.Placeable.ScoreBoard>());
			mod.GetTileEntity<TEScoreBoard>().Kill(i, j);
		}

		public override void RightClick(int i, int j)
		{
			Tile tile = Main.tile[i, j];
			int left = i - (tile.frameX / 18);
			int top = j - (tile.frameY / 18);

			int index = mod.GetTileEntity<TEScoreBoard>().Find(left, top);
			if (index == -1)
			{
				return;
			}
			Main.NewText("Scores:");
			TEScoreBoard tEElementalPurge = (TEScoreBoard)TileEntity.ByID[index];
			foreach (var item in tEElementalPurge.scores)
			{
				Main.NewText(item.Key + ": " + item.Value);
			}
		}

		public override void MouseOver(int i, int j)
		{
			MouseOverBoth(i, j);
		}

		public override void MouseOverFar(int i, int j)
		{
			MouseOverBoth(i, j);
		}

		public void MouseOverBoth(int i, int j)
		{
			Tile tile = Main.tile[i, j];
			int left = i - (tile.frameX % 36 / 18);
			int top = j - (tile.frameY % 36 / 18);
			Main.signBubble = true;
			Main.signX = left * 16 + 16;
			Main.signY = top * 16;
		}
	}
}