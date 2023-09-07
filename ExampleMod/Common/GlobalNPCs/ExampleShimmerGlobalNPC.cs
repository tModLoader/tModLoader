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

			new ShimmerTransformation() // Since we're not in ModNPC we instantiate manually
				.AddNPCResult(NPCID.Frog, 1) // Vanilla frog
				.AddCondition(Condition.InBeach) // On the beach
				.Register(transformationFor); // Registers for every entity passed within the array

			NPCID.Sets.ShimmerIgnoreNPCSpawnedFromStatue[NPCID.Goldfish] = true; // The goldfish statue spawns can undergo any shimmer operation
		}

		public override void OnShimmer(NPC npc) {
			if (Main.rand.NextBool(100)) { // One in every 100 shimmer operations we randomly spawn a bee hat
				int itemIndex = Item.NewItem(npc.GetSource_Misc("shimmer"), npc.Center, ItemID.BeeHat, 1);
				if (Main.netMode == NetmodeID.MultiplayerClient)
					NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemIndex, 1f);
			}
		}

		public override bool CanShimmer(NPC npc) {
			return !npc.TypeName.Contains("Slime"); // Prevents any entity with "Slime" in its name from shimmering
		}
	}
}