using ExampleMod.NPCs.PuritySpirit;
using ExampleMod.Tiles;
using ExampleMod.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.GameContent.Dyes;
using Terraria.GameContent.UI;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

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
		internal UserInterface examplePersonUserInterface;
		internal ExampleUI exampleUI;
		public static ModHotKey RandomBuffHotKey;
		public static int FaceCustomCurrencyID;

		// Your mod instance has a Logger field, use it.
		// OPTIONAL: You can create your own logger this way, recommended is a custom logging class if you do a lot of logging
		// You need to reference the log4net library to do this, this can be found in the tModLoader repository
		// inside the references folder. You do not have to add this to build.txt as tML has it natively.
		// internal ILog Logging = LogManager.GetLogger("ExampleMod");

		public ExampleMod()
		{
			// By default, all Autoload properties are True. You only need to change this if you know what you are doing.
			//Properties = new ModProperties()
			//{
			//	Autoload = true,
			//	AutoloadGores = true,
			//	AutoloadSounds = true,
			//	AutoloadBackgrounds = true
			//};
		}

		public override void Load()
		{
			instance = this;
			// Will show up in client.log under the ExampleMod name
			Logger.InfoFormat("{0} example logging", this.Name);
			// ErrorLogger.Log("blabla"); REPLACE THIS WITH ABOVE

			// Adds boss head textures for the Abomination boss
			for (int k = 1; k <= 4; k++)
			{
				AddBossHeadTexture(captiveElementHead + k);
				AddBossHeadTexture(captiveElement2Head + k);
			}

			// Registers a new hotkey
			RandomBuffHotKey = RegisterHotKey("Random Buff", "P"); // See https://docs.microsoft.com/en-us/previous-versions/windows/xna/bb197781(v%3dxnagamestudio.41) for special keys

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

				// Change the vanilla loom texture
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
				GameShaders.Armor.BindShader(ItemType<Items.ExampleDye>(), new ArmorShaderData(new Ref<Effect>(GetEffect("Effects/ExampleEffect")), "ExampleDyePass"));
				GameShaders.Hair.BindShader(ItemType<Items.ExampleHairDye>(), new LegacyHairShaderData().UseLegacyMethod((Player player, Color newColor, ref bool lighting) => Color.Green));
				GameShaders.Misc["ExampleMod:DeathAnimation"] = new MiscShaderData(new Ref<Effect>(GetEffect("Effects/ExampleEffectDeath")), "DeathAnimation").UseImage("Images/Misc/Perlin");


				// Custom UI
				exampleUI = new ExampleUI();
				exampleUI.Activate();
				exampleUserInterface = new UserInterface();
				exampleUserInterface.SetState(exampleUI);

				// UserInterface can only show 1 UIState at a time. If you want different "pages" for a UI, switch between UIStates on the same UserInterface instance. 
				// We want both the Coin counter and the Example Person UI to be independent and coexist simultaneously, so we have them each in their own UserInterface.
				examplePersonUserInterface = new UserInterface();
				// We will call .SetState later in ExamplePerson.OnChatButtonClicked
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

			// Volcano warning is for the random volcano tremor
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
			RecipeGroup group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemType("ExampleItem")), new int[]
			{
				ItemType("ExampleItem"),
				ItemType("EquipMaterial"),
				ItemType("BossItem")
			});
			// Registers the new recipe group with the specified name
			RecipeGroup.RegisterGroup("ExampleMod:ExampleItem", group);

			// Modifying a vanilla recipe group. Now we can use Lava Snail to craft Snail Statue
			RecipeGroup SnailGroup = RecipeGroup.recipeGroups[RecipeGroup.recipeGroupIDs["Snails"]];
			SnailGroup.ValidItems.Add(ItemType<NPCs.ExampleCritterItem>());
		}

		// Learn how to do Recipes: https://github.com/blushiemagic/tModLoader/wiki/Basic-Recipes 
		public override void AddRecipes()
		{
			// Here is an example of a recipe.
			ModRecipe recipe = new ModRecipe(this);
			recipe.AddIngredient(this.ItemType("ExampleItem"));
			recipe.SetResult(ItemID.Wood, 999);
			recipe.AddRecipe();

			// To make ExampleMod more organized, the rest of the recipes are added elsewhere, see the method calls below.
			// See RecipeHelper.cs
			RecipeHelper.AddExampleRecipes(this);
			RecipeHelper.ExampleRecipeEditing(this);
		}

		public override void UpdateMusic(ref int music, ref MusicPriority priority)
		{
			if (Main.myPlayer != -1 && !Main.gameMenu && Main.LocalPlayer.active)
			{
				// Make sure your logic here goes from lowest priority to highest so your intended priority is maintained.
				if (Main.LocalPlayer.GetModPlayer<ExamplePlayer>().ZoneExample)
				{
					music = GetSoundSlot(SoundType.Music, "Sounds/Music/DriveMusic");
					priority = MusicPriority.BiomeLow;
				}
				if (Main.LocalPlayer.HasBuff(BuffType("CarMount")))
				{
					music = GetSoundSlot(SoundType.Music, "Sounds/Music/DriveMusic");
					priority = MusicPriority.Environment;
				}
			}
		}

		public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
		{
			if (ExampleWorld.exampleTiles > 0)
			{
				float exampleStrength = ExampleWorld.exampleTiles / 200f;
				exampleStrength = Math.Min(exampleStrength, 1f);

				int sunR = backgroundColor.R;
				int sunG = backgroundColor.G;
				int sunB = backgroundColor.B;
				// Remove some green and more red.
				sunR -= (int)(180f * exampleStrength * (backgroundColor.R / 255f));
				sunG -= (int)(90f * exampleStrength * (backgroundColor.G / 255f));
				sunR = Utils.Clamp(sunR, 15, 255);
				sunG = Utils.Clamp(sunG, 15, 255);
				sunB = Utils.Clamp(sunB, 15, 255);
				backgroundColor.R = (byte)sunR;
				backgroundColor.G = (byte)sunG;
				backgroundColor.B = (byte)sunB;
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
		/* To be fixed later.
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
		*/

		public override void UpdateUI(GameTime gameTime)
		{
			if (exampleUserInterface != null && ExampleUI.visible)
				exampleUserInterface.Update(gameTime);
			if (examplePersonUserInterface != null)
				examplePersonUserInterface.Update(gameTime);
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
							exampleUserInterface.Draw(Main.spriteBatch, new GameTime());
						}
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
			int InventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
			if (InventoryIndex != -1)
			{
				layers.Insert(InventoryIndex + 1, new LegacyGameInterfaceLayer(
					"ExampleMod: Example Person UI",
					delegate
					{
						// If the current UIState of the UserInterface is null, nothing will draw. We don't need to track a separate .visible value.
						examplePersonUserInterface.Draw(Main.spriteBatch, new GameTime());
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
							Logger.Error("Error: Projectile not found");
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
					player.GetModPlayer<ExamplePlayer>().heroLives = lives;
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
				// This message syncs ExamplePlayer.exampleLifeFruits
				case ExampleModMessageType.ExamplePlayerSyncPlayer:
					byte playernumber = reader.ReadByte();
					ExamplePlayer examplePlayer = Main.player[playernumber].GetModPlayer<ExamplePlayer>();
					int exampleLifeFruits = reader.ReadInt32();
					examplePlayer.exampleLifeFruits = exampleLifeFruits;
					examplePlayer.nonStopParty = reader.ReadBoolean();
					// SyncPlayer will be called automatically, so there is no need to forward this data to other clients.
					break;
				case ExampleModMessageType.NonStopPartyChanged:
					playernumber = reader.ReadByte();
					examplePlayer = Main.player[playernumber].GetModPlayer<ExamplePlayer>();
					examplePlayer.nonStopParty = reader.ReadBoolean();
					// Unlike SyncPlayer, here we have to relay/forward these changes to all other connected clients
					if (Main.netMode == NetmodeID.Server)
					{
						var packet = GetPacket();
						packet.Write((byte)ExampleModMessageType.NonStopPartyChanged);
						packet.Write(playernumber);
						packet.Write(examplePlayer.nonStopParty);
						packet.Send(-1, playernumber);
					}
					break;
				default:
					Logger.WarnFormat("ExampleMod: Unknown Message type: {0}", msgType);
					break;
			}
		}
	}

	enum ExampleModMessageType : byte
	{
		SetTremorTime,
		VolcanicRubbleMultiplayerFix,
		PuritySpirit,
		HeroLives,
		ExamplePlayerSyncPlayer,
		NonStopPartyChanged
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

