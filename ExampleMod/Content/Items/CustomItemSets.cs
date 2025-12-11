using ExampleMod.Content.Items.Consumables;
using ExampleMod.Content.Items.Weapons;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	// This class demonstrates "custom ID sets". "Custom ID sets" are arrays indexed by content ids such as those seen in all the ID classes (ItemID.Sets, NPCID.Sets, ProjectileID.Sets, etc). This allows mods to easily implement content id-specific data.
	// These custom ID sets are registered with a key, giving them a public identity and making them "named ID sets".
	// "Named ID sets" are automatically merged when other mods declare the same ID set, facilitating "ad-hoc" collaboration that doesn't require mod dependencies.
	// See https://github.com/tModLoader/tModLoader/pull/4381 for more information about custom and named ID sets.

	// This [ReinitializeDuringResizeArrays] attribute will cause this class's static constructor to be called during the ResizeArrays step of mod loading. This is essential for any class with field initializers calling SetFactory methods.
	// This will allow the arrays to have the correct lengths after all content has been loaded into the game. This reinitialization happens before ModSystem.ResizeArrays, avoiding potential issues from mod load order.
	[ReinitializeDuringResizeArrays]
	public static class CustomItemSets
	{
		// Named ID set example. This will behave the same as any other ItemID.Sets array.
		public const string FlamingWeaponCustomSetKey = "FlamingWeapon";

		// To create a named ID set for items, we use the ItemID.Sets.Factory.CreateNamedSet method and provide a string key.
		// The key we provide using this method overload will automatically have "ModName/" added to the start, meaning that the real key for this example is "ExampleMod/FlamingWeapon".
		// This is then optionally followed by the Description method. The description explains how this mod uses the set. Other mods can view this description using the /customsets chat command.
		// Finally the RegisterXSet method is called to register the set.
		// We can pass in any initial data to the RegisterXSet method, in this case we are indicating that FieryGreatsword and ExampleSword should be true in this set. Note that it is also possible to set these set values in SetStaticDefaults instead, which is more typical. See ExampleFlail.cs for an example.
		// By registering the set, other mods can access it the key. The key and default value must be consistent with other mods. Remember that the Mod name is part of the key that that other mods will be using to access this set.
		public static bool[] FlamingWeapon = ItemID.Sets.Factory.CreateNamedSet(FlamingWeaponCustomSetKey)
			.Description("Causes \"Hahahah, burn!\" to randomly show in chat when used")
			.RegisterBoolSet(false, ItemID.FieryGreatsword, ModContent.ItemType<ExampleSword>());
		// Note that by using the ReinitializeDuringResizeArrays approach, ModContent.ItemType<ExampleSword>() is a valid input since modded content IDs will be assigned and retrievable during the reinitialization. Without ReinitializeDuringResizeArrays the code may incorrectly use 0 as the value of ModContent.ItemType<ExampleSword>() because modded IDs may not have been assigned yet when the class is first initialized.

		// If sharing a custom ID set with other mods is not needed at all, the CreateXSet methods can be used to create a non-named custom ID set. Use this option for sets you are positive will never be accessed by other mods.
		// public static int[] UnsharedSetExample_SpicyLevel = ItemID.Sets.Factory.CreateIntSet(0, ItemID.PadThai, 5, ModContent.ItemType<ExampleFoodItem>(), 10);
	}

	public class CustomItemSetsSystem : ModSystem
	{
		public override void Load() {
			// The MergeNamedSets method can be used in rare situations where sets with different names need to be merged but the mods can't just release an update with the common set name.
			// ItemID.Sets.Factory.MergeNamedSets<bool>("OtherMod/FireWeapons", "ExampleMod/FlamingWeapon");
		}

		public override void ResizeArrays() {
			// ResizeArrays is the earliest method called after all content has loaded and have been assigned ID values.
			// This is where methods such as SetFactory.CreateNamedSet should be called if not using the ReinitializeDuringResizeArrays attribute to do this automatically with a field initializer.

			// For example, we could put "CustomItemSets.FlamingWeapon = ItemID.Sets.Factory.CreateNamedSet(CustomItemSets.FlamingWeaponCustomSetKey).RegisterBoolSet(false, ItemID.FieryGreatsword);" here instead of in the CustomItemSets class field initializers.
			// We could also move the FlamingWeapon field to this class if we make sure to use [ReinitializeDuringResizeArrays] and have no other static fields that we wouldn't want to reset.
		}

		public override void SetStaticDefaults() {
			// We can further edit the ID sets here. These changes will still be consistent between all mods accessing this set since the array reference is shared.
			CustomItemSets.FlamingWeapon[ItemID.FireWhip] = true;
			CustomItemSets.FlamingWeapon[ItemID.HelFire] = true;
		}
	}

	// This class showcases the actual use of our FlamingWeapon set in this mod. Each mod using a shared set might have their own interpretation of the set and their corresponding effects. Modders can use the /customsets chat command to output all the registered named ID sets and corresponding metadata, including additional info passed in by mods using each set. It is up to mod makers to collaborate to ensure that the meaning and effects of named sets are sensible.
	public class CustomSetsModPlayer : ModPlayer
	{
		public override void OnHitAnything(float x, float y, Entity victim) {
			if (CustomItemSets.FlamingWeapon[Player.HeldItem.type] && Main.rand.NextBool(100)) {
				CombatText.NewText(Player.getRect(), Color.Red, "Hahahah, burn!");
			}
		}
	}
}
