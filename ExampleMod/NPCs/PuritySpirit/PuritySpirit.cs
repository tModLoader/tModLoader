using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ExampleMod.Projectiles.PuritySpirit;

namespace ExampleMod.NPCs.PuritySpirit
{
	public class PuritySpirit : ModNPC
	{
		private const int size = 120;
		private const int particleSize = 12;
		public static readonly int arenaWidth = (int)(1.3f * NPC.sWidth);
		public static readonly int arenaHeight = (int)(1.3f * NPC.sHeight);

		public override void SetDefaults()
		{
			npc.name = "PuritySpirit";
			npc.displayName = "Spirit of Purity";
			npc.aiStyle = -1;
			npc.lifeMax = 200000;
			npc.damage = 0;
			npc.defense = 70;
			npc.knockBackResist = 0f;
			npc.dontTakeDamage = true;
			npc.width = size;
			npc.height = size;
			npc.value = Item.buyPrice(0, 50, 0, 0);
			npc.npcSlots = 50f;
			npc.boss = true;
			npc.lavaImmune = true;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.soundHit = 1;
			npc.soundKilled = 0;
			npc.alpha = 255;
			for (int k = 0; k < npc.buffImmune.Length; k++)
			{
				npc.buffImmune[k] = true;
			}
			NPCID.Sets.MustAlwaysDraw[npc.type] = true;
			music = MusicID.Title;
		}

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
		{
			npc.lifeMax = (int)(npc.lifeMax / Main.expertLife * 1.2f * bossLifeScale);
			npc.defense = 72;
		}

		private int difficulty
		{
			get
			{
				double strength = (double)npc.life / (double)npc.lifeMax;
				int difficulty = (int)(4.0 * (1.0 - strength));
				if (Main.expertMode)
				{
					difficulty++;
				}
				return difficulty;
			}
		}

		private float difficultyGradient
		{
			get
			{
				double strength = (double)npc.life / (double)npc.lifeMax;
				double difficulty = 4.0 * (1.0 - strength);
				return (float)(difficulty % 1.0);
			}
		}

		private float timeMultiplier
		{
			get
			{
				return 1f - (difficulty + difficultyGradient) * 0.2f;
			}
		}

		private int stage
		{
			get
			{
				return (int)npc.ai[0];
			}
			set
			{
				npc.ai[0] = value;
			}
		}

		private float attackTimer
		{
			get
			{
				return npc.ai[1];
			}
			set
			{
				npc.ai[1] = value;
			}
		}

		private int attack
		{
			get
			{
				return (int)npc.ai[2];
			}
			set
			{
				npc.ai[2] = value;
			}
		}

		private int attackProgress
		{
			get
			{
				return (int)npc.ai[3];
			}
			set
			{
				npc.ai[3] = value;
			}
		}

		private int portalFrame
		{
			get
			{
				return (int)npc.localAI[0];
			}
			set
			{
				npc.localAI[0] = value;
			}
		}

		private IList<Particle> particles = new List<Particle>();
		private float[,] aura = new float[size, size];
		private const int dpsCap = 4000;
		private int damageTotal = 0;
		private bool saidRushMessage = false;
		private readonly IList<int> targets = new List<int>();
		public int[] attackWeights = new int[]{ 2000, 2000, 2000, 2000, 3000 };
		private const int minAttackWeight = 1000;
		private const int maxAttackWeight = 4000;

		public override void AI()
		{
			if (!Main.dedServ)
			{
				UpdateParticles();
				portalFrame++;
				portalFrame %= 6 * Main.projFrames[ProjectileID.PortalGunGate];
			}
			FindPlayers();
			if (targets.Count > 0 && npc.timeLeft < NPC.activeTime)
			{
				npc.timeLeft = NPC.activeTime;
			}
			damageTotal -= dpsCap;
			if (damageTotal < 0)
			{
				damageTotal = 0;
			}
			if (Main.netMode == 1)
			{
				return;
			}
			switch (stage)
			{
				case 0:
					Initialize();
					break;
				case 1:
					UltimateAttack();
					if (attackProgress == 0)
					{
						stage++;
						attackTimer = 160f * timeMultiplier;
						attack = -1;
					}
					break;
				case 2:
				case 4:
				case 6:
				case 8:
					DoAttack(4);
					break;
				case 10:
					DoAttack(5);
					break;
			}
		}

