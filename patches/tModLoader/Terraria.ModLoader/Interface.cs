using System;
using System.IO;
using Terraria;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.UI;

namespace Terraria.ModLoader
{
	internal static class Interface
	{
		internal const int modsMenuID = 10000;
		internal const int modSourcesID = 10001;
		//set initial Main.menuMode to loadModsID
		internal const int loadModsID = 10002;
		internal const int buildModID = 10003;
		internal const int buildAllModsID = 10004;
		internal const int errorMessageID = 10005;
		internal const int reloadModsID = 10006;
		internal const int modBrowserID = 10007;
		internal const int modInfoID = 10008;
		internal const int downloadModID = 10009;
		internal const int modControlsID = 10010;
		internal const int managePublishedID = 10011;
		internal const int updateMessageID = 10012;
		private static UIMods modsMenu = new UIMods();
		internal static UILoadMods loadMods = new UILoadMods();
		private static UIModSources modSources = new UIModSources();
		internal static UIBuildMod buildMod = new UIBuildMod();
		internal static UIErrorMessage errorMessage = new UIErrorMessage();
		internal static UIModBrowser modBrowser = new UIModBrowser();
		internal static UIModInfo modInfo = new UIModInfo();
		internal static UIDownloadMod downloadMod = new UIDownloadMod();
		internal static UIManagePublished managePublished = new UIManagePublished();
		internal static UIUpdateMessage updateMessage = new UIUpdateMessage();
		//add to Terraria.Main.DrawMenu in Main.menuMode == 0 after achievements
		//Interface.AddMenuButtons(this, this.selectedMenu, array9, array7, ref num, ref num3, ref num9, ref num4);
		internal static void AddMenuButtons(Main main, int selectedMenu, string[] buttonNames, float[] buttonScales, ref int offY, ref int spacing, ref int buttonIndex, ref int numButtons)
		{
			buttonNames[buttonIndex] = "Mods";
			if (selectedMenu == buttonIndex)
			{
				Main.PlaySound(10, -1, -1, 1);
				Main.menuMode = modsMenuID;
			}
			buttonIndex++;
			numButtons++;
			buttonNames[buttonIndex] = "Mod Sources";
			if (selectedMenu == buttonIndex)
			{
				Main.PlaySound(10, -1, -1, 1);
				Main.menuMode = modSourcesID;
			}
			buttonIndex++;
			numButtons++;
			buttonNames[buttonIndex] = "Mod Browser (Beta)";
			if (selectedMenu == buttonIndex)
			{
				Main.PlaySound(10, -1, -1, 1);
				Main.menuMode = modBrowserID;
			}
			buttonIndex++;
			numButtons++;
			offY = 220;
			for (int k = 0; k < numButtons; k++)
			{
				buttonScales[k] = 0.82f;
			}
			spacing = 45;
		}

