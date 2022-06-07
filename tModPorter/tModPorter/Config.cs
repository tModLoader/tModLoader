using System.Collections.Generic;
using tModPorter.Rewriters;
using static tModPorter.Rewriters.RenameRewriter;
using static tModPorter.Rewriters.InvokeRewriter;
using static tModPorter.Rewriters.HookSignatureRewriter;

namespace tModPorter;

public static partial class Config
{
	public static List<BaseRewriter> CreateRewriters() => new() {
		new RenameRewriter(),
		new InvokeRewriter(),
		new HookSignatureRewriter(),
	};

	static Config() {
		RenameInstanceField("Terraria.ModLoader.ModType",		from: "mod",			to: "Mod");
		RenameInstanceField("Terraria.ModLoader.ModItem",		from: "item",			to: "Item");
		RenameInstanceField("Terraria.ModLoader.ModNPC",		from: "npc",			to: "NPC");
		RenameInstanceField("Terraria.ModLoader.ModPlayer",		from: "player",			to: "Player");
		RenameInstanceField("Terraria.ModLoader.ModProjectile",	from: "projectile",		to: "Projectile");
		RenameInstanceField("Terraria.ModLoader.ModMount",		from: "mountData",		to: "MountData");

		RenameInstanceField("Terraria.Item",			from: "modItem",		to: "ModItem");
		RenameInstanceField("Terraria.NPC",				from: "modNPC",			to: "ModNPC");
		RenameInstanceField("Terraria.Projectile",		from: "modProjectile",	to: "ModProjectile");
		RenameInstanceField("Terraria.Mount.MountData",	from: "modMountData",	to: "ModMount");
		RenameInstanceField("Terraria.Gore",			from: "modGore",		to: "ModGore");

		RenameType(from: "Terraria.ModLoader.ModMountData", to: "Terraria.ModLoader.ModMount");
		RenameType(from: "Terraria.ModLoader.ModWorld",		to: "Terraria.ModLoader.ModSystem");
		RenameType(from: "Terraria.ModLoader.ModHotKey",	to: "Terraria.ModLoader.ModKeybind");

		RenameInstanceField("Terraria.ModLoader.TooltipLine",	from: "text",			to: "Text");
		RenameInstanceField("Terraria.ModLoader.TooltipLine",	from: "mod",			to: "Mod");
		RenameInstanceField("Terraria.ModLoader.TooltipLine",	from: "isModifier",		to: "IsModifier");
		RenameInstanceField("Terraria.ModLoader.TooltipLine",	from: "isModifierBad",	to: "IsModifierBad");
		RenameInstanceField("Terraria.ModLoader.TooltipLine",	from: "overrideColor",	to: "OverrideColor");

		RenameInstanceField("Terraria.ModLoader.ModProjectile", from: "aiType",								  to: "AIType");
		RenameInstanceField("Terraria.ModLoader.ModProjectile", from: "cooldownSlot",						  to: "CooldownSlot");
		RenameInstanceField("Terraria.ModLoader.ModProjectile", from: "drawOffsetX",				 		  to: "DrawOffsetX");
		RenameInstanceField("Terraria.ModLoader.ModProjectile", from: "drawOriginOffsetY",					  to: "DrawOriginOffsetY");
		RenameInstanceField("Terraria.ModLoader.ModProjectile", from: "drawOriginOffsetX",					  to: "DrawOriginOffsetX");
		RenameInstanceField("Terraria.ModLoader.ModProjectile", from: "drawHeldProjInFrontOfHeldItemAndArms", to: "DrawHeldProjInFrontOfHeldItemAndArms");

		RenameInstanceField("Terraria.ModLoader.ModNPC", from: "aiType",		to: "AIType");
		RenameInstanceField("Terraria.ModLoader.ModNPC", from: "animationType", to: "AnimationType");
		RenameInstanceField("Terraria.ModLoader.ModNPC", from: "music",			to: "Music");
		RenameInstanceField("Terraria.ModLoader.ModNPC", from: "musicPriority", to: "SceneEffectPriority");
		RenameInstanceField("Terraria.ModLoader.ModNPC", from: "drawOffsetY",	to: "DrawOffsetY");
		RenameInstanceField("Terraria.ModLoader.ModNPC", from: "banner",		to: "Banner");
		RenameInstanceField("Terraria.ModLoader.ModNPC", from: "bannerItem",	to: "BannerItem");

		RenameInstanceField("Terraria.ModLoader.ModGore", from: "updateType",	to: "UpdateType");
		RenameInstanceField("Terraria.ModLoader.ModDust", from: "updateType",	to: "UpdateType");

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
		RefactorInstanceMethodCall("Terraria.Tile", "isTheSameAs",		AddComment("https://github.com/tModLoader/tModLoader/wiki/Update-Migration-Guide#tiles"));

		RenameStaticField("Terraria.ID.ItemUseStyleID", from: "HoldingUp",	to: "HoldUp");
		RenameStaticField("Terraria.ID.ItemUseStyleID", from: "HoldingOut",	to: "Shoot");
		RenameStaticField("Terraria.ID.ItemUseStyleID", from: "SwingThrow",	to: "Swing");
		RenameStaticField("Terraria.ID.ItemUseStyleID", from: "EatingUsing", to: "EatFood");
		RenameStaticField("Terraria.ID.ItemUseStyleID", from: "Stabbing",	to: "Thrust");

		RenameMethod("Terraria.ModLoader.ModItem",		from: "UpdateVanity",		to: "EquipFrameEffects");
		RenameMethod("Terraria.ModLoader.ModItem",		from: "NetRecieve",			to: "NetReceive");
		RenameMethod("Terraria.ModLoader.ModItem",		from: "NewPreReforge",		to: "PreReforge");
		RenameMethod("Terraria.ModLoader.GlobalItem",	from: "NewPreReforge",		to: "PreReforge");
		RenameMethod("Terraria.ModLoader.ModItem",		from: "GetWeaponCrit",		to: "ModifyWeaponCrit");
		RenameMethod("Terraria.ModLoader.GlobalItem",	from: "GetWeaponCrit",		to: "ModifyWeaponCrit");
		RenameMethod("Terraria.ModLoader.ModPlayer",	from: "GetWeaponCrit",		to: "ModifyWeaponCrit");
		RenameMethod("Terraria.ModLoader.ModItem",		from: "GetWeaponKnockback",	to: "ModifyWeaponKnockback");
		RenameMethod("Terraria.ModLoader.GlobalItem",	from: "GetWeaponKnockback",	to: "ModifyWeaponKnockback");
		RenameMethod("Terraria.ModLoader.ModPlayer",	from: "GetWeaponKnockback",	to: "ModifyWeaponKnockback");
		RenameMethod("Terraria.ModLoader.ModNPC",		from: "NPCLoot",			to: "OnKill");
		RenameMethod("Terraria.ModLoader.GlobalNPC",	from: "NPCLoot",			to: "OnKill");
		RenameMethod("Terraria.ModLoader.ModNPC",		from: "PreNPCLoot",			to: "PreKill");
		RenameMethod("Terraria.ModLoader.GlobalNPC",	from: "PreNPCLoot",			to: "PreKill");
		RenameMethod("Terraria.ModLoader.ModNPC",		from: "SpecialNPCLoot",		to: "SpecialOnKill");
		RenameMethod("Terraria.ModLoader.GlobalNPC",	from: "SpecialNPCLoot",		to: "SpecialOnKill");
		RenameMethod("Terraria.ModLoader.ModTile",		from: "NewRightClick",		to: "RightClick");
		RenameMethod("Terraria.ModLoader.ModTile",		from: "Dangersense",		to: "IsTileDangerous");
		RenameMethod("Terraria.ModLoader.GlobalTile",	from: "Dangersense",		to: "IsTileDangerous");
		RenameMethod("Terraria.ModLoader.ModTileEntity",from: "ValidTile",			to: "IsTileValidForEntity");
		RenameMethod("Terraria.ModLoader.EquipTexture", from: "UpdateVanity",		to: "FrameEffects");

		ChangeHookSignature("Terraria.ModLoader.ModItem",			"HoldStyle");
		ChangeHookSignature("Terraria.ModLoader.GlobalItem",		"HoldStyle");
		ChangeHookSignature("Terraria.ModLoader.ModItem",			"UseStyle");
		ChangeHookSignature("Terraria.ModLoader.GlobalItem",		"UseStyle");
		ChangeHookSignature("Terraria.ModLoader.ModItem",			"UseItem", comment: "Suggestion: Return null instead of false");
		ChangeHookSignature("Terraria.ModLoader.GlobalItem",		"UseItem", comment: "Suggestion: Return null instead of false");
		ChangeHookSignature("Terraria.ModLoader.ModItem",			"ModifyWeaponKnockback");
		ChangeHookSignature("Terraria.ModLoader.GlobalItem",		"ModifyWeaponKnockback");
		ChangeHookSignature("Terraria.ModLoader.ModPlayer",			"ModifyWeaponKnockback");
		ChangeHookSignature("Terraria.ModLoader.ModItem",			"ModifyWeaponCrit");
		ChangeHookSignature("Terraria.ModLoader.GlobalItem",		"ModifyWeaponCrit");
		ChangeHookSignature("Terraria.ModLoader.ModPlayer",			"ModifyWeaponCrit");
		ChangeHookSignature("Terraria.ModLoader.ModItem",			"ModifyWeaponDamage");
		ChangeHookSignature("Terraria.ModLoader.GlobalItem",		"ModifyWeaponDamage");
		ChangeHookSignature("Terraria.ModLoader.ModPlayer",			"ModifyWeaponDamage");
		ChangeHookSignature("Terraria.ModLoader.ModNPC",			"PreDraw");
		ChangeHookSignature("Terraria.ModLoader.GlobalNPC",			"PreDraw");
		ChangeHookSignature("Terraria.ModLoader.ModNPC",			"PostDraw");
		ChangeHookSignature("Terraria.ModLoader.GlobalNPC",			"PostDraw");
		ChangeHookSignature("Terraria.ModLoader.ModProjectile",		"PreDrawExtras");
		ChangeHookSignature("Terraria.ModLoader.GlobalProjectile",	"PreDrawExtras");
		ChangeHookSignature("Terraria.ModLoader.ModProjectile",		"PreDraw");
		ChangeHookSignature("Terraria.ModLoader.GlobalProjectile",	"PreDraw");
		ChangeHookSignature("Terraria.ModLoader.ModProjectile",		"PostDraw");
		ChangeHookSignature("Terraria.ModLoader.GlobalProjectile",	"PostDraw");
		ChangeHookSignature("Terraria.ModLoader.ModProjectile",		"CanDamage", comment: "Suggestion: Return null instead of false");
		ChangeHookSignature("Terraria.ModLoader.GlobalProjectile",	"CanDamage", comment: "Suggestion: Return null instead of false");
		ChangeHookSignature("Terraria.ModLoader.ModProjectile",		"TileCollideStyle");
		ChangeHookSignature("Terraria.ModLoader.GlobalProjectile",	"TileCollideStyle");
		ChangeHookSignature("Terraria.ModLoader.ModPlayer",			"DrawEffects");
		ChangeHookSignature("Terraria.ModLoader.ModPlayer",			"CatchFish");
		ChangeHookSignature("Terraria.ModLoader.ModTile",			"HasSmartInteract");
		ChangeHookSignature("Terraria.ModLoader.ModTile",			"DrawEffects");
		ChangeHookSignature("Terraria.ModLoader.GlobalTile",		"DrawEffects");
		ChangeHookSignature("Terraria.ModLoader.GlobalTile",		"IsTileDangerous", comment: "Suggestion: Return null instead of false");
		ChangeHookSignature("Terraria.ModLoader.ModBlockType",		"KillSound");
		ChangeHookSignature("Terraria.ModLoader.GlobalBlockType",	"KillSound");

		RenameMethod("Terraria.ModLoader.ModItem",		from: "Load",		to: "LoadData");
		RenameMethod("Terraria.ModLoader.ModItem",		from: "Save",		to: "SaveData");
		RenameMethod("Terraria.ModLoader.GlobalItem",	from: "Load",		to: "LoadData");
		RenameMethod("Terraria.ModLoader.GlobalItem",	from: "Save",		to: "SaveData");
		RenameMethod("Terraria.ModLoader.ModPlayer",	from: "Load",		to: "LoadData");
		RenameMethod("Terraria.ModLoader.ModPlayer",	from: "Save",		to: "SaveData");
		RenameMethod("Terraria.ModLoader.ModTileEntity",from: "Load",		to: "LoadData");
		RenameMethod("Terraria.ModLoader.ModTileEntity",from: "Save",		to: "SaveData");
		RenameMethod("Terraria.ModLoader.ModSystem",	from: "Load",		to: "LoadWorldData");
		RenameMethod("Terraria.ModLoader.ModSystem",	from: "Save",		to: "SaveWorldData");
		RenameMethod("Terraria.ModLoader.ModSystem",	from: "Initialize", to: "OnWorldLoad");
		RenameMethod("Terraria.ModLoader.ModSystem",	from: "PreUpdate",	to: "PreUpdateWorld");
		RenameMethod("Terraria.ModLoader.ModSystem",	from: "PostUpdate", to: "PostUpdateWorld");
		ChangeHookSignature("Terraria.ModLoader.ModItem",		"SaveData",			comment: "Suggestion: Edit tag parameter rather than returning new TagCompound");
		ChangeHookSignature("Terraria.ModLoader.GlobalItem",	"SaveData",			comment: "Suggestion: Edit tag parameter rather than returning new TagCompound");
		ChangeHookSignature("Terraria.ModLoader.ModPlayer",		"SaveData",			comment: "Suggestion: Edit tag parameter rather than returning new TagCompound");
		ChangeHookSignature("Terraria.ModLoader.ModTileEntity", "SaveData",			comment: "Suggestion: Edit tag parameter rather than returning new TagCompound");
		ChangeHookSignature("Terraria.ModLoader.ModSystem",		"SaveWorldData",	comment: "Suggestion: Edit tag parameter rather than returning new TagCompound");
		ChangeHookSignature("Terraria.ModLoader.ModSystem",		"OnWorldLoad",		comment: "Suggestion: Also concider overriding OnWorldUnload"); // TODO this doesn't work for just adding a comment when the sig didnt change
		//ChangeHookSignature("Terraria.ModLoader.ModSystem",	"TileCountsAvailable"); // TODO causes recursion
		ChangeHookSignature("Terraria.ModLoader.ModItem",		"Clone");
		ChangeHookSignature("Terraria.ModLoader.ModGore",		"OnSpawn");

		RenameMethod("Terraria.ModLoader.GlobalTile",	from: "SetDefaults", to: "SetStaticDefaults");
		RenameMethod("Terraria.ModLoader.GlobalWall",	from: "SetDefaults", to: "SetStaticDefaults");
		RenameMethod("Terraria.ModLoader.InfoDisplay",	from: "SetDefaults", to: "SetStaticDefaults");
		RenameMethod("Terraria.ModLoader.ModBlockType",	from: "SetDefaults", to: "SetStaticDefaults");
		RenameMethod("Terraria.ModLoader.ModBuff",		from: "SetDefaults", to: "SetStaticDefaults");
		RenameMethod("Terraria.ModLoader.ModDust",		from: "SetDefaults", to: "SetStaticDefaults");
		RenameMethod("Terraria.ModLoader.ModMount",		from: "SetDefaults", to: "SetStaticDefaults");
		RenameMethod("Terraria.ModLoader.ModPrefix",	from: "SetDefaults", to: "SetStaticDefaults");

		RenameInstanceField("Terraria.ModLoader.ModBlockType",		from: "drop",		 to: "ItemDrop");
		RenameInstanceField("Terraria.ModLoader.ModBlockType",		from: "dustType",    to: "DustType");

		RenameInstanceField("Terraria.ModLoader.ModTile", from: "dresserDrop",			 to: "DresserDrop");
		RenameInstanceField("Terraria.ModLoader.ModTile", from: "chestDrop",			 to: "ChestDrop");
		RenameInstanceField("Terraria.ModLoader.ModTile", from: "closeDoorID",			 to: "CloseDoorID");
		RenameInstanceField("Terraria.ModLoader.ModTile", from: "openDoorID",			 to: "OpenDoorID");
		RenameInstanceField("Terraria.ModLoader.ModTile", from: "minPick",				 to: "MinPick");
		RenameInstanceField("Terraria.ModLoader.ModTile", from: "mineResist",			 to: "MineResist");
		RenameInstanceField("Terraria.ModLoader.ModTile", from: "animationFrameHeight",  to: "AnimationFrameHeight");
		RenameInstanceField("Terraria.ModLoader.ModTile", from: "adjTiles",				 to: "AdjTiles");

		RenameInstanceField("Terraria.ModLoader.ModWaterStyle",		from: "Type",		to: "Slot");
		RenameInstanceField("Terraria.ModLoader.ModWaterfallStyle", from: "Type",		to: "Slot");

		RenameInstanceField("Terraria.ModLoader.NPCSpawnInfo", from: "desertCave", to: "DesertCave");
		RenameInstanceField("Terraria.ModLoader.NPCSpawnInfo", from: "granite", to: "Granite");
		RenameInstanceField("Terraria.ModLoader.NPCSpawnInfo", from: "invasion", to: "Invasion");
		RenameInstanceField("Terraria.ModLoader.NPCSpawnInfo", from: "lihzahrd", to: "Lihzahrd");
		RenameInstanceField("Terraria.ModLoader.NPCSpawnInfo", from: "marble", to: "Marble");
		RenameInstanceField("Terraria.ModLoader.NPCSpawnInfo", from: "planteraDefeated", to: "PlanteraDefeated");
		RenameInstanceField("Terraria.ModLoader.NPCSpawnInfo", from: "player", to: "Player");
		RenameInstanceField("Terraria.ModLoader.NPCSpawnInfo", from: "playerFloorX", to: "PlayerFloorX");
		RenameInstanceField("Terraria.ModLoader.NPCSpawnInfo", from: "playerFloorY", to: "PlayerFloorY");
		RenameInstanceField("Terraria.ModLoader.NPCSpawnInfo", from: "playerInTown", to: "PlayerInTown");
		RenameInstanceField("Terraria.ModLoader.NPCSpawnInfo", from: "playerSafe", to: "PlayerSafe");
		RenameInstanceField("Terraria.ModLoader.NPCSpawnInfo", from: "safeRangeX", to: "SafeRangeX");
		RenameInstanceField("Terraria.ModLoader.NPCSpawnInfo", from: "sky", to: "Sky");
		RenameInstanceField("Terraria.ModLoader.NPCSpawnInfo", from: "spawnTileType", to: "SpawnTileType");
		RenameInstanceField("Terraria.ModLoader.NPCSpawnInfo", from: "spawnTileX", to: "SpawnTileX");
		RenameInstanceField("Terraria.ModLoader.NPCSpawnInfo", from: "spawnTileY", to: "SpawnTileY");
		RenameInstanceField("Terraria.ModLoader.NPCSpawnInfo", from: "spiderCave", to: "SpiderCave");
		RenameInstanceField("Terraria.ModLoader.NPCSpawnInfo", from: "water", to: "Water");

		RefactorInstanceMethodCall("Terraria.ModLoader.Mod", "BuffType",		ToFindTypeCall("Terraria.ModLoader.ModBuff"));
		RefactorInstanceMethodCall("Terraria.ModLoader.Mod", "DustType",		ToFindTypeCall("Terraria.ModLoader.ModDust"));
		RefactorInstanceMethodCall("Terraria.ModLoader.Mod", "ItemType",		ToFindTypeCall("Terraria.ModLoader.ModItem"));
		RefactorInstanceMethodCall("Terraria.ModLoader.Mod", "MountType",		ToFindTypeCall("Terraria.ModLoader.ModMount"));
		RefactorInstanceMethodCall("Terraria.ModLoader.Mod", "NPCType",			ToFindTypeCall("Terraria.ModLoader.ModNPC"));
		RefactorInstanceMethodCall("Terraria.ModLoader.Mod", "PrefixType",		ToFindTypeCall("Terraria.ModLoader.ModPrefix"));
		RefactorInstanceMethodCall("Terraria.ModLoader.Mod", "ProjectileType",	ToFindTypeCall("Terraria.ModLoader.ModProjectile"));
		RefactorInstanceMethodCall("Terraria.ModLoader.Mod", "TileEntityType",	ToFindTypeCall("Terraria.ModLoader.ModTileEntity"));
		RefactorInstanceMethodCall("Terraria.ModLoader.Mod", "TileType",		ToFindTypeCall("Terraria.ModLoader.ModTile"));
		RefactorInstanceMethodCall("Terraria.ModLoader.Mod", "WallType",		ToFindTypeCall("Terraria.ModLoader.ModWall"));
		RefactorInstanceMethodCall("Terraria.ModLoader.Mod", "GetGoreSlot",		ToFindTypeCall("Terraria.ModLoader.ModGore"));

		RenameMethod("Terraria.ModLoader.Mod",			from: "TextureExists",	to: "HasAsset");
		RenameMethod("Terraria.ModLoader.ModContent",	from: "TextureExists",	to: "HasAsset");

		RenameType(from: "Terraria.ModLoader.PlayerHooks",		to: "Terraria.ModLoader.PlayerLoader");
		RenameType(from: "Terraria.ModLoader.MusicPriority",	to: "Terraria.ModLoader.SceneEffectPriority");
		RenameType(from: "Terraria.ModLoader.SpawnCondition",	to: "Terraria.ModLoader.Utilities.SpawnCondition");

		RenameNamespace(from: "Terraria.World.Generation", to: "Terraria.WorldBuilding");

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
		RenameStaticField("Terraria.Lighting",	from: "lightMode",			to: "LegacyEngine.Mode");

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

		RenameStaticField("Terraria.ID.NPCID.Sets",			from: "TechnicallyABoss",	to: "ShouldBeCountedAsBoss");
		RenameStaticField("Terraria.ID.ProjectileID.Sets",	from: "Homing",				to: "CultistIsResistantTo");

		RenameMethod("Terraria.Lighting",	from: "BlackOut",				to: "Clear");
		RenameMethod("Terraria.Utils",		from: "InverseLerp",			to: "GetLerpValue");
		RenameMethod("Terraria.NetMessage",	from: "BroadcastChatMessage",	to: "BroadcastChatMessage", newType: "Terraria.Chat.ChatHelper");
		RenameMethod("Terraria.Main",		from: "PlaySound",				to: "PlaySound",			newType: "Terraria.Audio.SoundEngine");

		AddTextureRenames();
	}
}