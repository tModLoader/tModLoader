using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	public class StartBag : ModItem
	{
		private List<Item> items = new List<Item>();

		public override string Texture => "ModLoader/StartBag";

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("{$tModLoader.StartBagItemName}");
			Tooltip.SetDefault("{$tModLoader.StartBagTooltip}\n{$CommonItemTooltip.RightClickToOpen}");
		}

		public override void SetDefaults() {
			item.width = 20;
			item.height = 20;
			item.rare = 1;
		}

		internal void AddItem(Item item) {
			items.Add(item);
		}

		public override bool CanRightClick() {
			return true;
		}

		public override void RightClick(Player player) {
			foreach (Item item in items) {
				int k = Item.NewItem((int)player.position.X, (int)player.position.Y, player.width, player.height,
							item.type, item.stack, false, item.prefix, false, false);
				if (Main.netMode == 1) {
					NetMessage.SendData(ID.MessageID.SyncItem, -1, -1, null, k, 1f);
				}
			}
		}

		public override TagCompound Save() {
			return new TagCompound { ["items"] = items };
		}

		public override void Load(TagCompound tag) {
			items = tag.Get<List<Item>>("items");
		}

		public override void LoadLegacy(BinaryReader reader) {
			int count = reader.ReadUInt16();
			for (int k = 0; k < count; k++) {
				var item = new Item();
				ItemIO.LoadLegacy(item, reader, true);
				AddItem(item);
			}
		}
	}
}
