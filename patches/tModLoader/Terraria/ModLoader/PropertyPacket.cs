using System;
using System.Collections.Generic;
using System.IO;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader;

public abstract class PropertyPacket : ModType {
	#region Internal
	private sealed class Loader : ILoadable {
		void ILoadable.Load(Mod mod) {
		}

		void ILoadable.Unload() {
			packetSerializers.Clear();
		}
	}

	private static readonly Dictionary<Type, PropertyPacketSerializer> packetSerializers = new();

	public sbyte Type { get; private set; }

	protected sealed override void Register() {
		Mod.packets.Add(this);
		Type = (sbyte)-Mod.packets.Count;

		ModTypeLookup<PropertyPacket>.Register(this);
	}

	public sealed override void SetupContent() {
		var serializer = PropertyPacketSerializer.Create(this);
		packetSerializers[GetType()] = serializer;

		Mod.Logger.Info($"Registered {nameof(PropertyPacket)} {Name} with type {Type} and {serializer.PropertyCount} total properties");
	}

	public sealed override void SetStaticDefaults() {
	}
	#endregion

	public abstract void HandlePacket();

	protected virtual void SetDefaults() {
	}

	private void Receive(BinaryReader reader, int sender) {
		if (packetSerializers.TryGetValue(GetType(), out var serializer)) {
			SetDefaults();
			serializer.Deserialize(reader, this);
			if (Main.netMode == NetmodeID.Server) {
				Send(-1, sender);
			}

			HandlePacket();
		}
		else {
			Mod.Logger.Error($"Failed receiving packet {GetType().FullName} because packet info is wrong type");
		}
	}

	public void Send(int toClient = -1, int ignoreClient = -1) {
		if (packetSerializers.TryGetValue(GetType(), out var serializer)) {
			var packet = serializer.Mod.GetPacket();
			packet.Write((sbyte)serializer.Type);
			serializer.Serialize(packet, this);
			packet.Send(toClient, ignoreClient);
		}
		else {
			Mod.Logger.Error($"Failed sending packet {GetType().FullName} because packet info is wrong type");
		}
	}

	public static void Handle(Mod mod, BinaryReader reader, int whoAmI) => Handle(mod, reader, whoAmI, out _);
	public static bool Handle(Mod mod, BinaryReader reader, int whoAmI, out int packetType) {
		packetType = reader.ReadSByte();
		if (packetType < 0) {
			int packetIndex = -packetType - 1;
			if (packetIndex < mod.packets.Count) {
				mod.packets[packetIndex].Receive(reader, whoAmI);
				return true;
			}
		}

		return false;
	}
	public static bool Handle<T>(Mod mod, BinaryReader reader, int whoAmI, out T packetType) where T : Enum {
		bool result = Handle(mod, reader, whoAmI, out int packetId);
		packetType = result ? default : (T)(object)packetId;
		return result;
	}

	public void HandleForAll() {
		Send(ignoreClient: Main.netMode switch {
			NetmodeID.MultiplayerClient => Main.myPlayer,
			_ => -1
		});

		HandlePacket();
	}
}
