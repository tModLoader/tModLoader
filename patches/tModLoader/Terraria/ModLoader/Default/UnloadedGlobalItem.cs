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

		public override void SaveData(Item item, TagCompound tag) {
			if (data.Count > 0) {
				tag["modData"] = data;
			}
		}

		public override void LoadData(Item item, TagCompound tag) {
			if (tag.ContainsKey("modData")) {
				ItemIO.LoadGlobals(item, tag.GetList<TagCompound>("modData"));
			}
		}
	}
}
