using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI.DownloadManager;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace Terraria.ModLoader.UI.ModBrowser
{
	internal partial class UIModBrowser : UIState
	{
		public UIModDownloadItem SelectedItem;
		
		// TODO maybe we can refactor this as a "BrowserState" enum
		public bool Loading;
		public bool aModUpdated;
		public bool anEnabledModDownloaded;
		public bool aNewModDownloaded;
		
		private bool _updateAvailable;
		private string _updateText;
		private string _updateUrl;
		private string _autoUpdateUrl;
		private string _specialModPackFilterTitle;
		private List<string> _specialModPackFilter;
		private readonly List<string> _missingMods = new List<string>();
		private readonly List<UIModDownloadItem> _items = new List<UIModDownloadItem>();
		
		internal bool UpdateNeeded;
		internal string Filter;
		
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
					_backgroundElement.BackgroundColor = UICommon.mainPanelBackground;
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

		internal static bool PlatformSupportsTls12 {
			get {
				foreach (SecurityProtocolType protocol in Enum.GetValues(typeof(SecurityProtocolType)))
					if (protocol.GetHashCode() == 3072)
						return true;
				return false;
			}
		}

		private void UpdateAllMods(UIMouseEvent @event, UIElement element) {
			if (Loading) return;
			var relevantMods = _items.Where(x => x.HasUpdate && !x.UpdateIsDowngrade).Select(x => x.ModName).ToList();
			EnqueueModBrowserDownloads(relevantMods, Language.GetTextValue("tModLoader.MBUpdateAll"));
			StartDownloading();
		}

		private void ClearFilters(UIMouseEvent @event, UIElement element) {
			SpecialModPackFilter = null;
			SpecialModPackFilterTitle = null;
			UpdateNeeded = true;
			Main.PlaySound(SoundID.MenuTick);
		}

		private void DownloadAllFilteredMods(UIMouseEvent @event, UIElement element) {
			EnqueueModBrowserDownloads(SpecialModPackFilter, SpecialModPackFilterTitle);
			StartDownloading();
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
			cts?.Cancel(false);
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
			if (Loading) return;
			Main.PlaySound(SoundID.MenuOpen);
			PopulateModBrowser();
		}

		// TODO if we store a browser 'state' we can probably refactor this
		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
			if (!UpdateNeeded) return;
			UpdateNeeded = false;
			if (!Loading) _backgroundElement.RemoveChild(_loaderElement);
			Filter = FilterTextBox.Text;
			ModList.Clear();
			ModList.AddRange(_items.Where(item => item.PassFilters()));
			bool hasNoModsFoundNotif = ModList.HasChild(NoModsFoundText);
			if (ModList.Count <= 0 && !hasNoModsFoundNotif)
				ModList.Add(NoModsFoundText);
			else if (hasNoModsFoundNotif)
				ModList.RemoveChild(NoModsFoundText);
			_rootElement.RemoveChild(_updateAllButton);
			if (SpecialModPackFilter == null && _items.Count(x => x.HasUpdate && !x.UpdateIsDowngrade) > 0) _rootElement.Append(_updateAllButton);
		}

		public override void OnActivate() {
			Main.clrInput();
			if (!Loading && _items.Count <= 0)
				PopulateModBrowser();
		}

		private bool RemoveItem(UIModDownloadItem item) => _items.Remove(item);

		internal void ClearItems() => _items.Clear();

		private CancellationTokenSource cts;
		
		// TODO C WebClient interface is kinda buggy, maybe we should use something like RestSharp
		private void PopulateModBrowser() {
			Loading = true;
			SpecialModPackFilter = null;
			SpecialModPackFilterTitle = null;
			_reloadButton.SetText(Language.GetTextValue("tModLoader.MBGettingData"));
			SetHeading(Language.GetTextValue("tModLoader.MenuModBrowser"));
			_backgroundElement.Append(_loaderElement);
			ModList.Clear();
			_items.Clear();
			ModList.Deactivate();
			try {
				cts = new CancellationTokenSource();

				Task.Factory.StartNew(() => {
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
				}, cts.Token);
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

				Loading = false;
				_reloadButton.SetText(Language.GetTextValue("tModLoader.MBReloadBrowser"));
			}
			else if (!e.Cancelled) {
				_reloadButton.SetText(Language.GetTextValue("tModLoader.MBPopulatingBrowser"));
				var result = e.Result;
				string response = Encoding.UTF8.GetString(result);
				Task.Factory
					.StartNew(ModOrganizer.FindMods)
					.ContinueWith(task => {
						PopulateFromJson(task.Result, response);
						Loading = false;
						_reloadButton.SetText(Language.GetTextValue("tModLoader.MBReloadBrowser"));
					}, TaskScheduler.Current);
			}
		}

		private void PopulateFromJson(LocalMod[] installedMods, string json) {
			try {
				JObject jsonObject;
				try {
					jsonObject = JObject.Parse(json);
				}
				catch (Exception e) {
					throw new Exception($"Bad JSON: {json}", e);
				}

				var updateObject = (JObject)jsonObject["update"];
				if (updateObject != null) {
					_updateAvailable = true;
					_updateText = (string)updateObject["message"];
					_updateUrl = (string)updateObject["url"];
					_autoUpdateUrl = (string)updateObject["autoupdateurl"];
				}

				var modlist = (JArray)jsonObject["modlist"];
				foreach (var mod in modlist.Children<JObject>()) _items.Add(UIModDownloadItem.FromJson(installedMods, mod));
				UpdateNeeded = true;
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
				var mod = _items.FirstOrDefault(x => x.ModName == desiredMod);
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
				Interface.modBrowser.UpdateNeeded = true;
				Main.menuMode = Interface.modBrowserID;
				if (_missingMods.Count > 0) 
					Interface.infoMessage.Show(Language.GetTextValue("tModLoader.MBModsNotFoundOnline", string.Join(",", _missingMods)), Interface.modBrowserID);
				_missingMods.Clear();
			};
			Main.menuMode = Interface.downloadManagerID;
		}

		internal UIModDownloadItem FindModDownloadItem(string modName) {
			return _items.FirstOrDefault(x => x.ModName.Equals(modName));
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
			UpdateNeeded = true;
		}

		private void SetHeading(string heading) {
			HeaderTextPanel.SetText(heading, 0.8f, true);
			HeaderTextPanel.Recalculate();
		}

		private void ShowOfflineTroubleshootingMessage() {
			var message = new UIMessageBox(Language.GetTextValue("tModLoader.MBOfflineTroubleshooting")) {
				Width = {Percent = 1},
				Height = {Pixels = 400, Percent = 0}
			};
			message.OnDoubleClick += (a, b) => {
				Process.Start("http://javid.ddns.net/tModLoader/DirectModDownloadListing.php");
			};
			ModList.Add(message);
			message.SetScrollbar(new UIScrollbar());
			_backgroundElement.RemoveChild(_loaderElement);
		}

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
	}
}