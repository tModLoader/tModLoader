using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.UI
{
	internal class UIModBrowser : UIState
	{
		public UIList modList;
		public UIModDownloadItem selectedItem;
		public bool loaded = false;

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
			uIElement.Append(uIPanel);
			modList = new UIList();
			modList.Width.Set(-25f, 1f);
			modList.Height.Set(0f, 1f);
			modList.ListPadding = 5f;
			uIPanel.Append(modList);
			UIScrollbar uIScrollbar = new UIScrollbar();
			uIScrollbar.SetView(100f, 1000f);
			uIScrollbar.Height.Set(0f, 1f);
			uIScrollbar.HAlign = 1f;
			uIPanel.Append(uIScrollbar);
			modList.SetScrollbar(uIScrollbar);
			UITextPanel uITextPanel = new UITextPanel("Mod Browser", 0.8f, true);
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
					xmlDoc.Load("http://javid.ddns.net/tModLoader/listmods.php");
				}
				catch
				{
					Main.menuMode = Interface.errorMessageID;
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
					UIModDownloadItem modItem = new UIModDownloadItem(/*this, */displayname, name, version, author, description, download, update, exists);
					modList.Add(modItem);
					//  }
				}
				loaded = true;
			}
		}
	}
}
