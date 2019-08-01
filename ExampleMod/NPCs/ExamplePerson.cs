using System;
using ExampleMod.Items;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.NPCs
{
	// [AutoloadHead] and npc.townNPC are extremely important and absolutely both necessary for any Town NPC to work at all.
	[AutoloadHead]
	public class ExamplePerson : ModNPC
	{
		public override string Texture => "ExampleMod/NPCs/ExamplePerson";

		public override string[] AltTextures => new[] { "ExampleMod/NPCs/ExamplePerson_Alt_1" };

		public override bool Autoload(ref string name) {
			name = "Example Person";
			return mod.Properties.Autoload;
		}

		public override void SetStaticDefaults() {
			// DisplayName automatically assigned from .lang files, but the commented line below is the normal approach.
			// DisplayName.SetDefault("Example Person");
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
			npc.townNPC = true;
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
			for (int k = 0; k < 255; k++) {
				Player player = Main.player[k];
				if (!player.active) {
					continue;
				}

				foreach (Item item in player.inventory) {
					if (item.type == mod.ItemType("ExampleItem") || item.type == mod.ItemType("ExampleBlock")) {
						return true;
					}
				}
			}
			return false;
		}

		// Example Person needs a house built out of ExampleMod tiles. You can delete this whole method in your townNPC for the regular house conditions.
		public override bool CheckConditions(int left, int right, int top, int bottom) {
			int score = 0;
			for (int x = left; x <= right; x++) {
				for (int y = top; y <= bottom; y++) {
					int type = Main.tile[x, y].type;
					if (type == mod.TileType("ExampleBlock") || type == mod.TileType("ExampleChair") || type == mod.TileType("ExampleWorkbench") || type == mod.TileType("ExampleBed") || type == mod.TileType("ExampleDoorOpen") || type == mod.TileType("ExampleDoorClosed")) {
						score++;
					}
					if (Main.tile[x, y].wall == mod.WallType("ExampleWall")) {
						score++;
					}
				}
			}
			return score >= (right - left) * (bottom - top) / 2;
		}

		public override string TownNPCName() {
			switch (WorldGen.genRand.Next(4)) {
				case 0:
					return "Someone";
				case 1:
					return "Somebody";
				case 2:
					return "Blocky";
				default:
					return "Colorless";
			}
		}

		public override void FindFrame(int frameHeight) {
			/*npc.frame.Width = 40;
			if (((int)Main.time / 10) % 2 == 0)
			{
				npc.frame.X = 40;
			}
			else
			{
				npc.frame.X = 0;
			}*/
		}

		public override string GetChat() {
			int partyGirl = NPC.FindFirstNPC(NPCID.PartyGirl);
			if (partyGirl >= 0 && Main.rand.NextBool(4)) {
				return "Can you please tell " + Main.npc[partyGirl].GivenName + " to stop decorating my house with colors?";
			} 
			switch (Main.rand.Next(4)) {
				case 0:
					return "Sometimes I feel like I'm different from everyone else here.";
				case 1:
					return "What's your favorite color? My favorite colors are white and black.";
				case 2:
					{
						// Main.npcChatCornerItem shows a single item in the corner, like the Angler Quest chat.
						Main.npcChatCornerItem = ItemID.HiveBackpack;
						return $"Hey, if you find a [i:{ItemID.HiveBackpack}], I can upgrade it for you.";
					}
				default:
					return "What? I don't have any arms or legs? Oh, don't be ridiculous!";
			}
		}

		/* 
		// Consider using this alternate approach to choosing a random thing. Very useful for a variety of use cases.
		// The WeightedRandom class needs "using Terraria.Utilities;" to use
		public override string GetChat()
		{
			WeightedRandom<string> chat = new WeightedRandom<string>();

			int partyGirl = NPC.FindFirstNPC(NPCID.PartyGirl);
			if (partyGirl >= 0 && Main.rand.NextBool(4))
			{
				chat.Add("Can you please tell " + Main.npc[partyGirl].GivenName + " to stop decorating my house with colors?");
			}
			chat.Add("Sometimes I feel like I'm different from everyone else here.");
			chat.Add("What's your favorite color? My favorite colors are white and black.");
			chat.Add("What? I don't have any arms or legs? Oh, don't be ridiculous!");
			chat.Add("This message has a weight of 5, meaning it appears 5 times more often.", 5.0);
			chat.Add("This message has a weight of 0.1, meaning it appears 10 times as rare.", 0.1);
			return chat; // chat is implicitly cast to a string. You can also do "return chat.Get();" if that makes you feel better
		}
		*/

		public override void SetChatButtons(ref string button, ref string button2) {
			button = Language.GetTextValue("LegacyInterface.28");
			button2 = "Awesomeify";
			if (Main.LocalPlayer.HasItem(ItemID.HiveBackpack))
				button = "Upgrade " + Lang.GetItemNameValue(ItemID.HiveBackpack);
		}

		public override void OnChatButtonClicked(bool firstButton, ref bool shop) {
			if (firstButton) {
				// We want 3 different functionalities for chat buttons, so we use HasItem to change button 1 between a shop and upgrade action.
				if (Main.LocalPlayer.HasItem(ItemID.HiveBackpack))
				{
					Main.PlaySound(SoundID.Item37); // Reforge/Anvil sound
					Main.npcChatText = $"I upgraded your {Lang.GetItemNameValue(ItemID.HiveBackpack)} to a {Lang.GetItemNameValue(mod.ItemType<Items.Accessories.WaspNest>())}";
					int hiveBackpackItemIndex = Main.LocalPlayer.FindItem(ItemID.HiveBackpack);
					Main.LocalPlayer.inventory[hiveBackpackItemIndex].TurnToAir();
					Main.LocalPlayer.QuickSpawnItem(mod.ItemType<Items.Accessories.WaspNest>());
					return;
				}
				shop = true;
			}
			else {
				// If the 2nd button is pressed, open the inventory...
				Main.playerInventory = true;
				// remove the chat window...
				Main.npcChatText = "";
				// and start an instance of our UIState.
				ExampleMod.Instance.ExamplePersonUserInterface.SetState(new UI.ExamplePersonUI());
				// Note that even though we remove the chat window, Main.LocalPlayer.talkNPC will still be set correctly and we are still technically chatting with the npc.
			}
		}

		public override void SetupShop(Chest shop, ref int nextSlot) {
			shop.item[nextSlot].SetDefaults(mod.ItemType("ExampleItem"));
			nextSlot++;
			shop.item[nextSlot].SetDefaults(mod.ItemType("EquipMaterial"));
			nextSlot++;
			shop.item[nextSlot].SetDefaults(mod.ItemType("BossItem"));
			nextSlot++;
			shop.item[nextSlot].SetDefaults(mod.ItemType("ExampleWorkbench"));
			nextSlot++;
			shop.item[nextSlot].SetDefaults(mod.ItemType("ExampleChair"));
			nextSlot++;
			shop.item[nextSlot].SetDefaults(mod.ItemType("ExampleDoor"));
			nextSlot++;
			shop.item[nextSlot].SetDefaults(mod.ItemType("ExampleBed"));
			nextSlot++;
			shop.item[nextSlot].SetDefaults(mod.ItemType("ExampleChest"));
			nextSlot++;
			shop.item[nextSlot].SetDefaults(mod.ItemType("ExamplePickaxe"));
			nextSlot++;
			shop.item[nextSlot].SetDefaults(mod.ItemType("ExampleHamaxe"));
			nextSlot++;
			if (Main.LocalPlayer.HasBuff(BuffID.Lifeforce)) {
				shop.item[nextSlot].SetDefaults(mod.ItemType("ExampleHealingPotion"));
				nextSlot++;
			}
			if (Main.LocalPlayer.GetModPlayer<ExamplePlayer>().ZoneExample && !ExampleConfigServer.Instance.DisableExampleWings) {
				shop.item[nextSlot].SetDefaults(mod.ItemType("ExampleWings"));
				nextSlot++;
			}
			if (Main.moonPhase < 2) {
				shop.item[nextSlot].SetDefaults(mod.ItemType("ExampleSword"));
				nextSlot++;
			}
			else if (Main.moonPhase < 4) {
				shop.item[nextSlot].SetDefaults(mod.ItemType("ExampleGun"));
				nextSlot++;
				shop.item[nextSlot].SetDefaults(mod.ItemType("ExampleBullet"));
				nextSlot++;
			}
			else if (Main.moonPhase < 6) {
				shop.item[nextSlot].SetDefaults(mod.ItemType("ExampleStaff"));
				nextSlot++;
			}
			else {
			}
			// Here is an example of how your npc can sell items from other mods.
			var modSummonersAssociation = ModLoader.GetMod("SummonersAssociation");
			if (modSummonersAssociation != null) {
				shop.item[nextSlot].SetDefaults(modSummonersAssociation.ItemType("BloodTalisman"));
				nextSlot++;
			}

			if (!Main.LocalPlayer.GetModPlayer<ExamplePlayer>().examplePersonGiftReceived && ExampleConfigServer.Instance.ExamplePersonFreeGiftList != null)
			{
				foreach (var item in ExampleConfigServer.Instance.ExamplePersonFreeGiftList)
				{
					if (item.IsUnloaded)
						continue;
					shop.item[nextSlot].SetDefaults(item.Type);
					shop.item[nextSlot].shopCustomPrice = 0;
					shop.item[nextSlot].GetGlobalItem<ExampleInstancedGlobalItem>().examplePersonFreeGift = true;
					nextSlot++;
					// TODO: Have tModLoader handle index issues.
				}	
			}
		}

		public override void NPCLoot() {
			Item.NewItem(npc.getRect(), mod.ItemType<Items.Armor.ExampleCostume>());
		}

		// Make this Town NPC teleport to the King and/or Queen statue when triggered.
		public override bool CanGoToStatue(bool toKingStatue) {
			return true;
		}

		// Make something happen when the npc teleports to a statue. Since this method only runs server side, any visual effects like dusts or gores have to be synced across all clients manually.
		public override void OnGoToStatue(bool toKingStatue) {
			if (Main.netMode == NetmodeID.Server) {
				ModPacket packet = mod.GetPacket();
				packet.Write((byte)ExampleModMessageType.ExampleTeleportToStatue);
				packet.Write((byte)npc.whoAmI);
				packet.Send();
			}
			else {
				StatueTeleport();
			}
		}

		// Create a square of pixels around the NPC on teleport.
		public void StatueTeleport() {
			for (int i = 0; i < 30; i++) {
				Vector2 position = Main.rand.NextVector2Square(-20, 21);
				if (Math.Abs(position.X) > Math.Abs(position.Y)) {
					position.X = Math.Sign(position.X) * 20;
				}
				else {
					position.Y = Math.Sign(position.Y) * 20;
				}
				Dust.NewDustPerfect(npc.Center + position, mod.DustType<Dusts.Pixel>(), Vector2.Zero).noGravity = true;
			}
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
