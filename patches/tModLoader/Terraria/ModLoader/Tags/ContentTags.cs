namespace Terraria.ModLoader.Tags
{
	public static class ContentTags
	{
		public static void AddTag<TGroup>(string tag, int id) where TGroup : TagGroup
			=> ModContent.GetInstance<TGroup>().AddTag(tag, id);

		public static void RemoveTag<TGroup>(string tag, int id) where TGroup : TagGroup
			=> ModContent.GetInstance<TGroup>().RemoveTag(tag, id);

		public static bool HasTag<TGroup>(string tag, int id) where TGroup : TagGroup
			=> ModContent.GetInstance<TGroup>().HasTag(tag, id);
	}
}
