using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.UI;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader.Default;
using Terraria.GameContent.UI.Elements;

namespace Terraria.ModLoader;

// TODO: further documentation
/// <summary>
/// This serves as a central place to store equipment slots and their corresponding textures. You will use this to obtain the IDs for your equipment textures.
/// </summary>
public class AccessorySlotLoader : Loader<ModAccessorySlot>
{
	private static Player Player => Main.LocalPlayer;

	internal static ModAccessorySlotPlayer ModSlotPlayer(Player player) => player.GetModPlayer<ModAccessorySlotPlayer>();

	public AccessorySlotLoader() => Initialize(0);

	// Note: Because of the weird nuance of both vanity and functional being in same array, we have to reduce down to half the size
	private ModAccessorySlot GetIdCorrected(int id) => (id >= list.Count) ? new UnloadedAccessorySlot(id, "TEMP'd") : list[id];
	public ModAccessorySlot Get(int id, Player player) => GetIdCorrected(id % ModSlotPlayer(player).SlotCount);
	public ModAccessorySlot Get(int id) => Get(id, Player);

	public const int MaxVanillaSlotCount = 2 + 5;

	// DRAWING CODE ///////////////////////////////////////////////////////////////////
	internal int GetAccessorySlotPerColumn()
	{
		float minimumClearance = DrawVerticalAlignment + 2 * 56 * Main.inventoryScale + 4;
		return (int)((Main.screenHeight - minimumClearance) / (56 * Main.inventoryScale) - 1.8f);
	}

	/// <summary>
	/// The variable known as num20 used to align all equipment slot drawing in Main.
	/// Represents the y position where equipment slots start to be drawn from.
	/// </summary>
	static public int DrawVerticalAlignment { get; private set; }

	/// <summary>
	/// The variable that determines where the DefenseIcon will be drawn, after considering all slot information.
	/// </summary>
	static public Vector2 DefenseIconPosition { get; private set; }

	public void DrawAccSlots(int num20)
	{
		int skip = 0;
		DrawVerticalAlignment = num20;
		Color color = Main.inventoryBack;

		for (int vanillaSlot = 3; vanillaSlot < Player.dye.Length; vanillaSlot++) {
			if (!Draw(skip, false, vanillaSlot, color)) {
				skip++;
			}
		}

		for (int modSlot = 0; modSlot < ModSlotPlayer(Player).SlotCount; modSlot++) {
			if (!Draw(skip, true, modSlot, color))
				skip++;
		}

		// there are no slots to be drawn by us.
		if (skip == MaxVanillaSlotCount + ModSlotPlayer(Player).SlotCount) {
			ModSlotPlayer(Player).scrollbarSlotPosition = 0;
			return;
		}

		int accessoryPerColumn = GetAccessorySlotPerColumn();
		int slotsToRender = ModSlotPlayer(Player).SlotCount + MaxVanillaSlotCount - skip;
		int scrollIncrement = slotsToRender - accessoryPerColumn;

		if (scrollIncrement < 0) {
			accessoryPerColumn = slotsToRender;
			scrollIncrement = 0;
		}

		DefenseIconPosition = new Vector2(Main.screenWidth - 64 - 28, DrawVerticalAlignment + (accessoryPerColumn + 2) * 56 * Main.inventoryScale + 4);

		if (scrollIncrement > 0) {
			DrawScrollSwitch();

			if (ModSlotPlayer(Player).scrollSlots) {
				DrawScrollbar(accessoryPerColumn, slotsToRender, scrollIncrement);
			}
		}
		else
			ModSlotPlayer(Player).scrollbarSlotPosition = 0;
	}

	public static string[] scrollStackLang = { Language.GetTextValue("tModLoader.slotStack"), Language.GetTextValue("tModLoader.slotScroll") };

