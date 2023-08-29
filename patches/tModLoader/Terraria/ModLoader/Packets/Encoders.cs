using System.IO;
using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Terraria.ModLoader.Packets;

#region Primitive Types
public readonly struct BooleanEncoder : INetEncoder<bool>
{
	public readonly void Write(ModPacket packet, bool value) => packet.Write(value);
	public readonly bool Read(BinaryReader reader) => reader.ReadBoolean();
}

public readonly struct SByteEncoder : INetEncoder<sbyte>
{
	public readonly void Write(ModPacket packet, sbyte value) => packet.Write(value);
	public readonly sbyte Read(BinaryReader reader) => reader.ReadSByte();
}
public readonly struct ByteEncoder : INetEncoder<byte>
{
	public readonly void Write(ModPacket packet, byte value) => packet.Write(value);
	public readonly byte Read(BinaryReader reader) => reader.ReadByte();
}

public readonly struct ShortEncoder : INetEncoder<short>
{
	public readonly void Write(ModPacket packet, short value) => packet.Write(value);
	public readonly short Read(BinaryReader reader) => reader.ReadInt16();
}
public readonly struct UShortEncoder : INetEncoder<ushort>
{
	public readonly void Write(ModPacket packet, ushort value) => packet.Write(value);
	public readonly ushort Read(BinaryReader reader) => reader.ReadUInt16();
}

public readonly struct IntEncoder : INetEncoder<int>
{
	public readonly void Write(ModPacket packet, int value) => packet.Write(value);
	public readonly int Read(BinaryReader reader) => reader.ReadInt32();
}
public readonly struct UIntEncoder : INetEncoder<uint>
{
	public readonly void Write(ModPacket packet, uint value) => packet.Write(value);
	public readonly uint Read(BinaryReader reader) => reader.ReadUInt32();
}

public readonly struct LongEncoder : INetEncoder<long>
{
	public readonly void Write(ModPacket packet, long value) => packet.Write(value);
	public readonly long Read(BinaryReader reader) => reader.ReadInt64();
}
public readonly struct ULongEncoder : INetEncoder<ulong>
{
	public readonly void Write(ModPacket packet, ulong value) => packet.Write(value);
	public readonly ulong Read(BinaryReader reader) => reader.ReadUInt64();
}

public readonly struct HalfEncoder : INetEncoder<Half>
{
	public readonly void Write(ModPacket packet, Half value) => packet.Write(value);
	public readonly Half Read(BinaryReader reader) => reader.ReadHalf();
}
public readonly struct SingleEncoder : INetEncoder<float>
{
	public readonly void Write(ModPacket packet, float value) => packet.Write(value);
	public readonly float Read(BinaryReader reader) => reader.ReadSingle();
}
public readonly struct DoubleEncoder : INetEncoder<double>
{
	public readonly void Write(ModPacket packet, double value) => packet.Write(value);
	public readonly double Read(BinaryReader reader) => reader.ReadDouble();
}
public readonly struct DecimalEncoder : INetEncoder<decimal>
{
	public readonly void Write(ModPacket packet, decimal value) => packet.Write(value);
	public readonly decimal Read(BinaryReader reader) => reader.ReadDecimal();
}

public readonly struct CharEncoder : INetEncoder<char>
{
	public readonly void Write(ModPacket packet, char value) => packet.Write(value);
	public readonly char Read(BinaryReader reader) => reader.ReadChar();
}
public readonly struct StringEncoder : INetEncoder<string>
{
	public readonly void Write(ModPacket packet, string value) => packet.Write(value);
	public readonly string Read(BinaryReader reader) => reader.ReadString();
}
#endregion

#region 7-Bit encoders
public readonly struct Int32Bit7Encoder : INetEncoder<int>
{
	public readonly void Write(ModPacket packet, int value) => packet.Write7BitEncodedInt(value);
	public readonly int Read(BinaryReader reader) => reader.Read7BitEncodedInt();
}
public readonly struct Int64Bit7Encoder : INetEncoder<long>
{
	public readonly void Write(ModPacket packet, long value) => packet.Write7BitEncodedInt64(value);
	public readonly long Read(BinaryReader reader) => reader.Read7BitEncodedInt64();
}
#endregion

