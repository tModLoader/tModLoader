using ExampleMod.NPCs.Abomination;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Projectiles
{
	public class ElementLaser : ModProjectile
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Elemental Laser");
		}

		public override void SetDefaults() {
			projectile.width = 4;
			projectile.height = 4;
			projectile.timeLeft = 60;
			projectile.penetrate = -1;
			projectile.hostile = true;
			projectile.magic = true;
			projectile.tileCollide = false;
			projectile.ignoreWater = true;
		}

		public override void AI() {
			NPC npc = Main.npc[(int)projectile.ai[0]];
			if (projectile.localAI[0] == 0f) {
				if (npc.type == NPCType<CaptiveElement>() && npc.ai[1] == 2f && Main.expertMode) {
					cooldownSlot = 1;
				}
				projectile.Name = GetName();
			}
			projectile.Center = npc.Center;
			projectile.localAI[0] += 1f;
			projectile.alpha = (int)projectile.localAI[0] * 2;
			if (projectile.localAI[0] > 90f) {
				projectile.damage = 0;
			}
			if (projectile.localAI[0] > 120f) {
				projectile.Kill();
			}
		}

		public override void OnHitPlayer(Player target, int damage, bool crit) {
			if (Main.rand.NextBool() && projectile.ai[0] >= 0f) {
				int debuff = GetDebuff();
				if (debuff >= 0) {
					target.AddBuff(debuff, GetDebuffTime(), true);
				}
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			float point = 0f;
			Vector2 endPoint = Main.npc[(int)projectile.ai[1]].Center;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), projectile.Center, endPoint, 4f, ref point);
		}

		public string GetName() {
			NPC npc = Main.npc[(int)projectile.ai[0]];
			if (npc.type == NPCType<Abomination>()) {
				return "Fire Beam";
			}
			if (npc.ai[1] == 0f) {
				return "Frost Beam";
			}
			if (npc.ai[1] == 1f) {
				return "Spirit Beam";
			}
			if (npc.ai[1] == 2f) {
				return "Water Beam";
			}
			if (npc.ai[1] == 3f) {
				return "Venom Beam";
			}
			if (npc.ai[1] == 4f) {
				return "Ichor Beam";
			}
			return "Elemental Laser";
		}

		public Color GetColor() {
			NPC npc = Main.npc[(int)projectile.ai[0]];
			if (npc.type == NPCType<Abomination>()) {
				return new Color(250, 10, 0);
			}
			if (npc.ai[1] == 0f) {
				return new Color(0, 230, 230);
			}
			if (npc.ai[1] == 1f) {
				return new Color(0, 153, 230);
			}
			if (npc.ai[1] == 3f) {
				return new Color(0, 178, 0);
			}
			if (npc.ai[1] == 4f) {
				return new Color(230, 192, 0);
			}
			return Color.White;
		}

		public int GetDebuff() {
			NPC npc = Main.npc[(int)projectile.ai[0]];
			if (npc.type == NPCType<Abomination>()) {
				return BuffID.OnFire;
			}
			if (npc.ai[1] == 0f) {
				return BuffID.Frostburn;
			}
			if (npc.ai[1] == 1f) {
				return BuffType<Buffs.EtherealFlames>();
			}
			if (npc.ai[1] == 3f) {
				return BuffID.Venom;
			}
			if (npc.ai[1] == 4f) {
				return BuffID.Ichor;
			}
			return -1;
		}

		public int GetDebuffTime() {
			NPC npc = Main.npc[(int)projectile.ai[0]];
			if (npc.type == NPCType<Abomination>()) {
				return 600;
			}
			if (npc.ai[1] == 0f) {
				return 400;
			}
			if (npc.ai[1] == 1f) {
				return 300;
			}
			if (npc.ai[1] == 3f) {
				return 400;
			}
			if (npc.ai[1] == 4f) {
				return 900;
			}
			return -1;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
			Vector2 endPoint = Main.npc[(int)projectile.ai[1]].Center;
			Vector2 unit = endPoint - projectile.Center;
			float length = unit.Length();
			unit.Normalize();
			for (float k = 0; k <= length; k += 4f) {
				Vector2 drawPos = projectile.Center + unit * k - Main.screenPosition;
				Color alpha = GetColor() * ((255 - projectile.alpha) / 255f);
				spriteBatch.Draw(Main.projectileTexture[projectile.type], drawPos, null, alpha, k, new Vector2(2, 2), 1f, SpriteEffects.None, 0f);
			}
			return false;
		}
	}
}