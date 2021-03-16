using System;
using Terraria.ModLoader.Default;

namespace Terraria.ModLoader.IO
{
	internal class WallEntry : ModEntry
	{
		public static Func<TagCompound, WallEntry> DESERIALIZER = tag => DeserializeTag<WallEntry>(tag);

		public override string DefaultUnloadedType => ModContent.GetInstance<UnloadedWall>().FullName;
	}
}
