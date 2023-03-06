using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Buffs
{
	public class ExampleWhipDebuff : ModBuff
	{
		public static readonly int TagDamage = 5;

		public override void SetStaticDefaults() {
			// This allows the debuff to be inflicted on NPCs that would otherwise be immune to all debuffs.
			// Other mods may check it for different purposes.
			BuffID.Sets.IsAnNPCWhipDebuff[Type] = true;
		}

		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<ExampleWhipDebuffNPC>().tagBonusDamage += TagDamage;
		}
	}

	// Each whip debuff has different tag damage values. ExampleWhipAdvanced applies this debuff instead.
	public class ExampleWhipAdvancedDebuff : ModBuff
	{
		public static readonly int TagDamage = 10;

		public override void SetStaticDefaults() {
			BuffID.Sets.IsAnNPCWhipDebuff[Type] = true;
		}

		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<ExampleWhipDebuffNPC>().tagBonusDamage += TagDamage;
		}
	}

	public class ExampleWhipDebuffNPC : GlobalNPC
	{
		// This is required to store information on entities that isn't shared between them.
		public override bool InstancePerEntity => true;

		public int tagBonusDamage;

		public override void ResetEffects(NPC npc) {
			tagBonusDamage = 0;
		}

		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers) {
			// Only player attacks should benefit from this buff, hence the NPC and trap checks.
			if (tagBonusDamage > 0 && !projectile.npcProj && !projectile.trap && (projectile.minion || ProjectileID.Sets.MinionShot[projectile.type] || projectile.sentry || ProjectileID.Sets.SentryShot[projectile.type])) {
				modifiers.FlatBonusDamage += tagBonusDamage;
			}
		}
	}
}
