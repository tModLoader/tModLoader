using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.ModLoader.IO;
using System.Net;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.UI.Gamepad;
using Newtonsoft.Json.Linq;
using Terraria.Localization;

namespace Terraria.ModLoader.UI
{
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
		internal UIInputTextField filterTextBox;
		internal readonly List<UICycleImage> _categoryButtons = new List<UICycleImage>();
		private UITextPanel<string> reloadButton;
		private UITextPanel<string> clearButton;
		private UITextPanel<string> downloadAllButton;
		public UICycleImage uIToggleImage;
		public UICycleImage SearchFilterToggle;
		public bool loading;
		public ModBrowserSortMode sortMode = ModBrowserSortMode.RecentlyUpdated;
		public UpdateFilter updateFilterMode = UpdateFilter.Available;
		public SearchFilter searchFilterMode = SearchFilter.Name;
		internal string filter;
		private bool updateAvailable;
		private string updateText;
		private string updateURL;
		public bool aModUpdated = false;
		public bool aNewModDownloaded = false;
		private string _specialModPackFilterTitle;
		internal string SpecialModPackFilterTitle
		{
			get { return _specialModPackFilterTitle; }
			set
			{
				clearButton.SetText("Clear Special Filter: " + value);
				_specialModPackFilterTitle = value;
			}
		}
		private List<string> _specialModPackFilter;
		public List<string> SpecialModPackFilter
		{
			get { return _specialModPackFilter; }
			set
			{
				if (_specialModPackFilter != null && value == null)
				{
					uIPanel.BackgroundColor = new Color(33, 43, 79) * 0.8f;
					uIElement.RemoveChild(clearButton);
					uIElement.RemoveChild(downloadAllButton);
				}
				else if (_specialModPackFilter == null && value != null)
				{
					uIPanel.BackgroundColor = Color.Purple * 0.7f;
					uIElement.Append(clearButton);
					uIElement.Append(downloadAllButton);
				}
				_specialModPackFilter = value;
			}
		}


