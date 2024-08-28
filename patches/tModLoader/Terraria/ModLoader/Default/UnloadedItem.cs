using System.Collections.Generic;
using System.IO;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default;

[LegacyName("MysteryItem")]
public sealed class UnloadedItem : ModLoaderModItem
{
	[CloneByReference] // safe to share between clones, because it cannot be changed after creation/load
	private TagCompound data;

	public string ModName { get; private set; }
	public string ItemName { get; private set; }

	public override void SetDefaults()
	{
		Item.width = 20;
		Item.height = 20;
		Item.rare = 1;

		// Needs to be  > 1 for vanilla maxStack changes in 1.4.4,
		// but also conflicts with inability to know if two unloaded items are the same - Solxan
		Item.maxStack = int.MaxValue;
	}

	public override void SetStaticDefaults()
	{
		// Must not be researchable
		Item.ResearchUnlockCount = 0;
	}

	internal void Setup(TagCompound tag)
	{
		ModName = tag.GetString("mod");
		ItemName = tag.GetString("name");
		data = tag;
	}

	public override void ModifyTooltips(List<TooltipLine> tooltips)
	{
		for (int k = 0; k < tooltips.Count; k++) {
			if (tooltips[k].Name == "Tooltip0") {
				tooltips[k].Text = Language.GetTextValue(this.GetLocalizationKey("UnloadedItemModTooltip"), ModName);
			}
			else if (tooltips[k].Name == "Tooltip1") {
				tooltips[k].Text = Language.GetTextValue(this.GetLocalizationKey("UnloadedItemItemNameTooltip"), ItemName);
			}
		}
	}

	// Assume no two items are the same
	public override bool CanStack(Item source)
	{
		return false;
	}

	public override void SaveData(TagCompound tag)
	{
		foreach ((string key, object value) in data) {
			tag[key] = value;
		}
	}

	public override void LoadData(TagCompound tag)
	{
		Setup(tag);

		if (!ModContent.TryFind(ModName, ItemName, out ModItem modItem))
			return;

		if (modItem is UnloadedItem) { // Some previous bugs have lead to unloaded items containing unloaded items recursively
			LoadData(tag.GetCompound("data"));
			return;
		}

		var modData = tag.GetCompound("data");

		Item.SetDefaults(modItem.Type);

		ItemIO.LoadModdedPrefix(Item, tag);

		if (modData?.Count > 0) {
			Item.ModItem.LoadData(modData);
		}

		if (tag.ContainsKey("globalData")) {
			ItemIO.LoadGlobals(Item, tag.GetList<TagCompound>("globalData"));
		}
	}

	public override void NetSend(BinaryWriter writer)
	{
		TagIO.Write(data ?? new TagCompound(), writer);
	}

	public override void NetReceive(BinaryReader reader)
	{
		Setup(TagIO.Read(reader));
	}
}
