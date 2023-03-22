using Terraria.ModLoader;

namespace Terraria.ID;

partial class AmmoID
{
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
		/// If <see langword="true"/> for a given item type (<see cref="Item.type"/>), then items of that type are counted as arrows for the purposes of <see cref="Player.rocketDamage"/>.
		/// <br/> Defaults to <see langword="false"/>.
		/// </summary>
		public static bool[] IsRocket = Factory.CreateBoolSet(false, Rocket, StyngerBolt, JackOLantern, NailFriendly);
	}
}
