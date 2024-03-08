﻿using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader.Core;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace Terraria.ModLoader.UI;

/// <summary>
/// <paramref name="typeOrder"/> dictates the order specific explanations are shown:
/// <br/> 1: Download, 2: Switch Version, 3: Enable, 4: Disable, 5: Config Change
/// </summary>
internal record ReloadRequiredExplanation(int typeOrder, string mod, LocalMod localMod, string reason);

internal class UIServerModsDifferMessage : UIState, IHaveBackButtonCommand
{
	protected UIElement _area;
	private UIPanel messagePanel;
	private UIText message;
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

		var uIPanel = new UIPanel {
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

		var uIScrollbar = new UIScrollbar {
			Height = { Pixels = -118, Percent = 1f },
			Top = { Pixels = 112 },
			HAlign = 1f
		}.WithView(100f, 1000f);
		uIPanel.Append(uIScrollbar);

		UIPanel modPanel = new UIPanel {
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
		_continueButton.SetText(_continueButtonText);
		_backButton.SetText(_backText);

		modList.Clear();
		foreach (var entry in reloadRequiredExplanationEntries) {
			UIPanel panel = new UIPanel();
			panel.SetPadding(6);
			panel.Width.Set(0, 1f);
			panel.Height.Set(92, 0f);
			panel.BackgroundColor = UICommon.DefaultUIBlue;

			UIText modName = new UIText(entry.mod) {
				Top = { Pixels = 2 },
				Left = { Pixels = 85 }
			};
			UIText reason = new UIText(entry.reason) {
				Top = { Pixels = 30 },
				Left = { Pixels = 100 + 12 }
			};

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
		this.reloadRequiredExplanationEntries = reloadRequiredExplanationEntries?.OrderBy(x => x.typeOrder).ThenBy(x => x.mod).ToList();
		Main.menuMode = Interface.serverModsDifferMessageID;
		Main.MenuUI.SetState(null); // New SetState code ignores setting to current state, so this is necessary to ensure OnActivate is called.
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
