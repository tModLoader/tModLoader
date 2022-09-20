using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Terraria.ModLoader.IO;

/// <summary>
/// A class that holds the required info to serialize all the properties of a custom packet type
/// </summary>
internal abstract class CustomPacketSerializer
{
	public Mod Mod { get; private set; }
	public int Type { get; private set; }

	public static CustomPacketSerializer Create(ModCustomPacket packet) {
		var serializationType = typeof(CustomPacketSerializer<>).MakeGenericType(packet.GetType());
		var serializer = ((CustomPacketSerializer)Activator.CreateInstance(serializationType))!;
		serializer.Mod = packet.Mod;
		serializer.Type = packet.type;
		serializer.Init(packet.GetType());

		return serializer;
	}

	protected abstract void Init(Type packetType);

	public abstract int PropertyCount { get; }

	/// <summary>
	/// Reads all information from the binary reader and stores it in the custom packet
	/// </summary>
	/// <param name="reader"></param>
	/// <param name="modCustomPacket"></param>
	public abstract void Deserialize(BinaryReader reader, ModCustomPacket modCustomPacket);

	/// <summary>
	/// Writes all custom information from the custom packet into the ModPacket
	/// </summary>
	/// <param name="packet"></param>
	/// <param name="modCustomPacket"></param>
	public abstract void Serialize(ModPacket packet, ModCustomPacket modCustomPacket);
}

/// <inheritdoc cref="CustomPacketSerializer"/>
/// <typeparam name="T">The ModCustomPacket this serializes</typeparam>
internal class CustomPacketSerializer<T> : CustomPacketSerializer where T : ModCustomPacket
{
	private List<PacketPropertySerializer<T>> serializers;

	public override int PropertyCount => serializers.Count;

	protected override void Init(Type packetType) {
		var properties = packetType.GetProperties(BindingFlags.Public |
		                                          BindingFlags.NonPublic |
		                                          BindingFlags.DeclaredOnly |
		                                          BindingFlags.Instance);

		serializers = properties
			.Where(info => info.CanRead && info.CanWrite)
			.Select(PacketPropertySerializer.Create<T>)
			.OrderBy(serializer => serializer.PropertyType.Name)
			.ToList();
	}

	public override void Deserialize(BinaryReader reader, ModCustomPacket modCustomPacket) {
		if (modCustomPacket is not T) {
			Mod.Logger.Error(
				$"CustomPacket tried to deserialize as wrong type, was {modCustomPacket.GetType().Name} but expected {typeof(T).Name}");
			return;
		}

		serializers.ForEach(s => s.Deserialize(reader, modCustomPacket as T));
	}

	public override void Serialize(ModPacket packet, ModCustomPacket modCustomPacket) {
		if (modCustomPacket is not T) {
			Mod.Logger.Error(
				$"CustomPacket tried to serialize as wrong type, was {modCustomPacket.GetType().Name} but expected {typeof(T).Name}");
			return;
		}

		serializers.ForEach(s => s.Serialize(packet, modCustomPacket as T));
	}
}