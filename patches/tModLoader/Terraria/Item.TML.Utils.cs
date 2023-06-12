using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Terraria;

partial class Item
{
	/// <summary>
	/// A utility property for easily geting or setting the amount of items required for this item's current type to be researched.
	/// <br/> <b>NOTE:</b> The accessed values are stored per item type, not per item instance. You're recommended to only use the setter in load-time hooks, like <see cref="ModType.SetStaticDefaults"/>.
	/// </summary>
	public int ResearchUnlockCount {
		get => CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId.TryGetValue(type, out int result) ? result : 0;
		set => CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[type] = value;
	}
}