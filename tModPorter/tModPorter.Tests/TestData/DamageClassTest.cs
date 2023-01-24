using Terraria;
using Terraria.ModLoader;

public class DamageClassTest : DamageClass
{
	public void MethodA()
	{
		Item item = new();
		item.melee = true;
		item.magic = true;
		item.summon = true;
		item.thrown = true;
		item.ranged = true;

		item.magic = false;

		// can't port conditional setter, emit a suggestion
		item.ranged = 1 > 2;

		bool itemIsmelee = item.melee;
		bool itemIsmagic = item.magic;
		bool itemIssummon = item.summon;
		bool itemIsthrowing = item.thrown;
		bool itemIsRanged = item.ranged;

		Projectile proj = new();
		bool projIsminion = proj.minion; // Don't port minion, that is different
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

	public override void SetStaticDefaults()
	{
		ClassName.SetDefault("MyDamageClass");
	}
}