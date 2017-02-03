using System;

namespace Terraria.ModLoader.IO
{
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

		public TagSerializableSerializer() {
			var type = typeof(T);
			var field = type.GetField("DESERIALIZER");
			if (field == null)
				throw new ArgumentException($"Missing deserializer for type {type.FullName}.");
			if (field.FieldType != typeof(Func<TagCompound, T>))
				throw new ArgumentException(
					$"Invalid deserializer field type {field.FieldType} in {type.FullName} expected {typeof(Func<TagCompound, T>)}.");

			deserializer = (Func<TagCompound, T>)field.GetValue(null);
		}

		public override TagCompound Serialize(T value) {
			var tag = value.SerializeData();
			tag["<type>"] = value.GetType().FullName;
			return tag;
		}

		public override T Deserialize(TagCompound tag) {
			return deserializer(tag);
		}
	}
}