	internal void DrawScrollSwitch()
	{
		Texture2D value4 = TextureAssets.InventoryTickOn.Value;
		if (ModSlotPlayer(Player).scrollSlots)
			value4 = TextureAssets.InventoryTickOff.Value;

		int xLoc2 = Main.screenWidth - 64 - 28 + 47 + 9;
		int yLoc2 = (int)((float)(DrawVerticalAlignment) + (float)((0 + 3) * 56) * Main.inventoryScale) - 10;

		Main.spriteBatch.Draw(value4, new Vector2(xLoc2, yLoc2), Color.White * 0.7f);

		Rectangle rectangle = new Rectangle(xLoc2, yLoc2, value4.Width, value4.Height);
		if (!(rectangle.Contains(new Point(Main.mouseX, Main.mouseY)) && !PlayerInput.IgnoreMouseInterface)) {
			return;
		}

		Player player = Main.LocalPlayer;
		player.mouseInterface = true;
		if (Main.mouseLeft && Main.mouseLeftRelease) {
			ModSlotPlayer(Player).scrollSlots = !ModSlotPlayer(Player).scrollSlots;
			SoundEngine.PlaySound(12);
		}

		int num45 = ((!ModSlotPlayer(Player).scrollSlots) ? 0 : 1);
		Main.HoverItem = new Item();
		Main.hoverItemName = scrollStackLang[num45];
	}

	// This is a hacky solution to make it very vanilla-esque, at the cost of not actually using a UI proper.
	internal void DrawScrollbar(int accessoryPerColumn, int slotsToRender, int scrollIncrement)
	{
		int xLoc = Main.screenWidth - 64 - 28;

		int chkMax = (int)((float)(DrawVerticalAlignment) + (float)(((accessoryPerColumn) + 3) * 56) * Main.inventoryScale) + 4;
		int chkMin = (int)((float)(DrawVerticalAlignment) + (float)((0 + 3) * 56) * Main.inventoryScale) + 4;

		UIScrollbar scrollbar = new UIScrollbar();

		Rectangle rectangle = new Rectangle(xLoc + 47 + 6, chkMin, 5, chkMax - chkMin);
		scrollbar.DrawBar(Main.spriteBatch, Main.Assets.Request<Texture2D>("Images/UI/Scrollbar").Value, rectangle, Color.White);

		int barSize = (chkMax - chkMin) / (scrollIncrement + 1);
		rectangle = new Rectangle(xLoc + 47 + 5, chkMin + ModSlotPlayer(Player).scrollbarSlotPosition * barSize, 3, barSize);
		scrollbar.DrawBar(Main.spriteBatch, Main.Assets.Request<Texture2D>("Images/UI/ScrollbarInner").Value, rectangle, Color.White);

		rectangle = new Rectangle(xLoc - 47 * 2, chkMin, 47 * 3, chkMax - chkMin);
		if (!(rectangle.Contains(new Point(Main.mouseX, Main.mouseY)) && !PlayerInput.IgnoreMouseInterface)) {
			return;
		}

		PlayerInput.LockVanillaMouseScroll("ModLoader/Acc");

		int scrollDelta = ModSlotPlayer(Player).scrollbarSlotPosition + (int)PlayerInput.ScrollWheelDelta / 120;
		scrollDelta = Math.Min(scrollDelta, scrollIncrement);
		scrollDelta = Math.Max(scrollDelta, 0);

		ModSlotPlayer(Player).scrollbarSlotPosition = scrollDelta;
		PlayerInput.ScrollWheelDelta = 0;
	}

	int slotDrawLoopCounter = 0;

