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
using Terraria.ID;
using Terraria.UI.Gamepad;
using Newtonsoft.Json.Linq;

namespace Terraria.ModLoader.UI
{
	internal class UIModBrowser : UIState
	{
		public UIList modList;
		public UIList modListAll;
		public UIModDownloadItem selectedItem;
		public UITextPanel<string> uITextPanel;
		UIInputTextField filterTextBox;
		private List<UICycleImage> _categoryButtons = new List<UICycleImage>();
		UITextPanel<string> reloadButton;
		public bool loading;
		public SortModes sortMode = SortModes.RecentlyUpdated;
		public UpdateFilter updateFilterMode = UpdateFilter.Available;
		public SearchFilter searchFilterMode = SearchFilter.Name;
		internal string filter;
		private bool updateAvailable;
		private string updateText;
		private string updateURL;
		public bool aModUpdated = false;
		public bool aNewModDownloaded = false;
		public List<string> specialModPackFilter;

		public override void OnInitialize()
		{
			UIElement uIElement = new UIElement();
			uIElement.Width.Set(0f, 0.8f);
			uIElement.MaxWidth.Set(600f, 0f);
			uIElement.Top.Set(220f, 0f);
			uIElement.Height.Set(-220f, 1f);
			uIElement.HAlign = 0.5f;
			UIPanel uIPanel = new UIPanel();
			uIPanel.Width.Set(0f, 1f);
			uIPanel.Height.Set(-110f, 1f);
			uIPanel.BackgroundColor = new Color(33, 43, 79) * 0.8f;
			uIPanel.PaddingTop = 0f;
			uIElement.Append(uIPanel);
			modListAll = new UIList();
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
			uITextPanel = new UITextPanel<string>("Mod Browser", 0.8f, true);
			uITextPanel.HAlign = 0.5f;
			uITextPanel.Top.Set(-35f, 0f);
			uITextPanel.SetPadding(15f);
			uITextPanel.BackgroundColor = new Color(73, 94, 171);
			uIElement.Append(uITextPanel);
			reloadButton = new UITextPanel<string>("Loading...", 1f, false);
			reloadButton.Width.Set(-10f, 0.5f);
			reloadButton.Height.Set(25f, 0f);
			reloadButton.VAlign = 1f;
			reloadButton.Top.Set(-65f, 0f);
			reloadButton.OnMouseOver += UICommon.FadedMouseOver;
			reloadButton.OnMouseOut += UICommon.FadedMouseOut;
			reloadButton.OnClick += ReloadList;
			uIElement.Append(reloadButton);
			UITextPanel<string> backButton = new UITextPanel<string>("Back", 1f, false);
			backButton.Width.Set(-10f, 0.5f);
			backButton.Height.Set(25f, 0f);
			backButton.VAlign = 1f;
			backButton.Top.Set(-20f, 0f);
			backButton.OnMouseOver += UICommon.FadedMouseOver;
			backButton.OnMouseOut += UICommon.FadedMouseOut;
			backButton.OnClick += BackClick;
			uIElement.Append(backButton);
			base.Append(uIElement);
			UIElement uIElement2 = new UIElement();
			uIElement2.Width.Set(0f, 1f);
			uIElement2.Height.Set(32f, 0f);
			uIElement2.Top.Set(10f, 0f);
			Texture2D texture = Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.UI.UIModBrowserIcons.png"));
			UICycleImage uIToggleImage;
			for (int j = 0; j < 2; j++)
			{
				if (j == 0)
				{
					uIToggleImage = new UICycleImage(texture, 6, 32, 32, 0, 0);
					uIToggleImage.setCurrentState((int)sortMode);
					uIToggleImage.OnClick += (a, b) =>
					{
						sortMode = sortMode.NextEnum();
						SortList();
					};
				}
				else
				{
					uIToggleImage = new UICycleImage(texture, 3, 32, 32, 34, 0);
					uIToggleImage.setCurrentState((int)updateFilterMode);
					uIToggleImage.OnClick += (a, b) =>
					{
						updateFilterMode = updateFilterMode.NextEnum();
						SortList();
					};
				}
				uIToggleImage.Left.Set((float)(j * 36 + 8), 0f);
				_categoryButtons.Add(uIToggleImage);
				uIElement2.Append(uIToggleImage);
			}
			filterTextBox = new UIInputTextField("Type to search");
			filterTextBox.Top.Set(5, 0f);
			filterTextBox.Left.Set(-150, 1f);
			filterTextBox.OnTextChange += (sender, e) => SortList();
			uIElement2.Append(filterTextBox);
			UICycleImage SearchFilterToggle = new UICycleImage(texture, 2, 32, 32, 68, 0);
			SearchFilterToggle.setCurrentState((int)searchFilterMode);
			SearchFilterToggle.OnClick += (a, b) =>
			{
				searchFilterMode = searchFilterMode.NextEnum();
				SortList();
			};
			SearchFilterToggle.Left.Set(545f, 0f);
			_categoryButtons.Add(SearchFilterToggle);
			uIElement2.Append(SearchFilterToggle);
			uIPanel.Append(uIElement2);

			PopulateModBrowser();
		}

