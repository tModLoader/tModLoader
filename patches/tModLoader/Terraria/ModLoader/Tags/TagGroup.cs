using System;
using System.Collections.Generic;

namespace Terraria.ModLoader.Tags
{
	/// <summary> Derivatives of TagGroups are used to store and get tags associated with content IDs. </summary>
	[Autoload(true)]
	public abstract class TagGroup : ModType
	{
		public abstract int TypeCount { get; }

		internal Dictionary<string, TagData> TagNameToData = new Dictionary<string, TagData>(StringComparer.InvariantCultureIgnoreCase);

		protected sealed override void Register() => ModTypeLookup<TagGroup>.Register(this);

		public sealed override void Unload() {
			TagNameToData.Clear();

			TagNameToData = null;
		}

		/// <summary> Returns a TagData instance, which can be used to modify and check for tags. <para/> <b>Be sure to cache the return value whenever possible!</b> </summary>
		/// <param name="tagName"> The name of the tag. </param>
		public TagData GetTag(string tagName) {
			if (!TagNameToData.TryGetValue(tagName, out var data)) {
				TagNameToData[tagName] = data = new TagData(TypeCount);
			}

			return data;
		}
	}
}
