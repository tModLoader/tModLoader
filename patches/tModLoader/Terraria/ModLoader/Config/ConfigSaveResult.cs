namespace Terraria.ModLoader.Config;

/// <summary>
/// Contains potential results for <see cref="ModConfig.SaveChanges(Terraria.ModLoader.Config.ModConfig, System.Action{string, Microsoft.Xna.Framework.Color}, bool, bool)"/>.
/// </summary>
public enum ConfigSaveResult
{
	/// <summary> The provided config values have been successfully saved. </summary>
	Success,
	/// <summary> The provided config values have not been saved because they would require a reload. </summary>
	NeedsReload,
	/// <summary> The provided config values have been been sent to the server where <see cref="ModConfig.AcceptClientChanges(ModConfig, int, ref Localization.NetworkText)"/> will decide if they should be applied or not. This will be returned when attempting to save a <see cref="ConfigScope.ServerSide"/> config on a multiplayer client. </summary>
	RequestSentToServer
}
