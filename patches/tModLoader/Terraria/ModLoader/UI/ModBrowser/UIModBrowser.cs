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
	public bool Loading => _provider?.State == AsyncProvider.State.Loading;
	public bool anEnabledModUpdated;
	public bool aDisabledModUpdated;
	public bool aNewModDownloaded;

	private bool _updateAvailable;
	private string _updateText;
	private string _updateUrl;
	private string _autoUpdateUrl;
	private string _specialModPackFilterTitle;
	private List<string> _specialModPackFilter;
	private readonly List<string> _missingMods = new List<string>();

	AP_UIModDowloadItem _provider = null;

	// _items is only updated when everything is downloaded or aborted
	private readonly List<UIModDownloadItem> _items = new List<UIModDownloadItem>();

	internal bool UpdateNeeded;
	internal string Filter => FilterTextBox.Text;
	public UIState PreviousUIState { get; set; }

	/* Filters */
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

	public List<string> SpecialModPackFilter {
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
		if (Loading) return;
		var relevantMods = _items.Where(x => x.ModDownload.HasUpdate && !x.ModDownload.UpdateIsDowngrade).Select(x => x.ModDownload.ModName).ToList();
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
				return;
			}

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
		if ((newState == AsyncProvider.State.Aborted) || (newState == AsyncProvider.State.Completed)) {
			_reloadButton.SetText(Language.GetTextValue("tModLoader.MBReloadBrowser"));

			_items.Clear();
			_items.AddRange(_provider.GetData(false));
			if (SpecialModPackFilter == null && _items.Count(x => x.ModDownload.HasUpdate && !x.ModDownload.UpdateIsDowngrade) > 0)
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

		SetHeading(Language.GetTextValue("tModLoader.MenuModBrowser")); // @TODO: WHAT IS DOING THIS HERE???

		// Remove old data
		ModList.Clear();

		// Asynchronous load the Mod Browser
		if (uiOnly) {
			_provider?.RewrapUI();
			//ModList.ForceUpdateData(); // Not needed
		} else {
			_reloadButton.SetText(Language.GetTextValue("tModLoader.MBGettingData"));
			QueryParameters qparams = new();
			// @TODO: Populate qparams
			_provider = new AP_UIModDowloadItem(SocialBackend, qparams);
			ModList.SetProvider(_provider); // .Select(mdi => new UIModDownloadItem(mdi))
		}
	}

	/// <summary>
	///     Enqueues a list of mods, if found on the browser (also used for ModPacks)
	/// </summary>
	internal void DownloadMods(IEnumerable<string> modNames)
	{
		var downloads = new List<ModDownloadItem>();

		foreach (string desiredMod in modNames) {
			var mod = SocialBackend.Items.FirstOrDefault(x => x.ModName == desiredMod);

			if (mod == null) { // Not found on the browser
				_missingMods.Add(desiredMod);
			}
			else if (mod.Installed == null || mod.HasUpdate) { // Found, add to downloads
				downloads.Add(mod);
			}
		}

		// If no download detected for some reason (e.g. empty modpack filter), prevent switching UI
		if (downloads.Count <= 0)
			return;

		SocialBackend.SetupDownload(downloads, Interface.modBrowserID);

		if (_missingMods.Count > 0) {
			Interface.infoMessage.Show(Language.GetTextValue("tModLoader.MBModsNotFoundOnline", string.Join(",", _missingMods)), Interface.modBrowserID);
			_missingMods.Clear();
		}
	}

	private void SetHeading(string heading)
	{
		HeaderTextPanel.SetText(heading, 0.8f, true);
		HeaderTextPanel.Recalculate();
	}

	internal static void LogModBrowserException(Exception e)
	{
		Utils.ShowFancyErrorMessage($"{Language.GetTextValue("tModLoader.MBBrowserError")}\n\n{e.Message}\n{e.StackTrace}", 0);
	}

	internal void CleanupDeletedItem(string modName)
	{
		if (SocialBackend.Items.Count > 0) {
			SocialBackend.FindDownloadItem(modName).Installed = null;
			SocialBackend.FindDownloadItem(modName).NeedsGameRestart = true;
			PopulateModBrowser(uiOnly: true);
			UpdateNeeded = true;
		}
	}
}