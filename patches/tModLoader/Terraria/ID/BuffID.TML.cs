namespace Terraria.ID;

public partial class BuffID
{
	public partial class Sets
	{
		// Created based on 'Player.AddBuff_DetermineBuffTimeToAdd'.
		/// <summary>
		/// If <see langword="true"/> for a given <see cref="BuffID"/>, then that buff will have a longer duration in <see cref="Main.expertMode"/>.
		/// </summary>
		/// <remarks>
		/// The scale factor used is the debuff multiplier (<see cref="DataStructures.GameModeData.DebuffTimeMultiplier"/>) of <see cref="Main.GameModeInfo"/>.
		/// <br/> Despite the name, this set applies to <em>all</em> buffs, not just debuffs.
		/// </remarks>
		public static bool[] LongerExpertDebuff = Factory.CreateBoolSet(20, 22, 23, 24, 30, 31, 32, 33, 35, 36, 39, 44, 46, 47, 69, 70, 80);
	}
}
