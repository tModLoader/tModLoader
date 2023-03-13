﻿using ExampleMod.Content.Dusts;
using ExampleMod.Content.Items;
using ExampleMod.Content.Items.Armor;
using ExampleMod.Content.Items.Placeable.Furniture;
using ExampleMod.Content.Items.Tools;
using ExampleMod.Content.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Chat;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace ExampleMod.Content.NPCs
{
	[AutoloadHead]
	class ExampleTravelingMerchant : ModNPC
	{
		// Time of day for traveller to leave (6PM)
		public const double despawnTime = 48600.0;

		// the time of day the traveler will spawn (double.MaxValue for no spawn)
		// saved and loaded with the world in TravelingMerchantSystem
		public static double spawnTime = double.MaxValue;

		// The list of items in the traveler's shop. Saved with the world and set when the traveler spawns
		public List<Item> shopItems = new List<Item>();

		private static int ShimmerHeadIndex;
		private static Profiles.StackedNPCProfile NPCProfile;

		public override bool PreAI() {
			if ((!Main.dayTime || Main.time >= despawnTime) && !IsNpcOnscreen(NPC.Center)) // If it's past the despawn time and the NPC isn't onscreen
			{
				// Here we despawn the NPC and send a message stating that the NPC has despawned
				// LegacyMisc.35 is {0) has departed!
				if (Main.netMode == NetmodeID.SinglePlayer) Main.NewText(Language.GetTextValue("LegacyMisc.35", NPC.FullName), 50, 125, 255);
				else ChatHelper.BroadcastChatMessage(NetworkText.FromKey("LegacyMisc.35", NPC.GetFullNetName()), new Color(50, 125, 255));
				NPC.active = false;
				NPC.netSkip = -1;
				NPC.life = 0;
				return false;
			}

			return true;
		}

		public static void UpdateTravelingMerchant() {
			bool travelerIsThere = (NPC.FindFirstNPC(ModContent.NPCType<ExampleTravelingMerchant>()) != -1); // Find a Merchant if there's one spawned in the world

			// Main.time is set to 0 each morning, and only for one update. Sundialling will never skip past time 0 so this is the place for 'on new day' code
			if (Main.dayTime && Main.time == 0) {
				// insert code here to change the spawn chance based on other conditions (say, npcs which have arrived, or milestones the player has passed)
				// You can also add a day counter here to prevent the merchant from possibly spawning multiple days in a row.

				// NPC won't spawn today if it stayed all night
				if (!travelerIsThere && Main.rand.NextBool(4)) { // 4 = 25% Chance
																// Here we can make it so the NPC doesnt spawn at the EXACT same time every time it does spawn
					spawnTime = GetRandomSpawnTime(5400, 8100); // minTime = 6:00am, maxTime = 7:30am
				}
				else {
					spawnTime = double.MaxValue; // no spawn today
				}
			}

			// Spawn the traveler if the spawn conditions are met (time of day, no events, no sundial)
			if (!travelerIsThere && CanSpawnNow()) {
				int newTraveler = NPC.NewNPC(Terraria.Entity.GetSource_TownSpawn(), Main.spawnTileX * 16, Main.spawnTileY * 16, ModContent.NPCType<ExampleTravelingMerchant>(), 1); // Spawning at the world spawn
				NPC traveler = Main.npc[newTraveler];
				traveler.homeless = true;
				traveler.direction = Main.spawnTileX >= WorldGen.bestX ? -1 : 1;
				traveler.netUpdate = true;

				// Prevents the traveler from spawning again the same day
				spawnTime = double.MaxValue;

				// Annouce that the traveler has spawned in!
				if (Main.netMode == NetmodeID.SinglePlayer) Main.NewText(Language.GetTextValue("Announcement.HasArrived", traveler.FullName), 50, 125, 255);
				else ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Announcement.HasArrived", traveler.GetFullNetName()), new Color(50, 125, 255));
			}
		}

		private static bool CanSpawnNow() {
			// can't spawn if any events are running
			if (Main.eclipse || Main.invasionType > 0 && Main.invasionDelay == 0 && Main.invasionSize > 0)
				return false;

			// can't spawn if the sundial is active
			if (Main.IsFastForwardingTime())
				return false;

			// can spawn if daytime, and between the spawn and despawn times
			return Main.dayTime && Main.time >= spawnTime && Main.time < despawnTime;
		}

		private static bool IsNpcOnscreen(Vector2 center) {
			int w = NPC.sWidth + NPC.safeRangeX * 2;
			int h = NPC.sHeight + NPC.safeRangeY * 2;
			Rectangle npcScreenRect = new Rectangle((int)center.X - w / 2, (int)center.Y - h / 2, w, h);
			foreach (Player player in Main.player) {
				// If any player is close enough to the traveling merchant, it will prevent the npc from despawning
				if (player.active && player.getRect().Intersects(npcScreenRect)) return true;
			}
			return false;
		}

		public static double GetRandomSpawnTime(double minTime, double maxTime) {
			// A simple formula to get a random time between two chosen times
			return (maxTime - minTime) * Main.rand.NextDouble() + minTime;
		}

		public void CreateNewShop() {
			// create a list of item ids
			var itemIds = new List<int>();

			// For each slot we add a switch case to determine what should go in that slot
			switch (Main.rand.Next(2)) {
				case 0:
					itemIds.Add(ModContent.ItemType<ExampleItem>());
					break;
				default:
					itemIds.Add(ModContent.ItemType<ExampleSoul>());
					break;
			}

			switch (Main.rand.Next(2)) {
				case 0:
					itemIds.Add(ModContent.ItemType<ExampleDye>());
					break;
				default:
					itemIds.Add(ModContent.ItemType<ExampleHairDye>());
					break;
			}

			switch (Main.rand.Next(4)) {
				case 0:
					itemIds.Add(ModContent.ItemType<ExampleDoor>());
					break;
				case 1:
					itemIds.Add(ModContent.ItemType<ExampleBed>());
					break;
				case 2:
					itemIds.Add(ModContent.ItemType<ExampleChest>());
					break;
				default:
					itemIds.Add(ModContent.ItemType<ExamplePickaxe>());
					break;
			}

			// convert to a list of items
			shopItems = new List<Item>();
			foreach (int itemId in itemIds) {
				Item item = new Item();
				item.SetDefaults(itemId);
				shopItems.Add(item);
			}
		}

		public override void Load() {
			// Adds our Shimmer Head to the NPCHeadLoader.
			ShimmerHeadIndex = Mod.AddNPCHeadTexture(Type, Texture + "_Shimmer_Head");
		}

		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 25;
			NPCID.Sets.ExtraFramesCount[NPC.type] = 9;
			NPCID.Sets.AttackFrameCount[NPC.type] = 4;
			NPCID.Sets.DangerDetectRange[NPC.type] = 700;
			NPCID.Sets.AttackType[NPC.type] = 0;
			NPCID.Sets.AttackTime[NPC.type] = 90;
			NPCID.Sets.AttackAverageChance[NPC.type] = 30;
			NPCID.Sets.HatOffsetY[NPC.type] = 4;
			NPCID.Sets.ShimmerTownTransform[Type] = true;

			// Influences how the NPC looks in the Bestiary
			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers(0) {
				Velocity = 2f, // Draws the NPC in the bestiary as if its walking +2 tiles in the x direction
				Direction = -1 // -1 is left and 1 is right.
			};

			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

			NPCProfile = new Profiles.StackedNPCProfile(
				new Profiles.DefaultNPCProfile(Texture, NPCHeadLoader.GetHeadSlot(HeadTexture), Texture + "_Party"),
				new Profiles.DefaultNPCProfile(Texture + "_Shimmer", ShimmerHeadIndex)
			);
		}

		public override void SetDefaults() {
			NPC.townNPC = true;
			NPC.friendly = true;
			NPC.width = 18;
			NPC.height = 40;
			NPC.aiStyle = 7;
			NPC.damage = 10;
			NPC.defense = 15;
			NPC.lifeMax = 250;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 0.5f;
			AnimationType = NPCID.Guide;
			TownNPCStayingHomeless = true;
			CreateNewShop();
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface
			});
		}

		public override void SaveData(TagCompound tag) {
			tag["itemIds"] = shopItems;
		}

		public override void LoadData(TagCompound tag) {
			shopItems = tag.Get<List<Item>>("shopItems");
		}

		public override void HitEffect(NPC.HitInfo hit) {
			int num = NPC.life > 0 ? 1 : 5;
			for (int k = 0; k < num; k++) {
				Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<Sparkle>());
			}

			// Create gore when the NPC is killed.
			if (Main.netMode != NetmodeID.Server && NPC.life <= 0) {
				// Retrieve the gore types. This NPC has shimmer variants for head, arm, and leg gore. It also has a custom hat gore. (7 gores)
				// This NPC will spawn either the assigned party hat or a custom hat gore when not shimmered. When shimmered the top hat is part of the head and no hat gore is spawned.
				int hatGore = NPC.GetPartyHatGore();
				// If not wearing a party hat, and not shimmered, retrieve the custom hat gore 
				if (hatGore == 0 && !NPC.IsShimmerVariant) {
					hatGore = Mod.Find<ModGore>($"{Name}_Gore_Hat").Type;
				}
				string variant = "";
				if (NPC.IsShimmerVariant) variant += "_Shimmer";
				int headGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Head").Type;
				int armGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Arm").Type;
				int legGore = Mod.Find<ModGore>($"{Name}_Gore{variant}_Leg").Type;

				// Spawn the gores. The positions of the arms and legs are lowered for a more natural look.
				if (hatGore > 0) {
					Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, hatGore);
				}
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, headGore);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 20), NPC.velocity, armGore);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 20), NPC.velocity, armGore);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 34), NPC.velocity, legGore);
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(0, 34), NPC.velocity, legGore);
			}
		}

		public override bool UsesPartyHat() {
			// ExampleTravelingMerchant likes to keep his hat on while shimmered.
			if (NPC.IsShimmerVariant) {
				return false;
			}
			return true;
		}

		public override bool CanTownNPCSpawn(int numTownNPCs) {
			return false; // This should always be false, because we spawn in the Traveling Merchant manually
		}

		public override ITownNPCProfile TownNPCProfile() {
			return NPCProfile;
		}

		public override List<string> SetNPCNameList() {
			return new List<string>() {
				"Someone Else",
				"Somebody Else",
				"Blockster",
				"Colorful"
			};
		}

		public override string GetChat() {
			WeightedRandom<string> chat = new WeightedRandom<string>();

			int partyGirl = NPC.FindFirstNPC(NPCID.PartyGirl);
			if (partyGirl >= 0) {
				chat.Add(Language.GetTextValue("Mods.ExampleMod.Dialogue.ExampleTravelingMerchant.PartyGirlDialogue", Main.npc[partyGirl].GivenName));
			}

			chat.Add(Language.GetTextValue("Mods.ExampleMod.Dialogue.ExampleTravelingMerchant.StandardDialogue1"));
			chat.Add(Language.GetTextValue("Mods.ExampleMod.Dialogue.ExampleTravelingMerchant.StandardDialogue2"));
			chat.Add(Language.GetTextValue("Mods.ExampleMod.Dialogue.ExampleTravelingMerchant.StandardDialogue3"));

			string hivePackDialogue = Language.GetTextValue("Mods.ExampleMod.Dialogue.ExampleTravelingMerchant.HiveBackpackDialogue");
			chat.Add(hivePackDialogue);

			string dialogueLine = chat; // chat is implicitly cast to a string.
			if (hivePackDialogue.Equals(dialogueLine)) {
				// Main.npcChatCornerItem shows a single item in the corner, like the Angler Quest chat.
				Main.npcChatCornerItem = ItemID.HiveBackpack;
			}

			return dialogueLine;
		}

		public override void SetChatButtons(ref string button, ref string button2) {
			button = Language.GetTextValue("LegacyInterface.28");
		}

		public override void OnChatButtonClicked(bool firstButton, ref bool shop) {
			if (firstButton) {
				shop = true;
			}
		}

		public override void SetupShop(Chest shop, ref int nextSlot) {
			foreach (Item item in shopItems) {
				// We don't want "empty" items and unloaded items to appear
				if (item == null || item.type == ItemID.None)
					continue;

				shop.item[nextSlot].SetDefaults(item.type);
				nextSlot++;
			}
		}

		public override void AI() {
			NPC.homeless = true; // Make sure it stays homeless
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ExampleCostume>()));
		}

		public override void TownNPCAttackStrength(ref int damage, ref float knockback) {
			damage = 20;
			knockback = 4f;
		}

		public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown) {
			cooldown = 30;
			randExtraCooldown = 30;
		}

		public override void TownNPCAttackProj(ref int projType, ref int attackDelay) {
			projType = ModContent.ProjectileType<SparklingBall>();
			attackDelay = 1;
		}

		public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset) {
			multiplier = 12f;
			randomOffset = 2f;
		}
	}
}
