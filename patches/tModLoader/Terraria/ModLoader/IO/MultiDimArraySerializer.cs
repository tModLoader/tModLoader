#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.ModLoader.IO;

[Autoload(false)]
public class MultiDimArraySerializer : TagSerializer<Array, TagCompound>
{
	public delegate object Converter(object element);

	private const string Ranks = "ranks";
	private const string List = "list";

	public Type ArrayType { get; }
	public Type ElementType { get; }
	public int ArrayRank { get; }

	public MultiDimArraySerializer(Type arrayType)
	{
		ArgumentNullException.ThrowIfNull(arrayType);

		if (!arrayType.IsArray) {
			throw new ArgumentException("Must be an array type", nameof(arrayType));
		}

		ArrayType = arrayType;
		ElementType = arrayType.GetElementType()!;
		ArrayRank = arrayType.GetArrayRank();
	}

	public override TagCompound Serialize(Array array)
	{
		ArgumentNullException.ThrowIfNull(array);

		if (array.Length == 0) {
			return ToTagCompound(array);
		}

		var first = array.GetValue(new int[array.Rank]);
		var serializedType = TagIO.Serialize(first).GetType();

		var tagCompound = ToTagCompound(array, serializedType, TagIO.Serialize);

		return tagCompound;
	}

	public override Array Deserialize(TagCompound tag)
	{
		ArgumentNullException.ThrowIfNull(tag);

		return FromTagCompound(tag, ArrayType, e => TagIO.Deserialize(ElementType, e));
	}

	public override IList SerializeList(IList list)
	{
		ArgumentNullException.ThrowIfNull(list);

		var serializedList = new List<TagCompound>(list.Count);

		foreach (Array array in list) {
			serializedList.Add(Serialize(array));
		}

		return serializedList;
	}

	public override IList DeserializeList(IList list)
	{
		ArgumentNullException.ThrowIfNull(list);

		var listT = (IList<TagCompound>)list;
		var deserializedList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(ArrayType), listT.Count)!;

		foreach (var tagCompound in listT) {
			deserializedList.Add(Deserialize(tagCompound));
		}

		return deserializedList;
	}

	public static TagCompound ToTagCompound(Array array, Type? elementType = null, Converter? converter = null)
	{
		ArgumentNullException.ThrowIfNull(array);

		int[] ranks = new int[array.Rank];
		for (int i = 0; i < ranks.Length; i++) {
			ranks[i] = array.GetLength(i);
		}

		return new() {
			[Ranks] = ranks,
			[List] = ToList(array, elementType, converter),
		};
	}

	public static IList ToList(Array array, Type? elementType = null, Converter? converter = null)
	{
		ArgumentNullException.ThrowIfNull(array);

		var arrayType = array.GetType();
		var listType = typeof(List<>).MakeGenericType(elementType ?? arrayType.GetElementType()!);

		var list = (IList)Activator.CreateInstance(listType, array.Length)!;
		foreach (var o in array) {
			list.Add(converter != null ? converter(o) : o);
		}

		return list;
	}

	public static Array FromTagCompound(TagCompound tag, Type arrayType, Converter? converter = null)
	{
		ArgumentNullException.ThrowIfNull(tag);
		ArgumentNullException.ThrowIfNull(arrayType);
		if (!arrayType.IsArray) {
			throw new ArgumentException("Must be an array type", nameof(arrayType));
		}

		var elementType = arrayType.GetElementType()!;

		// If an empty TagCompound is passed, return an empty array.
		if (!tag.TryGet(Ranks, out int[] ranks)) {
			return Array.CreateInstance(elementType, new int[arrayType.GetArrayRank()]);
		}

		var list = tag.Get<List<object>>(List); // We don't care what our serialized type is, the converter will handle it

		return FromList(list, ranks, elementType, converter);
	}

	public static Array FromList(IList list, int[] arrayRanks, Type? elementType = null, Converter? converter = null)
	{
		ArgumentNullException.ThrowIfNull(list);
		ArgumentNullException.ThrowIfNull(arrayRanks);

		if (arrayRanks.Length == 0) {
			throw new ArgumentException("Array rank must be greater than 0");
		}

		if (list.Count != arrayRanks.Aggregate(1, (current, length) => current * length)) {
			throw new ArgumentException("List length does not match array length");
		}

		var type = list.GetType();
		elementType ??= type.GetElementType();
		if (elementType is null) {
			if (type.GetGenericArguments() is not { Length: 1 } genericArguments) {
				throw new ArgumentException("IList type must have exactly one generic argument");
			}

			elementType = genericArguments[0];
		}

		var array = Array.CreateInstance(elementType, arrayRanks);

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