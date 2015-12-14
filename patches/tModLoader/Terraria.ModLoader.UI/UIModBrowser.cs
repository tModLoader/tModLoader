using System;
using System.Diagnostics;
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

namespace Terraria.ModLoader.UI
{
	internal class UIModBrowser : UIState
	{
		public UIList modList;
		public UIModDownloadItem selectedItem;
		public UITextPanel uITextPanel;
		private List<UICycleImage> _categoryButtons = new List<UICycleImage>();
		public bool loaded = false;
		public SortModes sortMode = SortModes.RecentlyUpdated;

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
			uITextPanel = new UITextPanel("Mod Browser", 0.8f, true);
			uITextPanel.HAlign = 0.5f;
			uITextPanel.Top.Set(-35f, 0f);
			uITextPanel.SetPadding(15f);
			uITextPanel.BackgroundColor = new Color(73, 94, 171);
			uIElement.Append(uITextPanel);
			UITextPanel button = new UITextPanel("Reload List", 1f, false);
			button.Width.Set(-10f, 0.5f);
			button.Height.Set(25f, 0f);
			button.VAlign = 1f;
			button.Top.Set(-65f, 0f);
			button.OnMouseOver += new UIElement.MouseEvent(FadedMouseOver);
			button.OnMouseOut += new UIElement.MouseEvent(FadedMouseOut);
			button.OnClick += new UIElement.MouseEvent(ReloadList);
			uIElement.Append(button);
			UITextPanel button3 = new UITextPanel("Back", 1f, false);
			button3.Width.Set(-10f, 0.5f);
			button3.Height.Set(25f, 0f);
			button3.VAlign = 1f;
			button3.Top.Set(-20f, 0f);
			button3.OnMouseOver += new UIElement.MouseEvent(FadedMouseOver);
			button3.OnMouseOut += new UIElement.MouseEvent(FadedMouseOut);
			button3.OnClick += new UIElement.MouseEvent(BackClick);
			uIElement.Append(button3);
			base.Append(uIElement);
			UIElement uIElement2 = new UIElement();
			uIElement2.Width.Set(0f, 1f);
			uIElement2.Height.Set(32f, 0f);
			uIElement2.Top.Set(10f, 0f);
			Texture2D texture = Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.UI.UIModBrowserIcons.png"));
			for (int j = 0; j < 1; j++)
			{
				UICycleImage uIToggleImage = new UICycleImage(texture, 5, 32, 32, 0, 0);
				uIToggleImage.Left.Set((float)(j * 36 + 8), 0f);
				uIToggleImage.OnClick += new UIElement.MouseEvent(this.SortList);
				_categoryButtons.Add(uIToggleImage);
				uIElement2.Append(uIToggleImage);
			}
			uIPanel.Append(uIElement2);
		}

		private void SortList(UIMouseEvent evt, UIElement listeningElement)
		{
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
						vector.X = (float)(Main.screenWidth - 460);
					}
					Utils.DrawBorderStringFourWay(spriteBatch, Main.fontMouseText, text, vector.X, vector.Y, new Color((int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor), Color.Black, Vector2.Zero, 1f);
					return;
				}
			}
		}

		private static void FadedMouseOver(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(12, -1, -1, 1);
			((UIPanel)evt.Target).BackgroundColor = new Color(73, 94, 171);
		}

		private static void FadedMouseOut(UIMouseEvent evt, UIElement listeningElement)
		{
			((UIPanel)evt.Target).BackgroundColor = new Color(63, 82, 151) * 0.7f;
		}

		private static void BackClick(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(11, -1, -1, 1);
			Main.menuMode = 0;
		}

		private static void ReloadList(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10, -1, -1, 1);
			Interface.modBrowser.loaded = false;
			Main.menuMode = Interface.modBrowserID;
		}

		public override void OnActivate()
		{
			if (!loaded)
			{
				modList.Clear();
				TmodFile[] modFiles = ModLoader.FindMods();
				List<BuildProperties> modBuildProperties = new List<BuildProperties>();
				foreach (TmodFile tmodfile in modFiles)
				{
					modBuildProperties.Add(BuildProperties.ReadModFile(tmodfile));
				}
				XmlDocument xmlDoc = new XmlDocument();
				try
				{
					xmlDoc = GetDataFromUrl("http://javid.ddns.net/tModLoader/listmods.php");
				}
				catch (WebException e)
				{
					if (e.Status == WebExceptionStatus.Timeout)
					{
						uITextPanel.SetText("Mod Browser OFFLINE", 0.8f, true);
						return;
					}
				}
				catch (Exception e)
				{
					ErrorLogger.LogModBrowserException(e);
					return;
				}
				foreach (XmlNode xmlNode in xmlDoc.DocumentElement)
				{
					string displayname = xmlNode.SelectSingleNode("displayname").InnerText;
					string name = xmlNode.SelectSingleNode("name").InnerText;
					string version = xmlNode.SelectSingleNode("version").InnerText;
					string author = xmlNode.SelectSingleNode("author").InnerText;
					string description = xmlNode.SelectSingleNode("description").InnerText;
					string download = xmlNode.SelectSingleNode("download").InnerText;
					string timeStamp = xmlNode.SelectSingleNode("updateTimeStamp").InnerText;
					int downloads;
					Int32.TryParse(xmlNode.SelectSingleNode("downloads").InnerText, out downloads);
					bool exists = false;
					bool update = false;
					foreach (BuildProperties bp in modBuildProperties)
					{
						if (bp.displayName.Equals(displayname))
						{
							exists = true;
							if (!bp.version.Equals(version))
							{
								update = true;
							}
						}
					}
					//   if (!exists || update)
					//   {
					UIModDownloadItem modItem = new UIModDownloadItem(/*this, */displayname, name, version, author, description, download, downloads, timeStamp, update, exists);
					modList.Add(modItem);
					//  }
				}
				loaded = true;
			}
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
	}

	public static class SortModesExtensions
	{
		public static SortModes Next(this SortModes sortmode)
		{
			switch (sortmode)
			{
				case SortModes.DisplayNameAtoZ:
					return SortModes.DisplayNameZtoA;
				case SortModes.DisplayNameZtoA:
					return SortModes.DownloadsDescending;
				case SortModes.DownloadsDescending:
					return SortModes.DownloadsAscending;
				case SortModes.DownloadsAscending:
					return SortModes.RecentlyUpdated;
				case SortModes.RecentlyUpdated:
					return SortModes.DisplayNameAtoZ;
			}
			return SortModes.DisplayNameAtoZ;
		}

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
			}
			return "Unknown Sort";
		}
	}

	public enum SortModes
	{
		DisplayNameAtoZ,
		DisplayNameZtoA,
		RecentlyUpdated,
		DownloadsDescending,
		DownloadsAscending,
	}
}