		private void UpdateParticles()
		{
			foreach (Particle particle in particles)
			{
				particle.Update();
			}
			Vector2 newPos = new Vector2(Main.rand.Next(3 * size / 8, 5 * size / 8), Main.rand.Next(3 * size / 8, 5 * size / 8));
			double newAngle = 2 * Math.PI * Main.rand.NextDouble();
			Vector2 newVel = new Vector2((float)Math.Cos(newAngle), (float)Math.Sin(newAngle));
			newVel *= 0.5f * (1f + (float)Main.rand.NextDouble());
			particles.Add(new Particle(newPos, newVel));
			if (particles[0].strength <= 0f)
			{
				particles.RemoveAt(0);
			}
			for (int x = 0; x < size; x++)
			{
				for (int y = 0; y < size; y++)
				{
					aura[x, y] *= 0.97f;
				}
			}
			foreach (Particle particle in particles)
			{
				int minX = (int)particle.position.X - particleSize / 2;
				int minY = (int)particle.position.Y - particleSize / 2;
				int maxX = minX + particleSize;
				int maxY = minY + particleSize;
				for (int x = minX; x <= maxX; x++)
				{
					for (int y = minY; y <= maxY; y++)
					{
						if (x >= 0 && x < size && y >= 0 && y < size)
						{
							float strength = particle.strength;
							float offX = particle.position.X - x;
							float offY = particle.position.Y - y;
							strength *= 1f - (float)Math.Sqrt(offX * offX + offY * offY) / particleSize * 2;
							if (strength < 0f)
							{
								strength = 0f;
							}
							aura[x, y] = 1f - (1f - aura[x, y]) * (1f - strength);
						}
					}
				}
			}
		}

		public void FindPlayers()
		{
			targets.Clear();
			for (int k = 0; k < 255; k++)
			{
				if (Main.player[k].active)
				{
					targets.Add(k);
				}
			}
		}

		public void Initialize()
		{
			attackProgress++;
			if (attackProgress == 90)
			{
				Talk("You, who have challenged me...");
			}
			if (attackProgress == 180)
			{
				SetupCrystals(arenaWidth / 6, true);
			}
			if (attackProgress >= 420)
			{
				Talk("Show me the power that has saved Terraria!");
				Main.PlaySound(15, -1, -1, 0);
				attackProgress = 0;
				stage++;
				npc.dontTakeDamage = false;
			}
		}

		private void SetupCrystals(int radius, bool clockwise)
		{
			if (Main.netMode == 1)
			{
				return;
			}
			Vector2 center = npc.Center;
			for (int k = 0; k < 10; k++)
			{
				float angle = 2f * (float)Math.PI / 10f * k;
				Vector2 pos = center + radius * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
				int damage = 80;
				if (Main.expertMode)
				{
					damage = (int)(100 / Main.expertDamage);
				}
				int proj = Projectile.NewProjectile(pos.X, pos.Y, 0f, 0f, mod.ProjectileType("PureCrystal"), damage, 0f, Main.myPlayer, npc.whoAmI, angle);
				Main.projectile[proj].localAI[0] = radius;
				Main.projectile[proj].localAI[1] = clockwise ? 1 : -1;
				Main.projectile[proj].netUpdate = true;
			}
		}

		private void UltimateAttack()
		{
			attackProgress++;
			if (attackProgress <= 300 && Main.netMode != 1)
			{
				const int interval = 60;
				float x, y;
				if (attackProgress == 100)
				{
					int k = targets[Main.rand.Next(targets.Count)];
					x = Main.player[k].Center.X;
					y = Main.player[k].Center.Y;
				}
				else if (Main.rand.Next(5) == 0)
				{
					int k = targets[Main.rand.Next(targets.Count)];
					x = Main.player[k].Center.X + interval * Main.rand.Next(-5, 6);
					y = Main.player[k].Center.Y + interval * Main.rand.Next(-5, 6);
					if (x < npc.Center.X - arenaWidth / 2)
					{
						x += arenaWidth;
					}
					else if (x > npc.Center.X + arenaWidth / 2)
					{
						x -= arenaWidth;
					}
					if (y < npc.Center.Y - arenaHeight / 2)
					{
						y += arenaHeight;
					}
					else if (y > npc.Center.Y + arenaHeight / 2)
					{
						y -= arenaHeight;
					}
				}
				else
				{
					int leftBound = (-arenaWidth / 2 + 40) / interval;
					int rightBound = (arenaWidth / 2 - 40) / interval + 1;
					int upperBound = (-arenaHeight / 2 + 40) / interval;
					int lowerBound = (arenaHeight / 2 - 40) / interval + 1;
					x = npc.Center.X + interval * Main.rand.Next(leftBound, rightBound);
					y = npc.Center.Y + interval * Main.rand.Next(upperBound, lowerBound);
				}
				int damage = 500;
				int proj = Projectile.NewProjectile(x, y, 0f, 0f, mod.ProjectileType("VoidWorld"), damage, 0f, Main.myPlayer, 0f, 2 * Main.rand.Next(2) - 1);
				if (Main.rand.Next(10) == 0)
				{
					Main.projectile[proj].localAI[0] = 1f;
				}
			}
			if (attackProgress >= 480)
			{
				attackProgress = 0;
			}
		}

