using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Prefixes
{
	// We want to load 2 similar but slightly different versions of the prefix. 
	// To do this, we'll disable autoloading, and then in a separate ILoadable class, add the two variants directly
	[Autoload(false)]
	public class ExamplePrefix : ModPrefix
	{
		// This class will be Autoloaded because it imlements ILoadable.
		// We use it to add the two variants of the prefix
		public class ExamplePrefixLoader : ILoadable
		{
			public void Load(Mod mod) {
				mod.AddContent(new ExamplePrefix(1, "Awesome"));
				mod.AddContent(new ExamplePrefix(2, "Really_Awesome"));
			}

			public void Unload() { }
		}
		public override string Name => _displayName;
		private readonly int _power;
		private readonly string _name;

		// The prefix' constructor. Here we use it for setting the prefix's power and name.
		public ExamplePrefix(byte power, string name) {
			_power = power;
			_name = name;
		}

		// See documentation for vanilla weights and more information.
		// In case of multiple prefixes with similar functions this can be used with a switch/case to provide different chances for different prefixes
		// Note: a weight of 0f might still be rolled. See CanRoll to exclude prefixes.
		// Note: if you use PrefixCategory.Custom, actually use ModItem.ChoosePrefix instead.
		public override float RollChance(Item item)
			=> 5f;

		// Determines if it can roll at all.
		// Use this to control if a prefix can be rolled or not.
		public override bool CanRoll(Item item)
			=> true;

		// Change your category this way, defaults to PrefixCategory.Custom. Affects which items can get this prefix.
		public override PrefixCategory Category
			=> PrefixCategory.AnyWeapon;

		// Set the translations of this prefix with this function.
		public override void SetDefaults() {
			DisplayName.SetDefault(Name.Replace("_", " "));
		}

		// Use this function to modify these stats for items which have this prefix: Damage Multiplier, Knockback Multiplier, Use Time Multiplier,
		//			Scale Multiplier (Size), Shoot Speed Multiplier, Mana Multiplier (Mana cost), Crit Bonus.
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult,
			ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult *= 1f + 0.20f * _power;
		}

		// Modify the cost of items with this modifier with this function.
		public override void ModifyValue(ref float valueMult) {
			float multiplier = 1f + 0.05f * _power;
			valueMult *= multiplier;
		}

		// This is used to modify most other stats of items which have this modifier.
		public override void Apply(Item item) { }
	}
}
