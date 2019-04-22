using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.NPCs
{
	[AutoloadHead]
	class ExampleTravelingMerchant : ModNPC
	{
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

		public override void HitEffect(int hitDirection, double damage) {
			int num = npc.life > 0 ? 1 : 5;
			for (int k = 0; k < num; k++) {
				Dust.NewDust(npc.position, npc.width, npc.height, mod.DustType("Sparkle"));
			}
		}

		public override bool CanTownNPCSpawn(int numTownNPCs, int money) {
			for (int i = 0; i < Main.maxNPCs; i++) {
				if (Main.npc[i].type != mod.NPCType("ExamplePerson")) continue; // If the npc is not the Example Person, move onto the next npc
				return true; // If one Example Person exists, Example traveling Merchant can spawn
			}
			return false;
		}

		public override string TownNPCName() {
			switch (WorldGen.genRand.Next(4)) {
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
			// Here we get the random ints that were declared in ExampleWorld, and based on them make randomzied shop items
			// Remember that we saved the int array so even if we revisit the shop or leave the world, the items will remain the same until the traveler despawns
			// You can of course add to the Main.rand.Next(#) number to increase the amount of items that can randomly generate
			if (ExampleWorld.travelerItems[0] == 0) {
				shop.item[nextSlot].SetDefaults(mod.ItemType("ExampleItem"));
			}
			else if (ExampleWorld.travelerItems[0] == 1) {
				shop.item[nextSlot].SetDefaults(mod.ItemType("EquipMaterial"));
			}
			nextSlot++;

			if (ExampleWorld.travelerItems[1] == 0) {
				shop.item[nextSlot].SetDefaults(mod.ItemType("BossItem"));
			}
			else if (ExampleWorld.travelerItems[1] == 1) {
				shop.item[nextSlot].SetDefaults(mod.ItemType("ExampleWorkbench"));
			}
			else if (ExampleWorld.travelerItems[1] == 2) {
				shop.item[nextSlot].SetDefaults(mod.ItemType("ExampleChair"));
			}
			nextSlot++;

			if (ExampleWorld.travelerItems[2] == 0) {
				shop.item[nextSlot].SetDefaults(mod.ItemType("ExampleDoor"));
			}
			else if (ExampleWorld.travelerItems[2] == 1) {
				shop.item[nextSlot].SetDefaults(mod.ItemType("ExampleBed"));
			}
			else if (ExampleWorld.travelerItems[2] == 2) {
				shop.item[nextSlot].SetDefaults(mod.ItemType("ExampleChest"));
			}
			else if (ExampleWorld.travelerItems[2] == 3) {
				shop.item[nextSlot].SetDefaults(mod.ItemType("ExamplePickaxe"));
			}
			nextSlot++;
		}

		public override void AI() {
			npc.homeless = true; // Make sure it stays homeless
			base.AI();
		}

		public override void NPCLoot() {
			Item.NewItem(npc.getRect(), mod.ItemType<Items.Armor.ExampleCostume>());
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
			projType = mod.ProjectileType("SparklingBall");
			attackDelay = 1;
		}

		public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset) {
			multiplier = 12f;
			randomOffset = 2f;
		}
	}
}
