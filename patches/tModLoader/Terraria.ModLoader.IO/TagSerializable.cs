using System;
using System.Collections.Generic;
using System.Reflection;

namespace Terraria.ModLoader.IO
{
	public interface TagSerializable
	{
		TagCompound SerializeData();
	}

	public static class TagSerializables
	{
		internal static IDictionary<string, Delegate> deserializerCache = new Dictionary<string, Delegate>();

		internal static void Reset()
		{
			deserializerCache.Clear();
		}

		public static TagCompound Serialize(TagSerializable obj)
		{
			TagCompound tag = new TagCompound();
			tag["type"] = obj.GetType().FullName;

			TagCompound dataTag = obj.SerializeData();
			if (dataTag != null && dataTag.Count != 0)
				tag["data"] = dataTag;
			return tag;
		}

		public static T Deserialize<T>(TagCompound tag) where T : TagSerializable
		{
			if (tag == null)
				return default(T);

			Func<TagCompound, T> deserializer = GetDeserializer<T>(tag.GetString("type"));
			TagCompound dataTag = tag.HasTag("data") ? tag.GetCompound("data") : null;
			return deserializer(dataTag);
		}

		private static Func<TagCompound, T> GetDeserializer<T>(string name) where T : TagSerializable
		{
			if (deserializerCache.ContainsKey(name))
				return (Func<TagCompound, T>)deserializerCache[name];

			Type type = Type.GetType(name);
			if (type == null)
			{
				foreach (Mod mod in ModLoader.LoadedMods)
				{
					type = mod.Code.GetType(name);
					if (type == null)
						throw new TypeUnloadedException();
				}
			}

			FieldInfo deserializerField = type.GetField("DESERIALIZER");
			if (deserializerField == null)
				throw new Exception(string.Format("Missing deserializer for type {0}.", type.FullName));

			Func<TagCompound, T> deserializer = (Func<TagCompound, T>)deserializerField.GetValue(null);
			deserializerCache[name] = deserializer;
			return deserializer;
		}
	}

	public static class TagSerializableExtensions
	{
		public static TagCompound Serialize(this TagSerializable self)
		{
			return TagSerializables.Serialize(self);
		}
	}
}
