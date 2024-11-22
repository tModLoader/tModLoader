using ExampleMod.Common.Configs.ModConfigShowcases;
using ExampleMod.Content.Items.Weapons;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	// This class demonstrates "custom data sets". Custom data sets are arrays indexed by content ids such as those seen in all the ID classes (ItemID.Sets, NPCID.Sets, ProjectileID.Sets, etc). This allows mods to easily implement content id-specific data.
	// Custom data sets are automatically merged when other mods declare the same data set, facilitating "ad-hoc" collaboration that doesn't require mod dependencies.

	// This [ReinitializeDuringResizeArrays] attribute will cause this class's static constructor to be called during the ResizeArrays step of mod loading. This is essential for any class with field initializers calling SetFactory.CreateNamedXSet methods.
	// This will allow the arrays to have the correct lengths after all content has been loaded into the game. This reinitialization happens before ModSystem.ResizeArrays, avoiding potential issues from mod load order.
	[ReinitializeDuringResizeArrays]
	public static class CustomItemSets
	{
		// Custom item set example. This will behave the same as any other ItemID.Sets array.
		public const string FlamingWeaponCustomSetKey = "FlamingWeapon"; // This string MUST be consistent between mods working together.

		// To create a custom item set, we use the ItemID.Sets.Factory.CreateNamedXSet method and provide a string key.
		// We can also pass in any initial data, in this case we are indicating that FieryGreatsword and ExampleSword should be true in this set. Note that it is also possible to set these set values in SetStaticDefaults instead, which is more typical. See ExampleFlail.cs for an example.
		// This method also exposes the set for other mods to access via this key. The key and default value must be consistent with other mods.
		public static bool[] FlamingWeapon = ItemID.Sets.Factory.CreateNamedBoolSet(FlamingWeaponCustomSetKey, false, ItemID.FieryGreatsword, ModContent.ItemType<ExampleSword>());
		// Note that by using the ReinitializeDuringResizeArrays approach, ModContent.ItemType<ExampleSword>() is a valid input since modded content IDs will be assigned and retrievable during the reinitialization. Without ReinitializeDuringResizeArrays the code will incorrectly use 0 as the value of ModContent.ItemType<ExampleSword>() because modded IDs haven't been assigned yet.

		// If a set is specific to content in your mod, consider using the CreateNamedBoolSet(string modName, string key, ...) overload instead to avoid any potential conflicts with another mod that happens to use that same key for unrelated purposes.
	}

	public class CustomItemSetsSystem : ModSystem
	{
		public override void ResizeArrays() {
			// ResizeArrays is the earliest method called after all content has loaded and have been assigned ID values.
			// This is where methods such as SetFactory.CreateNamedBoolSet should be called if not using the ReinitializeDuringResizeArrays attribute to do this automatically with a field initializer.

			// For example, we could put "CustomItemSets.FlamingWeapon = ItemID.Sets.Factory.CreateNamedBoolSet(FlamingWeaponCustomSetKey, false, ItemID.FieryGreatsword);" here instead of in the CustomItemSets class field initializers.
		}

		public override void SetStaticDefaults() {
			// We can further edit the data sets here. These changes will still be consistent between all mods accessing this set since the object reference is shared.
			CustomItemSets.FlamingWeapon[ItemID.FireWhip] = true;
			CustomItemSets.FlamingWeapon[ItemID.HelFire] = true;

			// For example, we can add user-defined items to CustomItemSets.FlamingWeapon
			// Note that this example would need more code to work correctly for config values changed in-game, this is just showing a potential use-case.
			foreach (var itemDefinition in ModContent.GetInstance<ModConfigShowcaseDefaultValues>().ListOfItemDefinition) {
				if (itemDefinition.Type != -1) {
					CustomItemSets.FlamingWeapon[itemDefinition.Type] = true;
				}
			}
		}
	}

	// This class showcases the actual use of our FlamingWeapon set in this mod. Each mod using a shared set might have their own interpretation of the set and their corresponding effects. 
	public class CustomSetsModPlayer : ModPlayer
	{
		public override void OnHitAnything(float x, float y, Entity victim) {
			if (CustomItemSets.FlamingWeapon[Player.HeldItem.type] && Main.rand.NextBool(100)) {
				CombatText.NewText(Player.getRect(), Color.Red, "Hahahah, burn!");
			}
		}
	}
}
