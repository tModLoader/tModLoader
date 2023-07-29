using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Terraria.ModLoader.IO;

internal abstract class PacketPropertySerializer {
	public abstract Type PropertyType { get; }

	public static PacketPropertySerializer<T> Create<T>(PropertyInfo info) where T : PropertyPacket {
		var propertyType = info.PropertyType;
		if (!TypePropertySerializer.propertySerializers.TryGetValue(propertyType, out var propertySerializer)) {
			propertySerializer = ModContent.GetContent<TypePropertySerializer>()
				.Where(serializer => serializer.AffectSubClasses)
				.FirstOrDefault(serializer => serializer.Type.IsAssignableFrom(propertyType));
		}

		if (propertySerializer == null) {
			throw new SerializationException($"Can't create serializer for property {info.Name} with type {propertyType.FullName} in {nameof(PropertyPacket)} {typeof(T).FullName}.");
		}

		var serializer = (PacketPropertySerializer<T>)Activator.CreateInstance(typeof(PacketPropertySerializer<,>).MakeGenericType(typeof(T), propertyType));
		serializer!.Init(info, propertySerializer);
		return serializer;
	}

	protected abstract void Init(PropertyInfo property, TypePropertySerializer serializer);
}
internal abstract class PacketPropertySerializer<T> : PacketPropertySerializer where T : PropertyPacket {
	public abstract void Deserialize(BinaryReader reader, T customPacket);

	public abstract void Serialize(ModPacket packet, T customPacket);
}
internal sealed class PacketPropertySerializer<TPacket, TType> : PacketPropertySerializer<TPacket> where TPacket : PropertyPacket {
	private TypePropertySerializer serializer;
	private Action<TPacket, TType> setter;
	private Func<TPacket, TType> getter;

	public sealed override Type PropertyType => typeof(TType);

	protected sealed override void Init(PropertyInfo property, TypePropertySerializer tSerializer) {
		serializer = tSerializer;
		setter = property.GetSetMethod(true).CreateDelegate<Action<TPacket, TType>>();
		getter = property.GetGetMethod(true).CreateDelegate<Func<TPacket, TType>>();
	}

	public sealed override void Deserialize(BinaryReader reader, TPacket customPacket) => setter(customPacket, serializer.Read<TType>(reader));

	public sealed override void Serialize(ModPacket packet, TPacket customPacket) => serializer.Write(packet, getter(customPacket));
}