	/// <summary>
	/// Draws Vanilla and Modded Accessory Slots
	/// </summary>
	public bool Draw(int skip, bool modded, int slot, Color color)
	{
		bool flag3;
		bool flag4 = false;
		bool loadoutConflict = false;
		bool accessoryConflict = false;
		bool vanityConflict = false;

		if (modded) {
			flag3 = !ModdedIsItemSlotUnlockedAndUsable(slot, Player);
			flag4 = !ModdedCanSlotBeShown(slot);
			// flag3 && flag4: If not usable (flag3) and hidden when not usable (flag4), don't draw.

			// If there is a loadout conflict, force not usable but allow user to fix the issue by forcing it to show.
			ModAccessorySlotPlayer modAccessorySlotPlayer = ModSlotPlayer(Player);
			if (modAccessorySlotPlayer.SharedSlotHasLoadoutConflict(slot, vanitySlot: false)) {
				loadoutConflict = true;
				accessoryConflict = true;
			}
			if (modAccessorySlotPlayer.SharedSlotHasLoadoutConflict(slot, vanitySlot: true)) {
				loadoutConflict = true;
				vanityConflict = true;
			}
		}
		else {
			flag3 = !Player.IsItemSlotUnlockedAndUsable(slot);
			if (slot == 8) {
				flag4 = (slot == 8) && !Player.CanDemonHeartAccessoryBeShown();
			}
			else if (slot == 9) {
				flag4 = !Player.CanMasterModeAccessoryBeShown();
			}
		}

		if (flag4 && flag3 && !loadoutConflict || modded && IsHidden(slot)) {
			return false;
		}

		Main.inventoryBack = flag3 ? new Color(80, 80, 80, 80) : color;
		slotDrawLoopCounter = 0;
		int yLoc = 0, xLoc = 0;
		bool customLoc = false;

		if (modded) {
			ModAccessorySlot mAccSlot = Get(slot);
			customLoc = mAccSlot.CustomLocation.HasValue;
			if (!customLoc && Main.EquipPage != 0) {
				Main.inventoryBack = color;
				return false;
			}

			if (customLoc) {
				xLoc = (int)mAccSlot.CustomLocation?.X;
				yLoc = (int)mAccSlot.CustomLocation?.Y;
			}
			else if (!SetDrawLocation(slot + Player.dye.Length - 3, skip, ref xLoc, ref yLoc)) {
				Main.inventoryBack = color;
				return true;
			}

			var thisSlot = Get(slot);
			ModAccessorySlotPlayer modSlotPlayer = ModSlotPlayer(Player);

			Main.inventoryBack = flag3 || accessoryConflict ? new Color(80, 80, 80, 80) : color;
			if (thisSlot.DrawFunctionalSlot) {
				bool skipMouse = DrawVisibility(ref modSlotPlayer.exHideAccessory[slot], -10, xLoc, yLoc, out var xLoc2, out var yLoc2, out var value4);
				DrawSlot(modSlotPlayer.exAccessorySlot, -10, slot, flag3, xLoc, yLoc, skipMouse);
				Main.spriteBatch.Draw(value4, new Vector2(xLoc2, yLoc2), Color.White * 0.7f);
			}

			Main.inventoryBack = flag3 || vanityConflict ? new Color(80, 80, 80, 80) : color;
			if (thisSlot.DrawVanitySlot)
				DrawSlot(modSlotPlayer.exAccessorySlot, -11, slot + modSlotPlayer.SlotCount, flag3, xLoc, yLoc);

			Main.inventoryBack = flag3 ? new Color(80, 80, 80, 80) : color;
			if (thisSlot.DrawDyeSlot)
				DrawSlot(modSlotPlayer.exDyesAccessory, -12, slot, flag3, xLoc, yLoc);
		}
		else {
			if (!customLoc && Main.EquipPage != 0) {
				Main.inventoryBack = color;
				return false;
			}
			if (!SetDrawLocation(slot - 3, skip, ref xLoc, ref yLoc)) {
				Main.inventoryBack = color;
				return true;
			}

			bool skipMouse = DrawVisibility(ref Player.hideVisibleAccessory[slot], 10, xLoc, yLoc, out var xLoc2, out var yLoc2, out var value4);
			DrawSlot(Player.armor, 10, slot, flag3, xLoc, yLoc, skipMouse);
			Main.spriteBatch.Draw(value4, new Vector2(xLoc2, yLoc2), Color.White * 0.7f);
			DrawSlot(Player.armor, 11, slot + Player.dye.Length, flag3, xLoc, yLoc);
			DrawSlot(Player.dye, 12, slot, flag3, xLoc, yLoc);
		}

		Main.inventoryBack = color;
		return !customLoc;
	}

	/// <summary>
	/// Applies Xloc and Yloc data for the slot, based on ModAccessorySlotPlayer.scrollSlots
	/// </summary>
	internal bool SetDrawLocation(int trueSlot, int skip, ref int xLoc, ref int yLoc)
	{
		int accessoryPerColumn = GetAccessorySlotPerColumn();
		int xColumn = (int)(trueSlot / accessoryPerColumn);
		int yRow = trueSlot % accessoryPerColumn;

		if (ModSlotPlayer(Player).scrollSlots) {
			int row = yRow + (xColumn) * accessoryPerColumn - ModSlotPlayer(Player).scrollbarSlotPosition - skip;

			yLoc = (int)((float)(DrawVerticalAlignment) + (float)((row + 3) * 56) * Main.inventoryScale) + 4;
			int chkMin = (int)((float)(DrawVerticalAlignment) + (float)((0 + 3) * 56) * Main.inventoryScale) + 4;
			int chkMax = (int)((float)(DrawVerticalAlignment) + (float)(((accessoryPerColumn - 1) + 3) * 56) * Main.inventoryScale) + 4;

			if (yLoc > chkMax || yLoc < chkMin) {
				return false;
			}

			xLoc = Main.screenWidth - 64 - 28;
		}

		else {
			int row = yRow, tempSlot = trueSlot, col = xColumn;
			if (skip > 0) {
				tempSlot -= skip;
				row = tempSlot % accessoryPerColumn;
				col = tempSlot / accessoryPerColumn;
			}

			yLoc = (int)((float)(DrawVerticalAlignment) + (float)((row + 3) * 56) * Main.inventoryScale) + 4;
			if (col > 0) {
				xLoc = Main.screenWidth - 64 - 28 - 47 * 3 * col - 50;
			}
			else {
				xLoc = Main.screenWidth - 64 - 28 - 47 * 3 * col;
			}
		}

		return true;
	}

