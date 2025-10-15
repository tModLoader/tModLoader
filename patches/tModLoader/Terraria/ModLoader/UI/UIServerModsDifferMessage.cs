using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.Social.Steam;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace Terraria.ModLoader.UI;

/// <summary>
/// <paramref name="typeOrder"/> dictates the order specific explanations are shown:
/// <br/> 1: Download, 2: Switch Version, 3: Enable, 4: Disable, 5: Config Change
/// <para/> <paramref name="mod"/> is internal name, <paramref name="localMod"/> might be null for mods that need to be downloaded.
/// <br/><br/> <paramref name="riskState"/> dictates order within a typeOrder. Used for downloads ranging from most severe risk to least:
/// <br/> 1: Banned, 2: Not on workshop, 3: Hash doesn't match workshop files, 4: Low subscriber count, 5: On workshop, 6: Unable to access workshop
/// </summary>
internal record ReloadRequiredExplanation(int typeOrder, string mod, LocalMod localMod, string reason, DownloadModRiskState riskState = DownloadModRiskState.Unassigned);

internal enum DownloadModRiskState
{
	Unassigned,
	BannedOnWorkshop,
	NotOnWorkshop,
	HashDiffersFromWorkshop,
	LowSubscriberCount,
	AvailableOnWorkshop,
	UnableToVerify,
}

internal class UIServerModsDifferMessage : UIState, IHaveBackButtonCommand
{
	protected UIElement _area;
	private UIPanel uIPanel;
	private UIPanel messagePanel;
	private UIText message;
	private UIScrollbar uIScrollbar;
	private UIPanel warningMessagePanel;
	private UIText warningMessage;
	private UIPanel modPanel;
	private UIList modList;
	protected UITextPanel<string> _backButton;
	private UITextPanel<string> _continueButton;
	private UIState _gotoState;
	private string _message;
	private int _gotoMenu;
	private Action _continueButtonAction;
	private Action _backAction;
	private string _continueButtonText;
	private string _backText;
	private List<ReloadRequiredExplanation> reloadRequiredExplanationEntries;

	private const int WarningMessagePanelHeight = 70;

	public UIState PreviousUIState { get; set; }

	public override void OnInitialize()
	{
		_area = new UIElement {
			Width = { Percent = 0.8f },
			MaxWidth = UICommon.MaxPanelWidth,
			Top = { Pixels = 200 },
			Height = { Pixels = -200, Percent = 1f },
			HAlign = 0.5f
		};

		uIPanel = new UIPanel {
			Width = { Percent = 1f },
			Height = { Pixels = -100, Percent = 1f },
			BackgroundColor = UICommon.MainPanelBackground
		};
		_area.Append(uIPanel);

		messagePanel = new UIPanel {
			Width = { Percent = 1f },
			Height = { Pixels = 100f }
		};
		uIPanel.Append(messagePanel);

		message = new UIText("") {
			Width = { Percent = 1f },
			Height = { Percent = 1f }
		};
		message.IsWrapped = true;
		messagePanel.Append(message);

		uIScrollbar = new UIScrollbar {
			Height = { Pixels = -118, Percent = 1f },
			Top = { Pixels = 112 },
			HAlign = 1f
		}.WithView(100f, 1000f);
		uIPanel.Append(uIScrollbar);

		modPanel = new UIPanel {
			Top = { Pixels = 106 },
			Width = { Pixels = -24, Percent = 1f },
			Height = { Pixels = -106f, Percent = 1f }
		};
		modPanel.SetPadding(6);
		uIPanel.Append(modPanel);

		modList = new UIList {
			Width = { Percent = 1f },
			Height = { Percent = 1f },
			ListPadding = 5f
		};
		modList.ManualSortMethod = (e) => { };
		modPanel.Append(modList);

		modList.SetScrollbar(uIScrollbar);

		warningMessagePanel = new UIPanel {
			Width = { Percent = 1f },
			Height = { Pixels = WarningMessagePanelHeight },
			Top = { Pixels = -WarningMessagePanelHeight, Percent = 1f },
			BackgroundColor = Color.Red
		};

		warningMessage = new UIText("") {
			Width = { Percent = 1f },
			Height = { Percent = 1f }
		};
		warningMessage.IsWrapped = true;
		warningMessagePanel.Append(warningMessage);

		_backButton = new UITextPanel<string>("???", 0.7f, true) {
			Width = { Pixels = -6, Percent = 1 / 3f },
			Height = { Pixels = 50 },
			Left = { Percent = 0f },
			VAlign = 1f,
			Top = { Pixels = -45 }
		}.WithFadedMouseOver();
		_backButton.OnLeftClick += BackClick;
		_area.Append(_backButton);

		_continueButton = new UITextPanel<string>("???", 0.7f, true) {
			Width = { Pixels = -6, Percent = 2 / 3f },
			Height = { Pixels = 50 },
			VAlign = 1f,
			HAlign = 1f,
			Top = { Pixels = -45 }
		}.WithFadedMouseOver();
		_continueButton.OnLeftClick += ContinueClick;
		_area.Append(_continueButton);

		Append(_area);
	}

