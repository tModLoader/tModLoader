using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExampleMod.Tiles
{
	public class ExampleStatue : ModTile
	{
		public override void SetDefaults()
		{
			// For some reason, setting tileFrameImportant will cause world gen to fail. Stack Overflow
			//Main.tileFrameImportant[Type] = true;

			Main.tileObsidianKill[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.addTile(Type);
			AddMapEntry(new Color(190, 230, 190), "Party Zombie Statue");
			dustType = 11;
			disableSmartCursor = true;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(i * 16, j * 16, 32, 48, mod.ItemType("ExampleStatueItem"));
		}

		public override void HitWire(int i, int j)
		{
			// Find the coordinates of top left tile square through math
			int y = j - (Main.tile[i, j].frameY / 18);
			int x = i - (Main.tile[i, j].frameX / 18);

			Wiring.SkipWire(x, y);
			Wiring.SkipWire(x, y + 1);
			Wiring.SkipWire(x, y + 2);
			Wiring.SkipWire(x + 1, y);
			Wiring.SkipWire(x + 1, y + 1);
			Wiring.SkipWire(x + 1, y + 2);

			int spawnX = x * 16 + 16;
			int spawnY = (y + 3) * 16;
			int npcIndex = -1;
			if (Wiring.CheckMech(x, y, 30) && NPC.MechSpawn((float)spawnX, (float)spawnY, mod.NPCType("PartyZombie")))
			{
				npcIndex = NPC.NewNPC(spawnX, spawnY - 12, mod.NPCType("PartyZombie"));
			}
			if (npcIndex >= 0)
			{
				Main.npc[npcIndex].value = 0f;
				Main.npc[npcIndex].npcSlots = 0f;
			}
		}
	}

	public class ExampleStatueItem : ModItem
	{
		public override void SetDefaults()
		{
			item.CloneDefaults(ItemID.ArmorStatue);
			item.createTile = mod.TileType("ExampleStatue");
			item.placeStyle = 0;
		}
	}

	public class ExampleStatueModWorld : ModWorld
	{
		public override void PreWorldGen()
		{
			WorldGen.SetupStatueList();
			if (WorldGen.statueList.Any(point => point.X == mod.TileType("ExampleStatue")))
			{
				return;
			}
			Array.Resize(ref WorldGen.statueList, WorldGen.statueList.Length + 50);
			for (int i = WorldGen.statueList.Length - 50; i < WorldGen.statueList.Length; i++)
			{
				WorldGen.statueList[i] = new Point16(mod.TileType("ExampleStatue"), 0);
				WorldGen.StatuesWithTraps.Add(i);
			}
		}
	}
}
