using System.Collections.Generic;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria.ModLoader.Shops
{
	public static class ShopSellbackHelper
	{
		private class ItemMemo
		{
			public readonly int itemNetID;
			public readonly int itemPrefix;
			public int stack;

			public ItemMemo(Item item) {
				itemNetID = item.netID;
				itemPrefix = item.prefix;
				stack = item.stack;
			}

			public bool Matches(Item item) {
				if (item.netID == itemNetID)
					return item.prefix == itemPrefix;

				return false;
			}
		}

		private static List<ItemMemo> _memos = new List<ItemMemo>();

		public static void Add(Item item) {
			ItemMemo itemMemo = _memos.Find(x => x.Matches(item));
			if (itemMemo != null)
				itemMemo.stack += item.stack;
			else
				_memos.Add(new ItemMemo(item));
		}

		public static void Clear() {
			_memos.Clear();
		}

		public static int GetAmount(Item item) => _memos.Find(x => x.Matches(item))?.stack ?? 0;

		public static int Remove(Item item) {
			ItemMemo itemMemo = _memos.Find(x => x.Matches(item));
			if (itemMemo == null)
				return 0;

			int stack = itemMemo.stack;
			itemMemo.stack -= item.stack;
			if (itemMemo.stack <= 0)
			{
				_memos.Remove(itemMemo);
				return stack;
			}

			return stack - itemMemo.stack;
		}
	}

	public static class NPCShopManager
	{
		internal static List<NPCShop> shops = new List<NPCShop>();

		internal static Dictionary<int, Dictionary<string, CacheList>> entryCache = new Dictionary<int, Dictionary<string, CacheList>>();

		internal const int VanillaShopCount = 24;
		internal static int NextTypeID = VanillaShopCount;

		public static NPCShop CurrentShop => Main.npcShop > 0 ? GetShop(Main.npcShop) : null;

		public static NPCShop GetShop(int type) => shops[type];

		internal static void RegisterShop(NPCShop shop) {
			shop.Type = shop.NPCType switch
			{
				NPCID.Merchant => 1,
				NPCID.ArmsDealer => 2,
				NPCID.Dryad => 3,
				NPCID.Demolitionist => 4,
				NPCID.Clothier => 5,
				NPCID.GoblinTinkerer => 6,
				NPCID.Wizard => 7,
				NPCID.Mechanic => 8,
				NPCID.SantaClaus => 9,
				NPCID.Truffle => 10,
				NPCID.Steampunker => 11,
				NPCID.DyeTrader => 12,
				NPCID.PartyGirl => 13,
				NPCID.Cyborg => 14,
				NPCID.Painter => 15,
				NPCID.WitchDoctor => 16,
				NPCID.Pirate => 17,
				NPCID.Stylist => 18,
				NPCID.TravellingMerchant => 19,
				NPCID.SkeletonMerchant => 20,
				NPCID.DD2Bartender => 21,
				NPCID.Golfer => 22,
				NPCID.BestiaryGirl => 23,
				NPCID.Princess => 24,
				_ => NextTypeID++
			};

			shops.Add(shop);

			entryCache.Add(shop.Type, new Dictionary<string, CacheList>());
		}

		public static NPCShop GetShop<T>() where T : NPCShop => GetShop(ShopType<T>());

		public static int ShopType<T>() where T : NPCShop => ModContent.GetInstance<T>()?.Type ?? -1;

		internal static void Initialize() {
			// note: allow modded NPC to sell pylons?
			foreach (NPCShop shop in shops)
			{
				if (shop.Type > 23) continue;

				if (shop is TravellingMerchantShop || shop is SkeletonMerchantShop || Main.LocalPlayer.currentShoppingSettings.PriceAdjustment > 0.8500000238418579) continue;
				if (Main.LocalPlayer.ZoneCrimson || Main.LocalPlayer.ZoneCorrupt) continue;

				shop.CreateEntry(4876).AddCondition(
					!Condition.InSnow, !Condition.InDesert, !Condition.InBeach, !Condition.InJungle, !Condition.Halloween, !Condition.InGlowshroom,
					new SimpleCondition(NetworkText.FromKey("ShopConditions.PlayerPosY"), () => Main.LocalPlayer.position.Y < Main.worldSurface * 16.0));

				shop.CreateEntry(4920).AddCondition(Condition.InSnow);

				shop.CreateEntry(4919).AddCondition(Condition.InDesert);

				shop.CreateEntry(4917)
					.AddCondition(
						!Condition.InSnow, !Condition.InDesert, !Condition.InBeach, !Condition.InJungle, !Condition.Halloween, !Condition.InGlowshroom,
						new SimpleCondition(NetworkText.FromKey("ShopConditions.PlayerPosY"), () => Main.LocalPlayer.position.Y >= Main.worldSurface * 16.0));

				shop.CreateEntry(4918).AddCondition(Condition.InBeach, new SimpleCondition(NetworkText.FromKey("ShopConditions.PlayerPosY"), () => Main.LocalPlayer.position.Y < Main.worldSurface * 16.0));

				shop.CreateEntry(4875).AddCondition(Condition.InJungle);

				shop.CreateEntry(4916).AddCondition(Condition.InHallow);

				shop.CreateEntry(4921).AddCondition(Condition.InGlowshroom);
			}

			// foreach (KeyValuePair<int,NPCShop> pair in shops)
			// {
			// 	Logging.tML.Debug(Lang.GetNPCNameValue(pair.Key));
			// 	
			// 	foreach (NPCShop.Entry entry in pair.Value.pages.SelectMany(x=>x.Value.entries))
			// 	{
			// 		Logging.tML.Debug("\t"+entry.GetItems(false).Select(x=>x.HoverName).Aggregate((x,y)=>x + ", "+y));
			//
			// 		foreach (Condition condition in entry.Conditions)
			// 		{
			// 			Logging.tML.Debug("\t\t" + condition.Description);
			// 		}
			// 	}
			// }
		}

		public static void Unload() {
			NextTypeID = VanillaShopCount;

			shops.Clear();
			entryCache.Clear();
		}
	}
}