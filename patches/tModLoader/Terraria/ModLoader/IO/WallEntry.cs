using System;
using Terraria.ModLoader.Default;

namespace Terraria.ModLoader.IO
{
	internal class WallEntry : ModEntry
	{
		public static Func<TagCompound, WallEntry> DESERIALIZER = tag => new WallEntry(tag);

		public WallEntry(ModWall wall) : base(wall) { }

		public WallEntry(TagCompound tag) : base(tag) {}

		public override string DefaultUnloadedType => ModContent.GetInstance<UnloadedWall>().FullName;

		protected override string GetUnloadedType(ushort type) => DefaultUnloadedType;
	}
}
