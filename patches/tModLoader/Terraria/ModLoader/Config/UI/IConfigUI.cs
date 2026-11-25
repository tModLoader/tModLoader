using System;
using Microsoft.Xna.Framework;

namespace Terraria.ModLoader.Config.UI;

// TODO: consider what else should be "public APIs" -- will be hard to change later because all impls are required (unless we supply default impls?)
// - refresh UI? isnt that just OnConfigModified?...
// - getting config elements? might make sense if SetupConfigPage or WrapIt are made virtual + non-static
// - save/revert/restore
// - back subpage
// - unload?
// - scrollto
// - input blocking
// - fields -> everything between `HasDefaultValues` and `public class ConfigPage`
public interface IConfigUI
{
	int UpdateCount { get; }
	string ConfigElementTooltip { get; set; }

	void SetMod(Mod mod, ModConfig config, bool openedFromModder = false, Action onClose = null, string scrollToOption = null, bool centerScrolledOption = true);
	void OnConfigModified();
	void PushConfigPage(UIModConfig.ConfigPage configPage);
	void SetMessage(string text, Color color);
	void HandleOnCloseCallback();
}
