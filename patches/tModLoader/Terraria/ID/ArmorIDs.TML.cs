using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terraria.ID
{
	partial class ArmorIDs
	{
		partial class Head
		{
			partial class Sets
			{
				// Created based on 'fullHair' definition in 'Player.GetHairSettings'.
				public static bool[] DrawFullHair = Factory.CreateBoolSet(10, 12, 28, 42, 62, 97, 106, 113, 116, 119, 133, 138, 139, 163, 178, 181, 191, 198, 217, 218, 220, 222, 224, 225, 228, 229, 230, 232, 235, 238, 242, 243, 244, 245, 272);
				// Created based on 'hatHair' definition in 'Player.GetHairSettings'.
				public static bool[] DrawHatHair = Factory.CreateBoolSet(13, 14, 15, 16, 18, 21, 24, 25, 26, 29, 40, 44, 51, 56, 59, 60, 63, 64, 65, 67, 68, 69, 81, 92, 94, 95, 100, 114, 121, 126, 130, 136, 140, 143, 145, 158, 159, 161, 182, 184, 190, 195, 215, 216, 219, 223, 226, 227, 231, 233, 234, 262, 263, 264, 265, 267);
				// Created based on 'backHairDraw' definition in 'Player.GetHairSettings'.
				/// <summary>
				/// Index using Player.hair
				/// </summary>
				public static bool[] DrawBackHair = Factory.CreateBoolSet(51, 52, 53, 54, 55, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 101, 102, 103, 105, 106, 107, 108, 109, 110, 111, 113, 114, 115);
				// Created based on 'drawsBackHairWithoutHeadgear' definition in 'Player.GetHairSettings'.
				public static bool[] DrawsBackHairWithoutHeadgear = Factory.CreateBoolSet(0, 23, 259);
				// Created based on 'PlayerDrawLayers.DrawPlayer_21_Head_TheFace'.
				public static bool[] DrawHead = Factory.CreateBoolSet(true, 38, 135, 269);
			}
		}

		partial class Body
		{
			partial class Sets
			{
				// Created based on 'hidesTopSkin' definition in 'PlayerDrawSet.BoringSetup'.
				public static bool[] HidesTopSkin = Factory.CreateBoolSet(21, 22, 82, 83, 93);
				// Created based on 'hidesBottomSkin' definition in 'PlayerDrawSet.BoringSetup'.
				public static bool[] HidesBottomSkin = Factory.CreateBoolSet(93);
				// Created based on 'missingHand' definition in 'PlayerDrawSet.BoringSetup'.
				public static bool[] DrawHand = Factory.CreateBoolSet(77, 103, 41, 100, 10, 11, 12, 13, 14, 43, 15, 16, 20, 39, 50, 38, 40, 57, 44, 52, 53, 68, 81, 85, 88, 98, 86, 87, 99, 165, 166, 167, 171, 45, 168, 169, 42, 180, 181, 183, 186, 187, 188, 64, 189, 191, 192, 198, 199, 202, 203, 58, 59, 60, 61, 62, 63, 36, 104, 184, 74, 78, 185, 196, 197, 182, 87, 76, 209, 168, 210, 211, 213);
				// Created based on 'missingArm' definition in 'PlayerDrawSet.BoringSetup'.
				public static bool[] DrawArm = Factory.CreateBoolSet(83);
				// Created based on 'PlayerDrawLayers.DrawPlayer_12_Skin_Composite'.
				public static bool[] DrawBody = Factory.CreateBoolSet(true);
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
				// Created based on 'PlayerDrawLayers.DrawPlayer_12_Skin_Composite'.
				public static bool[] DrawLegs = Factory.CreateBoolSet(true);
			}
		}

		partial class Shoe
		{
			partial class Sets
			{
				// Created based on 'PlayerDrawLayers.ShouldOverrideLegs_CheckShoes'.
				public static bool[] OverridesLegs = Factory.CreateBoolSet(13);
				// Created based on 'PlayerDrawLayers.DrawPlayer_12_Skin_Composite'.
				public static bool[] DrawShoes = Factory.CreateBoolSet(true);
			}
		}
	}
}
