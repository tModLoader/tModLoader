using ExampleMod.Common.GlobalNPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Buffs
{
	public class ExamplePoisonDebuff : ModBuff
	{
		public override string Texture => "Terraria/Images/Buff_20";

		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			DamageOverTimeGlobalNPC.AmplifiedDebuffs.Add(Type);
			BuffID.Sets.GrantImmunityWith[Type].Add(BuffID.Poisoned);
		}

		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<DamageOverTimeGlobalNPC>().examplePoison = true;
		}
	}
}
