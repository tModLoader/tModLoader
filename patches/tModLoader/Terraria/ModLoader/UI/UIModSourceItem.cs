using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.OS;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI.ModBrowser;
using Terraria.Social.Steam;
using Terraria.UI;

namespace Terraria.ModLoader.UI;

//TODO common 'Item' code
internal class UIModSourceItem : UIPanel
{
	private readonly string _mod;
	internal readonly string modName;
	private readonly Asset<Texture2D> _dividerTexture;
	private readonly UIText _modName;
	private readonly UIText _lastBuildTime;
	private readonly UIAutoScaleTextTextPanel<string> needRebuildButton;
	private readonly LocalMod _builtMod;
	private bool _upgradePotentialChecked;
	private Stopwatch uploadTimer;
	private int contextButtonsLeft = -26;

	public UIModSourceItem(string mod, LocalMod builtMod)
	{
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

		if (builtMod != null) {
			string lastBuildTimeMessage = TimeHelper.HumanTimeSpanString(builtMod.lastModified, localTime: true);
			_lastBuildTime = new UIText(lastBuildTimeMessage) {
				HAlign = 1f,
				Left = { Pixels = -110 }, // There are potentially 4 buttons that might appear
				Top = { Pixels = 5 }
			};
			var ts = new TimeSpan(DateTime.Now.Ticks - builtMod.lastModified.Ticks);
			double delta = Math.Abs(ts.TotalSeconds);
			_lastBuildTime.TextColor = delta switch {
				< 5 * 60 => Color.Green, // 5 min
				< 60 * 60 => Color.Yellow, // 1 hour
				< 30 * 24 * 60 * 60 => Color.Orange, // 1 month
				_ => Color.Red,
			};

			Append(_lastBuildTime);
		}

		var buildButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MSBuild")) {
			Width = { Pixels = 100 },
			Height = { Pixels = 36 },
			Left = { Pixels = 10 },
			Top = { Pixels = 40 }
		}.WithFadedMouseOver();
		buildButton.PaddingTop -= 2f;
		buildButton.PaddingBottom -= 2f;
		buildButton.OnLeftClick += BuildMod;
		Append(buildButton);

		var buildReloadButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MSBuildReload"));
		buildReloadButton.CopyStyle(buildButton);
		buildReloadButton.Width.Pixels = 200;
		buildReloadButton.Left.Pixels = 150;
		buildReloadButton.WithFadedMouseOver();
		buildReloadButton.OnLeftClick += BuildAndReload;
		Append(buildReloadButton);

		_builtMod = builtMod;
		if (builtMod != null && LocalizationLoader.changedMods.Contains(modName)) {
			needRebuildButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MSRebuildRequired"));
			needRebuildButton.CopyStyle(buildReloadButton);
			needRebuildButton.Width.Pixels = 180;
			needRebuildButton.Left.Pixels = 360;
			needRebuildButton.BackgroundColor = Color.Red;
			Append(needRebuildButton);
		}
		else if (builtMod != null) {
			var publishButton = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MSPublish"));
			publishButton.CopyStyle(buildReloadButton);
			publishButton.Width.Pixels = 100;
			publishButton.Left.Pixels = 390;
			publishButton.WithFadedMouseOver();

			if (builtMod.properties.side == ModSide.Server) {
				publishButton.OnLeftClick += PublishServerSideMod;
				Append(publishButton);
			}
			else if (builtMod.Enabled) {
				publishButton.OnLeftClick += PublishMod;
				Append(publishButton);
			}
		}

		OnLeftDoubleClick += BuildAndReload;

