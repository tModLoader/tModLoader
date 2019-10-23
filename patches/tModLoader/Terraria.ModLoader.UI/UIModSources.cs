using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace Terraria.ModLoader.UI
{
	internal class UIModSources : UIState
	{
		private readonly List<UIModSourceItem> _items = new List<UIModSourceItem>();
		private UIList _modList;
		private bool _updateNeeded;
		private UIElement _uIElement;
		private UIPanel _uIPanel;
		private UILoaderAnimatedImage _uiLoader;
		private CancellationTokenSource _cts;

		public override void OnInitialize() {
			_uIElement = new UIElement {
				Width = { Percent = 0.8f },
				MaxWidth = UICommon.MaxPanelWidth,
				Top = { Pixels = 220 },
				Height = { Pixels = -220, Percent = 1f },
				HAlign = 0.5f
			};

			_uIPanel = new UIPanel {
				Width = { Percent = 1f },
				Height = { Pixels = -110, Percent = 1f },
				BackgroundColor = UICommon.MainPanelBackground
			};
			_uIElement.Append(_uIPanel);

			_uiLoader = new UILoaderAnimatedImage(0.5f, 0.5f, 1f);

			_modList = new UIList {
				Width = { Pixels = -25, Percent = 1f },
				Height = { Percent = 1f },
				ListPadding = 5f
			};
			_uIPanel.Append(_modList);

			var uIScrollbar = new UIScrollbar {
				Height = { Percent = 1f },
				HAlign = 1f
			}.WithView(100f, 1000f);
			_uIPanel.Append(uIScrollbar);
			_modList.SetScrollbar(uIScrollbar);

			var uIHeaderTextPanel = new UITextPanel<string>(Language.GetTextValue("tModLoader.MenuModSources"), 0.8f, true) {
				HAlign = 0.5f,
				Top = { Pixels = -35 },
				BackgroundColor = UICommon.DefaultUIBlue
			}.WithPadding(15f);
			_uIElement.Append(uIHeaderTextPanel);

			var buttonBA = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MSBuildAll")) {
				Width = { Pixels = -10, Percent = 1f / 3f },
				Height = { Pixels = 40 },
				VAlign = 1f,
				Top = { Pixels = -65 }
			};
			buttonBA.WithFadedMouseOver();
			buttonBA.OnClick += BuildMods;
			_uIElement.Append(buttonBA);

			var buttonBRA = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MSBuildReloadAll"));
			buttonBRA.CopyStyle(buttonBA);
			buttonBRA.HAlign = 0.5f;
			buttonBRA.WithFadedMouseOver();
			buttonBRA.OnClick += BuildAndReload;
			_uIElement.Append(buttonBRA);

			var buttonCreateMod = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MSCreateMod"));
			buttonCreateMod.CopyStyle(buttonBA);
			buttonCreateMod.HAlign = 1f;
			buttonCreateMod.WithFadedMouseOver();
			buttonCreateMod.OnClick += ButtonCreateMod_OnClick;
			_uIElement.Append(buttonCreateMod);

			var buttonB = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("UI.Back"));
			buttonB.CopyStyle(buttonBA);
			//buttonB.Width.Set(-10f, 1f / 3f);
			buttonB.Top.Pixels = -20;
			buttonB.WithFadedMouseOver();
			buttonB.OnClick += BackClick;
			_uIElement.Append(buttonB);

			var buttonOS = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MSOpenSources"));
			buttonOS.CopyStyle(buttonB);
			buttonOS.HAlign = .5f;
			buttonOS.WithFadedMouseOver();
			buttonOS.OnClick += OpenSources;
			_uIElement.Append(buttonOS);

			var buttonMP = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MSManagePublished"));
			buttonMP.CopyStyle(buttonB);
			buttonMP.HAlign = 1f;
			buttonMP.WithFadedMouseOver();
			buttonMP.OnClick += ManagePublished;
			_uIElement.Append(buttonMP);
			Append(_uIElement);
		}

		private void ButtonCreateMod_OnClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(11);
			Main.menuMode = Interface.createModID;
		}

		private void ManagePublished(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(11, -1, -1, 1);
			Main.menuMode = Interface.managePublishedID;
			if (ModLoader.modBrowserPassphrase == string.Empty) {
				Main.menuMode = Interface.enterPassphraseMenuID;
				Interface.enterPassphraseMenu.SetGotoMenu(Interface.managePublishedID);
			}
		}

		private void BackClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(11, -1, -1, 1);
			Main.menuMode = 0;
		}

		private void OpenSources(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10, -1, -1, 1);
			Directory.CreateDirectory(ModCompile.ModSourcePath);
			Process.Start(ModCompile.ModSourcePath);
		}

		private void BuildMods(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10, -1, -1, 1);
			if (_modList.Count > 0)
				Interface.buildMod.BuildAll(false);
		}

		private void BuildAndReload(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10, -1, -1, 1);
			if (_modList.Count > 0)
				Interface.buildMod.BuildAll(true);
		}

		public override void Draw(SpriteBatch spriteBatch) {
			base.Draw(spriteBatch);
			UILinkPointNavigator.Shortcuts.BackButtonCommand = 1;
		}

		public override void OnActivate() {
			_cts = new CancellationTokenSource();
			ModCompile.UpdateReferencesFolder();
			_uIPanel.Append(_uiLoader);
			_modList.Clear();
			_items.Clear();
			Populate();
		}

		public override void OnDeactivate() {
			_cts?.Cancel(false);
			_cts?.Dispose();
			_cts = null;
		}

		internal void Populate() {
			Task.Factory.StartNew(
				delegate {
					var modSources = ModCompile.FindModSources();
					var modFiles = ModOrganizer.FindMods();
					return Tuple.Create(modSources, modFiles);
				}, _cts.Token)
				.ContinueWith(task => {
					var modSources = task.Result.Item1;
					var modFiles = task.Result.Item2;
					foreach (string sourcePath in modSources) {
						var builtMod = modFiles.SingleOrDefault(m => m.Name == Path.GetFileName(sourcePath));
						_items.Add(new UIModSourceItem(sourcePath, builtMod));
					}
					_updateNeeded = true;
				}, _cts.Token, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
		}

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
			if (!_updateNeeded) return;
			_updateNeeded = false;
			_uIPanel.RemoveChild(_uiLoader);
			_modList.Clear();
			_modList.AddRange(_items);
		}
	}
}
