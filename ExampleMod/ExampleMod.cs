using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ExampleMod.NPCs.PuritySpirit;
using ExampleMod.Tiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using ExampleMod.UI;
using Terraria.UI;
using Terraria.DataStructures;
using Terraria.GameContent.UI;
using System;

namespace ExampleMod
{
	public class ExampleMod : Mod
	{
		internal static ExampleMod instance;
		public const string captiveElementHead = "ExampleMod/NPCs/Abomination/CaptiveElement_Head_Boss_";
		public const string captiveElement2Head = "ExampleMod/NPCs/Abomination/CaptiveElement2_Head_Boss_";
		// public static DynamicSpriteFont exampleFont; With the new fonts in 1.3.5, font files are pretty big now so we have removed this example. You can use https://forums.terraria.org/index.php?threads/dynamicspritefontgenerator-0-4-generate-fonts-without-xna-game-studio.57127/ to make dynamicspritefonts
		public static Effect exampleEffect;
		private UserInterface exampleUserInterface;
		internal ExampleUI exampleUI;
		public static ModHotKey RandomBuffHotKey;
		public static int FaceCustomCurrencyID;

		public ExampleMod()
		{
			Properties = new ModProperties()
			{
				Autoload = true,
				AutoloadGores = true,
				AutoloadSounds = true,
				AutoloadBackgrounds = true
			};
		}

		public override void Load()
		{
			instance = this;

			// Adds boss head textures for the Abomination boss
			for (int k = 1; k <= 4; k++)
			{
				AddBossHeadTexture(captiveElementHead + k);
				AddBossHeadTexture(captiveElement2Head + k);
			}

			// Registers a new hotkey
			RandomBuffHotKey = RegisterHotKey("Random Buff", "P");

			// Registers a new custom currency
			FaceCustomCurrencyID = CustomCurrencyManager.RegisterCurrency(new ExampleCustomCurrency(ItemType<Items.Face>(), 999L));

			// All code below runs only if we're not loading on a server
			if (!Main.dedServ)
			{
				// Add certain equip textures
				AddEquipTexture(null, EquipType.Legs, "ExampleRobe_Legs", "ExampleMod/Items/Armor/ExampleRobe_Legs");
				AddEquipTexture(new Items.Armor.BlockyHead(), null, EquipType.Head, "BlockyHead", "ExampleMod/Items/Armor/ExampleCostume_Head");
				AddEquipTexture(new Items.Armor.BlockyBody(), null, EquipType.Body, "BlockyBody", "ExampleMod/Items/Armor/ExampleCostume_Body", "ExampleMod/Items/Armor/ExampleCostume_Arms");
				AddEquipTexture(new Items.Armor.BlockyLegs(), null, EquipType.Legs, "BlockyLeg", "ExampleMod/Items/Armor/ExampleCostume_Legs");

				// Change the vanilla dungeon track
				// Main.music[MusicID.Dungeon] = GetMusic("Sounds/Music/DriveMusic");

				// Register a new music box
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/DriveMusic"), ItemType("ExampleMusicBox"), TileType("ExampleMusicBox"));

				// Change the vailla loom texture
				Main.instance.LoadTiles(TileID.Loom); // First load the tile texture
				Main.tileTexture[TileID.Loom] = GetTexture("Tiles/AnimatedLoom"); // Now we change it

				//What if....Replace a vanilla item texture and equip texture.
				//Main.itemTexture[ItemID.CopperHelmet] = GetTexture("Resprite/CopperHelmet_Item");
				//Item copperHelmet = new Item();
				//copperHelmet.SetDefaults(ItemID.CopperHelmet);
				//Main.armorHeadLoaded[copperHelmet.headSlot] = true;
				//Main.armorHeadTexture[copperHelmet.headSlot] = GetTexture("Resprite/CopperHelmet_Head");

				// Create new skies and screen filters
				Filters.Scene["ExampleMod:PuritySpirit"] = new Filter(new PuritySpiritScreenShaderData("FilterMiniTower").UseColor(0.4f, 0.9f, 0.4f).UseOpacity(0.7f), EffectPriority.VeryHigh);
				SkyManager.Instance["ExampleMod:PuritySpirit"] = new PuritySpiritSky();
				Filters.Scene["ExampleMod:MonolithVoid"] = new Filter(new ScreenShaderData("FilterMoonLord"), EffectPriority.Medium);
				SkyManager.Instance["ExampleMod:MonolithVoid"] = new VoidSky();
				// exampleFont = GetFont("Fonts/ExampleFont"); 
				exampleEffect = GetEffect("Effects/ExampleEffect");
				Ref<Effect> exampleEffectRef = new Ref<Effect>();
				exampleEffectRef.Value = exampleEffect;
				GameShaders.Armor.BindShader<ArmorShaderData>(ItemType<Items.ExampleDye>(), new ArmorShaderData(exampleEffectRef, "ExampleDyePass"));

				// Custom UI
				exampleUI = new ExampleUI();
				exampleUI.Activate();
				exampleUserInterface = new UserInterface();
				exampleUserInterface.SetState(exampleUI);
			}

			// Register custom mod translations, lives left is for Spirit of Purity
			ModTranslation text = CreateTranslation("LivesLeft");
			text.SetDefault("{0} has {1} lives left!");
			AddTranslation(text);
			text = CreateTranslation("LifeLeft");
			text.SetDefault("{0} has 1 life left!");
			AddTranslation(text);
			text = CreateTranslation("NPCTalk");
			text.SetDefault("<{0}> {1}");
			AddTranslation(text);

			// Volcano warning is for the random vulcano tremor
			text = CreateTranslation("VolcanoWarning");
			text.SetDefault("Did you hear something....A Volcano! Find Cover!");
			AddTranslation(text);
		}

