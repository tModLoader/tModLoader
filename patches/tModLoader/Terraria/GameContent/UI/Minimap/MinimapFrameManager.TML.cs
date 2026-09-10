using ReLogic.Content;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Terraria.GameContent.UI.Minimap;

partial class MinimapFrameManager
{
	private string _activeSelectionConfigKeyOriginal;

	internal void AddModdedFrames(AssetRequestMode mode)
	{
		if (Main.dedServ)
			return;

		foreach (ModMinimapFrame frame in MinimapFrameLoader.ModdedFrames)
			Options[frame.ConfigKey] = frame.CreateInstance(mode);

		SetActiveMinimapFromOriginalConfigKey();
	}

	internal void SetActiveMinimapFromOriginalConfigKey()
	{
		if (Main.dedServ)
			return;

		SetActiveMinimapFromLoadedConfigKey(_activeSelectionConfigKeyOriginal);
		_activeSelectionConfigKeyOriginal = ActiveSelectionConfigKey;
	}

	internal void ResetToVanilla()
	{
		if (Main.dedServ)
			return;

		foreach (ModMinimapFrame frame in MinimapFrameLoader.ModdedFrames)
			Options.Remove(frame.ConfigKey);

		SetActiveMinimapFromLoadedConfigKey(_activeSelectionConfigKeyOriginal);
		_activeSelectionConfigKeyOriginal = ActiveSelectionConfigKey;
	}

	public string ActiveSelectionDisplayName =>
		ModContent.TryFind<ModMinimapFrame>(ActiveSelectionConfigKey, out ModMinimapFrame frame)
			? frame.DisplayName.Value
			: Language.GetTextValue("UI.MinimapFrame_" + ActiveSelectionKeyName);

	private void SetActiveMinimapFromLoadedConfigKey(string configKey)
	{
		ActiveSelectionConfigKey = configKey;
		SetActiveMinimapFromLoadedConfigKey();
	}
}