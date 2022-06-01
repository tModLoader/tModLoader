using Terraria;
using Terraria.ID;
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

		float inverseLerp = Utils.InverseLerp(0f, 1f, 0.1f, false);
		Lighting.BlackOut();
		NetMessage.BroadcastChatMessage(null, Color.White, -1);

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

		var item = new Item();
		var owner = item.owner;
	}
}