		public override void Unload()
		{
			// All code below runs only if we're not loading on a server
			if (!Main.dedServ)
			{
				// Main.music[MusicID.Dungeon] = Main.soundBank.GetCue("Music_" + MusicID.Dungeon);
				Main.tileFrame[TileID.Loom] = 0; // Reset the frame of the loom tile
				Main.tileSetsLoaded[TileID.Loom] = false; // Causes the loom tile to reload its vanilla texture
			}

			// Unload static references
			// You need to clear static references to assets (Texture2D, SoundEffects, Effects). 
			exampleEffect = null;

			// In addition to that, if you want your mod to completely unload during unload, you need to clear static references to anything referencing your Mod class
			instance = null;
			RandomBuffHotKey = null;
		}

		public override void PostSetupContent()
		{
			// Showcases mod support with Boss Checklist without referencing the mod
			Mod bossChecklist = ModLoader.GetMod("BossChecklist");
			if (bossChecklist != null)
			{
				bossChecklist.Call("AddBossWithInfo", "Abomination", 5.5f, (Func<bool>)(() => ExampleWorld.downedAbomination), "Use a [i:" + ItemType<Items.Abomination.FoulOrb>() + "] in the underworld after Pletera has been defeated");
				bossChecklist.Call("AddBossWithInfo", "Purity Spirit", 15.5f, (Func<bool>)(() => ExampleWorld.downedPuritySpirit), "Kill a [i:" + ItemID.Bunny + "] in front of [i:" + ItemType<Items.Placeable.ElementalPurge>() + "]");
			}
		}

		public override void AddRecipeGroups()
		{
			// Creates a new recipe group
			RecipeGroup group = new RecipeGroup(() => Lang.misc[37] + " " + Lang.GetItemNameValue(ItemType("ExampleItem")), new int[]
			{
				ItemType("ExampleItem"),
				ItemType("EquipMaterial"),
				ItemType("BossItem")
			});
			// Registers the new recipe group with the specified name
			RecipeGroup.RegisterGroup("ExampleMod:ExampleItem", group);
		}

		// Learn how to do Recipes: https://github.com/blushiemagic/tModLoader/wiki/Basic-Recipes 
		public override void AddRecipes()
		{
			// Here is an example of a recipe.
			ModRecipe recipe = new ModRecipe(this);
			recipe.AddIngredient(null, "ExampleItem");
			recipe.SetResult(ItemID.Wood, 999);
			recipe.AddRecipe();

			// To make ExampleMod more organized, the rest of the recipes are added elsewhere, see the method calls below.
			// See RecipeHelper.cs
			RecipeHelper.AddExampleRecipes(this);
			RecipeHelper.ExampleRecipeEditing(this);
		}

		public override void UpdateMusic(ref int music)
		{
			if (Main.myPlayer != -1 && !Main.gameMenu)
			{
				if (Main.LocalPlayer.active
					&& (Main.LocalPlayer.FindBuffIndex(BuffType("CarMount")) != -1 || Main.LocalPlayer.GetModPlayer<ExamplePlayer>(this).ZoneExample))
				{
					music = GetSoundSlot(SoundType.Music, "Sounds/Music/DriveMusic");
				}
			}
		}

		const int ShakeLength = 5;
		int ShakeCount = 0;
		float previousRotation = 0;
		float targetRotation = 0;
		float previousOffsetX = 0;
		float previousOffsetY = 0;
		float targetOffsetX = 0;
		float targetOffsetY = 0;

