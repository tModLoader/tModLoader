using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Social.Steam;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace Terraria.ModLoader.UI.ModBrowser;

internal partial class UIModBrowser : UIState, IHaveBackButtonCommand
{
	// Used for swapping backend hosting
	public SocialBrowserModule SocialBackend => WorkshopBrowserModule.Instance;

	private class AP_UIModDowloadItem : AsyncProvider<UIModDownloadItem>
	{
		private SocialBrowserModule SocialBackend;
		private QueryParameters QParams;
		public AP_UIModDowloadItem(SocialBrowserModule socialBackend, QueryParameters qparams) : base()
		{
			SocialBackend = socialBackend;
			QParams = qparams;
		}

		public void RewrapUI()
		{
			lock (_Data) {
				_Data = _Data.Select(uimdi => new UIModDownloadItem(uimdi.ModDownload)).ToList();
				HasNewData = true;
			}
		}

		protected override async Task<bool> Run(CancellationToken token)
		{
			bool error = false;
			await foreach (var item in SocialBackend.QueryBrowser(QParams).WithCancellation(token)) {
				if (item is null) {
					error = true; // Save the error, but let the enumerator finish
				} else {
					lock (_Data) { // @TODO: lock in batches?
						_Data.Add(new UIModDownloadItem(item));
						HasNewData = true;
					}
				}
			}
			return !error;
		}
	}

	public static bool AvoidGithub;
	public static bool AvoidImgur;
	public static bool EarlyAutoUpdate;
	public static bool PlatformSupportsTls12 => true;

	public UIModDownloadItem SelectedItem;

	// TODO maybe we can refactor this as a "BrowserState" enum
	public bool Loading => _provider?.State.IsFinished() != true;
	public bool anEnabledModUpdated;
	public bool aDisabledModUpdated;
	public bool aNewModDownloaded;

	private bool _updateAvailable;
	private string _updateText;
	private string _updateUrl;
	private string _autoUpdateUrl;
	private string _specialModPackFilterTitle;
	private List<ModPubId_t> _specialModPackFilter;
	private readonly List<string> _missingMods = new List<string>();

	AP_UIModDowloadItem _provider = null;

	// _items is only updated when everything is downloaded or aborted
	private readonly List<UIModDownloadItem> _items = new List<UIModDownloadItem>();

	internal bool UpdateNeeded;
	public UIState PreviousUIState { get; set; }

	/* Filters */
	public QueryParameters FilterParameters => new() {
		searchTags = null, //new string[] { SocialBrowserModule.GetBrowserVersionNumber(BuildInfo.tMLVersion) },
		searchModIds = SpecialModPackFilter?.ToArray(),
		searchModSlugs = null,
		searchGeneric = SearchFilterMode == SearchFilter.Name ? Filter : null,
		searchAuthor = SearchFilterMode == SearchFilter.Author ? Filter : null,
		sortingParamater = SortMode,
		updateStatusFilter = UpdateFilterMode,
		modSideFilter = ModSideFilterMode,

		queryType = QueryType.SearchAll
	};

	internal string Filter => FilterTextBox.Text;

	public ModBrowserSortMode SortMode {
		get => SortModeFilterToggle.State;
		set => SortModeFilterToggle.SetCurrentState(value);
	}

	public UpdateFilter UpdateFilterMode {
		get => UpdateFilterToggle.State;
		set => UpdateFilterToggle.SetCurrentState(value);
	}

	public SearchFilter SearchFilterMode {
		get => SearchFilterToggle.State;
		set => SearchFilterToggle.SetCurrentState(value);
	}

	public ModSideFilter ModSideFilterMode {
		get => ModSideFilterToggle.State;
		set => ModSideFilterToggle.SetCurrentState(value);
	}

	internal string SpecialModPackFilterTitle {
		get => _specialModPackFilterTitle;
		set {
			_clearButton.SetText(Language.GetTextValue("tModLoader.MBClearSpecialFilter", value));
			_specialModPackFilterTitle = value;
		}
	}