	/// <summary>
	/// Is run in AccessorySlotLoader.Draw.
	/// Creates &amp; sets up Hide Visibility Button.
	/// </summary>
	internal bool DrawVisibility(ref bool visbility, int context, int xLoc, int yLoc, out int xLoc2, out int yLoc2, out Texture2D value4)
	{
		yLoc2 = yLoc - 2;
		xLoc2 = xLoc - 58 + 64 + 28;

		value4 = TextureAssets.InventoryTickOn.Value;
		if (visbility)
			value4 = TextureAssets.InventoryTickOff.Value;

		Rectangle rectangle = new Rectangle(xLoc2, yLoc2, value4.Width, value4.Height);
		int num45 = 0;
		bool skipCheck = false;
		if (rectangle.Contains(new Point(Main.mouseX, Main.mouseY)) && !PlayerInput.IgnoreMouseInterface) {
			skipCheck = true;
			Player.mouseInterface = true;

			if (Main.mouseLeft && Main.mouseLeftRelease) {
				visbility = !visbility;
				SoundEngine.PlaySound(12);

				if (Main.netMode == 1 && context > 0)
					NetMessage.SendData(4, -1, -1, null, Player.whoAmI);
			}

			num45 = ((!visbility) ? 1 : 2);
		}

		if (num45 > 0) {
			Main.HoverItem = new Item();
			Main.hoverItemName = Lang.inter[58 + num45].Value;
		}

		return skipCheck;
	}

	/// <summary>
	/// Is run in AccessorySlotLoader.Draw.
	/// Generates a significant amount of functionality for the slot, despite being named drawing because vanilla.
	/// At the end, calls this.DrawRedirect to enable custom drawing
	/// </summary>
	private void DrawSlot(Item[] items, int context, int slot, bool flag3, int xLoc, int yLoc, bool skipCheck = false)
	{
		bool flag = flag3 && !Main.mouseItem.IsAir;
		int xLoc1 = xLoc - 47 * (slotDrawLoopCounter++);
		bool isHovered = false;

		if (!skipCheck && Main.mouseX >= xLoc1 && (float)Main.mouseX <= (float)xLoc1 + (float)TextureAssets.InventoryBack.Width() * Main.inventoryScale && Main.mouseY >= yLoc
			&& (float)Main.mouseY <= (float)yLoc + (float)TextureAssets.InventoryBack.Height() * Main.inventoryScale && !PlayerInput.IgnoreMouseInterface) {
			isHovered = true;

			Player.mouseInterface = true;
			Main.armorHide = true;
			ItemSlot.OverrideHover(items, Math.Abs(context), slot);

			if (!flag) {
				if (Math.Abs(context) == 12) {
					if (Main.mouseRightRelease && Main.mouseRight)
						ItemSlot.RightClick(items, context, slot);

					ItemSlot.LeftClick(items, context, slot);
				}
				else if (Math.Abs(context) == 11) {
					ItemSlot.LeftClick(items, context, slot);
					ItemSlot.RightClick(items, context, slot);
				}
				else if (Math.Abs(context) == 10) {
					ItemSlot.LeftClick(items, context, slot);
				}
			}

			ItemSlot.MouseHover(items, Math.Abs(context), slot);

			if (context < 0) {
				OnHover(slot, context);

				// Override custom hover text for this important information
				if (Math.Abs(context) != 12 && ModSlotPlayer(Player).SharedSlotHasLoadoutConflict(slot, vanitySlot: Math.Abs(context) == 11)) {
					Main.HoverItem = new Item();
					Main.hoverItemName = Language.GetTextValue("tModLoader.SharedAccessorySlotConflictTooltip");
				}

				// TODO: We could also show the name of an UnloadedAccessorySlot in the tooltip.

				// Debug Code: 
				//Main.hoverItemName += " - Slot #" + slot;
			}
		}
		DrawRedirect(items, context, slot, new Vector2(xLoc1, yLoc), isHovered);
	}

