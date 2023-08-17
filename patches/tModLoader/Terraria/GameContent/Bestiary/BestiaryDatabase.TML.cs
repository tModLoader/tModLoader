using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace Terraria.GameContent.Bestiary;

public partial class BestiaryDatabase
{
	private List<BestiaryEntry> _vanillaEntries = new List<BestiaryEntry>();
	private Dictionary<Mod, List<BestiaryEntry>> _byMod = new Dictionary<Mod, List<BestiaryEntry>>();

	/// <summary>
	/// Gets entries from the database created by the mod specified
	/// </summary>
	/// <param name="mod">The mod to find entries from (null for Terraria)</param>
	/// <returns>A list of the entries created by the mod specified or null if it created none</returns>
	public List<BestiaryEntry> GetBestiaryEntriesByMod(Mod mod)
	{
		if (mod == null)
			return _vanillaEntries;

		_byMod.TryGetValue(mod, out var value);
		return value;
	}

	/// <summary>
	/// Gets the completed percent of the given mod's bestiary
	/// </summary>
	/// <param name="mod">The mod to calculate bestiary completeness (null for Terraria)</param>
	/// <returns>A float ranging from 0 to 1 representing the completeness of the bestiary or returns -1 if the mod has no entries</returns>
	public float GetCompletedPercentByMod(Mod mod)
	{
		if (mod == null) {
			return _vanillaEntries.Count(e => e.UIInfoProvider.GetEntryUICollectionInfo().UnlockState > BestiaryEntryUnlockState.NotKnownAtAll_0) / (float)_vanillaEntries.Count;
		}
		else if (_byMod.TryGetValue(mod, out var value)) {
			return value.Count(e => e.UIInfoProvider.GetEntryUICollectionInfo().UnlockState > BestiaryEntryUnlockState.NotKnownAtAll_0) / (float)value.Count;
		}

		return -1f;
	}
}
