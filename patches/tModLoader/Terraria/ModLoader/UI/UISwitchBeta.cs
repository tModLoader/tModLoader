using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Steamworks;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace Terraria.ModLoader.UI;

internal class UISwitchBeta : UIState, IHaveBackButtonCommand
{
	protected UIElement area;
	private UIPanel contentPanel;
	private UIPanel topMessagePanel;
	private UIText topMessage;
	private UIText wikiLink;
	private UIScrollbar contentPanelScrollbar;
	private UIPanel betaListPanel;
	private UIList betaList;
	protected UITextPanel<string> backButton;
	// confirmation dialog
	private UIAutoScaleTextTextPanel<LocalizedText> confirmDialogYesButton;
	private UIAutoScaleTextTextPanel<LocalizedText> confirmDialogNoButton;
	private UIText confirmDialogText;
	private UIImage blockInput;
	private UIPanel activeDialog;

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

		for (int i = 0; i < betaBranchCount; i++) {
			SteamApps.GetBetaInfo(i, out uint betaBranchFlagsValue, out uint buildID, out string betaName, 128, out string branchDescription, 1024);
			EBetaBranchFlags betaBranchFlags = (EBetaBranchFlags)betaBranchFlagsValue;

			if (betaBranchFlags.HasFlag(EBetaBranchFlags.k_EBetaBranch_Private))
				continue;

			UIPanel branchPanel = new UIPanel();
			branchPanel.SetPadding(6);
			branchPanel.Width.Set(0, 1f);
			branchPanel.Height.Set(102, 0f);
			branchPanel.BackgroundColor = UICommon.DefaultUIBlue;

			string betaNameDisplay = betaName;
			if (betaBranchFlags.HasFlag(EBetaBranchFlags.k_EBetaBranch_Default)) {
				betaNameDisplay = Language.GetTextValue("tModLoader.SwitchVersionDefaultBranchName");
				branchDescription = Language.GetTextValue("tModLoader.SwitchVersionDefaultBranchDescription");
			}
			// TODO: If requested, we could consider adding localization within tModLoader for the other beta branch names/descriptions, but it should be fine since this is the current status quo.

			UIText betaNameText = new UIText(betaNameDisplay) {
				Top = { Pixels = 2 },
				Left = { Pixels = 45 }
			};
			UIText betaDescriptionText = new UIText(branchDescription) {
				Top = { Pixels = 30 },
				Left = { Pixels = 60 + 12 },
				Width = StyleDimension.FromPixelsAndPercent(-72f, 1f),
				TextOriginX = 0
			};
			betaDescriptionText.IsWrapped = true;

			string betaIconPath = "Images/UI/WorldCreation/IconDifficultyNormal";
			if (betaName.Contains("legacy"))
				betaIconPath = "Images/UI/WorldCreation/IconDifficultyCreative";
			else if (!betaBranchFlags.HasFlag(EBetaBranchFlags.k_EBetaBranch_Default))
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

			if (betaBranchFlags.HasFlag(EBetaBranchFlags.k_EBetaBranch_Selected)) {
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
				if(Path.GetRelativePath(tModLoaderInstallDirectory, currentWorkingDirectory) != ".") {
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
					SoundEngine.PlaySound(SoundID.MenuClose);
					ShowConfirmationWindow((UIElement, Vector2) => {
						Logging.tML.Info($"Switching to beta branch: {betaName}");
						SteamApps.SetActiveBeta(betaName);
						Main.instance.Exit();
					}, "tModLoader.SwitchVersionConfirm");
				};
				branchPanel.Append(switchButton);
			}
		}
	}

	private void ShowConfirmationWindow(MouseEvent yesAction, string confirmDialogTextKey)
	{
		var confirmationDialog = new UIPanel() {
			Width = { Pixels = 500 },
			Height = { Pixels = 160 },
			HAlign = .5f,
			VAlign = .5f,
			BackgroundColor = new Color(63, 82, 151),
			BorderColor = Color.Black
		};
		confirmationDialog.SetPadding(6f);
		ShowConfirmDialog(confirmationDialog);

		confirmDialogYesButton = new UIAutoScaleTextTextPanel<LocalizedText>(Language.GetText("LegacyMenu.104")) {
			TextColor = Color.White,
			Width = new StyleDimension(-10f, 1f / 3f),
			Height = { Pixels = 40 },
			VAlign = .95f,
			HAlign = .15f
		}.WithFadedMouseOver();
		confirmDialogYesButton.OnLeftClick += yesAction;
		confirmDialogYesButton.OnLeftClick += CloseConfirmDialog;
		confirmationDialog.Append(confirmDialogYesButton);

		confirmDialogNoButton = new UIAutoScaleTextTextPanel<LocalizedText>(Language.GetText("LegacyMenu.105")) {
			TextColor = Color.White,
			Width = new StyleDimension(-10f, 1f / 3f),
			Height = { Pixels = 40 },
			VAlign = .95f,
			HAlign = .85f
		}.WithFadedMouseOver();
		confirmDialogNoButton.OnLeftClick += CloseConfirmDialog;
		confirmationDialog.Append(confirmDialogNoButton);

		confirmDialogText = new UIText(Language.GetTextValue(confirmDialogTextKey)) {
			Width = { Percent = .75f },
			HAlign = .5f,
			VAlign = .2f,
			IsWrapped = true
		};
		confirmationDialog.Append(confirmDialogText);
		Recalculate();
	}

	internal void CloseConfirmDialog(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(SoundID.MenuClose);
		blockInput?.Remove();
		activeDialog?.Remove();
	}

	internal void ShowConfirmDialog(UIPanel dialog)
	{
		blockInput = new UIImage(TextureAssets.Extra[190]) {
			Width = { Percent = 1 },
			Height = { Percent = 1 },
			Color = Color.Black * 0.5f,
			ScaleToFit = true
		};
		blockInput.Width = StyleDimension.Fill;
		blockInput.Height = StyleDimension.Fill;
		blockInput.OnLeftMouseDown += CloseConfirmDialog;
		Append(blockInput);

		Append(activeDialog = dialog);
	}

	private void BackClick(UIMouseEvent evt, UIElement listeningElement)
	{
		(this as IHaveBackButtonCommand).HandleBackButtonUsage();
	}

	public void HandleBackButtonUsage()
	{
		if (blockInput != null && HasChild(blockInput)) {
			CloseConfirmDialog(null, null);
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
