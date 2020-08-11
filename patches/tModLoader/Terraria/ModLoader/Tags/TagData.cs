using System.Collections.Generic;

namespace Terraria.ModLoader.Tags
{
	internal class TagData
	{
		public bool[] idToValue; //Accessed for quick HasTag checks.
		public List<int> entryList;
		public IReadOnlyList<int> readonlyEntryList; //Accessed for GetEntriesWithTag.

		public TagData(int maxEntries) {
			idToValue = new bool[maxEntries];
			entryList = new List<int>();
			readonlyEntryList = entryList.AsReadOnly();
		}

		public void Set(int id, bool value) {
			idToValue[id] = value;

			if (!value) {
				entryList.Remove(id);
			}
			else if (!entryList.Contains(id)) {
				entryList.Add(id);
			}
		}

		public bool Has(int id) => idToValue[id];
	}
}
