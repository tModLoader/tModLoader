using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace Terraria.ID;

partial class ArmorIDs
{
	partial class Head
	{
		partial class Sets
		{
			// Created based on 'fullHair' definition in 'Player.GetHairSettings'.
			/// <summary>
			/// If <see langword="true"/> for a given <see cref="Head"/>, then that equip will show the player's full hair when equipped.
			/// <br/> Defaults to <see langword="false"/>.
			/// </summary>
			public static bool[] DrawFullHair = Factory.CreateBoolSet(10, 12, 28, 42, 62, 97, 106, 113, 116, 119, 133, 138, 139, 163, 178, 181, 191, 198, 217, 218, 220, 222, 224, 225, 228, 229, 230, 232, 235, 238, 242, 243, 244, 245, 272, 273, 274, 277);

			// Created based on 'hatHair' definition in 'Player.GetHairSettings'.
			/// <summary>
			/// If <see langword="true"/> for a given <see cref="Head"/>, then that equip will show the player's hat hair when equipped.
			/// <br/> Defaults to <see langword="false"/>.
			/// </summary>
			/// <remarks>
			/// Hat hair textures can be accessed through <see cref="GameContent.TextureAssets.PlayerHairAlt"/>.
			/// </remarks>
			public static bool[] DrawHatHair = Factory.CreateBoolSet(13, 14, 15, 16, 18, 21, 24, 25, 26, 29, 40, 44, 51, 56, 59, 60, 63, 64, 65, 67, 68, 69, 81, 92, 94, 95, 100, 114, 121, 126, 130, 136, 140, 143, 145, 158, 159, 161, 182, 184, 190, 195, 215, 216, 219, 223, 226, 227, 231, 233, 234, 262, 263, 264, 265, 267, 275, 279, 280, 281);

			// Created based on 'drawsBackHairWithoutHeadgear' definition in 'Player.GetHairSettings'.
			/// <summary>
			/// If <see langword="true"/> for a given <see cref="Head"/>, then that equip will allow back hair (<see cref="HairID.Sets.DrawBackHair"/>) to draw in full when equipped.
			/// <br/> Defaults to <see langword="false"/>.
			/// </summary>
			public static bool[] DrawsBackHairWithoutHeadgear = Factory.CreateBoolSet(0, 23, 259);

			/// <summary>
			/// If <see langword="true"/> for a given <see cref="Head"/>, then that equip will allow a taller hat to render.
			/// <br/> Defaults to <see langword="false"/>.
			/// </summary>
			/// <remarks>
			/// This is used by, for example, wizard hats and gladiator helmet. Without this setting, some animation frames would render cropped at the top.
			/// </remarks>
			public static bool[] IsTallHat = Factory.CreateBoolSet(14, 56, 114, 158, 69, 180);

			/// <summary>
			/// If <see langword="true"/> for a given <see cref="Head"/>, then that equip will allow the player's head and face to be drawn.
			/// <br/> Defaults to <see langword="true"/>.
			/// </summary>
			// Created based on 'PlayerDrawLayers.DrawPlayer_21_Head_TheFace'.
			public static bool[] DrawHead = Factory.CreateBoolSet(true, 38, 135, 269);
		}
	}

	partial class Body
	{
		partial class Sets
		{
			// Created based on 'hidesTopSkin' definition in 'PlayerDrawSet.BoringSetup'.
			/// <summary>
			/// If <see langword="true"/> for a given <see cref="Body"/>, then that equip will hide the player's torso, arm, and hand skins when equipped.
			/// <br/> Defaults to <see langword="false"/>.
			/// </summary>
			/// <remarks>
			/// The <see cref="Body"/> equivalent to <see cref="Legs.Sets.HidesTopSkin"/>.
			/// </remarks>
			public static bool[] HidesTopSkin = Factory.CreateBoolSet(21, 22, 82, 83, 93);

			// Created based on 'hidesBottomSkin' definition in 'PlayerDrawSet.BoringSetup'.
			/// <summary>
			/// If <see langword="true"/> for a given <see cref="Body"/>, then that equip will hide the player's leg skin when equipped.
			/// <br/> Defaults to <see langword="false"/>.
			/// </summary>
			/// <remarks>
			/// The <see cref="Body"/> equivalent to <see cref="Legs.Sets.HidesBottomSkin"/>.
			/// </remarks>
			public static bool[] HidesBottomSkin = Factory.CreateBoolSet(93);

			// Created based on 'missingHand' definition in 'PlayerDrawSet.BoringSetup'.
			/// <summary>
			/// If <see langword="true"/> for a given <see cref="Body"/>, then that equip will hide the player's hand skins when equipped.
			/// <br/> Defaults to <see langword="true"/>.
			/// </summary>
			public static bool[] HidesHands = Factory.CreateBoolSet(true, 77, 103, 41, 100, 10, 11, 12, 13, 14, 43, 15, 16, 20, 39, 50, 38, 40, 57, 44, 52, 53, 68, 81, 85, 88, 98, 86, 87, 99, 165, 166, 167, 171, 45, 168, 169, 42, 180, 181, 183, 186, 187, 188, 64, 189, 191, 192, 198, 199, 202, 203, 58, 59, 60, 61, 62, 63, 36, 104, 184, 74, 78, 185, 196, 197, 182, 87, 76, 209, 168, 210, 211, 213);

