using Terraria;
using Terraria.ModLoader;

public class DamageModifiers : ModPlayer
{
	public override void UpdateEquips() {
		Player.GetDamage(DamageClass.Magic) += 2;
		Player.GetCritChance(DamageClass.Melee) *= 5;
#if COMPILE_ERROR
		Player.GetDamage(DamageClass.Summon) = 8;
#endif
	}
}