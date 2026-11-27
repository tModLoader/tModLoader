using ExampleMod.Content.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.NPCs
{
	public class ExampleRegeneratingSlime : ModNPC
	{
		public override string Texture => "Terraria/Images/NPC_1";

		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.BlueSlime];
		}

		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.BlueSlime);
			AIType = NPCID.BlueSlime;
			AnimationType = NPCID.BlueSlime;
			NPC.lifeMax = 300;
			NPC.lavaImmune = true;
		}

		public override void UpdateDOTTally(ref NPC.DOTTally tally) {
			// Ordinary regeneration is suppressed by damage-over-time debuffs.
			NPC.lifeRegen += 12;
			if (NPC.lavaWet) {
				// Supernatural regeneration still offsets damage while a debuff is active.
				tally.AddSuperNaturalRegeneration(24);
			}
		}

		public override void ModifyDOT(int buffType, ref NPC.DOTTally contribution) {
			if (buffType == ModContent.BuffType<ExamplePoisonDebuff>()) {
				contribution.LifeRegenLoss /= 2;
				contribution.LifeRegenLossThatCanBeBoosted /= 2;
				contribution.ExpectedDamagePerSecond /= 2;
			}
		}
	}
}
