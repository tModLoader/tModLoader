using System;
using System.Linq;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader.Core;

namespace Terraria.ModLoader;

public static class HairLoader
{
	public static int Count => HairID.Count + hairs.Count;

	internal static readonly IList<ModHair> hairs = new List<ModHair>();

	internal static int Register(ModHair hair)
	{
		hairs.Add(hair);
		return Count - 1;
	}

	public static ModHair GetHair(int type)
	{
		return type >= HairID.Count && type < Count ? hairs[type - HairID.Count] : null;
	}

	public static void UpdateUnlocks(HairstyleUnlocksHelper unlocksHelper, bool inCharacterCreation)
	{
		foreach (ModHair hair in hairs) {
			if (inCharacterCreation ? hair.AvailableDuringCharacterCreation : hair.GetUnlockConditions().All(x => x.IsMet()))
				unlocksHelper.AvailableHairstyles.Add(hair.Type);
		}
	}

	internal static void ResizeArrays()
	{
		Array.Resize(ref TextureAssets.PlayerHair, Count);
		Array.Resize(ref TextureAssets.PlayerHairAlt, Count);

		LoaderUtils.ResetStaticMembers(typeof(HairID));
	}

	internal static void Unload()
	{
		hairs.Clear();
	}
}