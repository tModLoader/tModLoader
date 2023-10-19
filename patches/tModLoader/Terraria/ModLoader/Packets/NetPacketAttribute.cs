using System;

namespace Terraria.ModLoader.Packets;

[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class NetPacketAttribute : Attribute {
	public Type ModType { get; }
	public bool AutoSerialize { get; set; } = true;

	public NetPacketAttribute(Type modType) {
		ModType = modType;
	}
}
