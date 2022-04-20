namespace Terraria.ID;

public partial class BuffID
{
	public partial class Sets
	{
		/// <summary>
		/// Set for debuffs.
		/// Causes debuffs to last twice as long on players in expert mode. Defaults to false.
		/// </summary>
		public static bool[] LongerExpertDebuff = Factory.CreateBoolSet(Poisoned, Darkness, Cursed, OnFire, Bleeding, Confused, Slow, Weak, Silenced, BrokenArmor, CursedInferno, Frostburn, Chilled, Frozen, Ichor, Venom, Blackout);
	}
}
