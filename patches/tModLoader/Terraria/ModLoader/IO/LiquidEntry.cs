using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Default;

namespace Terraria.ModLoader.IO
{
    internal class LiquidEntry : ModBlockEntry
    {
		public static Func<TagCompound, LiquidEntry> DESERIALIZER = tag => new LiquidEntry(tag);

		public LiquidEntry(ModLiquid wall) : base(wall) { }

		public LiquidEntry(TagCompound tag) : base(tag) { }

		public override ModBlockType DefaultUnloadedPlaceholder => ModContent.GetInstance<UnloadedLiquid>();
	}
}
