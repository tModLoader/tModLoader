using Terraria.ModLoader;

namespace Terraria.ID
{
	public partial class NPCID
	{
		public static partial class Sets
		{
			//IDs taken from start of NPC.NewNPC when determining num
			/// <summary>
			/// Whether or not the spawned NPC will start looking for a suitable slot from the end of <seealso cref="Main.npc"/>, ignoring the Start parameter of <see cref="NPC.NewNPC"/>.
			/// Useful if you have a multi-segmented boss and want its parts to draw over the main body (body will be in this set).
			/// </summary>
			public static bool[] SpawnFromLastEmptySlot = Factory.CreateBoolSet(222, 245);

			//Default ID is the skeleton merchant
			/// <summary>
			/// Whether or not a given NPC will act like a town NPC in terms of AI, animations, and attacks, but not in other regards, such as having a happiness button or appearing
			/// on the minimap, like the bone merchant in vanilla.
			/// </summary>
			public static bool[] ActsLikeTownNPC = Factory.CreateBoolSet(453);

			//Default ID is the skeleton merchant
			/// <summary>
			/// Whether or not a given NPC will spawn with a custom name like a town NPC. In order to determine what name will be selected, override the TownNPCName hook.
			/// True will force a name to be rolled regardless of vanilla behavior. False will have vanilla handle the naming.
			/// </summary>
			public static bool[] SpawnsWithCustomName = Factory.CreateBoolSet(453);

			// IDs taken from NPC.AI_007_TryForcingSitting
			/// <summary>
			/// Whether or not a given NPC can sit on suitable furniture (<see cref="TileID.Sets.CanBeSatOnForNPCs"/>)
			/// </summary>
			public static bool[] CannotSitOnFurniture = Factory.CreateBoolSet(638, 656);

			// IDs taken from Conditions.SoulOfWhateverConditionCanDrop
			/// <summary>
			/// Whether or not a given NPC is excluded from dropping hardmode souls (Soul of Night/Light)
			/// <br/>Contains vanilla NPCs that are easy to spawn in large numbers, preventing easy soul farming
			/// <br/>Do not add your NPC to this if it would be excluded automatically (i.e. critter, town NPC, or no coin drops)
			/// </summary>
			public static bool[] CannotDropSouls = Factory.CreateBoolSet(1, 13, 14, 15, 121, 535);

			//No Default IDs, as there is no vanilla precedent for this functionality
			/// <summary>
			/// Whether or not this NPC can still interact with doors if they use the Vanilla TownNPC aiStyle (AKA aiStyle == 7)
			/// but are not actually marked as Town NPCs (AKA npc.townNPC == true).
			/// </summary>
			/// <remarks>
			/// Note: This set DOES NOT DO ANYTHING if your NPC doesn't use the Vanilla TownNPC aiStyle (aiStyle == 7).
			/// </remarks>
			public static bool[] AllowDoorInteraction = Factory.CreateBoolSet();
		}
	}
}
