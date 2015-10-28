using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.DataStructures;

namespace ExampleMod.Tiles
{
	class ExampleMusicBox : ModTile
	{
		public override void SetDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileObsidianKill[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Origin = new Point16(0, 1);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.addTile(Type/*139*/);
			AddMapEntry(new Color(200, 200, 200), "Music Box");
		}

		public override void NearbyEffects(int i, int j, bool closer)
		{
			if (Main.tile[i, j].frameX >= 36)
			{
				Main.musicBox = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/DriveMusic");
			}
		}

		public override void HitWire(int i, int j)
		{
			int k;
			for (k = (int)(Main.tile[i, j].frameY / 18); k >= 2; k -= 2)
			{
			}
			int num = (int)(Main.tile[i, j].frameX / 18);
			if (num >= 2)
			{
				num -= 2;
			}
			int num2 = i - num;
			int num3 = j - k;
			for (int l = num2; l < num2 + 2; l++)
			{
				for (int m = num3; m < num3 + 2; m++)
				{
					if (Main.tile[l, m] == null)
					{
						Main.tile[l, m] = new Tile();
					}
					if (Main.tile[l, m].active() /*&& Main.tile[l, m].type == 139)*/)
					{
						if (Main.tile[l, m].frameX < 36)
						{
							Main.tile[l, m].frameX += 36;
						}
						else
						{
							Main.tile[l, m].frameX -= 36;
						}
					}
				}
			}
			if (Wiring.running)
			{
				Wiring.SkipWire(num2, num3);
				Wiring.SkipWire(num2 + 1, num3);
				Wiring.SkipWire(num2, num3 + 1);
				Wiring.SkipWire(num2 + 1, num3 + 1);
			}
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(i * 16, j * 16, 16, 48, mod.ItemType("ExampleMusicBox"));
		}

		public override void RightClick(int i, int j)
		{
			//if (this.releaseUseTile)
			//{
			Main.PlaySound(28, Player.tileTargetX * 16, Player.tileTargetY * 16, 0);
			int k;
			for (k = (int)(Main.tile[i, j].frameY / 18); k >= 2; k -= 2)
			{
			}
			int num = (int)(Main.tile[i, j].frameX / 18);
			if (num >= 2)
			{
				num -= 2;
			}
			int num2 = i - num;
			int num3 = j - k;
			for (int l = num2; l < num2 + 2; l++)
			{
				for (int m = num3; m < num3 + 2; m++)
				{
					if (Main.tile[l, m] == null)
					{
						Main.tile[l, m] = new Tile();
					}
					if (Main.tile[l, m].active() /*&& Main.tile[l, m].type == 139)*/)
					{
						if (Main.tile[l, m].frameX < 36)
						{
							Main.tile[l, m].frameX += 36;
						}
						else
						{
							Main.tile[l, m].frameX -= 36;
						}
					}
				}
			}
			if (Wiring.running)
			{
				Wiring.SkipWire(num2, num3);
				Wiring.SkipWire(num2 + 1, num3);
				Wiring.SkipWire(num2, num3 + 1);
				Wiring.SkipWire(num2 + 1, num3 + 1);
			}
		}

		public override void MouseOver(int i, int j)
		{
			Player player = Main.player[Main.myPlayer];
			player.noThrow = 2;
			player.showItemIcon = true;
			player.showItemIcon2 = mod.ItemType("ExampleMusicBox");
		}
	}
}
