using Terraria.WorldBuilding;

namespace Terraria
{
	public partial class WorldGen
	{
		#region WorldGen.GenerateWorld Values
		public static WorldGenConfiguration configuration;
		public static int copper;
		public static int iron;
		public static int silver;
		public static int gold;
		public static int dungeonSide;
		public static ushort jungleHut;
		public static int shellStartXLeft;
		public static int shellStartYLeft;
		public static int shellStartXRight;
		public static int shellStartYRight;
		public static int howFar;
		public static int[] PyrX;
		public static int[] PyrY;
		public static int numPyr;
		public static int jungleMinX;
		public static int jungleMaxX;
		public static int[] snowMinX;
		public static int[] snowMaxX;
		public static int snowTop;
		public static int snowBottom;
		public static float dub2 = 0f;
		public static int skyLakes;
		public static int beachBordersWidth;
		public static int beachSandRandomCenter;
		public static int beachSandRandomWidthRange;
		public static int beachSandDungeonExtraWidth;
		public static int beachSandJungleExtraWidth;
		public static int oceanWaterStartRandomMin;
		public static int oceanWaterStartRandomMax;
		public static int oceanWaterForcedJungleLength;
		public static int leftBeachEnd;
		public static int rightBeachStart;
		public static int minSsandBeforeWater;
		public static int evilBiomeBeachAvoidance;
		public static int evilBiomeAvoidanceMidFixer;
		public static int lakesBeachAvoidance;
		public static int smallHolesBeachAvoidance;
		public static int surfaceCavesBeachAvoidance2;
		public static int jungleOriginX;
		public static int snowOriginLeft;
		public static int snowOriginRight;
		public static int logX;
		public static int logY;
		public static int dungeonLocation;
		#endregion

		internal static void ClearGenerationPasses() => _generator?._passes.Clear();
	}
}
