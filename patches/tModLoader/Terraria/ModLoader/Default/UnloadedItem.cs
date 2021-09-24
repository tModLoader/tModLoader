using System.Collections.Generic;
using System.IO;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	[LegacyName("MysteryItem")]
	public class UnloadedItem : ModLoaderModItem
	{
		private TagCompound data;

		public string ModName { get; private set; }
		public string ItemName { get; private set; }

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("{$tModLoader.UnloadedItemItemName}");
			Tooltip.SetDefault("\n");
		}

		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 20;
			Item.rare = 1;
		}

		internal void Setup(TagCompound tag) {
			ModName = tag.GetString("mod");
			ItemName = tag.GetString("name");
			data = tag;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			for (int k = 0; k < tooltips.Count; k++) {
				if (tooltips[k].Name == "Tooltip0") {
					tooltips[k].text = Language.GetTextValue("tModLoader.UnloadedItemModTooltip", ModName);
				}
				else if (tooltips[k].Name == "Tooltip1") {
					tooltips[k].text = Language.GetTextValue("tModLoader.UnloadedItemItemNameTooltip", ItemName);
				}
			}
		}

		public override void SaveData(TagCompound tag) {
			foreach ((string key, object value) in data) {
				tag[key] = value;
			}
		}

		public override void LoadData(TagCompound tag) {
			Setup(tag);

			if (ModContent.TryFind(ModName, ItemName, out ModItem modItem)) {
				Item.SetDefaults(modItem.Type);

				if (data?.Count > 0) {
					Item.ModItem.LoadData(data);
				}

				if (tag.ContainsKey("globalData")) {
					ItemIO.LoadGlobals(Item, tag.GetList<TagCompound>("globalData"));
				}
			}
		}

		public override void NetSend(BinaryWriter writer) {
			TagIO.Write(data ?? new TagCompound(), writer);
		}

		public override void NetReceive(BinaryReader reader) {
			Setup(TagIO.Read(reader));
		}

		public override ModItem Clone(Item item) {
			var clone = (UnloadedItem)base.Clone(item);
			clone.data = (TagCompound)data?.Clone();
			return clone;
		}
	}
}
