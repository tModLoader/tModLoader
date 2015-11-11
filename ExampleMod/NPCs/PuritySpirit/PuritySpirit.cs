using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.NPCs.PuritySpirit
{
	public class PuritySpirit : ModNPC
	{
		private const int size = 120;
		private const int particleSize = 12;
		public static readonly int arenaWidth = 2 * NPC.sWidth;
		public static readonly int arenaHeight = 2 * NPC.sHeight;

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

		private float debuffTimer
		{
			get
			{
				return npc.ai[2];
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

		private IList<Particle> particles = new List<Particle>();
		private float[,] aura = new float[size, size];
		private const int dpsCap = 4000;
		private int damageTotal = 0;
		private bool saidRushMessage = false;
		private readonly IList<int> targets = new List<int>();
		public int[] attackWeights = new int[]{ 2000, 2000, 2000, 2000, 3000 };

		public override void AI()
		{
			if (!Main.dedServ)
			{
				UpdateParticles();
			}
			npc.dontTakeDamage = stage == 0;
			FindPlayers();
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
					DoAttack();
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
			stage++;
		}

		private void SetupCrystals(int radius)
		{
		}

		private void UltimateAttack()
		{
		}

		private void DoAttack()
		{
		}

		private void BeamAttack()
		{
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
					Main.NewText("<Spirit of Purity> Oh, in a rush now, are we?", 150, 250, 150);
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
