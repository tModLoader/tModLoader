using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	public class MysteryGlobalItem : GlobalItem
	{
		internal IList<TagCompound> data = new List<TagCompound>();
		public bool HasData => data.Count > 0;

		public override bool CloneNewInstances => true;

		public override GlobalItem Clone()
		{
			MysteryGlobalItem clone = (MysteryGlobalItem)base.Clone();
			if (data != null)
			{
				clone.data = TagIO.Clone(data);
			}
			return clone;
		}

		public override bool NeedsSaving(Item item)
		{
			return HasData;
		}

		public override TagCompound Save(Item item)
		{
			return new TagCompound {["modData"] = data};
		}

		public override void Load(Item item, TagCompound tag)
		{
			ItemIO.LoadGlobals(item, tag.GetList<TagCompound>("list"));
		}
	}
}
