using ExampleMod.Content.Liquids;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.NPCs
{
	public class ExampleAnglerFish : ModNPC
	{
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.Piranha];

			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.DryadsWardDebuff] = true; //prevents the example liquid's debuff from damaging our NPC

			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers {
				Position = new Vector2(0f, 6f),
				PortraitPositionYOverride = 6f,
				IsWet = true
			});
		}

		public override void SetDefaults() {
			NPC.noGravity = true;
			NPC.width = 18;
			NPC.height = 20;
			NPC.aiStyle = 16;
			NPC.damage = 50;
			NPC.defense = 2;
			NPC.lifeMax = 260;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.value = 500f;
		}

		public override void FindFrame(int frameHeight) {
			NPC.spriteDirection = NPC.direction;
			NPC.frameCounter += 1.0;
			if (NPC.wet) {
				if (NPC.frameCounter < 6.0) {
					NPC.frame.Y = 0;
				}
				else if (NPC.frameCounter < 12.0) {
					NPC.frame.Y = frameHeight;
				}
				else if (NPC.frameCounter < 18.0) {
					NPC.frame.Y = frameHeight * 2;
				}
				else if (NPC.frameCounter < 24.0) {
					NPC.frame.Y = frameHeight * 3;
				}
				else {
					NPC.frameCounter = 0.0;
				}
			}
			else if (NPC.frameCounter < 6.0) {
				NPC.frame.Y = frameHeight * 4;
			}
			else if (NPC.frameCounter < 12.0) {
				NPC.frame.Y = frameHeight * 5;
			}
			else {
				NPC.frameCounter = 0.0;
			}
		}

		public override void HitEffect(NPC.HitInfo hit) {
			if (Main.netMode == NetmodeID.Server) {
				return;
			}

			if (NPC.life > 0) {
				for (int num438 = 0; (double)num438 < hit.Damage / (double)NPC.lifeMax * 20.0; num438++) {
					Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, hit.HitDirection, -1f);
				}
				return;
			}
			for (int num439 = 0; num439 < 10; num439++) {
				Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, 2 * hit.HitDirection, -2f);
			}
			Gore.NewGore(new EntitySource_Death(NPC), new Vector2(NPC.position.X, NPC.position.Y), NPC.velocity, 85);
		}

		//Here we do the liquid check before spawning our NPC
		//If the spawning position is in our modded liquid, then the NPC spawns
		//There is an additional region check to make sure its not too close to the ocean and world spawn
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			bool spawnTileIsInExampleLiquid = false;
			if (Main.tile[spawnInfo.SpawnTileX, spawnInfo.SpawnTileY - 1].LiquidAmount > 0 && Main.tile[spawnInfo.SpawnTileX, spawnInfo.SpawnTileY - 2].LiquidAmount > 0) {
				if (Main.tile[spawnInfo.SpawnTileX, spawnInfo.SpawnTileY - 1].LiquidType == ModContent.LiquidType<ExampleLiquid>()) {
					spawnTileIsInExampleLiquid = true;
				}
			}
			if (spawnTileIsInExampleLiquid && Main.rand.NextBool(4) && ((spawnInfo.SpawnTileX > WorldGen.oceanDistance && spawnInfo.SpawnTileX < Main.maxTilesX - WorldGen.oceanDistance) || spawnInfo.SpawnTileY > Main.worldSurface + 50.0)) {
				return 1f;
			}
			return 0f;
		}
	}
}
