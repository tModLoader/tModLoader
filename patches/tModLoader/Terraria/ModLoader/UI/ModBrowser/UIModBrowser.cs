using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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

	// Debouncing to avoid sending unnecessary amount of start and abort queries
	// to Steam mainly when typing fast in the search bar
	public TimeSpan MinTimeBetweenUpdates = TimeSpan.FromMilliseconds(100);
	private Stopwatch DebounceTimer = null;
	internal bool UpdateNeeded;
	public UIState PreviousUIState { get; set; }

	/* Filters */
	private QueryParameters FilterParameters => new() {
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

		// MUST update all install states of installed mods so that
		// update will not leave some mods not updated @TODO: ???
		var imods = SocialBackend.GetInstalledModDownloadItems();
		foreach (var mod in imods) {
			mod.UpdateInstallState();
		}
		if (SpecialModPackFilter == null &&
			imods.Any(item => item.NeedUpdate)
		)
			_rootElement.Append(_updateAllButton);
	}

	private void UpdateAllMods(UIMouseEvent @event, UIElement element)
	{
		var relevantMods = SocialBackend.GetInstalledModDownloadItems()
			.Where(item => item.NeedUpdate)
			.Select(item => item.PublishId)
			.ToList();
		DownloadMods(relevantMods).ConfigureAwait(false).GetAwaiter().OnCompleted(
			CheckIfAnyModUpdateIsAvailable
		);
	}

	private void ClearTextFilters(UIMouseEvent @event, UIElement element)
	{
		PopulateModBrowser();
		SoundEngine.PlaySound(SoundID.MenuTick);
	}

	private void DownloadAllFilteredMods(UIMouseEvent @event, UIElement element)
	{
		// @TODO: Don't await, leave pending, could be problematic if exits UI?
		DownloadMods(SpecialModPackFilter).ConfigureAwait(false).GetAwaiter();
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
	private void ModListFinished(AsyncProviderState state, Exception e)
	{
		_browserStatus.SetCurrentState(state);
		_reloadButton.SetText(Language.GetText("tModLoader.MBReloadBrowser"));
	}

	public override void OnActivate()
	{
		base.OnActivate();
		Main.clrInput();
		if (_firstLoad) {
			SocialBackend.Initialize(); // Note this is currently synchronous
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
		// Updates the 'Update All' button using cached data from GetInstalledModDownloadedItems
		CheckIfAnyModUpdateIsAvailable();
	}

	public override void OnDeactivate()
	{
		DebounceTimer = null;
		base.OnDeactivate();
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		/* Old Code for Triggering an Update to tModLoader based on detecting a mod is for a newer version.
		 * Unfortunately, this is broken as of the revampt of ModBrowser under PR #3346, and not sure how to-readd relative to current environment
		if (_updateAvailable) {
			_updateAvailable = false;
			Interface.updateMessage.SetMessage(_updateText);
			Interface.updateMessage.SetGotoMenu(Interface.modBrowserID);
			Interface.updateMessage.SetURL(_updateUrl);
			Interface.updateMessage.SetAutoUpdateURL(_autoUpdateUrl);
			Main.menuMode = Interface.updateMessageID;
		}
		*/

		lock (modSlugsToUpdateInstallInfo) {
			foreach (var item in ModList.ReceivedItems.Where(
				d => modSlugsToUpdateInstallInfo.Contains(d.ModDownload.ModName)
			)) {
				item.ModDownload.UpdateInstallState();
				item.UpdateInstallDisplayState();
			}
			// @TODO: Shouldn't only delete processed slugs?
			modSlugsToUpdateInstallInfo.Clear();
		}

		if (
			(DebounceTimer is not null) &&
			(DebounceTimer.Elapsed >= MinTimeBetweenUpdates)
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
		ModList.SetEnumerable(SocialBackend.QueryBrowser(FilterParameters));
	}

	/// <summary>
	///     Enqueues a list of mods, if found on the browser (also used for ModPacks)
	/// </summary>
	internal async Task DownloadMods(List<ModPubId_t> modIds)
	{
		// @TODO: This too should become a Task since blocking
		var downloadsQueried = SocialBackend.DirectQueryItems(new QueryParameters() { searchModIds = modIds.ToArray() });

		for (int i = 0; i < modIds.Count(); i++) {
			if (downloadsQueried[i] == null)
				//TODO: Would be nice if this was name/slug, not ID
				_missingMods.Add(modIds[i].m_ModPubId);
		}

		var downloadShortList = ModDownloadItem.NeedsInstallOrUpdate(downloadsQueried);

		// If no download detected for some reason (e.g. empty modpack filter), prevent switching UI
		if (downloadShortList.Count() <= 0)
			return;

		await SocialBackend.SetupDownload(downloadShortList.ToList(), Interface.modBrowserID);

		// @TODO: Should not do UI stuff... :(
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