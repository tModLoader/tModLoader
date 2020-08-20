namespace Terraria.ModLoader
{
	public class Melee : DamageClass
	{
		public override string ClassName => Lang.tip[2].Value;
	}

	public class Ranged : DamageClass
	{
		public override string ClassName => Lang.tip[3].Value;
	}

	public class Magic : DamageClass
	{
		public override string ClassName => Lang.tip[4].Value;
	}

	public class Summon : DamageClass
	{
		public override string ClassName => Lang.tip[53].Value;
	}
}
