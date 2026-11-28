using System;
using Microsoft.Xna.Framework;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI;

public interface IConfigUI
{
	int UpdateCount { get; }
	string ConfigElementTooltip { get; set; }

	void SetConfig(ModConfig config, bool openedFromModder = false, Action onClose = null, string scrollToOption = null, bool centerScrolledOption = true) { }

	void OnConfigModified() { }
	void OnServerAcceptedChanges() { }

	void PushConfigPage(UIModConfig.ConfigPage configPage) { }
	bool TryGoBackConfigPage() => false;
	void SetMessage(string text, Color color) { }
	void ScrollTo(string scrollToOption, bool centerScrolledOption) { }
	void BlockInput(UIElement dialog) { }
	void UnblockInput() { }

	void SetupConfigPage(UIModConfig.ConfigPage configPage, object config, PropertyFieldWrapper memberInfo) { }
	Tuple<UIElement, UIElement> WrapIt(UIElement parent, ref int top, PropertyFieldWrapper memberInfo, object item, int order, object list = null, Type arrayType = null, int index = -1) => null;
	Tuple<UIElement, UIElement> HandleHeader(UIElement parent, ref int top, ref int order, PropertyFieldWrapper variable) => null;
}
