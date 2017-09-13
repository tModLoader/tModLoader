using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Terraria.UI.Gamepad;
using Terraria.GameContent.UI.States;
using Newtonsoft.Json;
using Terraria.Localization;

namespace Terraria.ModLoader.UI
{
	internal class UIModPacks : UIState
	{
		private UIList modListList;
		private UILoaderAnimatedImage uiLoader;
		private UIPanel scrollPanel;
		internal static string ModListSaveDirectory = ModLoader.ModPath + Path.DirectorySeparatorChar + "ModPacks";
		internal static TmodFile[] mods;

		public override void OnInitialize()
		{
			UIElement uIElement = new UIElement();
			uIElement.Width.Set(0f, 0.8f);
			uIElement.MaxWidth.Set(600f, 0f);
			uIElement.Top.Set(220f, 0f);
			uIElement.Height.Set(-220f, 1f);
			uIElement.HAlign = 0.5f;

			uiLoader = new UILoaderAnimatedImage(0.5f, 0.5f, 1f);

			scrollPanel = new UIPanel();
			scrollPanel.Width.Set(0f, 1f);
			scrollPanel.Height.Set(-65f, 1f);
			scrollPanel.BackgroundColor = new Color(33, 43, 79) * 0.8f;
			uIElement.Append(scrollPanel);

			modListList = new UIList();
			modListList.Width.Set(-25f, 1f);
			modListList.Height.Set(0f, 1f);
			modListList.ListPadding = 5f;
			scrollPanel.Append(modListList);

			UIScrollbar uIScrollbar = new UIScrollbar();
			uIScrollbar.SetView(100f, 1000f);
			uIScrollbar.Height.Set(0f, 1f);
			uIScrollbar.HAlign = 1f;
			scrollPanel.Append(uIScrollbar);
			modListList.SetScrollbar(uIScrollbar);

			UITextPanel<string> titleTextPanel = new UITextPanel<string>("Mod Packs", 0.8f, true);
			titleTextPanel.HAlign = 0.5f;
			titleTextPanel.Top.Set(-35f, 0f);
			titleTextPanel.SetPadding(15f);
			titleTextPanel.BackgroundColor = new Color(73, 94, 171);
			uIElement.Append(titleTextPanel);

			UITextPanel<string> backButton = new UITextPanel<string>(Language.GetTextValue("UI.Back"), 1f, false);
			backButton.Width.Set(-10f, 1f / 2f);
			backButton.Height.Set(25f, 0f);
			backButton.VAlign = 1f;
			backButton.Top.Set(-20f, 0f);
			backButton.OnMouseOver += UICommon.FadedMouseOver;
			backButton.OnMouseOut += UICommon.FadedMouseOut;
			backButton.OnClick += BackClick;
			uIElement.Append(backButton);

			UIColorTextPanel saveNewButton = new UIColorTextPanel("Save Enabled as New Mod Pack", Color.Green, 1f, false);
			saveNewButton.CopyStyle(backButton);
			saveNewButton.HAlign = 1f;
			saveNewButton.OnMouseOver += UICommon.FadedMouseOver;
			saveNewButton.OnMouseOut += UICommon.FadedMouseOut;
			saveNewButton.OnClick += SaveNewModList;
			uIElement.Append(saveNewButton);

			base.Append(uIElement);
		}

		private static void SaveNewModList(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(11, -1, -1, 1);
			Main.MenuUI.SetState(new UIVirtualKeyboard("Enter Mod Pack name", "", new UIVirtualKeyboard.KeyboardSubmitEvent(SaveModList), () => Main.menuMode = Interface.modPacksMenuID, 0));
			Main.menuMode = 888;
		}

