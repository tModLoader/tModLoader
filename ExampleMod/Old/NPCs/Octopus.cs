using ExampleMod.Items.Banners;
using ExampleMod.Projectiles;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.NPCs
{
	//ported from my tAPI mod because I'm lazy
	public class Octopus : Fish
	{
		public Octopus() {
			speed = 1f;
			speedY = 1f;
			acceleration = 0.05f;
			accelerationY = 0.05f;
			idleSpeed = 0.5f;
			bounces = false;
		}

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Octopus");
		}

		public override void SetDefaults() {
			npc.lifeMax = 1100;
			npc.damage = 160;
			npc.defense = 90;
			npc.knockBackResist = 0.3f;
			npc.width = 28;
			npc.height = 44;
			npc.aiStyle = -1;
			npc.noGravity = true;
			npc.HitSound = SoundID.NPCHit1;
			npc.DeathSound = SoundID.NPCDeath1;
			npc.value = Item.buyPrice(0, 0, 15, 0);
			npc.hide = true;
			banner = npc.type;
			bannerItem = ItemType<OctopusBanner>();
		}

		public override void AI() {
			if (npc.localAI[0] == 0f) {
				int damage = npc.damage / 2;
				if (Main.expertMode) {
					damage /= 2;
				}
				for (int k = 0; k < 6; k++) {
					int proj = Projectile.NewProjectile(npc.Center.X, npc.Center.Y, 0f, 0f, ProjectileType<OctopusArm>(), damage, 0, Main.myPlayer);
					if (proj == 1000) {
						npc.active = false;
						return;
					}
					OctopusArm arm = Main.projectile[proj].modProjectile as OctopusArm;
					arm.octopus = npc.whoAmI;
					arm.width = 16f;
					arm.length = OctopusArm.minLength;
					arm.minAngle = (k - 0.5f) * (float)Math.PI / 3f;
					arm.maxAngle = (k + 0.5f) * (float)Math.PI / 3f;
					Main.projectile[proj].rotation = (arm.minAngle + arm.maxAngle) / 2f;
					Main.projectile[proj].netUpdate = true;
				}
				npc.localAI[0] = 1f;
			}
			base.AI();
		}

		public override void FindFrame(int frameHeight) {
			npc.frame.Y = 0;
			npc.rotation = 0f;
		}

		public override void HitEffect(int hitDirection, double damage) {
			if (npc.life <= 0) {
				for (int k = 0; k < 20; k++) {
					Dust.NewDust(npc.position, npc.width, npc.height, 151, 2.5f * (float)hitDirection, -2.5f, 0, default(Color), 0.7f);
				}
				Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/OctopusHead"), 1f);
				for (int k = 0; k < 8; k++) {
					Vector2 pos = npc.position + new Vector2(Main.rand.Next(npc.width - 8), Main.rand.Next(npc.height / 2));
					Gore.NewGore(pos, npc.velocity, mod.GetGoreSlot("Gores/OctopusArm"), 1f);
				}
			}
			else {
				for (int k = 0; k < damage / npc.lifeMax * 50.0; k++) {
					Dust.NewDust(npc.position, npc.width, npc.height, 151, (float)hitDirection, -1f, 0, default(Color), 0.7f);
				}
			}
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return !spawnInfo.playerSafe && ExampleWorld.downedAbomination ? SpawnCondition.OceanMonster.Chance * 0.5f : 0f;
		}
	}
}