		internal static void AddSettingsMenuButtons(Main main, int selectedMenu, string[] buttonNames, float[] buttonScales, int[] virticalSpacing, ref int offY, ref int spacing, ref int buttonIndex, ref int numButtons)
		{
			buttonIndex++;
			numButtons++;
			buttonNames[buttonIndex] = "Mod " + Lang.menu[66];
			if (selectedMenu == buttonIndex)
			{
				Main.PlaySound(10, -1, -1, 1);
				Main.menuMode = modControlsID;
			}
			for (int k = 0; k < numButtons; k++)
			{
				buttonScales[k] = 0.73f;
				virticalSpacing[k] = 0;
			}
			virticalSpacing[numButtons - 1] = 8;
		}
		//add to end of if else chain of Main.menuMode in Terraria.Main.DrawMenu
		//Interface.ModLoaderMenus(this, this.selectedMenu, array9, array7, ref num, ref num3, ref num4);
		internal static void ModLoaderMenus(Main main, int selectedMenu, string[] buttonNames, float[] buttonScales, int[] buttonVerticalSpacing, ref int offY, ref int spacing, ref int numButtons)
		{
			if (Main.menuMode == modsMenuID)
			{
				Main.MenuUI.SetState(modsMenu);
				Main.menuMode = 888;
			}
			else if (Main.menuMode == modSourcesID)
			{
				Main.MenuUI.SetState(modSources);
				Main.menuMode = 888;
			}
			else if (Main.menuMode == loadModsID)
			{
				Main.MenuUI.SetState(loadMods);
				Main.menuMode = 888;
				ModLoader.Load();
			}
			else if (Main.menuMode == buildModID)
			{
				Main.MenuUI.SetState(buildMod);
				Main.menuMode = 888;
				ModLoader.BuildMod();
			}
			else if (Main.menuMode == buildAllModsID)
			{
				Main.MenuUI.SetState(buildMod);
				Main.menuMode = 888;
				ModLoader.BuildAllMods();
			}
			else if (Main.menuMode == errorMessageID)
			{
				Main.MenuUI.SetState(errorMessage);
				Main.menuMode = 888;
			}
			else if (Main.menuMode == reloadModsID)
			{
				ModLoader.Reload();
			}
			else if (Main.menuMode == modBrowserID)
			{
				Main.MenuUI.SetState(modBrowser);
				Main.menuMode = 888;
			}
			else if (Main.menuMode == modInfoID)
			{
				Main.MenuUI.SetState(modInfo);
				Main.menuMode = 888;
			}
			else if (Main.menuMode == downloadModID)
			{
				Main.MenuUI.SetState(downloadMod);
				Main.menuMode = 888;
			}
			else if (Main.menuMode == managePublishedID)
			{
				Main.MenuUI.SetState(managePublished);
				Main.menuMode = 888;
			}
			else if (Main.menuMode == modControlsID)
			{
				UIModControls.ModLoaderMenus(main, selectedMenu, buttonNames, buttonScales, buttonVerticalSpacing, ref offY, ref spacing, ref numButtons);
			}
			else if (Main.menuMode == updateMessageID)
			{
				Main.MenuUI.SetState(updateMessage);
				Main.menuMode = 888;
			}
		}

		internal static void ServerModMenu()
		{
			bool exit = false;
			while (!exit)
			{
				Console.WriteLine("Terraria Server " + Main.versionNumber2 + " - " + ModLoader.version);
				Console.WriteLine();
				TmodFile[] mods = ModLoader.FindMods();
				for (int k = 0; k < mods.Length; k++)
				{
					BuildProperties properties = BuildProperties.ReadModFile(mods[k]);
					string name = properties.displayName;
					if (name.Length == 0)
					{
						name = Path.GetFileNameWithoutExtension(mods[k].Name);
					}
					string line = (k + 1) + "\t\t" + name + "(";
					line += (ModLoader.IsEnabled(mods[k].Name) ? "enabled" : "disabled") + ")";
					Console.WriteLine(line);
				}
				Console.WriteLine("e\t\tEnable All");
				Console.WriteLine("d\t\tDisable All");
				Console.WriteLine("r\t\tReload and return to world menu");
				Console.WriteLine("Type a number to switch between enabled/disabled");
				Console.WriteLine();
				Console.WriteLine("Type a command: ");
				string command = Console.ReadLine();
				if (command == null)
				{
					command = "";
				}
				command = command.ToLower();
				Console.Clear();
				if (command == "e")
				{
					foreach (TmodFile mod in mods)
					{
						ModLoader.EnableMod(mod.Name);
					}
				}
				else if (command == "d")
				{
					foreach (TmodFile mod in mods)
					{
						ModLoader.DisableMod(mod.Name);
					}
				}
				else if (command == "r")
				{
					Console.WriteLine("Unloading mods...");
					ModLoader.Unload();
					ModLoader.do_Load(null);
					exit = true;
				}
				else
				{
					int value;
					if (Int32.TryParse(command, out value))
					{
						value--;
						if (value >= 0 && value < mods.Length)
						{
							ModLoader.SetModActive(mods[value].Name, !ModLoader.IsEnabled(mods[value].Name));
						}
					}
				}
			}
		}
	}
}
