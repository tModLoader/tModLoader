namespace Terraria.ID;

partial class ProjectileID
{
	partial class Sets
	{
		/// <summary>
		/// Used to scale down summon tag damage for fast hitting minions and sentries. 
		/// </summary>
		public static float[] SummonTagDamageMultiplier = Factory.CreateFloatSet(1f,
			ProjectileID.Smolstar, 0.75f,
			ProjectileID.DD2LightningAuraT1, 0.5f,
			ProjectileID.DD2LightningAuraT2, 0.5f,
			ProjectileID.DD2LightningAuraT3, 0.5f
		);
	}
}
