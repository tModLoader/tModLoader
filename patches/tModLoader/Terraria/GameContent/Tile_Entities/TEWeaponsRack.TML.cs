using System.IO;
using Terraria.ModLoader.IO;

namespace Terraria.GameContent.Tile_Entities
{
	public partial class TEWeaponsRack
	{
		public override TagCompound Save() => new TagCompound {
			{ "item", ItemIO.Save(item) }
		};

		public override void Load(TagCompound tag) => item = ItemIO.Load(tag.GetCompound("item"));

		public override void NetSend(BinaryWriter writer, bool lightSend) => ItemIO.Send(item, writer, true);

		public override void NetReceive(BinaryReader reader, bool lightReceive) => item = ItemIO.Receive(reader, true);
	}
}
