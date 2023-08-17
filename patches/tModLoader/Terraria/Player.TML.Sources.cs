using Terraria.DataStructures;

namespace Terraria;

#nullable enable

public partial class Player
{
	// Item Use / Equipment Effects

	public IEntitySource GetSource_Accessory(Item item, string? context = null)
		=> new EntitySource_ItemUse(this, item, context);

	public IEntitySource GetSource_Accessory_OnHurt(Item item, Entity? attacker, string? context = null)
		=> new EntitySource_ItemUse_OnHurt(this, item, attacker, context);

	public IEntitySource GetSource_Accessory_OnHurt(Item item, PlayerDeathReason damageSource, string? context = null)
		=> GetSource_Accessory_OnHurt(item, whoAmI == Main.myPlayer && damageSource.TryGetCausingEntity(out var attacker) ? attacker : null, context);

	public IEntitySource GetSource_OnHurt(PlayerDeathReason damageSource, string? context = null)
		=> GetSource_OnHurt(whoAmI == Main.myPlayer && damageSource.TryGetCausingEntity(out var attacker) ? attacker : null, context);

	public IEntitySource GetSource_OpenItem(int itemType, string? context = null)
		=> new EntitySource_ItemOpen(this, itemType, context);

	public IEntitySource GetSource_ItemUse(Item item, string? context = null)
		=> new EntitySource_ItemUse(this, item, context);

	public IEntitySource GetSource_ItemUse_WithPotentialAmmo(Item item, int ammoItemId, string? context = null)
		=> new EntitySource_ItemUse_WithAmmo(this, item, ammoItemId, context);
}