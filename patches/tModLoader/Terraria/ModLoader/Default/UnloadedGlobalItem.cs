using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	public class UnloadedGlobalItem : GlobalItem
	{
		internal IList<TagCompound> data = new List<TagCompound>();

		public override bool InstancePerEntity => true;

		public override GlobalItem Clone(Item item, Item newItem) {
			UnloadedGlobalItem clone = (UnloadedGlobalItem)base.Clone(item, newItem);
			if (data != null) {
				clone.data = TagIO.Clone(data);
			}
			return clone;
		}

		public override bool NeedsSaving(Item item) {
			return data.Count > 0;
		}

		public override TagCompound Save(Item item) {
			return new TagCompound { ["modData"] = data };
		}

		public override void Load(Item item, TagCompound tag) {
			ItemIO.LoadGlobals(item, tag.GetList<TagCompound>("modData"));
		}
	}
}
