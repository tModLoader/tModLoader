using System;

namespace Terraria.ModLoader.Tags
{
	/// <summary> This type cannot be derived from. Use TagShortcut with a generic parameter. </summary>
	public abstract class TagShortcut : ModType
	{
		/// <summary> The name of the tag that this shortcut should be associated with. This is only accessed once, during initialization. </summary>
		public abstract string TagName { get; }

		internal TagData tagData;

		internal TagShortcut() { }

		/// <summary> Sets whether or not the content piece with the provided Id has the tag that this shortcut points to. </summary>
		/// <param name="id">The content id.</param>
		/// <param name="value">Whether or not the tag should be present for the provided content id.</param>
		public void SetTag(int id, bool value = true)
			=> tagData.Set(id, value);

		/// <summary> Returns whether or not the content piece with the Id has the tag that this shortcut points to. </summary>
		/// <param name="id">The content id.</param>
		public bool HasTag(int id)
			=> tagData.Has(id);

		protected sealed override void Register() {
			ContentInstance.Register(this);
			ContentTags.TagShortcuts.Add(this);
		}

		internal abstract void UpdateData();
	}

	/// <summary>
	/// This class helps you heavily speed up your use of content tags with the power of generics, which will help avoid slow dictionary look-ups. <para/>
	/// Define your own shortcuts by making classes deriving from this one, and then use generic overloads of ContentTags methods with them.
	/// </summary>
	/// <typeparam name="TTagGroup">The tag group to grab a tag reference from. This is usually 'ItemTags', 'NPCTags', or etc. </typeparam>
	public abstract class TagShortcut<TTagGroup> : TagShortcut
		where TTagGroup : TagGroup
	{
		internal override void UpdateData() {
			string tagName = TagName ?? throw new ArgumentNullException($"'{GetType().Name}.{nameof(TagName)}' returned null.");

			tagData = ModContent.GetInstance<TTagGroup>().GetTagData(tagName);
		}
	}
}