	public List<ModPubId_t> SpecialModPackFilter {
		get => _specialModPackFilter;
		set {
			if (_specialModPackFilter != null && value == null) {
				_backgroundElement.BackgroundColor = UICommon.MainPanelBackground;
				_rootElement.RemoveChild(_clearButton);
				_rootElement.RemoveChild(_downloadAllButton);
			}
			else if (_specialModPackFilter == null && value != null) {
				_backgroundElement.BackgroundColor = Color.Purple * 0.7f;
				_rootElement.Append(_clearButton);
				_rootElement.Append(_downloadAllButton);
			}

			_specialModPackFilter = value;
		}
	}

	private void UpdateAllMods(UIMouseEvent @event, UIElement element)
	{
		if (Loading)
			return;

		var relevantMods = _items.Where(x => new ModDownloadItemInstallInfo(x.ModDownload).NeedUpdate).Select(x => x.ModDownload.PublishId).ToList();
		DownloadMods(relevantMods);
	}

	private void ClearFilters(UIMouseEvent @event, UIElement element)
	{
		SpecialModPackFilter = null;
		SpecialModPackFilterTitle = null;
		UpdateNeeded = true;
		SoundEngine.PlaySound(SoundID.MenuTick);
	}

	private void DownloadAllFilteredMods(UIMouseEvent @event, UIElement element)
	{
		DownloadMods(SpecialModPackFilter);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		// @TODO: Why this is on Draw??? (plus hard coded 101 :|)
		UILinkPointNavigator.Shortcuts.BackButtonCommand = 101;

		base.Draw(spriteBatch);
		for (int i = 0; i < CategoryButtons.Count; i++)
			if (CategoryButtons[i].IsMouseHovering) {
				string text;
				switch (i) {
					case 0:
						text = SortMode.ToFriendlyString();
						break;
					case 1:
						text = UpdateFilterMode.ToFriendlyString();
						break;
					case 2:
						text = ModSideFilterMode.ToFriendlyString();
						break;
					case 3:
						text = SearchFilterMode.ToFriendlyString();
						break;
					default:
						text = "None";
						break;
				}

				UICommon.DrawHoverStringInBounds(spriteBatch, text);
				// @TEMP: Was return here, it did "block" _updateAvailable processing
				break;
			}

		// @TODO: This feels a lot like an Update method...
		if (_updateAvailable) {
			_updateAvailable = false;
			Interface.updateMessage.SetMessage(_updateText);
			Interface.updateMessage.SetGotoMenu(Interface.modBrowserID);
			Interface.updateMessage.SetURL(_updateUrl);
			Interface.updateMessage.SetAutoUpdateURL(_autoUpdateUrl);
			Main.menuMode = Interface.updateMessageID;
		}

	}

	public void BackClick(UIMouseEvent evt, UIElement listeningElement)
	{
		bool reloadModsNeeded = aNewModDownloaded && ModLoader.autoReloadAndEnableModsLeavingModBrowser || anEnabledModUpdated;
		bool enableModsReminder = aNewModDownloaded && !ModLoader.dontRemindModBrowserDownloadEnable;
		bool reloadModsReminder = aDisabledModUpdated && !ModLoader.dontRemindModBrowserUpdateReload;

		if (reloadModsNeeded) {
			Main.menuMode = Interface.reloadModsID;
		}
		else if (enableModsReminder || reloadModsReminder) {
			string text = "";
			if(enableModsReminder)
				text += Language.GetTextValue("tModLoader.EnableModsReminder") + "\n\n";
			if (reloadModsReminder)
				text += Language.GetTextValue("tModLoader.ReloadModsReminder");
			Interface.infoMessage.Show(text,
				0, null, Language.GetTextValue("tModLoader.DontShowAgain"),
				() => {
					if(enableModsReminder)
						ModLoader.dontRemindModBrowserDownloadEnable = true;
					if (reloadModsReminder)
						ModLoader.dontRemindModBrowserUpdateReload = true;
					Main.SaveSettings();
				});
		}

		anEnabledModUpdated = false;
		aNewModDownloaded = false;
		aDisabledModUpdated = false;

		(this as IHaveBackButtonCommand).HandleBackButtonUsage();
	}

