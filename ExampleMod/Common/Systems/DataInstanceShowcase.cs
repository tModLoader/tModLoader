using ExampleMod.Content.DamageClasses;
using ExampleMod.Content.Items;
using ExampleMod.Content.Items.Accessories;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.Systems
{
	/// <summary>
	/// This class showcases usage of the DataInstance class. DataInstance facilitate "ad-hoc" collaboration through shared data instances. This is similar to the "custom data sets" feature but can be used for any data type.
	/// What this means is rather than including direct dependencies on mods, mods can collaborate with any mod (mods they might not even be aware of) as long as they use data with names/identifiers agreed upon by the community. Whether there is just one mod or many mods attempting to utilize this data, all mods will share access to the same data instance.
	/// See the https://github.com/tModLoader/tModLoader/wiki/Custom-Sets-and-DataInstance wiki page for some usages of this feature from other mods.
	/// The examples here are contrived examples meant to illustrate correct usage and other notes or warnings.
	/// See the notes in https://github.com/tModLoader/tModLoader/pull/4381 to see what the code in the other mod would look like for accessing the data instances shared by these approaches.
	/// As a brief reminder, this is the basics of how mods and their content are loaded:
	///	   1. The Load method is called on all classes, and content IDs are assigned
	///	   2. Data structures are resized once all content has been assigned an ID
	///	   3. SetupContent/SetStaticDefaults is called on all content. This is where content updates data arrays like ItemID.Sets.IsFood or Main.tileFrameImportant
	/// </summary>
	public class DataInstanceShowcase : ModSystem
	{
		// Arbitrary data examples:
		// Mods can achieve ad-hoc collaboration thorough simple name-object pairings. Or, simple collaboration with a dependency can be done without writing Mod.Call code. 

		// This data is "owned" by this mod. This is shared with other mods that might want to use the data.
		private static List<string> Books = ["Test1", "Test2"];
		
		// This data is shared with other mods in an ad-hoc manner, for all mods to freely use or modify.
		public static HashSet<string> BannedWords = new();

		// This data is retrieved from another mod.
		public static List<DamageClass> DataFromOtherMod;

		public override void Load() {
			// Exposing data owned by this mod needs to happen in Load so that other mods can retrieve the data in later methods, like SetStaticDefaults.
			// Other mods will use the key "ExampleMod/Books" to access this data.
			DataInstance<List<string>>.Expose(Mod, "Books", Books);
			Books.Add("Test3");

			// This data is not intended to be "owned" by this mod, so we use GetOrAdd to register this data or retrieve the existing instance of this data registered by a mod that loaded before this mod.
			// Other mods will use the key "BannedWords" to access this data.
			DataInstance<HashSet<string>>.GetOrAdd("BannedWords", ref BannedWords);
			// Also note that we only edit the data after GetOrAdd, otherwise there is a potential for edits to be lost.
			BannedWords.Add("yolo");
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

	// This command helps visualize the shared data once other mods collaborate with the DataInstance examples in DataInstanceShowcase and with the custom item sets in CustomItemSets.
	// Type /shareddata in chat to view the contents of these sets
	public class SharedDataCommand : ModCommand
	{
		public override string Command => "shareddata";

		public override CommandType Type => CommandType.Chat;

		public override void Action(CommandCaller caller, string input, string[] args) {
			caller.Reply("True values in FlamingWeapon: " + string.Join(", ", CustomItemSets.FlamingWeapon.GetTrueIndexes().Select(ItemID.Search.GetName)));

			caller.Reply("True values in CantEquipWith_HiveBackpack: " + string.Join(", ", WaspNestGlobalItem.CantEquipWith_HiveBackpack.GetTrueIndexes().Select(ItemID.Search.GetName)));

			if (DataInstanceShowcase.DataFromOtherMod != null) {
				caller.Reply("DataFromOtherMod: " + string.Join(", ", DataInstanceShowcase.DataFromOtherMod.Select(x => x.DisplayName)));
			}

			caller.Reply("BannedWords: " + string.Join(", ", DataInstanceShowcase.BannedWords));
		}
	}
}
