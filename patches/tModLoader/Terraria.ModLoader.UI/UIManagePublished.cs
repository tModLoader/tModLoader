using System;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using System.Net;
using System.Collections.Specialized;
using Terraria.ID;
using Terraria.UI.Gamepad;
using Newtonsoft.Json.Linq;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Localization;

namespace Terraria.ModLoader.UI
{
	internal class UIManagePublished : UIState
	{
		private UIList myPublishedMods;
		public UITextPanel<string> uITextPanel;

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
			myPublishedMods = new UIList();
			myPublishedMods.Width.Set(-25f, 1f);
			myPublishedMods.Height.Set(0f, 1f);
			myPublishedMods.ListPadding = 5f;
			uIPanel.Append(myPublishedMods);
			UIScrollbar uIScrollbar = new UIScrollbar();
			uIScrollbar.SetView(100f, 1000f);
			uIScrollbar.Height.Set(0f, 1f);
			uIScrollbar.HAlign = 1f;
			uIPanel.Append(uIScrollbar);
			myPublishedMods.SetScrollbar(uIScrollbar);
			uITextPanel = new UITextPanel<string>("My Published Mods", 0.8f, true);
			uITextPanel.HAlign = 0.5f;
			uITextPanel.Top.Set(-35f, 0f);
			uITextPanel.SetPadding(15f);
			uITextPanel.BackgroundColor = new Color(73, 94, 171);
			uIElement.Append(uITextPanel);
			UITextPanel<string> backButton = new UITextPanel<string>(Language.GetTextValue("UI.Back"), 1f, false);
			backButton.VAlign = 1f;
			backButton.Height.Set(25f, 0f);
			backButton.Width.Set(-10f, 1f / 2f);
			backButton.Top.Set(-20f, 0f);
			backButton.OnMouseOver += UICommon.FadedMouseOver;
			backButton.OnMouseOut += UICommon.FadedMouseOut;
			backButton.OnClick += BackClick;
			uIElement.Append(backButton);
			base.Append(uIElement);
		}

		private static void BackClick(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(SoundID.MenuClose);
			Main.menuMode = Interface.modSourcesID;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);
			UILinkPointNavigator.Shortcuts.BackButtonCommand = 100;
			UILinkPointNavigator.Shortcuts.BackButtonGoto = Interface.modSourcesID;
		}

		public override void OnActivate()
		{
			myPublishedMods.Clear();
			uITextPanel.SetText("My Published Mods", 0.8f, true);
			string response = "";
			try
			{
				System.Net.ServicePointManager.Expect100Continue = false;
				string url = "http://javid.ddns.net/tModLoader/listmymods.php";
				var values = new NameValueCollection
				{
					{ "steamid64", ModLoader.SteamID64 },
					{ "modloaderversion", ModLoader.versionedName },
					{ "passphrase", ModLoader.modBrowserPassphrase },
				};
				byte[] result = IO.UploadFile.UploadFiles(url, null, values);
				response = System.Text.Encoding.UTF8.GetString(result);
			}
			catch (WebException e)
			{
				if (e.Status == WebExceptionStatus.Timeout)
				{
					uITextPanel.SetText("Mod Browser OFFLINE (Busy)", 0.8f, true);
					return;
				}
				uITextPanel.SetText("Mod Browser OFFLINE.", 0.8f, true);
				return;
			}
			catch (Exception e)
			{
				ErrorLogger.LogModBrowserException(e);
				return;
			}
			try
			{
				JArray a = JArray.Parse(response);

				foreach (JObject o in a.Children<JObject>())
				{
					UIModManageItem modItem = new UIModManageItem(
						(string)o["displayname"],
						(string)o["name"],
						(string)o["version"],
						(string)o["author"],
						(string)o["downloads"],
						(string)o["downloadsversion"],
						(string)o["modloaderversion"]
					);
					myPublishedMods.Add(modItem);
				}
			}
			catch (Exception e)
			{
				ErrorLogger.LogModBrowserException(e);
				return;
			}
		}
	}
}
