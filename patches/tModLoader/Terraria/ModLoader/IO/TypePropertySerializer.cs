using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Terraria.ModLoader.IO;

public abstract class TypePropertySerializer : ModType {
	private sealed class Loader : ILoadable {
		void ILoadable.Load(Mod mod) {
		}

		void ILoadable.Unload() {
			propertySerializers.Clear();
		}
	}

	internal static Dictionary<Type, TypePropertySerializer> propertySerializers = new();

	public abstract Type Type { get; }

	public virtual bool AffectSubClasses => false;

	protected sealed override void Register() {
		ref var serializer = ref CollectionsMarshal.GetValueRefOrAddDefault(propertySerializers, Type, out bool exists);
		if (exists) {
			Mod.Logger.Warn($"A {nameof(TypePropertySerializer)} for type {Type.FullName} already exists, not overwriting");
		}
		else {
			serializer = this;
		}
	}

	public sealed override void SetupContent() {
	}

	public sealed override void SetStaticDefaults() {
	}

	public abstract T Read<T>(BinaryReader reader);

	public abstract void Write<T>(ModPacket packet, T value);
}
public abstract class TypePropertySerializer<TType> : TypePropertySerializer {
	public sealed override Type Type => typeof(TType);

	public sealed override T Read<T>(BinaryReader reader) {
		return Read(reader) switch {
			T t => t,
			null => default,
			_ => throw new SerializationException($"Result was of type {Read(reader).GetType().FullName} when expected {typeof(T).FullName} in serializer {GetType().FullName}")
		};
	}

	public sealed override void Write<T>(ModPacket packet, T value) {
		Write(packet, value switch {
			TType t => t,
			null => default,
			_ => throw new SerializationException($"Could not write value of type {value.GetType().FullName} to packet when expected {typeof(TType).FullName} in serializer {GetType().FullName}")
		});
	}

	protected abstract TType Read(BinaryReader reader);

	protected abstract void Write(ModPacket packet, TType value);
}

#region Primitive (aka simple) types
public sealed class BitsBytePropertySerializer : TypePropertySerializer<BitsByte> {
	protected sealed override BitsByte Read(BinaryReader reader) => reader.ReadByte();
	protected sealed override void Write(ModPacket packet, BitsByte value) => packet.Write(value);
}
public sealed class SBytePropertySerializer : TypePropertySerializer<sbyte> {
	protected sealed override sbyte Read(BinaryReader reader) => reader.ReadSByte();
	protected sealed override void Write(ModPacket packet, sbyte value) => packet.Write(value);
}
public sealed class BytePropertySerializer : TypePropertySerializer<byte> {
	protected sealed override byte Read(BinaryReader reader) => reader.ReadByte();
	protected sealed override void Write(ModPacket packet, byte value) => packet.Write(value);
}
public sealed class Int16PropertySerializer : TypePropertySerializer<short> {
	protected sealed override short Read(BinaryReader reader) => reader.ReadInt16();
	protected sealed override void Write(ModPacket packet, short value) => packet.Write(value);
}
public sealed class UInt16PropertySerializer : TypePropertySerializer<ushort> {
	protected sealed override ushort Read(BinaryReader reader) => reader.ReadUInt16();
	protected sealed override void Write(ModPacket packet, ushort value) => packet.Write(value);
}
public sealed class Int32PropertySerializer : TypePropertySerializer<int> {
	protected sealed override int Read(BinaryReader reader) => reader.ReadInt32();
	protected sealed override void Write(ModPacket packet, int value) => packet.Write(value);
}
public sealed class UInt32PropertySerializer : TypePropertySerializer<uint> {
	protected sealed override uint Read(BinaryReader reader) => reader.ReadUInt32();
	protected sealed override void Write(ModPacket packet, uint value) => packet.Write(value);
}
public sealed class Int64PropertySerializer : TypePropertySerializer<long> {
	protected sealed override long Read(BinaryReader reader) => reader.ReadInt64();
	protected sealed override void Write(ModPacket packet, long value) => packet.Write(value);
}
public sealed class UInt64PropertySerializer : TypePropertySerializer<ulong> {
	protected sealed override ulong Read(BinaryReader reader) => reader.ReadUInt64();
	protected sealed override void Write(ModPacket packet, ulong value) => packet.Write(value);
}
public sealed class SinglePropertySerializer : TypePropertySerializer<float> {
	protected sealed override float Read(BinaryReader reader) => reader.ReadSingle();
	protected sealed override void Write(ModPacket packet, float value) => packet.Write(value);
}
public sealed class DoublePropertySerializer : TypePropertySerializer<double> {
	protected sealed override double Read(BinaryReader reader) => reader.ReadDouble();
	protected sealed override void Write(ModPacket packet, double value) => packet.Write(value);
}
public sealed class DecimalPropertySerializer : TypePropertySerializer<decimal> {
	protected sealed override decimal Read(BinaryReader reader) => reader.ReadDecimal();
	protected sealed override void Write(ModPacket packet, decimal value) => packet.Write(value);
}
public sealed class CharPropertySerializer : TypePropertySerializer<char> {
	protected sealed override char Read(BinaryReader reader) => reader.ReadChar();
	protected sealed override void Write(ModPacket packet, char value) => packet.Write(value);
}
public sealed class StringPropertySerializer : TypePropertySerializer<string> {
	protected sealed override string Read(BinaryReader reader) => reader.ReadString();
	protected sealed override void Write(ModPacket packet, string value) => packet.Write(value);
}
#endregion

#region Complex Types
public sealed class PlayerPropertySerializer : TypePropertySerializer<Player> {
	protected sealed override Player Read(BinaryReader reader) {
		int index = reader.ReadByte();
		if (index == Main.maxPlayers)
			return default(Player);
		return Main.player[index];
	}

	protected sealed override void Write(ModPacket packet, Player value) => packet.Write(value?.whoAmI ?? Main.maxPlayers);
}
public sealed class NPCPropertySerializer : TypePropertySerializer<NPC> {
	protected sealed override NPC Read(BinaryReader reader) {
		int index = reader.ReadByte();
		if (index == Main.maxNPCs)
			return default(NPC);
		return Main.npc[index];
	}

	protected sealed override void Write(ModPacket packet, NPC value) => packet.Write(value?.whoAmI ?? Main.maxNPCs);
}
public sealed class ProjectilePropertySerializer : TypePropertySerializer<Projectile> {
	protected sealed override Projectile Read(BinaryReader reader) {
		int index = reader.ReadUInt16();
		if (index == Main.maxProjectiles)
			return default(Projectile);
		return Main.projectile[index];
	}

	protected sealed override void Write(ModPacket packet, Projectile value) => packet.Write(value?.whoAmI ?? Main.maxProjectiles);
}
#endregion