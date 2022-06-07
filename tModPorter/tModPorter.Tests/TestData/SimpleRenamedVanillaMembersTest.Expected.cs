using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.ObjectData;
using Microsoft.Xna.Framework;

public class SimpleRenamedVanillaMembersTest
{
	void Method() {
		var dresserX = Main.interactedDresserTopLeftX;
		var dresserY = Main.interactedDresserTopLeftY;
		var activePlayerCount = Main.CurrentFrameFlags.ActivePlayersCount;
		var globalTime = Main.GlobalTimeWrappedHourly;
		var itemLockoutTime = Main.timeItemSlotCannotBeReusedFor;
		var maxInventory = Main.InventorySlotsTotal;
		var quickBG = Main.instantBGTransitionCounter;
		var smartCursorEnabled = Main.SmartCursorIsUsed; // get only property in 1.4
		var tileValue = Main.tileOreFinderPriority;
		var worldRate = Main.desiredWorldTilesUpdateRate;
		var lightMode = Lighting.LegacyEngine.Mode;
		var technicallyABoss = NPCID.Sets.ShouldBeCountedAsBoss;
		var homing = ProjectileID.Sets.CultistIsResistantTo;
		var rasterizer = Main.Rasterizer;

		var waterCandles = Main.SceneMetrics.WaterCandleCount;
		var peaceCandles = Main.SceneMetrics.PeaceCandleCount;
		var partyMonoliths = Main.SceneMetrics.PartyMonolithCount;
		var evilTiles = Main.SceneMetrics.EvilTileCount;
		var holyTiles = Main.SceneMetrics.HolyTileCount;
		var meteorTile = Main.SceneMetrics.MeteorTileCount;
		var jungleTiles = Main.SceneMetrics.JungleTileCount;
		var snowTiles = Main.SceneMetrics.SnowTileCount;
		var bloodTiles = Main.SceneMetrics.BloodTileCount;
		var sandTiles = Main.SceneMetrics.SandTileCount;
		var shroomTiles = Main.SceneMetrics.MushroomTileCount;
		var dungeonTiles = Main.SceneMetrics.DungeonTileCount; // private set in 1.4

		var monolithType = Main.SceneMetrics.ActiveMonolithType; // private set in 1.4
		var clock = Main.SceneMetrics.HasClock;
		var campfire = Main.SceneMetrics.HasCampfire;
		var starInBottle = Main.SceneMetrics.HasStarInBottle;
		var heartLantern = Main.SceneMetrics.HasHeartLantern;
		var sunflower = Main.SceneMetrics.HasSunflower;

		float inverseLerp = Utils.GetLerpValue(0f, 1f, 0.1f, false);
		Lighting.Clear();
		ChatHelper.BroadcastChatMessage(null, Color.White, -1);

		int water = LiquidID.Water;
		int honey = LiquidID.Honey;
		int lava = LiquidID.Lava;

		// Yes. The variables are named with opposing sides in 1.3, the underlying values are the same
		int type_Solid = (int)BlockType.Solid;
		int type_Halfbrick = (int)BlockType.HalfBlock;
		int type_SlopeDownRight = (int)BlockType.SlopeDownLeft;
		int type_SlopeDownLeft = (int)BlockType.SlopeDownRight;
		int type_SlopeUpRight = (int)BlockType.SlopeUpLeft;
		int type_SlopeUpLeft = (int)BlockType.SlopeUpRight;

		var tileObjectData = new TileObjectData();
		var hookCheck = tileObjectData.HookCheckIfCanPlace;

		var player = new Player();
		var hideVisual = player.hideVisibleAccessory;
		var showItemIcon = player.cursorItemIconEnabled;
		var hideshowItemIcon2 = player.cursorItemIconID;
		var showItemIconText = player.cursorItemIconText;
		var zoneHoly = player.ZoneHallow;
		var doubleJumpBlizzard = player.hasJumpOption_Blizzard;
		var doubleJumpCloud = player.hasJumpOption_Cloud;
		var doubleJumpFart = player.hasJumpOption_Fart;
		var doubleJumpSail = player.hasJumpOption_Sail;
		var doubleJumpSandstorm = player.hasJumpOption_Sandstorm;
		var doubleJumpUnicorn = player.hasJumpOption_Unicorn;
		var hasBanner = Main.SceneMetrics.hasBanner;
		var bannerBuff = Main.SceneMetrics.NPCBannerBuff;
		var extraAccessorySlots = player.GetAmountOfExtraAccessorySlotsToShow();

		Main.PlayerRenderer.DrawPlayer(Main.Camera, player, Vector2.Zero, 0f, Vector2.Zero, 1f);

		var item = new Item();
		var owner = item.playerIndexTheItemIsReservedFor;

		var item2 = new Item();
		var isTheSameAs = item.type == item2.type;
		var isTheSameAsNegated = item.type != item2.type;
		var isTheSameAsNegatedVariant = item.type != item2.type;
		var isNotTheSameAs = item.IsNotSameTypePrefixAndStack(item2);
	}
}