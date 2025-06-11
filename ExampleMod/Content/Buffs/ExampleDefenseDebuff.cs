using ExampleMod.Common.GlobalNPCs;
using ExampleMod.Common.Players;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Buffs
{
	/// <summary>
	/// This debuff reduces enemy armor by 25%. Use <see cref="Content.Items.Weapons.HitModifiersShowcase"/> or <see cref="Items.Consumables.ExampleFlask"/> to apply.
	/// By using a buff we can apply to both players and NPCs, and also rely on vanilla to sync the AddBuff calls so we don't need to write our own netcode
	/// </summary>
	public class ExampleDefenseDebuff : ModBuff
	{
		public const int DefenseReductionPercent = 25;
		public static float DefenseMultiplier = 1 - DefenseReductionPercent / 100f;

		public override void SetStaticDefaults() {
			Main.pvpBuff[Type] = true; // This buff can be applied by other players in Pvp, so we need this to be true.

			// Our BuffImmuneGlobalNPC class changes some buff immunity logic. NPCs immune to Ichor will automatically be immune to this buff.
			BuffImmuneGlobalNPC.SetDefenseDebuffStaticDefaults(Type);
		}

		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<DamageModificationGlobalNPC>().exampleDefenseDebuff = true;
		}

		public override void Update(Player player, ref int buffIndex) {
			player.GetModPlayer<ExampleDamageModificationPlayer>().exampleDefenseDebuff = true;
			player.statDefense *= DefenseMultiplier;
		}
	}
}
