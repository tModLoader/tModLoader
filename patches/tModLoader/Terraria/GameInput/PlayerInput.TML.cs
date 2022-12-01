using System.Collections.Generic;

namespace Terraria.GameInput;

public partial class PlayerInput
{
	internal static List<string> MouseInModdedUI = new();

	static PlayerInput()
	{
		InsertExtraMouseButtonsIntoTriggerList(KnownTriggers);
	}

	/// <summary>
	/// Locks the vanilla scrollbar for the upcoming cycle when called. Autoclears in Player.
	/// Takes a string to denote that your UI has registered to lock vanilla scrolling. String does not need to be unique.
	/// </summary>
	public static void LockVanillaMouseScroll(string myUI)
	{
		if (!MouseInModdedUI.Contains(myUI))
			MouseInModdedUI.Add(myUI);
	}

	internal static void InsertExtraMouseButtonsIntoTriggerList(List<string> list)
	{
		int insertionIndex = list.FindLastIndex(s => s.Contains("Mouse")) + 1;

		list.InsertRange(insertionIndex, new[] {
			TriggerNames.MouseMiddle,
			TriggerNames.MouseXButton1,
			TriggerNames.MouseXButton2,
		});
	}

	private static void OnReset(KeyConfiguration c, InputMode mode)
	{
		if (mode != InputMode.Keyboard && mode != InputMode.KeyboardUI) {
			return;
		}

		c.KeyStatus[TriggerNames.MouseMiddle].Add("Mouse3");
		c.KeyStatus[TriggerNames.MouseXButton1].Add("Mouse4");
		c.KeyStatus[TriggerNames.MouseXButton2].Add("Mouse5");
	}
}
