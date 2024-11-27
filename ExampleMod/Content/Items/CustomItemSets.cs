using ExampleMod.Common.Configs;
using ExampleMod.Content.Items.Accessories;
using ExampleMod.Content.Items.Weapons;
using Microsoft.Xna.Framework;
using System.Linq;
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
		public const string FlamingWeaponCustomSetKey = "FlamingWeapon"; // This string MUST be consistent between mods working together.

		// To create a named ID set for items, we use the ItemID.Sets.Factory.CreateNamedXSet method and provide a string key.
		// We can also pass in any initial data, in this case we are indicating that FieryGreatsword and ExampleSword should be true in this set. Note that it is also possible to set these set values in SetStaticDefaults instead, which is more typical. See ExampleFlail.cs for an example.
		// This method also exposes the set for other mods to access via this key. The key and default value must be consistent with other mods.
		public static bool[] FlamingWeapon = ItemID.Sets.Factory.CreateNamedBoolSet(FlamingWeaponCustomSetKey, false, ItemID.FieryGreatsword, ModContent.ItemType<ExampleSword>());
		// Note that by using the ReinitializeDuringResizeArrays approach, ModContent.ItemType<ExampleSword>() is a valid input since modded content IDs will be assigned and retrievable during the reinitialization. Without ReinitializeDuringResizeArrays the code will incorrectly use 0 as the value of ModContent.ItemType<ExampleSword>() because modded IDs haven't been assigned yet.

		// If a named ID set is specific to content in your mod, consider using the CreateNamedBoolSet(string modName, string key, ...) overload instead to avoid any potential conflicts with another mod that happens to use that same key for unrelated purposes.
		// If sharing a custom ID set with other mods is not needed at all, the CreateXSet methods can be used to create a non-named custom ID set.
	}

	public class CustomItemSetsSystem : ModSystem
	{
		public override void ResizeArrays() {
			// ResizeArrays is the earliest method called after all content has loaded and have been assigned ID values.
			// This is where methods such as SetFactory.CreateNamedBoolSet should be called if not using the ReinitializeDuringResizeArrays attribute to do this automatically with a field initializer.

			// For example, we could put "CustomItemSets.FlamingWeapon = ItemID.Sets.Factory.CreateNamedBoolSet(FlamingWeaponCustomSetKey, false, ItemID.FieryGreatsword);" here instead of in the CustomItemSets class field initializers.
		}

		public override void SetStaticDefaults() {
			// We can further edit the ID sets here. These changes will still be consistent between all mods accessing this set since the array reference is shared.
			CustomItemSets.FlamingWeapon[ItemID.FireWhip] = true;
			CustomItemSets.FlamingWeapon[ItemID.HelFire] = true;
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

	// This command helps visualize the custom ID set data to verify its contents.
	// Type /customsets in chat to view the contents of these sets
	public class CustomSetsCommand : ModCommand
	{
		public override string Command => "customsets";

		public override string Description => "View custom ID set values, see CustomItemSets.cs";

		public override CommandType Type => CommandType.Chat;

		public override void Action(CommandCaller caller, string input, string[] args) {
			caller.Reply("True values in FlamingWeapon: " + string.Join(", ", CustomItemSets.FlamingWeapon.GetTrueIndexes().Select(ItemID.Search.GetName)));
			// Or, to see display names instead of internal names:
			// caller.Reply("True values in FlamingWeapon: " + string.Join(", ", CustomItemSets.FlamingWeapon.GetTrueIndexes().Select(Lang.GetItemNameValue)));

			caller.Reply("True values in CantEquipWith_HiveBackpack: " + string.Join(", ", WaspNestGlobalItem.CantEquipWith_HiveBackpack.GetTrueIndexes().Select(ItemID.Search.GetName)));
		}
	}
}
