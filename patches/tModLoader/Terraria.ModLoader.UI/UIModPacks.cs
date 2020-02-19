using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
		internal const string MODPACK_REGEX = "[^a-zA-Z0-9_.-]+";
		internal static string ModPacksDirectory = Path.Combine(ModLoader.ModPath, "ModPacks");
		internal static string[] Mods;

		private UIList _modPacks;
		private UILoaderAnimatedImage _uiLoader;
		private UIPanel _scrollPanel;
		private CancellationTokenSource _cts;

		public override void OnInitialize() {
			var uIElement = new UIElement {
				Width = { Percent = 0.8f },
				MaxWidth = UICommon.MaxPanelWidth,
				Top = { Pixels = 220 },
				Height = { Pixels = -220, Percent = 1f },
				HAlign = 0.5f
			};

			_uiLoader = new UILoaderAnimatedImage(0.5f, 0.5f);

			_scrollPanel = new UIPanel {
				Width = { Percent = 1f },
				Height = { Pixels = -65, Percent = 1f },
				BackgroundColor = UICommon.MainPanelBackground
			};
			uIElement.Append(_scrollPanel);

			_modPacks = new UIList {
				Width = { Pixels = -25, Percent = 1f },
				Height = { Percent = 1f },
				ListPadding = 5f
			};
			_scrollPanel.Append(_modPacks);

			var uIScrollbar = new UIScrollbar {
				Height = { Percent = 1f },
				HAlign = 1f
			}.WithView(100f, 1000f);
			_scrollPanel.Append(uIScrollbar);
			_modPacks.SetScrollbar(uIScrollbar);

			var titleTextPanel = new UITextPanel<string>(Language.GetTextValue("tModLoader.ModPacksHeader"), 0.8f, true) {
				HAlign = 0.5f,
				Top = { Pixels = -35 },
				BackgroundColor = UICommon.DefaultUIBlue
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

		private static UIVirtualKeyboard _virtualKeyboard;
		private static UIVirtualKeyboard VirtualKeyboard =>
			_virtualKeyboard ?? (_virtualKeyboard = new UIVirtualKeyboard(
				Language.GetTextValue("tModLoader.ModPacksEnterModPackName"), "", SaveModList, () => Main.menuMode = Interface.modPacksMenuID));

		private static void SaveNewModList(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(11);
			VirtualKeyboard.Text = "";
			Main.MenuUI.SetState(VirtualKeyboard);
			Main.menuMode = 888;
		}

		public static void SaveModList(string filename) {
			// Sanitize input if not valid
			if (!IsValidModpackName(filename)) {
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
			Main.PlaySound(11);
			Main.menuMode = Interface.modsMenuID;
		}

		public override void Draw(SpriteBatch spriteBatch) {
			base.Draw(spriteBatch);
			UILinkPointNavigator.Shortcuts.BackButtonCommand = 100;
			UILinkPointNavigator.Shortcuts.BackButtonGoto = Interface.modsMenuID;
		}

		internal static string SanitizeModpackName(string name)
			=> Regex.Replace(name, MODPACK_REGEX, string.Empty, RegexOptions.Compiled);

		internal static bool IsValidModpackName(string name)
			=> !Regex.Match(name, MODPACK_REGEX, RegexOptions.Compiled).Success && name.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;

		public override void OnDeactivate() {
			_cts?.Cancel(false);
			_cts?.Dispose();
			_cts = null;
		}

		public override void OnActivate() {
			_cts = new CancellationTokenSource();
			_scrollPanel.Append(_uiLoader);
			_modPacks.Clear();

			Task.Factory
				.StartNew(delegate {
					Mods = ModOrganizer.FindMods().Select(m => m.Name).ToArray();
					Directory.CreateDirectory(ModPacksDirectory);
					return Directory.GetFiles(ModPacksDirectory, "*.json", SearchOption.TopDirectoryOnly);
				}, _cts.Token)
				.ContinueWith(task => {
					foreach (string modPackPath in task.Result) {
						try {
							if (!IsValidModpackName(Path.GetFileNameWithoutExtension(modPackPath))) {
								throw new Exception();
							}
							string[] modPackMods = JsonConvert.DeserializeObject<string[]>(File.ReadAllText(modPackPath));
							_modPacks.Add(new UIModPackItem(Path.GetFileNameWithoutExtension(modPackPath), modPackMods));
						}
						catch {
							var badModPackMessage = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.ModPackMalformed", Path.GetFileName(modPackPath))) {
								Width = { Percent = 1 },
								Height = { Pixels = 50, Percent = 0 }
							};
							_modPacks.Add(badModPackMessage);
						}
					}
					_scrollPanel.RemoveChild(_uiLoader);
				}, _cts.Token, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
		}
	}
}
