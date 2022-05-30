using Terraria;
using Terraria.ModLoader;

public class DamageClassPlayerFields : ModPlayer {
	public void MethodA() {
		Player.GetAttackSpeed(DamageClass.Melee) = 1;
		Player.GetKnockback(DamageClass.Summon).Base = 1;
		Player.GetArmorPenetration(DamageClass.Generic) += 1;
		Player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) = 2;
	}

	public void MethodB(Player player) {
		player.GetAttackSpeed(DamageClass.Melee) = 1;
		A.meleeSpeed = 2;
	}

	class A {
		public static int meleeSpeed = 0;
	}
}