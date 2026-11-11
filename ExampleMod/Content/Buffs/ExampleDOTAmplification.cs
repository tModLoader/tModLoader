using ExampleMod.Common.GlobalNPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Buffs
{
	public class ExampleDOTAmplification : ModBuff
	{
		public override string Texture => "Terraria/Images/Buff_54";

		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
		}

		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<DamageOverTimeGlobalNPC>().exampleAmplification = true;
		}
	}
}
