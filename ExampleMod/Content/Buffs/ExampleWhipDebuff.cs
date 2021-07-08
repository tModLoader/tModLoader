using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Buffs
{
	public class ExampleWhipDebuff : ModBuff
	{
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<ExampleWhipDebuffNPC>().markedByExampleWhip = true;
		}
	}

	public class ExampleWhipDebuffNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;
		public override bool CloneNewInstances => true;

		public bool markedByExampleWhip;

		public override void ResetEffects(NPC npc) {
			markedByExampleWhip = false;
		}

		// Currently, this is inconsistent with vanilla, increasing damage after it is randomised instead of before. It will be changed to a different hook in the future.
		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			if (markedByExampleWhip && !projectile.npcProj && !projectile.trap && (projectile.minion || ProjectileID.Sets.MinionShot[projectile.type])) {
				damage += 5;
			}
		}
	}
}
