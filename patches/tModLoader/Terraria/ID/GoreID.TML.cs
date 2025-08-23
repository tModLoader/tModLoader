namespace Terraria.ID;

public partial class GoreID
{
	public static partial class Sets
	{
		// TML: Definition from 'Main.DrawGore()' and 'Main.DrawGoreBehind()'.
		/// <summary>
		/// If <see langword="true"/> for a given gore type (<see cref="Gore.type"/>), then that gore draws behind tiles.
		/// <br/> Defaults to <see langword="false"/>.
		/// </summary>
		public static bool[] DrawBehind = Factory.CreateBoolSet();

		/// <summary>
		/// If <see langword="true"/>, this gore will behave as a dripping liquid droplet. Useful for gore used in <see cref="ModLoader.ModWaterStyle.GetDropletGore"/>.
		/// <br/> Defaults to <see langword="false"/>.
		/// </summary>
		public static bool[] LiquidDroplet = Factory.CreateBoolSet(706, 707, 708, 709, 710, 711, 712, 713, 714, 715, 716, 717, 943, 1147, 1160, 1161, 1162);

		// TML: Definition from GoreID.Sets.SpecialAI[Type] == 3 check in 'Gore.NewGore'
		/// <summary>
		/// If <see langword="true"/> for a given gore type (<see cref="Gore.type"/>), then that gore has columns in its sprite corresponding to the amount of tile paints, and Gore.Frame.CurrentColumn will be set to tile.TileColor on spawn if it's coming from a tree via wind effects.
		/// <br/> Defaults to <see langword="false"/>.
		/// </summary>
		public static bool[] PaintedFallingLeaf = Factory.CreateBoolSet(910, 911, 912, 913, 914, 915, 916, 917, 918, 919, 920, 921, 922, 923, 924, 925, 1113, 1114, 1115, 1116, 1117, 1118, 1119, 1120, 1121, 1248, 1249, 1250, 1251, 1252, 1253, 1254, 1255, 1257, 1278);
	}
}