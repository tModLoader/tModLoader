using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExampleMod.Items;
using ExampleMod.Items.Armor;
using ExampleMod.NPCs;

namespace ExampleMod
{
	public class ExampleMod : Mod
	{
		public const string captiveElementHead = "ExampleMod/NPCs/Abomination/CaptiveElement_Head_Boss_";
		public const string captiveElement2Head = "ExampleMod/NPCs/Abomination/CaptiveElement2_Head_Boss_";
		private double pressedRandomBuffHotKeyTime;

		public override void SetModInfo(out string name, ref ModProperties properties)
		{
			name = "ExampleMod";
			properties.Autoload = true;
			properties.AutoloadGores = true;
			properties.AutoloadSounds = true;
		}

		public override void Load()
		{
			for (int k = 1; k <= 4; k++)
			{
				AddBossHeadTexture(captiveElementHead + k);
				AddBossHeadTexture(captiveElement2Head + k);
			}
			RegisterHotKey("Random Buff", "P");
			Main.music[MusicID.Dungeon].ModMusic = GetSound("Sounds/Music/ExampleMusic").CreateInstance();
			AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/DriveMusic"), ItemType("ExampleMusicBox"), TileType("ExampleMusicBox"));
			Main.instance.LoadTiles(TileID.Loom);
			Main.tileTexture[TileID.Loom] = GetTexture("Tiles/AnimatedLoom");
		}

		public override void Unload()
		{
			Main.music[MusicID.Dungeon].ModMusic = null;
			Main.tileFrame[TileID.Loom] = 0;
			Main.tileSetsLoaded[TileID.Loom] = false;
		}

		public override void AddCraftGroups()
		{
			AddCraftGroup("ExampleItem", Lang.misc[37] + " " + GetItem("ExampleItem").item.name,
				ItemType("ExampleItem"), ItemType("EquipMaterial"), ItemType("BossItem"));
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(this);
			recipe.AddIngredient(null, "ExampleItem");
			recipe.SetResult(ItemID.Wood, 999);
			recipe.AddRecipe();
			recipe = new ModRecipe(this);
			recipe.AddIngredient(null, "ExampleItem");
			recipe.SetResult(ItemID.Silk, 999);
			recipe.AddRecipe();
			recipe = new ModRecipe(this);
			recipe.AddIngredient(null, "ExampleItem");
			recipe.SetResult(ItemID.IronOre, 999);
			recipe.AddRecipe();
			recipe = new ModRecipe(this);
			recipe.AddIngredient(null, "ExampleItem");
			recipe.SetResult(ItemID.GravitationPotion, 20);
			recipe.AddRecipe();
			recipe = new ModRecipe(this);
			recipe.AddIngredient(null, "ExampleItem");
			recipe.SetResult(ItemID.GoldChest);
			recipe.AddRecipe();
			recipe = new ModRecipe(this);
			recipe.AddIngredient(null, "ExampleItem");
			recipe.SetResult(ItemID.MusicBoxDungeon);
			recipe.AddRecipe();
			RecipeHelper.AddBossRecipes(this);
		}

