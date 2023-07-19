using ExampleMod.Common.Commands;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalNPCs
{
	public class ExampleShimmerGlobalNPC : GlobalNPC
	{
		public override void SetStaticDefaults() {
			(ModShimmerTypeID, int)[] transformationFor = new (ModShimmerTypeID, int)[] {
				(ModShimmerTypeID.NPC, NPCID.Goldfish),
				(ModShimmerTypeID.NPC, NPCID.Crab),
				(ModShimmerTypeID.NPC, NPCID.Penguin),
				(ModShimmerTypeID.NPC, NPCID.PartyBunny),
			};

			new ModShimmer() // Since we're not in ModNPC we instantiate manually
				.AddNPCResult(NPCID.Frog, 1) // Vanilla frog
				.AddCondition(Condition.InBeach) // On the beach
				.Register(transformationFor); // Registers for every entity passed within the array

			NPCID.Sets.IgnoreNPCSpawnedFromStatue[NPCID.Goldfish] = true; // The goldfish statue spawns can undergo any shimmer operation
		}

		public override void OnShimmer(NPC npc) {
		}

		public override bool CanShimmer(NPC npc) {
			return !npc.TypeName.Contains("Slime"); // Prevents any entity with "Slime" in its name from shimmering
		}
	}
}