using Terraria;
using Terraria.ID;
using Terraria.ObjectData;
using Microsoft.Xna.Framework;

public class SimpleRenamedVanillaMembersTest
{
	void Method() {
		var dresserX = Main.dresserX;
		var dresserY = Main.dresserY;
		var activePlayerCount = Main.ActivePlayerCount;
		var globalTime = Main.GlobalTime;
		var itemLockoutTime = Main.itemLockoutTime;
		var maxInventory = Main.maxInventory;
		var quickBG = Main.quickBG;
		var smartCursorEnabled = Main.SmartCursorEnabled; // get only property in 1.4
		var tileValue = Main.tileValue;
		var worldRate = Main.worldRate;
		var lightMode = Lighting.lightMode;
		var technicallyABoss = NPCID.Sets.TechnicallyABoss;
		var homing = ProjectileID.Sets.Homing;
		var rasterizer = Main.instance.Rasterizer;

		var waterCandles = Main.waterCandles;
		var peaceCandles = Main.peaceCandles;
		var partyMonoliths = Main.partyMonoliths;
		var evilTiles = Main.evilTiles;
		var holyTiles = Main.holyTiles;
		var meteorTile = Main.meteorTile;
		var jungleTiles = Main.jungleTiles;
		var snowTiles = Main.snowTiles;
		var bloodTiles = Main.bloodTiles;
		var sandTiles = Main.sandTiles;
		var shroomTiles = Main.shroomTiles;
		var dungeonTiles = Main.dungeonTiles; // private set in 1.4

		var monolithType = Main.monolithType; // private set in 1.4
		var clock = Main.clock;
		var campfire = Main.campfire;
		var starInBottle = Main.starInBottle;
		var heartLantern = Main.heartLantern;
		var sunflower = Main.sunflower;

		float inverseLerp = Utils.InverseLerp(0f, 1f, 0.1f, false);
		Lighting.BlackOut();
		NetMessage.BroadcastChatMessage(null, Color.White, -1);

		int water = Tile.Liquid_Water;
		int honey = Tile.Liquid_Honey;
		int lava = Tile.Liquid_Lava;

		// Yes. The variables are named with opposing sides in 1.3, the underlying values are the same
		int type_Solid = Tile.Type_Solid;
		int type_Halfbrick = Tile.Type_Halfbrick;
		int type_SlopeDownRight = Tile.Type_SlopeDownRight;
		int type_SlopeDownLeft = Tile.Type_SlopeDownLeft;
		int type_SlopeUpRight = Tile.Type_SlopeUpRight;
		int type_SlopeUpLeft = Tile.Type_SlopeUpLeft;

		var tileObjectData = new TileObjectData();
		var hookCheck = tileObjectData.HookCheck;

		var player = new Player();
		var hideVisual = player.hideVisual;
		var showItemIcon = player.showItemIcon;
		var hideshowItemIcon2 = player.showItemIcon2;
		var showItemIconText = player.showItemIconText;
		var zoneHoly = player.ZoneHoly;
		var doubleJumpBlizzard = player.doubleJumpBlizzard;
		var doubleJumpCloud = player.doubleJumpCloud;
		var doubleJumpFart = player.doubleJumpFart;
		var doubleJumpSail = player.doubleJumpSail;
		var doubleJumpSandstorm = player.doubleJumpSandstorm;
		var doubleJumpUnicorn = player.doubleJumpUnicorn;
		var hasBanner = player.hasBanner;
		var bannerBuff = player.NPCBannerBuff;
		var extraAccessorySlots = player.extraAccessorySlots;

		Main.DrawPlayer(player, Vector2.Zero, 0f, Vector2.Zero, 1f);

		var item = new Item();
		var owner = item.owner;

		var item2 = new Item();
		var isTheSameAs = item.IsTheSameAs(item2);
		var isTheSameAsNegated = !item.IsTheSameAs(item2);
		var isTheSameAsNegatedVariant = item.IsTheSameAs(item2) == false;
		var isNotTheSameAs = item.IsNotTheSameAs(item2);
	}
}