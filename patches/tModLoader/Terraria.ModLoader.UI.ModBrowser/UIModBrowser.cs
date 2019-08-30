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
using Ionic.Zlib;
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
		public static bool AvoidGithub;
		public static bool AvoidImgur;
		public static bool PlatformSupportsTls12
			=> FrameworkVersion.Framework != Framework.Mono || FrameworkVersion.Version >= new Version(5, 20);

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
		private readonly List<UIModDownloadItem> _items = new List<UIModDownloadItem>();

		internal bool UpdateNeeded;
		internal string Filter => FilterTextBox.Text;

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
			var relevantMods = _items.Where(x => x.HasUpdate && !x.UpdateIsDowngrade).Select(x => x.ModName).ToList();
			DownloadMods(relevantMods);
		}

		private void ClearFilters(UIMouseEvent @event, UIElement element) {
			SpecialModPackFilter = null;
			SpecialModPackFilterTitle = null;
			UpdateNeeded = true;
			Main.PlaySound(SoundID.MenuTick);
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
			_cts?.Cancel(false);
			Main.PlaySound(SoundID.MenuClose);
			Main.menuMode = 0;

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
		}

		private void ReloadList(UIMouseEvent evt, UIElement listeningElement) {
			if (Loading) return;
			Main.PlaySound(SoundID.MenuOpen);
			PopulateModBrowser();
		}

		// TODO if we store a browser 'state' we can probably refactor this
		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
			if (!UpdateNeeded || Loading) return;
			UpdateNeeded = false;
			if (!Loading) _backgroundElement.RemoveChild(_loaderElement);
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
			if (!Loading && _items.Count <= 0) {
				PopulateModBrowser();
			}
		}

		internal bool RemoveItem(UIModDownloadItem item) => _items.Remove(item);

		internal void ClearItems() => _items.Clear();

		private CancellationTokenSource _cts;
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
				_cts = new CancellationTokenSource();

				Task.Factory.StartNew(() => {
					ServicePointManager.Expect100Continue = false;
					string url = "http://javid.ddns.net/tModLoader/listmods.php";
					var values = new NameValueCollection {
						{"modloaderversion", ModLoader.versionedName},
						{"platform", ModLoader.CompressedPlatformRepresentation},
						{"netversion", FrameworkVersion.Version.ToString()}
					};
					using (var client = new WebClient()) {
						ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, policyErrors) => { return true; };
						client.UploadValuesCompleted += UploadComplete;
						client.UploadValuesAsync(new Uri(url), "POST", values);
					}
				}, _cts.Token);
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

				JArray modlist;
				string modlist_compressed = (string)jsonObject["modlist_compressed"];
				if (modlist_compressed != null) {
					byte[] data = Convert.FromBase64String(modlist_compressed);
					using (GZipStream zip = new GZipStream(new MemoryStream(data), CompressionMode.Decompress))
					using (var reader = new StreamReader(zip))
						modlist = JArray.Parse(reader.ReadToEnd());
				}
				else {
					// Fallback if needed.
					modlist = (JArray)jsonObject["modlist"];
				}
				foreach (var mod in modlist.Children<JObject>()) _items.Add(UIModDownloadItem.FromJson(installedMods, mod));
				UpdateNeeded = true;
			}
			catch (Exception e) {
				LogModBrowserException(e);
			}
		}

		/// <summary>
		///     Enqueues a list of mods, if found on the browser (also used for ModPacks)
		/// </summary>
		internal void DownloadMods(IEnumerable<string> modNames) {
			var downloads = new List<DownloadFile>();

			foreach (string desiredMod in modNames) {
				var mod = _items.FirstOrDefault(x => x.ModName == desiredMod);
				if (mod == null) { // Not found on the browser
					_missingMods.Add(desiredMod);
				}
				else if (mod.Installed == null || mod.HasUpdate) { // Found, add to downloads
					var modDownload = mod.GetModDownload();
					downloads.Add(modDownload);
				}
			}

			// If no download detected for some reason (e.g. empty modpack filter), prevent switching UI
			if (downloads.Count <= 0) return;

			Main.PlaySound(SoundID.MenuTick);
			Interface.downloadProgress.gotoMenu = Interface.modBrowserID;
			Interface.downloadProgress.OnDownloadsComplete += () => {
				if (_missingMods.Count > 0) {
					Interface.infoMessage.Show(Language.GetTextValue("tModLoader.MBModsNotFoundOnline", string.Join(",", _missingMods)), Interface.modBrowserID);
				}
				_missingMods.Clear();
			};

			Interface.downloadProgress.HandleDownloads(downloads.ToArray());
		}

		internal UIModDownloadItem FindModDownloadItem(string modName)
			=> _items.FirstOrDefault(x => x.ModName.Equals(modName));

		//		private void OnModDownloadCompleted(HttpDownloadRequest req) {
		//			if (req.Response.StatusCode != HttpStatusCode.OK) {
		//				string errorKey = req.Response.StatusCode == HttpStatusCode.ServiceUnavailable ? "MBExceededBandwidth" : "MBUnknownMBError";
		//				Interface.errorMessage.Show(Language.GetTextValue("tModLoader." + errorKey), 0);
		//			}
		//			else if (req.Success && req.CustomData is UIModDownloadItem currentDownload) {
		//				ProcessDownloadedMod(req, currentDownload);
		//			}
		//		}

		private void SetHeading(string heading) {
			HeaderTextPanel.SetText(heading, 0.8f, true);
			HeaderTextPanel.Recalculate();
		}

		private void ShowOfflineTroubleshootingMessage() {
			var message = new UIMessageBox(Language.GetTextValue("tModLoader.MBOfflineTroubleshooting")) {
				Width = { Percent = 1 },
				Height = { Pixels = 400, Percent = 0 }
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
	}
}