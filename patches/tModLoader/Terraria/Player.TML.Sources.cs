using Terraria.DataStructures;

namespace Terraria;

#nullable enable

public partial class Player
{
	// Item Use / Equipment Effects

	public IEntitySource GetSource_Accessory(Item item, string? context = null)
		=> new EntitySource_ItemUse(this, item, context);

	public IEntitySource GetSource_OpenItem(int itemType, string? context = null)
		=> new EntitySource_ItemOpen(this, itemType, context);

	public IEntitySource GetSource_ItemUse(Item item, string? context = null)
		=> new EntitySource_ItemUse(this, item, context);

	public IEntitySource GetSource_ItemUse_WithPotentialAmmo(Item item, int ammoItemId, string? context = null)
		=> new EntitySource_ItemUse_WithAmmo(this, item, ammoItemId, context);
}