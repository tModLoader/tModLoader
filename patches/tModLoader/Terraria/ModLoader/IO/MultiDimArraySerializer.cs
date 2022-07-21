#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Terraria.ModLoader.IO;

[Autoload(false)]
public class MultiDimArraySerializer : TagSerializer<Array, TagCompound>
{
	public delegate object Converter(object elem);

	private static readonly MethodInfo TagCompoundGetListMethodInfo =
		typeof(TagCompound).GetMethod(nameof(TagCompound.GetList), BindingFlags.Instance | BindingFlags.Public)!;

	public Type ArrayType { get; }
	public Type ElementType { get; }

	public MultiDimArraySerializer(Type arrayType) {
		ArgumentNullException.ThrowIfNull(arrayType);

		if (!arrayType.IsArray) {
			throw new ArgumentException("Type must be an array type", nameof(arrayType));
		}

		ArrayType = arrayType;
		ElementType = arrayType.GetElementType()!;
	}

	public override TagCompound Serialize(Array array) {
		ArgumentNullException.ThrowIfNull(array);

		if (array.Length == 0) {
			return ToTagCompound(array);
		}

		var first = array.GetValue(new int[array.Rank]);
		var serializedType = TagIO.Serialize(first).GetType();

		var tagCompound = ToTagCompound(array, serializedType, TagIO.Serialize);
		tagCompound["serializedType"] = serializedType.FullName;

		return tagCompound;
	}

	public override Array Deserialize(TagCompound tag) {
		ArgumentNullException.ThrowIfNull(tag);

		var serializedTypeFullName = tag.Get<string>("serializedType");
		Type? serializedType = null;
		if (!string.IsNullOrEmpty(serializedTypeFullName)) {
			serializedType = GetType(serializedTypeFullName);
		}

		return FromTagCompound(tag, ElementType, e => TagIO.Deserialize(ElementType, e), serializedType);
	}

	public override IList SerializeList(IList value) {
		ArgumentNullException.ThrowIfNull(list);

		var serializedList = new List<TagCompound>();

		foreach (Array array in value) {
			serializedList.Add(Serialize(array));
		}

		return serializedList;
	}

	public override IList DeserializeList(IList list) {
		ArgumentNullException.ThrowIfNull(list);

		var listT = (IList<TagCompound>)list;
		var deserializedList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(ArrayType), listT.Count)!;

		foreach (var tagCompound in listT) {
			deserializedList.Add(Deserialize(tagCompound));
		}

		return deserializedList;
	}

	public static TagCompound ToTagCompound(Array array, Type? elemType = null, Converter? converter = null) {
		ArgumentNullException.ThrowIfNull(array);

		TagCompound tag = new();

		byte rank = (byte)array.Rank;
		tag["rank"] = rank;
		for (int i = 0; i < rank; i++) {
			tag["rank-" + i] = array.GetLength(i);
		}

		tag["list"] = ToList(array, elemType, converter);

		return tag;
	}

	public static Array FromTagCompound(TagCompound tag, Type elemType, Converter? converter = null, Type? sourceType = null) {
		ArgumentNullException.ThrowIfNull(tag);
		ArgumentNullException.ThrowIfNull(elemType);

		byte rank = tag.GetByte("rank");
		int[] arrayRanks = new int[rank];
		for (int i = 0; i < rank; i++) {
			arrayRanks[i] = tag.GetInt("rank-" + i);
		}

		var tagCompoundGetList = TagCompoundGetListMethodInfo.MakeGenericMethod(sourceType ?? elemType);
		var list = (IList)tagCompoundGetList.Invoke(tag, new object[] { "list" })!;

		return FromList(list, arrayRanks, elemType, converter);
	}

	public static IList ToList(Array array, Type? elemType = null, Converter? converter = null) {
		ArgumentNullException.ThrowIfNull(array);

		var arrayType = array.GetType();
		var listType = typeof(List<>).MakeGenericType(elemType ?? arrayType.GetElementType()!);

		var list = (IList)Activator.CreateInstance(listType, array.Length)!;
		foreach (var o in array) {
			list.Add(converter != null ? converter(o) : o);
		}

		return list;
	}

	public static Array FromList(IList list, int[] arrayRanks, Type? elemType = null, Converter? converter = null) {
		ArgumentNullException.ThrowIfNull(list);
		ArgumentNullException.ThrowIfNull(arrayRanks);

		if (arrayRanks.Length == 0) {
			throw new ArgumentException("Array rank must be greater than 0");
		}

		if (list.Count != arrayRanks.Aggregate(1, (current, length) => current * length)) {
			throw new ArgumentException("List length does not match array length");
		}

		var type = list.GetType();
		elemType ??= type.GetElementType();
		if (elemType is null) {
			if (type.GetGenericArguments() is not { Length: 1 } genericArguments) {
				throw new ArgumentException("IList type must have exactly one generic argument");
			}

			elemType = genericArguments[0];
		}

		var array = Array.CreateInstance(elemType, arrayRanks);

		int[] indices = new int[arrayRanks.Length];
		foreach (var e in list) {
			var value = e;
			for (int r = indices.Length - 1; r >= 0; r--) {
				if (indices[r] < arrayRanks[r]) {
					break;
				}

				if (r == 0) {
					goto end;
				}

				indices[r] = 0;
				indices[r - 1]++;
			}

			if (converter != null) {
				value = converter(value);
			}

			array.SetValue(value, indices);
			indices[^1]++;
		}

		end:
		return array;
	}
}