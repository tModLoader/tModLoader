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
		/// An NPC will automatically become immune (<see cref="NPC.buffImmune"/>) to the indexed buff type if it is already immune to any buff in the corresponding List. <br/>
		/// For NPCs with <see cref="NPCID.Sets.ImmuneToAllBuffs"/> or <see cref="NPCID.Sets.ImmuneToRegularBuffs"/> set to true, the reverse of that logic is used to automatically set the NPC as vulnerable: The NPC will automatically become vulnerable to the indexed buff type if it is specifically vulnerable to all of the buffs in the corresponding List.<br/><br/>
		/// This set helps standardize several buff immunity inheritance rules that are effectively present in Terraria while providing modders the ability to do the same. For example, all NPC with immunity to <see cref="OnFire"/> are also immune to <see cref="OnFire3"/> (aka, "On Fire!" and "Hellfire").<br/>
		/// Modders can use this set to create buffs that act as variants of existing buffs, and should inherit the immunity value of those existing buffs. NPC from Terraria and other mods will automatically have these immunity rules applied, regardless of mod load order.
		/// </summary>
		public static List<int>[] GrantImmunityWith = Factory.CreateCustomSet<List<int>>(null,
			OnFire3, new List<int>() { OnFire }, // 48
			OnFire, new List<int>() { OnFire3 },
			Frostburn2, new List<int>() { Frostburn }, // 28
			Frostburn, new List<int>() { Frostburn2 },
			// These are not fully reciprocal. If immune to Venom, then immune to Poisoned, but not necessarily the other way.
			Poisoned, new List<int>() { Venom } // 5
		    // Ichor, new List<int>() { BetsysCurse } // 3
		);

		static Sets()
		{
			for (int i = 0; i < GrantImmunityWith.Length; i++) {
				GrantImmunityWith[i] ??= new List<int>(); // Done here instead of CreateCustomSet to not share List reference.
			}
		}
	}
}
