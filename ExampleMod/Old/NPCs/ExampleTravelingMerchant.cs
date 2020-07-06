using ExampleMod.Dusts;
using ExampleMod.Items;
using ExampleMod.Items.Placeable;
using ExampleMod.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.NPCs
{
	[AutoloadHead]
	class ExampleTravelingMerchant : ModNPC
	{
		// Time of day for traveller to leave (6PM)
		public const double despawnTime = 48600.0;

		// the time of day the traveler will spawn (double.MaxValue for no spawn)
		// saved and loaded with the world in ExampleWorld
		public static double spawnTime = double.MaxValue;

		// The list of items in the traveler's shop. Saved with the world and reset when a new traveler spawns
		public static List<Item> shopItems = new List<Item>();

		public static NPC FindNPC(int npcType) => Main.npc.FirstOrDefault(npc => npc.type == npcType && npc.active);

		public static void UpdateTravelingMerchant() {
			NPC traveler = FindNPC(NPCType<ExampleTravelingMerchant>()); // Find an Explorer if there's one spawned in the world
			if (traveler != null && (!Main.dayTime || Main.time >= despawnTime) && !IsNpcOnscreen(traveler.Center)) // If it's past the despawn time and the NPC isn't onscreen
			{
				// Here we despawn the NPC and send a message stating that the NPC has despawned
				if (Main.netMode == NetmodeID.SinglePlayer) Main.NewText(traveler.FullName + " has departed!", 50, 125, 255);
				else NetMessage.BroadcastChatMessage(NetworkText.FromLiteral(traveler.FullName + " has departed!"), new Color(50, 125, 255));
				traveler.active = false;
				traveler.netSkip = -1;
				traveler.life = 0;
				traveler = null;
			}

			// Main.time is set to 0 each morning, and only for one update. Sundialling will never skip past time 0 so this is the place for 'on new day' code
			if (Main.dayTime && Main.time == 0) {
				// insert code here to change the spawn chance based on other conditions (say, npcs which have arrived, or milestones the player has passed)
				// You can also add a day counter here to prevent the merchant from possibly spawning multiple days in a row.

				// NPC won't spawn today if it stayed all night
				if (traveler == null && Main.rand.NextBool(4)) { // 4 = 25% Chance
																// Here we can make it so the NPC doesnt spawn at the EXACT same time every time it does spawn
					spawnTime = GetRandomSpawnTime(5400, 8100); // minTime = 6:00am, maxTime = 7:30am
				}
				else {
					spawnTime = double.MaxValue; // no spawn today
				}
			}

			// Spawn the traveler if the spawn conditions are met (time of day, no events, no sundial)
			if (traveler == null && CanSpawnNow()) {
				int newTraveler = NPC.NewNPC(Main.spawnTileX * 16, Main.spawnTileY * 16, NPCType<ExampleTravelingMerchant>(), 1); // Spawning at the world spawn
				traveler = Main.npc[newTraveler];
				traveler.homeless = true;
				traveler.direction = Main.spawnTileX >= WorldGen.bestX ? -1 : 1;
				traveler.netUpdate = true;
				shopItems = CreateNewShop();

				// Prevents the traveler from spawning again the same day
				spawnTime = double.MaxValue;

				// Annouce that the traveler has spawned in!
				if (Main.netMode == NetmodeID.SinglePlayer) Main.NewText(Language.GetTextValue("Announcement.HasArrived", traveler.FullName), 50, 125, 255);
				else NetMessage.BroadcastChatMessage(NetworkText.FromKey("Announcement.HasArrived", traveler.GetFullNetName()), new Color(50, 125, 255));
			}
		}

		private static bool CanSpawnNow() {
			// can't spawn if any events are running
			if (Main.eclipse || Main.invasionType > 0 && Main.invasionDelay == 0 && Main.invasionSize > 0)
				return false;

			// can't spawn if the sundial is active
			if (Main.fastForwardTime)
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

		public static List<Item> CreateNewShop() {
			// create a list of item ids
			var itemIds = new List<int>();

			// For each slot we add a switch case to determine what should go in that slot
			switch (Main.rand.Next(2)) {
				case 0:
					itemIds.Add(ItemType<ExampleItem>());
					break;
				default:
					itemIds.Add(ItemType<EquipMaterial>());
					break;
			}

			switch (Main.rand.Next(3)) {
				case 0:
					itemIds.Add(ItemType<BossItem>());
					break;
				case 1:
					itemIds.Add(ItemType<ExampleWorkbench>());
					break;
				default:
					itemIds.Add(ItemType<ExampleChair>());
					break;
			}

			switch (Main.rand.Next(4)) {
				case 0:
					itemIds.Add(ItemType<ExampleDoor>());
					break;
				case 1:
					itemIds.Add(ItemType<ExampleBed>());
					break;
				case 2:
					itemIds.Add(ItemType<ExampleChest>());
					break;
				default:
					itemIds.Add(ItemType<ExamplePickaxe>());
					break;
			}

			// conver to a list of items
			var items = new List<Item>();
			foreach (int itemId in itemIds) {
				Item item = new Item();
				item.SetDefaults(itemId);
				items.Add(item);
			}
			return items;
		}

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Example Traveler");
			Main.npcFrameCount[npc.type] = 25;
			NPCID.Sets.ExtraFramesCount[npc.type] = 9;
			NPCID.Sets.AttackFrameCount[npc.type] = 4;
			NPCID.Sets.DangerDetectRange[npc.type] = 700;
			NPCID.Sets.AttackType[npc.type] = 0;
			NPCID.Sets.AttackTime[npc.type] = 90;
			NPCID.Sets.AttackAverageChance[npc.type] = 30;
			NPCID.Sets.HatOffsetY[npc.type] = 4;
		}

		public override void SetDefaults() {
			npc.townNPC = true; // This will be changed once the NPC is spawned
			npc.friendly = true;
			npc.width = 18;
			npc.height = 40;
			npc.aiStyle = 7;
			npc.damage = 10;
			npc.defense = 15;
			npc.lifeMax = 250;
			npc.HitSound = SoundID.NPCHit1;
			npc.DeathSound = SoundID.NPCDeath1;
			npc.knockBackResist = 0.5f;
			animationType = NPCID.Guide;
		}

		public static TagCompound Save() {
			return new TagCompound {
				["spawnTime"] = spawnTime,
				["shopItems"] = shopItems
			};
		}

		public static void Load(TagCompound tag) {
			spawnTime = tag.GetDouble("spawnTime");
			shopItems = tag.Get<List<Item>>("shopItems");
		}

		public override void HitEffect(int hitDirection, double damage) {
			int num = npc.life > 0 ? 1 : 5;
			for (int k = 0; k < num; k++) {
				Dust.NewDust(npc.position, npc.width, npc.height, DustType<Sparkle>());
			}
		}

		public override bool CanTownNPCSpawn(int numTownNPCs, int money) {
			return false; // This should always be false, because we spawn in the Travleing Merchant manually
		}

		public override string TownNPCName() {
			switch (Main.rand.Next(4)) {
				case 0:
					return "Someone";
				case 1:
					return "Somebody";
				case 2:
					return "Blockster";
				default:
					return "Colorful";
			}
		}

		public override string GetChat() {
			int partyGirl = NPC.FindFirstNPC(NPCID.PartyGirl);
			if (partyGirl >= 0 && Main.rand.NextBool(4)) {
				return "Can you please tell " + Main.npc[partyGirl].GivenName + " to stop decorating my cousin's house with colors?";
			}
			switch (Main.rand.Next(4)) {
				case 0:
					return "Sometimes my cousin feels like they're different from everyone else here.";
				case 1:
					return "What's your favorite color? My cousin's favorite colors are white and black.";
				case 2: {
						// Main.npcChatCornerItem shows a single item in the corner, like the Angler Quest chat.
						Main.npcChatCornerItem = ItemID.HiveBackpack;
						return $"Hey, if you find a [i:{ItemID.HiveBackpack}], my cousin can upgrade it for you.";
					}
				default:
					return "What? My cousin doesn't have any arms or legs? Oh, don't be ridiculous!";
			}
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
				// We dont want "empty" items and unloaded items to appear
				if (item == null || item.type == ItemID.None)
					continue;

				shop.item[nextSlot].SetDefaults(item.type);
				nextSlot++;
			}
		}

		public override void AI() {
			npc.homeless = true; // Make sure it stays homeless
		}

		public override void NPCLoot() {
			Item.NewItem(npc.getRect(), ItemType<Items.Armor.ExampleCostume>());
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
			projType = ProjectileType<SparklingBall>();
			attackDelay = 1;
		}

		public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset) {
			multiplier = 12f;
			randomOffset = 2f;
		}
	}
}
