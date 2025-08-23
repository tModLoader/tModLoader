using System;
using System.Collections.Generic;
using Terraria.ModLoader.Core;

namespace Terraria.ModLoader.IO;

//implement this interface and add
//public static Func<TagCompound, ClassName> DESERIALIZER
//to your class to make it automatically serializable
public interface TagSerializable
{
	TagCompound SerializeData();
}

internal class TagSerializableSerializer<T> : TagSerializer<T, TagCompound> where T : TagSerializable
{
	private Func<TagCompound, T> deserializer;
	private Dictionary<string, TagSerializer> subtypeSerializers;

	public TagSerializableSerializer()
	{
		var type = typeof(T);
		var field = type.GetField("DESERIALIZER");
		if (field != null) {
			if (field.FieldType != typeof(Func<TagCompound, T>))
				throw new ArgumentException(
					$"Invalid deserializer field type {field.FieldType} in {type.FullName} expected {typeof(Func<TagCompound, T>)}.");

			deserializer = (Func<TagCompound, T>)field.GetValue(null);
		}
	}

	public override TagCompound Serialize(T value)
	{
		var tag = value.SerializeData();
		tag["<type>"] = value.GetType().FullName;
		return tag;
	}

	public override T Deserialize(TagCompound tag)
	{
		if (tag.TryGet<string>("<type>", out var typeName) && typeName != Type.FullName && TryGetSubtypeSerializer(typeName, out var subtypeSerializer))
			return (T)subtypeSerializer.Deserialize(tag);

		if (deserializer == null) {
			var msg = $"Missing deserializer for type '{Type.FullName}'";
			if (typeName != null && typeName != Type.FullName)
				msg += $", subtype '{typeName}'";

			throw new ArgumentException(msg);
		}

		return deserializer(tag);
	}

	private bool TryGetSubtypeSerializer(string typeName, out TagSerializer subtypeSerializer)
	{
		subtypeSerializers ??= [];
		if (!subtypeSerializers.TryGetValue(typeName, out subtypeSerializer)) {
			var subtype = AssemblyManager.FindSubtype(typeof(T), typeName);
			if (subtype != null)
				TryGetSerializer(subtype, out subtypeSerializer);

			subtypeSerializers[typeName] = subtypeSerializer; // cache the serializer, even if it's null
		}

		return subtypeSerializer != null;
	}
}
