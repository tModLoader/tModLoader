using Terraria.ModLoader;

namespace Terraria.ID;

partial class AmmoID
{
	partial class Sets
	{
		public static SetFactory Factory = new(ItemLoader.ItemCount);

		//TODO: Document, especially what code areas these are based on.

		public static bool[] IsArrow = Factory.CreateBoolSet(false, Arrow, Stake);
		public static bool[] IsBullet = Factory.CreateBoolSet(false, Bullet, CandyCorn);
		public static bool[] IsRocket = Factory.CreateBoolSet(false, Rocket, StyngerBolt, JackOLantern, NailFriendly);
	}
}
