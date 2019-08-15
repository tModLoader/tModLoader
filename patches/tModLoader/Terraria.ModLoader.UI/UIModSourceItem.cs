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
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.UI.ModBrowser;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Terraria.ModLoader.UI
{
	//TODO common 'Item' code
	internal class UIModSourceItem : UIPanel
	{
		private readonly string _mod;
		private readonly Texture2D _dividerTexture;
		private readonly UIText _modName;
		private readonly LocalMod _builtMod;
		private bool _upgradePotentialChecked;

		public UIModSourceItem(string mod, LocalMod builtMod) {
			_mod = mod;

			BorderColor = new Color(89, 116, 213) * 0.7f;
			_dividerTexture = UICommon.DividerTexture;
			Height.Pixels = 90;
			Width.Percent = 1f;
			SetPadding(6f);

			string addendum = Path.GetFileName(mod).Contains(" ") ? $"  [c/FF0000:{Language.GetTextValue("tModLoader.MSModSourcesCantHaveSpaces")}]" : "";
			_modName = new UIText(Path.GetFileName(mod) + addendum) {
				Left = { Pixels = 10 },
				Top = { Pixels = 5 }
			};
			Append(_modName);

			var buildButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MSBuild")) {
				Width = { Pixels = 100 },
				Height = { Pixels = 36 },
				Left = { Pixels = 10 },
				Top = { Pixels = 40 }
			}.WithFadedMouseOver();
			buildButton.PaddingTop -= 2f;
			buildButton.PaddingBottom -= 2f;
			buildButton.OnClick += BuildMod;
			Append(buildButton);

			var buildReloadButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MSBuildReload"));
			buildReloadButton.CopyStyle(buildButton);
			buildReloadButton.Width.Pixels = 200;
			buildReloadButton.Left.Pixels = 150;
			buildReloadButton.WithFadedMouseOver();
			buildReloadButton.OnClick += BuildAndReload;
			Append(buildReloadButton);

			_builtMod = builtMod;
			if (builtMod != null) {
				var publishButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MSPublish"));
				publishButton.CopyStyle(buildReloadButton);
				publishButton.Width.Pixels = 100;
				publishButton.Left.Pixels = 390;
				publishButton.WithFadedMouseOver();
				publishButton.OnClick += PublishMod;
				Append(publishButton);
			}
			OnDoubleClick += BuildAndReload;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);
			CalculatedStyle innerDimensions = GetInnerDimensions();
			Vector2 drawPos = new Vector2(innerDimensions.X + 5f, innerDimensions.Y + 30f);
			spriteBatch.Draw(_dividerTexture, drawPos, null, Color.White, 0f, Vector2.Zero, new Vector2((innerDimensions.Width - 10f) / 8f, 1f), SpriteEffects.None, 0f);

			// This code here rather than ctor since the delay for dozens of mod source folders is noticable.
			if (!_upgradePotentialChecked) {
				_upgradePotentialChecked = true;
				string modFolderName = Path.GetFileName(_mod);
				string csprojFile = Path.Combine(_mod, $"{modFolderName}.csproj");
				if (!File.Exists(csprojFile) || !File.ReadAllText(csprojFile).Contains("tModLoader.targets")) {
					var icon = UICommon.ButtonExclamationTexture;
					var upgradeCSProjButton = new UIHoverImage(icon, Language.GetTextValue("tModLoader.MSUpgradeCSProj")) {
						Left = { Pixels = -26, Percent = 1f },
						Top = { Pixels = 4 }
					};
					upgradeCSProjButton.OnClick += (s, e) => {
						File.WriteAllText(csprojFile, Interface.createMod.GetModCsproj(modFolderName));
						string propertiesFolder = Path.Combine(_mod, "Properties");
						string AssemblyInfoFile = Path.Combine(propertiesFolder, "AssemblyInfo.cs");
						if (File.Exists(AssemblyInfoFile))
							File.Delete(AssemblyInfoFile);
						Directory.CreateDirectory(propertiesFolder);
						File.WriteAllText(Path.Combine(propertiesFolder, $"launchSettings.json"), Interface.createMod.GetLaunchSettings());
						Main.PlaySound(SoundID.MenuOpen);
						Main.menuMode = Interface.modSourcesID;
					};
					Append(upgradeCSProjButton);
				}
			}
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

		public override int CompareTo(object obj) {
			UIModSourceItem uIModSourceItem = obj as UIModSourceItem;
			if (uIModSourceItem == null) {
				return base.CompareTo(obj);
			}
			if (uIModSourceItem._builtMod == null && _builtMod == null)
				return _modName.Text.CompareTo(uIModSourceItem._modName.Text);
			if (uIModSourceItem._builtMod == null)
				return -1;
			if (_builtMod == null)
				return 1;
			return uIModSourceItem._builtMod.lastModified.CompareTo(_builtMod.lastModified);
		}

		private void BuildMod(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10);
			Interface.buildMod.Build(_mod, false);
		}

		private void BuildAndReload(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(10);
			Interface.buildMod.Build(_mod, true);
		}

		private void PublishMod(UIMouseEvent evt, UIElement listeningElement) {
			if (ModLoader.modBrowserPassphrase == "") {
				Main.menuMode = Interface.enterPassphraseMenuID;
				Interface.enterPassphraseMenu.SetGotoMenu(Interface.modSourcesID);
				return;
			}
			Main.PlaySound(10);
			try {
				var modFile = _builtMod.modFile;
				var bp = _builtMod.properties;

				var files = new List<UploadFile>();
				files.Add(new UploadFile {
					Name = "file",
					Filename = Path.GetFileName(modFile.path),
					//    ContentType = "text/plain",
					Content = File.ReadAllBytes(modFile.path)
				});
				if (modFile.HasFile("icon.png")) {
					using (modFile.Open())
						files.Add(new UploadFile {
							Name = "iconfile",
							Filename = "icon.png",
							Content = modFile.GetBytes("icon.png")
						});
				}
				if (bp.beta)
					throw new WebException(Language.GetTextValue("tModLoader.BetaModCantPublishError"));
				if (bp.buildVersion != modFile.tModLoaderVersion)
					throw new WebException(Language.GetTextValue("OutdatedModCantPublishError.BetaModCantPublishError"));

				var values = new NameValueCollection
				{
					{ "displayname", bp.displayName },
					{ "displaynameclean", string.Join("", ChatManager.ParseMessage(bp.displayName, Color.White).Where(x=> x.GetType() == typeof(TextSnippet)).Select(x => x.Text)) },
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
				using (PatientWebClient client = new PatientWebClient()) {
					ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, policyErrors) => true;
					Interface.uploadModProgress.SetDownloading(modFile.name);
					Interface.uploadModProgress.SetCancel(() => {
						client.CancelAsync();
					});
					client.UploadProgressChanged += (s, e) => Interface.uploadModProgress.SetProgress(e);
					client.UploadDataCompleted += (s, e) => PublishUploadDataComplete(s, e, modFile);

					var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x", System.Globalization.NumberFormatInfo.InvariantInfo);
					client.Headers["Content-Type"] = "multipart/form-data; boundary=" + boundary;
					//boundary = "--" + boundary;
					byte[] data = UploadFile.GetUploadFilesRequestData(files, values);
					client.UploadDataAsync(new Uri(url), data);
				}
				Main.menuMode = Interface.uploadModProgressID;
			}
			catch (WebException e) {
				UIModBrowser.LogModBrowserException(e);
			}
		}

		private void PublishUploadDataComplete(object s, UploadDataCompletedEventArgs e, TmodFile theTModFile) {
			if (e.Error != null) {
				if (e.Cancelled) {
					Main.menuMode = Interface.modSourcesID;
					return;
				}
				UIModBrowser.LogModBrowserException(e.Error);
				return;
			}
			ModLoader.GetMod(theTModFile.name)?.Close();
			var result = e.Result;
			int responseLength = result.Length;
			if (result.Length > 256 && result[result.Length - 256 - 1] == '~') {
				using (var fileStream = File.Open(theTModFile.path, FileMode.Open, FileAccess.ReadWrite))
				using (var fileReader = new BinaryReader(fileStream))
				using (var fileWriter = new BinaryWriter(fileStream)) {
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
			protected override WebRequest GetWebRequest(Uri uri) {
				HttpWebRequest w = (HttpWebRequest)base.GetWebRequest(uri);
				w.Timeout = System.Threading.Timeout.Infinite;
				w.AllowWriteStreamBuffering = false; // Should use less ram.
				return w;
			}
		}
	}
}