#region Enum
public readonly unsafe struct EnumEncoder<T> : INetEncoder<T> where T : unmanaged, Enum
{
	public readonly void Write(ModPacket packet, T value)
	{
		if (sizeof(T) == sizeof(byte)) {
			packet.Write(*(byte*)&value);
		}
		else if (sizeof(T) == sizeof(ushort)) {
			packet.Write(*(ushort*)&value);
		}
		else if (sizeof(T) == sizeof(uint)) {
			packet.Write(*(uint*)&value);
		}
		else if (sizeof(T) == sizeof(ulong)) {
			packet.Write(*(ulong*)&value);
		}
	}

	public readonly T Read(BinaryReader reader)
	{
		if (sizeof(T) == sizeof(byte)) {
			byte value = reader.ReadByte();
			return *(T*)&value;
		}
		else if (sizeof(T) == sizeof(ushort)) {
			ushort value = reader.ReadUInt16();
			return *(T*)&value;
		}
		else if (sizeof(T) == sizeof(uint)) {
			uint value = reader.ReadUInt32();
			return *(T*)&value;
		}
		else if (sizeof(T) == sizeof(ulong)) {
			ulong value = reader.ReadUInt64();
			return *(T*)&value;
		}
		else {
			return default;
		}
	}
}
#endregion

#region Vector2
public readonly struct Vector2Encoder : INetEncoder<Vector2>
{
	public readonly void Write(ModPacket packet, Vector2 value) => packet.WriteVector2(value);
	public readonly Vector2 Read(BinaryReader reader) => reader.ReadVector2();
}
public readonly struct PackedVector2Encoder : INetEncoder<Vector2>
{
	public readonly void Write(ModPacket packet, Vector2 value) => packet.WritePackedVector2(value);
	public readonly Vector2 Read(BinaryReader reader) => reader.ReadPackedVector2();
}
#endregion

#region Color
public readonly struct RGBAColorEncoder : INetEncoder<Color>
{
	public readonly void Write(ModPacket packet, Color value) => packet.Write(value.PackedValue);
	public readonly Color Read(BinaryReader reader) => new Color {
		PackedValue = reader.ReadUInt32()
	};
}
public readonly struct RGBColorEncoder : INetEncoder<Color>
{
	public readonly void Write(ModPacket packet, Color value) => packet.WriteRGB(value);
	public readonly Color Read(BinaryReader reader) => reader.ReadRGB();
}
#endregion

#region Array and List
public readonly struct ArrayEncoder<TType, TEncoder> : INetEncoder<TType[]> where TEncoder : INetEncoder
{
	public readonly void Write(ModPacket packet, TType[] value)
	{
		packet.Write7BitEncodedInt64(value.LongLength);

		var encoder = default(TEncoder);
		foreach (var element in value.AsSpan()) {
			encoder.Write(packet, element);
		}
	}

	public TType[] Read(BinaryReader reader)
	{
		var array = new TType[reader.Read7BitEncodedInt64()];

		var encoder = default(TEncoder);
		for (long i = 0; i < array.LongLength; i++) {
			array[i] = encoder.Read<TType>(reader);
		}

		return array;
	}
}
public readonly struct ListEncoder<TType, TEncoder> : INetEncoder<List<TType>> where TEncoder : INetEncoder
{
	public readonly void Write(ModPacket packet, List<TType> value)
	{
		packet.Write7BitEncodedInt(value.Count);

		var encoder = default(TEncoder);
		foreach (var element in CollectionsMarshal.AsSpan(value)) {
			encoder.Write(packet, element);
		}
	}

	public readonly List<TType> Read(BinaryReader reader)
	{
		int count = reader.Read7BitEncodedInt();
		var list = new List<TType>(count);

		var encoder = default(TEncoder);
		for (int i = 0; i < count; i++) {
			list.Add(encoder.Read<TType>(reader));
		}

		return list;
	}
}

#endregion

