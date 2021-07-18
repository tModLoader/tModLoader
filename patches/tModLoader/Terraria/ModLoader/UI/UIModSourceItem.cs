using Hjson;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.Engine;
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
		internal readonly string modName;
		private readonly Asset<Texture2D> _dividerTexture;
		private readonly UIText _modName;
		private readonly LocalMod _builtMod;
		private bool _upgradePotentialChecked;
		private Stopwatch uploadTimer;

		public UIModSourceItem(string mod, LocalMod builtMod) {
			_mod = mod;

			BorderColor = new Color(89, 116, 213) * 0.7f;
			_dividerTexture = UICommon.DividerTexture;
			Height.Pixels = 90;
			Width.Percent = 1f;
			SetPadding(6f);

			string addendum = Path.GetFileName(mod).Contains(" ") ? $"  [c/FF0000:{Language.GetTextValue("tModLoader.MSModSourcesCantHaveSpaces")}]" : "";
			modName = Path.GetFileName(mod);
			_modName = new UIText(modName + addendum) {
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
			spriteBatch.Draw(_dividerTexture.Value, drawPos, null, Color.White, 0f, Vector2.Zero, new Vector2((innerDimensions.Width - 10f) / 8f, 1f), SpriteEffects.None, 0f);

			// This code here rather than ctor since the delay for dozens of mod source folders is noticable.
			if (!_upgradePotentialChecked) {
				_upgradePotentialChecked = true;
				string modFolderName = Path.GetFileName(_mod);
				string csprojFile = Path.Combine(_mod, $"{modFolderName}.csproj");

				int leftPixels = -26;

				if (!File.Exists(csprojFile) || Interface.createMod.CsprojUpdateNeeded(File.ReadAllText(csprojFile))) {
					var icon = UICommon.ButtonExclamationTexture;
					var upgradeCSProjButton = new UIHoverImage(icon, Language.GetTextValue("tModLoader.MSUpgradeCSProj")) {
						Left = { Pixels = leftPixels, Percent = 1f },
						Top = { Pixels = 4 }
					};
					upgradeCSProjButton.OnClick += (s, e) => {
						File.WriteAllText(csprojFile, Interface.createMod.GetModCsproj(modFolderName));
						string propertiesFolder = Path.Combine(_mod, "Properties");
						string AssemblyInfoFile = Path.Combine(propertiesFolder, "AssemblyInfo.cs");
						if (File.Exists(AssemblyInfoFile))
							File.Delete(AssemblyInfoFile);

						try {
							string objFolder = Path.Combine(_mod, "obj"); // Old files can cause some issues.
							if (Directory.Exists(objFolder))
								Directory.Delete(objFolder, true);
							string binFolder = Path.Combine(_mod, "bin");
							if (Directory.Exists(binFolder))
								Directory.Delete(binFolder, true);
						}
						catch (Exception) {
						}

						Directory.CreateDirectory(propertiesFolder);
						File.WriteAllText(Path.Combine(propertiesFolder, $"launchSettings.json"), Interface.createMod.GetLaunchSettings());
						SoundEngine.PlaySound(SoundID.MenuOpen);
						Main.menuMode = Interface.modSourcesID;

						upgradeCSProjButton.Remove();
					};
					Append(upgradeCSProjButton);

					leftPixels -= 26;
				}

				// Display upgrade .lang files button if any .lang files present
				string[] files = Directory.GetFiles(_mod, "*.lang", SearchOption.AllDirectories);
				if (files.Length > 0) {
					var icon = UICommon.ButtonExclamationTexture;
					var upgradeLangFilesButton = new UIHoverImage(icon, Language.GetTextValue("tModLoader.MSUpgradeLangFiles")) {
						Left = { Pixels = leftPixels, Percent = 1f },
						Top = { Pixels = 4 }
					};
					upgradeLangFilesButton.OnClick += (s, e) => {
						foreach (var file in files) {
							UpgradeLangFile(file);
						}

						upgradeLangFilesButton.Remove();
					};
					Append(upgradeLangFilesButton);
				}
			}
		}

		private void UpgradeLangFile(String langFile) {
			string[] contents = File.ReadAllLines(langFile, Encoding.UTF8);

			JObject obj = new JObject();

			foreach (string line in contents) {
				if (line.Trim().StartsWith("#")) {
					continue;
				}

				int split = line.IndexOf('=');
				if (split < 0)
					continue; // lines witout a = are ignored
				string key = line.Substring(0, split).Trim().Replace(" ", "_");
				string value = line.Substring(split + 1); // removed .Trim() since sometimes it is desired.
				if (value.Length == 0) {
					continue;
				}
				value = value.Replace("\\n", "\n");

				string[] splitKey = key.Split(".");

				var curObj = obj;
				foreach (string k in splitKey.SkipLast(1)) {
					if (!curObj.ContainsKey(k)) {
						curObj.Add(k, new JObject());
					}

					var existingVal = curObj.GetValue(k);
					if (existingVal.Type == JTokenType.Object) {
						curObj = (JObject) existingVal;
					}
					else {
						// someone assigned a value to this key - move this value to special
						//  "$parentVal" key in newly created object
						curObj[k] = new JObject();
						curObj = (JObject) curObj.GetValue(k);
						curObj["$parentVal"] = existingVal;
					}
				}

				var lastKey = splitKey.Last();
				if (curObj.ContainsKey(splitKey.Last()) && curObj[lastKey] is JObject) {
					// this value has children - needs to go into object as a $parentValue entry
					((JObject) curObj[lastKey]).Add("$parentValue", value);
				}
				curObj.Add(splitKey.Last(), value);
			}
			
			// convert JSON to HJSON and dump to new file
			// don't delete old .lang file - let the user do this when they are happy
			string newFile = Path.ChangeExtension(langFile, "hjson");
			string hjsonContents = JsonValue.Parse(obj.ToString()).ToString(Stringify.Hjson);
			File.WriteAllText(newFile, hjsonContents);
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
			SoundEngine.PlaySound(10);
			Interface.buildMod.Build(_mod, false);
		}

		private void BuildAndReload(UIMouseEvent evt, UIElement listeningElement) {
			SoundEngine.PlaySound(10);
			Interface.buildMod.Build(_mod, true);
		}

		private void PublishMod(UIMouseEvent evt, UIElement listeningElement) {
			if (ModLoader.modBrowserPassphrase == "") {
				Main.menuMode = Interface.enterPassphraseMenuID;
				Interface.enterPassphraseMenu.SetGotoMenu(Interface.modSourcesID, Interface.modSourcesID);
				return;
			}
			SoundEngine.PlaySound(10);
			try {
				if (ModLoader.GetMod(_builtMod.Name) == null) {
					if (!_builtMod.Enabled)
						_builtMod.Enabled = true;
					Main.menuMode = Interface.reloadModsID;
					ModLoader.OnSuccessfulLoad += () => {
						PublishMod(null, null);
					};
					return;
				}

				var modFile = _builtMod.modFile;
				var bp = _builtMod.properties;

				PublishModInner(modFile, bp);
			} catch (WebException e) {
				UIModBrowser.LogModBrowserException(e);
			}
		}

		internal static void PublishModCommandLine(string modName, string passphrase, string steamid64) {
			try {
				InstallVerifier.IsGoG = true;
				ModLoader.SteamID64 = steamid64;
				ModLoader.modBrowserPassphrase = passphrase;

				if (string.IsNullOrWhiteSpace(ModLoader.modBrowserPassphrase) || string.IsNullOrWhiteSpace(ModLoader.SteamID64)) {
					throw new Exception("-passphrase and -steamid64 are required for publishing via command line");
				}

				LocalMod localMod;
				var modPath = Path.Combine(ModLoader.ModPath, modName + ".tmod");
				var modFile = new TmodFile(modPath);
				using (modFile.Open()) // savehere, -tmlsavedirectory, normal (test linux too)
					localMod = new LocalMod(modFile);

				PublishModInner(modFile, localMod.properties, true);
			}
			catch (Exception e) {
				Console.WriteLine("Something went wrong with command line mod publishing.");
				Console.WriteLine(e.ToString());
				Environment.Exit(1);
			}
			Environment.Exit(0);
		}

		private static void PublishModInner(TmodFile modFile, BuildProperties bp, bool commandLine = false) {
			var files = new List<UploadFile>();
			files.Add(new UploadFile {
				Name = "file",
				Filename = Path.GetFileName(modFile.path),
				//    ContentType = "text/plain",
				Content = File.ReadAllBytes(modFile.path)
			});
			if (modFile.HasFile("icon.png")) { // Test this on server
				using (modFile.Open())
					files.Add(new UploadFile {
						Name = "iconfile",
						Filename = "icon.png",
						Content = modFile.GetBytes("icon.png")
					});
			}
			//if (bp.beta)
			//	throw new WebException(Language.GetTextValue("tModLoader.BetaModCantPublishError"));
			if (bp.buildVersion != modFile.TModLoaderVersion)
				throw new WebException(Language.GetTextValue("OutdatedModCantPublishError.BetaModCantPublishError"));

			var values = new NameValueCollection
			{
					{ "displayname", bp.displayName },
					{ "displaynameclean", string.Join("", ChatManager.ParseMessage(bp.displayName, Color.White).Where(x=> x.GetType() == typeof(TextSnippet)).Select(x => x.Text)) },
					{ "name", modFile.Name },
					{ "version", "v"+bp.version },
					{ "author", bp.author },
					{ "homepage", bp.homepage },
					{ "description", bp.description },
					{ "steamid64", ModLoader.SteamID64 },
					{ "modloaderversion", "tModLoader v"+modFile.TModLoaderVersion },
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
				Interface.progress.Show(displayText: $"Uploading: {modFile.Name}", gotoMenu: Interface.modSourcesID, cancel: client.CancelAsync);

				var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x", System.Globalization.NumberFormatInfo.InvariantInfo);
				client.Headers["Content-Type"] = "multipart/form-data; boundary=" + boundary;
				//boundary = "--" + boundary;
				byte[] data = UploadFile.GetUploadFilesRequestData(files, values, boundary);
				if (commandLine) {
					var result = client.UploadData(new Uri(url), data); // could use async version for progress output maybe
					string response = HandlePublishResponse(modFile, result);
					Console.WriteLine(Language.GetTextValue("tModLoader.MBServerResponse", response));
					if (result.Length <= 256 || result[result.Length - 256 - 1] != '~') {
						throw new Exception("Publish failed due to invalid response from server");
					}
				}
				else {
					client.UploadDataCompleted += (s, e) => PublishUploadDataComplete(s, e, modFile);
					client.UploadProgressChanged += (s, e) => Interface.progress.Progress = (float)e.BytesSent / e.TotalBytesToSend;
					client.UploadDataAsync(new Uri(url), data);
				}
			}
		}

		private static void PublishUploadDataComplete(object s, UploadDataCompletedEventArgs e, TmodFile theTModFile) {
			if (e.Error != null) {
				if (e.Cancelled) {
					Main.menuMode = Interface.modSourcesID;
					return;
				}
				UIModBrowser.LogModBrowserException(e.Error);
				return;
			}

			if (ModLoader.TryGetMod(theTModFile.Name, out var mod))
				mod.Close();

			var result = e.Result;
			string response = HandlePublishResponse(theTModFile, result);
			UIModBrowser.LogModPublishInfo(response);
		}

		private static string HandlePublishResponse(TmodFile theTModFile, byte[] result) {
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
			return response;
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
