//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//TODO UNUSED delete?
//namespace Terraria.ModLoader.UI
//{
//	internal static class UIModControls
//	{
//		const int ControlsPerPage = 10;
//		static int page = 0;
//		internal static void ModLoaderMenus(Main main, int selectedMenu, string[] buttonNames, float[] buttonScales, int[] buttonVerticalSpacing, ref int offY, ref int spacing, ref int numButtons)
//		{
//			offY = 176;
//			spacing = 22;

//			int TotalModHotKeys = ModLoader.modHotKeys.Count;
//			bool Multipage = TotalModHotKeys > ControlsPerPage;
//			if (page * ControlsPerPage >= TotalModHotKeys)
//			{
//				page = 0;
//			}
//			int ShownModHotKeys = Math.Min(ControlsPerPage, TotalModHotKeys - page * ControlsPerPage);
//			numButtons = 2 + ShownModHotKeys + (Multipage ? 1 : 0);
//			int startIndex = page * ControlsPerPage;
//			int endIndex = page * ControlsPerPage + ShownModHotKeys;

//			int k = 0;
//			int j = 0;
//			string[] currentBinding = new string[ShownModHotKeys];
//			string[] hotKeyName = new string[ShownModHotKeys];
//			foreach (var a in ModLoader.modHotKeys)
//			{
//				if (j >= startIndex && j < endIndex)
//				{
//					currentBinding[k] = a.Value.Item2;
//					hotKeyName[k] = a.Key;
//					k++;
//				}
//				j++;
//			}
//			//string[] hotKeyName = ModLoader.modHotKeys.Keys.ToArray<string>();

//			if (Main.setKey >= 0)
//			{
//				currentBinding[Main.setKey] = "_";
//			}
//			for (int i = 0; i < ShownModHotKeys; i++)
//			{
//				buttonNames[i] = hotKeyName[i] + (hotKeyName[i].Length <= 18 ? "                    ".Substring(hotKeyName[i].Length) : "  ") + currentBinding[i];
//			}
//			for (int i = 0; i < numButtons; i++)
//			{
//				//array8[num26] = true; // left aligned
//				buttonScales[i] = 0.45f;
//				//		array5[num26] = -80; // alignment?
//			}
//			if (Multipage)
//			{
//				buttonVerticalSpacing[numButtons - 3] = 30;
//				buttonScales[numButtons - 3] = 0.8f;
//				buttonNames[numButtons - 3] = "Next Page";
//			}
//			buttonVerticalSpacing[numButtons - 2] = 60;
//			buttonScales[numButtons - 2] = 0.8f;
//			//	array4[14] = 6;
//			buttonNames[numButtons - 2] = Lang.menu[86]; // "Reset to Default"
//			buttonScales[numButtons - 1] = 0.8f;
//			//	array4[15] = 16;
//			buttonVerticalSpacing[numButtons - 1] = 90;
//			buttonNames[numButtons - 1] = Lang.menu[5]; // "Back"
//			if (selectedMenu == numButtons - 1)
//			{
//				Main.setKey = -1;
//				Main.menuMode = 11;
//				Main.PlaySound(11, -1, -1, 1);
//			}
//			else if (selectedMenu == numButtons - 2)
//			{
//				foreach (string key in ModLoader.modHotKeys.Keys.ToList())
//				{
//					ModLoader.modHotKeys[key] = new Tuple<Mod, string, string>(ModLoader.modHotKeys[key].Item1, ModLoader.modHotKeys[key].Item3, ModLoader.modHotKeys[key].Item3);
//				}
//				Main.setKey = -1;
//				Main.PlaySound(11, -1, -1, 1);
//			}
//			else if (Multipage && selectedMenu == numButtons - 3)
//			{
//				page++;
//				Main.setKey = -1;
//				Main.PlaySound(11, -1, -1, 1);
//			}
//			else if (selectedMenu >= 0)
//			{
//				Main.setKey = selectedMenu;
//			}
//			if (Main.setKey >= 0)
//			{
//				Microsoft.Xna.Framework.Input.Keys[] pressedKeys2 = Main.keyState.GetPressedKeys();
//				if (pressedKeys2.Length > 0)
//				{
//					string a3 = string.Concat(pressedKeys2[0]);
//					if (a3 != "None")
//					{
//						ModLoader.modHotKeys[ModLoader.modHotKeys.ElementAt(Main.setKey + page * ControlsPerPage).Key] = new Tuple<Mod, string, string>(ModLoader.modHotKeys.ElementAt(Main.setKey + page * ControlsPerPage).Value.Item1, a3, ModLoader.modHotKeys.ElementAt(Main.setKey + page * ControlsPerPage).Value.Item3);
//						Main.setKey = -1;
//					}
//				}
//			}
//		}
//	}
//}
