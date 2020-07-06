using ExampleMod.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;

namespace ExampleMod.Projectiles
{
	//imported from my tAPI mod because I'm lazy
	public class OctopusArm : ModProjectile
	{
		public float width {
			get => projectile.ai[0];
			set => projectile.ai[0] = value;
		}

		public float length {
			get => projectile.ai[1];
			set => projectile.ai[1] = value;
		}

		public float minAngle {
			get => projectile.localAI[0];
			set => projectile.localAI[0] = value;
		}

		public float maxAngle {
			get => projectile.localAI[1];
			set => projectile.localAI[1] = value;
		}

		public float angleSpeed;
		public float lengthSpeed;
		public int octopus = -1;
		private int netUpdateCounter;
		private const float maxAngleSpeed = 0.01f;
		private const float angleBuffer = (float)Math.PI / 12f;
		public const float minLength = 80f;
		private const float maxLength = 400f;
		private const float maxLengthSpeed = 1f;

		public override void SetDefaults() {
			projectile.width = 1;
			projectile.height = 1;
			projectile.hostile = true;
			projectile.timeLeft = 2;
			projectile.penetrate = -1;
			projectile.tileCollide = false;
			projectile.ignoreWater = true;
		}

		public override void AI() {
			NPC npc = Main.npc[octopus];
			if (!npc.active || npc.type != NPCType<Octopus>()) {
				return;
			}
			projectile.timeLeft = 2;
			Player player = Main.player[npc.target];
			projectile.position = npc.Center;
			Vector2 offset = player.Center - projectile.position;
			float distance = offset.Length() + 32f;
			Angle currAngle = new Angle(projectile.rotation);
			Angle angleToPlayer = new Angle((float)Math.Atan2(offset.Y, offset.X));
			Angle min = new Angle(minAngle);
			Angle max = new Angle(maxAngle);
			Angle limit = new Angle((minAngle + maxAngle) / 2f);
			if (limit.Between(min, max)) {
				limit = limit.Opposite();
			}
			Angle buffer = new Angle(angleBuffer);
			if (angleToPlayer.Between(min - buffer, max + buffer)) {
				if (currAngle.Between(max, limit)) {
					angleSpeed -= maxAngleSpeed / 10f;
				}
				else if (currAngle.Between(limit, min)) {
					angleSpeed += maxAngleSpeed / 10f;
				}
				else if (currAngle.ClockwiseFrom(angleToPlayer)) {
					angleSpeed += maxAngleSpeed / 10f;
				}
				else {
					angleSpeed -= maxAngleSpeed / 10f;
				}
				if (length > maxLength) {
					lengthSpeed -= maxLengthSpeed / 10f;
				}
				else if (length < minLength) {
					lengthSpeed += maxLengthSpeed / 10f;
				}
				else if (distance > length) {
					lengthSpeed += maxLengthSpeed / 10f;
				}
				else if (distance < length) {
					lengthSpeed -= maxLengthSpeed / 10f;
				}
			}
			else {
				if (currAngle.Between(max, limit)) {
					angleSpeed -= maxAngleSpeed / 10f;
				}
				else if (currAngle.Between(limit, min)) {
					angleSpeed += maxAngleSpeed / 10f;
				}
				else if (angleSpeed > 0f) {
					angleSpeed += maxAngleSpeed / 20f;
				}
				else if (angleSpeed < 0f) {
					angleSpeed -= maxAngleSpeed / 20f;
				}
				else {
					angleSpeed = maxAngleSpeed / 20f;
				}
				if (length > minLength) {
					lengthSpeed -= maxLengthSpeed / 10f;
				}
				else {
					lengthSpeed += maxLengthSpeed / 10f;
				}
			}
			if (angleSpeed > maxAngleSpeed) {
				angleSpeed = maxAngleSpeed;
			}
			else if (angleSpeed < -maxAngleSpeed) {
				angleSpeed = -maxAngleSpeed;
			}
			if (lengthSpeed > maxLengthSpeed) {
				lengthSpeed = maxLengthSpeed;
			}
			else if (lengthSpeed < -maxLengthSpeed) {
				lengthSpeed = -maxLengthSpeed;
			}
			projectile.rotation += angleSpeed;
			length += lengthSpeed;
			if (Main.netMode == NetmodeID.Server) {
				netUpdateCounter++;
				if (netUpdateCounter >= 300) {
					projectile.netUpdate = true;
					netUpdateCounter = 0;
				}
			}
		}

		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(projectile.rotation);
			writer.Write(minAngle);
			writer.Write(maxAngle);
			writer.Write(angleSpeed);
			writer.Write(lengthSpeed);
			writer.Write((short)octopus);
		}

		public override void ReceiveExtraAI(BinaryReader reader) {
			projectile.rotation = reader.ReadSingle();
			minAngle = reader.ReadSingle();
			maxAngle = reader.ReadSingle();
			angleSpeed = reader.ReadSingle();
			lengthSpeed = reader.ReadSingle();
			octopus = reader.ReadInt16();
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			float point = 0f;
			return Collision.CheckAABBvLineCollision(new Vector2(targetHitbox.X, targetHitbox.Y), new Vector2(targetHitbox.Width, targetHitbox.Height), projectile.position, projectile.position + length * new Vector2((float)Math.Cos(projectile.rotation), (float)Math.Sin(projectile.rotation)), width, ref point);
		}

		public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit) {
			projectile.rotation %= 2f * (float)Math.PI;
			if (projectile.rotation % (float)Math.PI == 0f) {
				projectile.direction = -target.direction;
			}
			else if (projectile.rotation % (float)Math.PI / 2f == 0f) {
				projectile.direction = target.Center.X < projectile.position.X ? -1 : 1;
			}
			else {
				float yOffset = target.Center.Y - projectile.position.Y;
				float x = projectile.position.X + yOffset / (float)Math.Tan(projectile.rotation);
				projectile.direction = target.Center.X < x ? -1 : 1;
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
			Texture2D unit = Main.projectileTexture[projectile.type];
			int unitLength = unit.Width;
			int numUnits = (int)Math.Ceiling(length / unitLength);
			float increment = 0f;
			if (numUnits > 1) {
				increment = (length - unitLength) / (numUnits - 1);
			}
			Vector2 direction = new Vector2((float)Math.Cos(projectile.rotation), (float)Math.Sin(projectile.rotation));
			SpriteEffects effects = SpriteEffects.None;
			if (projectile.spriteDirection == -1) {
				effects = SpriteEffects.FlipVertically;
			}
			for (int k = 1; k <= numUnits; k++) {
				Texture2D image = unit;
				if (k == numUnits) {
					image = mod.GetTexture("Projectiles/OctopusArmTip");
				}
				Vector2 pos = projectile.position + direction * (increment * (k - 1) + unitLength / 2f);
				Color color = Lighting.GetColor((int)(pos.X / 16f), (int)(pos.Y / 16f));
				spriteBatch.Draw(image, pos - Main.screenPosition, null, projectile.GetAlpha(color), projectile.rotation, new Vector2(unit.Width / 2, unit.Height / 2), 1f, effects, 0f);
			}
			return false;
		}

		public override void PostDraw(SpriteBatch sb, Color lightColor) {
			Main.instance.DrawNPC(octopus, false);
		}
	}
}