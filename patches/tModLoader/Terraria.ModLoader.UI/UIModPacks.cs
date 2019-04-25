using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
		private UIList modPacks;
		private UILoaderAnimatedImage uiLoader;
		private UIPanel scrollPanel;
		internal static string ModPacksDirectory = Path.Combine(ModLoader.ModPath, "ModPacks");
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

			modPacks = new UIList {
				Width = { Pixels = -25, Percent = 1f },
				Height = { Percent = 1f },
				ListPadding = 5f
			};
			scrollPanel.Append(modPacks);

			var uIScrollbar = new UIScrollbar {
				Height = { Percent = 1f },
				HAlign = 1f
			}.WithView(100f, 1000f);
			scrollPanel.Append(uIScrollbar);
			modPacks.SetScrollbar(uIScrollbar);

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

		private static readonly UIVirtualKeyboard VirtualKeyboard = new UIVirtualKeyboard(Language.GetTextValue("tModLoader.ModPacksEnterModPackName"), "", SaveModList, () => Main.menuMode = Interface.modPacksMenuID, 0);
		private static void SaveNewModList(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(11, -1, -1, 1);
			Main.MenuUI.SetState(VirtualKeyboard);
			Main.menuMode = 888;
		}

		public static void SaveModList(string filename) {
			// Sanitize input if not valid
			if (!IsValidModpackName(filename.Split(Path.DirectorySeparatorChar).Last())) {
				VirtualKeyboard.Text = SanitizeModpackName(filename);
				return;
			}
			// TODO
			//Main.menuMode = Interface.modsMenuID;

			//Main.PlaySound(10, -1, -1, 1);
			Directory.CreateDirectory(ModPacksDirectory);

			string path = Path.Combine(ModPacksDirectory, filename + ".json");
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

		internal const string MODPACK_REGEX = "[^a-zA-Z0-9_.-]+";
		internal static string SanitizeModpackName(string name) 
			=> Regex.Replace(name, MODPACK_REGEX, string.Empty, RegexOptions.Compiled);

		internal static bool IsValidModpackName(string name)
			=> !Regex.Match(name, MODPACK_REGEX, RegexOptions.Compiled).Success && name.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;

		public override void OnActivate() {
			scrollPanel.Append(uiLoader);
			modPacks.Clear();
			Task.Factory
				.StartNew(delegate {
					mods = ModOrganizer.FindMods().Select(m => m.Name).ToArray();

					Directory.CreateDirectory(ModPacksDirectory);
					return Directory.GetFiles(ModPacksDirectory, "*.json", SearchOption.TopDirectoryOnly);
				})
				.ContinueWith(task => {
					foreach (string modPackPath in task.Result) {
						try {
							if (modPackPath.EndsWith("/") || !IsValidModpackName(modPackPath.Split(Path.DirectorySeparatorChar).Last())) {
								throw new Exception();
							}
							string[] modPackMods = JsonConvert.DeserializeObject<string[]>(File.ReadAllText(modPackPath));
							modPacks.Add(new UIModPackItem(Path.GetFileNameWithoutExtension(modPackPath), modPackMods));
						}
						catch {
							var badModPackMessage = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.ModPackMalformed", Path.GetFileName(modPackPath))) {
								Width = { Percent = 1 },
								Height = { Pixels = 50, Percent = 0 }
							};
							modPacks.Add(badModPackMessage);
						}
					}
					scrollPanel.RemoveChild(uiLoader);
				}, TaskScheduler.FromCurrentSynchronizationContext());
		}
	}
}
