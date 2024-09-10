using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ReLogic.Reflection;

namespace Terraria.ID;

partial class MessageID
{
	// Sent by Clients who wish to change ConfigScope.ServerSide ModConfigs. Clients send Modname, configname, and json string.
	// Server determines if ModConfig.ReloadRequired and ModConfig.ShouldAcceptClientChanges. Replies with ShouldAcceptClientChanges message if rejected.
	// Client receives bool success, message, if success, additionally modname, configname, json and applies them locally.
	public const byte InGameChangeConfig = 249;

	// Contains a netID followed by custom data sent by mods
	// Special case netID == -1, is sent by the server in response to SyncMods and contains the netIDs of every non-server only mod
	// NetIDs will be sent for no-sync mods, and packets will be ignored if the mod is not installed on the client
	public const byte ModPacket = 250;

	// Sent instead of LoadPlayer for non-vanilla clients {
	//    value of ModNet.AllowVanillaClients is synchronized for common net spec
	//    list of all mods loaded on the server with side == Both {name, version, hash, isBrowserSigned}
	// }
	// The client then enables/disables mods to ensure a matching mod set
	// If the client is missing a mod, or has a different hash, it sends ModFile with the name of the mod
	// If mod downloading is disabled, or only signed mods are accepted, and the given mod isn't signed, an error message is displayed
	// If there are no mods to be downloaded, a reload may be performed if necessary, and then the client returns SyncMods
	// when the server receives SyncMods, it sends ModPacket with the netIDs and then LoadPlayer
	public const byte SyncMods = 251;

	// The server receives the name of one of the mods sent in SyncMods
	// Sends one packet containing the display name and length, then a series of packets containing up to 64k bytes containing the contents of the file
	// Client displays the downloading mod UI when it receives the first packet with display name and length
	// Once the file is downloaded, the client either sends a request for the next file, or reloads and sends SyncMods
	public const byte ModFile = 252;

	// Sent periodically while mods are reloading to keep connection alive. Default timeout is 2 minutes, which a large modpack might need to reload.
	public const byte KeepAliveDuringModReload = 253;

	public static readonly IdDictionary Search = IdDictionary.Create<MessageID, byte>();
	public static string GetName(int id) => Search.TryGetName(id, out string name) ? name : "Unknown";
}

