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
		//This is a neat trick that uses the fact that NPCs have all NPC.ai[] values set to 0f on spawn (if not otherwise changed).
		//We set ParentIndex to a number in the body after spawning it. If we set ParentIndex to 3, NPC.ai[0] will be 4. If NPC.ai[0] is 0, ParentIndex will be -1.
		//Now combine both facts, and the conclusion is that if this NPC spawns by other means (not from the body), ParentIndex will be -1, allowing us to distinguish
		//between a proper spawn and an invalid/"cheated" spawn
		public int ParentIndex {
			get => (int)NPC.ai[0] - 1;
			set => NPC.ai[0] = value + 1;
		}

		public bool HasParent => ParentIndex > -1;

		public int PositionIndex {
			get => (int)NPC.ai[1] - 1;
			set => NPC.ai[1] = value + 1;
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
			NPC.width = 30;
			NPC.height = 30;
			NPC.damage = 7;
			NPC.defense = 0;
			NPC.lifeMax = 50;
			NPC.HitSound = SoundID.NPCHit9;
			NPC.DeathSound = SoundID.NPCDeath11;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.knockBackResist = 0.8f;
			NPC.alpha = 255; //This makes it transparent upon spawning, we have to manually fade it in in AI()
			NPC.netAlways = true;

			NPC.aiStyle = -1;
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
			if (NPC.IsABestiaryIconDummy) {
				//This is required because we have NPC.alpha = 255, in the bestiary it would look transparent
				return NPC.GetBestiaryEntryColor();
			}
			return Color.White * (1f - NPC.alpha / 255f);
		}

		public override void HitEffect(int hitDirection, double damage) {
			if (NPC.life <= 0) {
				//If this NPC dies, spawn some visuals

				int dustType = 59; //Some blue dust, read the dust guide on the wiki for how to find the perfect dust
				for (int i = 0; i < 20; i++) {
					Vector2 velocity = NPC.velocity + new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f));
					Dust dust = Dust.NewDustPerfect(NPC.Center, dustType, velocity, 26, Color.White, Main.rand.NextFloat(1.5f, 2.4f));
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
				NPC.active = false;
				NPC.life = 0;
				NetMessage.SendData(MessageID.SyncNPC, number: NPC.whoAmI);
				return true;
			}
			return false;
		}

		private void FadeIn() {
			//Fade in (we have NPC.alpha = 255 in SetDefaults which means it spawns transparent)
			if (NPC.alpha > 0) {
				NPC.alpha -= 10;
				if (NPC.alpha < 0) {
					NPC.alpha = 0;
				}
			}
		}

		private void MoveInFormation() {
			NPC parentNPC = Main.npc[ParentIndex];

			//This basically turns the NPCs PositionIndex into a number between 0f and TwoPi to determine where around
			//the main body it is positioned at
			float rad = (float)PositionIndex / MinionBossBody.MinionCount() * MathHelper.TwoPi;

			float distanceFromBody = parentNPC.width + NPC.width;

			//offset is now a vector that will determine the position of the NPC based on its index
			Vector2 offset = Vector2.One.RotatedBy(rad) * distanceFromBody;

			Vector2 destination = parentNPC.Center + offset;
			Vector2 toDestination = destination - NPC.Center;
			Vector2 toDestinationNormalized = Vector2.Normalize(toDestination);

			float speed = 8f;
			float inertia = 20;

			Vector2 moveTo = toDestinationNormalized * speed;
			NPC.velocity = (NPC.velocity * (inertia - 1) + moveTo) / inertia;
		}
	}
}
