using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terraria.ID
{
	partial class ArmorIDs
	{
		partial class Body
		{
			partial class Sets
			{
				// Created based on 'hidesTopSkin' definition in 'PlayerDrawSet.BoringSetup'.
				public static bool[] HidesTopSkin = Factory.CreateBoolSet(21, 22, 82, 83, 93);
				// Created based on 'hidesBottomSkin' definition in 'PlayerDrawSet.BoringSetup'.
				public static bool[] HidesBottomSkin = Factory.CreateBoolSet(93);
			}
		}

		partial class Legs
		{
			partial class Sets
			{
				// Created based on 'PlayerDrawLayers.ShouldOverrideLegs_CheckPants'.
				public static bool[] OverridesLegs = Factory.CreateBoolSet(67, 106, 138, 140, 143, 217, 222, 226, 228);
				public static bool[] HidesTopSkin = Factory.CreateBoolSet();
				// Created based on 'hidesBottomSkin' definition in 'PlayerDrawSet.BoringSetup'.
				public static bool[] HidesBottomSkin = Factory.CreateBoolSet(20, 21);
			}
		}

		partial class Shoe
		{
			partial class Sets
			{
				// Created based on 'PlayerDrawLayers.ShouldOverrideLegs_CheckShoes'.
				public static bool[] OverridesLegs = Factory.CreateBoolSet(13);
			}
		}
	}
}
