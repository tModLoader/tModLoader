using System;
using Terraria.ModLoader.Default;

namespace Terraria.ModLoader.IO;

internal class LiquidEntry : ModBlockEntry
{
	public static Func<TagCompound, LiquidEntry> DESERIALIZER = tag => new LiquidEntry(tag);

	public LiquidEntry(ModLiquid liquid) : base(liquid) { }

	public LiquidEntry(TagCompound tag) : base(tag) { }

	public override ModBlockType DefaultUnloadedPlaceholder => ModContent.GetInstance<UnloadedLiquid>();
}
