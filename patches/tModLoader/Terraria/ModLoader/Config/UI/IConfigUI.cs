using System;

namespace Terraria.ModLoader.Config.UI;

public interface IConfigUI
{
	// TODO: add all the methods used to interface with config UI here
	void SetMod(Mod mod, ModConfig config, bool openedFromModder = false, Action onClose = null, string scrollToOption = null, bool centerScrolledOption = true);
}
