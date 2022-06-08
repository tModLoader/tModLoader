using static tModPorter.Rewriters.RenameRewriter;
using static tModPorter.Rewriters.InvokeRewriter;
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
		RefactorInstanceMethodCall("Terraria.Tile", "isTheSameAs",		AddComment("https://github.com/tModLoader/tModLoader/wiki/Update-Migration-Guide#tiles"));

		RenameStaticField("Terraria.ID.ItemUseStyleID", from: "HoldingUp",	to: "HoldUp");
		RenameStaticField("Terraria.ID.ItemUseStyleID", from: "HoldingOut",	to: "Shoot");
		RenameStaticField("Terraria.ID.ItemUseStyleID", from: "SwingThrow",	to: "Swing");
		RenameStaticField("Terraria.ID.ItemUseStyleID", from: "EatingUsing", to: "EatFood");
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

		RenameMethod("Terraria.Lighting",	from: "BlackOut",				to: "Clear");
		RenameMethod("Terraria.Utils",		from: "InverseLerp",			to: "GetLerpValue");
		RenameMethod("Terraria.NetMessage",	from: "BroadcastChatMessage",	to: "BroadcastChatMessage", newType: "Terraria.Chat.ChatHelper");
		RenameMethod("Terraria.Main",		from: "PlaySound",				to: "PlaySound",			newType: "Terraria.Audio.SoundEngine");
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
		RenameTextureAsset(from: "wireUITexture",		to: "WireUi");

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

