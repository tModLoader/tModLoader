using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using ReLogic.OS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace Terraria.ModLoader.UI;

internal class UIModSources : UIState, IHaveBackButtonCommand
{
	public UIState PreviousUIState { get; set; }
	private readonly List<UIModSourceItem> _items = new List<UIModSourceItem>();
	private UIList _modList;
	private float modListViewPosition;
	private bool _updateNeeded;
	private UIElement _uIElement;
	private UIPanel _uIPanel;
	private UIInputTextField filterTextBox;
	private UILoaderAnimatedImage _uiLoader;
	private UIElement _links;
	private CancellationTokenSource _cts;
	private static bool dotnetSDKFound;

	public override void OnInitialize()
	{
		_uIElement = new UIElement {
			Width = { Percent = 0.8f },
			MaxWidth = UICommon.MaxPanelWidth,
			Top = { Pixels = 220 },
			Height = { Pixels = -220, Percent = 1f },
			HAlign = 0.5f
		};

		_uIPanel = new UIPanel {
			Width = { Percent = 1f },
			Height = { Pixels = -65, Percent = 1f },
			BackgroundColor = UICommon.MainPanelBackground,
			PaddingTop = 0f
		};
		_uIElement.Append(_uIPanel);

		_uiLoader = new UILoaderAnimatedImage(0.5f, 0.5f, 1f);

		var upperMenuContainer = new UIElement {
			Width = { Percent = 1f },
			Height = { Pixels = 82 },
			Top = { Pixels = 10 }
		};
		var filterTextBoxBackground = new UIPanel {
			Top = { Percent = 0f },
			Left = { Pixels = -135, Percent = 1f },
			Width = { Pixels = 135 },
			Height = { Pixels = 32 }
		};
		filterTextBoxBackground.OnRightClick += (a, b) => filterTextBox.Text = "";
		upperMenuContainer.Append(filterTextBoxBackground);

		filterTextBox = new UIInputTextField(Language.GetTextValue("tModLoader.ModsTypeToSearch")) {
			Top = { Pixels = 5 },
			Left = { Pixels = -125, Percent = 1f },
			Width = { Pixels = 120 },
			Height = { Pixels = 20 }
		};
		filterTextBox.OnRightClick += (a, b) => filterTextBox.Text = "";
		filterTextBox.OnTextChange += (a, b) => _updateNeeded = true;
		upperMenuContainer.Append(filterTextBox);
		_uIPanel.Append(upperMenuContainer);

		_modList = new UIList {
			Width = { Pixels = -25, Percent = 1f },
			Height = { Pixels = -134, Percent = 1f },
			Top = { Pixels = 134 },
			ListPadding = 5f
		};
		_uIPanel.Append(_modList);

		var uIScrollbar = new UIScrollbar {
			Height = { Pixels = -134, Percent = 1f },
			Top = { Pixels = 134 },
			HAlign = 1f
		}.WithView(100f, 1000f);
		_uIPanel.Append(uIScrollbar);
		_modList.SetScrollbar(uIScrollbar);

		var uIHeaderTextPanel = new UITextPanel<string>(Language.GetTextValue("tModLoader.MenuModSources"), 0.8f, true) {
			HAlign = 0.5f,
			Top = { Pixels = -35 },
			BackgroundColor = UICommon.DefaultUIBlue
		}.WithPadding(15f);
		_uIElement.Append(uIHeaderTextPanel);

		_links = new UIPanel {
			Width = { Percent = 1f },
			Height = { Pixels = 78 },
			Top = { Pixels = 46 },
		};
		_links.SetPadding(8);
		_uIPanel.Append(_links);

		AddLink(Language.GetText("tModLoader.VersionUpgrade"), 0.5f, 0f, "https://github.com/tModLoader/tModLoader/wiki/Update-Migration-Guide");
		AddLink(Language.GetText("tModLoader.WikiLink"), 0f, 0.5f, "https://github.com/tModLoader/tModLoader/wiki/");
		string exampleModBranch = BuildInfo.IsStable ? "stable" : (BuildInfo.IsPreview ? "preview" : "1.4.4");
		AddLink(Language.GetText("tModLoader.ExampleModLink"), 1f, 0.5f, $"https://github.com/tModLoader/tModLoader/tree/{exampleModBranch}/ExampleMod");
		string docsURL = BuildInfo.IsStable ? "stable" : "preview";
		AddLink(Language.GetText("tModLoader.DocumentationLink"), 0f, 1f, $"https://docs.tmodloader.net/docs/{docsURL}/annotated.html");
		AddLink(Language.GetText("tModLoader.DiscordLink"), 1f, 1f, "https://tmodloader.net/discord");

		var buttonBA = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MSBuildAll")) {
			Width = { Pixels = -10, Percent = 1f / 3f },
			Height = { Pixels = 40 },
			VAlign = 1f,
			Top = { Pixels = -65 }
		};
		buttonBA.WithFadedMouseOver();
		buttonBA.OnLeftClick += BuildMods;
		//_uIElement.Append(buttonBA);

