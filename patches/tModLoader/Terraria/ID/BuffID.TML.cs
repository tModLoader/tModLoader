using System.Collections.Generic;

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

		/// <summary>
		/// An NPC will automatically become immune to the indexed buff type if it is already immune to any buff in the corresponding List.
		/// </summary>
		public static List<int>[] GrantImmunityWith = Factory.CreateCustomSet<List<int>>(null,
			OnFire3, new List<int>() { OnFire },
			Frostburn2, new List<int>() { Frostburn },
			Poisoned, new List<int>() { Venom }
			// Ichor, new List<int>() { BetsysCurse }
		);

		static Sets()
		{
			for (int i = 0; i < GrantImmunityWith.Length; i++) {
				GrantImmunityWith[i] ??= new List<int>(); // Done here instead of CreateCustomSet to not share List reference.
			}
		}
	}
}
