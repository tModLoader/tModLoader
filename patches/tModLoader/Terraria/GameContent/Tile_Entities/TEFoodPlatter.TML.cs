using System.IO;
using Terraria.ModLoader.IO;

namespace Terraria.GameContent.Tile_Entities;

public partial class TEFoodPlatter
{
	public override void SaveData(TagCompound tag)
	{
		tag["item"] = ItemIO.Save(item);
	}

	public override void LoadData(TagCompound tag) => item = ItemIO.Load(tag.GetCompound("item"));

	public override void NetSend(BinaryWriter writer) => ItemIO.Send(item, writer, true);

	public override void NetReceive(BinaryReader reader) => item = ItemIO.Receive(reader, true);
}
