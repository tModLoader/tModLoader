using Terraria;
using Terraria.ModLoader;

public class DamageClassPlayerFields : ModPlayer {
	public void MethodA() {
		player.meleeSpeed = 1;
		player.minionKB = 1;
		player.armorPenetration += 1;
		player.whipUseTimeMultiplier = 2;
	}

	public override void UpdateEquips() {
		player.allDamage += 0.10f;
		player.meleeDamage += 0.1f;
		player.rangedDamage += 0.1f;
		player.magicDamage += 2;
		player.minionDamage += 0.1f;
		player.thrownDamage += 0.1f;
		player.rocketDamage += 0.3f;

		player.allDamageMult *= 0.08f;
		player.meleeDamageMult *= 0.08f;
		player.rangedDamageMult *= 0.08f;
		player.magicDamageMult *= 0.08f;
		player.minionDamageMult *= 0.08f;
		player.thrownDamageMult *= 0.08f;

		player.allCrit += 1;
		player.meleeCrit *= 5;
		player.rangedCrit *= 5;
		player.magicCrit *= 5;
		player.thrownCrit *= 5;
		player.minionDamage = 8;
	}

	public void MethodB(Player player) {
		player.meleeSpeed = 1;
		A.meleeSpeed = 2;
	}

	class A {
		public static int meleeSpeed = 0;
	}
}