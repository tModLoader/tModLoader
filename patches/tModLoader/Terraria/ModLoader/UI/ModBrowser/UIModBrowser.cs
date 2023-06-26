using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI.Elements;
using Terraria.Social.Base;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace Terraria.ModLoader.UI.ModBrowser;

internal partial class UIModBrowser : UIState, IHaveBackButtonCommand
{
	// Used for swapping backend hosting
	public SocialBrowserModule SocialBackend;

	public UIModBrowser(SocialBrowserModule socialBackend)
	{
		ModOrganizer.PostLocalModsChanged += CbLocalModsChanged;
		SocialBackend = socialBackend;
	}

	public class UIAsyncList_ModDownloadItem : UIAsyncList<ModDownloadItem, UIModDownloadItem>
	{
		protected override UIModDownloadItem GenElement(ModDownloadItem resource)
		{
			return new UIModDownloadItem(resource);
		}
		protected override void UpdateElement(UIModDownloadItem element)
		{
			element.UpdateInstallInfo();
		}
	}

	public static bool AvoidGithub;
	public static bool AvoidImgur;
	public static bool EarlyAutoUpdate;
	public static bool PlatformSupportsTls12 => true;

	public UIModDownloadItem SelectedItem;

	// TODO maybe we can refactor this as a "BrowserState" enum
	public bool Loading => !ModList.State.IsFinished();
	public bool anEnabledModUpdated;
	public bool aDisabledModUpdated;
	public bool aNewModDownloaded;

	private bool _firstLoad = true;

	private bool _updateAvailable;
	private string _updateText;
	private string _updateUrl;
	private string _autoUpdateUrl;
	private string _specialModPackFilterTitle;
	private List<ModPubId_t> _specialModPackFilter;
	private readonly List<string> _missingMods = new List<string>();

	private HashSet<string> modSlugsToUpdateInstallInfo = new();

	public const int DEBOUNCE_MS = 100;
	private Stopwatch DebounceTimer = null;
	internal bool UpdateNeeded;
	public UIState PreviousUIState { get; set; }

	/* Filters */
	public QueryParameters FilterParameters => new() {
		searchTags = new string[] { SocialBrowserModule.GetBrowserVersionNumber(BuildInfo.tMLVersion) },
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

	private void CheckIfAnyModUpdateIsAvailable()
	{
		_rootElement.RemoveChild(_updateAllButton);
		if (
			SpecialModPackFilter == null &&
			SocialBackend.GetInstalledModDownloadItems().Where(item => { item.UpdateInstallState(); return item.NeedUpdate; }).Count() > 0
		)
			_rootElement.Append(_updateAllButton);
	}

	private void UpdateAllMods(UIMouseEvent @event, UIElement element)
	{
		var relevantMods = SocialBackend.GetInstalledModDownloadItems()
			.Where(item => item.NeedUpdate)
			.Select(item => item.PublishId)
			.ToList();
		DownloadMods(relevantMods);

		CheckIfAnyModUpdateIsAvailable();
	}

	private void ClearTextFilters(UIMouseEvent @event, UIElement element)
	{
		// These and already done in PopulateModBrowser
		//SpecialModPackFilter = null;
		//SpecialModPackFilterTitle = null;

		PopulateModBrowser();
		SoundEngine.PlaySound(SoundID.MenuTick);
	}

	private void DownloadAllFilteredMods(UIMouseEvent @event, UIElement element)
	{
		DownloadMods(SpecialModPackFilter);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		// @TODO: Why this is done on Draw? (plus hard coded 101 :|)
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
			ModList.Cancel();
		} else {
			SoundEngine.PlaySound(SoundID.MenuOpen);
			PopulateModBrowser();
		}
	}

	private void ModListStartLoading(AsyncProviderState state)
	{
		_browserStatus.SetCurrentState(state);
		_reloadButton.SetText(Language.GetText("tModLoader.MBGettingData"));
	}
	private void ModListFinished(AsyncProviderState state)
	{
		_browserStatus.SetCurrentState(state);
		_reloadButton.SetText(Language.GetText("tModLoader.MBReloadBrowser"));
	}

	public override void OnActivate()
	{
		base.OnActivate();
		Main.clrInput();
		if (_firstLoad) {
			PopulateModBrowser();
		}

		// Check for mods to update
		// @NOTE: Now it's done only once on load
		CheckIfAnyModUpdateIsAvailable();

		DebounceTimer = null;
	}

	private void CbLocalModsChanged(HashSet<string> modSlugs)
	{
		// Can be called outside main thread
		lock (modSlugsToUpdateInstallInfo) {
			modSlugsToUpdateInstallInfo.UnionWith(modSlugs);
		}
	}

	public override void OnDeactivate()
	{
		DebounceTimer = null;
		base.OnDeactivate();
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (_updateAvailable) {
			_updateAvailable = false;
			Interface.updateMessage.SetMessage(_updateText);
			Interface.updateMessage.SetGotoMenu(Interface.modBrowserID);
			Interface.updateMessage.SetURL(_updateUrl);
			Interface.updateMessage.SetAutoUpdateURL(_autoUpdateUrl);
			Main.menuMode = Interface.updateMessageID;
		}

		// @TODO: UpdateInstallInfo is "blocking" even tho it's not a Task
		// Make sure this doesn't hang the process!!!
		lock (modSlugsToUpdateInstallInfo) {
			foreach (var item in ModList.ReceivedItems.Where(
				d => modSlugsToUpdateInstallInfo.Contains(d.ModDownload.ModName)
			)) {
				item.UpdateInstallInfo();
			}
			// @TODO: Shouldn't only delete processed slugs?
			modSlugsToUpdateInstallInfo.Clear();
		}

		if (
			(DebounceTimer is not null) &&
			(DebounceTimer.ElapsedMilliseconds >= DEBOUNCE_MS)
		) {
			// No need to count more
			DebounceTimer.Stop();
			DebounceTimer = null;
		}

		if (UpdateNeeded) {
			// Debounce logic
			if (DebounceTimer is null) {
				UpdateNeeded = false;
				PopulateModBrowser();
				DebounceTimer = new();
				DebounceTimer.Start();
			}
		}
	}

	internal void PopulateModBrowser()
	{
		_firstLoad = false;

		// Only called if using mod browser and not for modpacks
		SpecialModPackFilter = null;
		SpecialModPackFilterTitle = null;

		// Since we could have used modpacks before fix the title if wrong
		SetHeading(Language.GetText("tModLoader.MenuModBrowser"));

		// Old data will be removed and old provider aborted when setting the new provider `ModList.SetProvider`

		// Asynchronous load the Mod Browser
		// @TODO: "Chicken Bones" here the line below blocks
		// the final version imo should have THIS line not the
		// one uncommented below
		//ModList.SetEnumerable(SocialBackend.QueryBrowser(FilterParameters));
		ModList.SetEnumerable(SocialBackend.QueryBrowser(FilterParameters), true);
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