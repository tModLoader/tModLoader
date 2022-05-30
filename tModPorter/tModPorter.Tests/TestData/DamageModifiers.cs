using Terraria;
using Terraria.ModLoader;

public class DamageModifiers : ModPlayer
{
	public override void UpdateEquips() {
		player.magicDamage += 2;
		player.meleeCrit *= 5;
#if COMPILE_ERROR
		player.minionDamage = 8;
#endif
	}
}