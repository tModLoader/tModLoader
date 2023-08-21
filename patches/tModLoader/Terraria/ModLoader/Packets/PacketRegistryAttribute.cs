using System;

namespace Terraria.ModLoader.Packets;

[AttributeUsage(AttributeTargets.Class)]
public sealed class PacketRegistryAttribute : Attribute {
	public PacketRegistryAttribute() {
	}
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class NetPacketIdOfAttribute : Attribute {
	public Type Type { get; }

	public NetPacketIdOfAttribute(Type type) {
		Type = type;
	}
}
