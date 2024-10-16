using ExampleMod.Content.DamageClasses;
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
	/// This class shows off usage of DataInstance and SetHandler classes. These classes facilitate ad-hoc collaboration through shared data instances. What this means is rather than including direct dependencies on mods, mods can collaborate with any mod (mods they might not even be aware of) as long as they use data with names/identifiers agreed upon by the community. Whether there is just 1 mod or many mods attempting to utilize this data, all mods will share access to the same data instance.
	/// The examples here are contrived examples meant to illustrate correct usage and other notes or warnings. See WaspNest.cs to see how CantEquipWith_HiveBackpack is used in the contained classes for a real example.
	/// See the notes in https://github.com/tModLoader/tModLoader/pull/PutPRNumberHere to see what the code in the other mod would look like for accessing the data instances shared by these approaches.
	/// </summary>
	public class CustomSetsSystem : ModSystem
	{
		// Custom item set example. This will behave the same as any other ItemID.Sets array.
		public const string FlamingWeaponCustomSetKey = "FlamingWeapon"; // This string MUST be consistent between mods working together.
		public static bool[] FlamingWeapon; // Don't initialize the array using "= ItemID.Sets.Factory.CreateBoolSet(false);" here, that will NOT work correctly.

		// Arbitrary data examples:
		// Mods can achieve ad-hoc collaboration thorough simple name-object pairings. Or, simple collaboration with a dependency can be done without writing Mod.Call code. 
		// This data is exposed for other mods to use or modify. 
		public static HashSet<string> bannedWords = new();
		// This data is retrieved from another mod.
		public static List<DamageClass> dataFromOtherMod;

		public override void Load() {
			// Exposing data needs to happen in Load so that other mods can retrieve the data in SetStaticDefaults
			DataInstance.Expose(Mod, "BannedWords", bannedWords);
			bannedWords.Add("yolo");
		}

		public override void ResizeArrays() {
			// ResizeArrays is the earliest method called after all content has loaded and has been assigned ID values.
			// This is where methods such as SetFactory.CreateBoolSet and SetHandler.RegisterCustomSet should be called.

			// To create a custom item set, we use the ItemID.Sets.Factory.CreateXSet method.
			FlamingWeapon = ItemID.Sets.Factory.CreateBoolSet(false, ItemID.FieryGreatsword);

			// SetHandler.RegisterCustomSet exposed the set for other mods to access. The key and default value must be consistent with other mods.
			SetHandler.RegisterCustomSet(FlamingWeaponCustomSetKey, false, ref FlamingWeapon);

			// We can further edit the set. These changes will be consistent between all mods accessing this set since the object reference is shared.
			FlamingWeapon[ItemID.FireWhip] = true;
			FlamingWeapon[ItemID.HelFire] = true;

			// See also ExampleFlail.cs. In ExampleFlail.SetStaticDefaults we also designate that weapon as a "FlamingWeapon".
			// Finally, see CustomSetsModPlayer below where code uses FlamingWeapon for a unique gameplay effect
		}

		public override void SetStaticDefaults() {
			// SetStaticDefaults is an appropriate place to retrieve data exposed by other mods.
			dataFromOtherMod = DataInstance.Retrieve("CustomSetTest1", "specialDamageClasses") as List<DamageClass>;
			if (dataFromOtherMod != null) {
				dataFromOtherMod.Add(ModContent.GetInstance<ExampleDamageClass>());
			}
		}
	}

	public class CustomSetsModPlayer : ModPlayer {
		public override void OnHitAnything(float x, float y, Entity victim) {
			if(CustomSetsSystem.FlamingWeapon[Player.HeldItem.type] && Main.rand.NextBool(100)) {
				CombatText.NewText(Player.getRect(), Color.Red, "Hahahah, burn!");
			}
		}
	}

	public class CustomSetsCommand : ModCommand
	{
		public override string Command => "customsets";

		public override CommandType Type => CommandType.Chat;

		public override void Action(CommandCaller caller, string input, string[] args) {
			caller.Reply("True values in FlamingWeapon: " + string.Join(", ", CustomSetsSystem.FlamingWeapon.GetTrueIndexes().Select(ItemID.Search.GetName)));

			caller.Reply("True values in CantEquipWith_HiveBackpack: " + string.Join(", ", WaspNestSystem.CantEquipWith_HiveBackpack.GetTrueIndexes().Select(ItemID.Search.GetName)));

			if (CustomSetsSystem.dataFromOtherMod != null) {
				caller.Reply("dataFromOtherMod: " + string.Join(", " , CustomSetsSystem.dataFromOtherMod.Select(x => x.DisplayName)));
			}

			caller.Reply("bannedWords: " + string.Join(", ", CustomSetsSystem.bannedWords));
		}
	}
}