		// Volcano Tremor
		public override Matrix ModifyTransformMatrix(Matrix Transform)
		{
			if (!Main.gameMenu)
			{
				ExampleWorld world = GetModWorld<ExampleWorld>();
				if (world.VolcanoTremorTime > 0)
				{
					if (world.VolcanoTremorTime % ShakeLength == 0)
					{
						ShakeCount = 0;
						previousRotation = targetRotation;
						previousOffsetX = targetOffsetX;
						previousOffsetY = targetOffsetY;
						targetRotation = (Main.rand.NextFloat() - .5f) * MathHelper.ToRadians(15);
						targetOffsetX = Main.rand.Next(60) - 30;
						targetOffsetY = Main.rand.Next(40) - 20;
						if (world.VolcanoTremorTime == ShakeLength)
						{
							targetRotation = 0;
							targetOffsetX = 0;
							targetOffsetY = 0;
						}
					}
					float transX = Main.screenWidth / 2;
					float transY = Main.screenHeight / 2;

					float lerp = (float)(ShakeCount) / ShakeLength;
					float rotation = MathHelper.Lerp(previousRotation, targetRotation, lerp);
					float offsetX = MathHelper.Lerp(previousOffsetX, targetOffsetX, lerp);
					float offsetY = MathHelper.Lerp(previousOffsetY, targetOffsetY, lerp);

					world.VolcanoTremorTime--;
					ShakeCount++;


					return Transform
						* Matrix.CreateTranslation(-transX, -transY, 0f)
						* Matrix.CreateRotationZ(rotation)
						* Matrix.CreateTranslation(transX, transY, 0f)
						* Matrix.CreateTranslation(offsetX, offsetY, 0f);
					//Matrix.CreateFromAxisAngle(new Vector3(Main.screenWidth / 2, Main.screenHeight / 2, 0f), .2f);
					//Matrix.CreateRotationZ(MathHelper.ToRadians(30));
				}
			}
			return Transform;
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int MouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (MouseTextIndex != -1)
			{
				layers.Insert(MouseTextIndex, new LegacyGameInterfaceLayer(
					"ExampleMod: Coins Per Minute",
					delegate
					{
						if (ExampleUI.visible)
						{
							exampleUserInterface.Update(Main._drawInterfaceGameTime);
							exampleUI.Draw(Main.spriteBatch);
						}
						return true;
					},
					InterfaceScaleType.UI)
				);
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

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			ExampleModMessageType msgType = (ExampleModMessageType)reader.ReadByte();
			switch (msgType)
			{
				// This message sent by the server to initialize the Volcano Tremor on clients
				case ExampleModMessageType.SetTremorTime:
					int tremorTime = reader.ReadInt32();
					ExampleWorld world = GetModWorld<ExampleWorld>();
					world.VolcanoTremorTime = tremorTime;
					break;
				// This message sent by the server to initialize the Volcano Rubble.
				case ExampleModMessageType.VolcanicRubbleMultiplayerFix:
					int numberProjectiles = reader.ReadInt32();
					for (int i = 0; i < numberProjectiles; i++)
					{
						int identity = reader.ReadInt32();
						bool found = false;
						for (int j = 0; j < 1000; j++)
						{
							if (Main.projectile[j].owner == 255 && Main.projectile[j].identity == identity && Main.projectile[j].active)
							{
								Main.projectile[j].hostile = true;
								//Main.projectile[j].name = "Volcanic Rubble";
								found = true;
								break;
							}
						}
						if (!found)
						{
							ErrorLogger.Log("Error: Projectile not found");
						}
					}
					break;
				case ExampleModMessageType.PuritySpirit:
					PuritySpirit spirit = Main.npc[reader.ReadInt32()].modNPC as PuritySpirit;
					if (spirit != null && spirit.npc.active)
					{
						spirit.HandlePacket(reader);
					}
					break;
				case ExampleModMessageType.HeroLives:
					Player player = Main.player[reader.ReadInt32()];
					int lives = reader.ReadInt32();
					player.GetModPlayer<ExamplePlayer>(this).heroLives = lives;
					if (lives > 0)
					{
						NetworkText text;
						if (lives == 1)
						{
							text = NetworkText.FromKey("Mods.ExampleMod.LifeLeft", player.name);
						}
						else
						{
							text = NetworkText.FromKey("Mods.ExampleMod.LivesLeft", player.name, lives);
						}
						NetMessage.BroadcastChatMessage(text, new Color(255, 25, 25));
					}
					break;
				default:
					ErrorLogger.Log("ExampleMod: Unknown Message type: " + msgType);
					break;
			}
		}
	}

	enum ExampleModMessageType : byte
	{
		SetTremorTime,
		VolcanicRubbleMultiplayerFix,
		PuritySpirit,
		HeroLives
	}

	/*public static class ExampleModExtensions
	{
		public static int CountItem(this Player player, int type)
		{
			int count = 0;
			for (int i = 0; i < 58; i++)
			{
				if (type == player.inventory[i].type && player.inventory[i].stack > 0)
				{
					count += player.inventory[i].stack;
				}
			}
			return count;
		}
	}*/
}

