using System;

namespace Terraria.ModLoader.Packets;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public sealed class EncodedAsAttribute : Attribute {
	public Type EncoderType { get; }

	public EncodedAsAttribute(Type encoderType) {
		EncoderType = encoderType;
	}
}

/// <summary>
/// Encodes all properties of specified type with specified encoder.
/// <br/>
/// Does not overrides <seealso cref="EncodedAsAttribute{T}"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
public sealed class GloballyEncodedAsAttribute : Attribute {
	public Type Type { get; }
	public Type Encoder { get; }

	public GloballyEncodedAsAttribute(Type type, Type encoder) {
		Type = type;
		Encoder = encoder;
	}
}