		public override void OnInitialize()
		{
			uIElement = new UIElement();
			uIElement.Width.Set(0f, 0.8f);
			uIElement.MaxWidth.Set(600f, 0f);
			uIElement.Top.Set(220f, 0f);
			uIElement.Height.Set(-220f, 1f);
			uIElement.HAlign = 0.5f;

			uIPanel = new UIPanel();
			uIPanel.Width.Set(0f, 1f);
			uIPanel.Height.Set(-110f, 1f);
			uIPanel.BackgroundColor = new Color(33, 43, 79) * 0.8f;
			uIPanel.PaddingTop = 0f;
			uIElement.Append(uIPanel);

			uILoader = new UILoaderAnimatedImage(0.5f, 0.5f, 1f);

			modList = new UIList();
			modList.Width.Set(-25f, 1f);
			modList.Height.Set(-50f, 1f);
			modList.Top.Set(50f, 0f);
			modList.ListPadding = 5f;
			uIPanel.Append(modList);

			UIScrollbar uIScrollbar = new UIScrollbar();
			uIScrollbar.SetView(100f, 1000f);
			uIScrollbar.Height.Set(-50f, 1f);
			uIScrollbar.Top.Set(50f, 0f);
			uIScrollbar.HAlign = 1f;
			uIPanel.Append(uIScrollbar);

			modList.SetScrollbar(uIScrollbar);
			uIHeaderTextPanel = new UITextPanel<string>(Language.GetTextValue("tModLoader.MenuModBrowser"), 0.8f, true);
			uIHeaderTextPanel.HAlign = 0.5f;
			uIHeaderTextPanel.Top.Set(-35f, 0f);
			uIHeaderTextPanel.SetPadding(15f);
			uIHeaderTextPanel.BackgroundColor = new Color(73, 94, 171);
			uIElement.Append(uIHeaderTextPanel);

			reloadButton = new UITextPanel<string>("Getting data...", 1f, false);
			reloadButton.Width.Set(-10f, 0.5f);
			reloadButton.Height.Set(25f, 0f);
			reloadButton.VAlign = 1f;
			reloadButton.Top.Set(-65f, 0f);
			reloadButton.OnMouseOver += UICommon.FadedMouseOver;
			reloadButton.OnMouseOut += UICommon.FadedMouseOut;
			reloadButton.OnClick += ReloadList;
			uIElement.Append(reloadButton);

			UITextPanel<string> backButton = new UITextPanel<string>(Language.GetTextValue("UI.Back"), 1f, false);
			backButton.Width.Set(-10f, 0.5f);
			backButton.Height.Set(25f, 0f);
			backButton.VAlign = 1f;
			backButton.Top.Set(-20f, 0f);
			backButton.OnMouseOver += UICommon.FadedMouseOver;
			backButton.OnMouseOut += UICommon.FadedMouseOut;
			backButton.OnClick += BackClick;
			uIElement.Append(backButton);

			clearButton = new UITextPanel<string>("Clear Special Filter: ??", 1f, false);
			clearButton.Width.Set(-10f, 0.5f);
			clearButton.Height.Set(25f, 0f);
			clearButton.HAlign = 1f;
			clearButton.VAlign = 1f;
			clearButton.Top.Set(-65f, 0f);
			clearButton.BackgroundColor = Color.Purple * 0.7f;
			clearButton.OnMouseOver += (s, e) => UICommon.CustomFadedMouseOver(Color.Purple, s, e);
			clearButton.OnMouseOut += (s, e) => UICommon.CustomFadedMouseOut(Color.Purple * 0.7f, s, e);
			clearButton.OnClick += (s, e) =>
			{
				Interface.modBrowser.SpecialModPackFilter = null;
				Interface.modBrowser.SpecialModPackFilterTitle = null;
				Interface.modBrowser.updateNeeded = true;
				Main.PlaySound(SoundID.MenuTick);
			};

			downloadAllButton = new UITextPanel<string>("Download All", 1f, false);
			downloadAllButton.Width.Set(-10f, 0.5f);
			downloadAllButton.Height.Set(25f, 0f);
			downloadAllButton.HAlign = 1f;
			downloadAllButton.VAlign = 1f;
			downloadAllButton.Top.Set(-20f, 0f);
			downloadAllButton.BackgroundColor = Color.Azure * 0.7f;
			downloadAllButton.OnMouseOver += (s, e) => UICommon.CustomFadedMouseOver(Color.Azure, s, e);
			downloadAllButton.OnMouseOut += (s, e) => UICommon.CustomFadedMouseOut(Color.Azure * 0.7f, s, e);
			downloadAllButton.OnClick += (s, e) => DownloadMods(SpecialModPackFilter, SpecialModPackFilterTitle);

			Append(uIElement);

			UIElement upperMenuContainer = new UIElement();
			upperMenuContainer.Width.Set(0f, 1f);
			upperMenuContainer.Height.Set(32f, 0f);
			upperMenuContainer.Top.Set(10f, 0f);
			Texture2D texture = Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.UI.UIModBrowserIcons.png"));
			for (int j = 0; j < 2; j++)
			{
				if (j == 0)
				{
					uIToggleImage = new UICycleImage(texture, 6, 32, 32, 0, 0);
					uIToggleImage.setCurrentState((int)sortMode);
					uIToggleImage.OnClick += (a, b) =>
					{
						sortMode = sortMode.NextEnum();
						updateNeeded = true;
					};
					uIToggleImage.OnRightClick += (a, b) =>
					{
						sortMode = sortMode.PreviousEnum();
						updateNeeded = true;
					};
				}
				else
				{
					uIToggleImage = new UICycleImage(texture, 3, 32, 32, 34, 0);
					uIToggleImage.setCurrentState((int)updateFilterMode);
					uIToggleImage.OnClick += (a, b) =>
					{
						updateFilterMode = updateFilterMode.NextEnum();
						updateNeeded = true;
					};
					uIToggleImage.OnRightClick += (a, b) =>
					{
						updateFilterMode = updateFilterMode.PreviousEnum();
						updateNeeded = true;
					};
				}
				uIToggleImage.Left.Set((float)(j * 36 + 8), 0f);
				_categoryButtons.Add(uIToggleImage);
				upperMenuContainer.Append(uIToggleImage);
			}
			UIPanel filterTextBoxBackground = new UIPanel();
			filterTextBoxBackground.Top.Set(0f, 0f);
			filterTextBoxBackground.Left.Set(-170, 1f);
			filterTextBoxBackground.Width.Set(135f, 0f);
			filterTextBoxBackground.Height.Set(40f, 0f);
			upperMenuContainer.Append(filterTextBoxBackground);

			filterTextBox = new UIInputTextField(Language.GetTextValue("tModLoader.ModsTypeToSearch"));
			filterTextBox.Top.Set(5, 0f);
			filterTextBox.Left.Set(-160, 1f);
			filterTextBox.OnTextChange += (sender, e) => updateNeeded = true;
			upperMenuContainer.Append(filterTextBox);

			SearchFilterToggle = new UICycleImage(texture, 2, 32, 32, 68, 0);
			SearchFilterToggle.setCurrentState((int)searchFilterMode);
			SearchFilterToggle.OnClick += (a, b) =>
			{
				searchFilterMode = searchFilterMode.NextEnum();
				updateNeeded = true;
			};
			SearchFilterToggle.OnRightClick += (a, b) =>
			{
				searchFilterMode = searchFilterMode.PreviousEnum();
				updateNeeded = true;
			};
			SearchFilterToggle.Left.Set(545f, 0f);
			_categoryButtons.Add(SearchFilterToggle);
			upperMenuContainer.Append(SearchFilterToggle);
			uIPanel.Append(upperMenuContainer);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);
			for (int i = 0; i < this._categoryButtons.Count; i++)
			{
				if (this._categoryButtons[i].IsMouseHovering)
				{
					string text;
					switch (i)
					{
						case 0:
							text = sortMode.ToFriendlyString();
							break;
						case 1:
							text = updateFilterMode.ToFriendlyString();
							break;
						case 2:
							text = searchFilterMode.ToFriendlyString();
							break;
						default:
							text = "None";
							break;
					}
					float x = Main.fontMouseText.MeasureString(text).X;
					Vector2 vector = new Vector2((float)Main.mouseX, (float)Main.mouseY) + new Vector2(16f);
					if (vector.Y > (float)(Main.screenHeight - 30))
					{
						vector.Y = (float)(Main.screenHeight - 30);
					}
					if (vector.X > (float)Main.screenWidth - x)
					{
						vector.X = (float)(Main.screenWidth - x - 30);
					}
					Utils.DrawBorderStringFourWay(spriteBatch, Main.fontMouseText, text, vector.X, vector.Y, new Color((int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor), Color.Black, Vector2.Zero, 1f);
					return;
				}
			}
			if (updateAvailable)
			{
				updateAvailable = false;
				Interface.updateMessage.SetMessage(updateText);
				Interface.updateMessage.SetGotoMenu(Interface.modBrowserID);
				Interface.updateMessage.SetURL(updateURL);
				Main.menuMode = Interface.updateMessageID;
			}
			UILinkPointNavigator.Shortcuts.BackButtonCommand = 101;
		}