		private void DoAttack(int numAttacks)
		{
			if (attackTimer > 0f)
			{
				attackTimer -= 1f;
				return;
			}
			if (attack < 0)
			{
				int totalWeight = 0;
				for (int k = 0; k < numAttacks; k++)
				{
					totalWeight += attackWeights[k];
				}
				int choice = Main.rand.Next(totalWeight);
				for (attack = 0; attack < numAttacks; attack++)
				{
					if (choice < attackWeights[attack])
					{
						break;
					}
					choice -= attackWeights[attack];
				}
				attack = 0;
				npc.netUpdate = true;
			}
			switch (attack)
			{
				case 0:
					BeamAttack();
					break;
				case 1:
					SnakeAttack();
					break;
				case 2:
					LaserAttack();
					break;
				case 3:
					SphereAttack();
					break;
				case 4:
					UltimateAttack();
					break;
			}
			if (attackProgress == 0)
			{
				attackTimer += 160f * timeMultiplier;
				attack = -1;
			}
		}

		private void BeamAttack()
		{
			if (attackProgress == 0)
			{
				float y = npc.Center.Y;
				int damage = 250;
				for (int k = 0; k < targets.Count; k++)
				{
					float x = Main.player[targets[k]].Center.X;
					Projectile.NewProjectile(x, y, 0f, 0f, mod.ProjectileType("PurityBeam"), damage, 0f, Main.myPlayer, arenaHeight);
					for (int j = -1; j <= 1; j += 2)
					{
						float spawnX = x + j * Main.rand.Next(200, 401);
						if (spawnX > npc.Center.X + arenaWidth / 2)
						{
							spawnX -= arenaWidth;
						}
						else if (spawnX < npc.Center.X - arenaWidth / 2)
						{
							spawnX += arenaWidth;
						}
						Projectile.NewProjectile(spawnX, y, 0f, 0f, mod.ProjectileType("PurityBeam"), damage, 0f, Main.myPlayer, arenaHeight);
					}
				}
				for (int k = 0; k < 2 * (difficulty + 1); k++)
				{
					Projectile.NewProjectile(npc.Center.X + Main.rand.Next(-arenaWidth / 2 + 50, arenaWidth / 2 - 50 + 1), y, 0f, 0f, mod.ProjectileType("PurityBeam"), damage, 0f, Main.myPlayer, arenaHeight);
				}
				attackProgress = (int)(PurityBeam.charge + 60f);
			}
			attackProgress--;
			if (attackProgress < 0)
			{
				attackProgress = 0;
			}
		}

		private void SnakeAttack()
		{
		}

		private void LaserAttack()
		{
		}

		private void SphereAttack()
		{
		}

		private void GiveDebuffs()
		{
		}

		public override bool? CanBeHitByItem(Player player, Item item)
		{
			return CanBeHitByPlayer(player);
		}

		public override void ModifyHitByItem(Player player, Item item, ref int damage, ref float knockback, ref bool crit)
		{
			ModifyHit(ref damage);
		}

		public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit)
		{
			OnHit(damage);
		}

		public override bool? CanBeHitByProjectile(Projectile projectile)
		{
			return CanBeHitByPlayer(Main.player[projectile.owner]);
		}

		public override void ModifyHitByProjectile(Projectile projectile, ref int damage, ref float knockback, ref bool crit)
		{
			ModifyHit(ref damage);
		}

