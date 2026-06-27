using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ExampleMod.Common.Systems
{
	// See ExampleMod/Common/Systems/DownedBossSystem.cs for more information about saving world data.
	public class ExampleTownPetSystem : ModSystem
	{
		/// <summary>
		/// The bool for whether the Example Town Pet License has been used.
		/// <para/> (Doesn't really have anything to do with buying, but it is named as such to match the vanilla NPC.boughtCat, NPC.boughtDog, and NPC.boughtBunny)
		/// </summary>
		public static bool boughtExampleTownPet = false;

		public override void ClearWorld() {
			boughtExampleTownPet = false;
		}

		public override void SaveWorldData(TagCompound tag) {
			if (boughtExampleTownPet) {
				tag["boughtExampleTownPet"] = true;
			}
		}

		public override void LoadWorldData(TagCompound tag) {
			boughtExampleTownPet = tag.ContainsKey("boughtExampleTownPet");
		}

		public override void NetSend(BinaryWriter writer) {
			writer.WriteFlags(boughtExampleTownPet);
		}

		public override void NetReceive(BinaryReader reader) {
			reader.ReadFlags(out boughtExampleTownPet);
		}
	}
}