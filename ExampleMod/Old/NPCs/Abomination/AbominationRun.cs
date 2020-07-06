using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.NPCs.Abomination
{
	//ported from my tAPI mod because I'm lazy
	[AutoloadBossHead]
	public class AbominationRun : ModNPC
	{
		public override string Texture => "ExampleMod/NPCs/Abomination/Abomination";

		public override string HeadTexture => "ExampleMod/NPCs/Abomination/Abomination_Head_Boss";

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("The Abomination");
			Main.npcFrameCount[npc.type] = 2;
		}

		public override void SetDefaults() {
			npc.aiStyle = -1;
			npc.lifeMax = 40000;
			npc.damage = 100;
			npc.defense = 55;
			npc.knockBackResist = 0f;
			npc.dontTakeDamage = true;
			npc.width = 100;
			npc.height = 100;
			npc.value = Item.buyPrice(0, 20, 0, 0);
			npc.npcSlots = 15f;
			npc.boss = true;
			npc.lavaImmune = true;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.HitSound = SoundID.NPCHit1;
			npc.DeathSound = SoundID.NPCDeath1;
			npc.buffImmune[24] = true;
			music = MusicID.Boss2;
			// Custom Music: music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/DriveMusic");
		}

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale) {
			npc.lifeMax = (int)(npc.lifeMax * 0.625f * bossLifeScale);
			npc.damage = (int)(npc.damage * 0.6f);
		}

		public override void AI() {
			if (npc.localAI[0] == 0f) {
				SoundEngine.PlaySound(SoundID.Roar, npc.position, 0);
				npc.localAI[0] = 1f;
			}
			npc.velocity.Y += 1f;
			if (npc.timeLeft > 10) {
				npc.timeLeft = 10;
			}
		}

		public override void OnHitPlayer(Player player, int damage, bool crit) {
			if (Main.expertMode || Main.rand.NextBool()) {
				player.AddBuff(BuffID.OnFire, 600, true);
			}
		}
	}
}
