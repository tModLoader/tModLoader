using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	public class MysteryWorld : ModWorld
	{
		internal IList<TagCompound> data;
		internal IList<TagCompound> mysteryNPCs;
		internal IList<TagCompound> mysteryKillCounts;

		public override void Initialize() {
			data = new List<TagCompound>();
			mysteryNPCs = new List<TagCompound>();
			mysteryKillCounts = new List<TagCompound>();
		}

		public override TagCompound Save() {
			return new TagCompound {
				["list"] = data,
				["mysteryNPCs"] = mysteryNPCs,
				["mysteryKillCounts"] = mysteryKillCounts
			};
		}

		public override void Load(TagCompound tag) {
			WorldIO.LoadModData(tag.GetList<TagCompound>("list"));
			WorldIO.LoadNPCs(tag.GetList<TagCompound>("mysteryNPCs"));
			WorldIO.LoadNPCKillCounts(tag.GetList<TagCompound>("mysteryKillCounts"));
		}
	}
}