#region Spans
public readonly struct SpanEncoder<TType, TEncoder> : INetEncoder where TType : struct where TEncoder : INetEncoder
{
	public readonly void Write(ModPacket packet, Span<TType> value)
	{
		packet.Write7BitEncodedInt(value.Length);

		var encoder = default(TEncoder);
		foreach (ref var element in value) {
			encoder.Write(packet, element);
		}
	}

	public readonly Span<TType> Read(BinaryReader reader) => default(ArrayEncoder<TType, TEncoder>).Read(reader);

	void INetEncoder.Write<T>(ModPacket packet, T value) => throw new NotImplementedException();

	T INetEncoder.Read<T>(BinaryReader reader) => throw new NotImplementedException();
}
public readonly struct ReadOnlySpanEncoder<TType, TEncoder> : INetEncoder where TType : struct where TEncoder : INetEncoder
{
	public readonly void Write(ModPacket packet, ReadOnlySpan<TType> value)
	{
		packet.Write7BitEncodedInt(value.Length);

		var encoder = default(TEncoder);
		foreach (var element in value) {
			encoder.Write(packet, element);
		}
	}

	public readonly ReadOnlySpan<TType> Read(BinaryReader reader) => default(ArrayEncoder<TType, TEncoder>).Read(reader);

	void INetEncoder.Write<T>(ModPacket packet, T value) => throw new NotImplementedException();

	T INetEncoder.Read<T>(BinaryReader reader) => throw new NotImplementedException();
}
#endregion

#region Value Tuples
public readonly struct ValueTupleEncoder<T1_0, T1_1>
	: INetEncoder<ValueTuple<T1_0>>
	where T1_1 : INetEncoder
{
	public readonly void Write(ModPacket packet, ValueTuple<T1_0> value) => default(T1_1).Write(packet, value.Item1);
	public readonly ValueTuple<T1_0> Read(BinaryReader reader) => new(default(T1_1).Read<T1_0>(reader));
}

public readonly struct ValueTupleEncoder<T1_0, T1_1, T2_0, T2_1>
	: INetEncoder<(T1_0, T2_0)>
	where T1_1 : INetEncoder
	where T2_1 : INetEncoder
{
	public readonly void Write(ModPacket packet, (T1_0, T2_0) value)
	{
		default(T1_1).Write(packet, value.Item1);
		default(T2_1).Write(packet, value.Item2);
	}

	public readonly (T1_0, T2_0) Read(BinaryReader reader)
	{
		var item1 = default(T1_1).Read<T1_0>(reader);
		var item2 = default(T2_1).Read<T2_0>(reader);

		return (item1, item2);
	}
}

public readonly struct ValueTupleEncoder<T1_0, T1_1, T2_0, T2_1, T3_0, T3_1>
	: INetEncoder<(T1_0, T2_0, T3_0)>
	where T1_1 : INetEncoder
	where T2_1 : INetEncoder
	where T3_1 : INetEncoder
{
	public readonly void Write(ModPacket packet, (T1_0, T2_0, T3_0) value)
	{
		default(T1_1).Write(packet, value.Item1);
		default(T2_1).Write(packet, value.Item2);
		default(T3_1).Write(packet, value.Item3);
	}

	public readonly (T1_0, T2_0, T3_0) Read(BinaryReader reader)
	{
		var item1 = default(T1_1).Read<T1_0>(reader);
		var item2 = default(T2_1).Read<T2_0>(reader);
		var item3 = default(T3_1).Read<T3_0>(reader);

		return (item1, item2, item3);
	}
}

public readonly struct ValueTupleEncoder<T1_0, T1_1, T2_0, T2_1, T3_0, T3_1, T4_0, T4_1>
	: INetEncoder<(T1_0, T2_0, T3_0, T4_0)>
	where T1_1 : INetEncoder
	where T2_1 : INetEncoder
	where T3_1 : INetEncoder
	where T4_1 : INetEncoder
{
	public readonly void Write(ModPacket packet, (T1_0, T2_0, T3_0, T4_0) value)
	{
		default(T1_1).Write(packet, value.Item1);
		default(T2_1).Write(packet, value.Item2);
		default(T3_1).Write(packet, value.Item3);
		default(T4_1).Write(packet, value.Item4);
	}

	public readonly (T1_0, T2_0, T3_0, T4_0) Read(BinaryReader reader)
	{
		var item1 = default(T1_1).Read<T1_0>(reader);
		var item2 = default(T2_1).Read<T2_0>(reader);
		var item3 = default(T3_1).Read<T3_0>(reader);
		var item4 = default(T4_1).Read<T4_0>(reader);

		return (item1, item2, item3, item4);
	}
}

