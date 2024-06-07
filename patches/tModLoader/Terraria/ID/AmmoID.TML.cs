using ReLogic.Reflection;
#if !TMLANALYZER
using Terraria.ModLoader;
#endif

namespace Terraria.ID;

/// <summary>
/// AmmoID entries represent ammo types. Ammo items that share the same AmmoID value assigned to <see cref="Item.ammo"/> can all be used as ammo for weapons using that same value for <see cref="Item.useAmmo"/>. AmmoID values are actually equivalent to the <see cref="ItemID"/> value of the iconic ammo item.<br/>
/// The <see href="https://github.com/tModLoader/tModLoader/wiki/Basic-Ammo">Basic Ammo Guide</see> teaches more about ammo.
/// </summary>
partial class AmmoID
{
#if !TMLANALYZER
	partial class Sets
	{
		public static SetFactory Factory = new(ItemLoader.ItemCount);

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
	}
#endif

	public static readonly IdDictionary Search = IdDictionary.Create(typeof(AmmoID), typeof(int));
}