		public static void BackClick(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(SoundID.MenuClose);
			Main.menuMode = 0;
			if (Interface.modBrowser.aModUpdated && !ModLoader.dontRemindModBrowserUpdateReload)
			{
				Interface.advancedInfoMessage.SetMessage("You have updated a mod. Remember to reload mods for it to take effect.");
				Interface.advancedInfoMessage.SetGotoMenu(0);
				Interface.advancedInfoMessage.SetAltMessage("Don't show again");
				Interface.advancedInfoMessage.SetAltAction(() => { ModLoader.dontRemindModBrowserUpdateReload = true; Main.SaveSettings(); });
				Main.menuMode = Interface.advancedInfoMessageID;
			}
			else if (Interface.modBrowser.aNewModDownloaded && !ModLoader.dontRemindModBrowserDownloadEnable)
			{
				Interface.advancedInfoMessage.SetMessage("Your recently downloaded mods are currently disabled. Remember to enable and reload if you intend to use them.");
				Interface.advancedInfoMessage.SetGotoMenu(0);
				Interface.advancedInfoMessage.SetAltMessage("Don't show again");
				Interface.advancedInfoMessage.SetAltAction(() => { ModLoader.dontRemindModBrowserDownloadEnable = true; Main.SaveSettings(); });
				Main.menuMode = Interface.advancedInfoMessageID;
			}
			Interface.modBrowser.aModUpdated = false;
			Interface.modBrowser.aNewModDownloaded = false;
		}

		private void ReloadList(UIMouseEvent evt, UIElement listeningElement)
		{
			if (!loading)
			{
				Main.PlaySound(SoundID.MenuOpen);
				PopulateModBrowser();
			}
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			if (!updateNeeded) return;
			updateNeeded = false;
			if (!loading) uIPanel.RemoveChild(uILoader);
			filter = filterTextBox.currentString;
			modList.Clear();
			modList.AddRange(items.Where(item => item.PassFilters()));
		}

