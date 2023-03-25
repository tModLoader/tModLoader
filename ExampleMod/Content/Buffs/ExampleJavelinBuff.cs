using ExampleMod.Common.GlobalNPCs;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Buffs
{
	public class ExampleJavelinDebuff : ModBuff
	{
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<DamageOverTimeGlobalNPC>().exampleJavelinDebuff = true;
		}
	}
}
