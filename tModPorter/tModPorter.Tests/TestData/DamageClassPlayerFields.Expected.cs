using Terraria;
using Terraria.ModLoader;

public class DamageClassPlayerFields : ModPlayer {
	public void MethodA() {
		Player.GetAttackSpeed(DamageClass.Melee) = 1;
		Player.GetKnockback(DamageClass.Summon).Base = 1;
		Player.GetArmorPenetration(DamageClass.Generic) += 1;
		Player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) = 2;
	}

	public override void UpdateEquips() {
		Player.GetDamage(DamageClass.Generic) += 0.10f;
		Player.GetDamage(DamageClass.Melee) += 0.1f;
		Player.GetDamage(DamageClass.Ranged) += 0.1f;
		Player.GetDamage(DamageClass.Magic) += 2;
		Player.GetDamage(DamageClass.Summon) += 0.1f;
		Player.GetDamage(DamageClass.Throwing) += 0.1f;
		Player.specialistDamage += 0.3f;

		Player.GetDamage(DamageClass.Generic) *= 0.08f;
		Player.GetDamage(DamageClass.Melee) *= 0.08f;
		Player.GetDamage(DamageClass.Ranged) *= 0.08f;
		Player.GetDamage(DamageClass.Magic) *= 0.08f;
		Player.GetDamage(DamageClass.Summon) *= 0.08f;
		Player.GetDamage(DamageClass.Throwing) *= 0.08f;

		Player.GetCritChance(DamageClass.Generic) += 1;
		Player.GetCritChance(DamageClass.Melee) *= 5;
		Player.GetCritChance(DamageClass.Ranged) *= 5;
		Player.GetCritChance(DamageClass.Magic) *= 5;
		Player.GetCritChance(DamageClass.Throwing) *= 5;
#if COMPILE_ERROR
		Player.GetDamage(DamageClass.Summon) = 8;
#endif
	}

	public void MethodB(Player player) {
		player.GetAttackSpeed(DamageClass.Melee) = 1;
		A.meleeSpeed = 2;
	}

	class A {
		public static int meleeSpeed = 0;
	}
}