using System;
using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace ExampleMod.Content.Tiles.Banners
{
	// This tile is for enemy banners (https://terraria.wiki.gg/wiki/Banners_(enemy)). Several ModNPC in ExampleMod (ExampleZombieThief and PartyZombie) share a banner with an existing enemy, but the enemies represented in this tile have their own.
	// This class inherits from ModBannerTile. By inheriting from ModBannerTile, most of the logic needed to implement an enemy banner tile is automatically handled.
	// When placed, this tile will provide bonus damage to specific enemies.
	// To support a new NPC, do the following:
	//   1. Add an entry to the bottom of the EnemyBanner.StyleID enum.
	//   2. Add an item texture and corresponding ModItem class to the Content/Items/Placeable/Banners folder
	//   3. Update Content/Tiles/Banners/EnemyBanner.png with the tile sprite
	//   4. Assign the banner in each ModNPC that shares the banner. Do this by setting ModNPC.Banner and ModNPC.BannerItem in ModNPC.SetDefaults. In addition, call the RegisterStyle method as well in ModNPC.SetDefaults for the representative ModNPC. See ExampleCustomAISlimeNPC.SetDefaults and ExampleWormHead/Body/Tail.SetDefaults for examples.
	public class EnemyBanner : ModBannerTile
	{
		// This enum keeps our code clean and readable.
		// Each enum entry has a numerical value (0, 1, ...) which corresponds to the tile style of the placed banner.
		public enum StyleID
		{
			ExampleWormHead,
			ExampleCustomAISlimeNPC
		}
	}

	/*
	// EnemyBannerLoader and AutoloadedBannerItem below show a more automatic approach to implementing the banner items. Rather than making a class for each banner item (ExampleWormHeadBanner, ExampleCustomAISlimeNPCBanner, etc), a single class is loaded multiple times, once for each place style.
	// EnemyBannerLoader automatically loads an AutoloadedBannerItem instance for each supported enemy type.
	// This approach is especially useful for mods with a large number of NPC and can facilitate cleaner code and help avoid hard to find typos and bugs.
	// If using this approach, be aware that you'll need to change the following in ModNPC.SetDefaults:
	// BannerItem = ModContent.ItemType<ExampleCustomAISlimeNPCBanner>();
	// to
	// BannerItem = Mod.Find<ModItem>("ExampleCustomAISlimeNPCBanner").Type;
	// or
	// BannerItem = Mod.Find<ModItem>($"{nameof(EnemyBanner.StyleID.ExampleCustomAISlimeNPC)}Banner").Type;

	public class EnemyBannerLoader : ILoadable
	{
		public void Load(Mod mod) {
			// For each entry in EnemyBanner.StyleID, we dynamically load an AutoloadedBannerItem.
			foreach (EnemyBanner.StyleID styleID in Enum.GetValues(typeof(EnemyBanner.StyleID))) {
				mod.AddContent(new AutoloadedBannerItem(styleID.ToString() + "Banner", (int)styleID));
			}
		}

		public void Unload() {
		}
	}

	// All banner placing items are essentially the same aside from their placeStyle.
	// This class would be used by the EnemyBannerLoader class to automatically load items for each supported npc type.
	// Note that if you use this approach, move this class to the ExampleMod.Content.Items.Placeable.Banners namespace if you want to keep loading banner item textures from that folder.
	public class AutoloadedBannerItem : ModItem
	{
		private string itemName;
		private int placeStyle;

		public override string Name => itemName;

		protected override bool CloneNewInstances => true;

		public AutoloadedBannerItem(string itemName, int placeStyle) {
			this.itemName = itemName;
			this.placeStyle = placeStyle;
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<EnemyBanner>(), placeStyle);
			Item.width = 10;
			Item.height = 24;
			Item.SetShopValues(ItemRarityColor.Blue1, Item.buyPrice(silver: 10));
		}
	}
	*/
}
