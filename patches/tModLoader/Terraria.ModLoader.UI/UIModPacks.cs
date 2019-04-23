using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace Terraria.ModLoader.UI
{
	internal class UIModPacks : UIState
	{
		private UIList modListList;
		private UILoaderAnimatedImage uiLoader;
		private UIPanel scrollPanel;
		internal static string ModListSaveDirectory = ModLoader.ModPath + Path.DirectorySeparatorChar + "ModPacks";
		internal static string[] mods;

		public override void OnInitialize() {
			var uIElement = new UIElement {
				Width = { Percent = 0.8f },
				MaxWidth = UICommon.MaxPanelWidth,
				Top = { Pixels = 220 },
				Height = { Pixels = -220, Percent = 1f },
				HAlign = 0.5f
			};

			uiLoader = new UILoaderAnimatedImage(0.5f, 0.5f, 1f);

			scrollPanel = new UIPanel {
				Width = { Percent = 1f },
				Height = { Pixels = -65, Percent = 1f },
				BackgroundColor = UICommon.mainPanelBackground
			};
			uIElement.Append(scrollPanel);

			modListList = new UIList {
				Width = { Pixels = -25, Percent = 1f },
				Height = { Percent = 1f },
				ListPadding = 5f
			};
			scrollPanel.Append(modListList);

			var uIScrollbar = new UIScrollbar {
				Height = { Percent = 1f },
				HAlign = 1f
			}.WithView(100f, 1000f);
			scrollPanel.Append(uIScrollbar);
			modListList.SetScrollbar(uIScrollbar);

			var titleTextPanel = new UITextPanel<string>(Language.GetTextValue("tModLoader.ModPacksHeader"), 0.8f, true) {
				HAlign = 0.5f,
				Top = { Pixels = -35 },
				BackgroundColor = UICommon.defaultUIBlue
			}.WithPadding(15f);
			uIElement.Append(titleTextPanel);

			var backButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("UI.Back")) {
				Width = new StyleDimension(-10f, 1f / 2f),
				Height = { Pixels = 40 },
				VAlign = 1f,
				Top = { Pixels = -20 }
			}.WithFadedMouseOver();
			backButton.OnClick += BackClick;
			uIElement.Append(backButton);

			var saveNewButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.ModPacksSaveEnabledAsNewPack"));
			saveNewButton.CopyStyle(backButton);
			saveNewButton.TextColor = Color.Green;
			saveNewButton.HAlign = 1f;
			saveNewButton.WithFadedMouseOver();
			saveNewButton.OnClick += SaveNewModList;
			uIElement.Append(saveNewButton);

			Append(uIElement);
		}

		private static void SaveNewModList(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(11, -1, -1, 1);
			Main.MenuUI.SetState(new UIVirtualKeyboard(Language.GetTextValue("tModLoader.ModPacksEnterModPackName"), "", new UIVirtualKeyboard.KeyboardSubmitEvent(SaveModList), () => Main.menuMode = Interface.modPacksMenuID, 0));
			Main.menuMode = 888;
		}

		public static void SaveModList(string filename) {
			// TODO
			//Main.menuMode = Interface.modsMenuID;

			//Main.PlaySound(10, -1, -1, 1);
			Directory.CreateDirectory(ModListSaveDirectory);

			string path = ModListSaveDirectory + Path.DirectorySeparatorChar + filename + ".json";
			var foundMods = ModOrganizer.FindMods().Select(x => x.Name).Intersect(ModLoader.EnabledMods).ToList();
			string json = JsonConvert.SerializeObject(foundMods, Formatting.Indented);
			File.WriteAllText(path, json);

			Main.menuMode = Interface.modPacksMenuID; // should reload
		}

		private static void BackClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(11, -1, -1, 1);
			Main.menuMode = Interface.modsMenuID;
		}

		public override void Draw(SpriteBatch spriteBatch) {
			base.Draw(spriteBatch);
			UILinkPointNavigator.Shortcuts.BackButtonCommand = 100;
			UILinkPointNavigator.Shortcuts.BackButtonGoto = Interface.modsMenuID;
		}

		public override void OnActivate() {
			scrollPanel.Append(uiLoader);
			modListList.Clear();
			Task.Factory
				.StartNew(delegate {
					mods = ModOrganizer.FindMods().Select(m => m.Name).ToArray();
					return FindModLists();
				})
				.ContinueWith(task => {
					string[] modListsFullPath = task.Result;
					foreach (string modListFilePath in modListsFullPath) {
						try {
							string[] mods = { };
							//string path = ModListSaveDirectory + Path.DirectorySeparatorChar + modListFilePath + ".json";

							if (File.Exists(modListFilePath)) {
								using (StreamReader r = new StreamReader(modListFilePath)) {
									string json = r.ReadToEnd();

									mods = JsonConvert.DeserializeObject<string[]>(json);
								}
							}
							UIModPackItem modItem = new UIModPackItem(Path.GetFileNameWithoutExtension(modListFilePath), mods);
							modListList.Add(modItem);
						}
						catch {
							var badModPackMessage = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.ModPackMalformed", Path.GetFileName(modListFilePath))) {
								Width = { Percent = 1 },
								Height = { Pixels = 50, Percent = 0 }
							};
							modListList.Add(badModPackMessage);
						}
					}
					scrollPanel.RemoveChild(uiLoader);
				}, TaskScheduler.FromCurrentSynchronizationContext());
		}

		private string[] FindModLists() {
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
