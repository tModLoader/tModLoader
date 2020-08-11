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

		internal Dictionary<string, TagData> TagNameToData = new Dictionary<string, TagData>(StringComparer.InvariantCultureIgnoreCase);

		protected sealed override void Register() => ContentInstance.Register(this);

		public sealed override void Unload() {
			TagNameToData.Clear();

			TagNameToData = null;
		}

		/// <summary> Sets whether or not the content piece with the provided Id has the provided tag. </summary>
		/// <param name="tagName">The name of the tag.</param>
		/// <param name="id">The content id.</param>
		/// <param name="value">Whether or not the tag should be present for the provided content id.</param>
		public void SetTag(string tagName, int id, bool value = true)
			=> GetTagData(tagName).Set(id, value);

		/// <summary> Returns whether or not the content piece with the Id has the provided tag. </summary>
		/// <param name="tagName">The name of the tag.</param>
		/// <param name="id">The content id.</param>
		public bool HasTag(string tagName, int id)
			=> GetTagData(tagName).Has(id);

		internal TagData GetTagData(string tagName) {
			if (!TagNameToData.TryGetValue(tagName, out var data)) {
				TagNameToData[tagName] = data = new TagData(TypeCount);
			}

			return data;
		}
	}
}
