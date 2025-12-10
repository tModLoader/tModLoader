using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.OS;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Initializers;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI.ModBrowser;
using Terraria.Social.Base;
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
	private Task<string[]> langFileTask;
	private Task<bool> sourceUpgradeTask;
	private CancellationToken _modSourcesToken;

	public UIModSourceItem(string mod, LocalMod builtMod, CancellationToken modSourcesToken)
	{
		_mod = mod;
		_modSourcesToken = modSourcesToken;

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
				RemoveFloatingPointsFromDrawPosition = true,
				UseTooltipMouseText = true,
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

		if (File.Exists(csprojFile)) {
			bool customModSourceFolder = builtMod != null && !string.IsNullOrWhiteSpace(builtMod.properties.modSource) && builtMod.properties.modSource != Path.Combine(ModCompile.ModSourcePath, modName);
			var openFolderButton = new UIHoverImage(customModSourceFolder ? UICommon.ButtonOpenFolderCustom : UICommon.ButtonOpenFolder, customModSourceFolder ? Language.GetTextValue("tModLoader.MSOpenCustomSourceFolder", builtMod.properties.modSource) : Lang.inter[110].Value) {
				RemoveFloatingPointsFromDrawPosition = true,
				UseTooltipMouseText = true,
				Left = { Pixels = contextButtonsLeft, Percent = 1f },
				Top = { Pixels = 4 }
			};
			openFolderButton.OnLeftClick += (a, b) => Utils.OpenFolder(_mod);
			Append(openFolderButton);
			contextButtonsLeft -= 26;
		}
	}

	protected override void DrawChildren(SpriteBatch spriteBatch)
	{
		base.DrawChildren(spriteBatch);
		if (needRebuildButton?.IsMouseHovering == true) {
			UICommon.TooltipMouseText(Language.GetTextValue("tModLoader.MSLocalizationFilesChangedCantPublish"));
		}
		if (_lastBuildTime?.IsMouseHovering == true) {
			UICommon.TooltipMouseText(Language.GetTextValue("tModLoader.MSLastBuilt", TimeHelper.HumanTimeSpanString(_builtMod.lastModified, localTime: true)));
		}
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);
		CalculatedStyle innerDimensions = GetInnerDimensions();
		Vector2 drawPos = new Vector2(innerDimensions.X + 5f, innerDimensions.Y + 30f);
		spriteBatch.Draw(_dividerTexture.Value, drawPos, null, Color.White, 0f, Vector2.Zero, new Vector2((innerDimensions.Width - 10f) / 8f, 1f), SpriteEffects.None, 0f);

		if (!_upgradePotentialChecked) {
			_upgradePotentialChecked = true;

			StartUpgradeTasks();
		}

		// Display upgrade .lang files button if any .lang files present
		if (langFileTask is { IsCompleted: true }) {
			string[] result = langFileTask.Result;

			if (result.Length > 0) {
				AddLangFileUpgradeButton(result);
			}

			langFileTask = null;
		}

		// Display Run tModPorter when .csproj is valid
		if (sourceUpgradeTask is { IsCompleted: true }) {
			try {
				bool result = sourceUpgradeTask.GetAwaiter().GetResult();

				// Source upgrade needed.
				if (result) {
					AddCsProjUpgradeButton();
				}
				else {
					AddModPorterButton();
				}
			}
			catch (Exception e) {
				AddErrorButton(e);
			}
			finally {
				sourceUpgradeTask = null;
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

			// Test loading the mod under `Main.dedServ = true` conditions
			var p = new ProcessStartInfo() {
				UseShellExecute = true,
				FileName = Process.GetCurrentProcess().MainModule.FileName,
				Arguments = "tModLoader.dll -server -steam -testservermodloading " + _builtMod.modFile.Name
			};

			if (Program.LaunchParameters.TryGetValue("-tmlsavedirectory", out var tmlsavedirectory))
				p.Arguments += $@" -tmlsavedirectory ""{tmlsavedirectory}""";
			else if (Program.LaunchParameters.TryGetValue("-savedirectory", out var savedirectory))
				p.Arguments += $@" -savedirectory ""{savedirectory}""";

			var pending = Process.Start(p);
			pending.WaitForExit();
			int result = pending.ExitCode;
			if (result == 0) {
				string icon = Path.Combine(_mod, "icon_workshop.png");

				if (!File.Exists(icon))
					icon = Path.Combine(_mod, "icon.png");

				WorkshopHelper.PublishMod(_builtMod, icon);
			}
			else {
				Utils.ShowFancyErrorMessage(Language.GetTextValue("tModLoader.LoadError", _builtMod), Interface.modSourcesID);
				return;
			}
		}
		catch (WebException e) {
			UIModBrowser.LogModBrowserException(e, Interface.modSourcesID);
		}
	}

	internal static void TestServerModLoading(string modName)
	{
		// Create a new console for clarity
		Platform.Get<IWindowService>().ReleaseConsole();
		Platform.Get<IWindowService>().CreateAndRedirectConsole();
		Console.WriteLine("------------------------------------------------------------------");
		Console.WriteLine(Language.GetTextValue("tModLoader.TestServerModLoadingNotification"));
		Console.WriteLine("------------------------------------------------------------------");

		try {
			ModLoader.preparingServerSidePublish = true;
			LocalMod _builtMod;
			var modPath = Path.Combine(ModLoader.ModPath, modName + ".tmod");
			Console.WriteLine("Testing mod found at modPath: " + modPath);
			var modFile = new TmodFile(modPath);
			using (modFile.Open())
				_builtMod = new LocalMod(ModLocation.Local, modFile);

			string icon = Path.Combine(ModCompile.ModSourcePath, modName, "icon_workshop.png");
			if (!File.Exists(icon))
				icon = Path.Combine(ModCompile.ModSourcePath, modName, "icon.png");

			ModLoader.EnabledMods.Add(modName);
		}
		catch (Exception e) {
			Console.WriteLine("Something went wrong with command line mod publishing.");
			Console.WriteLine(e.ToString());
			Steamworks.SteamAPI.Shutdown();
			Environment.Exit(1);
		}
	}

	internal static void PublishModCommandLine(string modName)
	{
		try {
			var publishTags = LaunchInitializer.TryParameter("-publishtags")?.Split("&");

			WorkshopItemPublicSettingId? publicity = null;
			if (LaunchInitializer.TryParameter("-publicity") is string publicityString)
				if (int.TryParse(publicityString, out int publicityInt) && publicityInt >= 0 && publicityInt <= 3)
					publicity = (WorkshopItemPublicSettingId)publicityInt;

			LocalMod localMod;
			var modPath = Path.Combine(ModLoader.ModPath, modName + ".tmod");
			var modFile = new TmodFile(modPath);
			using (modFile.Open())
				localMod = new LocalMod(ModLocation.Local, modFile);

			string icon = Path.Combine(localMod.properties.modSource, "icon_workshop.png");
			if (!File.Exists(icon))
				icon = Path.Combine(localMod.properties.modSource, "icon.png");

			WorkshopHelper.PublishMod(localMod, icon, publishTags, publicity);
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

	private void StartUpgradeTasks()
	{
		langFileTask = Task.Run(
			() => Directory.GetFiles(_mod, "*.lang", SearchOption.AllDirectories),
			_modSourcesToken
		);

		sourceUpgradeTask = Task.Run(
			() => SourceManagement.SourceUpgradeNeeded(_mod),
			_modSourcesToken
		);
	}

	private void AddLangFileUpgradeButton(string[] result)
	{
		var icon = UICommon.ButtonUpgradeLang;
		var upgradeLangFilesButton = new UIHoverImage(icon, Language.GetTextValue("tModLoader.MSUpgradeLangFiles")) {
			RemoveFloatingPointsFromDrawPosition = true,
			UseTooltipMouseText = true,
			Left = { Pixels = contextButtonsLeft, Percent = 1f },
			Top = { Pixels = 4 }
		};

		upgradeLangFilesButton.OnLeftClick += (s, e) => {
			foreach (string file in result) {
				LocalizationLoader.UpgradeLangFile(file, modName);
			}

			upgradeLangFilesButton.Remove();

			SoundEngine.PlaySound(SoundID.MenuTick);
		};

		Append(upgradeLangFilesButton);

		contextButtonsLeft -= 26;
	}

	private void AddCsProjUpgradeButton()
	{
		var icon = UICommon.ButtonUpgradeCsproj;
		var upgradeCSProjButton = new UIHoverImage(icon, Language.GetTextValue("tModLoader.MSUpgradeCSProj")) {
			RemoveFloatingPointsFromDrawPosition = true,
			UseTooltipMouseText = true,
			Left = { Pixels = contextButtonsLeft, Percent = 1f },
			Top = { Pixels = 4 }
		};

		upgradeCSProjButton.OnLeftClick += (s, e) => {
			SourceManagement.UpgradeSource(_mod);

			SoundEngine.PlaySound(SoundID.MenuOpen);
			Main.menuMode = Interface.modSourcesID;

			upgradeCSProjButton.Remove();

			// When this button is pressed, the csproj no longer requires an upgrade. This means that the tModPorter button should now be added.
			AddModPorterButton();
		};

		Append(upgradeCSProjButton);

		contextButtonsLeft -= 26;
	}

	private void AddModPorterButton()
	{
		var pIcon = UICommon.ButtonRunTModPorter;
		var portModButton = new UIHoverImage(pIcon, Language.GetTextValue("tModLoader.MSPortToLatest")) {
			RemoveFloatingPointsFromDrawPosition = true,
			UseTooltipMouseText = true,
			Left = { Pixels = contextButtonsLeft, Percent = 1f },
			Top = { Pixels = 4 }
		};

		portModButton.OnLeftClick += (s, e) => {
			string modFolderName = Path.GetFileName(_mod);
			string csprojFile = Path.Combine(_mod, $"{modFolderName}.csproj");

			string args = $"\"{csprojFile}\"";
			var tMLPath = Path.GetDirectoryName(Path.GetFullPath(Assembly.GetExecutingAssembly().Location));
			var porterPath = Path.Combine(tMLPath, "tModPorter", (Platform.IsWindows ? "tModPorter.bat" : "tModPorter.sh"));

			var porterInfo = new ProcessStartInfo() {
				FileName = porterPath,
				Arguments = args,
				WorkingDirectory = tMLPath,
				UseShellExecute = true
			};

			try {
				var porter = ModCompile.StartOnHost(porterInfo);
			}
			catch (Exception ex) {
				Logging.tML.Error("Failed to start tModPorter", ex);
			}
		};

		Append(portModButton);

		contextButtonsLeft -= 26;
	}

	private void AddErrorButton(Exception e)
	{
		var modSaveErrorWarning = new UIHoverImage(UICommon.ButtonErrorTexture, Language.GetTextValue("tModLoader.MSSourceIssue")) {
			RemoveFloatingPointsFromDrawPosition = true,
			UseTooltipMouseText = true,
			Left = { Pixels = contextButtonsLeft, Percent = 1f },
			Top = { Pixels = 4 }
		};

		string fullError = Language.GetTextValue("tModLoader.MSSourceIssueMessage", modName, "\n\n" + e.ToString());
		modSaveErrorWarning.OnLeftClick += (a, b) => {
			Interface.infoMessage.Show(fullError, 888, Interface.modSources);
		};

		Append(modSaveErrorWarning);

		contextButtonsLeft -= 26;
	}
}
