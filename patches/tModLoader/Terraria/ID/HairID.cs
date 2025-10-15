using ReLogic.Reflection;
using Terraria.ModLoader;

namespace Terraria.ID;
public class HairID
{
	public class Sets
	{
		public static SetFactory Factory = new SetFactory(HairLoader.Count, nameof(HairID), Search);

		// Created based on 'backHairDraw' definition in 'Player.GetHairSettings'.
		/// <summary>
		/// If <see langword="true"/> for a given <strong><see cref="Player.hair"/></strong> value, then that hair will additionally draw behind the player's back using <see cref="DataStructures.PlayerDrawSet.hairBackFrame"/>. Set this to prevent long hair from drawing in front of capes.
		/// <br/> Defaults to <see langword="false"/>.
		/// </summary>
		/// <remarks>
		/// Back hair is drawn using <see cref="DataStructures.PlayerDrawLayers.HairBack"/>.
		/// </remarks>
		public static bool[] DrawBackHair = Factory.CreateBoolSet(51, 52, 53, 54, 55, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 101, 102, 103, 105, 106, 107, 108, 109, 110, 111, 113, 114, 115, 133, 134, 146, 162, 6);
	}

	public static readonly int Count = 165;

	public static IdDictionary Search = IdDictionary.Create<HairID, int>();
}
