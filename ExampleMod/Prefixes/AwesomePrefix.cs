using Terraria;
using Terraria.ModLoader;
using ExampleMod.Items;

namespace ExampleMod.Prefixes
{
	public class AwesomePrefix : ModPrefix
	{
		private readonly byte power = 0;

		// see documentation for vanilla weights and more information
		// note: a weight of 0f can still be rolled. see CanRoll to exclude prefixes.
		// note: if you use PrefixCategory.Custom, actually use ChoosePrefix instead, see ExampleInstancedGlobalItem
		public override float RollChance(Item item)
		{
			return 5f;
		} 

		// determines if it can roll at all.
		// use this to control if a prefixes can be rolled or not
		public override bool CanRoll(Item item)
		{
			return true;
		}

		// change your category this way, defaults to Custom
		public override PrefixCategory Category { get { return PrefixCategory.AnyWeapon; } }
		
		public AwesomePrefix()
		{
		}

		public AwesomePrefix(byte power)
		{
			this.power = power;
		}

		// Allow multiple prefix autoloading this way (permutations of the same prefix)
		public override bool Autoload(ref string name)
		{
			if (base.Autoload(ref name))
			{
				mod.AddPrefix("Awesome", new AwesomePrefix(1));
				mod.AddPrefix("ReallyAwesome", new AwesomePrefix(2));
			}
			return false;
		}

		public override void Apply(Item item)
		{
			item.GetGlobalItem<ExampleInstancedGlobalItem>().awesome = power;
		}

		public override void ModifyValue(ref float valueMult)
		{
			float multiplier = 1f + 0.05f * power;
			valueMult *= multiplier;
		}
	}
}