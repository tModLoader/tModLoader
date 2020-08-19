namespace Terraria.ModLoader
{
	public class Melee : ModDamageClass
	{
		public override string ClassName => Lang.tip[2].Value;
	}

	public class Ranged : ModDamageClass
	{
		public override string ClassName => Lang.tip[3].Value;
	}

	public class Magic : ModDamageClass
	{
		public override string ClassName => Lang.tip[4].Value;
	}

	public class Summon : ModDamageClass
	{
		public override string ClassName => Lang.tip[53].Value;
	}
}
