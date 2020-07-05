using System.Collections.Generic;
using System.IO;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	public class MysteryItem : ModItem
	{
		private string modName;
		private string itemName;
		private TagCompound data;

		public override string Texture => "ModLoader/MysteryItem";

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("{$tModLoader.UnloadedItemItemName}");
			Tooltip.SetDefault("\n");
		}

		public override void SetDefaults() {
			item.width = 20;
			item.height = 20;
			item.rare = 1;
		}

		internal void Setup(TagCompound tag) {
			modName = tag.GetString("mod");
			itemName = tag.GetString("name");
			data = tag;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			for (int k = 0; k < tooltips.Count; k++) {
				if (tooltips[k].Name == "Tooltip0") {
					tooltips[k].text = Language.GetTextValue("tModLoader.UnloadedItemModTooltip", modName);
				}
				else if (tooltips[k].Name == "Tooltip1") {
					tooltips[k].text = Language.GetTextValue("tModLoader.UnloadedItemItemNameTooltip", itemName);
				}
			}
		}

		public override TagCompound Save() {
			return data;
		}

		public override void Load(TagCompound tag) {
			Setup(tag);
			int type = ModLoader.GetMod(modName)?.ItemType(itemName) ?? 0;
			if (type > 0) {
				item.netDefaults(type);
				item.modItem.Load(tag.GetCompound("data"));
				ItemIO.LoadGlobals(item, tag.GetList<TagCompound>("globalData"));
			}
		}

		public override void LoadLegacy(BinaryReader reader) {
			string modName = reader.ReadString();
			bool hasGlobal = false;
			if (modName.Length == 0) {
				hasGlobal = true;
				modName = reader.ReadString();
			}
			Load(new TagCompound {
				["mod"] = modName,
				["name"] = reader.ReadString(),
				["hasGlobalSaving"] = hasGlobal,
				["legacyData"] = ItemIO.LegacyModData(int.MaxValue, reader, hasGlobal)
			});
		}

		public override void NetSend(BinaryWriter writer) {
			TagIO.Write(data ?? new TagCompound(), writer);
		}

		public override void NetRecieve(BinaryReader reader) {
			data = TagIO.Read(reader);
			modName = data.GetString("mod");
			itemName = data.GetString("name");
		}

		public override bool CloneNewInstances => true;

		public override ModItem Clone() {
			var clone = (MysteryItem)base.Clone();
			clone.data = (TagCompound)data?.Clone();
			return clone;
		}
	}
}
