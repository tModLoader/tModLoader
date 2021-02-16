using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	public class UnloadedEvent : ModEvent
	{
		internal IList<TagCompound> data;

		public override Texture2D Icon => null;

		public override TagCompound Save() {
			return new TagCompound() {
				[nameof(data)] = data
			};
		}

		public override void Load(TagCompound tag) {
			WorldIO.LoadModEventData(tag.GetList<TagCompound>("data"));
		}
	}
}
