using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.NPCs.MinionBoss
{
	//The minions spawned when the body spawns
	//Please read MinionBossBody.cs first for important comments, they won't be explained here again
	public class MinionBossMinion : ModNPC
	{
		public int ParentIndex {
			get => (int)npc.ai[0] - 1;
			set => npc.ai[0] = value + 1;
		}

		public bool HasParent => ParentIndex > -1;

		public int PositionIndex {
			get => (int)npc.ai[1] - 1;
			set => npc.ai[1] = value + 1;
		}

		public bool HasPosition => PositionIndex > -1;

		//Helper method to determine the body type
		public static int BodyType() {
			return ModContent.NPCType<MinionBossBody>();
		}

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Minion Boss Minion");
			Main.npcFrameCount[Type] = 1;

			//By default enemies gain health and attack if hardmode is reached. this NPC should not be affected by that
			NPCID.Sets.DontDoHardmodeScaling[Type] = true;
			//Enemies can pick up coins, let's prevent it for this NPC
			NPCID.Sets.CantTakeLunchMoney[Type] = true;
			//Automatically group with other bosses
			NPCID.Sets.BossBestiaryPriority.Add(Type);

			//Specify the debuffs it is immune to
			NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData {
				SpecificallyImmuneTo = new int[] {
					BuffID.Confused
				}
			};
			NPCID.Sets.DebuffImmunitySets.Add(Type, debuffData);
		}

		public override void SetDefaults() {
			npc.width = 30;
			npc.height = 30;
			npc.damage = 7;
			npc.defense = 0;
			npc.lifeMax = 50;
			npc.HitSound = SoundID.NPCHit9;
			npc.DeathSound = SoundID.NPCDeath11;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.knockBackResist = 0.8f;
			npc.alpha = 255; //This makes it transparent upon spawning, we have to manually fade it in in AI()
			npc.netAlways = true;

			npc.aiStyle = -1;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			//Makes it so whenever you beat the boss associated with it, it will also get unlocked immediately
			bestiaryEntry.UIInfoProvider = new CommonEnemyUICollectionInfoProvider(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[BodyType()], quickUnlock: true);

			bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement> {
				new MoonLordPortraitBackgroundProviderBestiaryInfoElement(), //Plain black background
				new FlavorTextBestiaryInfoElement("A minion protecting his boss from taking damage by sacrificing itself. If none are alive, the boss is exposed to damage.")
			});
		}

		public override Color? GetAlpha(Color drawColor) {
			if (npc.IsABestiaryIconDummy) {
				//This is required because we have npc.alpha = 255, in the bestiary it would look transparent
				return npc.GetBestiaryEntryColor();
			}
			return Color.White * (1f - npc.alpha / 255f);
		}

		public override void HitEffect(int hitDirection, double damage) {
			if (npc.life <= 0) {
				//If this NPC dies, spawn some visuals

				int dustType = 59; //Some blue dust
				for (int i = 0; i < 20; i++) {
					Dust dust = Dust.NewDustPerfect(npc.Center, dustType, new Vector2(Main.rand.NextFloat(-1.5f, 1.5f) + npc.velocity.X, Main.rand.NextFloat(-1.5f, 1f)), 26, Color.White, Main.rand.NextFloat(1.5f, 2.4f));
					dust.noLight = true;
					dust.noGravity = true;
					dust.fadeIn = Main.rand.NextFloat(0.3f, 0.8f);
				}
			}
		}

		public override void AI() {
			if (Despawn()) {
				return;
			}

			FadeIn();

			MoveInFormation();
		}

		private bool Despawn() {
			if (Main.netMode != NetmodeID.MultiplayerClient &&
				(!HasPosition || !HasParent || !Main.npc[ParentIndex].active || Main.npc[ParentIndex].type != BodyType())) {
				//* Not spawned by the boss body (didn't assign a position and parent) or
				//* Parent isn't active or
				//* Parent isn't the body
				//=> invalid, kill itself without dropping any items
				npc.active = false;
				npc.life = 0;
				NetMessage.SendData(MessageID.SyncNPC, number: npc.whoAmI);
				return true;
			}
			return false;
		}

		private void FadeIn() {
			//Fade in (we have npc.alpha = 255 in SetDefaults which means it spawns transparent)
			if (npc.alpha > 0) {
				npc.alpha -= 10;
				if (npc.alpha < 0) {
					npc.alpha = 0;
				}
			}
		}

		private void MoveInFormation() {
			NPC parentNPC = Main.npc[ParentIndex];

			//This basically turns the NPCs position in the Main.npc[] array into a number between 0f and TwoPi to determine where around
			//the main body it is positioned at
			float rad = (float)PositionIndex / MinionBossBody.MinionCount() * MathHelper.TwoPi;

			float distanceFromBody = parentNPC.width + npc.width * 2;

			//offset is now a vector that will determine the position of the NPC based on its index
			Vector2 offset = Vector2.One.RotatedBy(rad) * distanceFromBody;

			Vector2 destination = parentNPC.Center + offset;
			Vector2 toDestination = destination - npc.Center;
			Vector2 toDestinationNormalized = Vector2.Normalize(toDestination);

			float speed = 8f;
			float inertia = 20;

			Vector2 moveTo = toDestinationNormalized * speed;
			npc.velocity = (npc.velocity * (inertia - 1) + moveTo) / inertia;
		}
	}
}
