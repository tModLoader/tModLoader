using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Terraria;

partial class Item
{
	/// <summary>
	/// A utility property for easily getting or setting the amount of items required for this item's current type to be researched.
	/// <br/> By default, all modded items will have this set to 1. Set to 0 for un-researchable items, such as items that disappear on pickup. The <see href="https://terraria.wiki.gg/wiki/Journey_Mode#Research">Journey Mode Research wiki page</see> lists values for various types of items, use it as a guide for consistency.
	/// <br/> <b>NOTE:</b> The accessed values are stored per item type, not per item instance. You're recommended to only use the setter in load-time hooks, like <see cref="ModType.SetStaticDefaults"/>.
	/// </summary>
	public int ResearchUnlockCount {
		get => CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId.TryGetValue(type, out int result) ? result : 0;
		set {
			if (value < 1)
				CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId.Remove(type); // 0 would behave incorrectly, so remove.
			else
				CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[type] = value;
		}
	}
}