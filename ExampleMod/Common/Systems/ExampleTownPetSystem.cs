using System.IO;
using Terraria.ModLoader.IO;
using Terraria.ModLoader;
using Terraria;

namespace ExampleMod.Common.Systems
{
	// See ExampleMod.Common.Systems.DownedBossSystem.cs for more information about saving world data.

	public class ExampleTownPetSystem : ModSystem
	{
		/// <summary>
		/// <br>The bool for whether the Example Town Pet License has been used.</br>
		/// <br>(Doesn't really have anything to do with buying, but it is named as such to match the vanilla NPC.boughtCat, NPC.boughtDog, and NPC.boughtBunny)</br>
		/// </summary>
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