using System;
using System.Diagnostics.Metrics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using tModPorter.Rewriters;
using static tModPorter.Rewriters.AddComment;
using static tModPorter.Rewriters.MemberTypeRewriter;
using static tModPorter.Rewriters.InvokeRewriter;
using static tModPorter.Rewriters.MemberUseRewriter;
using static tModPorter.Rewriters.RenameRewriter;

namespace tModPorter;

public static partial class Config
{
	private static void AddTerrariaRefactors() {
		RenameNamespace(from: "Terraria.World.Generation", to: "Terraria.WorldBuilding");

		RenameInstanceField("Terraria.Tile", from: "frameX",		to: "TileFrameX");
		RenameInstanceField("Terraria.Tile", from: "frameY",		to: "TileFrameY");
		RenameInstanceField("Terraria.Tile", from: "liquid",		to: "LiquidAmount");
		RenameInstanceField("Terraria.Tile", from: "type",			to: "TileType");
		RenameInstanceField("Terraria.Tile", from: "wall",			to: "WallType");
		RenameInstanceField("Terraria.Tile", from: "wallFrameX",	to: "WallFrameX");
		RenameInstanceField("Terraria.Tile", from: "wallFrameY",	to: "WallFrameY");
		RenameInstanceField("Terraria.Item", from: "canBePlacedInVanityRegardlessOfConditions",	to: "hasVanityEffects");

		RefactorInstanceMethodCall("Terraria.Tile", "active",			GetterSetterToProperty("HasTile"));
		RefactorInstanceMethodCall("Terraria.Tile", "inActive",			GetterSetterToProperty("IsActuated"));
		RefactorInstanceMethodCall("Terraria.Tile", "actuator",			GetterSetterToProperty("HasActuator"));
		RefactorInstanceMethodCall("Terraria.Tile", "slope",			GetterSetterToProperty("Slope"));
		RefactorInstanceMethodCall("Terraria.Tile", "halfBrick",		GetterSetterToProperty("IsHalfBlock"));
		RefactorInstanceMethodCall("Terraria.Tile", "color",			GetterSetterToProperty("TileColor"));
		RefactorInstanceMethodCall("Terraria.Tile", "wallColor",		GetterSetterToProperty("WallColor"));
		RefactorInstanceMethodCall("Terraria.Tile", "frameNumber",		GetterSetterToProperty("TileFrameNumber"));
		RefactorInstanceMethodCall("Terraria.Tile", "wallFrameNumber",	GetterSetterToProperty("WallFrameNumber"));
		RefactorInstanceMethodCall("Terraria.Tile", "wallFrameX",		GetterSetterToProperty("WallFrameX"));
		RefactorInstanceMethodCall("Terraria.Tile", "wallFrameY",		GetterSetterToProperty("WallFrameY"));
		RefactorInstanceMethodCall("Terraria.Tile", "wire",				GetterSetterToProperty("RedWire"));
		RefactorInstanceMethodCall("Terraria.Tile", "wire2",			GetterSetterToProperty("BlueWire"));
		RefactorInstanceMethodCall("Terraria.Tile", "wire3",			GetterSetterToProperty("GreenWire"));
		RefactorInstanceMethodCall("Terraria.Tile", "wire4",			GetterSetterToProperty("YellowWire"));
		RefactorInstanceMethodCall("Terraria.Tile", "checkingLiquid",	GetterSetterToProperty("CheckingLiquid"));
		RefactorInstanceMethodCall("Terraria.Tile", "skipLiquid",		GetterSetterToProperty("SkipLiquid"));
		RefactorInstanceMethodCall("Terraria.Tile", "liquidType",		GetterSetterToProperty("LiquidType"));
		RefactorInstanceMethodCall("Terraria.Tile", "lava",				GetterSetterToProperty("LiquidType", "Terraria.ID.LiquidID", "Lava"));
		RefactorInstanceMethodCall("Terraria.Tile", "honey",			GetterSetterToProperty("LiquidType", "Terraria.ID.LiquidID", "Honey"));
		RefactorInstanceMethodCall("Terraria.Tile", "nactive",			GetterToProperty("HasUnactuatedTile"));
		RefactorInstanceMethodCall("Terraria.Tile", "blockType",		GetterToProperty("BlockType"));
		RefactorInstanceMethodCall("Terraria.Tile", "topSlope",			GetterToProperty("TopSlope"));
		RefactorInstanceMethodCall("Terraria.Tile", "bottomSlope",		GetterToProperty("BottomSlope"));
		RefactorInstanceMethodCall("Terraria.Tile", "leftSlope",		GetterToProperty("LeftSlope"));
		RefactorInstanceMethodCall("Terraria.Tile", "rightSlope",		GetterToProperty("RightSlope"));
		RefactorInstanceMethodCall("Terraria.Tile", "HasSameSlope",		ComparisonFunctionToPropertyEquality("BlockType"));
		RefactorInstanceMethodCall("Terraria.Tile", "isTheSameAs",		Comment("Suggestion: Read https://github.com/tModLoader/tModLoader/wiki/Update-Migration-Guide#tiles"));

		RenameStaticField("Terraria.ID.DustID",    from: "Fire",			to: "Torch");
		RenameStaticField("Terraria.ID.MessageID", from: "SendNPCBuffs",	to: "NPCBuffs");
		RenameStaticField("Terraria.ID.MessageID", from: "Unlock",			to: "LockAndUnlock");
		RenameStaticField("Terraria.ID.MessageID", from: "StartPlaying",	to: "InitialSpawn");
		RenameStaticField("Terraria.ID.MessageID", from: "SpawnBoss",		to: "SpawnBossUseLicenseStartEvent");
		RenameStaticField("Terraria.ID.MessageID", from: "Teleport",		to: "TeleportEntity");
		RenameStaticField("Terraria.ID.MessageID", from:"ClientHello",      to: "Hello");
		RenameStaticField("Terraria.ID.MessageID", from: "LoadPlayer",       to: "PlayerInfo");
		RenameStaticField("Terraria.ID.MessageID", from: "RequestWorldInfo", to: "RequestWorldData");
		RenameStaticField("Terraria.ID.MessageID", from: "RequestTileData",  to: "SpawnTileData");
		RenameStaticField("Terraria.ID.MessageID", from: "StatusText",       to: "StatusTextSize");
		RenameStaticField("Terraria.ID.MessageID", from: "FrameSection",     to: "TileFrameSection");
		RenameStaticField("Terraria.ID.MessageID", from: "SpawnPlayer",      to: "PlayerSpawn");
		RenameStaticField("Terraria.ID.MessageID", from: "PlayerHealth",     to: "PlayerLifeMana");
		RenameStaticField("Terraria.ID.MessageID", from: "TileChange",       to: "TileManipulation");
		RenameStaticField("Terraria.ID.MessageID", from: "MenuSunMoon",      to: "SetTime");
		RenameStaticField("Terraria.ID.MessageID", from: "ChangeDoor",       to: "ToggleDoorState");
		RenameStaticField("Terraria.ID.MessageID", from: "UnusedStrikeNPC",  to: "UnusedMeleeStrike");
		RenameStaticField("Terraria.ID.MessageID", from: "StrikeNPC",        to: "DamageNPC");
		RenameStaticField("Terraria.ID.MessageID", from: "PlayerPVP",        to: "TogglePVP");
		RenameStaticField("Terraria.ID.MessageID", from: "HealEffect",       to: "PlayerHeal");
		RenameStaticField("Terraria.ID.MessageID", from: "PlayerZone",       to: "SyncPlayerZone");
		RenameStaticField("Terraria.ID.MessageID", from: "ResetItemOwner",   to: "ReleaseItemOwnership");
		RenameStaticField("Terraria.ID.MessageID", from: "PlayerTalkingNPC", to: "SyncTalkNPC");
		RenameStaticField("Terraria.ID.MessageID", from: "ItemAnimation",    to: "ShotAnimationAndSound");
		RenameStaticField("Terraria.ID.MessageID", from: "MurderSomeoneElsesProjectile", to: "MurderSomeoneElsesPortal");

		RenameStaticField("Terraria.Tile", from: "Liquid_Water",		to: "Water",			newType: "Terraria.ID.LiquidID");
		RenameStaticField("Terraria.Tile", from: "Liquid_Honey",		to: "Honey",			newType: "Terraria.ID.LiquidID");
		RenameStaticField("Terraria.Tile", from: "Liquid_Lava",			to: "Lava",				newType: "Terraria.ID.LiquidID");
		RenameStaticField("Terraria.Tile", from: "Type_Solid",			to: "Solid",			newType: "Terraria.ID.BlockType");
		RenameStaticField("Terraria.Tile", from: "Type_Halfbrick",		to: "HalfBlock",		newType: "Terraria.ID.BlockType");
		RenameStaticField("Terraria.Tile", from: "Type_SlopeDownRight",	to: "SlopeDownLeft",	newType: "Terraria.ID.BlockType");
		RenameStaticField("Terraria.Tile", from: "Type_SlopeDownLeft",	to: "SlopeDownRight",	newType: "Terraria.ID.BlockType");
		RenameStaticField("Terraria.Tile", from: "Type_SlopeUpRight",	to: "SlopeUpLeft",		newType: "Terraria.ID.BlockType");
		RenameStaticField("Terraria.Tile", from: "Type_SlopeUpLeft",	to: "SlopeUpRight",		newType: "Terraria.ID.BlockType");

		RefactorInstanceMember("Terraria.Item",			"melee",			DamageTypeField("Melee", "Suggestion: Consider MeleeNoSpeed for no attack speed scaling"));
		RefactorInstanceMember("Terraria.Item",			"ranged",			DamageTypeField("Ranged"));
		RefactorInstanceMember("Terraria.Item",			"magic",			DamageTypeField("Magic"));
		RefactorInstanceMember("Terraria.Item",			"summon",			DamageTypeField("Summon"));
		RefactorInstanceMember("Terraria.Item",			"thrown",			DamageTypeField("Throwing"));
		RefactorInstanceMember("Terraria.Projectile",	"melee",			DamageTypeField("Melee"));
		RefactorInstanceMember("Terraria.Projectile",	"ranged",			DamageTypeField("Ranged"));
		RefactorInstanceMember("Terraria.Projectile",	"magic",			DamageTypeField("Magic"));
		RefactorInstanceMember("Terraria.Projectile",	"thrown",			DamageTypeField("Throwing"));
		RefactorInstanceMember("Terraria.Player",		"allDamage",		DamageModifier("Generic",	"GetDamage"));
		RefactorInstanceMember("Terraria.Player",		"meleeDamage",		DamageModifier("Melee",		"GetDamage"));
		RefactorInstanceMember("Terraria.Player",		"rangedDamage",		DamageModifier("Ranged",	"GetDamage"));
		RenameInstanceField("Terraria.Player",			"rocketDamage",		"specialistDamage");
		RefactorInstanceMember("Terraria.Player",		"magicDamage",		DamageModifier("Magic",		"GetDamage"));
		RefactorInstanceMember("Terraria.Player",		"minionDamage",		DamageModifier("Summon",	"GetDamage"));
		RefactorInstanceMember("Terraria.Player",		"thrownDamage",		DamageModifier("Throwing",	"GetDamage"));
		RefactorInstanceMember("Terraria.Player",		"allDamageMult",	DamageModifier("Generic",	"GetDamage"));
		RefactorInstanceMember("Terraria.Player",		"meleeDamageMult",	DamageModifier("Melee",		"GetDamage"));
		RefactorInstanceMember("Terraria.Player",		"rangedDamageMult",	DamageModifier("Ranged",	"GetDamage"));
		RefactorInstanceMember("Terraria.Player",		"magicDamageMult",	DamageModifier("Magic",		"GetDamage"));
		RefactorInstanceMember("Terraria.Player",		"minionDamageMult",	DamageModifier("Summon",	"GetDamage"));
		RefactorInstanceMember("Terraria.Player",		"thrownDamageMult",	DamageModifier("Throwing",	"GetDamage"));
		RefactorInstanceMember("Terraria.Player",		"allCrit",			DamageModifier("Generic",	"GetCritChance"));
		RefactorInstanceMember("Terraria.Player",		"meleeCrit",		DamageModifier("Melee",		"GetCritChance"));
		RefactorInstanceMember("Terraria.Player",		"rangedCrit",		DamageModifier("Ranged",	"GetCritChance"));
		RefactorInstanceMember("Terraria.Player",		"magicCrit",		DamageModifier("Magic",		"GetCritChance"));
		RefactorInstanceMember("Terraria.Player",		"thrownCrit",		DamageModifier("Throwing",	"GetCritChance"));
		RefactorInstanceMember("Terraria.Player",		"meleeSpeed",		DamageModifier("Melee",		"GetAttackSpeed"));
		RefactorInstanceMember("Terraria.Player",		"minionKB",			DamageModifier("Summon",	"GetKnockback", "Base"));
		RefactorInstanceMember("Terraria.Player",		"armorPenetration",	DamageModifier("Generic",	"GetArmorPenetration"));
		RefactorInstanceMember("Terraria.Player",		"whipUseTimeMultiplier",	DamageModifier("SummonMeleeSpeed",	"GetAttackSpeed"));

		RefactorInstanceMethodCall("Terraria.Player", "VanillaUpdateEquip",	Removed("Use either GrantPrefixBenefits (if Item.accessory) or GrantArmorBenefits (for armor slots)"));
		RenameMethod("Terraria.Player", "CanBuyItem", "CanAfford");

		RefactorInstanceMember("Terraria.Main", "fastForwardTime", Removed("Suggestion: IsFastForwardingTime(), fastForwardTimeToDawn or fastForwardTimeToDusk"));

		RenameStaticField("Terraria.ID.ItemUseStyleID", from: "HoldingUp",	to: "HoldUp");
		RenameStaticField("Terraria.ID.ItemUseStyleID", from: "HoldingOut",	to: "Shoot");
		RenameStaticField("Terraria.ID.ItemUseStyleID", from: "SwingThrow",	to: "Swing");
		RenameStaticField("Terraria.ID.ItemUseStyleID", from: "EatingUsing",to: "EatFood");
		RenameStaticField("Terraria.ID.ItemUseStyleID", from: "Stabbing",	to: "Thrust");

		RenameStaticField("Terraria.ID.NPCID.Sets",			from: "TechnicallyABoss",	to: "ShouldBeCountedAsBoss");
		RenameStaticField("Terraria.ID.ProjectileID.Sets",	from: "Homing",				to: "CultistIsResistantTo");

		RenameStaticField("Terraria.Main",		from: "dresserX",			to: "interactedDresserTopLeftX");
		RenameStaticField("Terraria.Main",		from: "dresserY",			to: "interactedDresserTopLeftY");
		RenameStaticField("Terraria.Main",		from: "ActivePlayerCount",	to: "CurrentFrameFlags.ActivePlayersCount");
		RenameStaticField("Terraria.Main",		from: "GlobalTime",			to: "GlobalTimeWrappedHourly");
		RenameStaticField("Terraria.Main",		from: "itemLockoutTime",	to: "timeItemSlotCannotBeReusedFor");
		RenameStaticField("Terraria.Main",		from: "maxInventory",		to: "InventorySlotsTotal");
		RenameStaticField("Terraria.Main",		from: "quickBG",			to: "instantBGTransitionCounter");
		RenameStaticField("Terraria.Main",		from: "SmartCursorEnabled",	to: "SmartCursorIsUsed");
		RenameStaticField("Terraria.Main",		from: "tileValue",			to: "tileOreFinderPriority");
		RenameStaticField("Terraria.Main",		from: "worldRate",			to: "desiredWorldTilesUpdateRate");
		RenameStaticField("Terraria.Main",		from: "waterCandles",		to: "SceneMetrics.WaterCandleCount");
		RenameStaticField("Terraria.Main",		from: "peaceCandles",		to: "SceneMetrics.PeaceCandleCount");
		RenameStaticField("Terraria.Main",		from: "partyMonoliths",		to: "SceneMetrics.PartyMonolithCount");
		RenameStaticField("Terraria.Main",		from: "evilTiles",			to: "SceneMetrics.EvilTileCount");
		RenameStaticField("Terraria.Main",		from: "holyTiles",			to: "SceneMetrics.HolyTileCount");
		RenameStaticField("Terraria.Main",		from: "meteorTile",			to: "SceneMetrics.MeteorTileCount");
		RenameStaticField("Terraria.Main",		from: "jungleTiles",		to: "SceneMetrics.JungleTileCount");
		RenameStaticField("Terraria.Main",		from: "snowTiles",			to: "SceneMetrics.SnowTileCount");
		RenameStaticField("Terraria.Main",		from: "bloodTiles",			to: "SceneMetrics.BloodTileCount");
		RenameStaticField("Terraria.Main",		from: "sandTiles",			to: "SceneMetrics.SandTileCount");
		RenameStaticField("Terraria.Main",		from: "shroomTiles",		to: "SceneMetrics.MushroomTileCount");
		RenameStaticField("Terraria.Main",		from: "dungeonTiles",		to: "SceneMetrics.DungeonTileCount");
		RenameStaticField("Terraria.Main",		from: "monolithType",		to: "SceneMetrics.ActiveMonolithType");
		RenameStaticField("Terraria.Main",		from: "clock",				to: "SceneMetrics.HasClock");
		RenameStaticField("Terraria.Main",		from: "campfire",			to: "SceneMetrics.HasCampfire");
		RenameStaticField("Terraria.Main",		from: "starInBottle",		to: "SceneMetrics.HasStarInBottle");
		RenameStaticField("Terraria.Main",		from: "heartLantern",		to: "SceneMetrics.HasHeartLantern");
		RenameStaticField("Terraria.Main",		from: "sunflower",			to: "SceneMetrics.HasSunflower");
		RenameStaticField("Terraria.Main",		from: "expertDebuffTime",	to: "GameModeInfo.DebuffTimeMultiplier");
		RenameStaticField("Terraria.Main",		from: "expertNPCDamage",	to: "GameModeInfo.TownNPCDamageMultiplier");
		RenameStaticField("Terraria.Main",		from: "expertLife",			to: "GameModeInfo.EnemyMaxLifeMultiplier");
		RenameStaticField("Terraria.Main",		from: "expertDamage",		to: "GameModeInfo.EnemyDamageMultiplier");
		RenameStaticField("Terraria.Main",		from: "expertKnockBack",	to: "GameModeInfo.KnockbackToEnemiesMultiplier");
		RenameStaticField("Terraria.Main",		from: "knockBackMultiplier",to: "GameModeInfo.KnockbackToEnemiesMultiplier");
		RenameStaticField("Terraria.Main",		from: "damageMultiplier",	to: "GameModeInfo.EnemyDamageMultiplier");
		RenameStaticField("Terraria.WorldGen",	from: "CopperTierOre",		to: "SavedOreTiers.Copper");
		RenameStaticField("Terraria.WorldGen",	from: "IronTierOre",		to: "SavedOreTiers.Iron");
		RenameStaticField("Terraria.WorldGen",	from: "SilverTierOre",		to: "SavedOreTiers.Silver");
		RenameStaticField("Terraria.WorldGen",	from: "GoldTierOre",		to: "SavedOreTiers.Gold");
		RenameStaticField("Terraria.WorldGen",	from: "oreTier1",			to: "SavedOreTiers.Cobalt");
		RenameStaticField("Terraria.WorldGen",	from: "oreTier2",			to: "SavedOreTiers.Mythril");
		RenameStaticField("Terraria.WorldGen",	from: "oreTier3",			to: "SavedOreTiers.Adamantite");

		RenameStaticField("Terraria.Lighting",	from: "lightMode",			to: "LegacyEngine.Mode");

		RenameInstanceField("Terraria.ObjectData.TileObjectData", from: "HookCheck", to: "HookCheckIfCanPlace");

		RenameInstanceField("Terraria.Item",	from: "owner",				to: "playerIndexTheItemIsReservedFor");
		RenameInstanceField("Terraria.Player",	from: "hideVisual",			to: "hideVisibleAccessory");
		RenameInstanceField("Terraria.Player",	from: "showItemIcon",		to: "cursorItemIconEnabled");
		RenameInstanceField("Terraria.Player",	from: "showItemIcon2",		to: "cursorItemIconID");
		RenameInstanceField("Terraria.Player",	from: "showItemIconText",	to: "cursorItemIconText");
		RenameInstanceField("Terraria.Player",	from: "ZoneHoly",			to: "ZoneHallow");
		RenameInstanceField("Terraria.Player",	from: "doubleJumpBlizzard",	to: "hasJumpOption_Blizzard");
		RenameInstanceField("Terraria.Player",	from: "doubleJumpCloud",	to: "hasJumpOption_Cloud");
		RenameInstanceField("Terraria.Player",	from: "doubleJumpFart",		to: "hasJumpOption_Fart");
		RenameInstanceField("Terraria.Player",	from: "doubleJumpSail",		to: "hasJumpOption_Sail");
		RenameInstanceField("Terraria.Player",	from: "doubleJumpSandstorm",to: "hasJumpOption_Sandstorm");
		RenameInstanceField("Terraria.Player",	from: "doubleJumpUnicorn",	to: "hasJumpOption_Unicorn");
		RenameInstanceField("Terraria.Player",	from: "thrownCost33",		to: "ThrownCost33");
		RenameInstanceField("Terraria.Player",	from: "thrownCost50",		to: "ThrownCost50");
		RenameInstanceField("Terraria.Player",	from: "thrownVelocity",		to: "ThrownVelocity");
		RenameInstanceField("Terraria.Player",	from: "discount",			to: "discountAvailable");

		RefactorInstanceMember("Terraria.Player",       "hasJumpOption_Cloud",              ExtraJumpField("CloudInABottle",     "Enabled"));
		RefactorInstanceMember("Terraria.Player",       "hasJumpOption_Sandstorm",          ExtraJumpField("SandstormInABottle", "Enabled"));
		RefactorInstanceMember("Terraria.Player",       "hasJumpOption_Blizzard",           ExtraJumpField("BlizzardInABottle",  "Enabled"));
		RefactorInstanceMember("Terraria.Player",       "hasJumpOption_Fart",               ExtraJumpField("FartInAJar",         "Enabled"));
		RefactorInstanceMember("Terraria.Player",       "hasJumpOption_Sail",               ExtraJumpField("TsunamiInABottle",   "Enabled"));
		RefactorInstanceMember("Terraria.Player",       "hasJumpOption_Unicorn",            ExtraJumpField("UnicornMount",       "Enabled"));
		RefactorInstanceMember("Terraria.Player",       "hasJumpOption_Santank",            ExtraJumpField("SantankMount",       "Enabled"));
		RefactorInstanceMember("Terraria.Player",       "hasJumpOption_WallOfFleshGoat",    ExtraJumpField("GoatMount",          "Enabled"));
		RefactorInstanceMember("Terraria.Player",       "hasJumpOption_Basilisk",           ExtraJumpField("BasiliskMount",      "Enabled"));
		RefactorInstanceMember("Terraria.Player",       "canJumpAgain_Cloud",               ExtraJumpField("CloudInABottle",     "Available"));
		RefactorInstanceMember("Terraria.Player",       "canJumpAgain_Sandstorm",           ExtraJumpField("SandstormInABottle", "Available"));
		RefactorInstanceMember("Terraria.Player",       "canJumpAgain_Blizzard",            ExtraJumpField("BlizzardInABottle",  "Available"));
		RefactorInstanceMember("Terraria.Player",       "canJumpAgain_Fart",                ExtraJumpField("FartInAJar",         "Available"));
		RefactorInstanceMember("Terraria.Player",       "canJumpAgain_Sail",                ExtraJumpField("TsunamiInABottle",   "Available"));
		RefactorInstanceMember("Terraria.Player",       "canJumpAgain_Unicorn",             ExtraJumpField("UnicornMount",       "Available"));
		RefactorInstanceMember("Terraria.Player",       "canJumpAgain_Santank",             ExtraJumpField("SantankMount",       "Available"));
		RefactorInstanceMember("Terraria.Player",       "canJumpAgain_WallOfFleshGoat",     ExtraJumpField("GoatMount",          "Available"));
		RefactorInstanceMember("Terraria.Player",       "canJumpAgain_Basilisk",            ExtraJumpField("BasiliskMount",      "Available"));
		RefactorInstanceMember("Terraria.Player",       "isPerformingJump_Cloud",           ExtraJumpField("CloudInABottle",     "Active"));
		RefactorInstanceMember("Terraria.Player",       "isPerformingJump_Sandstorm",       ExtraJumpField("SandstormInABottle", "Active"));
		RefactorInstanceMember("Terraria.Player",       "isPerformingJump_Blizzard",        ExtraJumpField("BlizzardInABottle",  "Active"));
		RefactorInstanceMember("Terraria.Player",       "isPerformingJump_Fart",            ExtraJumpField("FartInAJar",         "Active"));
		RefactorInstanceMember("Terraria.Player",       "isPerformingJump_Sail",            ExtraJumpField("TsunamiInABottle",   "Active"));
		RefactorInstanceMember("Terraria.Player",       "isPerformingJump_Unicorn",         ExtraJumpField("UnicornMount",       "Active"));
		RefactorInstanceMember("Terraria.Player",       "isPerformingJump_Santank",         ExtraJumpField("SantankMount",       "Active"));
		RefactorInstanceMember("Terraria.Player",       "isPerformingJump_WallOfFleshGoat", ExtraJumpField("GoatMount",          "Active"));
		RefactorInstanceMember("Terraria.Player",       "isPerformingJump_Basilisk",        ExtraJumpField("BasiliskMount",      "Active"));

		RenameMethod("Terraria.Item",		from: "IsNotTheSameAs",			to: "IsNotSameTypePrefixAndStack");
		RenameMethod("Terraria.Item",		from: "DefaultToPlacableWall",	to: "DefaultToPlaceableWall");
		RenameMethod("Terraria.Lighting",	from: "BlackOut",				to: "Clear");
		RenameMethod("Terraria.Utils",		from: "InverseLerp",			to: "GetLerpValue");
		RenameMethod("Terraria.NetMessage",	from: "BroadcastChatMessage",	to: "BroadcastChatMessage",		newType: "Terraria.Chat.ChatHelper");
		RenameMethod("Terraria.NetMessage",	from: "SendObjectPlacment",		to: "SendObjectPlacement");
		RenameMethod("Terraria.Player",		from: "IsAValidEquipmentSlotForIteration", to: "IsItemSlotUnlockedAndUsable");

		RenameMethod("Terraria.Main",		from: "PlayTrackedSound",		to: "PlaySound");
		// sound refactors don't rename to SoundEngine because the fact that it's on Main lets us handle SoundID.Roar -> WormDig and SoundID.Splash -> SplashWeak properly (style 1 default)
		RefactorStaticMethodCall("Terraria.Main",				"PlaySound", ConvertPlaySound(onMain: true));
		RefactorStaticMethodCall("Terraria.Audio.SoundEngine",	"PlaySound", ConvertPlaySound(onMain: false));

		RenameMethod("Terraria.Main", from: "GetActiveSound", to: "GetActiveSound", newType: "Terraria.Audio.SoundEngine");
		RefactorStaticMethodCall("Terraria.Audio.SoundEngine", "GetActiveSound", ToTryGet("TryGetActiveSound"));

		RenameType("Terraria.GameContent.UI.ResourceSets.HorizontalBarsPlayerReosurcesDisplaySet", "Terraria.GameContent.UI.ResourceSets.HorizontalBarsPlayerResourcesDisplaySet");

		RefactorStaticMember("Terraria.ID.TileID.Sets", "TouchDamageSands", Comment("Suggestion: Suffocate"));
		RefactorStaticMember("Terraria.ID.TileID.Sets", "TouchDamageOther", Comment("Suggestion: TouchDamageImmediate and possibly TouchDamageBleeding"));
		RefactorStaticMember("Terraria.ID.TileID.Sets", "TouchDamageVines", Comment("Suggestion: TouchDamageImmediate and TouchDamageDestroyTile"));


		RenameInstanceField("Terraria.UI.UIElement",	from: "OnMouseDown",	to: "OnLeftMouseDown");
		RenameInstanceField("Terraria.UI.UIElement",	from: "OnMouseUp",		to: "OnLeftMouseUp");
		RenameInstanceField("Terraria.UI.UIElement",	from: "OnClick",		to: "OnLeftClick");
		RenameInstanceField("Terraria.UI.UIElement",	from: "OnDoubleClick",	to: "OnLeftDoubleClick");
		RenameMethod("Terraria.UI.UIElement",			from: "MouseDown",		to: "LeftMouseDown");
		RenameMethod("Terraria.UI.UIElement",			from: "MouseUp",		to: "LeftMouseUp");
		RenameMethod("Terraria.UI.UIElement",			from: "Click",			to: "LeftClick");
		RenameMethod("Terraria.UI.UIElement",			from: "DoubleClick",	to: "LeftDoubleClick");

		AddWorldGenToGenVars();

		RenameInstanceField("Terraria.DataStructures.EntitySource_BossSpawn", "Entity", "Target");
		RenameInstanceField("Terraria.DataStructures.EntitySource_FishedOut", "Entity", "Fisher");
		RenameInstanceField("Terraria.DataStructures.EntitySource_OnHit", "EntityStriking", "Attacker");
		RenameInstanceField("Terraria.DataStructures.EntitySource_OnHit", "EntityStruck", "Victim");

		// 1.4.5                
		RenameStaticField("Terraria.ID.BuffID.Sets", from: "LongerExpertDebuff", to: "BuffTimeIsExtendedWithGameDifficulty");
		RenameStaticField("Terraria.ID.GoreID.Sets", from: "LiquidDroplet", to: "IsDrip");
		RenameStaticField("Terraria.ID.ImmunityCooldownID", from: "Bosses", to: "BossNoCheese");
		RenameStaticField("Terraria.ID.ItemID.Sets", from: "SortingPriorityBossSpawns", to: "SortingPriorityMiscImportants");
		RenameStaticField("Terraria.ID.ItemID.Sets", from: "BonusAttackSpeedMultiplier", to: "BonusMeleeSpeedMultiplier");
		RenameStaticField("Terraria.ID.MessageID", from: "TileSquare", to: "AreaTileChange");
		RenameStaticField("Terraria.ID.MessageID", from: "ShotAnimationAndSound", to: "ItemRotationAndAnimation");
		RenameStaticField("Terraria.ID.MessageID", from: "PlayerTeam", to: "TeamChange");
		RenameStaticField("Terraria.ID.MessageID", from: "RequestReadSign", to: "OpenSignRequest");
		RenameStaticField("Terraria.ID.MessageID", from: "ReadSign", to: "OpenSignResponse");
		RenameStaticField("Terraria.ID.MessageID", from: "AddPlayerBuff", to: "AddPlayerBuffPvP");
		RenameStaticField("Terraria.ID.MessageID", from: "PaintTile", to: "SyncTilePaintOrCoating");
		RenameStaticField("Terraria.ID.MessageID", from: "PaintWall", to: "SyncWallPaintOrCoating");
		RenameStaticField("Terraria.ID.MessageID", from: "NPCKillCountDeathTally", to: "Unused83");
		RenameStaticField("Terraria.ID.MessageID", from: "TEDisplayDollItemSync", to: "TEDisplayDollDataSync");
		RenameStaticFields("Terraria.ID.MusicID", "Night", "OverworldNight", "Title", "TitleClassic", "Jungle", "JungleDay", "TheHallow", "Hallow", "Space", "SpaceNight", "Boss4", "Golem", "AltOverworldDay", "OverworldDayAlt", "Ocean", "OceanDay", "RainSoundEffect", "RainAmbience", "Mushrooms", "Mushroom", "AltUnderground", "UndergroundAlt", "TheTowers", "LunarPillars", "Hell", "Underworld", "LunarBoss", "MoonLord", "GoblinInvasion", "GoblinArmy", "DayRemix", "OverworldDayRemix", "MenuMusic", "TitleJourneysBeginningWithIntro", "Monsoon", "Storm", "JungleUnderground", "UndergroundJungle", "ConsoleMenu", "TitleAlt", "OtherworldlyRain", "OtherworldRain", "OtherworldlyDay", "OtherworlddDay", "OtherworldlyNight", "OtherworldNight", "OtherworldlyUnderground", "OtherworldUnderground", "OtherworldlyDesert", "OtherworldDesert", "OtherworldlyOcean", "OtherworldOcean", "OtherworldlyMushrooms", "OtherworldMushroom", "OtherworldlyDungeon", "OtherworldDungeon", "OtherworldlySpace", "OtherworldSpace", "OtherworldlyUnderworld", "OtherworldUnderworld", "OtherworldlySnow", "OtherworldSnow", "OtherworldlyCorruption", "OtherworldCorruption", "OtherworldlyUGCorrption", "OtherworldUndergroundCorruption", "OtherworldlyCrimson", "OtherworldCrimson", "OtherworldlyUGCrimson", "OtherworldUndergroundCrimson", "OtherworldlyIce", "OtherworldIce", "OtherworldlyUGHallow", "OtherworldUndergroundHallow", "OtherworldlyEerie", "OtherworldEerie", "OtherworldlyBoss2", "OtherworldBoss2", "OtherworldlyBoss1", "OtherworldBoss1", "OtherworldlyInvasion", "OtherworldInvasion", "OtherworldlyTowers", "OtherworldLunarPillars", "OtherworldlyLunarBoss", "OtherworldMoonLord", "OtherworldlyPlantera", "OtherworldPlantera", "OtherworldlyJungle", "OtherworldJungle", "OtherworldlyWoF", "OtherworldWallOfFlesh", "OtherworldlyHallow", "OtherworldHallow", "Credits", "JourneysEnd", "Shimmer", "Aether");
		RenameStaticField("Terraria.ID.NPCID.Sets", from: "UsesNewTargetting", to: "UsesNewTargeting");
		RenameStaticField("Terraria.ID.NPCID.Sets", from: "GoldCrittersCollection", to: "IsGoldCritter").FollowBy(ConvertCollectionAddToSetTrue);
		RenameStaticField("Terraria.ID.NPCID.Sets", from: "ShouldBeCountedAsBoss", to: "ShouldBeCountedAsBossForBestiary");
		RenameStaticField("Terraria.ID.NPCID.Sets", from: "SpawnFromLastEmptySlot", to: "SearchSpawnSlotsInReverse");
		RenameStaticField("Terraria.ID.ProjectileID", from: "Web", to: "WebSlingerHook");
		RenameStaticField("Terraria.ID.ProjectileID.Sets", from: "MinionTargettingFeature", to: "MinionTargetingFeature");
		RenameStaticField("Terraria.ID.TileID.Sets", from: "CountsAsWaterSource", to: "CountsAsWaterForCrafting");
		RenameStaticField("Terraria.ID.TileID.Sets", from: "CountsAsHoneySource", to: "CountsAsHoneyForCrafting");
		RenameStaticField("Terraria.ID.TileID.Sets", from: "CountsAsLavaSource", to: "CountsAsLavaForCrafting");
		RenameStaticField("Terraria.ID.TileID.Sets", from: "CountsAsShimmerSource", to: "CountsAsShimmerForCrafting");
		RenameStaticField("Terraria.ID.TileID.Sets", from: "IsATrigger", to: "Wiring.IsATrigger");
		RenameStaticField("Terraria.ID.TileID.Sets", from: "IsAMechanism", to: "Wiring.IsAMechanism");
		RenameStaticField("Terraria.ID.TileID.Sets", from: "InteractibleByNPCs", to: "InteractableByNPCs");
		RenameStaticField("Terraria.ID.TileID.Sets", from: "Torch", to: "Torches");
		RenameStaticField("Terraria.ID.TileID.Sets", from: "Campfire", to: "Campfires");
		RenameStaticField("Terraria.ID.TileID.Sets", from: "WallsMergeWith", to: "TruncatesWalls");
		RenameStaticField("Terraria.ID.WallID.Sets", from: "Corrupt", to: "SpreadsCorruption");
		RenameStaticField("Terraria.ID.WallID.Sets", from: "Crimson", to: "SpreadsCrimson");
		RenameStaticField("Terraria.ID.WallID.Sets", from: "Hallow", to: "SpreadsHallow");
		RenameStaticField("Terraria.Main", from: "DisableIntenseVisualEffects", to: "FlashyEffectsWorld").FollowBy(InvertBool);
		RenameStaticField("Terraria.Main", "gameInactive", "GameplayActive", "Terraria.FocusHelper").FollowBy(InvertBool);
		RenameStaticField("Terraria.Main", "hasFocus", "AllowGameplayInputs", "Terraria.FocusHelper").FollowBy(AddCommentToFieldAccess("Suggestion: Also consider FocusHelper.AllowUIInputs, FocusHelper.UpdateVisualEffects, or others"));
		RenameStaticField("Terraria.Main", "LogicCheckScreenHeight", "MaxWorldViewSize.Y");
		RenameStaticField("Terraria.Main", "LogicCheckScreenWidth", "MaxWorldViewSize.X");
		RenameStaticField("Terraria.Main", "popupText", "popupText", "Terraria.PopupText");
		RenameStaticField("Terraria.Main", "recBigList", "PipsUseGrid");
		RenameStaticField("Terraria.Main", "recFastScroll", "PipsFastScroll");
		RenameStaticField("Terraria.NPC", "killCount", "killCount", "Terraria.GameContent.BannerSystem");
		RenameStaticField("Terraria.WorldGen", from: "gen", to: "isGeneratingOrLoadingWorld");

		RenameInstanceField("Terraria.Dust", from: "noLightEmittence", to: "noLightEmittance");
		RenameInstanceField("Terraria.Item", from: "netID", to: "type");
		RenameInstanceField("Terraria.Main", from: "HasInteractibleObjectThatIsNotATile", to: "HasInteractableObjectThatIsNotATile");
		RenameInstanceField("Terraria.Main.CurrentFrameFlags", from: "HadAnActiveInteractibleProjectile", to: "HadAnActiveInteractableProjectile");
		RenameInstanceFieldMultiple("Terraria.NPC.Spawner", froms: ["desertCave", "DesertCave"], to: "spawnUndergroundDesert");
		RenameInstanceFieldMultiple("Terraria.NPC.Spawner", froms: ["granite", "Granite"], to: "nearGranite");
		RenameInstanceFieldMultiple("Terraria.NPC.Spawner", froms: ["invasion", "Invasion"], to: "invaders");
		RenameInstanceFieldMultiple("Terraria.NPC.Spawner", froms: ["lihzahrd", "Lihzahrd"], to: "ZoneLihzhardTemple");
		RenameInstanceFieldMultiple("Terraria.NPC.Spawner", froms: ["marble", "Marble"], to: "nearMarble");
		RenameInstanceFieldMultiple("Terraria.NPC.Spawner", froms: ["planteraDefeated", "PlanteraDefeated"], to: "hardDungeon");
		RenameInstanceFieldMultiple("Terraria.NPC.Spawner", froms: ["player"], to: "Player");
		RenameInstanceFieldMultiple("Terraria.NPC.Spawner", froms: ["playerFloorX"], to: "PlayerFloorX");
		RenameInstanceFieldMultiple("Terraria.NPC.Spawner", froms: ["playerFloorY"], to: "PlayerFloorY");
		RenameInstanceFieldMultiple("Terraria.NPC.Spawner", froms: ["playerInTown", "PlayerInTown"], to: "spawnFriendly");
		RenameInstanceFieldMultiple("Terraria.NPC.Spawner", froms: ["playerSafe", "PlayerSafe"], to: "noWorms");
		RenameInstanceFieldMultiple("Terraria.NPC.Spawner", froms: ["safeRangeX"], to: "SafeRangeX");
		RenameInstanceFieldMultiple("Terraria.NPC.Spawner", froms: ["sky", "Sky"], to: "skyMob");
		RenameInstanceFieldMultiple("Terraria.NPC.Spawner", froms: ["spawnTileType"], to: "SpawnTileType");
		RenameInstanceFieldMultiple("Terraria.NPC.Spawner", froms: ["spawnTileX"], to: "SpawnTileX");
		RenameInstanceFieldMultiple("Terraria.NPC.Spawner", froms: ["spawnTileY"], to: "SpawnTileY");
		RenameInstanceFieldMultiple("Terraria.NPC.Spawner", froms: ["spiderCave", "SpiderCave"], to: "spawnSpider");
		RenameInstanceFieldMultiple("Terraria.NPC.Spawner", froms: ["water", "Water"], to: "waterTile");
		RenameInstanceField("Terraria.Player", from: "adjWater", to: "adjWaterSource");
		RenameInstanceField("Terraria.Player", from: "oldAdjWater", to: "oldAdjWaterSource");
		RenameInstanceField("Terraria.Player", from: "isPettingAnimal", to: "petting.isPetting");

		RenameMethod("Terraria.Item", "BannerToItem", "BannerToItem", "Terraria.GameContent.BannerSystem");
		RenameMethod("Terraria.Item", "BannerToNPC", "BannerToNPC", "Terraria.GameContent.BannerSystem");
		RenameMethod("Terraria.Item", "NPCtoBanner", "NPCtoBanner", "Terraria.GameContent.BannerSystem");
		RenameMethod("Terraria.Main", "DrawWindowsIMEPanel", "SetIMEPanelAnchor");
		RenameMethod("Terraria.Main", "ShouldShowInvisibleWalls", "ShouldShowInvisibleBlocksAndWalls");
		RenameMethod("Terraria.Player", "IsProjectileInteractibleAndInInteractionRange", "IsProjectileInteractableAndInInteractionRange");
		RenameMethod("Terraria.Localization.LocalizedText", from: "CanFormatWith", to: "ConditionsMetWith");
		RenameMethod("Terraria.Utils", from: "PlotTileArea", to: "FloodFillTile").FollowBy(AddCommentToMethodInvocation("Parameters have changed")); // No longer returns bool, parameters changed
		RenameMethod("Terraria.WorldGen", from: "CheckTight", to: "CheckStalactite");

		RefactorStaticMember("Terraria.ID.BuffID.Sets", "BasicMountData", Removed("Replace with BuffID.Sets.MountType[Type] = ModContent.MountType<MyMount>();"));
		RefactorStaticMember("Terraria.ID.ItemID.Sets", "ItemSpawnDecaySpeed", Removed("No longer used."));
		RefactorStaticMember("Terraria.ID.MountID.Sets", "FacePlayersVelocity", Removed("Now automatic for all minecarts"));
		RefactorStaticMember("Terraria.ID.ProjectileID.Sets", "HeldProjDoesNotUsePlayerGfxOffY", Removed("AI() should use master.RotatedRelativePoint(master.MountedCenter + ...) to position held projectiles"));
		RefactorStaticMember("Terraria.ID.ProjectileID.Sets", "DontAttachHideToAlpha", Removed("Now true by default. See Projectile.usesOwnerLight and Projectile.drawLayer for more details."));
		RefactorStaticMember("Terraria.Item", "whoAmI", Removed("Moved to WorldItem"));
		RefactorStaticMember("Terraria.Item", "beingGrabbed", Removed("Moved to WorldItem"));
		RefactorStaticMember("Terraria.Main", "GameModeInfo", RewriteIsJourneyMode);
		RefactorStaticMember("Terraria.Main", "musicBox2", Removed("Use Player.musicBox instead"));
		RefactorStaticMember("Terraria.NPC", "netSkip", Removed("No longer necessary when setting life <= 0 and was never necessary when setting active = false"));
		RefactorStaticMember("Terraria.Player", "RandomTeleportationAttemptSettings", Removed("Use Utils.RandomTeleportationAttemptSettings instead and populate all the relevant new fields"));

		RefactorInstanceMember("Terraria.NPC.Spawner", "PlayerFloorX", Removed("Player floor coordinates are no longer tracked by NPC.Spawner"));
		RefactorInstanceMember("Terraria.NPC.Spawner", "PlayerFloorY", Removed("Player floor coordinates are no longer tracked by NPC.Spawner"));
		RefactorInstanceMember("Terraria.Player", "oldAdjTile", Removed("No longer used."));

		RefactorStaticMethodCall("Terraria.Chest", "FindChestByGuessing", Removed("Use Chest.FindChest with the top left tile coordinate"));
		RefactorStaticMethodCall("Terraria.RecipeGroup", "RegisterGroup", Removed("Replace this and \"new RecipeGroup()\" with RecipeGroup.Register"));

		RefactorInstanceMethodCall("Terraria.Item", "SetDefaults", RemoveParameter(1, "noMatCheck", "bool"));
		RefactorInstanceMethodCall("Terraria.Player", "GetItem", RemoveParameter(0, "plr", "int"));
		RefactorInstanceMethodCall("Terraria.Player", "AddBuff", RemoveParameter(2, "quiet", "bool"));
		RefactorInstanceMethodCall("Terraria.Player", "AddBuff", RemoveParameter(3, "foodHack", "bool")); // Order seems to matter here
		RefactorInstanceMethodCall("Terraria.Tile", "water", GetterSetterToProperty("LiquidType", "Terraria.ID.LiquidID", "Water"));
		RefactorInstanceMethodCall("Terraria.Tile", "anyWater", GetterToProperty("HasWater"));
		RefactorInstanceMethodCall("Terraria.Tile", "anyLava", GetterToProperty("HasLava"));
		RefactorInstanceMethodCall("Terraria.Tile", "anyHoney", GetterToProperty("HasHoney"));
		RefactorInstanceMethodCall("Terraria.Tile", "anyShimmer", GetterToProperty("HasShimmer"));
		RefactorInstanceMethodCall("Terraria.Tile", "anyWire", GetterToProperty("HasWire"));
		RefactorInstanceMethodCall("Terraria.NPC", "ShouldBestiaryGirlBeLycantrope", ToStaticMethodCall("Terraria.NPC", "ShouldBestiaryGirlBeLycantrope"));
		RefactorInstanceMethodCall("Terraria.NPC", "SpawnWithHigherTime", Removed("No longer used."));
		RefactorInstanceMethodCall("Terraria.Player", "CheckForGoodTeleportationSpot", Removed("Use Utils.CheckForGoodTeleportationSpot instead."));
		RefactorInstanceMethodCall("Terraria.Recipe", "FindRecipes", Removed("No longer used."));

		ChangeStaticFieldType("Terraria.Main", "item",        from: "Terraria.Item", to: "Terraria.WorldItem");
		ChangeStaticFieldType("Terraria.Main", "ActiveItems", from: "Terraria.Item", to: "Terraria.WorldItem");

		RenameType("Terraria.ID.RecipeGroupID", "Terraria.ID.RecipeGroups");
	}

