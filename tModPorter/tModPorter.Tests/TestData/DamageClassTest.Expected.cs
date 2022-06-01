using Terraria;
using Terraria.ModLoader;

public class DamageClassTest : Mod
{
	public void MethodA()
	{
		Item item = new();
		item.DamageType = DamageClass.Melee;
		item.DamageType = DamageClass.Magic;
		item.DamageType = DamageClass.Summon;
		item.DamageType = DamageClass.Throwing;
		item.DamageType = DamageClass.Ranged;

#if COMPILE_ERROR
		item.magic = false /* Suggestion: remove. See https://github.com/tModLoader/tModLoader/wiki/Update-Migration-Guide#damage-classes */ ;

		// can't port conditional setter, emit a suggestion
		item.ranged/* Suggestion: item.DamageType = ... */ = 1 > 2;
#endif
	}

	public void MethodB(AnotherItemClass item)
	{
		item.melee = true;

		int melee = 0;
		melee = 1;
	}
}