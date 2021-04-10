using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Prefixes
{
    public class ExamplePrefix : ModPrefix
    {

        // The constructor. It's needed for autoloading.
        public ExamplePrefix() { }

        // See documentation for vanilla weights and more information.
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

		// Set the display name translation default for this prefix.
		public override void SetDefaults() => DisplayName.SetDefault("Exemplary");

        // Use this function to modify these stats for items which have this prefix: Damage Multiplier, Knockback Multiplier, Use Time Multiplier,
        //			Scale Multiplier (Size), Shoot Speed Multiplier, Mana Multiplier (Mana cost), Crit Bonus.
        public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult,
            ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus)
        {
            damageMult *= 1.20f;
        }

        // Modify the cost of items with this modifier with this function.
        public override void ModifyValue(ref float valueMult)
        {
            float multiplier = 1.05f;
            valueMult *= multiplier;
        }

        // This is used to modify most other stats of items which have this modifier.
        public override void Apply(Item item) { }
    }
}