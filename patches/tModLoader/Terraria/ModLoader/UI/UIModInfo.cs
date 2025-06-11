using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.Social.Base;
using Terraria.Social.Steam;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace Terraria.ModLoader.UI;

internal class UIModInfo : UIState
{
	private UIElement _uIElement;
	private UIMessageBox _modInfo;
	private UITextPanel<string> _uITextPanel;
	private UIAutoScaleTextTextPanel<string> _modHomepageButton;
	private UIAutoScaleTextTextPanel<string> _modSteamButton;
	private UIAutoScaleTextTextPanel<string> extractLocalizationButton;
	private UIAutoScaleTextTextPanel<string> fakeExtractLocalizationButton;
	private UIAutoScaleTextTextPanel<string> _extractButton;
	private UIAutoScaleTextTextPanel<string> _deleteButton;
	private UIAutoScaleTextTextPanel<string> _fakeDeleteButton; // easier than making new OnMouseOver code.
	private readonly UILoaderAnimatedImage _loaderElement = new UILoaderAnimatedImage(0.5f, 0.5f);

	private int _gotoMenu;
	private LocalMod _localMod;
	private string _url = string.Empty;
	private string _info = string.Empty;
	private string _modName = string.Empty;
	private string _modDisplayName = string.Empty;
	private ModPubId_t _publishedFileId;
	private bool _loading;
	private bool _ready;

	private CancellationTokenSource _cts;

	public override void OnInitialize()
	{
		_uIElement = new UIElement {
			Width = {Percent = 0.8f},
			MaxWidth = new StyleDimension(800f, 0f), //UICommon.MaxPanelWidth,
			Top = {Pixels = 220},
			Height = {Pixels = -220, Percent = 1f},
			HAlign = 0.5f
		};

		var uIPanel = new UIPanel {
			Width = {Percent = 1f},
			Height = {Pixels = -110, Percent = 1f},
			BackgroundColor = UICommon.MainPanelBackground
		};
		_uIElement.Append(uIPanel);

		_modInfo = new UIMessageBox(string.Empty) {
			Width = {Pixels = -25, Percent = 1f},
			Height = {Percent = 1f}
		};
		uIPanel.Append(_modInfo);

		var uIScrollbar = new UIScrollbar {
			Height = {Pixels = -12, Percent = 1f},
			VAlign = 0.5f,
			HAlign = 1f
		}.WithView(100f, 1000f);
		uIPanel.Append(uIScrollbar);

		_modInfo.SetScrollbar(uIScrollbar);
		_uITextPanel = new UITextPanel<string>(Language.GetTextValue("tModLoader.ModInfoHeader"), 0.8f, true) {
			HAlign = 0.5f,
			Top = {Pixels = -35},
			BackgroundColor = UICommon.DefaultUIBlue
		}.WithPadding(15f);
		_uIElement.Append(_uITextPanel);

		_modHomepageButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.ModInfoVisitHomepage")) {
			Width = { Pixels = -10, Percent = 0.333f },
			Height = {Pixels = 40},
			HAlign = 0.5f,
			VAlign = 1f,
			Top = {Pixels = -65}
		}.WithFadedMouseOver();
		_modHomepageButton.OnLeftClick += VisitModHomePage;

