using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalNPCs
{
	internal class DamageModificationGlobalNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;
		public bool exampleDefenseDebuff;

		public override void ResetEffects(NPC npc) {
			exampleDefenseDebuff = false;
		}

		public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers) {
			if (exampleDefenseDebuff) {
				// By convention, defense debuffs should be multiplicative 
				modifiers.Defense *= .75f;
			}
		}

		public override void DrawEffects(NPC npc, ref Color drawColor) {
			// This simple color effect indicates that the buff is active
			if (exampleDefenseDebuff) {
				drawColor.G = 0;
			}
		}
	}
}
