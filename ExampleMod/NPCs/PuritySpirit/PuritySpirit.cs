using ExampleMod.Dusts;
using ExampleMod.Items;
using ExampleMod.Items.Armor;
using ExampleMod.Items.Placeable;
using ExampleMod.Projectiles.PuritySpirit;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.NPCs.PuritySpirit
{
	[AutoloadBossHead]
	public class PuritySpirit : ModNPC
	{
		private const int size = 120;
		private const int particleSize = 12;
		public static readonly int arenaWidth = (int)(1.3f * NPC.sWidth);
		public static readonly int arenaHeight = (int)(1.3f * NPC.sHeight);

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Spirit of Purity");
			NPCID.Sets.MustAlwaysDraw[npc.type] = true;
		}

		public override void SetDefaults() {
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
			npc.HitSound = SoundID.NPCHit1;
			npc.DeathSound = null;
			npc.alpha = 255;
			for (int k = 0; k < npc.buffImmune.Length; k++) {
				npc.buffImmune[k] = true;
			}
			music = MusicID.Title;
			musicPriority = MusicPriority.BossMedium; // By default, musicPriority is BossLow
			bossBag = ItemType<PuritySpiritBag>();
		}

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale) {
			npc.lifeMax = (int)(npc.lifeMax / Main.expertLife * 1.2f * bossLifeScale);
			npc.defense = 72;
		}

		private int difficulty {
			get {
				double strength = (double)npc.life / (double)npc.lifeMax;
				int difficulty = (int)(4.0 * (1.0 - strength));
				if (Main.expertMode) {
					difficulty++;
				}
				return difficulty;
			}
		}

		private float difficultyGradient {
			get {
				double strength = (double)npc.life / (double)npc.lifeMax;
				double difficulty = 4.0 * (1.0 - strength);
				return (float)(difficulty % 1.0);
			}
		}

		private float timeMultiplier => 1f - (difficulty + difficultyGradient) * 0.2f;

		private int stage {
			get => (int)npc.ai[0];
			set => npc.ai[0] = value;
		}

		private float attackTimer {
			get => npc.ai[1];
			set => npc.ai[1] = value;
		}

		internal int attack {
			get => (int)npc.ai[2];
			private set => npc.ai[2] = value;
		}

		internal int attackProgress {
			get => (int)npc.ai[3];
			private set => npc.ai[3] = value;
		}

		private int portalFrame {
			get => (int)npc.localAI[0];
			set => npc.localAI[0] = value;
		}

		private int shieldTimer {
			get => (int)npc.localAI[1];
			set => npc.localAI[1] = value;
		}

		private IList<Particle> particles = new List<Particle>();
		private readonly float[,] aura = new float[size, size];
		internal const int dpsCap = 5000;
		private int damageTotal;
		private bool saidRushMessage;
		public readonly IList<int> targets = new List<int>();
		public int[] attackWeights = new[] { 2000, 2000, 2000, 2000, 3000 };
		public const int minAttackWeight = 1000;
		public const int maxAttackWeight = 4000;

		public override void AI() {
			if (!Main.dedServ) {
				UpdateParticles();
				portalFrame++;
				portalFrame %= 6 * Main.projFrames[ProjectileID.PortalGunGate];
			}
			FindPlayers();
			npc.timeLeft = NPC.activeTime;
			if (stage > 0 && targets.Count == 0) {
				attackProgress = 0;
				stage = -1;
			}
			damageTotal -= dpsCap;
			if (damageTotal < 0) {
				damageTotal = 0;
			}
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				return;
			}
			if (stage == 2 && difficulty > 0) {
				Projectile.NewProjectile(npc.Center.X - arenaWidth / 2, npc.Center.Y, NegativeWall.speed, 0f, ProjectileType<NegativeWall>(), 0, 0f, Main.myPlayer, npc.whoAmI, arenaHeight);
				stage++;
			}
			if (stage == 3 && difficulty > 1) {
				SetupCrystals(arenaWidth / 3, false);
				stage++;
			}
			if (stage == 4 && difficulty > 2) {
				Projectile.NewProjectile(npc.Center.X, npc.Center.Y - arenaHeight / 2, 0f, NegativeWall.speed, ProjectileType<NegativeWall>(), 0, 0f, Main.myPlayer, npc.whoAmI, -arenaWidth);
				stage++;
			}
			if (stage == 5 && difficulty > 3) {
				stage++;
			}
			switch (stage) {
				case -1:
					RunAway();
					break;
				case 0:
					Initialize();
					break;
				case 1:
				case 12:
					attack = 4;
					UltimateAttack();
					if (attackProgress == 0) {
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
					if (attackProgress == 0) {
						stage++;
					}
					else {
						DoAttack(5);
					}
					break;
				case 11:
					FinishFight1();
					break;
				case 13:
					FinishFight2();
					break;
			}
		}

		private void UpdateParticles() {
			foreach (Particle particle in particles) {
				particle.Update();
			}
			Vector2 newPos = new Vector2(Main.rand.Next(3 * size / 8, 5 * size / 8), Main.rand.Next(3 * size / 8, 5 * size / 8));
			double newAngle = 2 * Math.PI * Main.rand.NextDouble();
			Vector2 newVel = new Vector2((float)Math.Cos(newAngle), (float)Math.Sin(newAngle));
			newVel *= 0.5f * (1f + (float)Main.rand.NextDouble());
			particles.Add(new Particle(newPos, newVel));
			if (particles[0].strength <= 0f) {
				particles.RemoveAt(0);
			}
			for (int x = 0; x < size; x++) {
				for (int y = 0; y < size; y++) {
					aura[x, y] *= 0.97f;
				}
			}
			foreach (Particle particle in particles) {
				int minX = (int)particle.position.X - particleSize / 2;
				int minY = (int)particle.position.Y - particleSize / 2;
				int maxX = minX + particleSize;
				int maxY = minY + particleSize;
				for (int x = minX; x <= maxX; x++) {
					for (int y = minY; y <= maxY; y++) {
						if (x >= 0 && x < size && y >= 0 && y < size) {
							float strength = particle.strength;
							float offX = particle.position.X - x;
							float offY = particle.position.Y - y;
							strength *= 1f - (float)Math.Sqrt(offX * offX + offY * offY) / particleSize * 2;
							if (strength < 0f) {
								strength = 0f;
							}
							aura[x, y] = 1f - (1f - aura[x, y]) * (1f - strength);
						}
					}
				}
			}
		}

		public void FindPlayers() {
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				int originalCount = targets.Count;
				targets.Clear();
				for (int k = 0; k < 255; k++) {
					if (Main.player[k].active && Main.player[k].GetModPlayer<ExamplePlayer>().heroLives > 0) {
						targets.Add(k);
					}
				}
				if (Main.netMode == NetmodeID.Server && targets.Count != originalCount) {
					ModPacket netMessage = GetPacket(PuritySpiritMessageType.TargetList);
					netMessage.Write(targets.Count);
					foreach (int target in targets) {
						netMessage.Write(target);
					}
					netMessage.Send();
				}
			}
		}

		public void RunAway() {
			attackProgress++;
			if (attackProgress == 180) {
				Talk("Hmph. Was that the extent of your power?");
			}
			if (attackProgress >= 360) {
				npc.active = false;
			}
		}

		public void Initialize() {
			if (attackProgress == 1) {
				Vector2 center = npc.Center;
				for (int k = 0; k < 255; k++) {
					Player player = Main.player[k];
					if (player.active && player.position.X > center.X - arenaWidth / 2 && player.position.X + player.width < center.X + arenaWidth / 2 && player.position.Y > center.Y - arenaHeight / 2 && player.position.Y + player.height < center.Y + arenaHeight / 2) {
						player.GetModPlayer<ExamplePlayer>().heroLives = 3;
						if (Main.netMode == NetmodeID.Server) {
							ModPacket netMessage = GetPacket(PuritySpiritMessageType.HeroPlayer);
							netMessage.Send(k);
						}
					}
				}
				shieldTimer = 1000;
			}
			attackProgress++;
			if (attackProgress == 90) {
				if (ExampleWorld.downedPuritySpirit) {
					Talk("What, you again? Oh well...");
				}
				else {
					Talk("You, who have challenged me...");
				}
			}
			if (attackProgress == 180) {
				SetupCrystals(arenaWidth / 6, true);
			}
			if (attackProgress >= 420) {
				Talk("Show me the power that has saved Terraria!");
				attackProgress = 0;
				stage++;
				npc.dontTakeDamage = false;
				if (Main.netMode == NetmodeID.Server) {
					ModPacket netMessage = GetPacket(PuritySpiritMessageType.DontTakeDamage);
					netMessage.Write(false);
					netMessage.Send();
				}
			}
		}

		private void SetupCrystals(int radius, bool clockwise) {
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				return;
			}
			Vector2 center = npc.Center;
			for (int k = 0; k < 10; k++) {
				float angle = 2f * (float)Math.PI / 10f * k;
				Vector2 pos = center + radius * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
				int damage = 80;
				if (Main.expertMode) {
					damage = (int)(100 / Main.expertDamage);
				}
				int proj = Projectile.NewProjectile(pos.X, pos.Y, 0f, 0f, ProjectileType<PureCrystal>(), damage, 0f, Main.myPlayer, npc.whoAmI, angle);
				Main.projectile[proj].localAI[0] = radius;
				Main.projectile[proj].localAI[1] = clockwise ? 1 : -1;
				NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj);
			}
		}

		private void UltimateAttack() {
			if (attackProgress == 0) {
				PlaySound(15, 0);
				if (Main.netMode != NetmodeID.MultiplayerClient) {
					int damage = Main.expertMode ? 720 : 600;
					Projectile.NewProjectile(npc.Center.X, npc.Center.Y, 0f, 0f, ProjectileType<VoidWorld>(), damage, 0f, Main.myPlayer, npc.whoAmI, Main.rand.Next());
				}
			}
			attackProgress++;
			if (attackProgress >= 500) {
				attackProgress = 0;
			}
		}

		private void DoAttack(int numAttacks) {
			if (attackTimer > 0f) {
				attackTimer -= 1f;
				return;
			}
			if (attack < 0) {
				int totalWeight = 0;
				for (int k = 0; k < numAttacks; k++) {
					if (attackWeights[k] < minAttackWeight) {
						attackWeights[k] = minAttackWeight;
					}
					totalWeight += attackWeights[k];
				}
				int choice = Main.rand.Next(totalWeight);
				for (attack = 0; attack < numAttacks; attack++) {
					if (choice < attackWeights[attack]) {
						break;
					}
					choice -= attackWeights[attack];
				}
				attackWeights[attack] -= 80;
				npc.netUpdate = true;
			}
			switch (attack) {
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
			if (attackProgress == 0) {
				attackTimer += 160f * timeMultiplier;
				attack = -1;
			}
		}

		private void BeamAttack() {
			if (attackProgress == 0) {
				float y = npc.Center.Y;
				int damage = Main.expertMode ? 360 : 300;
				for (int k = 0; k < targets.Count; k++) {
					float x = Main.player[targets[k]].Center.X;
					Projectile.NewProjectile(x, y, 0f, 0f, ProjectileType<PurityBeam>(), damage, 0f, Main.myPlayer, arenaHeight);
					for (int j = -1; j <= 1; j += 2) {
						float spawnX = x + j * Main.rand.Next(200, 401);
						if (spawnX > npc.Center.X + arenaWidth / 2) {
							spawnX -= arenaWidth;
						}
						else if (spawnX < npc.Center.X - arenaWidth / 2) {
							spawnX += arenaWidth;
						}
						Projectile.NewProjectile(spawnX, y, 0f, 0f, ProjectileType<PurityBeam>(), damage, 0f, Main.myPlayer, arenaHeight);
					}
				}
				int numExtra = 2 * (difficulty + 1) - 2 * (targets.Count - 1);
				if (difficulty >= 2) {
					numExtra--;
				}
				if (difficulty >= 4) {
					numExtra--;
				}
				for (int k = 0; k < numExtra; k++) {
					Projectile.NewProjectile(npc.Center.X + Main.rand.Next(-arenaWidth / 2 + 50, arenaWidth / 2 - 50 + 1), y, 0f, 0f, ProjectileType<PurityBeam>(), damage, 0f, Main.myPlayer, arenaHeight);
				}
				attackProgress = (int)(PurityBeam.charge + 60f);
			}
			attackProgress--;
			if (attackProgress < 0) {
				attackProgress = 0;
			}
		}

		private void SnakeAttack() {
			if (attackProgress == 0) {
				int damage = Main.expertMode ? 60 : 80;
				Projectile.NewProjectile(npc.Center.X, npc.Center.Y, 0f, 0f, ProjectileType<PuritySnake>(), damage, 0f, Main.myPlayer, npc.whoAmI, timeMultiplier);
				attackProgress = 240;
			}
			attackProgress--;
			if (attackProgress < 0) {
				attackProgress = 0;
			}
		}

		private void LaserAttack() {
			if (attackProgress == 0) {
				int numAttacks = 3 + difficulty / 2;
				float timer = 30f + 20f * timeMultiplier;
				float totalTime = numAttacks * timer + 120f;
				int damage = Main.expertMode ? 55 : 80;
				for (int k = 0; k < numAttacks; k++) {
					int proj = Projectile.NewProjectile(npc.Center.X, npc.Center.Y, 0f, 0f, ProjectileType<NullLaser>(), damage, 0f, Main.myPlayer, npc.whoAmI, (int)(60f + k * timer));
					Main.projectile[proj].localAI[0] = (int)totalTime;
					((NullLaser)Main.projectile[proj].modProjectile).warningTime = timer;
					NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj);
				}
				attackProgress = (int)totalTime;
			}
			if (attackProgress % 20 == 0) {
				PlaySound(2, 15);
			}
			Dust.NewDust(npc.position, npc.width, npc.height, DustType<Sparkle>(), 0f, 0f, 0, new Color(0, 180, 0), 1.5f);
			attackProgress--;
			if (attackProgress < 0) {
				attackProgress = 0;
			}
		}

		private void SphereAttack() {
			if (attackProgress == 0) {
				int damage = Main.expertMode ? 60 : 80;
				float time = 60f + 60f * timeMultiplier;
				int rotationSpeed = Main.rand.Next(2) * 2 - 1;
				int numSpheres = 3 + difficulty / 2;
				int numGroups = 4 + difficulty / 3;
				float radius = PuritySphere.radius;
				for (int j = 0; j < numGroups || j < targets.Count; j++) {
					int target;
					Vector2 center;
					if (j < targets.Count) {
						target = targets[j];
						center = Main.player[target].Center;
					}
					else {
						target = 255;
						center = npc.Center + new Vector2(Main.rand.Next(-arenaWidth / 2 + (int)radius, arenaWidth / 2 - (int)radius + 1), Main.rand.Next(-arenaWidth / 2 + (int)radius, arenaWidth / 2 - (int)radius + 1));
					}
					float angle = (float)(Main.rand.NextDouble() * 2 * Math.PI / numSpheres);
					for (int k = 0; k < numSpheres; k++) {
						Vector2 pos = center + radius * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
						angle += 2f * (float)Math.PI / numSpheres;
						int proj = Projectile.NewProjectile(pos.X, pos.Y, 0f, 0f, ProjectileType<PuritySphere>(), damage, 0f, Main.myPlayer, center.X, center.Y);
						Main.projectile[proj].localAI[0] = target;
						Main.projectile[proj].localAI[1] = rotationSpeed;
						((PuritySphere)Main.projectile[proj].modProjectile).maxTimer = (int)time;
						NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj);
					}
				}
				attackProgress = 60 + (int)time + PuritySphere.strikeTime;
			}
			attackProgress--;
			if (attackProgress < 0) {
				attackProgress = 0;
			}
		}

		private void DoShield(int numShields) {
			int count = 0;
			for (int k = 0; k < 200; k++) {
				if (Main.npc[k].active && Main.npc[k].type == NPCType<PurityShield>() && Main.npc[k].ai[0] == npc.whoAmI) {
					count++;
				}
			}
			if (count >= numShields) {
				shieldTimer = 0;
				return;
			}
			float timeMult = timeMultiplier * 5f;
			shieldTimer++;
			if (shieldTimer >= 300 + 300 * timeMult) {
				float targetX = npc.Center.X + (Main.rand.Next(2) * 2 - 1) * arenaWidth / 4;
				float targetY = npc.Center.Y + (Main.rand.Next(2) * 2 - 1) * arenaHeight / 4;
				NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y + 40, NPCType<PurityShield>(), 0, npc.whoAmI, targetX, targetY);
				shieldTimer = 0;
			}
		}

		public override bool CheckDead() {
			if (stage < 10) {
				npc.active = true;
				npc.life = 1;
				npc.dontTakeDamage = true;
				if (Main.netMode == NetmodeID.Server) {
					ModPacket netMessage = GetPacket(PuritySpiritMessageType.DontTakeDamage);
					netMessage.Write(true);
					netMessage.Send();
				}
				if (Main.netMode != NetmodeID.MultiplayerClient) {
					stage = 10;
				}
				return false;
			}
			return true;
		}

		public void FinishFight1() {
			attackProgress++;
			if (attackProgress == 60) {
				Talk("This is... I thank you for demonstrating your power.");
			}
			if (attackProgress >= 240) {
				Talk("Please take this as a farewell gift.");
				stage++;
				attackProgress = 0;
			}
		}

		public void FinishFight2() {
			attackProgress++;
			if (attackProgress == 120) {
				if (ExampleWorld.downedPuritySpirit) {
					Talk("I wish you luck in your future farming.");
				}
				else {
					Talk("I wish you luck in your future endeavors.");
				}
			}
			if (attackProgress >= 180) {
				npc.dontTakeDamage = false;
				npc.HitSound = null;
				npc.StrikeNPCNoInteraction(9999, 0f, 0);
			}
		}

		public override void NPCLoot() {
			/* // Consider using this alternate approach to choosing drops.
			int trophyChoice = new WeightedRandom<int>(
				Tuple.Create(ItemType<Items.Placeable.PuritySpiritTrophy>(), 1.0),
				Tuple.Create(ItemType<Items.Placeable.BunnyTrophy>(), 1.0),
				Tuple.Create(ItemType<Items.Placeable.TreeTrophy>(), 1.0),
				Tuple.Create(0, 7.0)
			);
			if (trophyChoice > 0)
			{
				Item.NewItem(npc.getRect(), trophyChoice);
			}
			if (Main.expertMode)
			{
				npc.DropBossBags();
			}
			else
			{
				//This is an alternate syntax you can use
				//var maskChooser = new WeightedRandom<int>();
				//maskChooser.Add(ItemType<Items.Armor.PuritySpiritMask>());
				//maskChooser.Add(ItemType<Items.Armor.BunnyMask>());
				//maskChooser.Add(ItemID.Bunny, 5.0);
				//int maskChoice = maskChooser;

				int maskChoice = new WeightedRandom<int>(
					Tuple.Create(ItemType<Items.Armor.PuritySpiritMask>(), 1.0),
					Tuple.Create(ItemType<Items.Armor.BunnyMask>(), 1.0),
					Tuple.Create((int)ItemID.Bunny, 5.0)
				);
				Item.NewItem(npc.getRect(), maskChoice);
				Item.NewItem(npc.getRect(), ItemType<Items.PurityShield>());
			}
			ExampleWorld.downedPuritySpirit = true; */

			int choice = Main.rand.Next(10);
			int item = 0;
			switch (choice) {
				case 0:
					item = ItemType<PuritySpiritTrophy>();
					break;
				case 1:
					item = ItemType<BunnyTrophy>();
					break;
				case 2:
					item = ItemType<TreeTrophy>();
					break;
			}
			if (item > 0) {
				Item.NewItem(npc.getRect(), item);
			}
			if (Main.expertMode) {
				npc.DropBossBags();
			}
			else {
				choice = Main.rand.Next(7);
				if (choice == 0) {
					Item.NewItem(npc.getRect(), ItemType<PuritySpiritMask>());
				}
				else if (choice == 1) {
					Item.NewItem(npc.getRect(), ItemType<BunnyMask>());
				}
				if (choice != 1) {
					Item.NewItem(npc.getRect(), ItemID.Bunny);
				}
				Item.NewItem(npc.getRect(), ItemType<Items.PurityShield>());
			}
			if (!ExampleWorld.downedPuritySpirit) {
				ExampleWorld.downedPuritySpirit = true;
				if (Main.netMode == NetmodeID.Server) {
					NetMessage.SendData(MessageID.WorldData); // Immediately inform clients of new world state.
				}
			}
		}

		public override void BossLoot(ref string name, ref int potionType) {
			name = "The " + name;
			potionType = ItemID.SuperHealingPotion;
		}

		public override bool? CanBeHitByItem(Player player, Item item) {
			return CanBeHitByPlayer(player);
		}

		public override void ModifyHitByItem(Player player, Item item, ref int damage, ref float knockback, ref bool crit) {
			ModifyHit(ref damage);
		}

		public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit) {
			OnHit(damage);
		}

		public override bool? CanBeHitByProjectile(Projectile projectile) {
			return CanBeHitByPlayer(Main.player[projectile.owner]);
		}

		public override void ModifyHitByProjectile(Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			if (ProjectileID.Sets.StardustDragon[projectile.type]) {
				damage /= 2;
			}
			ModifyHit(ref damage);
		}

		public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit) {
			OnHit(damage);
		}

		private bool? CanBeHitByPlayer(Player player) {
			if (!targets.Contains(player.whoAmI)) {
				return false;
			}
			for (int k = 0; k < 200; k++) {
				if (Main.npc[k].active && Main.npc[k].type == NPCType<PurityShield>() && Main.npc[k].ai[0] == npc.whoAmI) {
					return false;
				}
			}
			return null;
		}

		private void ModifyHit(ref int damage) {
			if (damage > npc.lifeMax / 8) {
				damage = npc.lifeMax / 8;
			}
		}

		private void OnHit(int damage) {
			damageTotal += damage * 60;
			if (Main.netMode != NetmodeID.SinglePlayer) {
				ModPacket netMessage = GetPacket(PuritySpiritMessageType.Damage);
				netMessage.Write(damage * 60);
				if (Main.netMode == NetmodeID.MultiplayerClient) {
					netMessage.Write(Main.myPlayer);
				}
				netMessage.Send();
			}
		}

		public override bool StrikeNPC(ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit) {
			if (damageTotal >= dpsCap * 60) {
				if (!saidRushMessage && Main.netMode != NetmodeID.MultiplayerClient) {
					Talk("Oh, in a rush now, are we?");
					saidRushMessage = true;
				}
				damage = 0;
				return false;
			}
			return true;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor) {
			for (int x = 0; x < size; x++) {
				for (int y = 0; y < size; y++) {
					Vector2 drawPos = npc.position - Main.screenPosition;
					drawPos.X += x * 2 - size / 2;
					drawPos.Y += y * 2 - size / 2;
					spriteBatch.Draw(mod.GetTexture("NPCs/PuritySpirit/PurityParticle"), drawPos, null, Color.White * aura[x, y], 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
				}
			}
			spriteBatch.Draw(mod.GetTexture("NPCs/PuritySpirit/PurityEyes"), npc.position - Main.screenPosition, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
			return false;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color drawColor) {
			int portalWidth = 48;
			int portalDepth = 18;
			Color color = new Color(64, 255, 64);
			int centerX = (int)npc.Center.X;
			int centerY = (int)npc.Center.Y;
			Main.instance.LoadProjectile(ProjectileID.PortalGunGate);
			for (int x = centerX - arenaWidth / 2; x < centerX + arenaWidth / 2; x += portalWidth) {
				int frameNum = (portalFrame / 6 + x / portalWidth) % Main.projFrames[ProjectileID.PortalGunGate];
				Rectangle frame = new Rectangle(0, frameNum * (portalWidth + 2), portalDepth, portalWidth);
				Vector2 drawPos = new Vector2(x + portalWidth / 2, centerY - arenaHeight / 2) - Main.screenPosition;
				spriteBatch.Draw(Main.projectileTexture[ProjectileID.PortalGunGate], drawPos, frame, color, (float)-Math.PI / 2f, new Vector2(portalDepth / 2, portalWidth / 2), 1f, SpriteEffects.None, 0f);
				drawPos.Y += arenaHeight;
				spriteBatch.Draw(Main.projectileTexture[ProjectileID.PortalGunGate], drawPos, frame, color, (float)Math.PI / 2f, new Vector2(portalDepth / 2, portalWidth / 2), 1f, SpriteEffects.None, 0f);
			}
			for (int y = centerY - arenaHeight / 2; y < centerY + arenaHeight / 2; y += portalWidth) {
				int frameNum = (portalFrame / 6 + y / portalWidth) % Main.projFrames[ProjectileID.PortalGunGate];
				Rectangle frame = new Rectangle(0, frameNum * (portalWidth + 2), portalDepth, portalWidth);
				Vector2 drawPos = new Vector2(centerX - arenaWidth / 2, y + portalWidth / 2) - Main.screenPosition;
				spriteBatch.Draw(Main.projectileTexture[ProjectileID.PortalGunGate], drawPos, frame, color, (float)Math.PI, new Vector2(portalDepth / 2, portalWidth / 2), 1f, SpriteEffects.None, 0f);
				drawPos.X += arenaWidth;
				spriteBatch.Draw(Main.projectileTexture[ProjectileID.PortalGunGate], drawPos, frame, color, 0f, new Vector2(portalDepth / 2, portalWidth / 2), 1f, SpriteEffects.None, 0f);
			}
		}

		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) {
			scale = 1.5f;
			return null;
		}

		private void Talk(string message) {
			if (Main.netMode != NetmodeID.Server) {
				string text = Language.GetTextValue("Mods.ExampleMod.NPCTalk", Lang.GetNPCNameValue(npc.type), message);
				Main.NewText(text, 150, 250, 150);
			}
			else {
				NetworkText text = NetworkText.FromKey("Mods.ExampleMod.NPCTalk", Lang.GetNPCNameValue(npc.type), message);
				NetMessage.BroadcastChatMessage(text, new Color(150, 250, 150));
			}
		}

		private void PlaySound(int type, int style) {
			if (Main.netMode != NetmodeID.Server) {
				if (targets.Contains(Main.myPlayer)) {
					SoundEngine.PlaySound(type, -1, -1, style);
				}
				else {
					SoundEngine.PlaySound(type, (int)npc.position.X, (int)npc.position.Y, style);
				}
			}
			else {
				ModPacket netMessage = GetPacket(PuritySpiritMessageType.PlaySound);
				netMessage.Write(type);
				netMessage.Write(style);
				netMessage.Send();
			}
		}

		private ModPacket GetPacket(PuritySpiritMessageType type) {
			ModPacket packet = mod.GetPacket();
			packet.Write((byte)ExampleModMessageType.PuritySpirit);
			packet.Write(npc.whoAmI);
			packet.Write((byte)type);
			return packet;
		}

		public void HandlePacket(BinaryReader reader) {
			PuritySpiritMessageType type = (PuritySpiritMessageType)reader.ReadByte();
			if (type == PuritySpiritMessageType.HeroPlayer) {
				Player player = Main.LocalPlayer;
				player.GetModPlayer<ExamplePlayer>().heroLives = 3;
			}
			else if (type == PuritySpiritMessageType.TargetList) {
				int numTargets = reader.ReadInt32();
				targets.Clear();
				for (int k = 0; k < numTargets; k++) {
					targets.Add(reader.ReadInt32());
				}
			}
			else if (type == PuritySpiritMessageType.DontTakeDamage) {
				npc.dontTakeDamage = reader.ReadBoolean();
			}
			else if (type == PuritySpiritMessageType.PlaySound) {
				int soundType = reader.ReadInt32();
				int style = reader.ReadInt32();
				if (targets.Contains(Main.myPlayer)) {
					SoundEngine.PlaySound(soundType, -1, -1, style);
				}
				else {
					SoundEngine.PlaySound(soundType, (int)npc.position.X, (int)npc.position.Y, style);
				}
			}
			else if (type == PuritySpiritMessageType.Damage) {
				int damage = reader.ReadInt32();
				damageTotal += damage;
				if (Main.netMode == NetmodeID.Server) {
					ModPacket netMessage = GetPacket(PuritySpiritMessageType.Damage);
					int ignore = reader.ReadInt32();
					netMessage.Write(damage);
					netMessage.Send(-1, ignore);
				}
			}
		}
	}

	internal class Particle
	{
		internal Vector2 position;
		internal Vector2 velocity;
		internal float strength;

		internal Particle(Vector2 pos, Vector2 vel) {
			position = pos;
			velocity = vel;
			strength = 0.75f;
		}

		internal void Update() {
			position += velocity * strength;
			strength -= 0.01f;
		}
	}

	internal enum PuritySpiritMessageType : byte
	{
		HeroPlayer,
		TargetList,
		DontTakeDamage,
		PlaySound,
		Damage
	}
}
