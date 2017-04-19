using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader.Exceptions;

namespace Terraria.ModLoader
{
	internal static class RecipeGroupHelper
	{
		internal static void ResetRecipeGroups()
		{
			RecipeGroup.recipeGroups.Clear();
			RecipeGroup.recipeGroupIDs.Clear();
			RecipeGroup.nextRecipeGroupIndex = 0;
		}

		internal static void AddOldVanillaGroups()
		{
			RecipeGroup rec = new RecipeGroup(() => Lang.misc[37] + " " + Main.itemName[9], new int[]
				{
					ItemID.Wood,
					ItemID.Ebonwood,
					ItemID.RichMahogany,
					ItemID.Pearlwood,
					ItemID.Shadewood,
					ItemID.SpookyWood,
					ItemID.BorealWood,
					ItemID.PalmWood
				});
			RecipeGroupID.Wood = RecipeGroup.RegisterGroup("Wood", rec);
			rec = new RecipeGroup(() => Lang.misc[37] + " " + Main.itemName[22], new int[]
				{
					ItemID.IronBar,
					ItemID.LeadBar
				});
			RecipeGroupID.IronBar = RecipeGroup.RegisterGroup("IronBar", rec);
			rec = new RecipeGroup(() => Lang.misc[37] + " " + Lang.misc[38], new int[]
				{
					ItemID.RedPressurePlate,
					ItemID.GreenPressurePlate,
					ItemID.GrayPressurePlate,
					ItemID.BrownPressurePlate,
					ItemID.BluePressurePlate,
					ItemID.YellowPressurePlate,
					ItemID.LihzahrdPressurePlate
				});
			RecipeGroupID.PressurePlate = RecipeGroup.RegisterGroup("PresurePlate", rec);
			rec = new RecipeGroup(() => Lang.misc[37] + " " + Main.itemName[169], new int[]
				{
					ItemID.SandBlock,
					ItemID.PearlsandBlock,
					ItemID.CrimsandBlock,
					ItemID.EbonsandBlock,
					ItemID.HardenedSand
				});
			RecipeGroupID.Sand = RecipeGroup.RegisterGroup("Sand", rec);
			rec = new RecipeGroup(() => Lang.misc[37] + " " + Lang.misc[51], new int[]
				{
					ItemID.FragmentSolar,
					ItemID.FragmentVortex,
					ItemID.FragmentNebula,
					ItemID.FragmentStardust
				});
			RecipeGroupID.Fragment = RecipeGroup.RegisterGroup("Fragment", rec);
		}

		internal static void AddRecipeGroups()
		{
			foreach (Mod mod in ModLoader.mods.Values)
			{
				try
				{
					mod.AddRecipeGroups();
				}
				catch (Exception e)
				{
					ModLoader.DisableMod(mod.File);
					throw new AddRecipesException(mod, "An error occured in adding recipe groups for " + mod.Name, e);
				}
			}
			FixRecipeGroupLookups();
		}

		internal static void FixRecipeGroupLookups()
		{
			for (int k = 0; k < RecipeGroup.nextRecipeGroupIndex; k++)
			{
				RecipeGroup rec = RecipeGroup.recipeGroups[k];
				rec.ValidItemsLookup = new bool[ItemLoader.ItemCount];
				foreach (int type in rec.ValidItems)
				{
					rec.ValidItemsLookup[type] = true;
				}
			}
		}
	}
}