		public override void OnActivate()
		{
			Main.clrInput();
			if (!loading && items.Count <= 0)
				PopulateModBrowser();
		}

		internal void ClearItems()
		{
			items.Clear();
		}

		private void PopulateModBrowser()
		{
			loading = true;
			SpecialModPackFilter = null;
			SpecialModPackFilterTitle = null;
			reloadButton.SetText("Getting data...");
			SetHeading(Language.GetTextValue("tModLoader.MenuModBrowser"));
			uIPanel.Append(uILoader);
			modList.Clear();
			items.Clear();
			modList.Deactivate();
			try
			{
				ServicePointManager.Expect100Continue = false;
				string url = "http://javid.ddns.net/tModLoader/listmods.php";
				var values = new NameValueCollection
					{
						{ "modloaderversion", ModLoader.versionedName },
						{ "platform", ModLoader.compressedPlatformRepresentation },
					};
				using (WebClient client = new WebClient())
				{
					ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((sender, certificate, chain, policyErrors) => { return true; });
					client.UploadValuesCompleted += new UploadValuesCompletedEventHandler(UploadComplete);
					client.UploadValuesAsync(new Uri(url), "POST", values);
				}
			}
			catch (WebException e)
			{
				if (e.Status == WebExceptionStatus.Timeout)
				{
					SetHeading("Mod Browser OFFLINE (Busy)");
					return;
				}
				if (e.Status == WebExceptionStatus.ProtocolError)
				{
					var resp = (HttpWebResponse)e.Response;
					if (resp.StatusCode == HttpStatusCode.NotFound)
					{
						SetHeading("Mod Browser OFFLINE (404)");
						return;
					}
					SetHeading("Mod Browser OFFLINE..");
					return;
				}
			}
			catch (Exception e)
			{
				ErrorLogger.LogModBrowserException(e);
				return;
			}
		}