	public override void OnActivate()
	{
		message.SetText(_message);

		// Replace _continueButton since it is hard to change the OnMouseOver/OnMouseOut from WithFadedMouseOver
		_continueButton.Remove();
		_continueButton = new UITextPanel<string>("???", 0.7f, true) {
			Width = { Pixels = -6, Percent = 2 / 3f },
			Height = { Pixels = 50 },
			VAlign = 1f,
			HAlign = 1f,
			Top = { Pixels = -45 }
		};
		if (ModNet.ModNetDownloadQueued) {
			_continueButton.BackgroundColor = Color.Red * 0.7f;
			_continueButton.WithFadedMouseOver(overColor: Color.Red, outColor: Color.Red * 0.7f);
		}
		else {
			_continueButton.WithFadedMouseOver();
		}
		_continueButton.OnLeftClick += ContinueClick;
		_area.Append(_continueButton);

		_continueButton.SetText(_continueButtonText);
		_backButton.SetText(_backText);
		modPanel.Height.Pixels = -106f;
		uIScrollbar.Height.Pixels = -118f;
		warningMessagePanel.Remove();

		modList.Clear();
		foreach (var entry in reloadRequiredExplanationEntries) {
			UIPanel panel = new UIPanel();
			panel.SetPadding(6);
			panel.Width.Set(0, 1f);
			panel.Height.Set(92, 0f);
			panel.BackgroundColor = UICommon.DefaultUIBlue;

			UIText modName = new UIText(entry.localMod?.DisplayName ?? entry.mod) {
				Top = { Pixels = 2 },
				Left = { Pixels = 85 }
			};
			UIText reason = new UIText(entry.reason) {
				Top = { Pixels = 30 },
				Left = { Pixels = 100 + 12 },
				Width = StyleDimension.FromPixelsAndPercent(-112f, 1f),
				TextOriginX = 0
			};
			reason.IsWrapped = true;

			var modIcon = Main.Assets.Request<Texture2D>("Images/UI/DefaultResourcePackIcon", AssetRequestMode.ImmediateLoad);
			if (entry.localMod != null) {
				try {
					using (entry.localMod.modFile.Open())
					using (var s = entry.localMod.modFile.GetStream("icon.png")) {
						var iconTexture = Main.Assets.CreateUntracked<Texture2D>(s, ".png");

						if (iconTexture.Width() == 80 && iconTexture.Height() == 80) {
							modIcon = iconTexture;
						}
					}
				}
				catch (Exception e) {
					Logging.tML.Error("Unknown error", e);
				}
			}
			UIImage modIconImage = new UIImage(modIcon) {
				Left = { Percent = 0f },
				Top = { Percent = 0f },
				Width = { Pixels = 80 },
				Height = { Pixels = 80 },
				ScaleToFit = true,
			};

			panel.Append(modIconImage);
			panel.Append(modName);
			panel.Append(reason);

			modList.Add(panel);

			if (entry.typeOrder == 1) { // Download from server
				// TODO: Download from Server/Workshop toggle

				// Due to issues with chat tags and wrapped text, this is a separate UIText from reason
				UIText warning = new UIText("") {
					Top = { Pixels = reason.Top.Pixels + reason.GetOuterDimensions().Height - 18 },
					Left = { Pixels = 100 + 12 },
					Width = StyleDimension.FromPixelsAndPercent(-112f, 1f),
					IsWrapped = true,
					TextOriginX = 0,
					TextColor = Color.Red
				};

				string warningKey = entry.riskState switch {
					DownloadModRiskState.BannedOnWorkshop => "tModLoader.MPServerModBannedOnWorkshop",
					DownloadModRiskState.NotOnWorkshop => "tModLoader.MPServerModCustomBuild",
					DownloadModRiskState.HashDiffersFromWorkshop => "tModLoader.MPServerModCustomBuild",
					DownloadModRiskState.LowSubscriberCount => "tModLoader.MPServerModAvailableOnWorkshopLowSubscriberCount",
					DownloadModRiskState.AvailableOnWorkshop => "tModLoader.MPServerModAvailableOnWorkshop",
					DownloadModRiskState.UnableToVerify => "tModLoader.MPServerModUnableToVerify",
					_ => throw new NotImplementedException(),
				};
				warning.SetText(Language.GetTextValue(warningKey));
				if (entry.riskState == DownloadModRiskState.AvailableOnWorkshop)
					warning.TextColor = Color.White;

				panel.Append(warning);
				panel.Recalculate();
				int textHeight = (int)warning.GetDimensions().Height;
				panel.Height.Set(Math.Max(92, textHeight + warning.Top.Pixels - 12), 0f);
			}
		}

		if (Main.tServer != null) {
			UIPanel panel = new UIPanel();
			panel.Width.Set(0, 1f);
			panel.Height.Set(130, 0f);
			panel.BackgroundColor = Microsoft.Xna.Framework.Color.Orange;

			message = new UIText(Language.GetTextValue("tModLoader.ReloadRequiredHostAndPlayModWasDisabledHint")) {
				Width = { Percent = 1f },
				Height = { Percent = 1f }
			};
			message.IsWrapped = true;
			message.OnLeftClick += (a, b) => Utils.OpenToURL("https://github.com/tModLoader/tModLoader/wiki/Debugging-Multiplayer-Usage-Issues#when-i-join-my-own-server-mods-get-disabled");
			panel.Append(message);

			modList.Add(panel);
		}
		else {
			// Not host and play
			if (ModNet.ModNetDownloadQueued) {
				modPanel.Height.Pixels = -106 - 6 - WarningMessagePanelHeight;
				uIScrollbar.Height.Pixels = -118 - 6 - WarningMessagePanelHeight;
				uIPanel.Append(warningMessagePanel);
				warningMessage.SetText(Language.GetTextValue("tModLoader.MPServerModsDownloadFromServerWarning"));
			}
		}
	}

