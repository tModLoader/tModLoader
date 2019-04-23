using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI.DownloadManager;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace Terraria.ModLoader.UI.ModBrowser
{
	// TODO could use a refactor
	internal class UIModBrowser : UIState
	{
		public UIList modList;
		private readonly List<UIModDownloadItem> items = new List<UIModDownloadItem>();
		internal bool updateNeeded;
		public UIModDownloadItem selectedItem;
		private UIElement uIElement;
		private UIPanel uIPanel;
		private UILoaderAnimatedImage uILoader;
		public UITextPanel<string> uIHeaderTextPanel;
		public UIText uINoModsFoundText;
		internal UIInputTextField filterTextBox;
		internal readonly List<UICycleImage> _categoryButtons = new List<UICycleImage>();
		private UITextPanel<string> reloadButton;
		private UITextPanel<string> clearButton;
		private UITextPanel<string> downloadAllButton;
		private UITextPanel<string> updateAllButton;
		public UICycleImage UpdateFilterToggle;
		public UICycleImage SortModeFilterToggle;
		public UICycleImage ModSideFilterToggle;
		public UICycleImage SearchFilterToggle;
		public bool loading;
		public ModBrowserSortMode sortMode = ModBrowserSortMode.RecentlyUpdated;
		public UpdateFilter updateFilterMode = UpdateFilter.Available;
		public SearchFilter searchFilterMode = SearchFilter.Name;
		public ModSideFilter modSideFilterMode = ModSideFilter.All;
		internal string filter;
		private bool updateAvailable;
		private string updateText;
		private string updateURL;
		private string autoUpdateURL;
		public bool aModUpdated = false;
		public bool aNewModDownloaded = false;
		public bool anEnabledModDownloaded = false; // disregard user preference for mods that are enabled
		private string _specialModPackFilterTitle;
		internal string SpecialModPackFilterTitle {
			get => _specialModPackFilterTitle;
			set {
				clearButton.SetText(Language.GetTextValue("tModLoader.MBClearSpecialFilter", value));
				_specialModPackFilterTitle = value;
			}
		}
		private List<string> _specialModPackFilter;
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
				BackgroundColor = UICommon.mainPanelBackground,
				PaddingTop = 0f
			};
			uIElement.Append(uIPanel);

			uILoader = new UILoaderAnimatedImage(0.5f, 0.5f, 1f);

			modList = new UIList {
				Width = { Pixels = -25, Percent = 1f },
				Height = { Pixels = -50, Percent = 1f },
				Top = { Pixels = 50 },
				ListPadding = 5f
			};
			uIPanel.Append(modList);

			var uIScrollbar = new UIScrollbar {
				Height = { Pixels = -50, Percent = 1f },
				Top = { Pixels = 50 },
				HAlign = 1f
			}.WithView(100f, 1000f);
			uIPanel.Append(uIScrollbar);

			uINoModsFoundText = new UIText(Language.GetTextValue("tModLoader.MBNoModsFound")) {
				HAlign = 0.5f
			}.WithPadding(15f);

			modList.SetScrollbar(uIScrollbar);
			uIHeaderTextPanel = new UITextPanel<string>(Language.GetTextValue("tModLoader.MenuModBrowser"), 0.8f, true) {
				HAlign = 0.5f,
				Top = { Pixels = -35 },
				BackgroundColor = UICommon.defaultUIBlue
			}.WithPadding(15f);
			uIElement.Append(uIHeaderTextPanel);

			reloadButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.MBGettingData")) {
				Width = { Pixels = -10, Percent = 0.5f },
				Height = { Pixels = 25 },
				VAlign = 1f,
				Top = { Pixels = -65 }
			}.WithFadedMouseOver();
			reloadButton.OnClick += ReloadList;
			uIElement.Append(reloadButton);

			var backButton = new UITextPanel<string>(Language.GetTextValue("UI.Back")) {
				Width = { Pixels = -10, Percent = 0.5f },
				Height = { Pixels = 25 },
				VAlign = 1f,
				Top = { Pixels = -20 }
			}.WithFadedMouseOver();
			backButton.OnClick += BackClick;
			uIElement.Append(backButton);

			clearButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.MBClearSpecialFilter", "??")) {
				Width = { Pixels = -10, Percent = 0.5f },
				Height = { Pixels = 25 },
				HAlign = 1f,
				VAlign = 1f,
				Top = { Pixels = -65 },
				BackgroundColor = Color.Purple * 0.7f
			}.WithFadedMouseOver(Color.Purple, Color.Purple * 0.7f);
			clearButton.OnClick += (s, e) => {
				// TODO: isn't `Interface.modBrowser` redundant
				Interface.modBrowser.SpecialModPackFilter = null;
				Interface.modBrowser.SpecialModPackFilterTitle = null;
				Interface.modBrowser.updateNeeded = true;
				Main.PlaySound(SoundID.MenuTick);
			};

			downloadAllButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.MBDownloadAll")) {
				Width = { Pixels = -10, Percent = 0.5f },
				Height = { Pixels = 25 },
				HAlign = 1f,
				VAlign = 1f,
				Top = { Pixels = -20 },
				BackgroundColor = Color.Azure * 0.7f
			}.WithFadedMouseOver(Color.Azure, Color.Azure * 0.7f);
			downloadAllButton.OnClick += (s, e) => {
				EnqueueModBrowserDownloads(SpecialModPackFilter, SpecialModPackFilterTitle);
				StartDownloading();
			};

			updateAllButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.MBUpdateAll")) {
				Width = { Pixels = -10, Percent = 0.5f },
				Height = { Pixels = 25 },
				HAlign = 1f,
				VAlign = 1f,
				Top = { Pixels = -20 },
				BackgroundColor = Color.Orange * 0.7f
			}.WithFadedMouseOver(Color.Orange, Color.Orange * 0.7f);
			updateAllButton.OnClick += (s, e) => {
				//TODO: move all click events to separate methods, behavior buried in layout is hard to find
				if (!loading) {
					var updatableMods = items.Where(x => x.update && !x.updateIsDowngrade).Select(x => x.mod).ToList();
					EnqueueModBrowserDownloads(updatableMods, Language.GetTextValue("tModLoader.MBUpdateAll"));
					StartDownloading();
				}
			};

			Append(uIElement);

			var upperMenuContainer = new UIElement {
				Width = { Percent = 1f },
				Height = { Pixels = 32 },
				Top = { Pixels = 10 }
			};
			var texture = Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.UI.UIModBrowserIcons.png"));

			//TODO: lots of duplication in these buttons
			SortModeFilterToggle = new UICycleImage(texture, 6, 32, 32, 0, 0);
			SortModeFilterToggle.setCurrentState((int)sortMode);
			SortModeFilterToggle.OnClick += (a, b) => {
				sortMode = sortMode.NextEnum();
				updateNeeded = true;
			};
			SortModeFilterToggle.OnRightClick += (a, b) => {
				sortMode = sortMode.PreviousEnum();
				updateNeeded = true;
			};
			SortModeFilterToggle.Left.Pixels = 0 * 36 + 8;
			_categoryButtons.Add(SortModeFilterToggle);
			upperMenuContainer.Append(SortModeFilterToggle);

			UpdateFilterToggle = new UICycleImage(texture, 3, 32, 32, 34, 0);
			UpdateFilterToggle.setCurrentState((int)updateFilterMode);
			UpdateFilterToggle.OnClick += (a, b) => {
				updateFilterMode = updateFilterMode.NextEnum();
				updateNeeded = true;
			};
			UpdateFilterToggle.OnRightClick += (a, b) => {
				updateFilterMode = updateFilterMode.PreviousEnum();
				updateNeeded = true;
			};
			UpdateFilterToggle.Left.Pixels = 1 * 36 + 8;
			_categoryButtons.Add(UpdateFilterToggle);
			upperMenuContainer.Append(UpdateFilterToggle);

			ModSideFilterToggle = new UICycleImage(texture, 5, 32, 32, 34 * 5, 0);
			ModSideFilterToggle.setCurrentState((int)modSideFilterMode);
			ModSideFilterToggle.OnClick += (a, b) => {
				modSideFilterMode = modSideFilterMode.NextEnum();
				updateNeeded = true;
			};
			ModSideFilterToggle.OnRightClick += (a, b) => {
				modSideFilterMode = modSideFilterMode.PreviousEnum();
				updateNeeded = true;
			};
			ModSideFilterToggle.Left.Pixels = 2 * 36 + 8;
			_categoryButtons.Add(ModSideFilterToggle);
			upperMenuContainer.Append(ModSideFilterToggle);

			var filterTextBoxBackground = new UIPanel {
				Top = { Percent = 0f },
				Left = { Pixels = -170, Percent = 1f },
				Width = { Pixels = 135 },
				Height = { Pixels = 40 }
			};
			filterTextBoxBackground.OnRightClick += (a, b) => filterTextBox.Text = "";
			upperMenuContainer.Append(filterTextBoxBackground);

			filterTextBox = new UIInputTextField(Language.GetTextValue("tModLoader.ModsTypeToSearch")) {
				Top = { Pixels = 5 },
				Left = { Pixels = -160, Percent = 1f },
				Width = { Pixels = 100 },
				Height = { Pixels = 10 }
			};
			filterTextBox.OnTextChange += (sender, e) => updateNeeded = true;
			upperMenuContainer.Append(filterTextBox);

			SearchFilterToggle = new UICycleImage(texture, 2, 32, 32, 34 * 2, 0);
			SearchFilterToggle.setCurrentState((int)searchFilterMode);
			SearchFilterToggle.OnClick += (a, b) => {
				searchFilterMode = searchFilterMode.NextEnum();
				updateNeeded = true;
			};
			SearchFilterToggle.OnRightClick += (a, b) => {
				searchFilterMode = searchFilterMode.PreviousEnum();
				updateNeeded = true;
			};
			SearchFilterToggle.Left.Pixels = 545f;
			_categoryButtons.Add(SearchFilterToggle);
			upperMenuContainer.Append(SearchFilterToggle);
			uIPanel.Append(upperMenuContainer);
		}

		public override void Draw(SpriteBatch spriteBatch) {
			base.Draw(spriteBatch);
			for (int i = 0; i < _categoryButtons.Count; i++) {
				if (_categoryButtons[i].IsMouseHovering) {
					string text;
					switch (i) {
						case 0:
							text = sortMode.ToFriendlyString();
							break;
						case 1:
							text = updateFilterMode.ToFriendlyString();
							break;
						case 2:
							text = modSideFilterMode.ToFriendlyString();
							break;
						case 3:
							text = searchFilterMode.ToFriendlyString();
							break;
						default:
							text = "None";
							break;
					}
					UICommon.DrawHoverStringInBounds(spriteBatch, text);
					return;
				}
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

		public static void BackClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuClose);
			Main.menuMode = 0;
			if ((Interface.modBrowser.aModUpdated || Interface.modBrowser.aNewModDownloaded) && ModLoader.autoReloadAndEnableModsLeavingModBrowser || Interface.modBrowser.anEnabledModDownloaded) {
				Main.menuMode = Interface.reloadModsID;
			}
			//TODO why do I have to read this, the only difference between these is Enable vs Reload
			else if (Interface.modBrowser.aModUpdated && !ModLoader.dontRemindModBrowserUpdateReload) {
				Interface.infoMessage.Show(Language.GetTextValue("tModLoader.ReloadModsReminder"),
					0, null, Language.GetTextValue("tModLoader.DontShowAgain"),
					() => { ModLoader.dontRemindModBrowserUpdateReload = true; Main.SaveSettings(); });
			}
			else if (Interface.modBrowser.aNewModDownloaded && !ModLoader.dontRemindModBrowserDownloadEnable) {
				Interface.infoMessage.Show(Language.GetTextValue("tModLoader.EnableModsReminder"),
					0, null, Language.GetTextValue("tModLoader.DontShowAgain"),
					() => { ModLoader.dontRemindModBrowserDownloadEnable = true; Main.SaveSettings(); });
			}
			Interface.modBrowser.aModUpdated = false;
			Interface.modBrowser.aNewModDownloaded = false;
			Interface.modBrowser.anEnabledModDownloaded = false;
		}

		private void ReloadList(UIMouseEvent evt, UIElement listeningElement) {
			if (!loading) {
				Main.PlaySound(SoundID.MenuOpen);
				PopulateModBrowser();
			}
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
			if (SpecialModPackFilter == null && items.Count(x => x.update && !x.updateIsDowngrade) > 0) {
				uIElement.Append(updateAllButton);
			}

		}

		public override void OnActivate() {
			Main.clrInput();
			if (!loading && items.Count <= 0)
				PopulateModBrowser();
		}

		internal bool RemoveItem(UIModDownloadItem item) {
			return items.Remove(item);
		}

		internal void ClearItems() {
			items.Clear();
		}

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
				var values = new NameValueCollection
					{
						{ "modloaderversion", ModLoader.versionedName },
						{ "platform", ModLoader.compressedPlatformRepresentation },
					};
				using (WebClient client = new WebClient()) {
					ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((sender, certificate, chain, policyErrors) => { return true; });
					client.UploadValuesCompleted += new UploadValuesCompletedEventHandler(UploadComplete);
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
					HttpStatusCode httpStatusCode = GetHttpStatusCode(e.Error);
					if (httpStatusCode == HttpStatusCode.ServiceUnavailable) {
						SetHeading(Language.GetTextValue("tModLoader.MenuModBrowser") + " " + Language.GetTextValue("tModLoader.MBOfflineWithReason", Language.GetTextValue("tModLoader.MBBusy")));
					}
					else {
						SetHeading(Language.GetTextValue("tModLoader.MenuModBrowser") + " " + Language.GetTextValue("tModLoader.MBOfflineWithReason", Language.GetTextValue("tModLoader.MBUnknown")));
					}
				}
				loading = false;
				reloadButton.SetText(Language.GetTextValue("tModLoader.MBReloadBrowser"));
			}
			else if (!e.Cancelled) {
				reloadButton.SetText(Language.GetTextValue("tModLoader.MBPopulatingBrowser"));
				byte[] result = e.Result;
				string response = Encoding.UTF8.GetString(result);
				Task.Factory
					.StartNew(ModOrganizer.FindMods)
					.ContinueWith(task => {
						PopulateFromJSON(task.Result, response);
						loading = false;
						reloadButton.SetText(Language.GetTextValue("tModLoader.MBReloadBrowser"));
					}, TaskScheduler.FromCurrentSynchronizationContext());
			}
		}

		internal static bool PlatformSupportsTls12 {
			get {
				foreach (SecurityProtocolType protocol in Enum.GetValues(typeof(SecurityProtocolType))) {
					if (protocol.GetHashCode() == 3072) {
						return true;
					}
				}
				return false;
			}
		}

		private void PopulateFromJSON(LocalMod[] installedMods, string json) {
			string tls = PlatformSupportsTls12 ? "&tls12=y" : "";
			try {
				JObject jsonObject;
				try {
					jsonObject = JObject.Parse(json);
				}
				catch (Exception e) {
					throw new Exception("Bad JSON: " + json, e);
				}
				JObject updateObject = (JObject)jsonObject["update"];
				if (updateObject != null) {
					updateAvailable = true;
					updateText = (string)updateObject["message"];
					updateURL = (string)updateObject["url"];
					autoUpdateURL = (string)updateObject["autoupdateurl"];
				}
				JArray modlist = (JArray)jsonObject["modlist"];
				foreach (JObject mod in modlist.Children<JObject>()) {
					string displayname = (string)mod["displayname"];
					//reloadButton.SetText("Adding " + displayname + "...");
					string name = (string)mod["name"];
					string version = (string)mod["version"];
					string author = (string)mod["author"];
					string download = ((string)mod["download"] ?? $"http://javid.ddns.net/tModLoader/download.php?Down=mods/{name}.tmod") + tls;
					int downloads = (int)mod["downloads"];
					int hot = (int)mod["hot"]; // for now, hotness is just downloadsYesterday
					string timeStamp = (string)mod["updateTimeStamp"];
					//string[] modreferences = ((string)mod["modreferences"]).Split(',');
					string modreferences = ((string)mod["modreferences"] ?? "");
					ModSide modside = ModSide.Both; // TODO: add filter option for modside.
					string modIconURL = (string)mod["iconurl"];
					string modsideString = (string)mod["modside"];
					if (modsideString == "Client") modside = ModSide.Client;
					if (modsideString == "Server") modside = ModSide.Server;
					if (modsideString == "NoSync") modside = ModSide.NoSync;
					//bool exists = false; // unused?
					bool update = false;
					bool updateIsDowngrade = false;
					var installed = installedMods.FirstOrDefault(m => m.Name == name);
					if (installed != null) {
						//exists = true;
						var cVersion = new Version(version.Substring(1));
						if (cVersion > installed.modFile.version)
							update = true;
						else if (cVersion < installed.modFile.version)
							update = updateIsDowngrade = true;
					}
					UIModDownloadItem modItem = new UIModDownloadItem(displayname, name, version, author, modreferences, modside, modIconURL, download, downloads, hot, timeStamp, update, updateIsDowngrade, installed);
					items.Add(modItem);
				}
				updateNeeded = true;
			}
			catch (Exception e) {
				LogModBrowserException(e);
			}
		}

		/// <summary>
		/// Enqueue a Mod browser item to the download manager
		/// </summary>
		internal void EnqueueModBrowserDownload(UIModDownloadItem mod) {

			// TODO this will not support concurrent downloading
			string dlFilePath() => $"{ModLoader.ModPath}{Path.DirectorySeparatorChar}{DateTime.Now.Ticks}.tmod";

			Interface.downloadFile.EnqueueRequest(
				new HttpDownloadRequest(
					mod.displayname, 
					dlFilePath(),
					() => (HttpWebRequest)WebRequest.Create(mod.download),
					onFinish: OnModDownloadFinished,
					onCancel: OnModDownloadCancelled) {
					CustomData = mod
				});
		}

		private readonly List<string> _missingMods = new List<string>();

		/// <summary>
		/// Enqueues a list of mods, if found on the browser (also used for ModPacks)
		/// </summary>
		internal void EnqueueModBrowserDownloads(IEnumerable<string> modNames, string overrideUiTitle = null) {
			Main.PlaySound(SoundID.MenuTick);

			foreach (var desiredMod in modNames) {
				var mod = items.FirstOrDefault(x => x.mod == desiredMod);
				if (mod == null) {
					// Not found on the browser
					_missingMods.Add(desiredMod);
				}
				else if (mod.installed == null || mod.update) {
					// Found, enqueue download
					EnqueueModBrowserDownload(mod);
				}
			}

			Interface.downloadFile.OverrideName = overrideUiTitle;
		}

		/// <summary>
		/// Will prompt the download manager to begin downloading
		/// </summary>
		internal void StartDownloading() {
			Interface.downloadFile.OnQueueProcessed = () => {
				Interface.modBrowser.updateNeeded = true;
				Main.menuMode = Interface.modBrowserID;
				if (_missingMods.Count > 0) {
					Interface.infoMessage.Show(Language.GetTextValue("tModLoader.MBModsNotFoundOnline", string.Join(",", _missingMods)), Interface.modBrowserID);
				}
				_missingMods.Clear();
			};
			Main.menuMode = Interface.downloadFileID;
		}

		private void OnModDownloadCancelled(HttpDownloadRequest req) {
			try {
				File.Delete(req.OutputFilePath);
			}
			catch (Exception e) {
				Logging.tML.Error(Language.GetTextValue("tModLoader.MBDownloadFileProblem", req.OutputFilePath), e);
			}
			Main.menuMode = Interface.modBrowserID;
		}

		private void OnModDownloadFinished(HttpDownloadRequest req) {
			if (req.Response.StatusCode != HttpStatusCode.OK) {
				var errorKey = req.Response.StatusCode == HttpStatusCode.ServiceUnavailable ? "MBExceededBandwidth" : "MBUnknownMBError";
				Interface.errorMessage.Show(Language.GetTextValue("tModLoader." + errorKey), 0);
			}
			else if (req.CustomData is UIModDownloadItem currentDownload) {
				ProcessDownloadedMod(req, currentDownload);
			}
		}

		private void ProcessDownloadedMod(HttpDownloadRequest req, UIModDownloadItem currentDownload) {
			var mod = ModLoader.GetMod(currentDownload.mod);
			if (mod != null) {
				Logging.tML.Info(Language.GetTextValue("tModLoader.MBReleaseFileHandle", $"{mod.Name}: {mod.DisplayName}"));
				mod.File?.Close(); // if the mod is currently loaded, the file-handle needs to be released
				Interface.modBrowser.anEnabledModDownloaded = true;
			}
			try {
				//string destinationFileName = ModLoader.GetMod(currentDownload.mod) == null ? currentDownload.mod + ".tmod" : currentDownload.mod + ".tmod.update"; // if above fix has issues we can use this.
				File.Copy(req.OutputFilePath, $"{ModLoader.ModPath}{Path.DirectorySeparatorChar}{currentDownload.mod}.tmod", true);
				File.Delete(req.OutputFilePath);
			}
			catch (Exception e) {
				Logging.tML.Error(Language.GetTextValue("tModLoader.MBDownloadFileProblem", req.OutputFilePath), e);
			}
			finally {
				if (!currentDownload.update) {
					Interface.modBrowser.aNewModDownloaded = true;
				}
				else {
					Interface.modBrowser.aModUpdated = true;
				}
				if (ModLoader.autoReloadAndEnableModsLeavingModBrowser) {
					ModLoader.EnableMod(currentDownload.mod);
				}
				Interface.modBrowser.RemoveItem(currentDownload);
			}
		}

		private void SetHeading(string heading) {
			uIHeaderTextPanel.SetText(heading, 0.8f, true);
			uIHeaderTextPanel.Recalculate();
		}

		private void ShowOfflineTroubleshootingMessage() {
			var message = new UIMessageBox(Language.GetTextValue("tModLoader.MBOfflineTroubleshooting")) {
				Width = { Percent = 1 },
				Height = { Pixels = 400, Percent = 0 }
			};
			message.OnDoubleClick += (a, b) => {
				System.Diagnostics.Process.Start("http://javid.ddns.net/tModLoader/DirectModDownloadListing.php");
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
			if (err is WebException we) {
				if (we.Response is HttpWebResponse response) {
					return response.StatusCode;
				}
			}
			return 0;
		}

		internal static void LogModBrowserException(Exception e) {
			string errorMessage = Language.GetTextValue("tModLoader.MBBrowserError") + "\n\n" + e.Message + "\n" + e.StackTrace;
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
