using System.Collections.Generic;

namespace Terraria.ModLoader.Tags
{
	/// <summary> This class stores lists of content Ids associated with the tag that it represents. </summary>
	public sealed class TagData
	{
		private readonly bool[] idToValue; //Accessed for quick HasTag checks.
		private readonly List<int> entryList;
		private readonly IReadOnlyList<int> readonlyEntryList; //Accessed for GetEntriesWithTag.

		internal TagData(int maxEntries) {
			idToValue = new bool[maxEntries];
			entryList = new List<int>();
			readonlyEntryList = entryList.AsReadOnly();
		}

		/// <summary> Returns whether or not the content piece with the Id has this tag. </summary>
		/// <param name="id"> The content id. </param>
		public bool Has(int id) => idToValue[id];

		/// <summary> Returns a list of content Ids that have this tag. </summary>
		public IReadOnlyList<int> GetEntries() => readonlyEntryList;

		/// <summary> Sets whether or not the content piece with the provided Id has this tag. </summary>
		/// <param name="id"> The content id. </param>
		/// <param name="value"> Whether or not the tag should be present for the provided content id. </param>
		public void Set(int id, bool value) {
			idToValue[id] = value;

			if (!value) {
				entryList.Remove(id);
			}
			else if (!entryList.Contains(id)) {
				entryList.Add(id);
			}
		}
	}
}
