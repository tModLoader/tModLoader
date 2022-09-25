using System;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players;

public class ExampleDynamicPlayer : ModPlayer
{
	// This system will only be loaded when DynamicModConfig is instanced.
	public override bool IsLoadingEnabled(Mod mod) {
		return Configs.DynamicModConfig.Instance != null;
	}

	public override void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit) {
		if (Configs.DynamicModConfig.Instance.UltraRapidFire) {
			damage = (int)Math.Min((long)damage * 1000, int.MaxValue);
			crit = true;
		}
	}

	public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback,
		ref bool crit, ref int hitDirection) {
		if (Configs.DynamicModConfig.Instance.UltraRapidFire) {
			damage = (int)Math.Min((long)damage * 1000, int.MaxValue);
			crit = true;
		}
	}

	public override void ModifyHitByProjectile(Projectile proj, ref int damage, ref bool crit) {
		if (Configs.DynamicModConfig.Instance.UltraRapidFire) {
			damage = (int)Math.Min((long)damage * 10, int.MaxValue);
			crit = true;
		}
	}

	public override void ModifyHitByNPC(NPC npc, ref int damage, ref bool crit) {
		if (Configs.DynamicModConfig.Instance.UltraRapidFire) {
			damage = (int)Math.Min((long)damage * 10, int.MaxValue);
			crit = true;
		}
	}

	public override bool? CanConsumeBait(Item bait) {
		return !Configs.DynamicModConfig.Instance.UltraRapidFire;
	}

	public override bool CanConsumeAmmo(Item weapon, Item ammo) {
		return !Configs.DynamicModConfig.Instance.UltraRapidFire;
	}
}