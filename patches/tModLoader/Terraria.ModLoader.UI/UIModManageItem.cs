using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Specialized;
using System.Net;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.UI.ModBrowser;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	//TODO common 'Item' code
	internal class UIModManageItem : UIPanel
	{
		private const string UNPUBLISH_URL = "http://javid.ddns.net/tModLoader/unpublishmymod.php";

		public string Name;
		public string Displayname;
		public string Version;
		public string Author;
		private readonly Texture2D _dividerTexture;
		private readonly UIText _modName;
		private readonly UITextPanel<string> _unpublishButton;

		public UIModManageItem(string displayname, string name, string version, string author, string downloads, string downloadsversion, string modloaderversion) {
			Displayname = displayname;
			Version = version;
			Author = author;
			Name = name;

			BorderColor = new Color(89, 116, 213) * 0.7f;
			_dividerTexture = UICommon.DividerTexture;
			Height.Pixels = 90;
			Width.Percent = 1f;
			SetPadding(6f);

			string text = displayname + " " + version + " - by " + author + " - " + modloaderversion;
			_modName = new UIText(text) {
				Left = { Pixels = 10 },
				Top = { Pixels = 5 }
			};
			Append(_modName);
			var button = new UITextPanel<string>(Language.GetTextValue("tModLoader.MBMyPublishedModsStats", downloads, downloadsversion)) {
				Width = { Pixels = 260 },
				Height = { Pixels = 30 },
				Left = { Pixels = 10 },
				Top = { Pixels = 40 }
			}.WithFadedMouseOver();
			button.PaddingTop -= 2f;
			button.PaddingBottom -= 2f;
			Append(button);
			_unpublishButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.MBUnpublish"));
			_unpublishButton.CopyStyle(button);
			_unpublishButton.Width.Pixels = 150;
			_unpublishButton.Left.Pixels = 360;
			_unpublishButton.WithFadedMouseOver();
			_unpublishButton.OnClick += UnpublishMod;
			Append(_unpublishButton);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);
			CalculatedStyle innerDimensions = GetInnerDimensions();
			var drawPos = new Vector2(innerDimensions.X + 5f, innerDimensions.Y + 30f);
			spriteBatch.Draw(_dividerTexture, drawPos, null, Color.White, 0f, Vector2.Zero, new Vector2((innerDimensions.Width - 10f) / 8f, 1f), SpriteEffects.None, 0f);
		}

		public override void MouseOver(UIMouseEvent evt) {
			base.MouseOver(evt);
			BackgroundColor = UICommon.DefaultUIBlue;
			BorderColor = new Color(89, 116, 213);
		}

		public override void MouseOut(UIMouseEvent evt) {
			base.MouseOut(evt);
			BackgroundColor = new Color(63, 82, 151) * 0.7f;
			BorderColor = new Color(89, 116, 213) * 0.7f;
		}

		internal void UnpublishMod(UIMouseEvent evt, UIElement listeningElement) {
			if (ModLoader.modBrowserPassphrase == string.Empty) {
				Main.menuMode = Interface.enterPassphraseMenuID;
				Interface.enterPassphraseMenu.SetGotoMenu(Interface.managePublishedID);
				return;
			}
			Main.PlaySound(12);
			try {
				ServicePointManager.Expect100Continue = false;

				var values = new NameValueCollection
				{
					{ "name", Name },
					{ "steamid64", ModLoader.SteamID64 },
					{ "modloaderversion", ModLoader.versionedName },
					{ "passphrase", ModLoader.modBrowserPassphrase },
				};
				byte[] result = UploadFile.UploadFiles(UNPUBLISH_URL, null, values);
				string s = System.Text.Encoding.UTF8.GetString(result, 0, result.Length);
				UIModBrowser.LogModUnpublishInfo(s);
			}
			catch (Exception e) {
				UIModBrowser.LogModBrowserException(e);
			}
		}
	}
}
