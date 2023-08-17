namespace Terraria.ModLoader.Config;

/// <summary>
/// Each ModConfig class has a different scope. Failure to use the correct mode will lead to bugs.
/// </summary>
public enum ConfigScope
{
	/// <summary>
	/// This config is shared between all clients and maintained by the server. Use this for game-play changes that should affect all players the same. ServerSide also covers single player as well.
	/// </summary>
	ServerSide,
	/// <summary>
	/// This config is specific to the client. Use this for personalization options.
	/// </summary>
	ClientSide,
	// PlayerSpecific,
	// WorldSpecific
}
