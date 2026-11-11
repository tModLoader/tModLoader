using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Steamworks;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using Terraria.UI.Gamepad;
using Version = System.Version;

namespace Terraria.ModLoader.UI;

internal class UISwitchBeta : UIState, IHaveBackButtonCommand
{
	private record struct Branch(EBetaBranchFlags Flags, uint BuildID, string Name, string Description)
	{
		public bool Default;
		public bool Obsolete;
		public bool Preview;
		public Version TmlMajorMinor = null;
	}

	protected UIElement area;
	private UIPanel contentPanel;
	private UIPanel topMessagePanel;
	private UIText topMessage;
	private UIText wikiLink;
	private UIScrollbar contentPanelScrollbar;
	private UIPanel betaListPanel;
	private UIList betaList;
	protected UITextPanel<string> backButton;
	private UIConfirmDialog confirmDialog;

	public UIState PreviousUIState { get; set; }

	public override void OnInitialize()
	{
		area = new UIElement {
			Width = { Percent = 0.8f },
			MaxWidth = UICommon.MaxPanelWidth,
			Top = { Pixels = 200 },
			Height = { Pixels = -200, Percent = 1f },
			HAlign = 0.5f
		};

		contentPanel = new UIPanel {
			Width = { Percent = 1f },
			Height = { Pixels = -100, Percent = 1f },
			BackgroundColor = UICommon.MainPanelBackground
		};
		area.Append(contentPanel);

		int topMessagePanelHeight = 130;
		topMessagePanel = new UIPanel {
			Width = { Percent = 1f },
			Height = { Pixels = topMessagePanelHeight }
		};
		contentPanel.Append(topMessagePanel);

		topMessage = new UIText(Language.GetTextValue("tModLoader.SwitchVersionInstructions")) {
			Width = { Percent = 1f },
			Height = { Percent = 1f }
		};
		topMessage.IsWrapped = true;
		topMessagePanel.Append(topMessage);

		wikiLink = new UIText(Language.GetTextValue("tModLoader.SwitchVersionWikiLinkLabel")) {
			TextColor = Color.LightGray,
			Top = new StyleDimension(90, 0),
			Left = new StyleDimension(32, 0),
		};
		wikiLink.OnMouseOver += delegate (UIMouseEvent evt, UIElement listeningElement) {
			SoundEngine.PlaySound(SoundID.MenuTick);
			wikiLink.TextColor = Main.OurFavoriteColor;
		};
		wikiLink.OnMouseOut += delegate (UIMouseEvent evt, UIElement listeningElement) {
			wikiLink.TextColor = Color.LightGray;
		};
		wikiLink.OnLeftClick += delegate (UIMouseEvent evt, UIElement listeningElement) {
			SoundEngine.PlaySound(SoundID.MenuOpen);
			Utils.OpenToURL("https://github.com/tModLoader/tModLoader/wiki/tModLoader-guide-for-players#beta-branches");
		};
		topMessagePanel.Append(wikiLink);

		contentPanelScrollbar = new UIScrollbar {
			Height = { Pixels = -topMessagePanelHeight - 18, Percent = 1f },
			Top = { Pixels = topMessagePanelHeight + 12 },
			HAlign = 1f
		}.WithView(100f, 1000f);
		contentPanel.Append(contentPanelScrollbar);

		betaListPanel = new UIPanel {
			Top = { Pixels = topMessagePanelHeight + 6 },
			Width = { Pixels = -24, Percent = 1f },
			Height = { Pixels = -topMessagePanelHeight - 6, Percent = 1f }
		};
		betaListPanel.SetPadding(6);
		contentPanel.Append(betaListPanel);

		betaList = new UIList {
			Width = { Percent = 1f },
			Height = { Percent = 1f },
			ListPadding = 5f
		};
		betaList.ManualSortMethod = (e) => { };
		betaListPanel.Append(betaList);

		betaList.SetScrollbar(contentPanelScrollbar);

		backButton = new UITextPanel<string>(Language.GetTextValue("UI.Back"), 0.7f, true) {
			Width = { Pixels = -6, Percent = 1 / 3f },
			Height = { Pixels = 50 },
			Left = { Percent = 0f },
			VAlign = 1f,
			Top = { Pixels = -45 }
		}.WithFadedMouseOver();
		backButton.OnLeftClick += BackClick;
		area.Append(backButton);

		Append(area);
	}

