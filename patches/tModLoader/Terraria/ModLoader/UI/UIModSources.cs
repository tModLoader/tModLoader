using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace Terraria.ModLoader.UI;

internal class UIModSources : UIState, IHaveBackButtonCommand
{
	public UIState PreviousUIState { get; set; }
	private readonly List<UIModSourceItem> _items = new List<UIModSourceItem>();
	private UIList _modList;
	private float modListViewPosition;
	private bool _updateNeeded;
	private UIElement _uIElement;
	private UIPanel _uIPanel;
	private UIInputTextField filterTextBox;
	private UILoaderAnimatedImage _uiLoader;
	private CancellationTokenSource _cts;
	private bool dotnetSDKFound;

	public override void OnInitialize()
	{
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
			BackgroundColor = UICommon.MainPanelBackground,
			PaddingTop = 0f
		};
		_uIElement.Append(_uIPanel);

		_uiLoader = new UILoaderAnimatedImage(0.5f, 0.5f, 1f);

		var upperMenuContainer = new UIElement {
			Width = { Percent = 1f },
			Height = { Pixels = 32 },
			Top = { Pixels = 10 }
		};
		var filterTextBoxBackground = new UIPanel {
			Top = { Percent = 0f },
			Left = { Pixels = -135, Percent = 1f },
			Width = { Pixels = 135 },
			Height = { Pixels = 40 }
		};
		filterTextBoxBackground.OnRightClick += (a, b) => filterTextBox.Text = "";
		upperMenuContainer.Append(filterTextBoxBackground);

		filterTextBox = new UIInputTextField(Language.GetTextValue("tModLoader.ModsTypeToSearch")) {
			Top = { Pixels = 5 },
			Left = { Pixels = -125, Percent = 1f },
			Width = { Pixels = 120 },
			Height = { Pixels = 20 }
		};
		filterTextBox.OnRightClick += (a, b) => filterTextBox.Text = "";
		filterTextBox.OnTextChange += (a, b) => _updateNeeded = true;
		upperMenuContainer.Append(filterTextBox);
		_uIPanel.Append(upperMenuContainer);

		_modList = new UIList {
			Width = { Pixels = -25, Percent = 1f },
			Height = { Pixels = -50, Percent = 1f },
			Top = { Pixels = 50 },
			ListPadding = 5f
		};
		_uIPanel.Append(_modList);

		var uIScrollbar = new UIScrollbar {
			Height = { Pixels = -50, Percent = 1f },
			Top = { Pixels = 50 },
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
		//_uIElement.Append(buttonBA);

		var buttonBRA = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MSBuildReloadAll"));
		buttonBRA.CopyStyle(buttonBA);
		buttonBRA.HAlign = 0.5f;
		buttonBRA.WithFadedMouseOver();
		buttonBRA.OnClick += BuildAndReload;
		//_uIElement.Append(buttonBRA);

		var buttonCreateMod = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MSCreateMod"));
		buttonCreateMod.CopyStyle(buttonBA);
		buttonCreateMod.HAlign = 1f;
		buttonCreateMod.Top.Pixels = -20;
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

		Append(_uIElement);
	}

	private void ButtonCreateMod_OnClick(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(11);
		Main.menuMode = Interface.createModID;
	}

	private void BackClick(UIMouseEvent evt, UIElement listeningElement)
	{
		(this as IHaveBackButtonCommand).HandleBackButtonUsage();
	}

	private void OpenSources(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(10, -1, -1, 1);
		try {
			Directory.CreateDirectory(ModCompile.ModSourcePath);
			Utils.OpenFolder(ModCompile.ModSourcePath);
		} catch(Exception e) {
			Logging.tML.Error(e);
		}
	}

	private void BuildMods(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(10, -1, -1, 1);
		if (_modList.Count > 0)
			Interface.buildMod.BuildAll(false);
	}

	private void BuildAndReload(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(10, -1, -1, 1);
		if (_modList.Count > 0)
			Interface.buildMod.BuildAll(true);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		UILinkPointNavigator.Shortcuts.BackButtonCommand = 7;
		base.Draw(spriteBatch);
		DrawMigrationGuideLink();
	}

