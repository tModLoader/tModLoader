using ReLogic.Reflection;

namespace Terraria.ID;

partial class MessageID
{
	/// <summary>
	/// Sent by Clients who wish to change ConfigScope.ServerSide ModConfigs. Clients send Modname, configname, broadcast, and json string.
	/// <br/><br/> Server determines if ModConfig.ReloadRequired and ModConfig.ShouldAcceptClientChanges. Replies with ShouldAcceptClientChanges message if rejected.
	/// <br/><br/> Client receives bool success, message, modname, configname, broadcast, requestor player, if success additionally json, and applies them locally.
	/// </summary>
	public const byte InGameChangeConfig = 249;

	/// <summary>
	/// Contains a netID followed by custom data sent by mods
	/// <br/> Special case netID == -1, is sent by the server in response to SyncMods and contains the netIDs of every non-server only mod
	/// <br/> NetIDs will be sent for no-sync mods, and packets will be ignored if the mod is not installed on the client
	/// </summary>
	public const byte ModPacket = 250;

	/// <summary>
	/// Sent instead of LoadPlayer for non-vanilla clients
	/// <br/> - value of ModNet.AllowVanillaClients is synchronized for common net spec
	/// <br/> - list of all mods loaded on the server with side == Both {name, version, hash, isBrowserSigned}
	/// <br/> The client then enables/disables mods to ensure a matching mod set
	/// <br/> If the client is missing a mod, or has a different hash, it sends ModFile with the name of the mod
	/// <br/> If mod downloading is disabled, or only signed mods are accepted, and the given mod isn't signed, an error message is displayed
	/// <br/> If there are no mods to be downloaded, a reload may be performed if necessary, and then the client returns SyncMods
	/// <br/> when the server receives SyncMods, it sends ModPacket with the netIDs and then LoadPlayer
	/// </summary>
	public const byte SyncMods = 251;

	/// <summary>
	/// The server receives the name of one of the mods sent in SyncMods
	/// <br/><br/> Sends one packet containing the display name and length, then a series of packets containing up to 64k bytes containing the contents of the file
	/// <br/><br/> Client displays the downloading mod UI when it receives the first packet with display name and length
	/// </summary> Once the file is downloaded, the client either sends a request for the next file, or reloads and sends SyncMods
	public const byte ModFile = 252;

	/// <summary> Sent periodically while mods are reloading to keep connection alive. Default timeout is 2 minutes, which a large modpack might need to reload. </summary>
	public const byte KeepAliveDuringModReload = 253;

	public static readonly IdDictionary Search = IdDictionary.Create<MessageID, byte>();
	public static string GetName(int id) => Search.TryGetName(id, out string name) ? name : "Unknown";
}

