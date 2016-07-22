using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using ExampleMod.Projectiles;

namespace ExampleMod.NPCs
{
	//ported from my tAPI mod because I'm lazy
	public class Octopus : Fish
	{
		public Octopus()
		{
			npc.name = "Octopus";
			speed = 1f;
			speedY = 1f;
			acceleration = 0.05f;
			accelerationY = 0.05f;
			idleSpeed = 0.5f;
			bounces = false;
		}

		public override void SetDefaults()
		{
			npc.name = "Octopus";
			npc.lifeMax = 1100;
			npc.damage = 160;
			npc.defense = 90;
			npc.knockBackResist = 0.3f;
			npc.width = 28;
			npc.height = 44;
			npc.aiStyle = -1;
			npc.noGravity = true;
			npc.soundHit = 1;
			npc.soundKilled = 1;
			npc.value = Item.buyPrice(0, 0, 15, 0);
			banner = npc.type;
			bannerItem = mod.ItemType("OctopusBanner");
		}

		public override void AI()
		{
			if (npc.localAI[0] == 0f)
			{
				int damage = npc.damage / 2;
				if (Main.expertMode)
				{
					damage /= 2;
				}
				for (int k = 0; k < 6; k++)
				{
					int proj = Projectile.NewProjectile(npc.Center.X, npc.Center.Y, 0f, 0f, mod.ProjectileType("OctopusArm"), damage, 0, Main.myPlayer);
					if (proj == 1000)
					{
						npc.active = false;
						return;
					}
					OctopusArm arm = Main.projectile[proj].modProjectile as OctopusArm;
					arm.octopus = npc.whoAmI;
					arm.width = 16f;
					arm.length = Projectiles.OctopusArm.minLength;
					arm.minAngle = (k - 0.5f) * (float)Math.PI / 3f;
					arm.maxAngle = (k + 0.5f) * (float)Math.PI / 3f;
					Main.projectile[proj].rotation = (arm.minAngle + arm.maxAngle) / 2f;
					Main.projectile[proj].netUpdate = true;
				}
				npc.localAI[0] = 1f;
			}
			base.AI();
		}

		public override void FindFrame(int frameHeight)
		{
			npc.frame.Y = 0;
			npc.rotation = 0f;
		}

		public override void HitEffect(int hitDirection, double damage)
		{
			if (npc.life <= 0)
			{
				for (int k = 0; k < 20; k++)
				{
					Dust.NewDust(npc.position, npc.width, npc.height, 151, 2.5f * (float)hitDirection, -2.5f, 0, default(Color), 0.7f);
				}
				Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/OctopusHead"), 1f);
				for (int k = 0; k < 8; k++)
				{
					Vector2 pos = npc.position + new Vector2(Main.rand.Next(npc.width - 8), Main.rand.Next(npc.height / 2));
					Gore.NewGore(pos, npc.velocity, mod.GetGoreSlot("Gores/OctopusArm"), 1f);
				}
			}
			else
			{
				for (int k = 0; k < damage / npc.lifeMax * 50.0; k++)
				{
					Dust.NewDust(npc.position, npc.width, npc.height, 151, (float)hitDirection, -1f, 0, default(Color), 0.7f);
				}
			}
		}

		public override float CanSpawn(NPCSpawnInfo spawnInfo)
		{
			int x = spawnInfo.spawnTileX;
			int y = spawnInfo.spawnTileY;
			int tile = (int)Main.tile[x, y].type;
			return (ExampleMod.NormalSpawn(spawnInfo) && (tile == 53 || tile == 112 || tile == 116 || tile == 234) && ExampleMod.NoZoneAllowWater(spawnInfo) && spawnInfo.water) && y < Main.rockLayer && (x < 250 || x > Main.maxTilesX - 250) && !spawnInfo.playerSafe && ExampleWorld.downedAbomination ? 0.5f : 0f;
		}
	}
}