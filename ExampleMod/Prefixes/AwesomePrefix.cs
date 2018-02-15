using Terraria;
using Terraria.ModLoader;
using ExampleMod.Items;

namespace ExampleMod.Prefixes
{
	public class AwesomePrefix : ModPrefix
	{
		private byte power = 0;

		// change your category this way
		public override PrefixCategory Category { get { return PrefixCategory.Custom; } }
		
		public AwesomePrefix()
		{
		}

		public AwesomePrefix(byte power)
		{
			this.power = power;
		}

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