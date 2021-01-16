using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	[LegacyName("UnloadedWorld")]
	public class UnloadedSystem : ModSystem
	{
		internal IList<TagCompound> data;
		internal IList<TagCompound> unloadedNPCs;
		internal IList<TagCompound> unloadedKillCounts;

		public override void OnWorldLoad() {
			data = new List<TagCompound>();
			unloadedNPCs = new List<TagCompound>();
			unloadedKillCounts = new List<TagCompound>();
		}

		public override TagCompound SaveWorldData() {
			return new TagCompound {
				["list"] = data,
				["unloadedNPCs"] = unloadedNPCs,
				["unloadedKillCounts"] = unloadedKillCounts
			};
		}

		public override void LoadWorldData(TagCompound tag) {
			WorldIO.LoadModData(tag.GetList<TagCompound>("list"));
			WorldIO.LoadNPCs(tag.GetList<TagCompound>("unloadedNPCs"));
			WorldIO.LoadNPCKillCounts(tag.GetList<TagCompound>("unloadedKillCounts"));
		}
	}
}