		public override void UpdateMusic(ref int music)
		{
			if (Main.myPlayer != -1 && !Main.gameMenu)
			{
				if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].HasBuff(this.BuffType("CarMount")) != -1)
				{
					music = this.GetSoundSlot(SoundType.Music, "Sounds/Music/DriveMusic");
				}
			}
		}

		public override void HotKeyPressed(string name)
		{
			if (name == "Random Buff")
			{
				if (Math.Abs(Main.time - pressedRandomBuffHotKeyTime) > 60)
				{
					pressedRandomBuffHotKeyTime = Main.time;
					int buff = Main.rand.Next(BuffID.Count);
					Main.player[Main.myPlayer].AddBuff(buff, 600);
				}
			}
		}

		public override void ChatInput(string text)
		{
			if (text[0] != '/')
			{
				return;
			}
			text = text.Substring(1);
			int index = text.IndexOf(' ');
			string command;
			string[] args;
			if (index < 0)
			{
				command = text;
				args = new string[0];
			}
			else
			{
				command = text.Substring(0, index);
				args = text.Substring(index + 1).Split(' ');
			}
			if (command == "npc")
			{
				NPCCommand(args);
			}
			else if (command == "npcType")
			{
				NPCTypeCommand(args);
			}
			else if (command == "addTime")
			{
				AddTimeCommand(args);
			}
			else if (command == "item")
			{
				ItemCommand(args);
			}
		}

		private void NPCCommand(string[] args)
		{
			int type;
			if (args.Length == 0 || !Int32.TryParse(args[0], out type))
			{
				Main.NewText("Usage: /npc type [x] [y] [number]");
				Main.NewText("x and y may be preceded by ~ to use position relative to player");
				return;
			}
			try
			{
				Player player = Main.player[Main.myPlayer];
				int x;
				int y;
				int num = 1;
				if (args.Length > 2)
				{
					bool relativeX = false;
					bool relativeY = false;
					if (args[1][0] == '~')
					{
						relativeX = true;
						args[1] = args[1].Substring(1);
					}
					if (args[2][0] == '~')
					{
						relativeY = true;
						args[2] = args[2].Substring(1);
					}
					if (!Int32.TryParse(args[1], out x))
					{
						x = 0;
						relativeX = true;
					}
					if (!Int32.TryParse(args[2], out y))
					{
						y = 0;
						relativeY = true;
					}
					if (relativeX)
					{
						x += (int)player.Bottom.X;
					}
					if (relativeY)
					{
						y += (int)player.Bottom.Y;
					}
					if (args.Length > 3)
					{
						if (!Int32.TryParse(args[3], out num))
						{
							num = 1;
						}
					}
				}
				else
				{
					x = (int)player.Bottom.X;
					y = (int)player.Bottom.Y;
				}
				for (int k = 0; k < num; k++)
				{
					NPC.NewNPC(x, y, type);
				}
			}
			catch
			{
				Main.NewText("Usage: /npc type [x] [y] [number]");
				Main.NewText("x and y may be preceded by ~ to use position relative to player");
			}
		}

		private void NPCTypeCommand(string[] args)
		{
			if (args.Length < 2)
			{
				Main.NewText("Usage: /npcType modName npcName");
				return;
			}
			Mod mod = ModLoader.GetMod(args[0]);
			int type = mod == null ? 0 : mod.NPCType(args[1]);
			Main.NewText(type.ToString(), 255, 255, 0);
		}

		private void AddTimeCommand(string[] args)
		{
			int amount;
			if (args.Length == 0 || !Int32.TryParse(args[0], out amount))
			{
				Main.NewText("Usage: /addTime numTicks");
				return;
			}
			Main.time += amount;
		}

		private void ItemCommand(string[] args)
		{
			if (args.Length == 0)
			{
				Main.NewText("Usage: /item [type|name] [stack]");
				return;
			}
			try
			{
				Player player = Main.player[Main.myPlayer];
				int type;
				if (!Int32.TryParse(args[0], out type))
				{
					args[0] = args[0].Replace("_", " ");
					for (int k = 0; k < Main.itemName.Length; k++)
					{
						if (args[0] == Main.itemName[k])
						{
							type = k;
							break;
						}
					}
				}
				int stack;
				if (args.Length < 2 || !Int32.TryParse(args[1], out stack))
				{
					stack = 1;
				}
				Item.NewItem((int)player.position.X, (int)player.position.Y, player.width, player.height, type, stack);
			}
			catch
			{
				Main.NewText("Usage: /item [type|name] [stack]");
			}
		}
		//spawning helper methods imported from my tAPI mod
		public static bool NoInvasion(NPCSpawnInfo spawnInfo)
		{
			return !spawnInfo.invasion && ((!Main.pumpkinMoon && !Main.snowMoon) || spawnInfo.spawnTileY > Main.worldSurface || Main.dayTime) && (!Main.eclipse || spawnInfo.spawnTileY > Main.worldSurface || !Main.dayTime);
		}

		public static bool NoBiome(NPCSpawnInfo spawnInfo)
		{
			Player player = spawnInfo.player;
			return !player.ZoneJungle && !player.ZoneDungeon && !player.ZoneCorrupt && !player.ZoneCrimson && !player.ZoneHoly && !player.ZoneSnow && !player.ZoneUndergroundDesert;
		}

		public static bool NoZoneAllowWater(NPCSpawnInfo spawnInfo)
		{
			return !spawnInfo.sky && !spawnInfo.player.ZoneMeteor && !spawnInfo.spiderCave;
		}

		public static bool NoZone(NPCSpawnInfo spawnInfo)
		{
			return NoZoneAllowWater(spawnInfo) && !spawnInfo.water;
		}

		public static bool NormalSpawn(NPCSpawnInfo spawnInfo)
		{
			return !spawnInfo.playerInTown && NoInvasion(spawnInfo);
		}

		public static bool NoZoneNormalSpawn(NPCSpawnInfo spawnInfo)
		{
			return NormalSpawn(spawnInfo) && NoZone(spawnInfo);
		}

		public static bool NoZoneNormalSpawnAllowWater(NPCSpawnInfo spawnInfo)
		{
			return NormalSpawn(spawnInfo) && NoZoneAllowWater(spawnInfo);
		}

		public static bool NoBiomeNormalSpawn(NPCSpawnInfo spawnInfo)
		{
			return NormalSpawn(spawnInfo) && NoBiome(spawnInfo) && NoZone(spawnInfo);
		}
	}
}