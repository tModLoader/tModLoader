using System;
using System.IO;
using System.Net;
using Terraria.ID;
using Terraria.Localization;
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
		//internal const int modControlsID = 10010;
		internal const int managePublishedID = 10011;
		internal const int updateMessageID = 10012;
		internal const int infoMessageID = 10013;
		internal const int advancedInfoMessageID = 10014;
		internal const int enterPassphraseMenuID = 10015;
		internal const int modPacksMenuID = 10016;
		internal const int tModLoaderSettingsID = 10017;
		internal const int enterSteamIDMenuID = 10018;
		internal const int extractModID = 10019;
		internal const int downloadModsID = 10020;
		internal static UIMods modsMenu = new UIMods();
		internal static UILoadMods loadMods = new UILoadMods();
		private static UIModSources modSources = new UIModSources();
		internal static UIBuildMod buildMod = new UIBuildMod();
		internal static UIErrorMessage errorMessage = new UIErrorMessage();
		internal static UIModBrowser modBrowser = new UIModBrowser();
		internal static UIModInfo modInfo = new UIModInfo();
		internal static UIDownloadMod downloadMod = new UIDownloadMod();
		internal static UIManagePublished managePublished = new UIManagePublished();
		internal static UIUpdateMessage updateMessage = new UIUpdateMessage();
		internal static UIInfoMessage infoMessage = new UIInfoMessage();
		internal static UIAdvancedInfoMessage advancedInfoMessage = new UIAdvancedInfoMessage();
		internal static UIEnterPassphraseMenu enterPassphraseMenu = new UIEnterPassphraseMenu();
		internal static UIModPacks modPacksMenu = new UIModPacks();
		internal static UIEnterSteamIDMenu enterSteamIDMenu = new UIEnterSteamIDMenu();
		internal static UIExtractMod extractMod = new UIExtractMod();
		internal static UIDownloadMods downloadMods = new UIDownloadMods();
		//add to Terraria.Main.DrawMenu in Main.menuMode == 0 after achievements
		//Interface.AddMenuButtons(this, this.selectedMenu, array9, array7, ref num, ref num3, ref num10, ref num5);
		internal static void AddMenuButtons(Main main, int selectedMenu, string[] buttonNames, float[] buttonScales, ref int offY, ref int spacing, ref int buttonIndex, ref int numButtons)
		{
			buttonNames[buttonIndex] = Language.GetTextValue("tModLoader.MenuMods");
			if (selectedMenu == buttonIndex)
			{
				Main.PlaySound(10, -1, -1, 1);
				Main.menuMode = modsMenuID;
			}
			buttonIndex++;
			numButtons++;
			buttonNames[buttonIndex] = Language.GetTextValue("tModLoader.MenuModSources");
			if (selectedMenu == buttonIndex)
			{
				Main.PlaySound(10, -1, -1, 1);
				Main.menuMode = modSourcesID;
			}
			buttonIndex++;
			numButtons++;
			buttonNames[buttonIndex] = Language.GetTextValue("tModLoader.MenuModBrowser");
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

		internal static void ResetData()
		{
			modBrowser.modList?.Clear();
			modBrowser.sortMode = ModBrowserSortMode.RecentlyUpdated;
			modBrowser.updateFilterMode = UpdateFilter.Available;
			modBrowser.searchFilterMode = SearchFilter.Name;
			modBrowser.SearchFilterToggle?.setCurrentState(0);
			if (modBrowser._categoryButtons.Count == 2)
			{
				modBrowser._categoryButtons[0].setCurrentState(4);
				modBrowser._categoryButtons[1].setCurrentState(1);
			}
			modBrowser.loading = false;
			ModLoader.findModsCache.Clear();
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
		}

		//internal static void AddSettingsMenuButtons(Main main, int selectedMenu, string[] buttonNames, float[] buttonScales, int[] virticalSpacing, ref int offY, ref int spacing, ref int buttonIndex, ref int numButtons)
		//{
		//	buttonIndex++;
		//	numButtons++;
		//	buttonNames[buttonIndex] = "Mod " + Lang.menu[66];
		//	if (selectedMenu == buttonIndex)
		//	{
		//		Main.PlaySound(10, -1, -1, 1);
		//		Main.menuMode = modControlsID;
		//	}
		//	for (int k = 0; k < numButtons; k++)
		//	{
		//		buttonScales[k] = 0.73f;
		//		virticalSpacing[k] = 0;
		//	}
		//	virticalSpacing[numButtons - 1] = 8;
		//}

		//add to end of if else chain of Main.menuMode in Terraria.Main.DrawMenu
		//Interface.ModLoaderMenus(this, this.selectedMenu, array9, array7, array4, ref num2, ref num4, ref num5, ref flag5);
		internal static void ModLoaderMenus(Main main, int selectedMenu, string[] buttonNames, float[] buttonScales, int[] buttonVerticalSpacing, ref int offY, ref int spacing, ref int numButtons, ref bool backButtonDown)
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
			else if (Main.menuMode == downloadModsID)
			{
				Main.menuMode = 888;
				Main.MenuUI.SetState(downloadMods);
			}
			else if (Main.menuMode == managePublishedID)
			{
				Main.MenuUI.SetState(managePublished);
				Main.menuMode = 888;
			}
			//else if (Main.menuMode == modControlsID)
			//{
			//	UIModControls.ModLoaderMenus(main, selectedMenu, buttonNames, buttonScales, buttonVerticalSpacing, ref offY, ref spacing, ref numButtons);
			//}
			else if (Main.menuMode == updateMessageID)
			{
				Main.MenuUI.SetState(updateMessage);
				Main.menuMode = 888;
			}
			else if (Main.menuMode == infoMessageID)
			{
				Main.MenuUI.SetState(infoMessage);
				Main.menuMode = 888;
			}
			else if (Main.menuMode == advancedInfoMessageID)
			{
				Main.MenuUI.SetState(advancedInfoMessage);
				Main.menuMode = 888;
			}
			else if (Main.menuMode == enterPassphraseMenuID)
			{
				Main.MenuUI.SetState(enterPassphraseMenu);
				Main.menuMode = 888;
			}
			else if (Main.menuMode == enterSteamIDMenuID)
			{
				Main.MenuUI.SetState(enterSteamIDMenu);
				Main.menuMode = 888;
			}
			else if (Main.menuMode == modPacksMenuID)
			{
				Main.MenuUI.SetState(modPacksMenu);
				Main.menuMode = 888;
			}
			else if (Main.menuMode == extractModID)
			{
				Main.MenuUI.SetState(extractMod);
				Main.menuMode = 888;
			}
			else if (Main.menuMode == tModLoaderSettingsID)
			{
				offY = 210;
				spacing = 42;
				numButtons = 6;
				buttonVerticalSpacing[numButtons - 1] = 18;
				for (int i = 0; i < numButtons; i++)
				{
					buttonScales[i] = 0.75f;
				}
				int buttonIndex = 0;
				buttonNames[buttonIndex] = (ModNet.downloadModsFromServers ? Language.GetTextValue("tModLoader.DownloadFromServersYes") : Language.GetTextValue("tModLoader.DownloadFromServersNo"));
				if (selectedMenu == buttonIndex)
				{
					Main.PlaySound(SoundID.MenuTick);
					ModNet.downloadModsFromServers = !ModNet.downloadModsFromServers;
				}

				buttonIndex++;
				buttonNames[buttonIndex] = (ModNet.onlyDownloadSignedMods ? Language.GetTextValue("tModLoader.DownloadSignedYes") : Language.GetTextValue("tModLoader.DownloadSignedNo"));
				if (selectedMenu == buttonIndex)
				{
					Main.PlaySound(SoundID.MenuTick);
					ModNet.onlyDownloadSignedMods = !ModNet.onlyDownloadSignedMods;
				}

				buttonIndex++;
				buttonNames[buttonIndex] = (ModLoader.musicStreamMode == 0 ? Language.GetTextValue("tModLoader.MusicStreamModeConvert") : Language.GetTextValue("tModLoader.MusicStreamModeStream"));
				if (selectedMenu == buttonIndex)
				{
					Main.PlaySound(SoundID.MenuTick);
					ModLoader.musicStreamMode = (byte)((ModLoader.musicStreamMode + 1) % 2);
				}

				buttonIndex++;
				buttonNames[buttonIndex] = (Main.UseExperimentalFeatures ? Language.GetTextValue("tModLoader.ExperimentalFeaturesYes") : Language.GetTextValue("tModLoader.ExperimentalFeaturesNo"));
				if (selectedMenu == buttonIndex)
				{
					Main.PlaySound(SoundID.MenuTick);
					Main.UseExperimentalFeatures = !Main.UseExperimentalFeatures;
				}

				buttonIndex++;
				buttonNames[buttonIndex] = Language.GetTextValue("tModLoader.ClearMBCredentials");
				if (selectedMenu == buttonIndex)
				{
					Main.PlaySound(SoundID.MenuTick);
					ModLoader.modBrowserPassphrase = "";
					ModLoader.SteamID64 = "";
				}

				buttonIndex++;
				buttonNames[buttonIndex] = Lang.menu[5].Value;
				if (selectedMenu == buttonIndex || backButtonDown)
				{
					backButtonDown = false;
					Main.menuMode = 11;
					Main.PlaySound(11, -1, -1, 1);
				}
			}
		}

		internal static void ServerModMenu()
		{
			bool exit = false;
			while (!exit)
			{
				Console.WriteLine("Terraria Server " + Main.versionNumber2 + " - " + ModLoader.versionedName);
				Console.WriteLine();
				TmodFile[] mods = ModLoader.FindMods();
				for (int k = 0; k < mods.Length; k++)
				{
					BuildProperties properties = BuildProperties.ReadModFile(mods[k]);
					string name = properties.displayName;
					name = mods[k].name;
					string line = (k + 1) + "\t\t" + name + "(";
					line += (ModLoader.IsEnabled(mods[k]) ? "enabled" : "disabled") + ")";
					Console.WriteLine(line);
				}
				if (mods.Length == 0)
				{
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine($"No mods were found in: \"{ModLoader.ModPath}\"\nIf you are running a dedicated server, you may wish to use the 'modpath' command line switch or server config setting to specify a custom mods directory.\n");
					Console.ResetColor();
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
						ModLoader.EnableMod(mod);
					}
				}
				else if (command == "d")
				{
					foreach (TmodFile mod in mods)
					{
						ModLoader.DisableMod(mod);
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
							ModLoader.SetModActive(mods[value], !ModLoader.IsEnabled(mods[value]));
						}
					}
				}
			}
		}

		internal static void ServerModBrowserMenu()
		{
			bool exit = false;
			Console.Clear();
			while (!exit)
			{
				Console.WriteLine();
				Console.WriteLine("b\t\tReturn to world menu");
				Console.WriteLine();
				Console.WriteLine("Type an exact ModName to download: ");
				string command = Console.ReadLine();
				if (command == null)
				{
					command = "";
				}
				if (command == "b" || command == "B")
				{
					exit = true;
				}
				else
				{
					string modname = command;
					try
					{
						System.Net.ServicePointManager.ServerCertificateValidationCallback = (o, certificate, chain, errors) => true;
						using (WebClient client = new WebClient())
						{
							string downloadURL = client.DownloadString($"http://javid.ddns.net/tModLoader/tools/querymoddownloadurl.php?modname={modname}");
							if (downloadURL.StartsWith("Failed"))
							{
								Console.WriteLine(downloadURL);
							}
							else
							{
								string tempFile = ModLoader.ModPath + Path.DirectorySeparatorChar + "temporaryDownload.tmod";
								client.DownloadFile(downloadURL, tempFile);
								File.Copy(tempFile, ModLoader.ModPath + Path.DirectorySeparatorChar + downloadURL.Substring(downloadURL.LastIndexOf("/")), true);
								File.Delete(tempFile);
							}
							while (Console.KeyAvailable)
								Console.ReadKey(true);
						}
					}
					catch (Exception e)
					{
						Console.WriteLine("Error: Could not download " + modname + " -- " + e.ToString());
					}
				}
			}
			Console.Clear();
		}
	}
}
