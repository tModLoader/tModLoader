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
	}
}
