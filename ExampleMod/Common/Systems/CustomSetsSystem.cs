using ExampleMod.Content.DamageClasses;
using ExampleMod.Content.Items;
using ExampleMod.Content.Items.Accessories;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.Systems
{
	/// <summary>
	/// This class shows off usage of DataInstance and SetFactory classes. These classes facilitate ad-hoc collaboration through shared data instances. What this means is rather than including direct dependencies on mods, mods can collaborate with any mod (mods they might not even be aware of) as long as they use data with names/identifiers agreed upon by the community. Whether there is just 1 mod or many mods attempting to utilize this data, all mods will share access to the same data instance.
	/// Pay special attention to how the code is laid out for the usage of SetFactory and data arrays that depend on content counts (similar to the arrays in ItemID.Sets, for example). The code for these items must be written in special ways so that they are initialized to have the correct length. This is necessary due to how mods and their content are loaded in order.
	/// The examples here are contrived examples meant to illustrate correct usage and other notes or warnings. See WaspNest.cs to see how CantEquipWith_HiveBackpack is used in the contained classes for a real example.
	/// See the notes in https://github.com/tModLoader/tModLoader/pull/4381 to see what the code in the other mod would look like for accessing the data instances shared by these approaches.
	/// As a brief reminder, this is the basics of how mods and their content are loaded:
	///	   1. The Load method is called on all classes, and content IDs are assigned
	///	   2. Data structures are resized once all content has been assigned an ID
	///	   3. SetupContent/SetStaticDefaults is called on all content. This is where content updates data arrays like ItemID.Sets.IsFood or Main.tileFrameImportant
	/// </summary>
	public class CustomSetsSystem : ModSystem
	{
		// We use an inner class here to contain the data arrays and SetFactory instances. The [ReinitializeDuringResizeArrays] attribute will cause this class's static constructor to be called during the ResizeArrays step of mod loading. This will allow the arrays to have the correct lengths after all content has been loaded into the game. This reinitialization happens before ModSystem.ResizeArrays, avoiding potential issues from mod load order.
		[ReinitializeDuringResizeArrays]
		public class Sets : ILoadable
		{
			// Custom item set example. This will behave the same as any other ItemID.Sets array.
			public const string FlamingWeaponCustomSetKey = "FlamingWeapon"; // This string MUST be consistent between mods working together.

			// To create a custom item set, we use the ItemID.Sets.Factory.CreateNamedXSet method and provide a string key. We can also pass in any initial data, in this case we are indicating that FieryGreatsword should be true in this set.
			// This method also exposes the set for other mods to access via this key. The key and default value must be consistent with other mods.
			public static bool[] FlamingWeapon = ItemID.Sets.Factory.CreateNamedBoolSet(FlamingWeaponCustomSetKey, false, ItemID.FieryGreatsword);
			// Note that using this approach, ModContent.ItemType<ExampleItem>() is also a valid input since modded content IDs will be assigned and retrievable during the reinitialization.
			// If a set is specific to content in your mod, consider using the CreateNamedBoolSet(string modName, string key, ...) overload instead to avoid any potential conflicts with another mod that happens to use that same key for unrelated purposes.

			// Custom SetFactory example. This example imagines a new type of custom content added by this mod and an associated data array. Any mod accessing BookFactory during or after ResizeArrays will be able to call CreateNamedXSet and generate an array of the correct length taking into account all loaded Book content, regardless of if the mod loads before or after this mod.
			public static SetFactory BookFactory = new SetFactory(Books.Count);
			public static bool[] PictureBooks = BookFactory.CreateNamedBoolSet(FlamingWeaponCustomSetKey, false);

			public void Load(Mod mod) {
			}

			public void Unload() {
			}
		}

		// Arbitrary data examples:
		// Mods can achieve ad-hoc collaboration thorough simple name-object pairings. Or, simple collaboration with a dependency can be done without writing Mod.Call code. 

		// This data is "owned" by this mod. This is shared with other mods that might want to use the data.
		// Data structures holding data corresponding to custom content loaded by this mod should not be in a class with ReinitializeDuringResizeArrays annotating it, otherwise the data would be lost halfway through mod loading. 
		private static List<string> Books = ["Test1", "Test2"];
		
		// This data is shared with other mods in an ad-hoc manner, for all mods to freely use or modify.
		public static HashSet<string> BannedWords = new();

		// This data is retrieved from another mod.
		public static List<DamageClass> DataFromOtherMod;

		public override void Load() {
			// Exposing data owned by this mod needs to happen in Load so that other mods can retrieve the data in later methods, like SetStaticDefaults
			DataInstance<List<string>>.Expose(Mod, "Books", Books);
			Books.Add("Test3");

			// This data is not intended to be "owned" by this mod, so we use GetOrAdd to register this data or retrieve the existing instance of this data registered by a mod that loaded before this mod.
			DataInstance<HashSet<string>>.GetOrAdd("BannedWords", ref BannedWords);
			// Also note that we only edit the data after GetOrAdd, otherwise there is a potential for edits to be lost.
			BannedWords.Add("yolo");
		}

		public override void ResizeArrays() {
			// ResizeArrays is the earliest method called after all content has loaded and has been assigned ID values.
			// This is where methods such as SetFactory.CreateNamedBoolSet should be called if not using the ReinitializeDuringResizeArrays attribute to do this automatically with a field initializer.

			// For example, we could put "FlamingWeapon = ItemID.Sets.Factory.CreateNamedBoolSet(FlamingWeaponCustomSetKey, false, ItemID.FieryGreatsword);" here instead of in the Sets inner class. Note that creating a custom SetFactory in ResizeArrays is not recommended due to load order complications.

			// We can further edit the set here or in SetStaticDefaults. These changes will be consistent between all mods accessing this set since the object reference is shared.
			Sets.FlamingWeapon[ItemID.FireWhip] = true;
			Sets.FlamingWeapon[ItemID.HelFire] = true;
		}

		public override void SetStaticDefaults() {
			// SetStaticDefaults is an appropriate place to retrieve data exposed by other mods.
			// This example retrieves the "SpecialDamageClasses" data from the "CustomSetTest1" example mod mentioned in the notes of https://github.com/tModLoader/tModLoader/pull/4381, if that mod happens to be loaded.
			DataFromOtherMod = DataInstance<List<DamageClass>>.Retrieve("CustomSetTest1", "SpecialDamageClasses");
			if (DataFromOtherMod != null) {
				DataFromOtherMod.Add(ModContent.GetInstance<ExampleDamageClass>());
			}
		}
	}

	public class CustomSetsModPlayer : ModPlayer {
		public override void OnHitAnything(float x, float y, Entity victim) {
			if(CustomSetsSystem.Sets.FlamingWeapon[Player.HeldItem.type] && Main.rand.NextBool(100)) {
				CombatText.NewText(Player.getRect(), Color.Red, "Hahahah, burn!");
			}
		}
	}

	public class CustomSetsCommand : ModCommand
	{
		public override string Command => "customsets";

		public override CommandType Type => CommandType.Chat;

		public override void Action(CommandCaller caller, string input, string[] args) {
			caller.Reply("True values in FlamingWeapon: " + string.Join(", ", CustomSetsSystem.Sets.FlamingWeapon.GetTrueIndexes().Select(ItemID.Search.GetName)));

			caller.Reply("True values in CantEquipWith_HiveBackpack: " + string.Join(", ", WaspNestSystem.CantEquipWith_HiveBackpack.GetTrueIndexes().Select(ItemID.Search.GetName)));

			if (CustomSetsSystem.DataFromOtherMod != null) {
				caller.Reply("DataFromOtherMod: " + string.Join(", " , CustomSetsSystem.DataFromOtherMod.Select(x => x.DisplayName)));
			}

			caller.Reply("BannedWords: " + string.Join(", ", CustomSetsSystem.BannedWords));
		}
	}
}
