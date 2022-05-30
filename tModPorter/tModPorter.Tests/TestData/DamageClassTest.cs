using Terraria;
using Terraria.ModLoader;

public class DamageClassTest : Mod
{
	public void MethodA()
	{
		Item item = new();
		item.melee = true;
		item.magic = false;
		item.summon = true;
	}

	public void MethodB(AnotherItemClass item)
	{
		item.melee = true;

		int melee = 0;
		melee = 1;
	}
}