	internal void Show(string message, int gotoMenu, UIState gotoState = null, string continueButtonText = "", Action continueButtonAction = null, string backButtonText = null, Action backButtonAction = null, List<ReloadRequiredExplanation> reloadRequiredExplanationEntries = null)
	{
		if (!Program.IsMainThread) {
			// in some cases it would be better to block on this, but in other cases that might be a deadlock. Better to assume that letting the thread continue is the right choice
			Main.QueueMainThreadAction(() => Show(message, gotoMenu, gotoState, continueButtonText, continueButtonAction, backButtonText, backButtonAction, reloadRequiredExplanationEntries));
			return;
		}

		_message = message;
		_gotoMenu = gotoMenu;
		_gotoState = gotoState;
		_continueButtonText = continueButtonText;
		_continueButtonAction = continueButtonAction;
		_backText = backButtonText;
		_backAction = backButtonAction;
		this.reloadRequiredExplanationEntries = reloadRequiredExplanationEntries?.OrderBy(x => x.typeOrder).ThenBy(x => x.riskState).ThenBy(x => x.mod).ToList();
		Main.menuMode = Interface.serverModsDifferMessageID;
		Main.MenuUI.SetState(null); // New SetState code ignores setting to current state, so this is necessary to ensure OnActivate is called.
		Main.alreadyGrabbingSunOrMoon = false; // Prevents cursor from being invisible in rare situations because netmode is technically 1 at this menu so it won't reset correctly.
		Logging.tML.Info("ModsDifferMessage: " + message + "\n" + string.Join("\n", reloadRequiredExplanationEntries.Select(x => $"    {x.localMod?.DisplayNameClean ?? x.mod}: {Utils.CleanChatTags(x.reason).Replace("\n", " ")}")));
	}

	private void BackClick(UIMouseEvent evt, UIElement listeningElement)
	{
		HandleBackButtonUsage();
	}

	public void HandleBackButtonUsage()
	{
		SoundEngine.PlaySound(SoundID.MenuOpen);
		Main.menuMode = _gotoMenu;
		if (_gotoState != null)
			Main.MenuUI.SetState(_gotoState);
		_backAction();
	}

	private void ContinueClick(UIMouseEvent evt, UIElement listeningElement)
	{
		_continueButtonAction();
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);
		UILinkPointNavigator.Shortcuts.BackButtonCommand = 7;
	}
}
