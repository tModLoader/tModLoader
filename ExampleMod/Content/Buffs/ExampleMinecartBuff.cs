using Terraria.ModLoader;
using ExampleMod.Content.Mounts;

namespace ExampleMod.Content.Buffs
{
	public class ExampleMinecartBuff : MinecartBuffBase
	{
		// Constructor required as it is manually loaded
		public ExampleMinecartBuff(bool left) : base(left)
		{

		}

		public override int MountType => ModContent.MountType<ExampleMinecartMount>();
	}
}