		var buttonBRA = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MSBuildReloadAll"));
		buttonBRA.CopyStyle(buttonBA);
		buttonBRA.HAlign = 0.5f;
		buttonBRA.WithFadedMouseOver();
		buttonBRA.OnLeftClick += BuildAndReload;
		//_uIElement.Append(buttonBRA);

		var buttonCreateMod = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MSCreateMod"));
		buttonCreateMod.CopyStyle(buttonBA);
		buttonCreateMod.HAlign = 1f;
		buttonCreateMod.Top.Pixels = -20;
		buttonCreateMod.WithFadedMouseOver();
		buttonCreateMod.OnLeftClick += ButtonCreateMod_OnClick;
		_uIElement.Append(buttonCreateMod);

		var buttonB = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("UI.Back"));
		buttonB.CopyStyle(buttonBA);
		//buttonB.Width.Set(-10f, 1f / 3f);
		buttonB.Top.Pixels = -20;
		buttonB.WithFadedMouseOver();
		buttonB.OnLeftClick += BackClick;
		_uIElement.Append(buttonB);

		var buttonOS = new UIAutoScaleTextTextPanel<string>(Language.GetTextValue("tModLoader.MSOpenSources"));
		buttonOS.CopyStyle(buttonB);
		buttonOS.HAlign = .5f;
		buttonOS.WithFadedMouseOver();
		buttonOS.OnLeftClick += OpenSources;
		_uIElement.Append(buttonOS);

		Append(_uIElement);
	}

	private void AddLink(LocalizedText text, float hAlign, float vAlign, string url)
	{
		var link = new UIText(text) {
			TextColor = Color.White,
			HAlign = hAlign,
			VAlign = vAlign,
		};
		link.OnMouseOver += delegate (UIMouseEvent evt, UIElement listeningElement) {
			SoundEngine.PlaySound(SoundID.MenuTick);
			link.TextColor = Main.OurFavoriteColor;
		};
		link.OnMouseOut += delegate (UIMouseEvent evt, UIElement listeningElement) {
			link.TextColor = Color.White;
		};
		link.OnLeftClick += delegate (UIMouseEvent evt, UIElement listeningElement) {
			SoundEngine.PlaySound(SoundID.MenuOpen);
			Utils.OpenToURL(url);
		};
		_links.Append(link);
	}

	private void ButtonCreateMod_OnClick(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(11);
		Main.menuMode = Interface.createModID;
	}

	private void BackClick(UIMouseEvent evt, UIElement listeningElement)
	{
		(this as IHaveBackButtonCommand).HandleBackButtonUsage();
	}

	private void OpenSources(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(10, -1, -1, 1);
		try {
			Directory.CreateDirectory(ModCompile.ModSourcePath);
			Utils.OpenFolder(ModCompile.ModSourcePath);
		}
		catch (Exception e) {
			Logging.tML.Error(e);
		}
	}

	private void BuildMods(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(10, -1, -1, 1);
		if (_modList.Count > 0)
			Interface.buildMod.BuildAll(false);
	}

	private void BuildAndReload(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(10, -1, -1, 1);
		if (_modList.Count > 0)
			Interface.buildMod.BuildAll(true);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		UILinkPointNavigator.Shortcuts.BackButtonCommand = 7;
		base.Draw(spriteBatch);
	}

	public override void OnActivate()
	{
		_cts = new CancellationTokenSource();
		Main.clrInput();
		ModCompile.UpdateReferencesFolder();
		_uIPanel.Append(_uiLoader);
		_modList.Clear();
		_items.Clear();
		if (ShowInfoMessages())
			return;
		Populate();
	}

	public override void OnDeactivate()
	{
		_cts?.Cancel(false);
		_cts?.Dispose();
		_cts = null;
		modListViewPosition = _modList.ViewPosition;
	}

	private bool ShowInfoMessages()
	{
		if (!ModLoader.SeenFirstLaunchModderWelcomeMessage) {
			ShowWelcomeMessage("tModLoader.MSFirstLaunchModderWelcomeMessage", "tModLoader.ViewOnGitHub", "https://github.com/tModLoader/tModLoader/wiki/tModLoader-guide-for-developers");
			ModLoader.SeenFirstLaunchModderWelcomeMessage = true;
			Main.SaveSettings();
			return true;
		}

		if (!IsCompatibleDotnetSdkAvailable()) {
			if (IsRunningInSandbox()) {
				Utils.ShowFancyErrorMessage(Language.GetTextValue("tModLoader.DevModsInSandbox"), 888, PreviousUIState);
			}
			else {
				ShowWelcomeMessage("tModLoader.MSNetSDKNotFound", "tModLoader.DownloadNetSDK", "https://github.com/tModLoader/tModLoader/wiki/tModLoader-guide-for-developers#net-sdk", 888, PreviousUIState);
			}

			return true;
		}

		return false;
	}

	private void ShowWelcomeMessage(string messageKey, string altButtonTextKey, string url, int gotoMenu = Interface.modSourcesID, UIState state = null)
	{
		var dotnetVersion = Environment.Version.MajorMinor().ToString();
		Interface.infoMessage.Show(Language.GetTextValue(messageKey, dotnetVersion), gotoMenu, state, Language.GetTextValue(altButtonTextKey, dotnetVersion), () => Utils.OpenToURL(url));
	}

	private static string GetCommandToFindPathOfExecutable()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			return "where";

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
		   RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
		   RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
			return "which";

		Logging.tML.Debug("Getting command for finding path of the executable failed due to an unsupported operating system");
		return null;
	}

	private static IEnumerable<string> GetPossibleSystemDotnetPaths()
	{
		if (GetCommandToFindPathOfExecutable() is string cmd) {
			yield return ModCompile.StartOnHost(new ProcessStartInfo {
				FileName = cmd,
				Arguments = "dotnet",
				UseShellExecute = false,
				RedirectStandardOutput = true
			}).StandardOutput.ReadToEnd().Split("\n")[0].Trim();
		}

		// OSX fallback
		var pathsFile = "/etc/paths.d/dotnet";
		if (File.Exists(pathsFile)) {
			var contents = File.ReadAllText(pathsFile).Trim();
			Logging.tML.Debug($"Reading {pathsFile}: {contents}");
			yield return contents + "/dotnet";
		}

		// These fallbacks are generally pretty useless, since /usr/bin should almost always be on PATH
		// env var, often set on Linux
		if (Environment.GetEnvironmentVariable("DOTNET_ROOT") is string dotnetRoot) {
			Logging.tML.Debug($"Found env var DOTNET_ROOT: {dotnetRoot}");
			yield return $"{dotnetRoot}/dotnet";
		}

		// The Scripted install installs the SDK to "$HOME/.dotnet" by default on Linux/Mac but will not permanently change $PATH. (Many Linux distributions have package manager instructions, but not all, so some might use scripted install: "./dotnet-install.sh -channel 6.0".) https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-install-script
		yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dotnet", "dotnet");

		// general unix fallback
		yield return "/usr/bin/dotnet";
	}

	private static string GetSystemDotnetPath()
	{
		try {
			if (GetPossibleSystemDotnetPaths().FirstOrDefault(DoesDotnetWork) is string path) {
				Logging.tML.Debug($"System dotnet install located at: {path}");
				return path;
			}
		}
		catch (Exception) { }

		Logging.tML.Debug("Finding dotnet on PATH failed");
		return null;
	}

	private static bool IsCompatibleDotnetSdkAvailable()
	{
		if (dotnetSDKFound)
			return true;

		try {
			string dotnetFilename = GetSystemDotnetPath() ?? "dotnet";
			string output = ModCompile.StartOnHost(new ProcessStartInfo {
				FileName = dotnetFilename,
				Arguments = "--list-sdks",
				UseShellExecute = false,
				RedirectStandardOutput = true
			}).StandardOutput.ReadToEnd().Trim();
			Logging.tML.Info("Found list of sdks:" + '\n' + output);

			if (Platform.IsWindows && dotnetFilename.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86))) {
				Logging.tML.Warn("Building mods requires the 64 bit dotnet SDK to be installed, but the 32 bit dotnet SDK was found on the PATH. It is likely that you accidentally installed the 32 bit dotnet SDK and it is taking priority. This will prevent you from debugging or building mods in Visual Studio or any other IDE. To fix this, follow the instructions at https://github.com/tModLoader/tModLoader/wiki/tModLoader-guide-for-developers#net-sdk");
			}

			foreach (var line in output.Split('\n')) {
				var dotnetVersion = new Version(new Regex("([0-9.]+).*").Match(line).Groups[1].Value);
				if (dotnetVersion.Major == Environment.Version.Major) {
					dotnetSDKFound = true;
					return true;
				}
			}
		}
		catch (Exception e) {
			Logging.tML.Debug("'dotnet --list-sdks' check failed: ", e);
		}

		return dotnetSDKFound;
	}

	private static bool IsRunningInSandbox()
	{
		if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("FLATPAK_SANDBOX_DIR"))) {
			Logging.tML.Debug("Flatpak sandbox detected");
			return true;
		}

		return false;
	}

	private static bool DoesDotnetWork(string path)
	{
		if (string.IsNullOrWhiteSpace(path))
			return false;

		try {
			// Try and execute each possible dotnet installation path,
			// we can't simply check if the file exists as we may be inside a steam-runtime container environment
			// and the path may point to a file on the host.
			var proc = ModCompile.StartOnHost(new ProcessStartInfo {
				FileName = path,
				Arguments = "--version"
			});
			if (proc == null) return false;

			proc.WaitForExit();
			if (proc.ExitCode == 0) {
				return true;
			}
			else {
				Logging.tML.Debug("Process ended with exit code: " + proc.ExitCode);
			}
		}
		catch (Exception e)
		{
			Logging.tML.Debug("Caught Exception during dotnet check:" + e);
		}

		return false;
	}

	internal void Populate()
	{
		Task.Run(() => {
			// It's important to call FindAllMods here first to ensure AllFoundMods is
			// properly initialized and populated when it's referenced in FindModSources.
			var modFiles = ModOrganizer.FindAllMods();
			var modSources = ModCompile.FindModSources();

			foreach (string sourcePath in modSources) {
				var modName = Path.GetFileName(sourcePath);
				var builtMod = modFiles.Where(m => m.Name == modName).Where(m => m.location == ModLocation.Local).OrderByDescending(m => m.Version).FirstOrDefault();
				if (builtMod != null && !string.IsNullOrWhiteSpace(builtMod.properties.modSource) && builtMod.properties.modSource != sourcePath)
					builtMod = null;
				_items.Add(new UIModSourceItem(sourcePath, builtMod, _cts.Token));
			}
			_updateNeeded = true;
		});
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		ModBrowser.UIModBrowser.PageUpDownSupport(_modList);
		if (!_updateNeeded)
			return;
		_updateNeeded = false;
		_uIPanel.RemoveChild(_uiLoader);
		_modList.Clear();
		string filter = filterTextBox.Text;
		_modList.AddRange(_items.Where(item => filter.Length > 0 ? item.modName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) != -1 : true));
		if (_items.Count == 0) {
			var firstModGuidePanel = new UIPanel() {
				Width = new(0, 1f),
				Height = new(180, 0f),
			};
			firstModGuidePanel.OnLeftClick += (a, b) => {
				Utils.OpenToURL("https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide");
			};
			var firstModGuideText = new UIText(Language.GetTextValue("tModLoader.MSNoModSourcesLinkToBasicModdingGuide")) {
				IsWrapped = true,
				WrappedTextBottomPadding = 0f,
				Width = StyleDimension.Fill,
				TextOriginX = 0f,
				VAlign = 0.5f
			};
			firstModGuidePanel.Append(firstModGuideText);
			_modList.Add(firstModGuidePanel);
		}
		Recalculate();
		_modList.ViewPosition = modListViewPosition;
	}
}
