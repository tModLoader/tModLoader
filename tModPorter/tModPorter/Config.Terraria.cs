using static tModPorter.Rewriters.RenameRewriter;
using static tModPorter.Rewriters.InvokeRewriter;
using static tModPorter.Rewriters.MemberUseRewriter;
using static tModPorter.Rewriters.AddComment;
using System;

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

		RenameStaticField("Terraria.ID.DustID", from: "Fire", to: "Torch");

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
		RefactorInstanceMember("Terraria.Player",		"magicDamage",		DamageModifier("Magic",		"GetDamage"));
		RefactorInstanceMember("Terraria.Player",		"minionDamage",		DamageModifier("Summon",	"GetDamage"));
		RefactorInstanceMember("Terraria.Player",		"thrownDamage",		DamageModifier("Throwing",	"GetDamage"));
		RefactorInstanceMember("Terraria.Player",		"allDamageMult",	DamageModifier("Generic",	"GetDamage"));
		RefactorInstanceMember("Terraria.Player",		"meleeDamageMult",	DamageModifier("Melee",		"GetDamage"));
		RefactorInstanceMember("Terraria.Player",		"rangedDamageMult",	DamageModifier("Ranged",	"GetDamage"));
		RefactorInstanceMember("Terraria.Player",		"magicDamageMult",	DamageModifier("Magic",		"GetDamage"));
		RefactorInstanceMember("Terraria.Player",		"minionDamageMult",	DamageModifier("Summon",	"GetDamage"));
		RefactorInstanceMember("Terraria.Player",		"thrownDamageMult",	DamageModifier("Throwing",	"GetDamage"));
		RefactorInstanceMember("Terraria.Player",		"meleeCrit",		DamageModifier("Generic",	"GetCritChance"));
		RefactorInstanceMember("Terraria.Player",		"rangedCrit",		DamageModifier("Ranged",	"GetCritChance"));
		RefactorInstanceMember("Terraria.Player",		"magicCrit",		DamageModifier("Magic",		"GetCritChance"));
		RefactorInstanceMember("Terraria.Player",		"thrownCrit",		DamageModifier("Throwing",	"GetCritChance"));
		RefactorInstanceMember("Terraria.Player",		"meleeSpeed",		DamageModifier("Melee",		"GetAttackSpeed"));
		RefactorInstanceMember("Terraria.Player",		"minionKB",			DamageModifier("Summon",	"GetKnockback", "Base"));
		RefactorInstanceMember("Terraria.Player",		"armorPenetration",	DamageModifier("Generic",	"GetArmorPenetration"));
		RefactorInstanceMember("Terraria.Player",		"whipUseTimeMultiplier",	DamageModifier("SummonMeleeSpeed",	"GetAttackSpeed"));

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

		RenameMethod("Terraria.Item",		from: "IsNotTheSameAs",			to: "IsNotSameTypePrefixAndStack");
		RenameMethod("Terraria.Lighting",	from: "BlackOut",				to: "Clear");
		RenameMethod("Terraria.Utils",		from: "InverseLerp",			to: "GetLerpValue");
		RenameMethod("Terraria.NetMessage",	from: "BroadcastChatMessage",	to: "BroadcastChatMessage",		newType: "Terraria.Chat.ChatHelper");

		RenameMethod("Terraria.Main",		from: "PlayTrackedSound",		to: "PlaySound");
		// sound refactors don't rename to SoundEngine because the fact that it's on Main lets us handle SoundID.Roar -> WormDig and SoundID.Splash -> SplashWeak properly (style 1 default)
		RefactorStaticMethodCall("Terraria.Main",				"PlaySound", ConvertPlaySound(onMain: true));
		RefactorStaticMethodCall("Terraria.Audio.SoundEngine",	"PlaySound", ConvertPlaySound(onMain: false));

		RenameMethod("Terraria.Main", from: "GetActiveSound", to: "GetActiveSound", newType: "Terraria.Audio.SoundEngine");
		RefactorStaticMethodCall("Terraria.Audio.SoundEngine", "GetActiveSound", ToTryGet("TryGetActiveSound"));
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
}

