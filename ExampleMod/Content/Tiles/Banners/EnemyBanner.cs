using Terraria.ModLoader;

namespace ExampleMod.Content.Tiles.Banners
{
	// This tile is for enemy banners (https://terraria.wiki.gg/wiki/Banners_(enemy)). Several ModNPC in ExampleMod share an existing BannerID, but the enemies represented in this tile have their own.
	// This class inherits from BannerTile. By inheriting from BannerTile, most of the logic needed to implement an enemy banner tile is automatically handled.
	// When placed, this tile will provide bonus damage to specific BannerIDs. For individual enemies, a BannerID is usually the same as their NPCID, but some enemies share a BannerID with similar NPC.
	// To support a new NPC, simply add an item texture to the Content/Items/Placeable/Banners folder, a tile sprite to Content/Tiles/Banners/EnemyBanner.png, set ModNPC.Banner and ModNPC.BannerItem on the ModNPC, and add an entry to EnemyBanner.StyleIDs.
	public class EnemyBanner : BannerTile
	{
		// This enum keeps our code clean and readable.
		public enum StyleID
		{
			ExampleWormHead,
			ExampleCustomAISlimeNPC
		}

		/*
		// EnemyBannerLoader below and EnemyBannerItem show a more automatic approach to implementing the banner items. Rather than making a class for each banner item (ExampleWormHeadBanner, ExampleCustomAISlimeNPCBanner, etc), a single class is loaded multiple times, once for each place style.
		// EnemyBannerLoader automatically loads an EnemyBannerItem instance for each supported enemy banner. (See EnemyBannerItem.cs as well for more infomation)
		// This approach is especially useful for mods with a large number of NPC and can facilitate cleaner code and help avoid hard to find bugs.
		// If using this approach, be aware that you'll need to change the following in ModNPC.SetDefaults:
		// BannerItem = ModContent.ItemType<ExampleCustomAISlimeNPCBanner>();
		// to
		// BannerItem = Mod.Find<ModItem>("ExampleCustomAISlimeNPCBanner").Type;

		public class EnemyBannerLoader : ILoadable
		{
			public void Load(Mod mod) {
				// For each entry in EnemyBanner.StyleIDs, we dynamically load a EnemyBannerItem. 
				foreach (StyleID styleID in Enum.GetValues(typeof(StyleID))) {
					mod.AddContent(new EnemyBannerItem(styleID.ToString() + "Banner", (int)styleID));
				}
			}

			public void Unload() {
			}
		}
		*/
	}
}
