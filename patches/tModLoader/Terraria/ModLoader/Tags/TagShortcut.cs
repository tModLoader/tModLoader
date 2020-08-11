namespace Terraria.ModLoader.Tags
{
	/// <summary>
	/// This class helps you heavily speed up your use of content tags, with the power of generics, which will help avoid slow dictionary look-ups. <para/>
	/// Define your own shortcuts by making classes deriving from this one, and then use generic overloads of ContentTags methods with them.
	/// </summary>
	/// <typeparam name="TagGroup">The base content type that the tag will be grabbed from. This is usually 'Item', 'NPC', or 'Tile'. </typeparam>
	public abstract class TagShortcut<TagGroup>
	{
		public abstract string TagName { get; }
	}
}
