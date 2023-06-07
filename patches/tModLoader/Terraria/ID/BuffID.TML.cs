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
		/// For NPCs with <see cref="NPCID.Sets.FullyImmuneToBuffs"/> or <see cref="NPCID.Sets.DefaultDebuffImmunity"/> set to true, the reverse of that logic is used to automatically set the NPC as un-immune: The NPC will automatically become un-immune to the indexed buff type if it is already un-immune to all of the buffs in the corresponding List.<br/><br/>
		/// This set helps standardize several buff immunity inheritance rules that are effectively present in Terraria while providing modders the ability to do the same. For example, all NPC with immunity to <see cref="OnFire"/> are also immune to <see cref="OnFire3"/>.<br/>
		/// Modders can use this set to create buffs that act as variants of existing buffs, and should inherit the immunity value of those existing buffs. NPC from Terraria and other mods will automatically have these immunity rules applied, regardless of mod load order.
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