		internal void SortList()
		{
			filter = filterTextBox.currentString;
			modList.Clear();
			modList.AddRange(modListAll._items.Where(item => item.PassFilters()));
			modList.UpdateOrder();
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
							text = Interface.modBrowser.sortMode.ToFriendlyString();
							break;
						case 1:
							text = Interface.modBrowser.updateFilterMode.ToFriendlyString();
							break;
						case 2:
							text = Interface.modBrowser.searchFilterMode.ToFriendlyString();
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
			if (Interface.modBrowser.aModUpdated)
			{
				Interface.infoMessage.SetMessage("You have updated a mod. Remember to reload mods for it to take effect.");
				Interface.infoMessage.SetGotoMenu(0);
				Main.menuMode = Interface.infoMessageID;
			}
			else if (Interface.modBrowser.aNewModDownloaded)
			{
				Interface.infoMessage.SetMessage("Your recently downloaded mods are currently disabled. Remember to enable and reload if you intend to use them.");
				Interface.infoMessage.SetGotoMenu(0);
				Main.menuMode = Interface.infoMessageID;
			}
			Interface.modBrowser.aModUpdated = false;
			Interface.modBrowser.aNewModDownloaded = false;
		}

		private void ReloadList(UIMouseEvent evt, UIElement listeningElement)
		{
			if (loading)
				return;

			Main.PlaySound(SoundID.MenuOpen);
			specialModPackFilter = null;
			reloadButton.SetText("Reloading...");
			PopulateModBrowser();
		}

		public override void OnActivate()
		{
			Main.clrInput();
		}

		private void PopulateModBrowser()
		{
			loading = true;
			SetHeading("Mod Browser");
			modListAll.Clear();
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

		public void UploadComplete(Object sender, UploadValuesCompletedEventArgs e)
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
			}
			else if (!e.Cancelled)
			{
				byte[] result = e.Result;
				string response = System.Text.Encoding.UTF8.GetString(result);

				// TODO: UI will still be unresponsive here
				TmodFile[] modFiles = ModLoader.FindMods();
				List<BuildProperties> modBuildProperties = new List<BuildProperties>();
				foreach (TmodFile tmodfile in modFiles)
				{
					modBuildProperties.Add(BuildProperties.ReadModFile(tmodfile));
				}
				PopulateFromJSON(modBuildProperties, response);
			}
			loading = false;
			reloadButton.SetText("Reload Mods");
		}

		private void PopulateFromJSON(List<BuildProperties> modBuildProperties, string json)
		{
			try
			{
				JObject jsonObject = JObject.Parse(json);
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
					string name = (string)mod["name"];
					string version = (string)mod["version"];
					string author = (string)mod["author"];
					string download = (string)mod["download"];
					int downloads = (int)mod["downloads"];
					int hot = (int)mod["hot"]; // for now, hotness is just downloadsYesterday
					string timeStamp = (string)mod["updateTimeStamp"];
					bool exists = false;
					bool update = false;
					bool updateIsDowngrade = false;
					foreach (BuildProperties bp in modBuildProperties)
					{
						if (bp.displayName.Equals(displayname))
						{
							exists = true;
							if (!bp.version.Equals(new Version(version.Substring(1))))
							{
								update = true;
								if (bp.version > new Version(version.Substring(1)))
								{
									updateIsDowngrade = true;
								}
							}
						}
					}
					UIModDownloadItem modItem = new UIModDownloadItem(displayname, name, version, author, download, downloads, hot, timeStamp, update, updateIsDowngrade, exists);
					modListAll._items.Add(modItem); //add directly to the underlying, SortList will repopulate it anyway
				}
				SortList();
			}
			catch (Exception e)
			{
				ErrorLogger.LogModBrowserException(e);
				return;
			}
		}

		private void SetHeading(string heading)
		{
			uITextPanel.SetText(heading, 0.8f, true);
			uITextPanel.Recalculate();
		}

		public XmlDocument GetDataFromUrl(string url)
		{
			XmlDocument urlData = new XmlDocument();
			HttpWebRequest rq = (HttpWebRequest)WebRequest.Create(url);
			rq.Timeout = 5000;
			HttpWebResponse response = rq.GetResponse() as HttpWebResponse;
			using (Stream responseStream = response.GetResponseStream())
			{
				XmlTextReader reader = new XmlTextReader(responseStream);
				urlData.Load(reader);
			}
			return urlData;
		}

		HttpStatusCode GetHttpStatusCode(System.Exception err)
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

	public static class SortModesExtensions
	{
		public static string ToFriendlyString(this SortModes sortmode)
		{
			switch (sortmode)
			{
				case SortModes.DisplayNameAtoZ:
					return "Sort mod names alphabetically";
				case SortModes.DisplayNameZtoA:
					return "Sort mod names reverse-alphabetically";
				case SortModes.DownloadsDescending:
					return "Sort by downloads descending";
				case SortModes.DownloadsAscending:
					return "Sort by downloads ascending";
				case SortModes.RecentlyUpdated:
					return "Sort by recently updated";
				case SortModes.Hot:
					return "Sort by popularity";
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
					return "Show all mods";
				case UpdateFilter.Available:
					return "Show mods not installed and updates";
				case UpdateFilter.UpdateOnly:
					return "Show only updates";
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
					return "Search by Mod name";
				case SearchFilter.Author:
					return "Search by Author name";
			}
			return "Unknown Sort";
		}
	}

	public enum SortModes
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