		public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit)
		{
			OnHit(damage);
		}

		private bool? CanBeHitByPlayer(Player player)
		{
			if (!targets.Contains(player.whoAmI))
			{
				return false;
			}
			return null;
		}

		private void ModifyHit(ref int damage)
		{
			if (damage > npc.lifeMax / 8)
			{
				damage = npc.lifeMax / 8;
			}
		}

		private void OnHit(int damage)
		{
			damageTotal += damage * 60;
			//TODO - send information when server support is finished
		}

		public override bool StrikeNPC(ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit)
		{
			if (damageTotal >= dpsCap)
			{
				if (!saidRushMessage)
				{
					Talk("Oh, in a rush now, are we?");
					saidRushMessage = true;
				}
				damage = 0;
				return false;
			}
			return true;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
		{
			for (int x = 0; x < size; x++)
			{
				for (int y = 0; y < size; y++)
				{
					Vector2 drawPos = npc.position - Main.screenPosition;
					drawPos.X += x * 2 - size / 2;
					drawPos.Y += y * 2 - size / 2;
					spriteBatch.Draw(mod.GetTexture("NPCs/PuritySpirit/PurityParticle"), drawPos, null, Color.White * aura[x, y], 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
				}
			}
			spriteBatch.Draw(mod.GetTexture("NPCs/PuritySpirit/PurityEyes"), npc.position - Main.screenPosition, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
			return false;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
		{
			int portalWidth = 48;
			int portalDepth = 18;
			Color color = new Color(64, 255, 64);
			int centerX = (int)npc.Center.X;
			int centerY = (int)npc.Center.Y;
			Main.instance.LoadProjectile(ProjectileID.PortalGunGate);
			for (int x = centerX - arenaWidth / 2; x < centerX + arenaWidth / 2; x += portalWidth)
			{
				int frameNum = (portalFrame / 6 + x / portalWidth) % Main.projFrames[ProjectileID.PortalGunGate];
				Rectangle frame = new Rectangle(0, frameNum * (portalWidth + 2), portalDepth, portalWidth);
				Vector2 drawPos = new Vector2(x + portalWidth / 2, centerY - arenaHeight / 2) - Main.screenPosition;
				spriteBatch.Draw(Main.projectileTexture[ProjectileID.PortalGunGate], drawPos, frame, color, (float)-Math.PI / 2f, new Vector2(portalDepth / 2, portalWidth / 2), 1f, SpriteEffects.None, 0f);
				drawPos.Y += arenaHeight;
				spriteBatch.Draw(Main.projectileTexture[ProjectileID.PortalGunGate], drawPos, frame, color, (float)Math.PI / 2f, new Vector2(portalDepth / 2, portalWidth / 2), 1f, SpriteEffects.None, 0f);
			}
			for (int y = centerY - arenaHeight / 2; y < centerY + arenaHeight / 2; y += portalWidth)
			{
				int frameNum = (portalFrame / 6 + y / portalWidth) % Main.projFrames[ProjectileID.PortalGunGate];
				Rectangle frame = new Rectangle(0, frameNum * (portalWidth + 2), portalDepth, portalWidth);
				Vector2 drawPos = new Vector2(centerX - arenaWidth / 2, y + portalWidth / 2) - Main.screenPosition;
				spriteBatch.Draw(Main.projectileTexture[ProjectileID.PortalGunGate], drawPos, frame, color, (float)Math.PI, new Vector2(portalDepth / 2, portalWidth / 2), 1f, SpriteEffects.None, 0f);
				drawPos.X += arenaWidth;
				spriteBatch.Draw(Main.projectileTexture[ProjectileID.PortalGunGate], drawPos, frame, color, 0f, new Vector2(portalDepth / 2, portalWidth / 2), 1f, SpriteEffects.None, 0f);
			}
		}

		private void Talk(string message)
		{
			Main.NewText("<Spirit of Purity> " + message, 150, 250, 150);
		}
	}

	class Particle
	{
		internal Vector2 position;
		internal Vector2 velocity;
		internal float strength;

		internal Particle(Vector2 pos, Vector2 vel)
		{
			this.position = pos;
			this.velocity = vel;
			this.strength = 0.75f;
		}

		internal void Update()
		{
			this.position += this.velocity * this.strength;
			this.strength -= 0.01f;
		}
	}
}
