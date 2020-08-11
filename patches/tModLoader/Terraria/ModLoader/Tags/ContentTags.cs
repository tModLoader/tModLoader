using System;
using System.Collections.Generic;
using System.Reflection;

namespace Terraria.ModLoader.Tags
{
	public static class ContentTags
	{
		internal static readonly List<TagShortcut> TagShortcuts = new List<TagShortcut>();
		internal static readonly Dictionary<Type,TagGroup> TagGroupsByType = new Dictionary<Type, TagGroup>();

		/// <summary> Sets whether or not the content piece with the provided Id has the provided tag. </summary>
		/// <param name="tagName">The name of the tag.</param>
		/// <param name="id">The content id.</param>
		/// <param name="value">Whether or not the tag should be present for the provided content id.</param>
		public static void SetTag<TTagGroup>(string tag, int id, bool value = true) where TTagGroup : TagGroup
			=> ModContent.GetInstance<TTagGroup>().SetTag(tag, id, value);

		/// <summary> Returns whether or not the content piece with the Id has the provided tag. </summary>
		/// <param name="tagName">The name of the tag.</param>
		/// <param name="id">The content id.</param>
		public static bool HasTag<TTagGroup>(string tag, int id) where TTagGroup : TagGroup
			=> ModContent.GetInstance<TTagGroup>().HasTag(tag, id);

		/// <summary> Sets whether or not the content piece with the provided Id has the tag that the provided shortcut points to. </summary>
		/// <param name="id">The content id.</param>
		/// <param name="value">Whether or not the tag should be present for the provided content id.</param>
		public static void SetTag<TTagShortcut>(int id, bool value = true) where TTagShortcut : TagShortcut
			=> ModContent.GetInstance<TTagShortcut>().SetTag(id, value);

		/// <summary> Returns whether or not the content piece with the Id has the tag that the provided shortcut points to. </summary>
		/// <param name="id">The content id.</param>
		public static bool HasTag<TTagShortcut>(int id) where TTagShortcut : TagShortcut
			=> ModContent.GetInstance<TTagShortcut>().HasTag(id);

		internal static void Initialize() {
			//The following associates TagShortcuts with TagData instances.
			var updateDataMethod = typeof(TagShortcut<>).GetMethod(nameof(TagShortcut<TagGroup>.UpdateData),BindingFlags.Instance|BindingFlags.NonPublic);

			foreach (object obj in TagShortcuts) {
				updateDataMethod.Invoke(obj, null);
			}
		}
	}
}
