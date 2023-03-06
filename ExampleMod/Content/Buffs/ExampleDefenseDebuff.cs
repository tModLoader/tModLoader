using ExampleMod.Common.GlobalNPCs;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Buffs
{
	/// <summary>
	/// This debuff reduces enemy armor by 25%. Use <see cref="Content.Items.Weapons.HitModifiersShowcase"/> to apply.
	/// </summary>
	public class ExampleDefenseDebuff : ModBuff
	{
		public override string Texture => "ExampleMod/Content/Buffs/DebuffTemplate";

		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<DamageModificationGlobalNPC>().exampleDefenseDebuff = true; 
		}
	}
}
