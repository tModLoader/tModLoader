using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI.DownloadManager;
using Terraria.ModLoader.UI.ModBrowser.Elements;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace Terraria.ModLoader.UI.ModBrowser
{
	// TODO could use a refactor
	internal class UIModBrowser : UIState
	{
		internal readonly List<UICycleImage> _categoryButtons = new List<UICycleImage>();

		private readonly List<string> _missingMods = new List<string>();
		private readonly List<UIModDownloadItem> items = new List<UIModDownloadItem>();
		private List<string> _specialModPackFilter;
		private string _specialModPackFilterTitle;
		public bool aModUpdated;
		public bool anEnabledModDownloaded; // disregard user preference for mods that are enabled
		public bool aNewModDownloaded;
		private string autoUpdateURL;
		private UITextPanel<string> clearButton;
		private UITextPanel<string> downloadAllButton;
		internal string filter;
		internal UIInputTextField filterTextBox;
		public bool loading;
		public UIList modList;
		private UITextPanel<string> reloadButton;
		public UIModDownloadItem selectedItem;
		private UIElement uIElement;
		public UITextPanel<string> uIHeaderTextPanel;
		private UILoaderAnimatedImage uILoader;
		public UIText uINoModsFoundText;
		private UIPanel uIPanel;
		private UITextPanel<string> updateAllButton;
		private bool updateAvailable;
		internal bool updateNeeded;
		private string updateText;
		private string updateURL;
		
		/* Filters */
		public ModBrowserSortMode SortMode {
			get => SortModeFilterToggle.State;
			set => SortModeFilterToggle.SetCurrentState(value);
		}
		public readonly UIBrowserFilterToggle<ModBrowserSortMode> SortModeFilterToggle = new UIBrowserFilterToggle<ModBrowserSortMode>(0, 0) {
			Left = new StyleDimension {Pixels = 0 * 36 + 8}
		};

		public UpdateFilter UpdateFilterMode {
			get => UpdateFilterToggle.State;
			set => UpdateFilterToggle.SetCurrentState(value);
		}
		public readonly UIBrowserFilterToggle<UpdateFilter> UpdateFilterToggle = new UIBrowserFilterToggle<UpdateFilter>(34, 0) {
			Left = new StyleDimension {Pixels = 1 * 36 + 8}
		};

		public SearchFilter SearchFilterMode {
			get => SearchFilterToggle.State;
			set => SearchFilterToggle.SetCurrentState(value);
		}
		public readonly UIBrowserFilterToggle<SearchFilter> SearchFilterToggle = new UIBrowserFilterToggle<SearchFilter>(34 * 2, 0) {
			Left = new StyleDimension {Pixels = 545f}
		};
		
		public ModSideFilter ModSideFilterMode {
			get => ModSideFilterToggle.State;
			set => ModSideFilterToggle.SetCurrentState(value);
		}
		public readonly UIBrowserFilterToggle<ModSideFilter> ModSideFilterToggle = new UIBrowserFilterToggle<ModSideFilter>(34 * 5, 0) {
			Left = new StyleDimension {Pixels = 2 * 36 + 8}
		};
		
		internal string SpecialModPackFilterTitle {
			get => _specialModPackFilterTitle;
			set {
				clearButton.SetText(Language.GetTextValue("tModLoader.MBClearSpecialFilter", value));
				_specialModPackFilterTitle = value;
			}
		}

		public List<string> SpecialModPackFilter {
			get => _specialModPackFilter;
			set {
				if (_specialModPackFilter != null && value == null) {
					uIPanel.BackgroundColor = UICommon.mainPanelBackground;
					uIElement.RemoveChild(clearButton);
					uIElement.RemoveChild(downloadAllButton);
				}
				else if (_specialModPackFilter == null && value != null) {
					uIPanel.BackgroundColor = Color.Purple * 0.7f;
					uIElement.Append(clearButton);
					uIElement.Append(downloadAllButton);
				}

				_specialModPackFilter = value;
			}
		}

		internal static bool PlatformSupportsTls12 {
			get {
				foreach (SecurityProtocolType protocol in Enum.GetValues(typeof(SecurityProtocolType)))
					if (protocol.GetHashCode() == 3072)
						return true;
				return false;
			}
		}

		public override void OnInitialize() {
			uIElement = new UIElement {
				Width = {Percent = 0.8f},
				MaxWidth = UICommon.MaxPanelWidth,
				Top = {Pixels = 220},
				Height = {Pixels = -220, Percent = 1f},
				HAlign = 0.5f
			};

			uIPanel = new UIPanel {
				Width = {Percent = 1f},
				Height = {Pixels = -110, Percent = 1f},
				BackgroundColor = UICommon.mainPanelBackground,
				PaddingTop = 0f
			};
			uIElement.Append(uIPanel);

			uILoader = new UILoaderAnimatedImage(0.5f, 0.5f);

			modList = new UIList {
				Width = {Pixels = -25, Percent = 1f},
				Height = {Pixels = -50, Percent = 1f},
				Top = {Pixels = 50},
				ListPadding = 5f
			};
			uIPanel.Append(modList);

			var uIScrollbar = new UIScrollbar {
				Height = {Pixels = -50, Percent = 1f},
				Top = {Pixels = 50},
				HAlign = 1f
			}.WithView(100f, 1000f);
			uIPanel.Append(uIScrollbar);

			uINoModsFoundText = new UIText(Language.GetTextValue("tModLoader.MBNoModsFound")) {
				HAlign = 0.5f
			}.WithPadding(15f);

			modList.SetScrollbar(uIScrollbar);
			uIHeaderTextPanel = new UITextPanel<string>(Language.GetTextValue("tModLoader.MenuModBrowser"), 0.8f, true) {
				HAlign = 0.5f,
				Top = {Pixels = -35},
				BackgroundColor = UICommon.defaultUIBlue
			}.WithPadding(15f);
			uIElement.Append(uIHeaderTextPanel);

			reloadButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.MBGettingData")) {
				Width = {Pixels = -10, Percent = 0.5f},
				Height = {Pixels = 25},
				VAlign = 1f,
				Top = {Pixels = -65}
			}.WithFadedMouseOver();
			reloadButton.OnClick += ReloadList;
			uIElement.Append(reloadButton);

			var backButton = new UITextPanel<string>(Language.GetTextValue("UI.Back")) {
				Width = {Pixels = -10, Percent = 0.5f},
				Height = {Pixels = 25},
				VAlign = 1f,
				Top = {Pixels = -20}
			}.WithFadedMouseOver();
			backButton.OnClick += BackClick;
			uIElement.Append(backButton);

			clearButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.MBClearSpecialFilter", "??")) {
				Width = {Pixels = -10, Percent = 0.5f},
				Height = {Pixels = 25},
				HAlign = 1f,
				VAlign = 1f,
				Top = {Pixels = -65},
				BackgroundColor = Color.Purple * 0.7f
			}.WithFadedMouseOver(Color.Purple, Color.Purple * 0.7f);
			clearButton.OnClick += ClearFilters;

			downloadAllButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.MBDownloadAll")) {
				Width = {Pixels = -10, Percent = 0.5f},
				Height = {Pixels = 25},
				HAlign = 1f,
				VAlign = 1f,
				Top = {Pixels = -20},
				BackgroundColor = Color.Azure * 0.7f
			}.WithFadedMouseOver(Color.Azure, Color.Azure * 0.7f);
			downloadAllButton.OnClick += DownloadAllFilteredMods;

			updateAllButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.MBUpdateAll")) {
				Width = {Pixels = -10, Percent = 0.5f},
				Height = {Pixels = 25},
				HAlign = 1f,
				VAlign = 1f,
				Top = {Pixels = -20},
				BackgroundColor = Color.Orange * 0.7f
			}.WithFadedMouseOver(Color.Orange, Color.Orange * 0.7f);
			updateAllButton.OnClick += UpdateAllMods;

			Append(uIElement);

			var upperMenuContainer = new UIElement {
				Width = {Percent = 1f},
				Height = {Pixels = 32},
				Top = {Pixels = 10}
			};
			
			_categoryButtons.Add(SortModeFilterToggle);
			upperMenuContainer.Append(SortModeFilterToggle);
			
			_categoryButtons.Add(UpdateFilterToggle);
			upperMenuContainer.Append(UpdateFilterToggle);
			
			_categoryButtons.Add(ModSideFilterToggle);
			upperMenuContainer.Append(ModSideFilterToggle);
			
			_categoryButtons.Add(SearchFilterToggle);
			upperMenuContainer.Append(SearchFilterToggle);

			var filterTextBoxBackground = new UIPanel {
				Top = {Percent = 0f},
				Left = {Pixels = -170, Percent = 1f},
				Width = {Pixels = 135},
				Height = {Pixels = 40}
			};
			filterTextBoxBackground.OnRightClick += (a, b) => filterTextBox.Text = "";
			upperMenuContainer.Append(filterTextBoxBackground);

			filterTextBox = new UIInputTextField(Language.GetTextValue("tModLoader.ModsTypeToSearch")) {
				Top = {Pixels = 5},
				Left = {Pixels = -160, Percent = 1f},
				Width = {Pixels = 100},
				Height = {Pixels = 10}
			};
			filterTextBox.OnTextChange += (sender, e) => updateNeeded = true;
			upperMenuContainer.Append(filterTextBox);
			
			uIPanel.Append(upperMenuContainer);
		}

		private void UpdateAllMods(UIMouseEvent @event, UIElement element) {
			if (!loading) {
				var updatableMods = items.Where(x => x.HasUpdate && !x.UpdateIsDowngrade).Select(x => x.ModName).ToList();
				EnqueueModBrowserDownloads(updatableMods, Language.GetTextValue("tModLoader.MBUpdateAll"));
				StartDownloading();
			}
		}

		private void ClearFilters(UIMouseEvent @event, UIElement element) {
			SpecialModPackFilter = null;
			SpecialModPackFilterTitle = null;
			updateNeeded = true;
			Main.PlaySound(SoundID.MenuTick);
		}

		private void DownloadAllFilteredMods(UIMouseEvent @event, UIElement element) {
			EnqueueModBrowserDownloads(SpecialModPackFilter, SpecialModPackFilterTitle);
			StartDownloading();
		}

		public override void Draw(SpriteBatch spriteBatch) {
			base.Draw(spriteBatch);
			for (int i = 0; i < _categoryButtons.Count; i++)
				if (_categoryButtons[i].IsMouseHovering) {
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

			if (updateAvailable) {
				updateAvailable = false;
				Interface.updateMessage.SetMessage(updateText);
				Interface.updateMessage.SetGotoMenu(Interface.modBrowserID);
				Interface.updateMessage.SetURL(updateURL);
				Interface.updateMessage.SetAutoUpdateURL(autoUpdateURL);
				Main.menuMode = Interface.updateMessageID;
			}

			UILinkPointNavigator.Shortcuts.BackButtonCommand = 101;
		}

		public void BackClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuClose);
			Main.menuMode = 0;

			bool reloadModsNeeded = (aModUpdated || aNewModDownloaded) && ModLoader.autoReloadAndEnableModsLeavingModBrowser || anEnabledModDownloaded;
			bool reloadModsReminder = aModUpdated && !ModLoader.dontRemindModBrowserUpdateReload;
			bool enableModsReminder = aNewModDownloaded && !ModLoader.dontRemindModBrowserDownloadEnable;

			if (reloadModsNeeded) {
				Main.menuMode = Interface.reloadModsID;
			}
			else if (reloadModsReminder || enableModsReminder) {
				string text = Language.GetTextValue(reloadModsReminder ? "tModLoader.ReloadModsReminder" : "tModLoader.EnableModsReminder");
				Interface.infoMessage.Show(text,
					0, null, Language.GetTextValue("tModLoader.DontShowAgain"),
					() => {
						ModLoader.dontRemindModBrowserUpdateReload = true;
						Main.SaveSettings();
					});
			}

			aModUpdated = false;
			aNewModDownloaded = false;
			anEnabledModDownloaded = false;
		}

		private void ReloadList(UIMouseEvent evt, UIElement listeningElement) {
			if (loading) return;
			Main.PlaySound(SoundID.MenuOpen);
			PopulateModBrowser();
		}

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
			if (!updateNeeded) return;
			updateNeeded = false;
			if (!loading) uIPanel.RemoveChild(uILoader);
			filter = filterTextBox.Text;
			modList.Clear();
			modList.AddRange(items.Where(item => item.PassFilters()));
			bool hasNoModsFoundNotif = modList.HasChild(uINoModsFoundText);
			if (modList.Count <= 0 && !hasNoModsFoundNotif)
				modList.Add(uINoModsFoundText);
			else if (hasNoModsFoundNotif)
				modList.RemoveChild(uINoModsFoundText);
			uIElement.RemoveChild(updateAllButton);
			if (SpecialModPackFilter == null && items.Count(x => x.HasUpdate && !x.UpdateIsDowngrade) > 0) uIElement.Append(updateAllButton);
		}

		public override void OnActivate() {
			Main.clrInput();
			if (!loading && items.Count <= 0)
				PopulateModBrowser();
		}

		private bool RemoveItem(UIModDownloadItem item) => items.Remove(item);

		internal void ClearItems() => items.Clear();

		// TODO C WebClient interface is kinda buggy, maybe we should use something like RestSharp
		private void PopulateModBrowser() {
			loading = true;
			SpecialModPackFilter = null;
			SpecialModPackFilterTitle = null;
			reloadButton.SetText(Language.GetTextValue("tModLoader.MBGettingData"));
			SetHeading(Language.GetTextValue("tModLoader.MenuModBrowser"));
			uIPanel.Append(uILoader);
			modList.Clear();
			items.Clear();
			modList.Deactivate();
			try {
				ServicePointManager.Expect100Continue = false;
				string url = "http://javid.ddns.net/tModLoader/listmods.php";
				var values = new NameValueCollection {
					{"modloaderversion", ModLoader.versionedName},
					{"platform", ModLoader.compressedPlatformRepresentation}
				};
				using (var client = new WebClient()) {
					ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, policyErrors) => { return true; };
					client.UploadValuesCompleted += UploadComplete;
					client.UploadValuesAsync(new Uri(url), "POST", values);
				}
			}
			catch (WebException e) {
				ShowOfflineTroubleshootingMessage();
				if (e.Status == WebExceptionStatus.Timeout) {
					SetHeading(Language.GetTextValue("tModLoader.MenuModBrowser") + " " + Language.GetTextValue("tModLoader.MBOfflineWithReason", Language.GetTextValue("tModLoader.MBBusy")));
					return;
				}

				if (e.Status == WebExceptionStatus.ProtocolError) {
					var resp = (HttpWebResponse)e.Response;
					if (resp.StatusCode == HttpStatusCode.NotFound) {
						SetHeading(Language.GetTextValue("tModLoader.MenuModBrowser") + " " + Language.GetTextValue("tModLoader.MBOfflineWithReason", resp.StatusCode));
						return;
					}

					SetHeading(Language.GetTextValue("tModLoader.MenuModBrowser") + " " + Language.GetTextValue("tModLoader.MBOfflineWithReason", resp.StatusCode));
				}
			}
			catch (Exception e) {
				LogModBrowserException(e);
			}
		}

		public void UploadComplete(object sender, UploadValuesCompletedEventArgs e) {
			if (e.Error != null) {
				ShowOfflineTroubleshootingMessage();
				if (e.Cancelled) {
				}
				else {
					var httpStatusCode = GetHttpStatusCode(e.Error);
					if (httpStatusCode == HttpStatusCode.ServiceUnavailable)
						SetHeading(Language.GetTextValue("tModLoader.MenuModBrowser") + " " + Language.GetTextValue("tModLoader.MBOfflineWithReason", Language.GetTextValue("tModLoader.MBBusy")));
					else
						SetHeading(Language.GetTextValue("tModLoader.MenuModBrowser") + " " + Language.GetTextValue("tModLoader.MBOfflineWithReason", Language.GetTextValue("tModLoader.MBUnknown")));
				}

				loading = false;
				reloadButton.SetText(Language.GetTextValue("tModLoader.MBReloadBrowser"));
			}
			else if (!e.Cancelled) {
				reloadButton.SetText(Language.GetTextValue("tModLoader.MBPopulatingBrowser"));
				var result = e.Result;
				string response = Encoding.UTF8.GetString(result);
				Task.Factory
					.StartNew(ModOrganizer.FindMods)
					.ContinueWith(task => {
						PopulateFromJson(task.Result, response);
						loading = false;
						reloadButton.SetText(Language.GetTextValue("tModLoader.MBReloadBrowser"));
					}, TaskScheduler.FromCurrentSynchronizationContext());
			}
		}

		private void PopulateFromJson(LocalMod[] installedMods, string json) {
			try {
				JObject jsonObject;
				try {
					jsonObject = JObject.Parse(json);
				}
				catch (Exception e) {
					throw new Exception("Bad JSON: " + json, e);
				}

				var updateObject = (JObject)jsonObject["update"];
				if (updateObject != null) {
					updateAvailable = true;
					updateText = (string)updateObject["message"];
					updateURL = (string)updateObject["url"];
					autoUpdateURL = (string)updateObject["autoupdateurl"];
				}

				var modlist = (JArray)jsonObject["modlist"];
				foreach (var mod in modlist.Children<JObject>()) items.Add(UIModDownloadItem.FromJson(installedMods, mod));
				updateNeeded = true;
			}
			catch (Exception e) {
				LogModBrowserException(e);
			}
		}

		/// <summary>
		///     Enqueue a Mod browser item to the download manager
		/// </summary>
		internal void EnqueueModBrowserDownload(UIModDownloadItem mod) {
			var req = new HttpDownloadRequest(
				mod.DisplayName,
				$"{ModLoader.ModPath}{Path.DirectorySeparatorChar}{mod.ModName}.tmod",
				() => (HttpWebRequest)WebRequest.Create(mod.DownloadUrl),
				mod
			);
			req.OnComplete += () => { OnModDownloadCompleted(req); };
			Interface.downloadManager.EnqueueRequest(req);
		}

		/// <summary>
		///     Enqueues a list of mods, if found on the browser (also used for ModPacks)
		/// </summary>
		internal void EnqueueModBrowserDownloads(IEnumerable<string> modNames, string overrideUiTitle = null) {
			Main.PlaySound(SoundID.MenuTick);

			foreach (string desiredMod in modNames) {
				var mod = items.FirstOrDefault(x => x.ModName == desiredMod);
				if (mod == null) // Not found on the browser
					_missingMods.Add(desiredMod);
				else if (mod.Installed == null || mod.HasUpdate) // Found, enqueue download
					EnqueueModBrowserDownload(mod);
			}

			Interface.downloadManager.OverrideName = overrideUiTitle;
		}

		/// <summary>
		///     Will prompt the download manager to begin downloading
		/// </summary>
		internal void StartDownloading() {
			Interface.downloadManager.OnQueueProcessed = () => {
				Interface.modBrowser.updateNeeded = true;
				Main.menuMode = Interface.modBrowserID;
				if (_missingMods.Count > 0) 
					Interface.infoMessage.Show(Language.GetTextValue("tModLoader.MBModsNotFoundOnline", string.Join(",", _missingMods)), Interface.modBrowserID);
				_missingMods.Clear();
			};
			Main.menuMode = Interface.downloadManagerID;
		}

		private void OnModDownloadCompleted(HttpDownloadRequest req) {
			if (req.Response.StatusCode != HttpStatusCode.OK) {
				string errorKey = req.Response.StatusCode == HttpStatusCode.ServiceUnavailable ? "MBExceededBandwidth" : "MBUnknownMBError";
				Interface.errorMessage.Show(Language.GetTextValue("tModLoader." + errorKey), 0);
			}
			else if (req.CustomData is UIModDownloadItem currentDownload) {
				ProcessDownloadedMod(req, currentDownload);
			}
		}

		private void ProcessDownloadedMod(HttpDownloadRequest req, UIModDownloadItem currentDownload) {
			var mod = ModLoader.GetMod(currentDownload.ModName);
			if (mod != null) {
				Logging.tML.Info(Language.GetTextValue("tModLoader.MBReleaseFileHandle", $"{mod.Name}: {mod.DisplayName}"));
				mod.File?.Close(); // if the mod is currently loaded, the file-handle needs to be released
				Interface.modBrowser.anEnabledModDownloaded = true;
			}

			if (!currentDownload.HasUpdate) Interface.modBrowser.aNewModDownloaded = true;
			else Interface.modBrowser.aModUpdated = true;
			
			if (ModLoader.autoReloadAndEnableModsLeavingModBrowser) ModLoader.EnableMod(currentDownload.ModName);
			Interface.modBrowser.RemoveItem(currentDownload);
			updateNeeded = true;
		}

		private void SetHeading(string heading) {
			uIHeaderTextPanel.SetText(heading, 0.8f, true);
			uIHeaderTextPanel.Recalculate();
		}

		private void ShowOfflineTroubleshootingMessage() {
			var message = new UIMessageBox(Language.GetTextValue("tModLoader.MBOfflineTroubleshooting")) {
				Width = {Percent = 1},
				Height = {Pixels = 400, Percent = 0}
			};
			message.OnDoubleClick += (a, b) => {
				Process.Start("http://javid.ddns.net/tModLoader/DirectModDownloadListing.php");
			};
			modList.Add(message);
			message.SetScrollbar(new UIScrollbar());
			uIPanel.RemoveChild(uILoader);
		}

		// TODO why is this here?
		//unused
		//public XmlDocument GetDataFromUrl(string url)
		//{
		//	XmlDocument urlData = new XmlDocument();
		//	HttpWebRequest rq = (HttpWebRequest)WebRequest.Create(url);
		//	rq.Timeout = 5000;
		//	HttpWebResponse response = rq.GetResponse() as HttpWebResponse;
		//	using (Stream responseStream = response.GetResponseStream())
		//	{
		//		XmlTextReader reader = new XmlTextReader(responseStream);
		//		urlData.Load(reader);
		//	}
		//	return urlData;
		//}

		private HttpStatusCode GetHttpStatusCode(Exception err) {
			if (err is WebException we)
				if (we.Response is HttpWebResponse response)
					return response.StatusCode;
			return 0;
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

		internal static void LogModUnpublishInfo(string message) {
			Logging.tML.Info(message);
			Interface.errorMessage.Show(Language.GetTextValue("tModLoader.MBServerResponse", message), Interface.managePublishedID);
		}
	}
}