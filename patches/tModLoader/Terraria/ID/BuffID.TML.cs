namespace Terraria.ID;

public partial class BuffID
{
	public partial class Sets
	{
		/// <summary>
		/// Set for debuffs.
		/// Causes debuffs to last twice as long on players in expert mode. Defaults to false.
		/// </summary>
		public static bool[] LongerExpertDebuff = Factory.CreateBoolSet(20, 22, 23, 24, 30, 31, 32, 33, 35, 36, 39, 44, 46, 47, 69, 70, 80);
	}
}
