using System;
using Terraria.ModLoader.IO;

namespace Terraria
{
	public partial class Item : TagSerializable
	{
		public static readonly Func<TagCompound, Item> DESERIALIZER = ItemIO.Load;

		public TagCompound SerializeData()
		{
			return ItemIO.Save(this);
		}
	}
}