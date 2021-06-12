using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Social.Steam;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace Terraria.ModLoader.UI.ModBrowser
{
	internal partial class UIModBrowser : UIState, IHaveBackButtonCommand 
	{
		public static bool AvoidGithub;
		public static bool AvoidImgur;
		public static bool EarlyAutoUpdate;
		public static bool PlatformSupportsTls12 => true;

		internal static WorkshopHelper.QueryHelper SteamWorkshop { get; set; }

		public UIModDownloadItem SelectedItem;

		// TODO maybe we can refactor this as a "BrowserState" enum
		public bool Loading;
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

		internal readonly List<UIModDownloadItem> Items = new List<UIModDownloadItem>();
		

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

		private void UpdateAllMods(UIMouseEvent @event, UIElement element) {
			if (Loading) return;
			var relevantMods = Items.Where(x => x.HasUpdate && !x.UpdateIsDowngrade).Select(x => x.ModName).ToList();
			DownloadMods(relevantMods);
		}

		private void ClearFilters(UIMouseEvent @event, UIElement element) {
			SpecialModPackFilter = null;
			SpecialModPackFilterTitle = null;
			UpdateNeeded = true;
			SoundEngine.PlaySound(SoundID.MenuTick);
		}

		private void DownloadAllFilteredMods(UIMouseEvent @event, UIElement element) {
			DownloadMods(SpecialModPackFilter);
		}

		public override void Draw(SpriteBatch spriteBatch) {
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

			UILinkPointNavigator.Shortcuts.BackButtonCommand = 101;
		}

		public void BackClick(UIMouseEvent evt, UIElement listeningElement) {
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

		private void ReloadList(UIMouseEvent evt, UIElement listeningElement) {
			if (Loading) return;
			SoundEngine.PlaySound(SoundID.MenuOpen);
			PopulateModBrowser();
		}

		// TODO if we store a browser 'state' we can probably refactor this
		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
			if (!UpdateNeeded || Loading) return;
			UpdateNeeded = false;
			if (!Loading) _backgroundElement.RemoveChild(_loaderElement);
			ModList.Clear();
			ModList.AddRange(Items.Where(item => item.PassFilters()));
			bool hasNoModsFoundNotif = ModList.HasChild(NoModsFoundText);
			if (ModList.Count <= 0 && !hasNoModsFoundNotif)
				ModList.Add(NoModsFoundText);
			else if (hasNoModsFoundNotif)
				ModList.RemoveChild(NoModsFoundText);
			_rootElement.RemoveChild(_updateAllButton);
			if (SpecialModPackFilter == null && Items.Count(x => x.HasUpdate && !x.UpdateIsDowngrade) > 0) _rootElement.Append(_updateAllButton);
		}

		public override void OnActivate() {
			Main.clrInput();
			if (!Loading && Items.Count <= 0) {
				PopulateModBrowser();
			}
		}

		internal bool RemoveItem(UIModDownloadItem item) => Items.Remove(item);

		internal void ClearItems() => Items.Clear();

		internal void PopulateModBrowser() {
			// Initialize
			Loading = true;
			SpecialModPackFilter = null;
			SpecialModPackFilterTitle = null;
			_reloadButton.SetText(Language.GetTextValue("tModLoader.MBGettingData"));
			_backgroundElement.Append(_loaderElement);
			SetHeading(Language.GetTextValue("tModLoader.MenuModBrowser"));

			// Remove old resources
			if (SteamWorkshop != null) {
				SteamWorkshop.ReleaseWorkshopQuery();
				SteamWorkshop = null;
			}
			ModList.Clear();
			Items.Clear();
			ModList.Deactivate();

			// Populate
			SteamWorkshop = new WorkshopHelper.QueryHelper();
			Items.AddRange(SteamWorkshop.QueryWorkshop());
			UpdateNeeded = true;

			Loading = false;
			_reloadButton.SetText(Language.GetTextValue("tModLoader.MBReloadBrowser"));
		}

		/// <summary>
		///     Enqueues a list of mods, if found on the browser (also used for ModPacks)
		/// </summary>
		internal void DownloadMods(IEnumerable<string> modNames) {
			var downloads = new List<UIModDownloadItem>();

			foreach (string desiredMod in modNames) {
				var mod = Items.FirstOrDefault(x => x.ModName == desiredMod);
				if (mod == null) { // Not found on the browser
					_missingMods.Add(desiredMod);
				}
				else if (mod.Installed == null || mod.HasUpdate) { // Found, add to downloads
					downloads.Add(mod);
				}
			}

			// If no download detected for some reason (e.g. empty modpack filter), prevent switching UI
			if (downloads.Count <= 0) return;

			WorkshopHelper.ModManager.Download(downloads);

			if (_missingMods.Count > 0) {
				Interface.infoMessage.Show(Language.GetTextValue("tModLoader.MBModsNotFoundOnline", string.Join(",", _missingMods)), Interface.modBrowserID);
			}
			_missingMods.Clear();
		}

		internal UIModDownloadItem FindModDownloadItem(string publishId)
			=> Items.FirstOrDefault(x => x.PublishId.Equals(publishId));

		private void SetHeading(string heading) {
			HeaderTextPanel.SetText(heading, 0.8f, true);
			HeaderTextPanel.Recalculate();
		}

		internal static void LogModBrowserException(Exception e) {
			string errorMessage = $"{Language.GetTextValue("tModLoader.MBBrowserError")}\n\n{e.Message}\n{e.StackTrace}";
			Logging.tML.Error(errorMessage);
			Interface.errorMessage.Show(errorMessage, 0);
		}

		internal static void LogModPublishInfo(string message) {
			Logging.tML.Info(message);
			Interface.errorMessage.Show(Language.GetTextValue("tModLoader.MBServerResponse", message), Interface.modSourcesID);
		}
	}
}