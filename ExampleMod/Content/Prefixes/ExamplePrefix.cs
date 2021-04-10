using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Prefixes
{
	[Autoload(false)]
    public class ExamplePrefix : ModPrefix
    {
		// This it the class that will be Autoloaded, and then it will load and register each of the prefixes.
		public class ExamplePrefixLoader : ModType
		{
			List<ExamplePrefix> prefixes = new List<ExamplePrefix>();

			// This constructor puts values into the prefix list, which will be used in the next two functions.
			public ExamplePrefixLoader() {
				prefixes.Add(new ExamplePrefix(1, "Awesome"));
				prefixes.Add(new ExamplePrefix(2, "ReallyAwesome"));
			}

			// Called when this class is loaded, loads and registers the prefixes.
			protected override void Register() => prefixes.ForEach(prefix => Mod.AddContent(prefix));

			// It's currently pretty important to unload your static fields like this, to avoid having parts of your mod remain in memory when it's been unloaded.
			public override void Unload() => prefixes = null;
		}
		public override string Name => _displayName;
		private readonly byte _power;
		private readonly string _displayName;

        // The prefix' constructor. Here we use it for setting the prefix's power.
        public ExamplePrefix(byte power, string name)
        {
            _power = power;
			_displayName = name;
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

		// Set the display name and translations of this prefix with this function.
		public override void SetDefaults() => DisplayName.SetDefault(_displayName);

        // Use this function to modify these stats for items which have this prefix: Damage Multiplier, Knockback Multiplier, Use Time Multiplier,
        //			Scale Multiplier (Size), Shoot Speed Multiplier, Mana Multiplier (Mana cost), Crit Bonus.
        public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult,
            ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus)
        {
            damageMult *= 1f + 0.20f * _power;
        }

        // Modify the cost of items with this modifier with this function.
        public override void ModifyValue(ref float valueMult)
        {
            float multiplier = 1f + 0.05f * _power;
            valueMult *= multiplier;
        }

        // This is used to modify most other stats of items which have this modifier.
        public override void Apply(Item item) { }
    }
}