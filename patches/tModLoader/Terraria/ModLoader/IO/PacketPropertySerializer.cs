using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Terraria.ModLoader.IO;

/// <summary>
/// An object that holds a getter and setter for serializing a specific Property within a custom packet without
/// repeated reflection
/// </summary>
internal abstract class PacketPropertySerializer
{
	public abstract Type PropertyType { get; }

	/// <summary>
	/// Creates a new PacketPropertySerializer for a given property
	/// </summary>
	/// <param name="info"></param>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public static PacketPropertySerializer<T> Create<T>(PropertyInfo info) where T : ModCustomPacket {
		var propertyType = info.PropertyType;
		if (!ModPropertySerializer.propertySerializers.TryGetValue(propertyType, out var propertySerializer)) {
			propertySerializer = ModContent.GetContent<ModPropertySerializer>()
				.Where(serializer => serializer.AffectSubClasses)
				.FirstOrDefault(serializer => serializer.Type.IsAssignableFrom(propertyType));
		}

		if (propertySerializer == null) {
			throw new SerializationException(
				$"Can't create serializer for property {info.Name} with type {propertyType.Name} in ModCustomPacket {typeof(T).Name}.");
		}

		var serializer = Activator.CreateInstance(typeof(PacketPropertySerializer<,>)
			.MakeGenericType(typeof(T), propertyType)) as PacketPropertySerializer<T>;
		serializer!.Init(info, propertySerializer);
		return serializer;
	}

	protected abstract void Init(PropertyInfo property, ModPropertySerializer serializer);
}

/// <inheritdoc cref="PacketPropertySerializer"/>
/// <typeparam name="T">The packet type that this is for</typeparam>
internal abstract class PacketPropertySerializer<T> : PacketPropertySerializer where T : ModCustomPacket
{
	/// <summary>
	/// Reads information from the binary reader and stores it in the packet
	/// </summary>
	/// <param name="reader"></param>
	/// <param name="customPacket"></param>
	public virtual void Deserialize(BinaryReader reader, T customPacket) {
	}

	/// <summary>
	/// Gets the information from the packet and writes it into the ModPacket
	/// </summary>
	/// <param name="packet"></param>
	/// <param name="customPacket"></param>
	public virtual void Serialize(ModPacket packet, T customPacket) {
	}
}

/// <inheritdoc cref="PacketPropertySerializer"/>
/// <typeparam name="TPacket">The packet type that this is for</typeparam>
/// <typeparam name="TType">The property type this is for</typeparam>
internal class PacketPropertySerializer<TPacket, TType> : PacketPropertySerializer<TPacket>
	where TPacket : ModCustomPacket
{
	private ModPropertySerializer serializer;
	private Action<TPacket, TType> setter;
	private Func<TPacket, TType> getter;

	public override Type PropertyType => typeof(TType);

	protected sealed override void Init(PropertyInfo property, ModPropertySerializer tSerializer) {
		serializer = tSerializer;
		setter = (Action<TPacket, TType>)
			Delegate.CreateDelegate(typeof(Action<TPacket, TType>), null, property.GetSetMethod(true)!);
		getter = (Func<TPacket, TType>)
			Delegate.CreateDelegate(typeof(Func<TPacket, TType>), null, property.GetGetMethod(true)!);
	}

	public sealed override void Deserialize(BinaryReader reader, TPacket customPacket) =>
		setter(customPacket, serializer.Read<TType>(reader));

	public sealed override void Serialize(ModPacket packet, TPacket customPacket) =>
		serializer.Write(packet, getter(customPacket));
}