	public override void OnActivate()
	{
		betaList.Clear();

		int betaBranchCount = SteamApps.GetNumBetas(out _, out _);
		bool onBetaBranch = SteamApps.GetCurrentBetaName(out string branchName, 128);
		var tmlVersion = BuildInfo.tMLVersion.MajorMinor();
		var previewRegex = new Regex(@"preview\-[v]?(\d+\.\d+)", RegexOptions.Compiled);

		// Collect branches first.
		var branches = new List<Branch>(betaBranchCount);
		int newestBranchIdx = -1;
		for (int i = 0; i < betaBranchCount; i++) {
			SteamApps.GetBetaInfo(i, out uint flagsValue, out uint buildID, out string betaName, 128, out string branchDescription, 1024, out uint lastUpdated);

			var branch = new Branch((EBetaBranchFlags)flagsValue, buildID, betaName, branchDescription);

			if (branch.Flags.HasFlag(EBetaBranchFlags.k_EBetaBranch_Private))
				continue;

			branch.Default = branch.Flags.HasFlag(EBetaBranchFlags.k_EBetaBranch_Default);

			if (!onBetaBranch && branch.Default) {
				branch.TmlMajorMinor = tmlVersion;
			}
			else if (previewRegex.Match(branch.Name) is { Success: true } m && m.Groups[1] is { Value: string vStr } && Version.TryParse(vStr, out var version)) {
				branch.Preview = true;
				branch.TmlMajorMinor = version;
			}

			if (newestBranchIdx < 0 || (branch.TmlMajorMinor is { } v && v > branches[newestBranchIdx].TmlMajorMinor))
				newestBranchIdx = branches.Count;

			branches.Add(branch);
		}

		// Identify obsolete preview branches.
		foreach (ref var branch in CollectionsMarshal.AsSpan(branches)) {
			if (branch.Preview && branch.TmlMajorMinor is { } branchVersion
			&& newestBranchIdx >= 0 && branches[newestBranchIdx] is { } newest
			// Edge case: If the default branch is the newest, the branch information might be out of date.
			// Display all previews as obsolete in that case.
			&& (newest.Default ? (branch.TmlMajorMinor <= newest.TmlMajorMinor) : (branch.TmlMajorMinor < newest.TmlMajorMinor))) {
				branch.Obsolete = true;
			}
		}

		// Put obsolete branches last.
		branches.Sort((a, b) => a.Obsolete.CompareTo(b.Obsolete));

		for (int i = 0; i < branches.Count; i++) {
			var branch = branches[i];

			UIPanel branchPanel = new UIPanel();
			branchPanel.SetPadding(6);
			branchPanel.Width.Set(0, 1f);
			branchPanel.Height.Set(102, 0f);
			branchPanel.BackgroundColor = UICommon.DefaultUIBlue;

			string betaNameDisplay = branch.Name;
			if (branch.Flags.HasFlag(EBetaBranchFlags.k_EBetaBranch_Default)) {
				betaNameDisplay = Language.GetTextValue("tModLoader.SwitchVersionDefaultBranchName");
				branch.Description = Language.GetTextValue("tModLoader.SwitchVersionDefaultBranchDescription");
			}
			// TODO: If requested, we could consider adding localization within tModLoader for the other beta branch names/descriptions, but it should be fine since this is the current status quo.

			if (branch.Obsolete) {
				branchPanel.BackgroundColor = Color.Lerp(UICommon.DefaultUIBlue, Color.Black, 0.5f);
				branch.Description += $"\n{Language.GetTextValue("tModLoader.SwitchVersionObsoleteBranch")}";
			}

			UIText betaNameText = new UIText(betaNameDisplay) {
				Top = { Pixels = 2 },
				Left = { Pixels = 45 }
			};
			UIText betaDescriptionText = new UIText(branch.Description) {
				Top = { Pixels = 30 },
				Left = { Pixels = 60 + 12 },
				Width = StyleDimension.FromPixelsAndPercent(-72f, 1f),
				TextOriginX = 0
			};
			betaDescriptionText.IsWrapped = true;

			string betaIconPath = "Images/UI/WorldCreation/IconDifficultyNormal";
			if (branch.Name.Contains("legacy"))
				betaIconPath = "Images/UI/WorldCreation/IconDifficultyCreative";
			else if (!branch.Flags.HasFlag(EBetaBranchFlags.k_EBetaBranch_Default))
				betaIconPath = "Images/UI/WorldCreation/IconDifficultyExpert";

			UIImage betaIconImage = new UIImage(Main.Assets.Request<Texture2D>(betaIconPath, AssetRequestMode.ImmediateLoad)) {
				Left = { Percent = 0f },
				Top = { Percent = 0f },
				Width = { Pixels = 40 },
				Height = { Pixels = 40 },
				ScaleToFit = true,
			};

			branchPanel.Append(betaIconImage);
			branchPanel.Append(betaNameText);
			branchPanel.Append(betaDescriptionText);

			betaList.Add(branchPanel);

			if (branch.Flags.HasFlag(EBetaBranchFlags.k_EBetaBranch_Selected)) {
				float top = betaDescriptionText.Top.Pixels + betaDescriptionText.GetOuterDimensions().Height - 18;

				UIText selectedMessage = new UIText(Language.GetTextValue("tModLoader.SwitchVersionCurrentlySelected")) {
					Top = { Pixels = top },
					Left = { Pixels = 45 },
					Width = StyleDimension.FromPixelsAndPercent(-72f, 1f),
					IsWrapped = true,
					TextOriginX = 0,
					TextColor = Color.Green
				};
				branchPanel.Append(selectedMessage);
				branchPanel.Recalculate();
				top = selectedMessage.Top.Pixels + selectedMessage.GetOuterDimensions().Height - 18;

				SteamApps.GetAppInstallDir(Engine.Steam.TMLAppID_t, out string tModLoaderInstallDirectory, 1000);
				string currentWorkingDirectory = Environment.CurrentDirectory;
				if (Path.GetRelativePath(tModLoaderInstallDirectory, currentWorkingDirectory) != ".") {
					UIText notSteamInstallWarning = new UIText(Language.GetTextValue("tModLoader.SwitchVersionCurrentlySelectedButRunningSeparateInstallWarning")) {
						Top = { Pixels = top },
						Left = { Pixels = 45 },
						Width = StyleDimension.FromPixelsAndPercent(-72f, 1f),
						IsWrapped = true,
						TextOriginX = 0,
						TextColor = Color.Orange
					};
					branchPanel.Append(notSteamInstallWarning);
					branchPanel.Recalculate();
					top = notSteamInstallWarning.Top.Pixels + notSteamInstallWarning.GetOuterDimensions().Height - 18;
				}
				branchPanel.Height.Set(Math.Max(92, top), 0f);
			}
			else {
				var buttonPlayTexture = Main.Assets.Request<Texture2D>("Images/UI/ButtonPlay");
				UIHoverImage switchButton = new UIHoverImage(buttonPlayTexture, Language.GetTextValue("tModLoader.SwitchVersionSwitchButtonTooltip")) {
					HAlign = 1f,
					VAlign = 1f,
					Left = new(-6, 0),
					Top = new(-6, 0),
					UseTooltipMouseText = true,
				};
				switchButton.OnMouseOver += delegate (UIMouseEvent evt, UIElement listeningElement) {
					SoundEngine.PlaySound(SoundID.MenuTick);
				};
				switchButton.OnLeftClick += (a, b) => {
					ShowConfirmationWindow((UIElement, Vector2) => {
						Logging.tML.Info($"Switching to beta branch: {branch.Name}");
						SteamApps.SetActiveBeta(branch.Name);
						Main.instance.Exit();
					}, "tModLoader.SwitchVersionConfirm");
				};
				branchPanel.Append(switchButton);
			}
		}
	}

	private void ShowConfirmationWindow(MouseEvent yesAction, string confirmDialogTextKey)
	{
		ShowConfirmDialog(new UIConfirmDialog(
			showYesDontShowAgainButton: false,
			Language.GetText(confirmDialogTextKey),
			yesAction: yesAction
        ));
	}

	internal void ShowConfirmDialog(UIConfirmDialog dialog)
	{
		if (confirmDialog is not null)
			confirmDialog.Close(playSound: false);

		SoundEngine.PlaySound(SoundID.MenuOpen);
		confirmDialog = dialog;
		confirmDialog.OnClose = () => confirmDialog = null;
		Append(confirmDialog);
	}

	private void BackClick(UIMouseEvent evt, UIElement listeningElement)
	{
		(this as IHaveBackButtonCommand).HandleBackButtonUsage();
	}

	public void HandleBackButtonUsage()
	{
		if (confirmDialog != null) {
			confirmDialog.Close();
			return;
		}

		IHaveBackButtonCommand.GoBackTo(PreviousUIState);
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);
		UILinkPointNavigator.Shortcuts.BackButtonCommand = 7;
	}
}