		public void UploadComplete(object sender, UploadValuesCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				if (e.Cancelled)
				{
				}
				else
				{
					HttpStatusCode httpStatusCode = GetHttpStatusCode(e.Error);
					if (httpStatusCode == HttpStatusCode.ServiceUnavailable)
					{
						SetHeading("Mod Browser OFFLINE (Busy)");
					}
					else
					{
						SetHeading("Mod Browser OFFLINE (Unknown)");
					}
				}
				loading = false;
				reloadButton.SetText(Language.GetTextValue("tModLoader.MBReloadBrowser"));
			}
			else if (!e.Cancelled)
			{
				reloadButton.SetText("Populating browser...");
				byte[] result = e.Result;
				string response = Encoding.UTF8.GetString(result);
				if (SynchronizationContext.Current == null)
					SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
				Task.Factory
					.StartNew(ModLoader.FindMods)
					.ContinueWith(task =>
					{
						PopulateFromJSON(task.Result, response);
						loading = false;
						reloadButton.SetText(Language.GetTextValue("tModLoader.MBReloadBrowser"));
					}, TaskScheduler.FromCurrentSynchronizationContext());
			}
		}

		private void PopulateFromJSON(TmodFile[] installedMods, string json)
		{
			try
			{
				JObject jsonObject;
				try
				{
					jsonObject = JObject.Parse(json);
				}
				catch (Exception e)
				{
					throw new Exception("Bad JSON: " + json, e);
				}
				JObject updateObject = (JObject)jsonObject["update"];
				if (updateObject != null)
				{
					updateAvailable = true;
					updateText = (string)updateObject["message"];
					updateURL = (string)updateObject["url"];
				}
				JArray modlist = (JArray)jsonObject["modlist"];
				foreach (JObject mod in modlist.Children<JObject>())
				{
					string displayname = (string)mod["displayname"];
					//reloadButton.SetText("Adding " + displayname + "...");
					string name = (string)mod["name"];
					string version = (string)mod["version"];
					string author = (string)mod["author"];
					string download = (string)mod["download"];
					int downloads = (int)mod["downloads"];
					int hot = (int)mod["hot"]; // for now, hotness is just downloadsYesterday
					string timeStamp = (string)mod["updateTimeStamp"];
					//string[] modreferences = ((string)mod["modreferences"]).Split(',');
					string modreferences = (string)mod["modreferences"];
					ModSide modside = ModSide.Both; // TODO: add filter option for modside.
					string modIconURL = (string)mod["iconurl"];
					string modsideString = (string)mod["modside"];
					if (modsideString == "Client") modside = ModSide.Client;
					if (modsideString == "Server") modside = ModSide.Server;
					if (modsideString == "NoSync") modside = ModSide.NoSync;
					bool exists = false;
					bool update = false;
					bool updateIsDowngrade = false;
					var installed = installedMods.FirstOrDefault(m => m.name == name);
					if (installed != null)
					{
						exists = true;
						var cVersion = new Version(version.Substring(1));
						if (cVersion > installed.version)
							update = true;
						else if (cVersion < installed.version)
							update = updateIsDowngrade = true;
					}
					UIModDownloadItem modItem = new UIModDownloadItem(displayname, name, version, author, modreferences, modside, modIconURL, download, downloads, hot, timeStamp, update, updateIsDowngrade, installed);
					items.Add(modItem);
				}
				updateNeeded = true;
			}
			catch (Exception e)
			{
				ErrorLogger.LogModBrowserException(e);
				return;
			}
		}

		private void DownloadMods(List<string> specialModPackFilter, string SpecialModPackFilterTitle)
		{
			Main.PlaySound(SoundID.MenuTick);
			Interface.downloadMods.SetDownloading(SpecialModPackFilterTitle);
			Interface.downloadMods.SetModsToDownload(specialModPackFilter, items);
			Interface.modBrowser.updateNeeded = true;
			Main.menuMode = Interface.downloadModsID;
		}

		private void SetHeading(string heading)
		{
			uIHeaderTextPanel.SetText(heading, 0.8f, true);
			uIHeaderTextPanel.Recalculate();
		}

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

		private HttpStatusCode GetHttpStatusCode(System.Exception err)
		{
			if (err is WebException)
			{
				WebException we = (WebException)err;
				if (we.Response is HttpWebResponse)
				{
					HttpWebResponse response = (HttpWebResponse)we.Response;
					return response.StatusCode;
				}
			}
			return 0;
		}
	}

	public static class ModBrowserSortModesExtensions
	{
		public static string ToFriendlyString(this ModBrowserSortMode sortmode)
		{
			switch (sortmode)
			{
				case ModBrowserSortMode.DisplayNameAtoZ:
					return Language.GetTextValue("tModLoader.ModsSortNamesAlph");
				case ModBrowserSortMode.DisplayNameZtoA:
					return Language.GetTextValue("tModLoader.ModsSortNamesReverseAlph");
				case ModBrowserSortMode.DownloadsDescending:
					return Language.GetTextValue("tModLoader.MBSortDownloadDesc");
				case ModBrowserSortMode.DownloadsAscending:
					return Language.GetTextValue("tModLoader.MBSortDownloadAsc");
				case ModBrowserSortMode.RecentlyUpdated:
					return Language.GetTextValue("tModLoader.MBSortByRecentlyUpdated");
				case ModBrowserSortMode.Hot:
					return Language.GetTextValue("tModLoader.MBSortByPopularity");
			}
			return "Unknown Sort";
		}
	}

	public static class UpdateFilterModesExtensions
	{
		public static string ToFriendlyString(this UpdateFilter updateFilterMode)
		{
			switch (updateFilterMode)
			{
				case UpdateFilter.All:
					return Language.GetTextValue("tModLoader.MBShowAllMods");
				case UpdateFilter.Available:
					return Language.GetTextValue("tModLoader.MBShowNotInstalledUpdates");
				case UpdateFilter.UpdateOnly:
					return Language.GetTextValue("tModLoader.MBShowUpdates");
			}
			return "Unknown Sort";
		}
	}

	public static class SearchFilterModesExtensions
	{
		public static string ToFriendlyString(this SearchFilter searchFilterMode)
		{
			switch (searchFilterMode)
			{
				case SearchFilter.Name:
					return Language.GetTextValue("tModLoader.ModsSearchByModName");
				case SearchFilter.Author:
					return Language.GetTextValue("tModLoader.ModsSearchByAuthor");
			}
			return "Unknown Sort";
		}
	}

	public enum ModBrowserSortMode
	{
		DisplayNameAtoZ,
		DisplayNameZtoA,
		DownloadsDescending,
		DownloadsAscending,
		RecentlyUpdated,
		Hot,
	}

	public enum UpdateFilter
	{
		All,
		Available,
		UpdateOnly,
	}

	public enum SearchFilter
	{
		Name,
		Author,
	}
}
