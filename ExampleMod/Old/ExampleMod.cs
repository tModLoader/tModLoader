using ExampleMod.NPCs.PuritySpirit;
using ExampleMod.Tiles;
using ExampleMod.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using ExampleMod.Items.ExampleDamageClass;
using Terraria;
using Terraria.GameContent.Dyes;
using Terraria.GameContent.UI;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod
{
	public class ExampleMod : Mod
	{
		// With the new fonts in 1.3.5, font files are pretty big now so you need to generate the font file before building the mod.
		// You can use https://forums.terraria.org/index.php?threads/dynamicspritefontgenerator-0-4-generate-fonts-without-xna-game-studio.57127/ to make dynamicspritefonts
		public static DynamicSpriteFont exampleFont;

		private UserInterface _exampleUserInterface;

		internal UserInterface ExamplePersonUserInterface;
		internal ExampleUI ExampleUI;
		internal ExampleResourceBar ExampleResourceBar;

		// Your mod instance has a Logger field, use it.
		// OPTIONAL: You can create your own logger this way, recommended is a custom logging class if you do a lot of logging
		// You need to reference the log4net library to do this, this can be found in the tModLoader repository
		// inside the references folder. You do not have to add this to build.txt as tML has it natively.
		// internal ILog Logging = LogManager.GetLogger("ExampleMod");

		public override void Load() {
			// Will show up in client.log under the ExampleMod name
			Logger.InfoFormat("{0} example logging", Name);
			// In older tModLoader versions we used: ErrorLogger.Log("blabla");
			// Replace that with above

			// All code below runs only if we're not loading on a server
			if (!Main.dedServ) {
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
				GameShaders.Armor.BindShader(ModContent.ItemType<Items.ExampleDye>(), new ArmorShaderData(new Ref<Effect>(GetEffect("Effects/ExampleEffect")), "ExampleDyePass"));
				GameShaders.Hair.BindShader(ModContent.ItemType<Items.ExampleHairDye>(), new LegacyHairShaderData().UseLegacyMethod((Player player, Color newColor, ref bool lighting) => Color.Green));
				GameShaders.Misc["ExampleMod:DeathAnimation"] = new MiscShaderData(new Ref<Effect>(GetEffect("Effects/ExampleEffectDeath")), "DeathAnimation").UseImage("Images/Misc/Perlin");

				if (FontExists("Fonts/ExampleFont"))
					exampleFont = GetFont("Fonts/ExampleFont");

				// Custom UI
				ExampleUI = new ExampleUI();
				ExampleUI.Activate();
				_exampleUserInterface = new UserInterface();
				_exampleUserInterface.SetState(ExampleUI);

				// UserInterface can only show 1 UIState at a time. If you want different "pages" for a UI, switch between UIStates on the same UserInterface instance. 
				// We want both the Coin counter and the Example Person UI to be independent and coexist simultaneously, so we have them each in their own UserInterface.
				ExamplePersonUserInterface = new UserInterface();
				// We will call .SetState later in ExamplePerson.OnChatButtonClicked
			}

			// Register custom mod translations, lives left is for Spirit of Purity
			// See the .lang files in the Localization folder for an easier to manage approach to translations. These few examples are here just to illustrate the concept.
			ModTranslation text = CreateTranslation("LivesLeft");
			text.SetDefault("{0} has {1} lives left!");
			AddTranslation(text);
			text = CreateTranslation("LifeLeft");
			text.SetDefault("{0} has 1 life left!");
			AddTranslation(text);
			text = CreateTranslation("NPCTalk");
			text.SetDefault("<{0}> {1}");
			AddTranslation(text);
			text = CreateTranslation("Common.LocalizedLabelDynamic");
			text.SetDefault($"[i:{ModContent.ItemType<Items.Weapons.SpectreGun>()}]  This dynamic label is added in ExampleMod.Load");
			AddTranslation(text);

			text = CreateTranslation("BossSpawnInfo.Abomination");
			text.SetDefault("Use a [i:" + ModContent.ItemType<Items.Abomination.FoulOrb>() + "] in the underworld after Plantera has been defeated");
			AddTranslation(text);

			// Volcano warning is for the random volcano tremor
			text = CreateTranslation("VolcanoWarning");
			text.SetDefault("Did you hear something....A Volcano! Find Cover!");
			AddTranslation(text);
		}

		public override void Unload() {
			// All code below runs only if we're not loading on a server
			if (!Main.dedServ) {
				Main.tileFrame[TileID.Loom] = 0; // Reset the frame of the loom tile
				Main.tileSetsLoaded[TileID.Loom] = false; // Causes the loom tile to reload its vanilla texture
			}

			// Unload static references
			// You need to clear static references to assets (Texture2D, SoundEffects, Effects). 
			// In addition to that, if you want your mod to completely unload during unload, you need to clear static references to anything referencing your Mod class
			NPCs.ExampleTravelingMerchant.shopItems.Clear();
		}

		public override void UpdateMusic(ref int music, ref MusicPriority priority) {
			if (Main.myPlayer == -1 || Main.gameMenu || !Main.LocalPlayer.active) {
				return;
			}

			if (Main.LocalPlayer.HasBuff(BuffType("CarMount"))) {
				music = GetSoundSlot(SoundType.Music, "Sounds/Music/DriveMusic");
				priority = MusicPriority.Environment;
			}
		}

		public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor) {
			if (ExampleWorld.exampleTiles <= 0) {
				return;
			}

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

		//const int ShakeLength = 5;
		//readonly int ShakeCount = 0;
		//readonly float previousRotation = 0;
		//readonly float targetRotation = 0;
		//readonly float previousOffsetX = 0;
		//readonly float previousOffsetY = 0;
		//readonly float targetOffsetX = 0;
		//readonly float targetOffsetY = 0;

		// Volcano Tremor
		/* TODO To be fixed later.
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

		public override void UpdateUI(GameTime gameTime) {
			if (ExampleUI.Visible) {
				_exampleUserInterface?.Update(gameTime);
			}
			ExamplePersonUserInterface?.Update(gameTime);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (mouseTextIndex != -1) {
				layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
					"ExampleMod: Coins Per Minute",
					delegate {
						if (ExampleUI.Visible) {
							_exampleUserInterface.Draw(Main.spriteBatch, new GameTime());
						}
						return true;
					},
					InterfaceScaleType.UI)
				);
			}

			int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
			if (inventoryIndex != -1) {
				layers.Insert(inventoryIndex, new LegacyGameInterfaceLayer(
					"ExampleMod: Example Person UI",
					delegate {
						// If the current UIState of the UserInterface is null, nothing will draw. We don't need to track a separate .visible value.
						ExamplePersonUserInterface.Draw(Main.spriteBatch, new GameTime());
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}

		public static bool NoInvasion(NPCSpawnInfo spawnInfo) => !spawnInfo.invasion && (!Main.pumpkinMoon && !Main.snowMoon || spawnInfo.spawnTileY > Main.worldSurface || Main.dayTime) && (!Main.eclipse || spawnInfo.spawnTileY > Main.worldSurface || !Main.dayTime);

		public static bool NoBiome(NPCSpawnInfo spawnInfo) {
			Player player = spawnInfo.player;
			return !player.ZoneJungle && !player.ZoneDungeon && !player.ZoneCorrupt && !player.ZoneCrimson && !player.ZoneHoly && !player.ZoneSnow && !player.ZoneUndergroundDesert;
		}

		public static bool NoZoneAllowWater(NPCSpawnInfo spawnInfo) => !spawnInfo.sky && !spawnInfo.player.ZoneMeteor && !spawnInfo.spiderCave;

		public static bool NoZone(NPCSpawnInfo spawnInfo) => NoZoneAllowWater(spawnInfo) && !spawnInfo.water;

		public static bool NormalSpawn(NPCSpawnInfo spawnInfo) => !spawnInfo.playerInTown && NoInvasion(spawnInfo);

		public static bool NoZoneNormalSpawn(NPCSpawnInfo spawnInfo) => NormalSpawn(spawnInfo) && NoZone(spawnInfo);

		public static bool NoZoneNormalSpawnAllowWater(NPCSpawnInfo spawnInfo) => NormalSpawn(spawnInfo) && NoZoneAllowWater(spawnInfo);

		public static bool NoBiomeNormalSpawn(NPCSpawnInfo spawnInfo) => NormalSpawn(spawnInfo) && NoBiome(spawnInfo) && NoZone(spawnInfo);

		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			ExampleModMessageType msgType = (ExampleModMessageType)reader.ReadByte();
			switch (msgType) {
				// This message sent by the server to initialize the Volcano Tremor on clients
				case ExampleModMessageType.SetTremorTime:
					int tremorTime = reader.ReadInt32();
					ExampleWorld world = GetInstance<ExampleWorld>();
					world.VolcanoTremorTime = tremorTime;
					break;
				// This message sent by the server to initialize the Volcano Rubble.
				case ExampleModMessageType.VolcanicRubbleMultiplayerFix:
					int numberProjectiles = reader.ReadInt32();
					for (int i = 0; i < numberProjectiles; i++) {
						int identity = reader.ReadInt32();
						bool found = false;
						for (int j = 0; j < 1000; j++) {
							if (Main.projectile[j].owner == 255 && Main.projectile[j].identity == identity && Main.projectile[j].active) {
								Main.projectile[j].hostile = true;
								//Main.projectile[j].name = "Volcanic Rubble";
								found = true;
								break;
							}
						}
						if (!found) {
							Logger.Error("Error: Projectile not found");
						}
					}
					break;
				case ExampleModMessageType.PuritySpirit:
					if (Main.npc[reader.ReadInt32()].modNPC is PuritySpirit spirit && spirit.npc.active) {
						spirit.HandlePacket(reader);
					}
					break;
				case ExampleModMessageType.HeroLives:
					Player player = Main.player[reader.ReadInt32()];
					int lives = reader.ReadInt32();
					player.GetModPlayer<ExamplePlayer>().heroLives = lives;
					if (lives > 0) {
						NetworkText text;
						if (lives == 1) {
							text = NetworkText.FromKey("Mods.ExampleMod.LifeLeft", player.name);
						}
						else {
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
					if (Main.netMode == NetmodeID.Server) {
						var packet = GetPacket();
						packet.Write((byte)ExampleModMessageType.NonStopPartyChanged);
						packet.Write(playernumber);
						packet.Write(examplePlayer.nonStopParty);
						packet.Send(-1, playernumber);
					}
					break;
				case ExampleModMessageType.ExampleTeleportToStatue:
					if (Main.npc[reader.ReadByte()].modNPC is NPCs.ExamplePerson person && person.npc.active) {
						person.StatueTeleport();
					}
					break;
				default:
					Logger.WarnFormat("ExampleMod: Unknown Message type: {0}", msgType);
					break;
			}
		}
	}

	internal enum ExampleModMessageType : byte
	{
		SetTremorTime,
		VolcanicRubbleMultiplayerFix,
		PuritySpirit,
		HeroLives,
		ExamplePlayerSyncPlayer,
		NonStopPartyChanged,
		ExampleTeleportToStatue
	}
}

