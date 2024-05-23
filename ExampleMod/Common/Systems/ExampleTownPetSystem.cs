using System.IO;
using Terraria.ModLoader.IO;
using Terraria.ModLoader;
using Terraria;

namespace ExampleMod.Common.Systems
{
	public class ExampleTownPetSystem : ModSystem
	{
		public static bool boughtExampleTownPet = false;

		public override void SaveWorldData(TagCompound tag) {
			if (boughtExampleTownPet) {
				tag["boughtExampleTownPet"] = true;
			}
		}

		public override void LoadWorldData(TagCompound tag) {
			boughtExampleTownPet = tag.ContainsKey("boughtExampleTownPet");
		}

		public override void NetSend(BinaryWriter writer) {
			BitsByte flags = new BitsByte();
			flags[0] = boughtExampleTownPet;
			writer.Write(flags);
		}

		public override void NetReceive(BinaryReader reader) {
			BitsByte flags = reader.ReadByte();
			boughtExampleTownPet = flags[0];
		}
	}
}