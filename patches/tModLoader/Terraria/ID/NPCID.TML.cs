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

			//Default ID is vanilla slimes
			/// <summary>
			/// Whether or not a given NPC is considered a slime.
			/// </summary>
			public static bool[] IsSlime = Factory.CreateBoolSet(1, 147, 537, 184, 204, 16, 59, 71, 667, 50, 535, 225, 302, 333, 334, 335, 336, 141, 81, 121, 183, 122, 138, 244, 657, 658, 659, 660, 304);

			//Default ID is vanilla bunnies
			/// <summary>
			/// Whether or not a given NPC is considered a bunny.
			/// </summary>
			public static bool[] IsBunny = Factory.CreateBoolSet(46, 443, 646, 647, 648, 649, 650, 651, 652, 614, 303, 337, 47, 464, 540);

			//Default ID is vanilla squirrels
			/// <summary>
			/// Whether or not a given NPC is considered a squirrel.
			/// </summary>
			public static bool[] IsSquirrel = Factory.CreateBoolSet(299, 538, 539, 639, 640, 641, 642, 643, 644, 645);

			//Default ID is vanilla butterflies
			/// <summary>
			/// Whether or not a given NPC is considered a butterfly.
			/// </summary>
			public static bool[] IsButterfly = Factory.CreateBoolSet(356, 444, 653, 661);

			//Default ID is vanilla birds
			/// <summary>
			/// Whether or not a given NPC is considered a bird.
			/// </summary>
			public static bool[] IsBird = Factory.CreateBoolSet(74, 297, 298, 442);
		}
	}
}
