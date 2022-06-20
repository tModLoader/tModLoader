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

		var expertDebuffTime = Main.GameModeInfo.DebuffTimeMultiplier;
		var expertNPCDamage = Main.GameModeInfo.TownNPCDamageMultiplier;
		var expertLife = Main.GameModeInfo.EnemyMaxLifeMultiplier;
		var expertDamage = Main.GameModeInfo.EnemyDamageMultiplier;
		var expertKnockBack = Main.GameModeInfo.KnockbackToEnemiesMultiplier;
		var knockBackMultiplier = Main.GameModeInfo.KnockbackToEnemiesMultiplier;
		var damageMultiplier = Main.GameModeInfo.EnemyDamageMultiplier;

		int copperTierOreInt = WorldGen.SavedOreTiers.Copper;
#if COMPILE_ERROR // ushort -> int
		ushort copperTierOre = WorldGen.SavedOreTiers.Copper;
		ushort ironTierOre = WorldGen.SavedOreTiers.Iron;
		ushort silverTierOre = WorldGen.SavedOreTiers.Silver;
		ushort goldTierOre = WorldGen.SavedOreTiers.Gold;
#endif
		int oreTier1 = WorldGen.SavedOreTiers.Cobalt;
		int oreTier2 = WorldGen.SavedOreTiers.Mythril;
		int oreTier3 = WorldGen.SavedOreTiers.Adamantite;

		float inverseLerp = Utils.GetLerpValue(0f, 1f, 0.1f, false);
		Lighting.Clear();
		ChatHelper.BroadcastChatMessage(null, Color.White, -1);

		int dustFire = DustID.Torch;

		int water = LiquidID.Water;
		int honey = LiquidID.Honey;
		int lava = LiquidID.Lava;

		// Yes. The variables are named with opposing sides in 1.3, the underlying values are the same
#if COMPILE_ERROR // int -> BlockType
		int type_Solid = BlockType.Solid;
		int type_Halfbrick = BlockType.HalfBlock;
		int type_SlopeDownRight = BlockType.SlopeDownLeft;
		int type_SlopeDownLeft = BlockType.SlopeDownRight;
		int type_SlopeUpRight = BlockType.SlopeUpLeft;
		int type_SlopeUpLeft = BlockType.SlopeUpRight;
#endif

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
		var thrownCost33 = player.ThrownCost33;
		var thrownCost50 = player.ThrownCost50;
		var thrownVelocity = player.ThrownVelocity;

		Main.PlayerRenderer.DrawPlayer(Main.Camera, player, Vector2.Zero, 0f, Vector2.Zero, 1f);

		var item = new Item();
		var owner = item.playerIndexTheItemIsReservedFor;

		var item2 = new Item();
		var isTheSameAs = item.type == item2.type;
		var isTheSameAsExpression = (1 > 2 ? item : item2).type == (1 > 2 ? item2 : item).type;
		var isTheSameAsNegated = item.type != item2.type;
		var isTheSameAsNegatedVariant = item.type != item2.type;
		var isNotTheSameAs = item.IsNotSameTypePrefixAndStack(item2);

		NPC npc = new NPC();
		npc.damage = npc.GetAttackDamage_ScaledByStrength(80f); // int cast matches return type

		Utils.TileActionAttempt cut = DelegateMethods.CutTiles;
	}
}