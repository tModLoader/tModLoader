using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	public class UnloadedGlobalItem : GlobalItem
	{
		[CloneByReference] // safe to share between clones, because it cannot be changed after creation/load
		internal IList<TagCompound> data = new List<TagCompound>();

		public override bool InstancePerEntity => true;

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
