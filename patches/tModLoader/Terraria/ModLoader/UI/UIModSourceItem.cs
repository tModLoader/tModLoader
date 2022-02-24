using Hjson;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using ReLogic.Content;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI.ModBrowser;
using Terraria.Social;
using Terraria.Social.Base;
using Terraria.Social.Steam;
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

				if (builtMod.properties.side == ModSide.Server) {
					publishButton.OnClick += PublishServerSideMod;
					Append(publishButton);
				}
				else if (builtMod.Enabled) {
					publishButton.OnClick += PublishMod;
					Append(publishButton);
				}
			}

			OnDoubleClick += BuildAndReload;

			string modFolderName = Path.GetFileName(_mod);
			string csprojFile = Path.Combine(_mod, $"{modFolderName}.csproj");
			if (File.Exists(csprojFile)) {
				var openCSProjButton = new UIHoverImage(UICommon.CopyCodeButtonTexture, "Open .csproj") {
					Left = { Pixels = -52, Percent = 1f },
					Top = { Pixels = 4 }
				};
				openCSProjButton.OnClick += (a, b) => {
					Process.Start(
						new ProcessStartInfo(csprojFile) {
							UseShellExecute = true
						}
					);
				};
				Append(openCSProjButton);
			}
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
				//TODO: Make this asynchronous, as this can be quite expensive
				string[] files = Directory.GetFiles(_mod, "*.lang", SearchOption.AllDirectories);

				if (files.Length > 0) {
					var icon = UICommon.ButtonExclamationTexture;
					var upgradeLangFilesButton = new UIHoverImage(icon, Language.GetTextValue("tModLoader.MSUpgradeLangFiles")) {
						Left = { Pixels = leftPixels, Percent = 1f },
						Top = { Pixels = 4 }
					};

					upgradeLangFilesButton.OnClick += (s, e) => {
						foreach (string file in files) {
							LocalizationLoader.UpgradeLangFile(file, modName);
						}

						upgradeLangFilesButton.Remove();
					};

					Append(upgradeLangFilesButton);
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
			SoundEngine.PlaySound(10);
			Interface.buildMod.Build(_mod, false);
		}

		private void BuildAndReload(UIMouseEvent evt, UIElement listeningElement) {
			SoundEngine.PlaySound(10);
			Interface.buildMod.Build(_mod, true);
		}

		private void PublishMod(UIMouseEvent evt, UIElement listeningElement) {
			SoundEngine.PlaySound(10);
			try {
				if (!WorkshopHelper.ModManager.SteamUser) {
					Utils.ShowFancyErrorMessage(Language.GetTextValue("tModLoader.SteamPublishingLimit"), Interface.modSourcesID);
					return;
				}

				if (!ModLoader.TryGetMod(_builtMod.Name, out _)) {
					if (!_builtMod.Enabled)
						_builtMod.Enabled = true;
					Main.menuMode = Interface.reloadModsID;
					ModLoader.OnSuccessfulLoad += () => {
						Main.QueueMainThreadAction(() => {
							PublishMod(null, null);
						});
					};
					return;
				}

				var modFile = _builtMod.modFile;
				var bp = _builtMod.properties;

				PublishModInner(modFile, bp, Path.Combine(_mod, "icon.png"));
			}
			catch (WebException e) {
				UIModBrowser.LogModBrowserException(e);
			}
		}

		private void PublishServerSideMod(UIMouseEvent evt, UIElement listeningElement) {
			SoundEngine.PlaySound(10);
			try {
				if (!WorkshopHelper.ModManager.SteamUser) {
					Utils.ShowFancyErrorMessage(Language.GetTextValue("tModLoader.SteamPublishingLimit"), Interface.modSourcesID);
					return;
				}
				var p = new ProcessStartInfo() {
					UseShellExecute = true,
					FileName = Process.GetCurrentProcess().MainModule.FileName,
					Arguments = "tModLoader.dll -server -steam -publish " + _builtMod.modFile.path.Remove(_builtMod.modFile.path.LastIndexOf(".tmod"))
				};

				var pending = Process.Start(p);
				pending.WaitForExit();
			}
			catch (WebException e) {
				UIModBrowser.LogModBrowserException(e);
			}
		}


		internal static void PublishModCommandLine(string modName) {
			try {
				LocalMod localMod;
				var modPath = Path.Combine(ModLoader.ModPath, modName + ".tmod");
				var modFile = new TmodFile(modPath);
				using (modFile.Open()) // savehere, -tmlsavedirectory, normal (test linux too)
					localMod = new LocalMod(modFile);

				PublishModInner(modFile, localMod.properties, Path.Combine(ModCompile.ModSourcePath, modName, "icon.png"), true);
			}
			catch (Exception e) {
				Console.WriteLine("Something went wrong with command line mod publishing.");
				Console.WriteLine(e.ToString());
				Steamworks.SteamAPI.Shutdown();
				Environment.Exit(1);
			}
			Console.WriteLine("exiting ");
			Steamworks.SteamAPI.Shutdown();
			Environment.Exit(0);
		}

		private static void PublishModInner(TmodFile modFile, BuildProperties bp, string iconPath, bool commandLine = false) {
			if (bp.buildVersion != modFile.TModLoaderVersion)
				throw new WebException(Language.GetTextValue("OutdatedModCantPublishError.BetaModCantPublishError"));

			var values = new NameValueCollection
			{
				{ "displayname", bp.displayName },
				{ "displaynameclean", string.Join("", ChatManager.ParseMessage(bp.displayName, Color.White).Where(x => x.GetType() == typeof(TextSnippet)).Select(x => x.Text)) },
				{ "name", modFile.Name },
				{ "version", $"v{bp.version}" },
				{ "author", bp.author },
				{ "homepage", bp.homepage },
				{ "description", bp.description },
				{ "iconpath", iconPath },
				{ "manifestfolder", Path.Combine(ModCompile.ModSourcePath, modFile.Name) },
				{ "modloaderversion", $"tModLoader v{modFile.TModLoaderVersion}" },
				{ "modreferences", string.Join(", ", bp.modReferences.Select(x => x.mod)) },
				{ "modside", bp.side.ToFriendlyString() },
			};

			if (string.IsNullOrWhiteSpace(values["author"]))
				throw new WebException($"You need to specify an author in build.txt");

			if (string.IsNullOrWhiteSpace(values["version"]))
				throw new WebException($"You need to specify a version in build.txt");

			if (!Main.dedServ) {
				Main.MenuUI.SetState(new WorkshopPublishInfoStateForMods(Interface.modSources, modFile, values));
			}
			else {
				SocialAPI.LoadSteam();

				if ( /*SocialAPI.Workshop != null && */ modFile != null) {
					var publishSetttings = new WorkshopItemPublishSettings {
						Publicity = WorkshopItemPublicSettingId.Public,
						UsedTags = Array.Empty<WorkshopTagOption>(),
						PreviewImagePath = iconPath
					};
					WorkshopHelper.ModManager.SteamUser = true;
					SocialAPI.Workshop.PublishMod(modFile, values, publishSetttings);
				}
			}
		}
	}
}
