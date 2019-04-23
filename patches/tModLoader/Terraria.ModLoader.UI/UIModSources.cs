using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
		private UIList modList;
		private readonly List<UIModSourceItem> items = new List<UIModSourceItem>();
		private bool updateNeeded;
		private UIElement uIElement;
		private UIPanel uIPanel;
		private UILoaderAnimatedImage uiLoader;

		public override void OnInitialize() {
			uIElement = new UIElement {
				Width = { Percent = 0.8f },
				MaxWidth = UICommon.MaxPanelWidth,
				Top = { Pixels = 220 },
				Height = { Pixels = -220, Percent = 1f },
				HAlign = 0.5f
			};

			uIPanel = new UIPanel {
				Width = { Percent = 1f },
				Height = { Pixels = -110, Percent = 1f },
				BackgroundColor = UICommon.mainPanelBackground
			};
			uIElement.Append(uIPanel);

			uiLoader = new UILoaderAnimatedImage(0.5f, 0.5f, 1f);

			modList = new UIList {
				Width = { Pixels = -25, Percent = 1f },
				Height = { Percent = 1f },
				ListPadding = 5f
			};
			uIPanel.Append(modList);

			var uIScrollbar = new UIScrollbar {
				Height = { Percent = 1f },
				HAlign = 1f
			}.WithView(100f, 1000f);
			uIPanel.Append(uIScrollbar);
			modList.SetScrollbar(uIScrollbar);

			var uIHeaderTextPanel = new UITextPanel<string>(Language.GetTextValue("tModLoader.MenuModSources"), 0.8f, true) {
				HAlign = 0.5f,
				Top = { Pixels = -35 },
				BackgroundColor = UICommon.defaultUIBlue
			}.WithPadding(15f);
			uIElement.Append(uIHeaderTextPanel);

			var buttonBA = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MSBuildAll")) {
				Width = { Pixels = -10, Percent = 1f / 3f },
				Height = { Pixels = 40 },
				VAlign = 1f,
				Top = { Pixels = -65 }
			};
			buttonBA.WithFadedMouseOver();
			buttonBA.OnClick += BuildMods;
			uIElement.Append(buttonBA);

			var buttonBRA = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MSBuildReloadAll"));
			buttonBRA.CopyStyle(buttonBA);
			buttonBRA.HAlign = 0.5f;
			buttonBRA.WithFadedMouseOver();
			buttonBRA.OnClick += BuildAndReload;
			uIElement.Append(buttonBRA);

			var buttonCreateMod = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MSCreateMod"));
			buttonCreateMod.CopyStyle(buttonBA);
			buttonCreateMod.HAlign = 1f;
			buttonCreateMod.WithFadedMouseOver();
			buttonCreateMod.OnClick += ButtonCreateMod_OnClick;
			uIElement.Append(buttonCreateMod);

			var buttonB = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("UI.Back"));
			buttonB.CopyStyle(buttonBA);
			//buttonB.Width.Set(-10f, 1f / 3f);
			buttonB.Top.Pixels = -20;
			buttonB.WithFadedMouseOver();
			buttonB.OnClick += BackClick;
			uIElement.Append(buttonB);

			var buttonOS = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MSOpenSources"));
			buttonOS.CopyStyle(buttonB);
			buttonOS.HAlign = .5f;
			buttonOS.WithFadedMouseOver();
			buttonOS.OnClick += OpenSources;
			uIElement.Append(buttonOS);

			var buttonMP = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MSManagePublished"));
			buttonMP.CopyStyle(buttonB);
			buttonMP.HAlign = 1f;
			buttonMP.WithFadedMouseOver();
			buttonMP.OnClick += ManagePublished;
			uIElement.Append(buttonMP);
			Append(uIElement);
		}

		private void ButtonCreateMod_OnClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(11);
			Main.menuMode = Interface.createModID;
		}

		private void ManagePublished(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(11, -1, -1, 1);
			Main.menuMode = Interface.managePublishedID;
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
			if (modList.Count > 0)
				Interface.buildMod.BuildAll(false);
		}

		private void BuildAndReload(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10, -1, -1, 1);
			if (modList.Count > 0)
				Interface.buildMod.BuildAll(true);
		}

		public override void Draw(SpriteBatch spriteBatch) {
			base.Draw(spriteBatch);
			UILinkPointNavigator.Shortcuts.BackButtonCommand = 1;
		}

		public override void OnActivate() {
			ModCompile.UpdateReferencesFolder();
			uIPanel.Append(uiLoader);
			modList.Clear();
			items.Clear();
			Populate();
		}

		internal void Populate() {
			Task.Factory.StartNew(
				delegate {
					var modSources = ModCompile.FindModSources();
					var modFiles = ModOrganizer.FindMods();
					return Tuple.Create(modSources, modFiles);
				})
				.ContinueWith(task => {
					var modSources = task.Result.Item1;
					var modFiles = task.Result.Item2;
					foreach (string sourcePath in modSources) {
						var builtMod = modFiles.SingleOrDefault(m => m.Name == Path.GetFileName(sourcePath));
						items.Add(new UIModSourceItem(sourcePath, builtMod));
					}
					updateNeeded = true;
				}, TaskScheduler.FromCurrentSynchronizationContext());
		}

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
			if (!updateNeeded) return;
			updateNeeded = false;
			uIPanel.RemoveChild(uiLoader);
			modList.Clear();
			modList.AddRange(items);
		}
	}
}