	internal void DrawRedirect(Item[] inv, int context, int slot, Vector2 location, bool isHovered)
	{
		if (context < 0) {
			if (Get(slot).PreDraw(ContextToEnum(context), inv[slot], location, isHovered))
				ItemSlot.Draw(Main.spriteBatch, inv, context, slot, location);

			Get(slot).PostDraw(ContextToEnum(context), inv[slot], location, isHovered);
		}
		else {
			ItemSlot.Draw(Main.spriteBatch, inv, context, slot, location);
		}
	}

	/// <summary>
	/// Provides the Texture for a Modded Accessory Slot
	/// This probably will need optimization down the road.
	/// </summary>
	internal (Texture2D, bool shared) GetBackgroundTexture(int slot, int context)
	{
		var thisSlot = Get(slot);
		switch (context) {
			case -10:
				if (ModContent.RequestIfExists<Texture2D>(thisSlot.FunctionalBackgroundTexture, out var funcTexture))
					return (funcTexture.Value, true);
				break;
			case -11:
				if (ModContent.RequestIfExists<Texture2D>(thisSlot.VanityBackgroundTexture, out var vanityTexture))
					return (vanityTexture.Value, true);
				break;
			case -12:
				if (ModContent.RequestIfExists<Texture2D>(thisSlot.DyeBackgroundTexture, out var dyeTexture))
					return (dyeTexture.Value, true);
				break;
		}

		if (ModSlotPlayer(Player).IsSharedSlot(thisSlot.Type)) {
			// Shared slots retain the old background textures by default.
			return context switch {
				-10 => (TextureAssets.InventoryBack3.Value, true),
				-11 => (TextureAssets.InventoryBack8.Value, true),
				-12 => (TextureAssets.InventoryBack12.Value, true),
				_ => throw new NotImplementedException()
			};
		}
		return (TextureAssets.InventoryBack13.Value, false);
	}

	internal void DrawSlotTexture(Texture2D value6, Vector2 position, Rectangle rectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth, int slot, int context)
	{
		var thisSlot = Get(slot);
		Texture2D texture = null;
		switch (context) {
			case -10:
				if (ModContent.RequestIfExists<Texture2D>(thisSlot.FunctionalTexture, out var funcTexture))
					texture = funcTexture.Value;
				break;
			case -11:
				if (ModContent.RequestIfExists<Texture2D>(thisSlot.VanityTexture, out var vanityTexture))
					texture = vanityTexture.Value;
				break;
			case -12:
				if (ModContent.RequestIfExists<Texture2D>(thisSlot.DyeTexture, out var dyeTexture))
					texture = dyeTexture.Value;
				break;
		}

		if (texture == null) {
			texture = value6;
		}
		else {
			rectangle = new Rectangle(0, 0, texture.Width, texture.Height);
			origin = rectangle.Size() / 2;
		}

		Main.spriteBatch.Draw(texture, position, rectangle, color, rotation, origin, scale, effects, layerDepth);
	}

	// Functionality Related Code /////////////////////////////////////////////////////////////////////

	public AccessorySlotType ContextToEnum(int context) => (AccessorySlotType)Math.Abs(context);

	/// <summary>
	/// Checks if the ModAccessorySlot at the given index is enabled. Does not account for the functional or vanity slots individually being disabled due to conflicts arising from shared accessory slots.
	/// </summary>
	public bool ModdedIsItemSlotUnlockedAndUsable(int index, Player player) => Get(index, player).IsEnabled();

	/// <summary>
	/// Like <see cref="ModdedIsItemSlotUnlockedAndUsable(int, Player)"/>, except this also checks if the functional or vanity slot specifically is disabled due to conflicts arising from switching loadouts while using shared accessory slots.
	/// </summary>
	public bool ModdedIsSpecificItemSlotUnlockedAndUsable(int index, Player player, bool vanity)
	{
		ModAccessorySlot slot = Get(index, player);
		ModAccessorySlotPlayer modAccessorySlotPlayer = ModSlotPlayer(player);
		return !modAccessorySlotPlayer.SharedSlotHasLoadoutConflict(index, vanity) && slot.IsEnabled();
	}

