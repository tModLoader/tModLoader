namespace Terraria.ModLoader
{
	/// <summary>
	/// Class that contains data on the current values of local variables trapped within the scope
	/// of the GenerateWorld method in the WorldGen class.
	/// </summary>
	public class WorldGenLocals
	{

		public int DungeonSide {
			get;
			internal set;
		}

		public ushort JungleHutType {
			get;
			internal set;
		}

		public int LeftBeachShellStartX {
			get;
			internal set;
		}

		public int LeftBeachShellStartY {
			get;
			internal set;
		}

		public int RightBeachShellStartX {
			get;
			internal set;
		}

		public int RightBeachShellStartY {
			get;
			internal set;
		}

		public int HowFar {
			get;
			internal set;
		}

		public int[] PyramidXLocations {
			get;
			internal set;
		}

		public int[] PyramidYLocations {
			get;
			internal set;
		}

		public int NumberOfPyramids {
			get;
			internal set;
		}

		public int JungleMinX {
			get;
			internal set;
		}

		public int JungleMaxX {
			get;
			internal set;
		}

		public int[] SnowMinX {
			get;
			internal set;
		}

		public int[] SnowMaxX {
			get;
			internal set;
		}

		public int SnowTop {
			get;
			internal set;
		}

		public int SnowBottom {
			get;
			internal set;
		}

		public int SkyLakeCount {
			get;
			internal set;
		}

		public int LeftBeachEnd {
			get;
			internal set;
		}

		public int RightBeachStart {
			get;
			internal set;
		}

		public int JungleOriginX {
			get;
			internal set;
		}

		public int SnowOriginLeft {
			get;
			internal set;
		}

		public int SnowOriginRight {
			get;
			internal set;
		}

		public int FallenLogX {
			get;
			internal set;
		}

		public int FallenLogY {
			get;
			internal set;
		}

		public int DungeonLocation {
			get;
			internal set;
		}
	}
}
