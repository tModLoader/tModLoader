using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

#pragma warning disable CA1822 // Mark members as static
#nullable enable

namespace Terraria.GameContent.UI.States;

public partial class UIManageControls : UIState
{
	private const string ResetModKeybinds = "ResetModKeybinds";
	private const string ClearModKeybinds = "ClearModKeybinds";
	private const int TmlBindingGroupId = 5;

	private static readonly string[] tmlBindings = new[] {
		ResetModKeybinds,
		ClearModKeybinds,
	};
	private static readonly List<string> _ModBindings = new();
	private static readonly List<string> _ModNames = new();

	static UIManageControls()
	{
		_BindingsFullLine.AddRange(tmlBindings);
		_BindingsHalfSingleLine.AddRange(tmlBindings);
	}

	private void OnAssembleBindPanels()
	{
		// Handle mod keybinds

		_BindingsFullLine.RemoveAll(x => x.Contains('/')); // Removes Mod keybinds?
		_ModBindings.Clear();
		_ModNames.Clear();

		Mod? currentMod = null;

		foreach (var keybind in KeybindLoader.Keybinds) {
			if (currentMod != keybind.Mod) {
				currentMod = keybind.Mod;
				_ModBindings.Add(keybind.Mod.DisplayName);
				_ModNames.Add(keybind.Mod.DisplayName);
			}

			_ModBindings.Add(keybind.FullName);
			_BindingsFullLine.Add(keybind.FullName);
		}

		_ModBindings.AddRange(tmlBindings);
	}

	private void AddModBindingGroups()
	{
		_bindsKeyboard.Add(CreateBindingGroup(TmlBindingGroupId, _ModBindings, InputMode.Keyboard));
		_bindsGamepad.Add(CreateBindingGroup(TmlBindingGroupId, _ModBindings, InputMode.XBoxGamepad));
		_bindsKeyboardUI.Add(CreateBindingGroup(TmlBindingGroupId, _ModBindings, InputMode.KeyboardUI));
		_bindsGamepadUI.Add(CreateBindingGroup(TmlBindingGroupId, _ModBindings, InputMode.XBoxGamepadUI));
	}

	private UIElement? HandlePanelCreation(string bind, InputMode currentInputMode, Color color)
	{
		switch (bind) {
			case ResetModKeybinds: {
				var result = new UIKeybindingSimpleListItem(() => Lang.menu[86].Value, color);

				result.OnLeftClick += delegate (UIMouseEvent evt, UIElement listeningElement) {
					string copyableProfileName = GetCopyableProfileName();
					PlayerInput.CurrentProfile.CopyModKeybindSettingsFrom(PlayerInput.OriginalProfiles[copyableProfileName], currentInputMode);
				};

				return result;
			}
			case ClearModKeybinds: {
				var result = new UIKeybindingSimpleListItem(() => Language.GetTextValue("tModLoader.ModConfigClear"), color);

				result.OnLeftClick += delegate (UIMouseEvent evt, UIElement listeningElement) {
					foreach (var modKeybind in KeybindLoader.Keybinds) {
						PlayerInput.CurrentProfile.InputModes[currentInputMode].KeyStatus[modKeybind.FullName].Clear();
					}
				};

				return result;
			}
			default: {
				if (!_ModBindings.Contains(bind))
					return null;

				string defaultKey = KeybindLoader.modKeybinds[bind].DefaultBinding;
				var container = new UIElement();

				var left = new UIKeybindingListItem(bind, currentInputMode, color);
				left.Width.Precent = 0.58f;
				left.Height.Precent = 1f;

				container.Append(left);

				// TODO: Clear instead of Reset to Defaults if no default specified.
				var right = new UIKeybindingSimpleListItem(() => Lang.menu[86].Value + $" ({defaultKey})", color);

				right.OnLeftClick += delegate (UIMouseEvent evt, UIElement listeningElement) {
					string copyableProfileName = GetCopyableProfileName();
					PlayerInput.CurrentProfile.CopyIndividualModKeybindSettingsFrom(PlayerInput.OriginalProfiles[copyableProfileName], currentInputMode, bind);
				};

				right.Left.Precent = 0.6f;
				right.Width.Precent = 0.4f;
				right.Height.Precent = 1f;

				container.Append(right);

				return container;
			};
		}
	}
}
