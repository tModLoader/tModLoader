using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	public class UnloadedWorld : ModWorld
	{
		internal IList<TagCompound> data;
		internal IList<TagCompound> unloadedNPCs;
		internal IList<TagCompound> unloadedKillCounts;

		public override void Initialize() {
			data = new List<TagCompound>();
			unloadedNPCs = new List<TagCompound>();
			unloadedKillCounts = new List<TagCompound>();
		}

		public override TagCompound Save() {
			return new TagCompound {
				["list"] = data,
				["unloadedNPCs"] = unloadedNPCs,
				["unloadedKillCounts"] = unloadedKillCounts
			};
		}

		public override void Load(TagCompound tag) {
			WorldIO.LoadModData(tag.GetList<TagCompound>("list"));
			WorldIO.LoadNPCs(tag.GetList<TagCompound>("unloadedNPCs"));
			WorldIO.LoadNPCKillCounts(tag.GetList<TagCompound>("unloadedKillCounts"));
		}
	}
}
