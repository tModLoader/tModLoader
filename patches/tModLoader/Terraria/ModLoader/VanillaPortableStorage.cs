using Terraria.DataStructures;

namespace Terraria.ModLoader;

[Autoload(false)]
public sealed class PiggyBankPortableStorage : PortableStorage
{
	public override Chest GetInventory(Player player) => player.bank;

	public override ref TrackedProjectileReference GetProjectileReference(Player player) => ref player.piggyBankProjTracker;
}

[Autoload(false)]
public sealed class SafePortableStorage : PortableStorage
{
	public override Chest GetInventory(Player player) => player.bank;

	public override ref TrackedProjectileReference GetProjectileReference(Player player) => ref player.safeProjTracker;
}

[Autoload(false)]
public sealed class DefendersForgePortableStorage : PortableStorage
{
	public override Chest GetInventory(Player player) => player.bank;

	public override ref TrackedProjectileReference GetProjectileReference(Player player) => ref player.defendersForgeProjTracker;
}

[Autoload(false)]
public sealed class VoidVaultPortableStorage : PortableStorage
{
	public override Chest GetInventory(Player player) => player.bank4;

	public override ref TrackedProjectileReference GetProjectileReference(Player player) => ref player.voidLensChest;
}