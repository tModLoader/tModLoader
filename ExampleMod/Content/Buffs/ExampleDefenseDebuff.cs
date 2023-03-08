using ExampleMod.Common.GlobalNPCs;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Buffs
{
	/// <summary>
	/// This debuff reduces enemy armor by 25%. Use <see cref="Content.Items.Weapons.HitModifiersShowcase"/> to apply.
	/// By using a buff rather than a GlobalNPC, we can reduce performance overhead, and rely on vanilla to automatically sync the effect via AddNPCBuff/NPCBuffs
	/// </summary>
	public class ExampleDefenseDebuff : ModBuff
	{
		public const int DefenseReductionPercent = 25;
		public static float DefenseMultiplier = 1 - DefenseReductionPercent / 100f;

		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<DamageModificationGlobalNPC>().exampleDefenseDebuff = true; 
		}
	}
}
