using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics;
using Terraria.ModLoader.IO;
using Terraria.UI;
using System.Net;

namespace Terraria.ModLoader.UI
{
	internal class UIModDownloadItem : UIPanel
	{
		public string mod;
		public string displayname;
		public string version;
		public string author;
		public string description;
		public string homepage;
		public string download;
		public string timeStamp;
		public int downloads;
		private Texture2D dividerTexture;
		private UIText modName;
		UITextPanel button2;
		public bool update = false;
		public bool exists = false;

		public UIModDownloadItem(string displayname, string name, string version, string author, string description, string homepage, string download, int downloads, string timeStamp, bool update, bool exists)
		{
			this.displayname = displayname;
			this.mod = name;
			this.version = version;
			this.author = author;
			this.description = description;
			this.homepage = homepage;
			this.download = download;
			this.downloads = downloads;
			this.timeStamp = timeStamp;
			this.update = update;
			this.exists = exists;
			this.BorderColor = new Color(89, 116, 213) * 0.7f;
			this.dividerTexture = TextureManager.Load("Images/UI/Divider");
			this.Height.Set(90f, 0f);
			this.Width.Set(0f, 1f);
			base.SetPadding(6f);
			string text = displayname + " " + version + " - by " + author;
			this.modName = new UIText(text, 1f, false);
			this.modName.Left.Set(10f, 0f);
			this.modName.Top.Set(5f, 0f);
			base.Append(this.modName);
			UITextPanel button = new UITextPanel("More info", 1f, false);
			button.Width.Set(100f, 0f);
			button.Height.Set(30f, 0f);
			button.Left.Set(10f, 0f);
			button.Top.Set(40f, 0f);
			button.PaddingTop -= 2f;
			button.PaddingBottom -= 2f;
			button.OnMouseOver += new UIElement.MouseEvent(FadedMouseOver);
			button.OnMouseOut += new UIElement.MouseEvent(FadedMouseOut);
			button.OnClick += new UIElement.MouseEvent(this.Moreinfo);
			base.Append(button);
			if (update || !exists)
			{
				button2 = new UITextPanel(this.update ? "Update" : "Download", 1f, false);
				button2.CopyStyle(button);
				button2.Width.Set(200f, 0f);
				button2.Left.Set(150f, 0f);
				button2.OnMouseOver += new UIElement.MouseEvent(FadedMouseOver);
				button2.OnMouseOut += new UIElement.MouseEvent(FadedMouseOut);
				button2.OnClick += new UIElement.MouseEvent(this.DownloadMod);
				base.Append(button2);
			}
			base.OnDoubleClick += new UIElement.MouseEvent(this.Moreinfo);
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
			}
			return base.CompareTo(obj);
		}

		public override bool PassFilters()
		{
			if (Interface.modBrowser.filter.Length > 0)
			{
				if (displayname.IndexOf(Interface.modBrowser.filter, StringComparison.OrdinalIgnoreCase) == -1)
				{
					return false;
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
			spriteBatch.Draw(this.dividerTexture, drawPos, null, Color.White, 0f, Vector2.Zero, new Vector2((innerDimensions.Width - 10f) / 8f, 1f), SpriteEffects.None, 0f);
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

		private static void FadedMouseOver(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(12, -1, -1, 1);
			((UIPanel)evt.Target).BackgroundColor = new Color(73, 94, 171);
		}

		private static void FadedMouseOut(UIMouseEvent evt, UIElement listeningElement)
		{
			((UIPanel)evt.Target).BackgroundColor = new Color(63, 82, 151) * 0.7f;
		}

		internal void DownloadMod(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(12, -1, -1, 1);
			try
			{
				using (WebClient client = new WebClient())
				{
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
					};
					client.DownloadFileAsync(new Uri(download), ModLoader.ModPath + Path.DirectorySeparatorChar + mod + ".tmod");
					//client.DownloadFile(download, ModLoader.ModPath + Path.DirectorySeparatorChar + mod + ".tmod");
				}
				if (!update)
				{
					string path = ModLoader.ModPath + Path.DirectorySeparatorChar + mod + ".enabled";
					using (StreamWriter writer = File.CreateText(path))
					{
						writer.Write("false");
					}
				}
				base.RemoveChild(button2);
				Main.menuMode = Interface.downloadModID;
			}
			catch (WebException e)
			{
				ErrorLogger.LogModBrowserException(e);
			}
		}

		internal void Moreinfo(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10, -1, -1, 1);
			Interface.modInfo.SetModName(this.displayname);
			Interface.modInfo.SetModInfo(this.description);
			Interface.modInfo.SetGotoMenu(Interface.modBrowserID);
			Interface.modInfo.SetURL(this.homepage);
			Main.menuMode = Interface.modInfoID;
		}
	}
}
