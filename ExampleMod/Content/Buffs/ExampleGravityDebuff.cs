using System;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Buffs
{
	public class ExampleGravityDebuff : ModBuff
	{
		public int Counter;
		public override void Update(NPC npc, ref int buffIndex) {
			Counter++;
			npc.GravityMultiplier *= MathF.Cos(Counter / 100);
		}
	}
}