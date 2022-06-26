using System;
using System.Collections.Generic;
using System.IO;
using Terraria.ID;
using Terraria.ModLoader.Core;

namespace Terraria.ModLoader.IO;

/// <summary>
/// A definable ModType for a specific set of information that's send-able in multiplayer via packets, and the effect
/// that should happen based on that information
/// </summary>
public abstract class ModCustomPacket : ModType
{
	private static Dictionary<Type, CustomPacketSerializer> packetSerializers;
	private static Dictionary<Mod, List<ModCustomPacket>> packets;
	internal int type;

	public sealed override void Load() {
		packetSerializers ??= new Dictionary<Type, CustomPacketSerializer>();
		packets ??= new Dictionary<Mod, List<ModCustomPacket>>();
		base.Load();
	}

	public sealed override void Unload() {
		base.Unload();
		packetSerializers = null;
		packets = null;
	}

	/// <summary>
	/// Applies the effects of this packet
	/// </summary>
	public abstract void HandlePacket();

	/// <summary>
	/// If you have optional properties, initialize their default values here to avoid packets accidentally using the
	/// values from previously received packets
	/// </summary>
	protected virtual void SetDefaults() {
	}

	protected sealed override void Register() {
		if (!packets.ContainsKey(Mod)) {
			packets[Mod] = new List<ModCustomPacket>();
		}

		packets[Mod].Add(this);
		type = -packets[Mod].Count;
		ModTypeLookup<ModCustomPacket>.Register(this);
	}


	public sealed override void SetupContent() {
		// Doing this here because it's definitely after all the ModPropertySerializers have been loaded
		var packetType = GetType();
		var serialization = CustomPacketSerializer.Create(this);
		packetSerializers[packetType] = serialization;
		Mod.Logger.Info(
			$"Registered CustomPacket {Name} with type {type} and {serialization.PropertyCount} total properties");

		SetStaticDefaults();
	}

	public sealed override void SetStaticDefaults() {
		// No static defaults need to be set for custom packets?
	}

	private void Receive(BinaryReader reader, int sender) {
		if (packetSerializers.TryGetValue(GetType(), out var serializer)) {
			if (ModCompile.DeveloperMode) { // TODO remove these debug comments
				Mod.Logger.Info($"Receiving packet {GetType().Name} on netMode {Main.netMode}");
			}

			SetDefaults();
			serializer.Deserialize(reader, this);
			if (Main.netMode == NetmodeID.Server) {
				Send(-1, sender);
			}

			HandlePacket();

			if (ModCompile.DeveloperMode) {
				Mod.Logger.Info($"Handled packet {GetType().Name} on netMode {Main.netMode}");
			}
		}
		else {
			Mod.Logger.Error($"Failed receiving packet {GetType().Name} because packet info is wrong type");
		}
	}

	/// <summary>
	/// Serializes the information in this ModCustomPacket and then sends it using the same format as <see cref="ModPacket.Send"/>
	/// </summary>
	/// <param name="toClient">The toClient param of <see cref="ModPacket.Send"/></param>
	/// <param name="ignoreClient">The ignoreClient param of <see cref="ModPacket.Send"/></param>
	public void Send(int toClient = -1, int ignoreClient = -1) {
		if (packetSerializers.TryGetValue(GetType(), out var serializer)) {
			if (ModCompile.DeveloperMode) {
				serializer.Mod.Logger.Info(
					$"Sending packet {GetType().Name} on netMode {Main.netMode} with type {serializer.Type}");
			}

			var packet = serializer.Mod.GetPacket();
			packet.Write(serializer.Type);
			serializer.Serialize(packet, this);
			packet.Send(toClient, ignoreClient);
		}
		else {
			Mod?.Logger.Error($"Failed sending packet {GetType().Name} because packet info is wrong type");
		}
	}

	/// <summary>
	/// Causes all clients and the server to handle the results of this packet
	/// <br />
	/// Works for single-player or multiplayer
	/// </summary>
	public void HandleForAll() {
		switch (Main.netMode) {
			case NetmodeID.MultiplayerClient:
				Send(-1, Main.myPlayer);
				break;
			case NetmodeID.Server:
				Send();
				break;
		}

		HandlePacket();
	}


	/// <summary>
	/// Reads an Int32 from the BinaryReader and treats it as the packet's type, Handling it as a ModCustomPacket from
	/// the given Mod and returning true if it is negative, otherwise returning false and passing the packetType out
	/// </summary>
	/// <param name="mod">The Mod that the ModCustomPacket would be a part of</param>
	/// <param name="reader">The BinaryReader read in <see cref="Mod.HandlePacket"/></param>
	/// <param name="whoAmI">The int whoAmI in <see cref="Mod.HandlePacket"/></param>
	/// <param name="packetType">The packet type, > 0 when this returns false</param>
	/// <returns>Whether the packetType was for a ModCustomPacket</returns>
	public static bool Handle(Mod mod, BinaryReader reader, int whoAmI, out int packetType) {
		packetType = reader.ReadInt32();
		if (packetType < 0 && packets.TryGetValue(mod, out var customPackets)) {
			var packetIndex = -packetType - 1;
			if (packetIndex < customPackets.Count) {
				customPackets[packetIndex].Receive(reader, whoAmI);
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Reads an Int32 from the BinaryReader and treats it as the packet's type, Handling it as a ModCustomPacket from
	/// the given Mod
	/// </summary>
	/// <param name="mod">The Mod that the ModCustomPacket would be a part of</param>
	/// <param name="reader">The BinaryReader read in <see cref="Mod.HandlePacket"/></param>
	/// <param name="whoAmI">The int whoAmI in <see cref="Mod.HandlePacket"/></param>
	public static void Handle(Mod mod, BinaryReader reader, int whoAmI) {
		Handle(mod, reader, whoAmI, out _);
	}

	/// <summary>
	/// Reads an Int32 from the BinaryReader and treats it as the packet's type, Handling it as a ModCustomPacket from
	/// the given Mod and returning true if it is negative, otherwise returning false and passing the packetType out as
	/// the given Enum
	/// </summary>
	/// <param name="mod">The Mod that the ModCustomPacket would be a part of</param>
	/// <param name="reader">The BinaryReader read in <see cref="Mod.HandlePacket"/></param>
	/// <param name="whoAmI">The int whoAmI in <see cref="Mod.HandlePacket"/></param>
	/// <param name="packetType">The packet type, > 0 when this returns false</param>
	/// <returns>Whether the packetType was for a ModCustomPacket</returns>
	public static bool Handle<T>(Mod mod, BinaryReader reader, int whoAmI, out T packetType) where T : Enum {
		var result = Handle(mod, reader, whoAmI, out var packetId);
		if (result) {
			packetType = default;
		}
		else {
			packetType = (T)(packetId as object);
		}

		return result;
	}
}