	//TODO: simplify this method
	private void DrawMigrationGuideLink()
	{
		string versionUpgradeMessage = Language.GetTextValue("tModLoader.VersionUpgrade");
		float scale = 1f;

		var font = FontAssets.MouseText.Value;
		Vector2 sizes = font.MeasureString(versionUpgradeMessage);
		Vector2 origin = sizes;
		Color color = Color.IndianRed;
		if(sizes.X > 430) {
			scale = 430 / sizes.X;
			sizes.X *= scale;
		}

		int xLoc = (int)(Main.screenWidth / 2 + 134);
		int yLoc = (int)(sizes.Y + 244f);

		Main.spriteBatch.DrawString(font, versionUpgradeMessage, new Vector2(xLoc, yLoc), color, 0f, origin, new Vector2(scale, 1f), SpriteEffects.None, 0f);

		var rect = new Rectangle(xLoc - (int)sizes.X, yLoc - (int)sizes.Y, (int)sizes.X, (int)sizes.Y);
		if (!rect.Contains(new Point(Main.mouseX, Main.mouseY))) {
			return;
		}

		if (Main.mouseLeftRelease && Main.mouseLeft) {
			SoundEngine.PlaySound(SoundID.MenuOpen);
			Utils.OpenToURL("https://github.com/tModLoader/tModLoader/wiki/Update-Migration-Guide");
		}
	}

	public override void OnActivate()
	{
		_cts = new CancellationTokenSource();
		ModCompile.UpdateReferencesFolder();
		_uIPanel.Append(_uiLoader);
		_modList.Clear();
		_items.Clear();
		if (ShowInfoMessages())
			return;
		Populate();
	}

	public override void OnDeactivate()
	{
		_cts?.Cancel(false);
		_cts?.Dispose();
		_cts = null;
		modListViewPosition = _modList.ViewPosition;
	}

	private bool ShowInfoMessages()
	{
		if (!ModLoader.SeenFirstLaunchModderWelcomeMessage) {
			ShowWelcomeMessage("tModLoader.ViewOnGitHub", "https://github.com/tModLoader/tModLoader/wiki/Update-Migration-Guide");
			ModLoader.SeenFirstLaunchModderWelcomeMessage = true;
			Main.SaveSettings();
			return true;
		}

		if (!CheckDotnet()) {
			ShowWelcomeMessage("tModLoader.DownloadNetSDK", "https://github.com/tModLoader/tModLoader/wiki/tModLoader-guide-for-developers#developing-with-tmodloader", 888, PreviousUIState);
			return true;
		}

		return false;
	}

	private void ShowWelcomeMessage(string altButtonTextKey, string url, int gotoMenu = Interface.modSourcesID, UIState state = null)
	{
		Interface.infoMessage.Show(Language.GetTextValue("tModLoader.MSFirstLaunchModderWelcomeMessage"), gotoMenu, state, Language.GetTextValue(altButtonTextKey),
		() => {
			SoundEngine.PlaySound(SoundID.MenuOpen);
			Utils.OpenToURL(url);
		});
	}

	private bool CheckDotnet()
	{
		if (dotnetSDKFound)
			return true;

		try {
			string output = Process.Start(new ProcessStartInfo {
				FileName = "dotnet",
				Arguments = "--list-sdks",
				UseShellExecute = false,
				RedirectStandardOutput = true
			}).StandardOutput.ReadToEnd();
			Logging.tML.Info("\n" + output);

			foreach (var line in output.Split('\n')) {
				var dotnetVersion = new Version(new Regex("([0-9.]+).*").Match(line).Groups[1].Value);
				if (dotnetVersion >= new Version(6, 0)) {
					dotnetSDKFound = true;
					break;
				}
			}
		}
		catch (Exception e) {
			Logging.tML.Debug("'dotnet --list-sdks' check failed: ", e);
		}

		return dotnetSDKFound;
	}

	internal void Populate()
	{
		Task.Run(() => {
			var modSources = ModCompile.FindModSources();
			var modFiles = ModOrganizer.FindDevFolderMods();
			foreach (string sourcePath in modSources) {
				var builtMod = modFiles.SingleOrDefault(m => m.Name == Path.GetFileName(sourcePath));
				_items.Add(new UIModSourceItem(sourcePath, builtMod));
			}
			_updateNeeded = true;
		});
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		if (!_updateNeeded) return;
		_updateNeeded = false;
		_uIPanel.RemoveChild(_uiLoader);
		_modList.Clear();
		string filter = filterTextBox.Text;
		_modList.AddRange(_items.Where(item => filter.Length > 0 ? item.modName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) != -1 : true));
		Recalculate();
		_modList.ViewPosition = modListViewPosition;
	}
}
