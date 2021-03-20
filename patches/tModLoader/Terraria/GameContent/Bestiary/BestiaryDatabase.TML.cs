using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace Terraria.GameContent.Bestiary
{
	public partial class BestiaryDatabase
	{
		/// <summary>
		/// Gets entries from the database created by the mod specified
		/// </summary>
		/// <param name="mod">The mod to find entries from</param>
		/// <returns>A list of the entries created by the mod specified or null if it created none</returns>
		public List<BestiaryEntry> GetBestiaryEntriesByMod(Mod mod) {
			_byMod.TryGetValue(mod, out var value);
			return value;
		}

		/// <summary>
		/// Gets entries from the database created by Terraria
		/// </summary>
		/// <returns>A list of the entries the vanilla Terraria created</returns>
		public List<BestiaryEntry> GetTerrariaBestiaryEntires() {
			return _vanillaEntries;
		}

		/// <summary>
		/// Gets the completed percent of the given mod's bestiary
		/// </summary>
		/// <param name="mod">The mod to calculate bestiary completeness</param>
		/// <returns>A float ranging from 0 to 1 representing the completeness of the bestiary or returns -1 if the mod has no entries</returns>
		public float GetCompletedPercentByMod(Mod mod) {
			if (_byMod.TryGetValue(mod, out var value)) {
				return value.Count(e => e.UIInfoProvider.GetEntryUICollectionInfo().UnlockState > BestiaryEntryUnlockState.NotKnownAtAll_0) / (float)value.Count;
			}
			return -1f;
		}

		/// <summary>
		/// Gets the completed percent of Terraria's bestiary
		/// </summary>
		/// <returns>A float ranging from 0 to 1 representing the completeness of the bestiary</returns>
		public float GetTerrariaCompletedPercent() {
			return _vanillaEntries.Count(e => e.UIInfoProvider.GetEntryUICollectionInfo().UnlockState > BestiaryEntryUnlockState.NotKnownAtAll_0) / (float)_vanillaEntries.Count;
		}
	}
}
