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

		internal int attack
		{
			get
			{
				return (int)npc.ai[2];
			}
			private set
			{
				npc.ai[2] = value;
			}
		}

		internal int attackProgress
		{
			get
			{
				return (int)npc.ai[3];
			}
			private set
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

		private int shieldTimer
		{
			get
			{
				return (int)npc.localAI[1];
			}
			set
			{
				npc.localAI[1] = value;
			}
		}

		private IList<Particle> particles = new List<Particle>();
		private float[,] aura = new float[size, size];
		internal const int dpsCap = 5000;
		private int damageTotal = 0;
		private bool saidRushMessage = false;
		public readonly IList<int> targets = new List<int>();
		public int[] attackWeights = new int[]{ 2000, 2000, 2000, 2000, 3000 };
		public const int minAttackWeight = 1000;
		public const int maxAttackWeight = 4000;

		public override void AI()
		{
			if (!Main.dedServ)
			{
				UpdateParticles();
				portalFrame++;
				portalFrame %= 6 * Main.projFrames[ProjectileID.PortalGunGate];
			}
			FindPlayers();
			npc.timeLeft = NPC.activeTime;
			if (stage > 0 && targets.Count == 0)
			{
				attackProgress = 0;
				stage = -1;
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
			if (stage == 2 && difficulty > 0)
			{
				Projectile.NewProjectile(npc.Center.X - arenaWidth / 2, npc.Center.Y, NegativeWall.speed, 0f, mod.ProjectileType("NegativeWall"), 0, 0f, Main.myPlayer, npc.whoAmI, arenaHeight);
				stage++;
			}
			if (stage == 3 && difficulty > 1)
			{
				SetupCrystals(arenaWidth / 3, false);
				stage++;
			}
			if (stage == 4 && difficulty > 2)
			{
				Projectile.NewProjectile(npc.Center.X, npc.Center.Y - arenaHeight / 2, 0f, NegativeWall.speed, mod.ProjectileType("NegativeWall"), 0, 0f, Main.myPlayer, npc.whoAmI, -arenaWidth);
				shieldTimer = 600;
				stage++;
			}
			if (stage == 5 && difficulty > 3)
			{
				stage++;
			}
			switch (stage)
			{
				case -1:
					RunAway();
					break;
				case 0:
					Initialize();
					break;
				case 1:
				case 11:
					attack = 4;
					UltimateAttack();
					if (attackProgress == 0)
					{
						stage++;
						attackTimer = 160f * timeMultiplier;
						attack = -1;
					}
					break;
				case 2:
				case 3:
				case 4:
					DoAttack(4);
					break;
				case 5:
					DoAttack(4);
					DoShield(1);
					break;
				case 6:
					DoAttack(5);
					DoShield(2);
					break;
				case 10:
					FinishFight1();
					break;
				case 12:
					FinishFight2();
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
				if (Main.player[k].active && ((ExamplePlayer)Main.player[k].GetModPlayer(mod, "ExamplePlayer")).heroLives > 0)
				{
					targets.Add(k);
				}
			}
		}

		public void RunAway()
		{
			attackProgress++;
			if (attackProgress == 180)
			{
				Talk("Hmph. Was that the extent of your power?");
			}
			if (attackProgress >= 360)
			{
				npc.active = false;
			}
		}

		public void Initialize()
		{
			if (attackProgress == 0)
			{
				Vector2 center = npc.Center;
				for (int k = 0; k < 255; k++)
				{
					Player player = Main.player[k];
					if (player.active && player.position.X > center.X - arenaWidth / 2 && player.position.X + player.width < center.X + arenaWidth / 2 && player.position.Y > center.Y - arenaHeight / 2 && player.position.Y + player.height < center.Y + arenaHeight / 2)
					{
						((ExamplePlayer)player.GetModPlayer(mod, "ExamplePlayer")).heroLives = 3;
					}
				}
			}
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
			if (attackProgress == 0)
			{
				Main.PlaySound(15, -1, -1, 0);
				if (Main.netMode != 1)
				{
					int damage = Main.expertMode ? 720 : 600;
					Projectile.NewProjectile(npc.Center.X, npc.Center.Y, 0f, 0f, mod.ProjectileType("VoidWorld"), damage, 0f, Main.myPlayer, npc.whoAmI);
				}
			}
			attackProgress++;
			if (attackProgress >= 500)
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
					if (attackWeights[k] < minAttackWeight)
					{
						attackWeights[k] = minAttackWeight;
					}
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
				attackWeights[attack] -= 80;
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
				int damage = Main.expertMode ? 360 : 300;
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
				int numExtra = 2 * (difficulty + 1) - 2 * (targets.Count - 1);
				if (difficulty >= 2)
				{
					numExtra--;
				}
				if (difficulty >= 4)
				{
					numExtra--;
				}
				for (int k = 0; k < numExtra; k++)
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
			if (attackProgress == 0)
			{
				int damage = Main.expertMode ? 60 : 80;
				Projectile.NewProjectile(npc.Center.X, npc.Center.Y, 0f, 0f, mod.ProjectileType("PuritySnake"), damage, 0f, Main.myPlayer, npc.whoAmI, timeMultiplier);
				attackProgress = 240;
			}
			attackProgress--;
			if (attackProgress < 0)
			{
				attackProgress = 0;
			}
		}

		private void LaserAttack()
		{
			if (attackProgress == 0)
			{
				int numAttacks = 3 + difficulty / 2;
				float timer = 30f + 20f * timeMultiplier;
				float totalTime = numAttacks * timer + 120f;
				int damage = Main.expertMode ? 55 : 80;
				for (int k = 0; k < numAttacks; k++)
				{
					int proj = Projectile.NewProjectile(npc.Center.X, npc.Center.Y, 0f, 0f, mod.ProjectileType("NullLaser"), damage, 0f, Main.myPlayer, npc.whoAmI, (int)(60f + k * timer));
					Main.projectile[proj].localAI[0] = (int)totalTime;
					((NullLaser)Main.projectile[proj].modProjectile).warningTime = timer;
					Main.projectile[proj].netUpdate = true;
				}
				attackProgress = (int)totalTime;
			}
			if (attackProgress % 20 == 0)
			{
				if (targets.Contains(Main.myPlayer))
				{
					Main.PlaySound(2, -1, -1, 15);
				}
				else
				{
					Main.PlaySound(2, (int)npc.position.X, (int)npc.position.Y, 15);
				}
			}
			Dust.NewDust(npc.position, npc.width, npc.height, mod.DustType("Sparkle"), 0f, 0f, 0, new Color(0, 180, 0), 1.5f);
			attackProgress--;
			if (attackProgress < 0)
			{
				attackProgress = 0;
			}
		}

		private void SphereAttack()
		{
			if (attackProgress == 0)
			{
				int damage = Main.expertMode ? 60 : 80;
				float time = 60f + 60f * timeMultiplier;
				int rotationSpeed = Main.rand.Next(2) * 2 - 1;
				int numSpheres = 3 + difficulty / 2;
				int numGroups = 4 + difficulty / 3;
				float radius = PuritySphere.radius;
				for (int j = 0; j < numGroups || j < targets.Count; j++)
				{
					int target;
					Vector2 center;
					if (j < targets.Count)
					{
						target = targets[j];
						center = Main.player[target].Center;
					}
					else
					{
						target = 255;
						center = npc.Center + new Vector2(Main.rand.Next(-arenaWidth / 2 + (int)radius, arenaWidth / 2 - (int)radius + 1), Main.rand.Next(-arenaWidth / 2 + (int)radius, arenaWidth / 2 - (int)radius + 1));
					}
					float angle = (float)(Main.rand.NextDouble() * 2 * Math.PI / numSpheres);
					for (int k = 0; k < numSpheres; k++)
					{
						Vector2 pos = center + radius * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
						angle += 2f * (float)Math.PI / numSpheres;
						int proj = Projectile.NewProjectile(pos.X, pos.Y, 0f, 0f, mod.ProjectileType("PuritySphere"), damage, 0f, Main.myPlayer, center.X, center.Y);
						Main.projectile[proj].localAI[0] = target;
						Main.projectile[proj].localAI[1] = rotationSpeed;
						((PuritySphere)Main.projectile[proj].modProjectile).maxTimer = (int)time;
						Main.projectile[proj].netUpdate = true;
					}
				}
				attackProgress = 60 + (int)time + PuritySphere.strikeTime;
			}
			attackProgress--;
			if (attackProgress < 0)
			{
				attackProgress = 0;
			}
		}

		private void DoShield(int numShields)
		{
			int count = 0;
			for (int k = 0; k < 200; k++)
			{
				if (Main.npc[k].active && Main.npc[k].type == mod.NPCType("PurityShield") && Main.npc[k].ai[0] == npc.whoAmI)
				{
					count++;
				}
			}
			if (count >= numShields)
			{
				shieldTimer = 0;
				return;
			}
			float timeMult = timeMultiplier * 5f;
			shieldTimer++;
			if (shieldTimer >= 300 + 300 * timeMult)
			{
				float targetX = npc.Center.X + (Main.rand.Next(2) * 2 - 1) * arenaWidth / 4;
				float targetY = npc.Center.Y + (Main.rand.Next(2) * 2 - 1) * arenaHeight / 4;
				NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y + 40, mod.NPCType("PurityShield"), 0, npc.whoAmI, targetX, targetY);
				shieldTimer = 0;
			}
		}

		public override bool CheckDead()
		{
			if (stage < 10)
			{
				npc.active = true;
				npc.life = 1;
				npc.dontTakeDamage = true;
				stage = 10;
				attackProgress = 0;
				return false;
			}
			return true;
		}

		public void FinishFight1()
		{
			attackProgress++;
			if (attackProgress == 60)
			{
				Talk("This is... I thank you for demonstrating your power.");
			}
			if (attackProgress >= 240)
			{
				Talk("Please take this as a farewell gift.");
				stage++;
				attackProgress = 0;
			}
		}

		public void FinishFight2()
		{
			attackProgress++;
			if (attackProgress == 120)
			{
				Talk("I wish you luck in your future endeavors.");
			}
			if (attackProgress >= 180)
			{
				npc.dontTakeDamage = false;
				npc.StrikeNPCNoInteraction(9999, 0f, 0);
			}
		}

		public override void NPCLoot()
		{
			
		}

		public override void BossLoot(ref string name, ref int potionType)
		{
			name = "The " + npc.displayName;
			potionType = ItemID.SuperHealingPotion;
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
			for (int k = 0; k < 200; k++)
			{
				if (Main.npc[k].active && Main.npc[k].type == mod.NPCType("PurityShield") && Main.npc[k].ai[0] == npc.whoAmI)
				{
					return false;
				}
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
			if (damageTotal >= dpsCap * 60)
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