		string modFolderName = Path.GetFileName(_mod);
		string csprojFile = Path.Combine(_mod, $"{modFolderName}.csproj");
		if (File.Exists(csprojFile)) {
			var openCSProjButton = new UIHoverImage(UICommon.CopyCodeButtonTexture, Language.GetTextValue("tModLoader.MSOpenCSProj")) {
				Left = { Pixels = contextButtonsLeft, Percent = 1f },
				Top = { Pixels = 4 }
			};
			openCSProjButton.OnLeftClick += (a, b) => {
				// Due to "DOTNET_ROLL_FORWARD=Disable" environment variable being set for normal game launches, launching the .csproj directly results in a prompt to install .net 6.0.0 because of the inherited environment variables. This works around that for Windows users.
				if (Platform.IsWindows) {
					Process.Start(
						new ProcessStartInfo("explorer", csprojFile) {
							UseShellExecute = true
						}
					);
				}
				else {
					Process.Start(
						new ProcessStartInfo(csprojFile) {
							UseShellExecute = true
						}
					);
				}
			};
			Append(openCSProjButton);

			contextButtonsLeft -= 26;
		}
	}

	protected override void DrawChildren(SpriteBatch spriteBatch)
	{
		base.DrawChildren(spriteBatch);
		if (needRebuildButton?.IsMouseHovering == true) {
			UICommon.DrawHoverStringInBounds(spriteBatch, Language.GetTextValue("tModLoader.MSLocalizationFilesChangedCantPublish"), GetOuterDimensions().ToRectangle());
		}
		if (_lastBuildTime?.IsMouseHovering == true) {
			UICommon.DrawHoverStringInBounds(spriteBatch, Language.GetTextValue("tModLoader.MSLastBuilt", TimeHelper.HumanTimeSpanString(_builtMod.lastModified, localTime: true)), GetOuterDimensions().ToRectangle());
		}
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);
		CalculatedStyle innerDimensions = GetInnerDimensions();
		Vector2 drawPos = new Vector2(innerDimensions.X + 5f, innerDimensions.Y + 30f);
		spriteBatch.Draw(_dividerTexture.Value, drawPos, null, Color.White, 0f, Vector2.Zero, new Vector2((innerDimensions.Width - 10f) / 8f, 1f), SpriteEffects.None, 0f);

		// This code here rather than ctor since the delay for dozens of mod source folders is noticeable.
		if (!_upgradePotentialChecked) {
			_upgradePotentialChecked = true;
			string modFolderName = Path.GetFileName(_mod);
			string csprojFile = Path.Combine(_mod, $"{modFolderName}.csproj");

			bool projNeedsUpdate = false;
			if (!File.Exists(csprojFile) || Interface.createMod.CsprojUpdateNeeded(File.ReadAllText(csprojFile))) {
				var icon = UICommon.ButtonExclamationTexture;
				var upgradeCSProjButton = new UIHoverImage(icon, Language.GetTextValue("tModLoader.MSUpgradeCSProj")) {
					Left = { Pixels = contextButtonsLeft, Percent = 1f },
					Top = { Pixels = 4 }
				};
				upgradeCSProjButton.OnLeftClick += (s, e) => {
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
					_upgradePotentialChecked = false;
				};
				Append(upgradeCSProjButton);

				contextButtonsLeft -= 26;
				projNeedsUpdate = true;
			}

			// Display upgrade .lang files button if any .lang files present
			//TODO: Make this asynchronous, as this can be quite expensive
			string[] files = Directory.GetFiles(_mod, "*.lang", SearchOption.AllDirectories);

			if (files.Length > 0) {
				var icon = UICommon.ButtonExclamationTexture;
				var upgradeLangFilesButton = new UIHoverImage(icon, Language.GetTextValue("tModLoader.MSUpgradeLangFiles")) {
					Left = { Pixels = contextButtonsLeft, Percent = 1f },
					Top = { Pixels = 4 }
				};

				upgradeLangFilesButton.OnLeftClick += (s, e) => {
					foreach (string file in files) {
						LocalizationLoader.UpgradeLangFile(file, modName);
					}

					upgradeLangFilesButton.Remove();
				};

				Append(upgradeLangFilesButton);

				contextButtonsLeft -= 26;
			}


			// Display Run tModPorter when .csproj is valid
			if (!projNeedsUpdate) {
				var pIcon = UICommon.ButtonExclamationTexture;
				var portModButton = new UIHoverImage(pIcon, Language.GetTextValue("tModLoader.MSPortToLatest")) {
					Left = { Pixels = contextButtonsLeft, Percent = 1f },
					Top = { Pixels = 4 }
				};

				portModButton.OnLeftClick += (s, e) => {
					string modFolderName = Path.GetFileName(_mod);
					string csprojFile = Path.Combine(_mod, $"{modFolderName}.csproj");

					string args = $"\"{csprojFile}\"";
					var tMLPath = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
					// TODO: We need to find some way of launching Linux scripts in a console window that is shown, not hidden. Probably requires logic to call "gnome-terminal/xterm/konsole -e command" depending on whatever is installed (https://askubuntu.com/a/46630)
					var porterPath =  Path.Combine(Path.GetDirectoryName(tMLPath), "tModPorter", (Platform.IsWindows ? "tModPorter.bat" : "tModPorter.sh"));

					var porterInfo = new ProcessStartInfo() {
						FileName = porterPath,
						Arguments = args,
						UseShellExecute = true
					};

					var porter = Process.Start(porterInfo);
				};

				Append(portModButton);

				contextButtonsLeft -= 26;
			}
		}
	}

	public override void MouseOver(UIMouseEvent evt)
	{
		base.MouseOver(evt);
		BackgroundColor = UICommon.DefaultUIBlue;
		BorderColor = new Color(89, 116, 213);
	}

	public override void MouseOut(UIMouseEvent evt)
	{
		base.MouseOut(evt);
		BackgroundColor = new Color(63, 82, 151) * 0.7f;
		BorderColor = new Color(89, 116, 213) * 0.7f;
	}

	public override int CompareTo(object obj)
	{
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

	private void BuildMod(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(10);
		Interface.buildMod.Build(_mod, false);
	}

	private void BuildAndReload(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(10);
		Interface.buildMod.Build(_mod, true);
	}

	private void PublishMod(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(10);
		try {
			if (!SteamedWraps.SteamClient) {
				Utils.ShowFancyErrorMessage(Language.GetTextValue("tModLoader.SteamPublishingLimit"), Interface.modSourcesID);
				return;
			}

			if (!ModLoader.TryGetMod(_builtMod.Name, out _)) {
				if (!_builtMod.Enabled)
					_builtMod.Enabled = true;
				Main.menuMode = Interface.reloadModsID;
				ModLoader.OnSuccessfulLoad += () => {
					Main.QueueMainThreadAction(() => {
						// Delay publishing to when the mod is completely reloaded in main thread
						PublishMod(null, null);
					});
				};
				return;
			}

			string icon = Path.Combine(_mod, "icon_workshop.png");
			if (!File.Exists(icon))
				icon = Path.Combine(_mod, "icon.png");

			WorkshopHelper.PublishMod(_builtMod, icon);
		}
		catch (WebException e) {
			UIModBrowser.LogModBrowserException(e, Interface.modSourcesID);
		}
	}

	private void PublishServerSideMod(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(10);
		try {
			if (!SteamedWraps.SteamClient) {
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
			UIModBrowser.LogModBrowserException(e, Interface.modSourcesID);
		}
	}

	internal static void PublishModCommandLine(string modName)
	{
		try {
			LocalMod localMod;
			var modPath = Path.Combine(ModLoader.ModPath, modName + ".tmod");
			var modFile = new TmodFile(modPath);
			using (modFile.Open()) // savehere, -tmlsavedirectory, normal (test linux too)
				localMod = new LocalMod(modFile);

			string icon = Path.Combine(ModCompile.ModSourcePath, modName, "icon_workshop.png");
			if (!File.Exists(icon))
				icon = Path.Combine(ModCompile.ModSourcePath, modName, "icon.png");

			WorkshopHelper.PublishMod(localMod, icon);
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
}
