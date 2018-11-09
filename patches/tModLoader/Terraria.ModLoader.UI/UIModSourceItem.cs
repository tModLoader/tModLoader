using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIModSourceItem : UIPanel
	{
		private string mod;
		private Texture2D dividerTexture;
		private UIText modName;
		private LocalMod builtMod;

		public UIModSourceItem(string mod, LocalMod builtMod)
		{
			this.mod = mod;
			this.BorderColor = new Color(89, 116, 213) * 0.7f;
			this.dividerTexture = TextureManager.Load("Images/UI/Divider");
			this.Height.Set(90f, 0f);
			this.Width.Set(0f, 1f);
			base.SetPadding(6f);
			string addendum = Path.GetFileName(mod).Contains(" ") ? $"  [c/FF0000:{Language.GetTextValue("tModLoader.MSModSourcesCantHaveSpaces")}]" : "";
			this.modName = new UIText(Path.GetFileName(mod) + addendum, 1f, false);
			this.modName.Left.Set(10f, 0f);
			this.modName.Top.Set(5f, 0f);
			base.Append(this.modName);
			UIAutoScaleTextTextPanel<string> button = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MSBuild"), 1f, false);
			button.Width.Set(100f, 0f);
			button.Height.Set(36f, 0f);
			button.Left.Set(10f, 0f);
			button.Top.Set(40f, 0f);
			button.PaddingTop -= 2f;
			button.PaddingBottom -= 2f;
			button.OnMouseOver += UICommon.FadedMouseOver;
			button.OnMouseOut += UICommon.FadedMouseOut;
			button.OnClick += this.BuildMod;
			base.Append(button);
			UIAutoScaleTextTextPanel<string> button2 = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MSBuildReload"), 1f, false);
			button2.CopyStyle(button);
			button2.Width.Set(200f, 0f);
			button2.Left.Set(150f, 0f);
			button2.OnMouseOver += UICommon.FadedMouseOver;
			button2.OnMouseOut += UICommon.FadedMouseOut;
			button2.OnClick += this.BuildAndReload;
			base.Append(button2);
			this.builtMod = builtMod;
			if (builtMod != null)
			{
				UIAutoScaleTextTextPanel<string> button3 = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MSPublish"), 1f, false);
				button3.CopyStyle(button2);
				button3.Width.Set(100f, 0f);
				button3.Left.Set(390f, 0f);
				button3.OnMouseOver += UICommon.FadedMouseOver;
				button3.OnMouseOut += UICommon.FadedMouseOut;
				button3.OnClick += this.Publish;
				base.Append(button3);
			}
			base.OnDoubleClick += this.BuildAndReload;
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

		public override int CompareTo(object obj)
		{
			UIModSourceItem uIModSourceItem = obj as UIModSourceItem;
			if (uIModSourceItem == null)
			{
				return base.CompareTo(obj);
			}
			if (uIModSourceItem.builtMod == null && builtMod == null)
				return modName.Text.CompareTo(uIModSourceItem.modName.Text);
			if (uIModSourceItem.builtMod == null)
				return -1;
			if (builtMod == null)
				return 1;
			return uIModSourceItem.builtMod.lastModified.CompareTo(builtMod.lastModified);
		}

		private void BuildMod(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10, -1, -1, 1);
			ModLoader.modToBuild = this.mod;
			ModLoader.reloadAfterBuild = false;
			ModLoader.buildAll = false;
			Main.menuMode = Interface.buildModID;
		}

		private void BuildAndReload(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10, -1, -1, 1);
			ModLoader.modToBuild = this.mod;
			ModLoader.reloadAfterBuild = true;
			ModLoader.buildAll = false;
			Main.menuMode = Interface.buildModID;
		}

		private void Publish(UIMouseEvent evt, UIElement listeningElement)
		{
			if (ModLoader.modBrowserPassphrase == "")
			{
				Main.menuMode = Interface.enterPassphraseMenuID;
				Interface.enterPassphraseMenu.SetGotoMenu(Interface.modSourcesID);
				return;
			}
			Main.PlaySound(10);
			try
			{
				var modFile = builtMod.modFile;
				var bp = builtMod.properties;

				var files = new List<UploadFile>();
				files.Add(new UploadFile
				{
					Name = "file",
					Filename = Path.GetFileName(modFile.path),
					//    ContentType = "text/plain",
					Content = File.ReadAllBytes(modFile.path)
				});
				if (modFile.HasFile("icon.png"))
				{
					files.Add(new UploadFile
					{
						Name = "iconfile",
						Filename = "icon.png",
						Content = modFile.GetFile("icon.png")
					});
				}
				if (bp.beta)
					throw new WebException(Language.GetTextValue("tModLoader.BetaModCantPublishError"));
				var values = new NameValueCollection
				{
					{ "displayname", bp.displayName },
					{ "name", modFile.name },
					{ "version", "v"+bp.version },
					{ "author", bp.author },
					{ "homepage", bp.homepage },
					{ "description", bp.description },
					{ "steamid64", ModLoader.SteamID64 },
					{ "modloaderversion", "tModLoader v"+modFile.tModLoaderVersion },
					{ "passphrase", ModLoader.modBrowserPassphrase },
					{ "modreferences", String.Join(", ", bp.modReferences.Select(x => x.mod)) },
					{ "modside", bp.side.ToFriendlyString() },
				};
				if (values["steamid64"].Length != 17)
					throw new WebException($"The steamid64 '{values["steamid64"]}' is invalid, verify that you are logged into Steam and don't have a pirated copy of Terraria.");
				if (string.IsNullOrEmpty(values["author"]))
					throw new WebException($"You need to specify an author in build.txt");
				ServicePointManager.Expect100Continue = false;
				string url = "http://javid.ddns.net/tModLoader/publishmod.php";
				using (PatientWebClient client = new PatientWebClient())
				{
					ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, policyErrors) => true;
					Interface.uploadMod.SetDownloading(modFile.name);
					Interface.uploadMod.SetCancel(() =>
					{
						Main.menuMode = Interface.modSourcesID;
						client.CancelAsync();
					});
					client.UploadProgressChanged += (s, e) => Interface.uploadMod.SetProgress(e);
					client.UploadDataCompleted += (s, e) => PublishUploadDataComplete(s, e, modFile);

					var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x", System.Globalization.NumberFormatInfo.InvariantInfo);
					client.Headers["Content-Type"] = "multipart/form-data; boundary=" + boundary;
					//boundary = "--" + boundary;
					byte[] data = UploadFile.GetUploadFilesRequestData(files, values);
					client.UploadDataAsync(new Uri(url), data);
				}
				Main.menuMode = Interface.uploadModID;
			}
			catch (WebException e)
			{
				UIModBrowser.LogModBrowserException(e);
			}
		}

		private void PublishUploadDataComplete(object s, UploadDataCompletedEventArgs e, TmodFile theTModFile)
		{
			if (e.Error != null)
			{
				if (e.Cancelled)
				{
					Main.menuMode = Interface.modSourcesID;
					return;
				}
				UIModBrowser.LogModBrowserException(e.Error);
				return;
			}
			var result = e.Result;
			int responseLength = result.Length;
			if (result.Length > 256 && result[result.Length - 256 - 1] == '~')
			{
				using (var fileStream = File.Open(theTModFile.path, FileMode.Open, FileAccess.ReadWrite))
				using (var fileReader = new BinaryReader(fileStream))
				using (var fileWriter = new BinaryWriter(fileStream))
				{
					fileReader.ReadBytes(4); // "TMOD"
					fileReader.ReadString(); // ModLoader.version.ToString()
					fileReader.ReadBytes(20); // hash
					if (fileStream.Length - fileStream.Position > 256) // Extrememly basic check in case ReadString errors?
						fileWriter.Write(result, result.Length - 256, 256);
				}
				responseLength -= 257;
			}
			string response = Encoding.UTF8.GetString(result, 0, responseLength);
			UIModBrowser.LogModPublishInfo(response);
		}

		private class PatientWebClient : WebClient
		{
			protected override WebRequest GetWebRequest(Uri uri)
			{
				HttpWebRequest w = (HttpWebRequest)base.GetWebRequest(uri);
				w.Timeout = System.Threading.Timeout.Infinite;
				w.AllowWriteStreamBuffering = false; // Should use less ram.
				return w;
			}
		}
	}
}
