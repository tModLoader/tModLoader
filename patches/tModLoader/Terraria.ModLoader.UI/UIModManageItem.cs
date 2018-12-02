using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Specialized;
using System.Net;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIModManageItem : UIPanel
	{
		public string name;
		public string displayname;
		public string version;
		public string author;
		private readonly Texture2D dividerTexture;
		private readonly UIText modName;
		UITextPanel<string> button2;

		public UIModManageItem(string displayname, string name, string version, string author, string downloads, string downloadsversion, string modloaderversion) {
			this.displayname = displayname;
			this.version = version;
			this.author = author;
			this.name = name;
			this.BorderColor = new Color(89, 116, 213) * 0.7f;
			this.dividerTexture = TextureManager.Load("Images/UI/Divider");
			this.Height.Set(90f, 0f);
			this.Width.Set(0f, 1f);
			base.SetPadding(6f);
			string text = displayname + " " + version + " - by " + author + " - " + modloaderversion;
			this.modName = new UIText(text, 1f, false);
			this.modName.Left.Set(10f, 0f);
			this.modName.Top.Set(5f, 0f);
			base.Append(this.modName);
			UITextPanel<string> button = new UITextPanel<string>(Language.GetTextValue("tModLoader.MBMyPublishedModsStats", downloads, downloadsversion), 1f, false);
			button.Width.Set(260f, 0f);
			button.Height.Set(30f, 0f);
			button.Left.Set(10f, 0f);
			button.Top.Set(40f, 0f);
			button.PaddingTop -= 2f;
			button.PaddingBottom -= 2f;
			//	button.OnMouseOver += UICommon.FadedMouseOver;
			//	button.OnMouseOut += UICommon.FadedMouseOut;
			base.Append(button);
			button2 = new UITextPanel<string>(Language.GetTextValue("tModLoader.MBUnpublish"), 1f, false);
			button2.CopyStyle(button);
			button2.Width.Set(150f, 0f);
			button2.Left.Set(360f, 0f);
			button2.OnMouseOver += UICommon.FadedMouseOver;
			button2.OnMouseOut += UICommon.FadedMouseOut;
			button2.OnClick += this.Unpublish;
			base.Append(button2);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);
			CalculatedStyle innerDimensions = base.GetInnerDimensions();
			Vector2 drawPos = new Vector2(innerDimensions.X + 5f, innerDimensions.Y + 30f);
			spriteBatch.Draw(this.dividerTexture, drawPos, null, Color.White, 0f, Vector2.Zero, new Vector2((innerDimensions.Width - 10f) / 8f, 1f), SpriteEffects.None, 0f);
		}

		public override void MouseOver(UIMouseEvent evt) {
			base.MouseOver(evt);
			this.BackgroundColor = new Color(73, 94, 171);
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
