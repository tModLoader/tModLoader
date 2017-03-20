using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics;
using Terraria.ModLoader.IO;
using Terraria.UI;
using System.Net;
using System.Net.Security;
using System.Collections.Specialized;
using System.Xml;
using System.Text;
using Terraria.ID;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Terraria.ModLoader.UI
{
	internal class UIModDownloadItem : UIPanel
	{
		public string mod;
		public string displayname;
		public string version;
		public string author;
		public string download;
		public string timeStamp;
		public string modreferences;
		public ModSide modside;
		public int downloads;
		public int hot;
		private readonly Texture2D dividerTexture;
		private readonly Texture2D innerPanelTexture;
		private readonly UIText modName;
		readonly UITextPanel<string> updateButton;
		public bool update = false;
		public bool updateIsDowngrade = false;
		public bool exists = false;

		public UIModDownloadItem(string displayname, string name, string version, string author, string modreferences, ModSide modside, string download, int downloads, int hot, string timeStamp, bool update, bool updateIsDowngrade, bool exists)
		{
			this.displayname = displayname;
			this.mod = name;
			this.version = version;
			this.author = author;
			this.modreferences = modreferences;
			this.modside = modside;
			this.download = download;
			this.downloads = downloads;
			this.hot = hot;
			this.timeStamp = timeStamp;
			this.update = update;
			this.updateIsDowngrade = updateIsDowngrade;
			this.exists = exists;
			this.BorderColor = new Color(89, 116, 213) * 0.7f;
			this.dividerTexture = TextureManager.Load("Images/UI/Divider");
			this.innerPanelTexture = TextureManager.Load("Images/UI/InnerPanelBackground");
			this.Height.Set(90f, 0f);
			this.Width.Set(0f, 1f);
			base.SetPadding(6f);
			string text = displayname + " " + version;
			this.modName = new UIText(text, 1f, false);
			this.modName.Left.Set(10f, 0f);
			this.modName.Top.Set(5f, 0f);
			base.Append(this.modName);
			UITextPanel<string> button = new UITextPanel<string>("More info", 1f, false);
			button.Width.Set(100f, 0f);
			button.Height.Set(30f, 0f);
			button.Left.Set(5f, 0f);
			button.Top.Set(40f, 0f);
			button.PaddingTop -= 2f;
			button.PaddingBottom -= 2f;
			button.OnMouseOver += UICommon.FadedMouseOver;
			button.OnMouseOut += UICommon.FadedMouseOut;
			button.OnClick += RequestMoreinfo;
			base.Append(button);
			if (update || !exists)
			{
				updateButton = new UITextPanel<string>(this.update ? (updateIsDowngrade ? "Downgrade" : "Update") : "Download", 1f,
					false);
				updateButton.CopyStyle(button);
				updateButton.Width.Set(200f, 0f);
				updateButton.Left.Set(button.Width.Pixels + button.Left.Pixels * 2f + 5f, 0f);
				updateButton.OnMouseOver += UICommon.FadedMouseOver;
				updateButton.OnMouseOut += UICommon.FadedMouseOut;
				updateButton.OnClick += this.DownloadMod;
				base.Append(updateButton);
			}
			if (modreferences.Length > 0)
			{
				UIHoverImage modReferenceIcon = new UIHoverImage(Main.quicksIconTexture, "This mod depends on: " + modreferences);
				modReferenceIcon.Left.Set(-135f, 1f);
				modReferenceIcon.Top.Set(50f, 0f);
				modReferenceIcon.OnClick += (s, e) =>
				{
					UIModDownloadItem modListItem = (UIModDownloadItem) e.Parent;
					Interface.modBrowser.SpecialModPackFilter = modListItem.modreferences.Split(',').ToList();
					Interface.modBrowser.SpecialModPackFilterTitle = "Dependencies"; // Toolong of \n" + modListItem.modName.Text;
					Interface.modBrowser.filterTextBox.currentString = "";
					Interface.modBrowser.SortList();
					Main.PlaySound(SoundID.MenuOpen);
				};
				Append(modReferenceIcon);
			}
			base.OnDoubleClick += RequestMoreinfo;
		}

		public override int CompareTo(object obj)
		{
			switch (Interface.modBrowser.sortMode)
			{
				case SortModes.DisplayNameAtoZ:
					return this.displayname.CompareTo((obj as UIModDownloadItem).displayname);
				case SortModes.DisplayNameZtoA:
					return -1 * this.displayname.CompareTo((obj as UIModDownloadItem).displayname);
				case SortModes.DownloadsAscending:
					return this.downloads.CompareTo((obj as UIModDownloadItem).downloads);
				case SortModes.DownloadsDescending:
					return -1 * this.downloads.CompareTo((obj as UIModDownloadItem).downloads);
				case SortModes.RecentlyUpdated:
					return -1 * this.timeStamp.CompareTo((obj as UIModDownloadItem).timeStamp);
				case SortModes.Hot:
					return -1 * this.hot.CompareTo((obj as UIModDownloadItem).hot);
			}
			return base.CompareTo(obj);
		}

		public override bool PassFilters()
		{
			if (Interface.modBrowser.SpecialModPackFilter != null && !Interface.modBrowser.SpecialModPackFilter.Contains(mod))
			{
				return false;
			}
			if (Interface.modBrowser.filter.Length > 0)
			{
				if (Interface.modBrowser.searchFilterMode == SearchFilter.Author)
				{
					if (author.IndexOf(Interface.modBrowser.filter, StringComparison.OrdinalIgnoreCase) == -1)
					{
						return false;
					}
				}
				else
				{
					if (displayname.IndexOf(Interface.modBrowser.filter, StringComparison.OrdinalIgnoreCase) == -1)
					{
						return false;
					}
				}
			}
			switch (Interface.modBrowser.updateFilterMode)
			{
				case UpdateFilter.All:
					return true;
				case UpdateFilter.Available:
					return update || !exists;
				case UpdateFilter.UpdateOnly:
					return update;
			}
			return true;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);

			CalculatedStyle innerDimensions = base.GetInnerDimensions();
			Vector2 drawPos = new Vector2(innerDimensions.X + 5f, innerDimensions.Y + 30f);
			//draw divider
			spriteBatch.Draw(this.dividerTexture, drawPos, null, Color.White, 0f, Vector2.Zero, new Vector2((innerDimensions.Width - 10f) / 8f, 1f), SpriteEffects.None, 0f);
			// change pos for button
			const int baseWidth = 125; // something like 1 days ago is ~110px, XX minutes ago is ~120 px (longest)
			drawPos = new Vector2(innerDimensions.X + innerDimensions.Width - baseWidth, innerDimensions.Y + 45);
			this.DrawPanel(spriteBatch, drawPos, (float)baseWidth);
			this.DrawTimeText(spriteBatch, drawPos + new Vector2(0f, 5f), baseWidth); // x offset (padding) to do in method
		}

		protected override void DrawChildren(SpriteBatch spriteBatch)
		{
			base.DrawChildren(spriteBatch);

			// show authors on mod title hover, after everything else
			// main.hoverItemName isn't drawn in UI
			if (this.modName.IsMouseHovering)
			{
				string text = "By: " + author;
				float x = Main.fontMouseText.MeasureString(text).X;
				Vector2 vector = Main.MouseScreen + new Vector2(16f);
				if (vector.Y > (float)(Main.screenHeight - 30))
				{
					vector.Y = (float)(Main.screenHeight - 30);
				}
				if (vector.X > (float)Main.screenWidth - x)
				{
					vector.X = (float)(Main.screenWidth - x - 30);
				}
				Utils.DrawBorderStringFourWay(spriteBatch, Main.fontMouseText, text, vector.X, vector.Y, new Color((int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor), Color.Black, Vector2.Zero, 1f);
			}
		}

		private void DrawPanel(SpriteBatch spriteBatch, Vector2 position, float width)
		{
			spriteBatch.Draw(this.innerPanelTexture, position, new Rectangle?(new Rectangle(0, 0, 8, this.innerPanelTexture.Height)), Color.White);
			spriteBatch.Draw(this.innerPanelTexture, new Vector2(position.X + 8f, position.Y), new Rectangle?(new Rectangle(8, 0, 8, this.innerPanelTexture.Height)), Color.White, 0f, Vector2.Zero, new Vector2((width - 16f) / 8f, 1f), SpriteEffects.None, 0f);
			spriteBatch.Draw(this.innerPanelTexture, new Vector2(position.X + width - 8f, position.Y), new Rectangle?(new Rectangle(16, 0, 8, this.innerPanelTexture.Height)), Color.White);
		}

		private void DrawTimeText(SpriteBatch spriteBatch, Vector2 drawPos, int baseWidth)
		{
			if (timeStamp == "0000-00-00 00:00:00")
			{
				return;
			}
			try
			{
				DateTime MyDateTime = DateTime.Parse(timeStamp); // parse date
				string text = TimeHelper.HumanTimeSpanString(MyDateTime); // get time text
				int textWidth = (int)Main.fontMouseText.MeasureString(text).X; // measure text width
				int diffWidth = baseWidth - textWidth; // get difference
				drawPos.X += diffWidth * 0.5f; // add difference as padding
				Utils.DrawBorderString(spriteBatch, text, drawPos, Color.White, 1f, 0f, 0f, -1);
			}
			catch
			{
				return;
			}
		}

		public override void MouseOver(UIMouseEvent evt)
		{
			base.MouseOver(evt);
			this.BackgroundColor = new Color(73, 94, 171);
			this.BorderColor = new Color(89, 116, 213);
		}

		public override void MouseOut(UIMouseEvent evt)
		{
			base.MouseOut(evt);
			this.BackgroundColor = new Color(63, 82, 151) * 0.7f;
			this.BorderColor = new Color(89, 116, 213) * 0.7f;
		}

		internal void DownloadMod(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(12, -1, -1, 1);
			try
			{
				using (WebClient client = new WebClient())
				{
					ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((sender, certificate, chain, policyErrors) => { return true; });
					Interface.modBrowser.selectedItem = this;
					Interface.downloadMod.SetDownloading(displayname);
					Interface.downloadMod.SetCancel(client.CancelAsync);
					client.DownloadProgressChanged += (s, e) =>
					{
						Interface.downloadMod.SetProgress(e);
					};
					client.DownloadFileCompleted += (s, e) =>
					{
						Main.menuMode = Interface.modBrowserID;
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
									Interface.errorMessage.SetMessage("The Mod Browser server is under heavy load. Try again later.");
									Interface.errorMessage.SetGotoMenu(0);
									Interface.errorMessage.SetFile(ErrorLogger.LogPath);
									Main.gameMenu = true;
									Main.menuMode = Interface.errorMessageID;
								}
								else
								{
									Interface.errorMessage.SetMessage("Unknown Mod Browser Error. Try again later.");
									Interface.errorMessage.SetGotoMenu(0);
									Interface.errorMessage.SetFile(ErrorLogger.LogPath);
									Main.gameMenu = true;
									Main.menuMode = Interface.errorMessageID;
								}
							}
						}
						else if (!e.Cancelled)
						{
							// Downloaded OK
							File.Copy(ModLoader.ModPath + Path.DirectorySeparatorChar + "temporaryDownload.tmod", ModLoader.ModPath + Path.DirectorySeparatorChar + mod + ".tmod", true);
							if (!update)
							{
								Interface.modBrowser.aNewModDownloaded = true;
								string path = ModLoader.ModPath + Path.DirectorySeparatorChar + mod + ".enabled";
								using (StreamWriter writer = File.CreateText(path))
								{
									writer.Write("false");
								}
							}
							else
							{
								Interface.modBrowser.aModUpdated = true;
							}
							RemoveChild(updateButton);
						}
						// Clean up: Delete temp
						File.Delete(ModLoader.ModPath + Path.DirectorySeparatorChar + "temporaryDownload.tmod");
					};
					client.DownloadFileAsync(new Uri(download), ModLoader.ModPath + Path.DirectorySeparatorChar + "temporaryDownload.tmod");
					//client.DownloadFileAsync(new Uri(download), ModLoader.ModPath + Path.DirectorySeparatorChar + mod + ".tmod");
				}
				Main.menuMode = Interface.downloadModID;
			}
			catch (WebException e)
			{
				ErrorLogger.LogModBrowserException(e);
			}
		}

		internal void RequestMoreinfo(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(SoundID.MenuOpen);
			try
			{
				ServicePointManager.Expect100Continue = false;
				string url = "http://javid.ddns.net/tModLoader/moddescription.php";
				var values = new NameValueCollection
					{
						{ "modname", mod },
					};
				using (WebClient client = new WebClient())
				{
					ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((sender, certificate, chain, policyErrors) => { return true; });
					client.UploadValuesCompleted += new UploadValuesCompletedEventHandler(Moreinfo);
					client.UploadValuesAsync(new Uri(url), "POST", values);
				}
			}
			catch (Exception e)
			{
				ErrorLogger.LogModBrowserException(e);
				return;
			}
		}

		internal void Moreinfo(Object sender, UploadValuesCompletedEventArgs e)
		{
			string description = "There was a problem, try again";
			string homepage = "";
			if (!e.Cancelled)
			{
				string response = Encoding.UTF8.GetString(e.Result);
				JObject joResponse = JObject.Parse(response);
				description = (string)joResponse["description"];
				homepage = (string)joResponse["homepage"];
			}

			Interface.modInfo.SetModName(this.displayname);
			Interface.modInfo.SetModInfo(description);
			Interface.modInfo.SetGotoMenu(Interface.modBrowserID);
			Interface.modInfo.SetURL(homepage);
			Main.menuMode = Interface.modInfoID;
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

	class TimeHelper
	{
		const int SECOND = 1;
		const int MINUTE = 60 * SECOND;
		const int HOUR = 60 * MINUTE;
		const int DAY = 24 * HOUR;
		const int MONTH = 30 * DAY;

		public static string HumanTimeSpanString(DateTime yourDate)
		{
			var ts = new TimeSpan(DateTime.UtcNow.Ticks - yourDate.Ticks);
			double delta = Math.Abs(ts.TotalSeconds);

			if (delta < 1 * MINUTE)
				return ts.Seconds == 1 ? "1 second ago" : ts.Seconds + " seconds ago";

			if (delta < 2 * MINUTE)
				return "1 minute ago";

			if (delta < 45 * MINUTE)
				return ts.Minutes + " minutes ago";

			if (delta < 90 * MINUTE)
				return "1 hour ago";

			if (delta < 24 * HOUR)
				return ts.Hours + " hours ago";

			if (delta < 48 * HOUR)
				return "1 day ago";

			if (delta < 30 * DAY)
				return ts.Days + " days ago";

			if (delta < 12 * MONTH)
			{
				int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
				return months <= 1 ? "1 month ago" : months + " months ago";
			}
			else
			{
				int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
				return years <= 1 ? "1 year ago" : years + " years ago";
			}
		}
	}
}
