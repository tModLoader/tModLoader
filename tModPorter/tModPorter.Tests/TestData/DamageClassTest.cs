using Terraria;

public class DamageClassTest
{
	public void MethodA()
	{
		Item item = new();
		item.melee = true;
		item.magic = true;
		item.summon = true;
		item.thrown = true;
		item.ranged = true;

#if COMPILE_ERROR
		item.magic = false;

		// can't port conditional setter, emit a suggestion
		item.ranged = 1 > 2;
#endif

		bool itemIsmelee = item.melee;
		bool itemIsmagic = item.magic;
		bool itemIssummon = item.summon;
		bool itemIsthrowing = item.thrown;
		bool itemIsRanged = item.ranged;

		Projectile proj = new();
		// No minion, that is different
		bool projIsmelee = proj.melee;
		bool projIsmagic = proj.magic;
		bool projIsthrowing = proj.thrown;
		bool projIsRanged = proj.ranged;
	}

	public void MethodB(AnotherItemClass item)
	{
		item.melee = true;

		int melee = 0;
		melee = 1;
	}
}