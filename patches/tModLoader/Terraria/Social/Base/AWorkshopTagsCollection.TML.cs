using System;
using System.Collections.Generic;

namespace Terraria.Social.Base;

public abstract partial class AWorkshopTagsCollection
{
	[Obsolete("Use SteamedWraps.ModTags instead")]
	public readonly List<WorkshopTagOption> ModTags = new List<WorkshopTagOption>();

	protected void AddModTag(string tagNameKey, string tagInternalName)
	{
		ModTags.Add(new WorkshopTagOption(tagNameKey, tagInternalName));
	}
}