		public static void SaveModList(string filename)
		{
			// TODO
			//Main.menuMode = Interface.modsMenuID;

			string[] enabledMods = ModLoader.FindMods()
				.Where(ModLoader.IsEnabled)
				.Select(mod => mod.name)
				.ToArray();

			//Main.PlaySound(10, -1, -1, 1);
			Directory.CreateDirectory(ModListSaveDirectory);

			string path = ModListSaveDirectory + Path.DirectorySeparatorChar + filename + ".json";

			string json = JsonConvert.SerializeObject(enabledMods, Newtonsoft.Json.Formatting.Indented);
			File.WriteAllText(path, json);

			Main.menuMode = Interface.modPacksMenuID; // should reload
		}

		private static void BackClick(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(11, -1, -1, 1);
			Main.menuMode = Interface.modsMenuID;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);
			UILinkPointNavigator.Shortcuts.BackButtonCommand = 100;
			UILinkPointNavigator.Shortcuts.BackButtonGoto = Interface.modsMenuID;
		}

		public override void OnActivate()
		{
			scrollPanel.Append(uiLoader);
			modListList.Clear();
			if (SynchronizationContext.Current == null)
				SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
			Task.Factory
				.StartNew(delegate
				{
					mods = ModLoader.FindMods();
					return FindModLists();
				})
				.ContinueWith(task =>
				{
					string[] modListsFullPath = task.Result;
					foreach (string modListFilePath in modListsFullPath)
					{
						string[] mods = { };
						//string path = ModListSaveDirectory + Path.DirectorySeparatorChar + modListFilePath + ".json";

						if (File.Exists(modListFilePath))
						{
							using (StreamReader r = new StreamReader(modListFilePath))
							{
								string json = r.ReadToEnd();
								mods = JsonConvert.DeserializeObject<string[]>(json);
							}
						}

						UIModPackItem modItem = new UIModPackItem(Path.GetFileNameWithoutExtension(modListFilePath), mods);
						modListList.Add(modItem);
					}
					scrollPanel.RemoveChild(uiLoader);
				}, TaskScheduler.FromCurrentSynchronizationContext());
		}

		private string[] FindModLists()
		{
			Directory.CreateDirectory(ModListSaveDirectory);
			string[] files = Directory.GetFiles(ModListSaveDirectory, "*.json", SearchOption.TopDirectoryOnly);
			//string[] files = Directory.GetFiles(ModListSaveDirectory, "*.json", SearchOption.TopDirectoryOnly).Select(file => Path.GetFileNameWithoutExtension(file)).ToArray();
			return files;
		}
	}
}
/*

		private static void SaveModList(UIMouseEvent evt, UIElement listeningElement)
		{
			// enabled, not necessarially loaded.
			string[] enabledMods = ModLoader.FindMods()
				.Where(ModLoader.IsEnabled)
				.Select(mod => mod.name)
				.ToArray();

			Main.PlaySound(10, -1, -1, 1);
			Directory.CreateDirectory(ModLoader.ModPath + Path.DirectorySeparatorChar + "ModLists");

			string path = ModLoader.ModPath + Path.DirectorySeparatorChar + "ModLists" + Path.DirectorySeparatorChar + "testlist.json";

			string json = JsonConvert.SerializeObject(enabledMods, Newtonsoft.Json.Formatting.Indented);
			File.WriteAllText(path, json);

		}

		private static void LoadModList(UIMouseEvent evt, UIElement listeningElement)
		{
			string[] mods = { };
			string path = ModLoader.ModPath + Path.DirectorySeparatorChar + "ModLists" + Path.DirectorySeparatorChar + "testlist.json";

			if (File.Exists(path))
			{
				using (StreamReader r = new StreamReader(path))
				{
					string json = r.ReadToEnd();
					mods = JsonConvert.DeserializeObject<string[]>(json);
				}
			}

			// Actually Set mods to enabled.

			Main.PlaySound(11, -1, -1, 1);
			Interface.infoMessage.SetMessage($"The Following mods were enabled:\n{String.Join(", ", mods)}\n, some disabled: ");
			Interface.infoMessage.SetGotoMenu(Interface.reloadModsID);
			Main.menuMode = Interface.infoMessageID;
		}
		*/
