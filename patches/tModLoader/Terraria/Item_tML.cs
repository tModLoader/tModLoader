using Terraria.ModLoader.IO;

namespace Terraria
{
	public partial class Item : TagSerializable
	{
		public static readonly TagDeserializer<Item> DESERIALIZER = new TagDeserializer<Item>(ItemIO.Load);

		public TagCompound SerializeData()
		{
			return ItemIO.Save(this);
		}
	}
}