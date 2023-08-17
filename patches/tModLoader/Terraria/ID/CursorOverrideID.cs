using Terraria.GameInput;

namespace Terraria.ID;

public class CursorOverrideID
{
	/// <summary>
	/// Default cursor
	/// </summary>
	public const short DefaultCursor = 0;
	/// <summary>
	/// Smart cursor enabled
	/// </summary>
	public const short SmartCursor = 1;
	/// <summary>
	/// Try to display items in chat messages
	/// </summary>
	public const short Magnifiers = 2;
	/// <summary>
	/// Try to mark as favorite
	/// </summary>
	public const short FavoriteStar = 3;
	/// <summary>
	/// The first in-game camera point
	/// <br>If <see cref="Main.cursorOverride"/> is set to this, the color will be the same as the cursor color</br>
	/// </summary>
	public const short CameraLight = 4;
	/// <summary>
	/// The second in-game camera point
	/// <br>If <see cref="Main.cursorOverride"/> is set to this, the color will be the same as the cursor color</br>
	/// </summary>
	public const short CameraDark = 5;
	/// <summary>
	/// Quick trash
	/// </summary>
	public const short TrashCan = 6;
	/// <summary>
	/// From guide slot, research slot, reforge slot, etc. to inventory
	/// </summary>
	public const short BackInventory = 7;
	/// <summary>
	/// From chest to inventory
	/// </summary>
	public const short ChestToInventory = 8;
	/// <summary>
	/// From inventory to chest
	/// </summary>
	public const short InventoryToChest = 9;
	/// <summary>
	/// Quick sell items to NPC
	/// </summary>
	public const short QuickSell = 10;
	/// <summary>
	/// Default cursor outline
	/// </summary>
	public const short DefaultCursorOutline = 11;
	/// <summary>
	/// Smart cursor outline
	/// </summary>
	public const short SmartCursorOutline = 12;
	/// <summary>
	/// Smart cursor enabled if <see cref="PlayerInput.SettingsForUI.ShowGamepadCursor"/> is true
	/// <br>When using the gamepad in smart cursor mode, the cursor that is relatively close to the player</br>
	/// </summary>
	public const short GamepadSmartCursor = 13;
	/// <summary>
	/// Smart cursor outline if <see cref="PlayerInput.SettingsForUI.ShowGamepadCursor"/> is true
	/// </summary>
	public const short GamepadSmartCursorOutline = 14;
	/// <summary>
	/// Default cursor if <see cref="PlayerInput.SettingsForUI.ShowGamepadCursor"/> is true
	/// </summary>
	public const short GamepadDefaultCursor = 15;
	/// <summary>
	/// Default cursor outline if <see cref="PlayerInput.SettingsForUI.ShowGamepadCursor"/> is true
	/// </summary>
	public const short GamepadDefaultCursorOutline = 16;
	/// <summary>
	/// Actual cursor position indicator if smart cursor enabled and <see cref="PlayerInput.SettingsForUI.ShowGamepadCursor"/> is true
	/// <br>When using the gamepad in smart cursor mode, the cursor that is relatively far away from the player</br>
	/// </summary>
	public const short GamepadSmartCursorAlt = 17;
}
