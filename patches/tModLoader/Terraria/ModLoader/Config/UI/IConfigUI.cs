using System;
using Microsoft.Xna.Framework;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI;

// TODO: in future, should include:
// - save, revert, restore
// - ConfigElement API
public interface IConfigUI
{
	int UpdateCount { get; }
	string ConfigElementTooltip { get; set; }

	void SetConfig(ModConfig config, bool openedFromModder = false, Action onClose = null, string scrollToOption = null, bool centerScrolledOption = true) { }

	void OnConfigModified() { }
	void OnServerAcceptedChanges() { }

	void PushConfigPage(UIModConfig.ConfigPage configPage) { }
	bool TryGoBackConfigPage() { return false; }
	void SetMessage(string text, Color color) { }
	void ScrollTo(string scrollToOption, bool centerScrolledOption) { }
	void BlockInput(UIElement dialog) { }
	void UnblockInput() { }
}
