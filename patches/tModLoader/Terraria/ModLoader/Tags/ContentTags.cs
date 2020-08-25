using System;
using System.Collections.Generic;

namespace Terraria.ModLoader.Tags
{
	public static class ContentTags
	{
		internal static readonly Dictionary<Type,TagGroup> TagGroupsByType = new Dictionary<Type, TagGroup>();

		/// <summary> Returns a TagData instance, which can be used to modify and check for tags. <para/> <b>Be sure to cache the return value whenever possible!</b> </summary>
		/// <typeparam name="TTagGroup"> The tag group that the tag comes from. </typeparam>
		/// <param name="tagName"> The name of the tag. </param>
		public static TagData Get<TTagGroup>(string tagName) where TTagGroup : TagGroup
			=> GetGroup<TTagGroup>().GetTag(tagName);

		/// <summary> Returns an instance of the specified TagGroup. This is just a shorthand for ModContent.GetInstance. </summary>
		/// <typeparam name="TTagGroup"> The tag group that the tag comes from. </typeparam>
		/// <param name="tag"> The name of the tag. </param>
		public static TTagGroup GetGroup<TTagGroup>() where TTagGroup : TagGroup
			=> ModContent.GetInstance<TTagGroup>();
	}
}