	private static void AddTextureRenames() {
		var getValue = AccessMember("Value");

		void RenameTextureAsset(string from, string to) =>	RenameStaticField("Terraria.Main", from, to, newType: "Terraria.GameContent.TextureAssets").FollowBy(getValue);
		void RenameFontAsset(string from, string to) =>		RenameStaticField("Terraria.Main", from, to, newType: "Terraria.GameContent.FontAssets").FollowBy(getValue);


		RenameFontAsset(from: "fontDeathText",	to: "DeathText");
		RenameFontAsset(from: "fontItemText",	to: "ItemStack");
		RenameFontAsset(from: "fontMouseText",	to: "MouseText");
		RenameFontAsset(from: "fontCombatText", to: "CombatText");

		RenameTextureAsset(from: "projectileTexture",	to: "Projectile");
		RenameTextureAsset(from: "itemTexture",			to: "Item");
		RenameTextureAsset(from: "npcTexture",			to: "Npc");
		RenameTextureAsset(from: "buffTexture",			to: "Buff");
		RenameTextureAsset(from: "goreTexture",			to: "Gore");
		RenameTextureAsset(from: "dustTexture",			to: "Dust");
		RenameTextureAsset(from: "tileTexture",			to: "Tile");
		RenameTextureAsset(from: "glowMaskTexture",		to: "GlowMask");
		RenameTextureAsset(from: "npcHeadTexture",		to: "NpcHead");
		RenameTextureAsset(from: "npcHeadBossTexture",	to: "NpcHeadBoss");
		RenameTextureAsset(from: "chainsTexture",		to: "Chains");
		RenameTextureAsset(from: "blackTileTexture",	to: "BlackTile");
		RenameTextureAsset(from: "magicPixel",			to: "MagicPixel");
		RenameTextureAsset(from: "fishingLineTexture",	to: "FishingLine");
		RenameTextureAsset(from: "wireUITexture",		to: "WireUi");
		RenameTextureAsset(from: "gemTexture",			to: "Gem");

		void RenameMultipleTextures(int n, Func<string, string> from, Func<string, string> to) {
			for (int i = 0; i <= n; i++) {
				var s = i > 1 ? i.ToString() : "";
				RenameTextureAsset(from(s), to(s));
			}
		}

		RenameMultipleTextures(4,	s => $"wire{s}Texture",				s => $"Wire{s}");
		RenameMultipleTextures(43,	s => $"chain{s}Texture",			s => $"Chain{s}");
		RenameMultipleTextures(18,	s => $"inventoryBack{s}Texture",	s => $"InventoryBack{s}");
		RenameMultipleTextures(3,	s => $"sun{s}Texture",				s => $"Sun{s}");
	}

