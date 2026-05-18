using Terraria.DataStructures;

namespace Terraria.ModLoader;

/// <summary>
/// A &quot;portable storage&quot; definition, encompassing an inventory
/// (<see cref="Chest"/>) and <see cref="TrackedProjectileReference"/> per-player.
/// <para/> The actual <see cref="Chest"/> and <see cref="TrackedProjectileReference"/>
/// references are not implemented by this class since their behavior is arbitrary
/// and ownership may be handled however the implementor pleases.
/// </summary>
public abstract class PortableStorage : ModType
{
	/// <summary>
	/// The storage definition for the Piggy Bank (and Money Trough/Eye Bone)
	/// </summary>
	public static PortableStorage PiggyBank { get; } = new PiggyBankPortableStorage();

	/// <summary>
	/// The storage definition for the Safe.
	/// </summary>
	public static PortableStorage Safe { get; } = new SafePortableStorage();

	/// <summary>
	/// The storage definition for the Defender's Forge.
	/// </summary>
	public static PortableStorage DefendersForge { get; } = new DefendersForgePortableStorage();

	/// <summary>
	/// The storage definition for the Void Vault (and Void Bag).
	/// </summary>
	public static PortableStorage VoidVault { get; } = new VoidVaultPortableStorage();

	public int Type { get; internal set; }

	public int ChestType => PortableStorageLoader.ReverseIds(Type);

	protected sealed override void Register()
	{
		ModTypeLookup<PortableStorage>.Register(this);
		Type = PortableStorageLoader.Register(this);
	}

	public sealed override void SetupContent()
	{
		SetStaticDefaults();
	}

	// The actual chest and tracked proj references are not implemented by this
	// class.  Developers may choose to implement it however they'd like; for
	// example, providing a single chest could be used across all players for
	// shared storage.

	/// <summary>
	/// The inventory belonging to this storage definition.
	/// </summary>
	/// <param name="player">The player the storage is requested on, assumedly the owner.</param>
	public abstract Chest GetInventory(Player player);

	/// <summary>
	/// The tracked reference to the projectile handling this portable storage
	/// definition, if present.
	/// </summary>
	/// <param name="player">The player the reference is requested on, assumedly the owner.</param>
	public abstract ref TrackedProjectileReference GetProjectileReference(Player player);
}