	private void ReloadList(UIMouseEvent evt, UIElement listeningElement)
	{
		if (Loading) {
			SoundEngine.PlaySound(SoundID.MenuOpen);
			ModList.AbortLoading();
		} else {
			SoundEngine.PlaySound(SoundID.MenuOpen);
			PopulateModBrowser(uiOnly: false);
		}
	}

	private void ModListStateChanged(AsyncProvider.State newState, AsyncProvider.State oldState)
	{
		_browserStatus.SetCurrentState(newState);
		_rootElement.RemoveChild(_updateAllButton);
		if (newState.IsFinished()) {
			_reloadButton.SetText(Language.GetText("tModLoader.MBReloadBrowser"));

			_items.Clear();
			_items.AddRange(_provider.GetData(false));
			if (SpecialModPackFilter == null && _items.Count(x => new ModDownloadItemInstallInfo(x.ModDownload).NeedUpdate) > 0)
				_rootElement.Append(_updateAllButton);
		}
	}

	public override void OnActivate()
	{
		Main.clrInput();
		if (_provider is null) { // @TODO: Will search and stuff remain in the state???
			PopulateModBrowser(uiOnly: false);
		}
	}

	internal void PopulateModBrowser(bool uiOnly)
	{
		// Initialize
		SpecialModPackFilter = null;
		SpecialModPackFilterTitle = null;

		SetHeading(Language.GetText("tModLoader.MenuModBrowser")); // @TODO: WHAT IS DOING THIS HERE???

		// Remove old data
		ModList.Clear();

		// Asynchronous load the Mod Browser
		if (uiOnly) {
			_provider?.RewrapUI();
			//ModList.ForceUpdateData(); // Not needed
		} else {
			_reloadButton.SetText(Language.GetText("tModLoader.MBGettingData"));
			QueryParameters qparams = FilterParameters;
			// @TODO: Populate qparams
			_provider = new AP_UIModDowloadItem(SocialBackend, qparams);
			ModList.SetProvider(_provider); // .Select(mdi => new UIModDownloadItem(mdi))
		}
	}

	/// <summary>
	///     Enqueues a list of mods, if found on the browser (also used for ModPacks)
	/// </summary>
	internal void DownloadMods(List<ModPubId_t> modIds)
	{
		var downloadsQueried = SocialBackend.DirectQueryItems(new QueryParameters() { searchModIds = modIds.ToArray() });

		for (int i = 0; i < modIds.Count(); i++) {
			if (downloadsQueried[i] == null)
				//TODO: Would be nice if this was name/slug, not ID
				_missingMods.Add(modIds[i].m_ModPubId);
		}

		var downloadShortList = ModDownloadItem.FilterOutInstalled(downloadsQueried);

		// If no download detected for some reason (e.g. empty modpack filter), prevent switching UI
		if (downloadShortList.Count() <= 0)
			return;

		SocialBackend.SetupDownload(downloadShortList.ToList(), Interface.modBrowserID);

		if (_missingMods.Count > 0) {
			Interface.infoMessage.Show(Language.GetTextValue("tModLoader.MBModsNotFoundOnline", string.Join(",", _missingMods)), Interface.modBrowserID);
			_missingMods.Clear();
		}
	}

	private void SetHeading(LocalizedText heading)
	{
		HeaderTextPanel.SetText(heading, 0.8f, true);
		HeaderTextPanel.Recalculate();
	}

	internal static void LogModBrowserException(Exception e)
	{
		Utils.ShowFancyErrorMessage($"{Language.GetTextValue("tModLoader.MBBrowserError")}\n\n{e.Message}\n{e.StackTrace}", 0);
	}
}