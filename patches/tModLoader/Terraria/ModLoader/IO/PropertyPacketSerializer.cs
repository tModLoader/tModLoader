using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Terraria.ModLoader.IO;

internal abstract class PropertyPacketSerializer {
	public Mod Mod { get; private set; }
	public int Type { get; private set; }

	public abstract int PropertyCount { get; }

	public static PropertyPacketSerializer Create(PropertyPacket propertyPacket) {
		var serializationType = typeof(PropertyPacketSerializer<>).MakeGenericType(propertyPacket.GetType());
		var serializer = ((PropertyPacketSerializer)Activator.CreateInstance(serializationType))!;
		serializer.Mod = propertyPacket.Mod;
		serializer.Type = propertyPacket.Type;
		serializer.Init(propertyPacket.GetType());
		return serializer;
	}

	protected abstract void Init(Type packetType);

	public abstract void Serialize(ModPacket packet, PropertyPacket propertyPacket);
	public abstract void Deserialize(BinaryReader reader, PropertyPacket propertyPacket);
}
internal sealed class PropertyPacketSerializer<T> : PropertyPacketSerializer where T : PropertyPacket {
	private List<PacketPropertySerializer<T>> serializers;

	public sealed override int PropertyCount => serializers.Count;

	protected sealed override void Init(Type packetType) {
		serializers = packetType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance)
			.Where(info => info.CanRead && info.CanWrite)
			.Select(PacketPropertySerializer.Create<T>)
			.OrderBy(serializer => serializer.PropertyType.Name)
			.ToList();
	}

	public sealed override void Serialize(ModPacket packet, PropertyPacket propertyPacket) {
		serializers.ForEach(s => s.Serialize(packet, (T)propertyPacket));
	}

	public sealed override void Deserialize(BinaryReader reader, PropertyPacket propertyPacket) {
		serializers.ForEach(s => s.Deserialize(reader, (T)propertyPacket));
	}
}