		_modSteamButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.ModInfoVisitSteampage")) {
			Width = { Pixels = -10, Percent = 0.333f },
			Height = { Pixels = 40 },
			HAlign = 0f,
			VAlign = 1f,
			Top = { Pixels = -65 }
		}.WithFadedMouseOver();
		_modSteamButton.OnLeftClick += VisitModHostPage;

		extractLocalizationButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.ModInfoExtractLocalization")) {
			Width = { Pixels = -10, Percent = 0.333f },
			Height = { Pixels = 40 },
			HAlign = 1f,
			VAlign = 1f,
			Top = { Pixels = -65 }
		}.WithFadedMouseOver();
		extractLocalizationButton.OnLeftClick += ExtractLocalization;

		fakeExtractLocalizationButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.ModInfoExtractLocalization")) {
			Width = { Pixels = -10, Percent = 0.333f },
			Height = { Pixels = 40 },
			HAlign = 1f,
			VAlign = 1f,
			Top = { Pixels = -65 }
		};
		fakeExtractLocalizationButton.BackgroundColor = Color.Gray;

		var backButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("UI.Back")) {
			Width = {Pixels = -10, Percent = 0.333f},
			Height = {Pixels = 40},
			VAlign = 1f,
			Top = {Pixels = -20}
		}.WithFadedMouseOver();
		backButton.OnLeftClick += BackClick;
		_uIElement.Append(backButton);

		_extractButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.ModInfoExtract")) {
			Width = {Pixels = -10, Percent = 0.333f},
			Height = {Pixels = 40},
			VAlign = 1f,
			HAlign = 0.5f,
			Top = {Pixels = -20}
		}.WithFadedMouseOver();
		_extractButton.OnLeftClick += ExtractMod;

		_deleteButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("UI.Delete")) {
			Width = {Pixels = -10, Percent = 0.333f},
			Height = {Pixels = 40},
			VAlign = 1f,
			HAlign = 1f,
			Top = {Pixels = -20}
		}.WithFadedMouseOver();
		_deleteButton.OnLeftClick += DeleteMod;

		_fakeDeleteButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("UI.Delete")) {
			Width = { Pixels = -10, Percent = 0.333f },
			Height = { Pixels = 40 },
			VAlign = 1f,
			HAlign = 1f,
			Top = { Pixels = -20 }
		};
		_fakeDeleteButton.BackgroundColor = Color.Gray;

		Append(_uIElement);
	}

	private void ExtractLocalization(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(SoundID.MenuOpen);
		// No need for a separate UIState, the process should be quick.
		bool success = LocalizationLoader.ExtractLocalizationFiles(_modName);
		if(success)
			extractLocalizationButton.SetText(Language.GetTextValue("tModLoader.ModInfoExtracted"));
	}

	internal void Show(string modName, string displayName, int gotoMenu, LocalMod localMod, string description = "", string url = "")
	{
		_modName = modName;
		_modDisplayName = displayName;
		_gotoMenu = gotoMenu;
		_localMod = localMod;
		_info = description;
		if (_info.Equals("")) {
			_info = Language.GetTextValue("tModLoader.ModInfoNoDescriptionAvailable");
		}
		_url = url;
		if (localMod != null && WorkshopHelper.GetPublishIdLocal(localMod.modFile, out ulong publishId))
			_publishedFileId = new ModPubId_t() { m_ModPubId = publishId.ToString() };
		else
			_publishedFileId = default;

		Main.gameMenu = true;
		Main.menuMode = Interface.modInfoID;
	}

	public override void OnDeactivate()
	{
		base.OnDeactivate();

		_info = string.Empty;
		_localMod = null;
		_gotoMenu = 0;
		_modName = string.Empty;
		_modDisplayName = string.Empty;
		_url = string.Empty;
		_modHomepageButton.Remove();
		_modSteamButton.Remove();
		extractLocalizationButton.Remove();
		fakeExtractLocalizationButton.Remove();
		_deleteButton.Remove();
		_fakeDeleteButton.Remove();
		_extractButton.Remove();
	}

	private void BackClick(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(SoundID.MenuClose);
		Main.menuMode = _gotoMenu;
	}

	private void ExtractMod(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(SoundID.MenuOpen);
		Interface.extractMod.Show(_localMod, _gotoMenu);
	}

	private void DeleteMod(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(SoundID.MenuClose);

		ModOrganizer.DeleteMod(_localMod);
		Main.menuMode = _gotoMenu;
	}

	private void VisitModHomePage(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(10);
		Utils.OpenToURL(_url);
	}

	private void VisitModHostPage(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(10);
		VisitModHostPageInner();
	}

	private void VisitModHostPageInner()
	{
		Utils.OpenToURL(Interface.modBrowser.SocialBackend.GetModWebPage(_publishedFileId));
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);

		UILinkPointNavigator.Shortcuts.BackButtonCommand = 100;
		UILinkPointNavigator.Shortcuts.BackButtonGoto = _gotoMenu;

		if (_modHomepageButton.IsMouseHovering) {
			UICommon.TooltipMouseText(_url);
		}
		if (_fakeDeleteButton.IsMouseHovering) {
			UICommon.TooltipMouseText(Language.GetTextValue("tModLoader.ModInfoDisableModToDelete"));
		}
		if (fakeExtractLocalizationButton.IsMouseHovering) {
			UICommon.TooltipMouseText(Language.GetTextValue("tModLoader.ModInfoEnableModToExtractLocalizationFiles"));
		}
	}

	public override void OnActivate()
	{
		_modInfo.SetText(_info);
		_uITextPanel.SetText(Language.GetTextValue("tModLoader.ModInfoHeader") + _modDisplayName, 0.8f, true);
		_loading = false;
		_ready = true;
	}

	public override void Update(GameTime gameTime)
	{
		if (!_loading && _ready) {
			_modInfo.SetText(_info);

			if (!string.IsNullOrEmpty(_url)){
				_uIElement.Append(_modHomepageButton);
			}

			if (!string.IsNullOrEmpty(_publishedFileId.m_ModPubId)) {
				_uIElement.Append(_modSteamButton);
			}

			if (_localMod != null) {
				bool realDeleteButton = ModLoader.Mods.All(x => x.Name != _localMod.Name);
				_uIElement.AddOrRemoveChild(_deleteButton, realDeleteButton);
				_uIElement.AddOrRemoveChild(_fakeDeleteButton, !realDeleteButton);
				_uIElement.AddOrRemoveChild(extractLocalizationButton, !realDeleteButton); // show real only if mod enabled
				_uIElement.AddOrRemoveChild(fakeExtractLocalizationButton, realDeleteButton);
				extractLocalizationButton.SetText(Language.GetTextValue("tModLoader.ModInfoExtractLocalization"));
				_uIElement.Append(_extractButton);
			}
			Recalculate();
			_modInfo.RemoveChild(_loaderElement);
			_ready = false;
		}

		base.Update(gameTime);
	}
}