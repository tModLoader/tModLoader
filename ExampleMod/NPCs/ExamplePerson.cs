using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.NPCs
{
	public class ExamplePerson : ModNPC
	{
		public override bool Autoload(ref string name, ref string texture, ref string[] altTextures)
		{
			name = "Example Person";
			altTextures = new string[] { "ExampleMod/NPCs/ExamplePerson_Alt_1" };
			return mod.Properties.Autoload;
		}

		public override void SetDefaults()
		{
			npc.name = "Example Person";
			npc.townNPC = true;
			npc.friendly = true;
			npc.width = 18;
			npc.height = 40;
			npc.aiStyle = 7;
			npc.damage = 10;
			npc.defense = 15;
			npc.lifeMax = 250;
			npc.soundHit = 1;
			npc.soundKilled = 1;
			npc.knockBackResist = 0.5f;
			Main.npcFrameCount[npc.type] = 25;
			NPCID.Sets.ExtraFramesCount[npc.type] = 9;
			NPCID.Sets.AttackFrameCount[npc.type] = 4;
			NPCID.Sets.DangerDetectRange[npc.type] = 700;
			NPCID.Sets.AttackType[npc.type] = 0;
			NPCID.Sets.AttackTime[npc.type] = 90;
			NPCID.Sets.AttackAverageChance[npc.type] = 30;
			NPCID.Sets.HatOffsetY[npc.type] = 4;
			NPCID.Sets.ExtraTextureCount[npc.type] = 1;
			animationType = NPCID.Guide;
		}

		public override void HitEffect(int hitDirection, double damage)
		{
			int num = npc.life > 0 ? 1 : 5;
			for (int k = 0; k < num; k++)
			{
				Dust.NewDust(npc.position, npc.width, npc.height, mod.DustType("Sparkle"));
			}
		}

		public override bool CanTownNPCSpawn(int numTownNPCs, int money)
		{
			for (int k = 0; k < 255; k++)
			{
				Player player = Main.player[k];
				if (player.active)
				{
					for (int j = 0; j < player.inventory.Length; j++)
					{
						if (player.inventory[j].type == mod.ItemType("ExampleItem") || player.inventory[j].type == mod.ItemType("ExampleBlock"))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public override bool CheckConditions(int left, int right, int top, int bottom)
		{
			int score = 0;
			for (int x = left; x <= right; x++)
			{
				for (int y = top; y <= bottom; y++)
				{
					int type = Main.tile[x, y].type;
					if (type == mod.TileType("ExampleBlock") || type == mod.TileType("ExampleChair") || type == mod.TileType("ExampleWorkbench") || type == mod.TileType("ExampleBed") || type == mod.TileType("ExampleDoorOpen") || type == mod.TileType("ExampleDoorClosed"))
					{
						score++;
					}
					if (Main.tile[x, y].wall == mod.WallType("ExampleWall"))
					{
						score++;
					}
				}
			}
			return score >= (right - left) * (bottom - top) / 2;
		}

		public override string TownNPCName()
		{
			switch (WorldGen.genRand.Next(4))
			{
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

		public override void FindFrame(int frameHeight)
		{
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

		public override string GetChat()
		{
			int partyGirl = NPC.FindFirstNPC(NPCID.PartyGirl);
			if (partyGirl >= 0 && Main.rand.Next(4) == 0)
			{
				return "Can you please tell " + Main.npc[partyGirl].displayName + " to stop decorating my house with colors?";
			}
			switch (Main.rand.Next(3))
			{
				case 0:
					return "Sometimes I feel like I'm different from everyone else here.";
				case 1:
					return "What's your favorite color? My favorite colors are white and black.";
				default:
					return "What? I don't have any arms or legs? Oh, don't be ridiculous!";
			}
		}

		public override void SetChatButtons(ref string button, ref string button2)
		{
			button = Lang.inter[28];
		}

		public override void OnChatButtonClicked(bool firstButton, ref bool shop)
		{
			if (firstButton)
			{
				shop = true;
			}
		}

		public override void SetupShop(Chest shop, ref int nextSlot)
		{
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
			if (Main.player[Main.myPlayer].GetModPlayer<ExamplePlayer>(mod).ZoneExample)
			{
				shop.item[nextSlot].SetDefaults(mod.ItemType("ExampleWings"));
				nextSlot++;
			}
			if (Main.moonPhase < 2)
			{
				shop.item[nextSlot].SetDefaults(mod.ItemType("ExampleSword"));
				nextSlot++;
			}
			else if (Main.moonPhase < 4)
			{
				shop.item[nextSlot].SetDefaults(mod.ItemType("ExampleGun"));
				nextSlot++;
				shop.item[nextSlot].SetDefaults(mod.ItemType("ExampleBullet"));
				nextSlot++;
			}
			else if (Main.moonPhase < 6)
			{
				shop.item[nextSlot].SetDefaults(mod.ItemType("ExampleStaff"));
				nextSlot++;
			}
			else
			{
			}
			// Here is an example of how your npc can sell items from other mods.
			if (ModLoader.GetLoadedMods().Contains("SummonersAssociation"))
			{
				shop.item[nextSlot].SetDefaults(ModLoader.GetMod("SummonersAssociation").ItemType("BloodTalisman"));
				nextSlot++;
			}
		}

		public override void TownNPCAttackStrength(ref int damage, ref float knockback)
		{
			damage = 20;
			knockback = 4f;
		}

		public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
		{
			cooldown = 30;
			randExtraCooldown = 30;
		}

		public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
		{
			projType = mod.ProjectileType("SparklingBall");
			attackDelay = 1;
		}

		public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
		{
			multiplier = 12f;
			randomOffset = 2f;
		}
	}
}