using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.ModLoader;
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
		// not-yet-implemented
		var rasterizer = Main.Rasterizer;
		// instead-expect
#if COMPILE_ERROR
		var rasterizer = Main.instance.Rasterizer;
#endif

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

#if COMPILE_ERROR
		if (Main.fastForwardTime/* tModPorter Note: Removed. Suggestion: IsFastForwardingTime(), fastForwardTimeToDawn or fastForwardTimeToDusk */) { }
#endif

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
		var doubleJumpBlizzard = player.GetJumpState(ExtraJump.BlizzardInABottle).Enabled;
		var doubleJumpCloud = player.GetJumpState(ExtraJump.CloudInABottle).Enabled;
		var doubleJumpFart = player.GetJumpState(ExtraJump.FartInAJar).Enabled;
		var doubleJumpSail = player.GetJumpState(ExtraJump.TsunamiInABottle).Enabled;
		var doubleJumpSandstorm = player.GetJumpState(ExtraJump.SandstormInABottle).Enabled;
		var doubleJumpUnicorn = player.GetJumpState(ExtraJump.UnicornMount).Enabled;
		// not-yet-implemented
		var hasBanner = Main.SceneMetrics.hasBanner;
		var bannerBuff = Main.SceneMetrics.NPCBannerBuff;
		var extraAccessorySlots = player.GetAmountOfExtraAccessorySlotsToShow();
		// instead-expect
#if COMPILE_ERROR
		var hasBanner = player.hasBanner;
		var bannerBuff = player.NPCBannerBuff;
		var extraAccessorySlots = player.extraAccessorySlots;
#endif
		var thrownCost33 = player.ThrownCost33;
		var thrownCost50 = player.ThrownCost50;
		var thrownVelocity = player.ThrownVelocity;
		var discount = player.discountAvailable;
		player.IsItemSlotUnlockedAndUsable(0);
#if COMPILE_ERROR
		player.VanillaUpdateEquip(null)/* tModPorter Note: Removed. Use either GrantPrefixBenefits (if Item.accessory) or GrantArmorBenefits (for armor slots) */;
#endif
		player.CanAfford(100000);

		// not-yet-implemented
		Main.PlayerRenderer.DrawPlayer(Main.Camera, player, Vector2.Zero, 0f, Vector2.Zero, 1f);
		// instead-expect
#if COMPILE_ERROR
		Main.DrawPlayer(player, Vector2.Zero, 0f, Vector2.Zero, 1f);
#endif

		var item = new Item();
		var owner = item.playerIndexTheItemIsReservedFor;
		var vanity = item.hasVanityEffects;
		item.DefaultToPlaceableWall(0);

		var item2 = new Item();
		// not-yet-implemented
		var isTheSameAs = item.type == item2.type;
		var isTheSameAsExpression = (1 > 2 ? item : item2).type == (1 > 2 ? item2 : item).type;
		var isTheSameAsNegated = item.type != item2.type;
		var isTheSameAsNegatedVariant = item.type != item2.type;
		// instead-expect
#if COMPILE_ERROR
		var isTheSameAs = item.IsTheSameAs(item2);
		var isTheSameAsExpression = (1 > 2 ? item : item2).IsTheSameAs((1 > 2 ? item2 : item));
		var isTheSameAsNegated = !item.IsTheSameAs(item2);
		var isTheSameAsNegatedVariant = item.IsTheSameAs(item2) == false;
#endif
		var isNotTheSameAs = item.IsNotSameTypePrefixAndStack(item2);

		NPC npc = new NPC();
		// not-yet-implemented
		npc.damage = npc.GetAttackDamage_ScaledByStrength(80f); // int cast matches return type
		// instead-expect
		npc.damage = (int)(80f * Main.GameModeInfo.EnemyDamageMultiplier); // int cast matches return type
#if COMPILE_ERROR
#endif

		// not-yet-implemented
		Utils.TileActionAttempt cut = DelegateMethods.CutTiles;
		// instead-expect
#if COMPILE_ERROR
		Utils.PerLinePoint cut = DelegateMethods.CutTiles;
#endif
	}
}