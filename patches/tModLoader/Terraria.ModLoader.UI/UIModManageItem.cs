using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Specialized;
using System.Net;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.UI.ModBrowser;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	//TODO common 'Item' code
	internal class UIModManageItem : UIPanel
	{
		public string name;
		public string displayname;
		public string version;
		public string author;
		private Texture2D dividerTexture;
		private UIText modName;
		UITextPanel<string> button2;

		public UIModManageItem(string displayname, string name, string version, string author, string downloads, string downloadsversion, string modloaderversion) {
			this.displayname = displayname;
			this.version = version;
			this.author = author;
			this.name = name;

			BorderColor = new Color(89, 116, 213) * 0.7f;
			dividerTexture = UICommon.dividerTexture;
			Height.Pixels = 90;
			Width.Percent = 1f;
			SetPadding(6f);

			string text = displayname + " " + version + " - by " + author + " - " + modloaderversion;
			modName = new UIText(text) {
				Left = { Pixels = 10 },
				Top = { Pixels = 5 }
			};
			Append(modName);
			var button = new UITextPanel<string>(Language.GetTextValue("tModLoader.MBMyPublishedModsStats", downloads, downloadsversion)) {
				Width = { Pixels = 260 },
				Height = { Pixels = 30 },
				Left = { Pixels = 10 },
				Top = { Pixels = 40 }
			};
			button.PaddingTop -= 2f;
			button.PaddingBottom -= 2f;
			//	button.OnMouseOver += UICommon.FadedMouseOver;
			//	button.OnMouseOut += UICommon.FadedMouseOut;
			Append(button);
			button2 = new UITextPanel<string>(Language.GetTextValue("tModLoader.MBUnpublish"));
			button2.CopyStyle(button);
			button2.Width.Pixels = 150;
			button2.Left.Pixels = 360;
			button2.WithFadedMouseOver();
			button2.OnClick += Unpublish;
			Append(button2);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);
			CalculatedStyle innerDimensions = base.GetInnerDimensions();
			var drawPos = new Vector2(innerDimensions.X + 5f, innerDimensions.Y + 30f);
			spriteBatch.Draw(dividerTexture, drawPos, null, Color.White, 0f, Vector2.Zero, new Vector2((innerDimensions.Width - 10f) / 8f, 1f), SpriteEffects.None, 0f);
		}

		public override void MouseOver(UIMouseEvent evt) {
			base.MouseOver(evt);
			this.BackgroundColor = UICommon.defaultUIBlue;
			this.BorderColor = new Color(89, 116, 213);
		}

		public override void MouseOut(UIMouseEvent evt) {
			base.MouseOut(evt);
			this.BackgroundColor = new Color(63, 82, 151) * 0.7f;
			this.BorderColor = new Color(89, 116, 213) * 0.7f;
		}

		internal void Unpublish(UIMouseEvent evt, UIElement listeningElement) {
			if (ModLoader.modBrowserPassphrase == "") {
				Main.menuMode = Interface.enterPassphraseMenuID;
				Interface.enterPassphraseMenu.SetGotoMenu(Interface.managePublishedID);
				return;
			}
			Main.PlaySound(12);
			try {
				ServicePointManager.Expect100Continue = false;
				string url = "http://javid.ddns.net/tModLoader/unpublishmymod.php";
				var values = new NameValueCollection
				{
					{ "name", this.name },
					{ "steamid64", ModLoader.SteamID64 },
					{ "modloaderversion", ModLoader.versionedName },
					{ "passphrase", ModLoader.modBrowserPassphrase },
				};
				byte[] result = UploadFile.UploadFiles(url, null, values);
				string s = System.Text.Encoding.UTF8.GetString(result, 0, result.Length);
				UIModBrowser.LogModUnpublishInfo(s);
			}
			catch (Exception e) {
				UIModBrowser.LogModBrowserException(e);
			}
		}
	}
}
