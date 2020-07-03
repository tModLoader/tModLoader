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

		public override void LoadLegacy(BinaryReader reader) {
			var list = new List<TagCompound>();
			int count = reader.ReadUInt16();
			for (int k = 0; k < count; k++) {
				list.Add(new TagCompound {
					["mod"] = reader.ReadString(),
					["name"] = reader.ReadString(),
					["legacyData"] = reader.ReadBytes(reader.ReadUInt16())
				});
			}
			Load(new TagCompound { ["list"] = list });
		}
	}
}
