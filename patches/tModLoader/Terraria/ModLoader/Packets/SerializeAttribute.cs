using System;

namespace Terraria.ModLoader.Packets;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public sealed class SerializeAttribute : Attribute {
	public SerializeAttribute() {
	}
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public sealed class IgnoreAttribute : Attribute {
	public IgnoreAttribute() {
	}
}