	public void CustomUpdateEquips(int index, Player player) => Get(index, player).ApplyEquipEffects();

	public bool ModdedCanSlotBeShown(int index) => Get(index).IsVisibleWhenNotEnabled();

	public bool IsHidden(int index) => Get(index).IsHidden();

	public bool CanAcceptItem(int index, Item checkItem, int context) => Get(index).CanAcceptItem(checkItem, ContextToEnum(context));

	public bool CanPlayerAcceptItem(Player player, int index, Item checkItem, int context) => Get(index, player).CanAcceptItem(checkItem, ContextToEnum(context));

	public void OnHover(int index, int context) => Get(index).OnMouseHover(ContextToEnum(context));

	/// <summary>
	/// Checks if the provided item can go in to the provided slot.
	/// Includes checking if the item already exists in either of Player.Armor or ModSlotPlayer.exAccessorySlot
	/// Invokes directly ItemSlot.AccCheck &amp; ModSlot.CanAcceptItem
	/// Note that this doesn't check for conflicts of shared slots with the items of other loadouts, that check is done in <see cref="ModAccessorySlotPlayer.DetectConflictsWithSharedSlots"/> to prevent a confusing user experience. The accessory slot acts like a disabled slot while in conflict, allowing the player to choose how to fix the issue.
	/// </summary>
	public bool ModSlotCheck(Item checkItem, int slot, int context) => CanAcceptItem(slot, checkItem, context) &&
		!ItemSlot.AccCheck_ForLocalPlayer(Player.armor.Concat(ModSlotPlayer(Player).exAccessorySlot).ToArray(), checkItem, slot + Player.armor.Length);

	private static Item dummyAccessoryCheckItem = new Item();
	/// <summary>
	/// Similar to <see cref="ModSlotCheck(Item, int, int)"/> except it ignores the item in <paramref name="slot"/> since that item is being passed in as <paramref name="checkItem"/>.
	/// </summary>
	public bool IsAccessoryInConflict(Player player, Item checkItem, int slot, int context)
	{
		if (checkItem.IsAir)
			return false;
		bool canGoInSlot = CanPlayerAcceptItem(player, slot, checkItem, context);
		if (!canGoInSlot)
			return true;
		Item[] itemCollection = player.armor.Concat(ModSlotPlayer(player).exAccessorySlot).ToArray();
		itemCollection[slot + player.armor.Length] = dummyAccessoryCheckItem;
		return ItemSlot.AccCheck_ForPlayer(player, itemCollection, checkItem, slot + player.armor.Length);
	}

	/// <summary>
	/// After checking for empty slots in ItemSlot.AccessorySwap, this allows for changing what the target slot will be if the accessory isn't already equipped.
	/// DOES NOT affect vanilla behavior of swapping items like for like where existing in a slot
	/// </summary>
	public void ModifyDefaultSwapSlot(Item item, ref int accSlotToSwapTo)
	{
		for (int num = ModSlotPlayer(Player).SlotCount - 1; num >= 0; num--) {
			if (ModdedIsSpecificItemSlotUnlockedAndUsable(num, Player, vanity: false)) {
				if (Get(num).ModifyDefaultSwapSlot(item, accSlotToSwapTo)) {
					accSlotToSwapTo = num + 20;
				}
			}
		}
	}

	// TODO: Look into if this should have an actual hook later, and which class to associate to (item or player). Not a priority to the Accessory Slot ModType PR
	/// <summary>
	/// Mirrors Player.GetPreferredGolfBallToUse.
	/// Provides the golf ball projectile from an accessory slot.
	/// </summary>
	public bool PreferredGolfBall(ref int projType)
	{
		for (int num = ModSlotPlayer(Player).SlotCount * 2 - 1; num >= 0; num--) {
			if (ModdedIsSpecificItemSlotUnlockedAndUsable(num, Player, vanity: num >= ModSlotPlayer(Player).SlotCount)) {
				Item item2 = ModSlotPlayer(Player).exAccessorySlot[num];
				if (!item2.IsAir && item2.shoot > 0 && ProjectileID.Sets.IsAGolfBall[item2.shoot]) {
					projType = item2.shoot;
					return true;
				}
			}
		}
		return false;
	}
}

public enum AccessorySlotType
{
	FunctionalSlot = 10,
	VanitySlot = 11,
	DyeSlot = 12
}