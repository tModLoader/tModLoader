using Terraria;
using Terraria.ModLoader;

public class DamageClassTest : Mod
{
	public void MethodA()
	{
		Item item = new();
		item.melee = true;
		item.magic = true;
		item.summon = true;
		item.throwing = true;
		item.ranged = true;

		item.magic = false;

		// can't port conditional setter, emit a suggestion
		item.ranged = 1 > 2;
	}

	public void MethodB(AnotherItemClass item)
	{
		item.melee = true;

		int melee = 0;
		melee = 1;
	}
}