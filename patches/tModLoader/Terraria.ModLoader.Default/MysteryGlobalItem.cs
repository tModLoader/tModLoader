using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	public class MysteryGlobalItem : GlobalItem
	{
		public override bool NeedsSaving(Item item)
		{
			return item.GetModInfo<MysteryGlobalItemInfo>(mod).HasData;
		}

		public override TagCompound Save(Item item)
		{
			return new TagCompound {["modData"] = item.GetModInfo<MysteryGlobalItemInfo>(mod).data};
		}

		public override void Load(Item item, TagCompound tag)
		{
			ItemIO.LoadGlobals(item, tag.GetList<TagCompound>("list"));
		}
	}

	public class MysteryGlobalItemInfo : ItemInfo
	{
		internal IList<TagCompound> data = new List<TagCompound>();
		public bool HasData => data.Count > 0;

		public override ItemInfo Clone()
		{
			var clone = (MysteryGlobalItemInfo)base.Clone();
			if (data != null)
				clone.data = TagIO.Clone(data);

			return clone;
		}
	}
}
