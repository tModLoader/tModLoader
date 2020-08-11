using System;
using System.Collections.Generic;

namespace Terraria.ModLoader.Tags
{
	/// <summary>
	/// Derivatives of TagGroups are used to store and get tags associated with content IDs.
	/// </summary>
	[Autoload(true)]
	public abstract class TagGroup : ModType
	{
		public abstract int TypeCount { get; }

		internal Dictionary<string, TagData> TagToIds = new Dictionary<string, TagData>(StringComparer.InvariantCultureIgnoreCase);

		protected sealed override void Register() => ContentInstance.Register(this);

		public sealed override void Unload() {
			TagToIds.Clear();

			TagToIds = null;
		}

		private TagData GetTagData(string tagName) {
			if (!TagToIds.TryGetValue(tagName, out var data)) {
				TagToIds[tagName] = data = new TagData(TypeCount);
			}

			return data;
		}

		public void AddTag(string tagName, int id) {
			var data = GetTagData(tagName);

			data.idToValue[id] = true;

			if (!data.entryList.Contains(id)) {
				data.entryList.Add(id);
			}
		}

		public void RemoveTag(string tagName, int id) {
			var data = GetTagData(tagName);

			data.idToValue[id] = false;

			data.entryList.Remove(id);
		}

		public bool HasTag(string tagName, int id) => GetTagData(tagName).idToValue[id];
	}
}
