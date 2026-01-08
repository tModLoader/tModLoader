using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Steamworks;
using Terraria.Audio;
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
			TextColor = Color.White,
			Top = new StyleDimension(90, 0),
			Left = new StyleDimension(32, 0),
		};
		wikiLink.OnMouseOver += delegate (UIMouseEvent evt, UIElement listeningElement) {
			SoundEngine.PlaySound(SoundID.MenuTick);
			wikiLink.TextColor = Main.OurFavoriteColor;
		};
		wikiLink.OnMouseOut += delegate (UIMouseEvent evt, UIElement listeningElement) {
			wikiLink.TextColor = Color.White;
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

			UIText betaNameText = new UIText(betaName) {
				Top = { Pixels = 2 },
				Left = { Pixels = 85 }
			};
			UIText betaDescriptionText = new UIText(branchDescription) {
				Top = { Pixels = 30 },
				Left = { Pixels = 100 + 12 },
				Width = StyleDimension.FromPixelsAndPercent(-112f, 1f),
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
				Width = { Pixels = 80 },
				Height = { Pixels = 80 },
				ScaleToFit = true,
			};

			branchPanel.Append(betaIconImage);
			branchPanel.Append(betaNameText);
			branchPanel.Append(betaDescriptionText);

			betaList.Add(branchPanel);

			if (betaBranchFlags.HasFlag(EBetaBranchFlags.k_EBetaBranch_Selected)) {
				UIText selectedMessage = new UIText("") {
					Top = { Pixels = betaDescriptionText.Top.Pixels + betaDescriptionText.GetOuterDimensions().Height - 18 },
					Left = { Pixels = 100 + 12 },
					Width = StyleDimension.FromPixelsAndPercent(-112f, 1f),
					IsWrapped = true,
					TextOriginX = 0,
					TextColor = Color.Red
				};

				selectedMessage.SetText(Language.GetTextValue("tModLoader.SwitchVersionCurrentlySelected"));
				// TODO: Do we need to care if the current exe is the steam exe or not? (such as a dev build)
				selectedMessage.TextColor = Color.Green;

				branchPanel.Append(selectedMessage);
				branchPanel.Recalculate();
				int textHeight = (int)selectedMessage.GetDimensions().Height;
				branchPanel.Height.Set(Math.Max(92, textHeight + selectedMessage.Top.Pixels - 12), 0f);
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
					Logging.tML.Info($"Switching to beta branch: {betaName}");
					SteamApps.SetActiveBeta(betaName);
					Main.instance.Exit();
				};
				branchPanel.Append(switchButton);
			}
		}
	}

	private void BackClick(UIMouseEvent evt, UIElement listeningElement)
	{
		(this as IHaveBackButtonCommand).HandleBackButtonUsage();
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);
		UILinkPointNavigator.Shortcuts.BackButtonCommand = 7;
	}
}