			// Created based on 'missingArm' definition in 'PlayerDrawSet.BoringSetup'.
			/// <summary>
			/// If <see langword="true"/> for a given <see cref="Body"/>, then that equip will hide the player's arm skin when equipped.
			/// <br/> Defaults to <see langword="false"/>.
			/// </summary>
			/// <remarks>
			/// Hiding arms also hides hands (<see cref="HidesHands"/>).
			/// </remarks>
			public static bool[] HidesArms = Factory.CreateBoolSet(83);
		}
	}

	partial class Legs
	{
		partial class Sets
		{
			/// <summary>
			/// If <see langword="true"/> for a given <see cref="Legs"/>, then that equip will hide <see cref="Shoe"/> equips when equipped.
			/// <br/> Defaults to <see langword="false"/>.
			/// </summary>
			// Created based on 'PlayerDrawLayers.ShouldOverrideLegs_CheckPants'.
			public static bool[] OverridesLegs = Factory.CreateBoolSet(67, 106, 138, 140, 143, 217, 222, 226, 228);

			// Created based on 'hidesTopSkin' definition in 'PlayerDrawSet.BoringSetup'.
			/// <summary>
			/// If <see langword="true"/> for a given <see cref="Legs"/>, then that equip will hide the player's torso, arm, and hand skins when equipped.
			/// <br/> Defaults to <see langword="false"/>.
			/// </summary>
			/// <remarks>
			/// The <see cref="Legs"/> equivalent to <see cref="Body.Sets.HidesTopSkin"/>.
			/// </remarks>
			public static bool[] HidesTopSkin = Factory.CreateBoolSet();

			// Created based on 'hidesBottomSkin' definition in 'PlayerDrawSet.BoringSetup'.
			/// <summary>
			/// If <see langword="true"/> for a given <see cref="Legs"/>, then that equip will hide the player's leg skin when equipped.
			/// <br/> Defaults to <see langword="false"/>.
			/// </summary>
			/// <remarks>
			/// The <see cref="Legs"/> equivalent to <see cref="Body.Sets.HidesBottomSkin"/>.
			/// </remarks>
			public static bool[] HidesBottomSkin = Factory.CreateBoolSet(20, 21);
		}
	}

	partial class Shoe
	{
		partial class Sets
		{
			// Created based on 'PlayerDrawLayers.ShouldOverrideLegs_CheckShoes'.
			/// <summary>
			/// If <see langword="true"/> for a given <see cref="Shoe"/>, then that equip will hide the player's legs when equipped.
			/// <br/> Defaults to <see langword="false"/>.
			/// </summary>
			public static bool[] OverridesLegs = Factory.CreateBoolSet(15);
		}
	}

	partial class Wing
	{
		partial class Sets
		{
			public static SetFactory Factory = new(EquipLoader.nextEquip[EquipType.Wings]);

			// Below three sets created based on snippets from 'Player.Update'.
			/// <summary>
			/// If <see langword="true"/> for a given <see cref="Wing"/>, then the player can hover while that wing is equipped (<see cref="Player.wingsLogic"/>).
			/// <br/> Defaults to <see langword="false"/>.
			/// </summary>
			public static bool[] CanHover = Factory.CreateBoolSet(22, 28, 30, 31, 33, 35, 37, 45);

			/// <summary>
			/// If <see langword="true"/> for a given <see cref="Wing"/>, then that wing will be considered to be "in use" if the player is attempting to hover with it.
			/// <br/> If <see langword="false"/>, then that wing will not be considered "in use".
			/// <br/> If <see langword="null"/>, then that wing will be considered "in use" if it can hover (<see cref="CanHover"/>).
			/// <br/> Defaults to <see langword="null"/>.
			/// </summary>
			public static bool?[] InUseWhenAttemptingHover = Factory.CreateCustomSet<bool?>(null,
				29, true,
				31, false,
				32, true
				);

			/// <summary>
			/// The multiplier on the player's Y velocity when entering a hovering state (<see cref="CanHover"/>).
			/// <br/> Defaults to <c>0.9f</c>.
			/// </summary>
			public static float[] HoverSlowdown = Factory.CreateFloatSet(0.9f,
				45, 0.8f,
				37, 0.9f * 0.92f // For some reason, Betsy's Wings have their hovering slowdown applied twice.
				);

			// Created based on 'Player.WingMovement'.
			/// <summary>
			/// The <see cref="Player.wingTime"/> usage of a given <see cref="Wing"/> per frame while the player is attempting to hover (<see cref="Player.TryingToHoverDown"/>) and stationary (<c>!<see cref="Player.controlLeft"/> &amp;&amp; !<see cref="Player.controlRight"/></c>).
			/// <br/> A pair of wings <b>does not need to be able to hover</b> to use this set.
			/// <br/> Defaults to <c>1f</c>.
			/// </summary>
			public static float[] StationaryHoverUsage = Factory.CreateFloatSet(1f,
				22, 0.5f,
				28, 0.5f,
				30, 0.5f,
				31, 0.5f,
				37, 0.5f,
				45, 0.5f
				);
		}
	}
}