	private static void AddWorldGenToGenVars() {
		static void RenameGenVar(string name) => RenameStaticField("Terraria.WorldGen", name, name, "Terraria.WorldBuilding.GenVars");

		RenameGenVar("configuration");
		RenameGenVar("structures");
		RenameGenVar("copper");
		RenameGenVar("iron");
		RenameGenVar("silver");
		RenameGenVar("gold");
		RenameGenVar("copperBar");
		RenameGenVar("ironBar");
		RenameGenVar("silverBar");
		RenameGenVar("goldBar");
		RenameGenVar("mossTile");
		RenameGenVar("mossWall");
		RenameGenVar("lavaLine");
		RenameGenVar("waterLine");
		RenameGenVar("worldSurfaceLow");
		RenameGenVar("worldSurface");
		RenameGenVar("worldSurfaceHigh");
		RenameGenVar("rockLayerLow");
		RenameGenVar("rockLayer");
		RenameGenVar("rockLayerHigh");
		RenameGenVar("snowTop");
		RenameGenVar("snowBottom");
		RenameGenVar("snowOriginLeft");
		RenameGenVar("snowOriginRight");
		RenameGenVar("snowMinX");
		RenameGenVar("snowMaxX");
		RenameGenVar("leftBeachEnd");
		RenameGenVar("rightBeachStart");
		RenameGenVar("beachBordersWidth");
		RenameGenVar("beachSandRandomCenter");
		RenameGenVar("beachSandRandomWidthRange");
		RenameGenVar("beachSandDungeonExtraWidth");
		RenameGenVar("beachSandJungleExtraWidth");
		RenameGenVar("shellStartXLeft");
		RenameGenVar("shellStartYLeft");
		RenameGenVar("shellStartXRight");
		RenameGenVar("shellStartYRight");
		RenameGenVar("oceanWaterStartRandomMin");
		RenameGenVar("oceanWaterStartRandomMax");
		RenameGenVar("oceanWaterForcedJungleLength");
		RenameGenVar("evilBiomeBeachAvoidance");
		RenameGenVar("evilBiomeAvoidanceMidFixer");
		RenameGenVar("lakesBeachAvoidance");
		RenameGenVar("smallHolesBeachAvoidance");
		RenameGenVar("surfaceCavesBeachAvoidance2");
		RenameGenVar("maxOceanCaveTreasure");
		RenameGenVar("numOceanCaveTreasure");
		RenameGenVar("oceanCaveTreasure");
		RenameGenVar("skipDesertTileCheck");
		RenameGenVar("UndergroundDesertLocation");
		RenameGenVar("UndergroundDesertHiveLocation");
		RenameGenVar("desertHiveHigh");
		RenameGenVar("desertHiveLow");
		RenameGenVar("desertHiveLeft");
		RenameGenVar("desertHiveRight");
		RenameGenVar("numLarva");
		RenameGenVar("larvaY");
		RenameGenVar("larvaX");
		RenameGenVar("numPyr");
		RenameGenVar("PyrX");
		RenameGenVar("PyrY");
		RenameGenVar("jungleOriginX");
		RenameGenVar("jungleMinX");
		RenameGenVar("jungleMaxX");
		RenameGenVar("JungleX");
		RenameGenVar("jungleHut");
		RenameGenVar("mudWall");
		RenameGenVar("JungleItemCount");
		RenameGenVar("JChestX");
		RenameGenVar("JChestY");
		RenameGenVar("numJChests");
		RenameGenVar("tLeft");
		RenameGenVar("tRight");
		RenameGenVar("tTop");
		RenameGenVar("tBottom");
		RenameGenVar("tRooms");
		RenameGenVar("lAltarX");
		RenameGenVar("lAltarY");
		RenameGenVar("dungeonSide");
		RenameGenVar("dungeonLocation");
		RenameGenVar("dungeonLake");
		RenameGenVar("crackedType");
		RenameGenVar("dungeonX");
		RenameGenVar("dungeonY");
		RenameGenVar("lastDungeonHall");
		RenameGenVar("maxDRooms");
		RenameGenVar("numDRooms");
		RenameGenVar("dRoomX");
		RenameGenVar("dRoomY");
		RenameGenVar("dRoomSize");
		RenameGenVar("dRoomTreasure");
		RenameGenVar("dRoomL");
		RenameGenVar("dRoomR");
		RenameGenVar("dRoomT");
		RenameGenVar("dRoomB");
		RenameGenVar("numDDoors");
		RenameGenVar("DDoorX");
		RenameGenVar("DDoorY");
		RenameGenVar("DDoorPos");
		RenameGenVar("numDungeonPlatforms");
		RenameGenVar("dungeonPlatformX");
		RenameGenVar("dungeonPlatformY");
		RenameGenVar("dEnteranceX");
		RenameGenVar("dSurface");
		RenameGenVar("dxStrength1");
		RenameGenVar("dyStrength1");
		RenameGenVar("dxStrength2");
		RenameGenVar("dyStrength2");
		RenameGenVar("dMinX");
		RenameGenVar("dMaxX");
		RenameGenVar("dMinY");
		RenameGenVar("dMaxY");
		RenameGenVar("skyLakes");
		RenameGenVar("generatedShadowKey");
		RenameGenVar("numIslandHouses");
		RenameStaticField("Terraria.WorldGen", "houseCount", "skyIslandHouseCount", "Terraria.WorldBuilding.GenVars");
		RenameGenVar("skyLake");
		RenameGenVar("floatingIslandHouseX");
		RenameGenVar("floatingIslandHouseY");
		RenameGenVar("floatingIslandStyle");
		RenameGenVar("numMCaves");
		RenameGenVar("mCaveX");
		RenameGenVar("mCaveY");
		RenameGenVar("maxTunnels");
		RenameGenVar("numTunnels");
		RenameGenVar("tunnelX");
		RenameGenVar("maxOrePatch");
		RenameGenVar("numOrePatch");
		RenameGenVar("orePatchX");
		RenameGenVar("maxMushroomBiomes");
		RenameGenVar("numMushroomBiomes");
		RenameGenVar("mushroomBiomesPosition");
		RenameGenVar("logX");
		RenameGenVar("logY");
		RenameGenVar("maxLakes");
		RenameGenVar("numLakes");
		RenameGenVar("LakeX");
		RenameGenVar("maxOasis");
		RenameGenVar("numOasis");
		RenameGenVar("oasisPosition");
		RenameGenVar("oasisWidth");
		RenameGenVar("oasisHeight");
		RenameGenVar("hellChest");
		RenameGenVar("hellChestItem");
		RenameGenVar("statueList");
		RenameGenVar("StatuesWithTraps");
	}
}
