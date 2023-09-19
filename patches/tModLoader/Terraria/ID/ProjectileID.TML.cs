namespace Terraria.ID;

#if TMLCODEASSIST
[tModCodeAssist.IDType.Sets.AssociatedName(ModLoader.Annotations.IDTypeAttribute.Projectile)]
#endif
partial class ProjectileID
{
#if !TMLCODEASSIST
	partial class Sets
	{
		/// <summary>
		/// Set of Grapple Hook Projectile IDs that determines whether or not said projectile can only have one copy of it within the world per player.
		/// </summary>
		public static bool[] SingleGrappleHook = Factory.CreateBoolSet(false, 13, 315, 230, 231, 232, 233, 234, 235, 331, 753, 865, 935);

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
#endif
}
