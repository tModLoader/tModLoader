using Terraria.ModLoader;

namespace Terraria.ID;

/// <summary>
/// AmmoID entries represent ammo types. Ammo items that share the same AmmoID value assigned to <see cref="Item.ammo"/> can all be used as ammo for weapons using that same value for <see cref="Item.useAmmo"/>. AmmoID values are actually equivalent to the <see cref="ItemID"/> value of the iconic ammo item.<br/>
/// The <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-Ammo">Basic Ammo Guide</see> teaches more about ammo.
/// </summary>
partial class AmmoID
{
	partial class Sets
	{
		public static SetFactory Factory = new(ItemLoader.ItemCount, nameof(AmmoID), ItemID.Search);

		// Elements for the below three "Is<x>" sets from Player.GetWeaponDamage() and Player.PickAmmo().
		// https://github.com/tModLoader/tModLoader/pull/2288

		/// <summary>
		/// If <see langword="true"/> for a given item type (<see cref="Item.type"/>), then items of that type are counted as arrows for the purposes of <see cref="Player.arrowDamage"/> and the <see cref="ItemID.MagicQuiver"/>.
		/// <br/> Defaults to <see langword="false"/>.
		/// </summary>
		public static bool[] IsArrow = Factory.CreateBoolSet(false, Arrow, Stake);
		/// <summary>
		/// If <see langword="true"/> for a given item type (<see cref="Item.type"/>), then items of that type are counted as bullets for the purposes of <see cref="Player.bulletDamage"/>.
		/// <br/> Defaults to <see langword="false"/>.
		/// </summary>
		public static bool[] IsBullet = Factory.CreateBoolSet(false, Bullet, CandyCorn);
		/// <summary>
		/// If <see langword="true"/> for a given item type (<see cref="Item.type"/>), then items of that type are counted as specialist ammo for the purposes of <see cref="Player.specialistDamage"/>.
		/// <br/> Defaults to <see langword="false"/>.
		/// </summary>
		public static bool[] IsSpecialist = Factory.CreateBoolSet(false, Rocket, StyngerBolt, JackOLantern, NailFriendly, Coin, Flare, Dart, Snowball, Sand, FallenStar, Gel);

		/// <summary>
		/// Maps a weapon item type (<see cref="Item.type"/>) to a fallback item type to use when determining weapon-specific projectiles for an ammo. If an entry is not found in <see cref="SpecificLauncherAmmoProjectileMatches"/> for the weapon item and ammo item pair, then the fallback item provided by this set will be used as a fallback query into <see cref="SpecificLauncherAmmoProjectileMatches"/>.
		/// <para/> This enables weapons, most commonly weapons using rocket ammo, to properly use "variant projectiles" for specific ammo without tedious copy and paste or cross-mod integrations. For example, Mod A could add an upgraded <see cref="ItemID.SnowmanCannon"/> using this fallback mechanism. If Mod B adds a new rocket ammo item and associated snow themed variant projectile to <see cref="SpecificLauncherAmmoProjectileMatches"/>, then Mod A's weapon would automatically use that variant projectile when using that ammo.
		/// <para/> Defaults to <c>-1</c>.
		/// </summary>
		public static int[] SpecificLauncherAmmoProjectileFallback = Factory.CreateIntSet();
	}
}
