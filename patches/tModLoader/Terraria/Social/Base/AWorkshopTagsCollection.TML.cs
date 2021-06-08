using System.Collections.Generic;

namespace Terraria.Social.Base
{
	public abstract partial class AWorkshopTagsCollection
	{
		public readonly List<WorkshopTagOption> WorldTags = new List<WorkshopTagOption>();
		public readonly List<WorkshopTagOption> ResourcePackTags = new List<WorkshopTagOption>();

		protected void AddWorldTag(string tagNameKey, string tagInternalName) {
			WorldTags.Add(new WorkshopTagOption(tagNameKey, tagInternalName));
		}

		protected void AddResourcePackTag(string tagNameKey, string tagInternalName) {
			ResourcePackTags.Add(new WorkshopTagOption(tagNameKey, tagInternalName));
		}
	}
}
