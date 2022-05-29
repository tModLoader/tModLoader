using Terraria;
using Terraria.ModLoader;

public class DamageClassTest : Mod
{
	public void MethodA()
	{
		Item item = new();
		item.DamageType = DamageClass.Melee;
		// item.magic = false /* tModPorter - this is redundant, for more info see https://github.com/tModLoader/tModLoader/wiki/Update-Migration-Guide#damage-classes */ ;
		item.DamageType = DamageClass.Summon;
	}

	public void MethodB(AnotherItemClass item)
	{
		item.melee = true;

		int melee = 0;
		melee = 1;
	}
}