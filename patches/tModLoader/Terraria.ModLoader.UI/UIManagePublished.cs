using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Specialized;
using System.Net;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace Terraria.ModLoader.UI
{
	internal class UIManagePublished : UIState
	{
		private UIList myPublishedMods;
		public UITextPanel<string> textPanel;

		public override void OnInitialize() {
			var area = new UIElement {
				Width = { Percent = 0.8f },
				MaxWidth = UICommon.MaxPanelWidth,
				Top = { Pixels = 220 },
				Height = { Pixels = -220, Percent = 1f },
				HAlign = 0.5f
			};

			var uIPanel = new UIPanel {
				Width = { Percent = 1f },
				Height = { Pixels = -110, Percent = 1f },
				BackgroundColor = UICommon.mainPanelBackground
			};
			area.Append(uIPanel);

			myPublishedMods = new UIList {
				Width = { Pixels = -25, Percent = 1f },
				Height = { Percent = 1f },
				ListPadding = 5f
			};
			uIPanel.Append(myPublishedMods);

			var uIScrollbar = new UIScrollbar {
				Height = { Percent = 1f },
				HAlign = 1f
			}.WithView(100f, 1000f);
			uIPanel.Append(uIScrollbar);
			myPublishedMods.SetScrollbar(uIScrollbar);

			textPanel = new UITextPanel<string>(Language.GetTextValue("tModLoader.MBMyPublishedMods"), 0.8f, true) {
				HAlign = 0.5f,
				Top = { Pixels = -35 },
				BackgroundColor = UICommon.defaultUIBlue
			}.WithPadding(15);
			area.Append(textPanel);

			var backButton = new UITextPanel<string>(Language.GetTextValue("UI.Back")) {
				VAlign = 1f,
				Height = { Pixels = 25 },
				Width = new StyleDimension(-10f, 1f / 2f),
				Top = { Pixels = -20 }
			}.WithFadedMouseOver();
			backButton.OnClick += BackClick;
			area.Append(backButton);

			Append(area);
		}

		private static void BackClick(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuClose);
			Main.menuMode = Interface.modSourcesID;
		}

		public override void Draw(SpriteBatch spriteBatch) {
			base.Draw(spriteBatch);
			UILinkPointNavigator.Shortcuts.BackButtonCommand = 100;
			UILinkPointNavigator.Shortcuts.BackButtonGoto = Interface.modSourcesID;
		}

		public override void OnActivate() {
			myPublishedMods.Clear();
			textPanel.SetText(Language.GetTextValue("tModLoader.MBMyPublishedMods"), 0.8f, true);
			string response = "";
			try {
				ServicePointManager.Expect100Continue = false;
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
			catch (WebException e) {
				if (e.Status == WebExceptionStatus.Timeout) {
					textPanel.SetText(Language.GetTextValue("tModLoader.MenuModBrowser") + " " + Language.GetTextValue("tModLoader.MBOfflineWithReason", Language.GetTextValue("tModLoader.MBBusy")), 0.8f, true);
					return;
				}
				textPanel.SetText(Language.GetTextValue("tModLoader.MenuModBrowser") + " " + Language.GetTextValue("tModLoader.MBOfflineWithReason", ""), 0.8f, true);
				return;
			}
			catch (Exception e) {
				UIModBrowser.LogModBrowserException(e);
				return;
			}
			try {
				var a = JArray.Parse(response);

				foreach (JObject o in a.Children<JObject>()) {
					var modItem = new UIModManageItem(
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
			catch (Exception e) {
				UIModBrowser.LogModBrowserException(e);
			}
		}
	}
}
