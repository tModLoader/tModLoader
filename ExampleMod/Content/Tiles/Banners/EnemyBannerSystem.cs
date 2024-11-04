using ExampleMod.Content.Items.Placeable.Banners;
using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace ExampleMod.Content.Tiles.Banners
{
	// This class automates the loading of an EnemyBannerItem instance for each supported enemy banner.
	// In Load, we load a EnemyBannerItem for each supported BannerID/NPCID.
	// In PostSetupContent, we map each placement style to the NPCID. This needs to be done after loading so that the ModNPC have had a chance to load first.
	public class EnemyBannerSystem : ModSystem
	{
		private static Dictionary<int, int> tileStyleToBannerIDMapping = new();

		public static void RegisterBanner(int tileStyle, int bannerID) => tileStyleToBannerIDMapping[tileStyle] = bannerID;

		// Given an tile place style, returns the corresponding BannerID.
		public static int GetBannerID(int tileStyle) => tileStyleToBannerIDMapping.TryGetValue(tileStyle, out var id) ? id : -1;

		public override void Load() {
			// For each entry in EnemyBanner.StyleIDs, we dynamically load a EnemyBannerItem. 
			foreach (var styleName in EnemyBanner.StyleIDs.Search.Names) {
				int placeStyle = EnemyBanner.StyleIDs.Search.GetId(styleName);
				int? killsToBanner = null;
				if (styleName ==  nameof(EnemyBanner.StyleIDs.ExampleWormHead)) {
					killsToBanner = 25; // Weird to have this here in the ModSystem class...
				}
				Mod.AddContent(new EnemyBannerItem(styleName + "Banner", placeStyle, killsToBanner));
			}

			// Enum alt approach
			foreach (EnemyBanner.StyleIDsEnum styleID in Enum.GetValues(typeof(EnemyBanner.StyleIDsEnum))) {
				int? killsToBanner = null;
				if (styleID == EnemyBanner.StyleIDsEnum.ExampleWormHead) {
					killsToBanner = 25; // Weird to have this here in the ModSystem class...
				}
				Mod.AddContent(new EnemyBannerItem(styleID.ToString() + "Banner", (int)styleID, killsToBanner));
			}
		}

		public override void PostSetupContent() {
			// Now that all content has loaded, we can create a mapping of placeStyle to BannerIDs:
			foreach (var styleName in EnemyBanner.StyleIDs.Search.Names) {
				int placeStyle = EnemyBanner.StyleIDs.Search.GetId(styleName);
				int bannerID = Mod.Find<ModNPC>(styleName).Banner;
				RegisterBanner(placeStyle, bannerID);
			}

			// Enum alt approach
			foreach (var styleID in Enum.GetValues(typeof(EnemyBanner.StyleIDsEnum))) {
				int bannerID = Mod.Find<ModNPC>(styleID.ToString()).Banner;
				RegisterBanner((int)styleID, bannerID);
			}
		}
	}
}
