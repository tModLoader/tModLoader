using ExampleMod.Content.DamageClasses;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalItems
{
	// This file shows examples of creating and setting your own sorting group in Journey mode's Duplication menu, as well as changing the sorting groups of existing items.
	// Creating your own research sorting group can be useful if your mod has a specific custom item type, or the vanilla sorting method doesn't assign the right group to your item.
	// While you can do this in ModItem, there are benefits to adding all modded items to sorting groups in bulk using GlobalItem, as shown here.
	public class ExampleResearchSorting : GlobalItem
	{
		// Here we add every weapon using the ExampleDamageClass damage class to a single custom sorting group. We also add an existing item, the copper shortsword, to a vanilla sorting group.
		// These can be interchanged, modded items can go in vanilla sorting groups and vice versa.
		public override void ModifyResearchSorting(Item item, ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
			if (item.DamageType.CountsAsClass<ExampleDamageClass>()) {
				itemGroup = (ContentSamples.CreativeHelper.ItemGroup)531; // This number is where the item sort is in relation to any other sorts added by vanilla or mods; 531 set here is in between the MeleeWeapon and RangedWeapon sorts. To know where your custom group relates to the vanilla sorting numbers, refer to the vanilla ItemGroup class, which you can easily get to by pressing F12 if using Visual Studio.
			}

			if (item.type == ItemID.CopperShortsword) {
				itemGroup = ContentSamples.CreativeHelper.ItemGroup.EventItem; // Changed the copper shortsword's default sorting to be with the event items instead of melee weapons.
				// Vanilla already has many default research sorting groups that you can add your item into. It is usually done automatically with a few exceptions. For an example of an exception, refer to the ExampleFishingCrate file.
			}

			// Modders who want to quickly check over the current item sorting group values for items in their mod can use the following code snippet, then check client.log.
			/*
			if (item.ModItem?.Mod == Mod) {
				Mod.Logger.Info($"{item.ModItem.Name}: {itemGroup}");
			}
			*/
		}
	}
}