public readonly struct ValueTupleEncoder<T1_0, T1_1, T2_0, T2_1, T3_0, T3_1, T4_0, T4_1, T5_0, T5_1>
	: INetEncoder<(T1_0, T2_0, T3_0, T4_0, T5_0)>
	where T1_1 : INetEncoder
	where T2_1 : INetEncoder
	where T3_1 : INetEncoder
	where T4_1 : INetEncoder
	where T5_1 : INetEncoder
{
	public readonly void Write(ModPacket packet, (T1_0, T2_0, T3_0, T4_0, T5_0) value)
	{
		default(T1_1).Write(packet, value.Item1);
		default(T2_1).Write(packet, value.Item2);
		default(T3_1).Write(packet, value.Item3);
		default(T4_1).Write(packet, value.Item4);
		default(T5_1).Write(packet, value.Item5);
	}

	public readonly (T1_0, T2_0, T3_0, T4_0, T5_0) Read(BinaryReader reader)
	{
		var item1 = default(T1_1).Read<T1_0>(reader);
		var item2 = default(T2_1).Read<T2_0>(reader);
		var item3 = default(T3_1).Read<T3_0>(reader);
		var item4 = default(T4_1).Read<T4_0>(reader);
		var item5 = default(T5_1).Read<T5_0>(reader);

		return (item1, item2, item3, item4, item5);
	}
}

public readonly struct ValueTupleEncoder<T1_0, T1_1, T2_0, T2_1, T3_0, T3_1, T4_0, T4_1, T5_0, T5_1, T6_0, T6_1>
	: INetEncoder<(T1_0, T2_0, T3_0, T4_0, T5_0, T6_0)>
	where T1_1 : INetEncoder
	where T2_1 : INetEncoder
	where T3_1 : INetEncoder
	where T4_1 : INetEncoder
	where T5_1 : INetEncoder
	where T6_1 : INetEncoder
{
	public readonly void Write(ModPacket packet, (T1_0, T2_0, T3_0, T4_0, T5_0, T6_0) value)
	{
		default(T1_1).Write(packet, value.Item1);
		default(T2_1).Write(packet, value.Item2);
		default(T3_1).Write(packet, value.Item3);
		default(T4_1).Write(packet, value.Item4);
		default(T5_1).Write(packet, value.Item5);
		default(T6_1).Write(packet, value.Item6);
	}

	public readonly (T1_0, T2_0, T3_0, T4_0, T5_0, T6_0) Read(BinaryReader reader)
	{
		var item1 = default(T1_1).Read<T1_0>(reader);
		var item2 = default(T2_1).Read<T2_0>(reader);
		var item3 = default(T3_1).Read<T3_0>(reader);
		var item4 = default(T4_1).Read<T4_0>(reader);
		var item5 = default(T5_1).Read<T5_0>(reader);
		var item6 = default(T6_1).Read<T6_0>(reader);

		return (item1, item2, item3, item4, item5, item6);
	}
}

