using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terraria.ModLoader.UI
{
	internal static class UIModControls
	{
		internal static void ModLoaderMenus(Main main, int selectedMenu, string[] buttonNames, float[] buttonScales, int[] buttonVerticalSpacing, ref int offY, ref int spacing, ref int numButtons)
		{
			string[] currentBinding = new string[ModLoader.modHotKeys.Count];
			int j = 0;
			foreach (var a in ModLoader.modHotKeys.Values)
			{
				currentBinding[j] = a.Item2;
				j++;
			}
			string[] hotKeyName = ModLoader.modHotKeys.Keys.ToArray<string>();
			offY = 176;
			spacing = 22;
			numButtons = 2 + ModLoader.modHotKeys.Count;
			if (Main.setKey >= 0)
			{
				currentBinding[Main.setKey] = "_";
			}
			for (int i = 0; i < hotKeyName.Length; i++)
			{
				buttonNames[i] = hotKeyName[i] + (hotKeyName[i].Length <= 18 ? "                    ".Substring(hotKeyName[i].Length) : "  ") + currentBinding[i];
			}
			for (int num26 = 0; num26 < numButtons - 2; num26++)
			{
				//		array8[num26] = true; // left aligned
				buttonScales[num26] = 0.45f;
				//		array5[num26] = -80; // alignment?
			}
			buttonScales[numButtons - 2] = 0.8f;
			//	array4[14] = 6;
			buttonNames[numButtons - 2] = Lang.menu[86];
			buttonScales[numButtons - 1] = 0.8f;
			//	array4[15] = 16;
			buttonVerticalSpacing[numButtons - 1] = 20;
			buttonNames[numButtons - 1] = Lang.menu[5];
			if (selectedMenu == numButtons - 1)
			{
				Main.menuMode = 11;
				Main.PlaySound(11, -1, -1, 1);
			}
			else if (selectedMenu == numButtons - 2)
			{
				foreach (string key in ModLoader.modHotKeys.Keys.ToList())
				{
					ModLoader.modHotKeys[key] = new Tuple<Mod, string, string>(ModLoader.modHotKeys[key].Item1, ModLoader.modHotKeys[key].Item3, ModLoader.modHotKeys[key].Item3);
				}
				Main.setKey = -1;
				Main.PlaySound(11, -1, -1, 1);
			}
			else if (selectedMenu >= 0)
			{
				Main.setKey = selectedMenu;
			}
			if (Main.setKey >= 0)
			{
				Microsoft.Xna.Framework.Input.Keys[] pressedKeys2 = Main.keyState.GetPressedKeys();
				if (pressedKeys2.Length > 0)
				{
					string a3 = string.Concat(pressedKeys2[0]);
					if (a3 != "None")
					{
						ModLoader.modHotKeys[ModLoader.modHotKeys.ElementAt(Main.setKey).Key] = new Tuple<Mod, string, string>(ModLoader.modHotKeys.ElementAt(Main.setKey).Value.Item1, a3, ModLoader.modHotKeys.ElementAt(Main.setKey).Value.Item3);
						Main.setKey = -1;
					}
				}
			}
		}
	}
}
