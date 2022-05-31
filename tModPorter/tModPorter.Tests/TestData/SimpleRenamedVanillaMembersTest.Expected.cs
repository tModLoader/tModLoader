using Terraria;
using Terraria.Chat;
using Terraria.ID;
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

		float inverseLerp = Utils.GetLerpValue(0f, 1f, 0.1f, false);
		Lighting.Clear();
		ChatHelper.BroadcastChatMessage(null, Color.White, -1);

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

		var item = new Item();
		var owner = item.playerIndexTheItemIsReservedFor;
	}
}