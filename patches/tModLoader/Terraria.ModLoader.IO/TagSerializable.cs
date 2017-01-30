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
		internal static IDictionary<string, Type> typeCache = new Dictionary<string, Type>();

		internal static void Reset()
		{
			typeCache.Clear();
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

			Type type = GetType(tag.GetString("type"));
			if (type == null)
				throw new TypeUnloadedException();

			FieldInfo deserializerField = type.GetField("DESERIALIZER");
			if (deserializerField == null)
				throw new Exception(string.Format("Missing deserializer for type {0}.", type.FullName));

			Func<TagCompound, T> deserializer = (Func<TagCompound, T>)deserializerField.GetValue(null);
			TagCompound dataTag = tag.HasTag("data") ? tag.GetCompound("data") : null;
			return deserializer(dataTag);
		}

		private static Type GetType(string name)
		{
			if (typeCache.ContainsKey(name))
				return typeCache[name];

			Type type = Type.GetType(name);
			if (type != null)
			{
				typeCache[name] = type;
				return type;
			}

			foreach (Mod mod in ModLoader.LoadedMods)
			{
				type = mod.Code.GetType(name);
				if (type != null)
				{
					typeCache[name] = type;
					return type;
				}
			}

			return null;
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
