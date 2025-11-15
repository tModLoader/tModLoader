using ExampleMod.Content.Biomes;
using ExampleMod.Content.EmoteBubbles;
using ExampleMod.Content.Items;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.Utilities;

namespace ExampleMod.Content.NPCs
{
	// The ExampleZombieThief is essentially the same as a regular Zombie, but it steals ExampleItems and keep them until it is killed, being saved with the world if it has enough of them.
	public class ExampleZombieThief : ModNPC
	{
		public int StolenItems = 0;

		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.Zombie];

			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				// Influences how the NPC looks in the Bestiary
				Velocity = 1f // Draws the NPC in the bestiary as if its walking +1 tiles in the x direction
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
		}

		public override void SetDefaults() {
			NPC.width = 18;
			NPC.height = 40;
			NPC.damage = 14;
			NPC.defense = 6;
			NPC.lifeMax = 200;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath2;
			NPC.value = 60f;
			NPC.knockBackResist = 0.5f;
			NPC.aiStyle = NPCAIStyleID.Fighter; // Fighter AI, important to choose the aiStyle that matches the NPCID that we want to mimic

			AIType = NPCID.Zombie; // Use vanilla zombie's type when executing AI code. (This also means it will try to despawn during daytime)
			AnimationType = NPCID.Zombie; // Use vanilla zombie's type when executing animation code. Important to also match Main.npcFrameCount[NPC.type] in SetStaticDefaults.
			Banner = Item.NPCtoBanner(NPCID.Zombie); // Makes this NPC get affected by the normal zombie banner.
			BannerItem = Item.BannerToItem(Banner); // Makes kills of this NPC go towards dropping the banner it's associated with.
			SpawnModBiomes = [ModContent.GetInstance<ExampleSurfaceBiome>().Type]; // Associates this NPC with the ExampleSurfaceBiome in Bestiary
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			// We can use AddRange instead of calling Add multiple times in order to add multiple items at once
			bestiaryEntry.Info.AddRange([
				// Sets the spawning conditions of this NPC that is listed in the bestiary.
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,

				// Sets the description of this NPC that is listed in the bestiary.
				new FlavorTextBestiaryInfoElement("Mods.ExampleMod.Bestiary.ExampleZombieThief"),
			]);
		}

		public override void AI() {
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				return;
			}

			Rectangle hitbox = NPC.Hitbox;
			foreach (Item item in Main.item) {
				// Pickup the items only if the NPC touches them and they aren't already being grabbed by a player
				if (item.active && !item.beingGrabbed && item.type == ModContent.ItemType<ExampleItem>() && hitbox.Intersects(item.Hitbox)) {
					item.active = false;
					StolenItems += item.stack;

					NetMessage.SendData(MessageID.SyncItem, number: item.whoAmI);

					// Show emote when stealing an example item
					EmoteBubble.NewBubble(ModContent.EmoteBubbleType<ExampleItemEmote>(), new WorldUIAnchor(NPC), 90);
				}
			}
		}

		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(StolenItems);
		}

		public override void ReceiveExtraAI(BinaryReader reader) {
			StolenItems = reader.ReadInt32();
		}

		public override void OnKill() {
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				return;
			}

			// Drop all the stolen items when the NPC dies
			while (StolenItems > 0) {
				// Loop until all items are dropped, to avoid dropping more than maxStack items
				int droppedAmount = Math.Min(ModContent.GetInstance<ExampleItem>().Item.maxStack, StolenItems);
				StolenItems -= droppedAmount;
				Item.NewItem(NPC.GetSource_Death(), NPC.Center, ModContent.ItemType<ExampleItem>(), droppedAmount, true);
			}
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			// Can only spawn in the ExampleSurfaceBiome and if there are no other ExampleZombieThiefs
			if (spawnInfo.Player.InModBiome(ModContent.GetInstance<ExampleSurfaceBiome>()) && !NPC.AnyNPCs(Type)) {
				return SpawnCondition.OverworldNightMonster.Chance * 0.1f; // Spawn with 1/10th the chance of a regular zombie.
			}

			return 0f;
		}

		public override bool NeedSaving() {
			return StolenItems >= 10; // Only save if the NPC has more than 10 stolen items, to avoid keeping the NPC in memory if it only has few
		}

		public override void SaveData(TagCompound tag) {
			if (StolenItems > 0) {
				// Note that at this point it may have less than 10 stolen items, if another mod or part of our decides to save the NPC
				tag["StolenItems"] = StolenItems;
			}
		}

		public override void LoadData(TagCompound tag) {
			StolenItems = tag.GetInt("StolenItems");
		}
	}
}