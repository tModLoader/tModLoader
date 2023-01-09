using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default;

public class StartBag : ModLoaderModItem
{
	[CloneByReference] // safe to share between clones, because it cannot be changed after creation/load
	private List<Item> items = new List<Item>();

	public override void SetDefaults()
	{
		Item.width = 20;
		Item.height = 20;
		Item.rare = 1;
	}

	internal void AddItem(Item item)
	{
		items.Add(item);
	}

	public override bool CanRightClick()
	{
		return true;
	}

	public override void RightClick(Player player)
	{
		var itemSource = player.GetItemSource_OpenItem(Type);

		foreach (Item item in items) {
			int k = Item.NewItem(itemSource, player.getRect(), item.type, item.stack, prefixGiven: item.prefix);

			if (Main.netMode == 1) {
				NetMessage.SendData(ID.MessageID.SyncItem, -1, -1, null, k, 1f);
			}
		}
	}

	public override void SaveData(TagCompound tag)
	{
		tag["items"] = items;
	}

	public override void LoadData(TagCompound tag)
	{
		items = tag.Get<List<Item>>("items");
	}
}
