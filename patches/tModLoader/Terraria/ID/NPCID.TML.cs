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
		}
	}
}