public readonly struct ValueTupleEncoder<T1_0, T1_1, T2_0, T2_1, T3_0, T3_1, T4_0, T4_1, T5_0, T5_1, T6_0, T6_1, T7_0, T7_1>
	: INetEncoder<(T1_0, T2_0, T3_0, T4_0, T5_0, T6_0, T7_0)>
	where T1_1 : INetEncoder
	where T2_1 : INetEncoder
	where T3_1 : INetEncoder
	where T4_1 : INetEncoder
	where T5_1 : INetEncoder
	where T6_1 : INetEncoder
	where T7_1 : INetEncoder
{
	public readonly void Write(ModPacket packet, (T1_0, T2_0, T3_0, T4_0, T5_0, T6_0, T7_0) value)
	{
		default(T1_1).Write(packet, value.Item1);
		default(T2_1).Write(packet, value.Item2);
		default(T3_1).Write(packet, value.Item3);
		default(T4_1).Write(packet, value.Item4);
		default(T5_1).Write(packet, value.Item5);
		default(T6_1).Write(packet, value.Item6);
		default(T7_1).Write(packet, value.Item7);
	}

	public readonly (T1_0, T2_0, T3_0, T4_0, T5_0, T6_0, T7_0) Read(BinaryReader reader)
	{
		var item1 = default(T1_1).Read<T1_0>(reader);
		var item2 = default(T2_1).Read<T2_0>(reader);
		var item3 = default(T3_1).Read<T3_0>(reader);
		var item4 = default(T4_1).Read<T4_0>(reader);
		var item5 = default(T5_1).Read<T5_0>(reader);
		var item6 = default(T6_1).Read<T6_0>(reader);
		var item7 = default(T7_1).Read<T7_0>(reader);

		return (item1, item2, item3, item4, item5, item6, item7);
	}
}

public readonly struct ValueTupleEncoder<T1_0, T1_1, T2_0, T2_1, T3_0, T3_1, T4_0, T4_1, T5_0, T5_1, T6_0, T6_1, T7_0, T7_1, T8_0, T8_1>
	: INetEncoder<(T1_0, T2_0, T3_0, T4_0, T5_0, T6_0, T7_0, T8_0)>
	where T1_1 : INetEncoder
	where T2_1 : INetEncoder
	where T3_1 : INetEncoder
	where T4_1 : INetEncoder
	where T5_1 : INetEncoder
	where T6_1 : INetEncoder
	where T7_1 : INetEncoder
	where T8_1 : INetEncoder
{
	public readonly void Write(ModPacket packet, (T1_0, T2_0, T3_0, T4_0, T5_0, T6_0, T7_0, T8_0) value)
	{
		default(T1_1).Write(packet, value.Item1);
		default(T2_1).Write(packet, value.Item2);
		default(T3_1).Write(packet, value.Item3);
		default(T4_1).Write(packet, value.Item4);
		default(T5_1).Write(packet, value.Item5);
		default(T6_1).Write(packet, value.Item6);
		default(T7_1).Write(packet, value.Item7);
		default(T8_1).Write(packet, value.Item8);
	}

	public readonly (T1_0, T2_0, T3_0, T4_0, T5_0, T6_0, T7_0, T8_0) Read(BinaryReader reader)
	{
		var item1 = default(T1_1).Read<T1_0>(reader);
		var item2 = default(T2_1).Read<T2_0>(reader);
		var item3 = default(T3_1).Read<T3_0>(reader);
		var item4 = default(T4_1).Read<T4_0>(reader);
		var item5 = default(T5_1).Read<T5_0>(reader);
		var item6 = default(T6_1).Read<T6_0>(reader);
		var item7 = default(T7_1).Read<T7_0>(reader);
		var item8 = default(T8_1).Read<T8_0>(reader);

		return (item1, item2, item3, item4, item5, item6, item7, item8);
	}
}
#endregion

#region Mod Types
public readonly struct ModPlayerEncoder : INetEncoder<ModPlayer>
{
	public readonly void Write(ModPacket packet, ModPlayer value)
	{
		default(Player.Encoder).Write(packet, value.Player);
		packet.Write(value.Index);
	}

	public readonly ModPlayer Read(BinaryReader reader)
	{
		var player = default(Player.Encoder).Read(reader);
		ushort index = reader.ReadUInt16();

		return player.modPlayers[index];
	}
}

public readonly struct ModNPCEncoder : INetEncoder<ModNPC>
{
	public readonly void Write(ModPacket packet, ModNPC value) => default(NPC.Encoder).Write(packet, value.NPC);
	public readonly ModNPC Read(BinaryReader reader) => default(NPC.Encoder).Read(reader).ModNPC;
}

public readonly struct ModProjectileEncoder : INetEncoder<ModProjectile>
{
	public readonly void Write(ModPacket packet, ModProjectile value) => default(Projectile.Encoder).Write(packet, value.Projectile);
	public readonly ModProjectile Read(BinaryReader reader) => default(Projectile.Encoder).Read(reader).ModProjectile;
}
#endregion
