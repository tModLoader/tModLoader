using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace Terraria.ModLoader.IO;

/// <summary>
/// A ModType representing a way to serialize and deserialize a certain type between a BinaryReader and a ModPacket
/// <br/>
/// Mods can add their own custom property serializers if they want
/// </summary>
public abstract class ModPropertySerializer : ModType
{
	internal static Dictionary<Type, ModPropertySerializer> propertySerializers;

	/// <summary>
	/// The type of the property
	/// </summary>
	public abstract Type Type { get; }

	public sealed override void Load() {
		propertySerializers ??= new Dictionary<Type, ModPropertySerializer>();
	}

	/// <summary>
	/// Signifies that this is capable of serializing not just it's exact matching type, but types that descend from it
	/// as well, e.g. ModPlayer
	/// </summary>
	public virtual bool AffectSubClasses => false;

	public sealed override void SetupContent() {
	}

	public override void SetStaticDefaults() {
	}

	public sealed override void Unload() {
		propertySerializers = null;
	}

	protected sealed override void Register() {
		if (propertySerializers.ContainsKey(Type)) {
			Mod.Logger.Warn($"A ModPropertySerializer for type {Type.Name} already exists, not overwriting");
		}
		else {
			propertySerializers[Type] = this;
		}
	}

	/// <summary>
	/// Reads/parses the desired type from the BinaryReader
	/// </summary>
	public abstract T Read<T>(BinaryReader reader);

	/// <summary>
	/// Parses/writes the desired type to the ModPacket
	/// </summary>
	public abstract void Write<T>(ModPacket packet, T value);

	/// <summary>
	/// Gets the element of the array at the given index, or null/default if the index is negative or out of bounds
	/// </summary>
	protected static T GetOrDefault<T>(T[] array, int index) =>
		index < 0 || index >= array.Length ? default : array[index];
}

public abstract class ModPropertySerializer<TType> : ModPropertySerializer
{
	public override Type Type => typeof(TType);

	public override T Read<T>(BinaryReader reader) {
		return Read(reader) switch {
			T t => t,
			null => default,
			_ => throw new SerializationException(
				$"Result was of type {Read(reader).GetType().Name} when expected {typeof(T).Name} in serializer {GetType().Name}")
		};
	}

	public override void Write<T>(ModPacket packet, T value) {
		switch (value) {
			case TType t:
				Write(packet, t);
				return;
			case null:
				Write(packet, default);
				return;
			default:
				throw new SerializationException(
					$"Could not write value of type {value.GetType().Name} to packet when expected {typeof(TType).Name} in serializer {GetType().Name}");
		}
	}

	/// <inheritdoc cref="Read{T}"/>
	protected abstract TType Read(BinaryReader reader);
	/// <inheritdoc cref="Write{T}"/>
	protected abstract void Write(ModPacket packet, TType value);
}

#region Simple Types

public class BytePropertySerializer : ModPropertySerializer<byte>
{
	protected override byte Read(BinaryReader reader) => reader.ReadByte();
	protected override void Write(ModPacket packet, byte value) => packet.Write(value);
}

public class BoolPropertySerializer : ModPropertySerializer<bool>
{
	protected override bool Read(BinaryReader reader) => reader.ReadBoolean();
	protected override void Write(ModPacket packet, bool value) => packet.Write(value);
}

public class ShortPropertySerializer : ModPropertySerializer<short>
{
	protected override short Read(BinaryReader reader) => reader.ReadInt16();
	protected override void Write(ModPacket packet, short value) => packet.Write(value);
}

public class IntPropertySerializer : ModPropertySerializer<int>
{
	protected override int Read(BinaryReader reader) => reader.ReadInt32();
	protected override void Write(ModPacket packet, int value) => packet.Write(value);
}

public class LongPropertySerializer : ModPropertySerializer<long>
{
	protected override long Read(BinaryReader reader) => reader.ReadInt64();
	protected override void Write(ModPacket packet, long value) => packet.Write(value);
}

public class StringPropertySerializer : ModPropertySerializer<string>
{
	protected override string Read(BinaryReader reader) => reader.ReadString();
	protected override void Write(ModPacket packet, string value) => packet.Write(value ?? "");
}

public class CharPropertySerializer : ModPropertySerializer<char>
{
	protected override char Read(BinaryReader reader) => reader.ReadChar();
	protected override void Write(ModPacket packet, char value) => packet.Write(value);
}

#endregion

#region Complex Types

public class PlayerPropertySerializer : ModPropertySerializer<Player>
{
	protected override Player Read(BinaryReader reader) => GetOrDefault(Main.player, reader.ReadInt32());

	protected override void Write(ModPacket packet, Player value) => packet.Write(value?.whoAmI ?? -1);
}

public class NPCPropertySerializer : ModPropertySerializer<NPC>
{
	protected override NPC Read(BinaryReader reader) => GetOrDefault(Main.npc, reader.ReadInt32());
	protected override void Write(ModPacket packet, NPC value) => packet.Write(value?.whoAmI ?? -1);
}

public class ProjectilePropertySerializer : ModPropertySerializer<Projectile>
{
	protected override Projectile Read(BinaryReader reader) => GetOrDefault(Main.projectile, reader.ReadInt32());
	protected override void Write(ModPacket packet, Projectile value) => packet.Write(value?.whoAmI ?? -1);
}

/// <summary>
/// This is maybe a little overkill for ease of serialization, but there's no harm to it
/// </summary>
public class ModPlayerPropertySerializer : ModPropertySerializer<ModPlayer>
{
	public override bool AffectSubClasses => true;

	protected override ModPlayer Read(BinaryReader reader) {
		var playerIndex = reader.ReadInt32();
		if (playerIndex == -1) return null;

		var modPlayerIndex = reader.ReadUInt16();

		return Main.player[playerIndex].modPlayers[modPlayerIndex];
	}

	protected override void Write(ModPacket packet, ModPlayer value) {
		packet.Write(value?.Player.whoAmI ?? -1);
		if (value != null) {
			packet.Write(value.index);
		}
	}
}

public class ModProjectilePropertySerializer : ModPropertySerializer<ModProjectile>
{
	public override bool AffectSubClasses => true;

	protected override ModProjectile Read(BinaryReader reader) =>
		GetOrDefault(Main.projectile, reader.ReadInt32())?.ModProjectile;

	protected override void Write(ModPacket packet, ModProjectile value) =>
		packet.Write(value?.Projectile.whoAmI ?? -1);
}

public class ModNPCPropertySerializer : ModPropertySerializer<ModNPC>
{
	public override bool AffectSubClasses => true;

	protected override ModNPC Read(BinaryReader reader) => GetOrDefault(Main.npc, reader.ReadInt32())?.ModNPC;

	protected override void Write(ModPacket packet, ModNPC value) => packet.Write(value?.NPC.whoAmI ?? -1);
}

#endregion