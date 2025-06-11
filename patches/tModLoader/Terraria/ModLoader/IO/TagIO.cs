using Ionic.Zlib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

#nullable enable

namespace Terraria.ModLoader.IO;

public static class TagIO
{
	private abstract class PayloadHandler
	{
		public abstract Type PayloadType { get; }
		public abstract object Default();
		public abstract object Read(BinaryReader r);
		public abstract void Write(BinaryWriter w, object v);
		public abstract IList ReadList(BinaryReader r, int size);
		public abstract void WriteList(BinaryWriter w, IList list);
		public abstract object Clone(object o);
		public abstract IList CloneList(IList list);
	}

	private class PayloadHandler<T> : PayloadHandler
		where T : notnull
	{
		internal Func<BinaryReader, T> reader;
		internal Action<BinaryWriter, T> writer;

		public PayloadHandler(Func<BinaryReader, T> reader, Action<BinaryWriter, T> writer)
		{
			this.reader = reader;
			this.writer = writer;
		}

		public override Type PayloadType => typeof(T);
		public override object Read(BinaryReader r) => reader(r);
		public override void Write(BinaryWriter w, object v) => writer(w, (T)v);

		public override IList ReadList(BinaryReader r, int size)
		{
			var list = new List<T>(size);
			for (int i = 0; i < size; i++)
				list.Add(reader(r));

			return list;
		}

		public override void WriteList(BinaryWriter w, IList list)
		{
			foreach (T t in (IEnumerable<T>)list)
				writer(w, t);
		}

		public override object Clone(object o) => o;
		public override IList CloneList(IList list) => CloneList((IList<T>)list);
		public virtual IList CloneList(IList<T> list) => new List<T>(list);

		public override object Default() => default(T)!;
	}

	private class ClassPayloadHandler<T> : PayloadHandler<T> where T : class
	{
		private Func<T, T> clone;
		private Func<T>? makeDefault;

		public ClassPayloadHandler(Func<BinaryReader, T> reader, Action<BinaryWriter, T> writer,
				Func<T, T> clone, Func<T>? makeDefault = null) :
				base(reader, writer)
		{
			this.clone = clone;
			this.makeDefault = makeDefault;
		}

		public override object Clone(object o) => clone((T)o);
		public override IList CloneList(IList<T> list) => list.Select(clone).ToList();
		public override object Default() => makeDefault!(); // If makeDefault is null, it's our job to handle default values to ensure this is never called
	}

	private static readonly PayloadHandler[] PayloadHandlers = {
		null!, // Unused
		new PayloadHandler<byte>(r => r.ReadByte(), (w, v) => w.Write(v)),
		new PayloadHandler<short>(r => r.ReadInt16(), (w, v) => w.Write(v)),
		new PayloadHandler<int>(r => r.ReadInt32(), (w, v) => w.Write(v)),
		new PayloadHandler<long>(r => r.ReadInt64(), (w, v) => w.Write(v)),
		new PayloadHandler<float>(r => r.ReadSingle(), (w, v) => w.Write(v)),
		new PayloadHandler<double>(r => r.ReadDouble(), (w, v) => w.Write(v)),
		new ClassPayloadHandler<byte[]>(
			r => r.ReadBytes(r.ReadInt32()),
			(w, v) => {
				w.Write(v.Length);
				w.Write(v);
			},
			v => (byte[]) v.Clone(),
			() => Array.Empty<byte>()),
		new ClassPayloadHandler<string>(
			r => Encoding.UTF8.GetString(r.BaseStream.ReadByteSpan(r.ReadInt16())),
			(w, v) => {
				var b = Encoding.UTF8.GetBytes(v);
				w.Write((short)b.Length);
				w.Write(b);
			},
			v => v,
			() => ""),
		new ClassPayloadHandler<IList>(
			r => GetHandler(r.ReadByte()).ReadList(r, r.ReadInt32()),
			(w, v) => {
				int id;
				try {
					id = GetPayloadId(GetListElementType(v.GetType()));
				}
				catch (IOException) {
					throw new IOException("Invalid NBT list type: " + v.GetType());
				}
				w.Write((byte)id);
				w.Write(v.Count);
				PayloadHandlers![id].WriteList(w, v);
			},
			v => {
				try {
					return GetHandler(GetPayloadId(GetListElementType(v.GetType()))).CloneList(v);
				}
				catch (IOException) {
					throw new IOException("Invalid NBT list type: " + v.GetType());
				}
			}),
		new ClassPayloadHandler<TagCompound>(
			r => {
				var compound = new TagCompound();
				object? tag;
				while ((tag = ReadTag(r, out string? name)) != null)
					compound.Set(name, tag);

				return compound;
			},
			(w, v) => {
				foreach (var entry in v)
					if (entry.Value != null)
						WriteTag(entry.Key, entry.Value, w);

				w.Write((byte)0);
			},
			v => (TagCompound) v.Clone(),
			() => new TagCompound()),
		new ClassPayloadHandler<int[]>(
			r => {
				var ia = new int[r.ReadInt32()];
				for (int i = 0; i < ia.Length; i++)
					ia[i] = r.ReadInt32();
				return ia;
			},
			(w, v) => {
				w.Write(v.Length);
				foreach (int i in v)
					w.Write(i);
			},
			v => (int[]) v.Clone(),
			() => Array.Empty<int>()),
	};

	private static readonly Dictionary<Type, int> PayloadIDs =
		Enumerable.Range(1, PayloadHandlers.Length - 1).ToDictionary(i => PayloadHandlers[i].PayloadType);

	private static readonly PayloadHandler<string> StringHandler = (PayloadHandler<string>)PayloadHandlers[8];

	private static PayloadHandler GetHandler(int id)
	{
		if (id < 1 || id >= PayloadHandlers.Length)
			throw new IOException("Invalid NBT payload id: " + id);

		return PayloadHandlers[id];
	}

	private static int GetPayloadId(Type t)
	{
		if (PayloadIDs.TryGetValue(t, out int id))
			return id;

		if (typeof(IList).IsAssignableFrom(t))
			return 9;

		throw new IOException($"Invalid NBT payload type '{t}'");
	}

	private static Type GetListElementType(Type type)
	{
		var elemType = type.GetElementType();
		if (elemType is not null)
			return elemType;

		while (true) {
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
				return type.GetGenericArguments()[0];

			if (type.BaseType is null)
				break;

			type = type.BaseType;
		}

		throw new IOException($"Invalid NBT payload type '{type}'");
	}

	public static object Serialize(object value)
	{
		ArgumentNullException.ThrowIfNull(value);

		// some very quick checks which can save on heavier dict lookups
		if (value is string or int or TagCompound or List<TagCompound>)
			return value;

		var type = value.GetType();

		if (TagSerializer.TryGetSerializer(type, out TagSerializer? serializer))
			return serializer.Serialize(value);

		// does a base level typecheck with throw
		if (GetPayloadId(type) != 9)
			return value;

		var list = (IList)value;
		var elemType = GetListElementType(type);
		if (TagSerializer.TryGetSerializer(elemType, out serializer))
			return serializer.SerializeList(list);

		if (GetPayloadId(elemType) != 9)
			return list; // already a valid NBT list type

		// list of lists conversion
		var serializedList = new List<IList>(list.Count);
		foreach (var elem in list)
			serializedList.Add((IList)Serialize(elem));

		return serializedList;
	}

	public static T Deserialize<T>(object? tag)
	{
		if (tag is T t) return t;
		return (T)Deserialize(typeof(T), tag);
	}

	public static object Deserialize(Type type, object? tag)
	{
		ArgumentNullException.ThrowIfNull(type);

		if (type.IsInstanceOfType(tag))
			return tag;

		if (TagSerializer.TryGetSerializer(type, out TagSerializer? serializer)) {
			if (tag == null)
				tag = Deserialize(serializer.TagType, null);

			return serializer.Deserialize(tag);
		}

		// normal nbt type with missing value
		if (tag == null && !type.IsArray) {
			if (type.GetGenericArguments().Length == 0)
				return GetHandler(GetPayloadId(type)).Default();

			if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
				return Activator.CreateInstance(type)!;
		}

		//list conversion required
		if (tag == null || tag is IList || type.IsArray) {
			if (type.IsArray) {
				// Only 1d arrays reach here
				var elemType = type.GetElementType()!;

				if (tag == null)
					return Array.CreateInstance(elemType, 0);

				var serializedList = (IList)tag;

				if (TagSerializer.TryGetSerializer(elemType, out serializer)) {
					IList array = Array.CreateInstance(elemType, serializedList.Count);
					for (int i = 0; i < serializedList.Count; i++)
						array[i] = serializer.Deserialize(serializedList[i]!);

					return array;
				}

				//create a strongly typed nested array
				IList deserializedArray = Array.CreateInstance(elemType, serializedList.Count);
				for (int i = 0; i < serializedList.Count; i++)
					deserializedArray[i] = Deserialize(elemType, serializedList[i]);

				return deserializedArray;
			}

			if (type.GetGenericArguments().Length == 1) {
				var elemType = type.GetGenericArguments()[0];
				var newListType = typeof(List<>).MakeGenericType(elemType);
				if (type.IsAssignableFrom(newListType)) { //if the desired type is a superclass of List<elemType>
					if (tag == null)
						return Activator.CreateInstance(newListType)!;

					if (TagSerializer.TryGetSerializer(elemType, out serializer))
						return serializer.DeserializeList((IList)tag);

					//create a strongly typed nested list
					var oldList = (IList)tag;
					var newList = (IList)Activator.CreateInstance(newListType, oldList.Count)!;
					foreach (var elem in oldList)
						newList.Add(Deserialize(elemType, elem));

					return newList;
				}
			}
		}

		if (tag == null) // unable to create an empty list subclassing the desired type
			throw new IOException($"Invalid NBT payload type '{type}'");

		throw new InvalidCastException($"Unable to cast object of type '{tag.GetType()}' to type '{type}'");
	}

	public static T Clone<T>(T o) where T : notnull => (T)GetHandler(GetPayloadId(o.GetType())).Clone(o);

	public static object? ReadTag(BinaryReader r, out string? name)
	{
		int id = r.ReadByte();
		if (id == 0) {
			name = null;
			return null;
		}

		name = StringHandler.reader(r);
		return ReadTagImpl(id, r);
	}

	public static object? ReadTagImpl(int id, BinaryReader r) => PayloadHandlers[id].Read(r);

	public static void WriteTag(string name, object tag, BinaryWriter w)
	{
		int id = GetPayloadId(tag.GetType());
		w.Write((byte)id);
		StringHandler.writer(w, name);
		PayloadHandlers[id].Write(w, tag);
	}

	public static TagCompound FromFile(string path, bool compressed = true)
	{
		try {
			using (Stream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
				return FromStream(fs, compressed);
		}
		catch (IOException e) {
			throw new IOException("Failed to read NBT file: " + path, e);
		}
	}

	public static TagCompound FromStream(Stream stream, bool compressed = true)
	{
		if (compressed) {
			stream = new GZipStream(stream, CompressionMode.Decompress);

			// Can cut parsing times by up to half
			// The deserialized tag is stored in full memory anyway, so assume we have enough for the serialized representation too
			var ms = new MemoryStream(1<<20);
			stream.CopyTo(ms);
			ms.Position = 0;
			stream = ms;
		}
		return Read(new BigEndianReader(stream));
	}

	public static TagCompound Read(BinaryReader reader)
	{
		var tag = ReadTag(reader, out string? name);
		if (tag is not TagCompound compound)
			throw new IOException("Root tag not a TagCompound");

		return compound;
	}

	public static void ToFile(TagCompound root, string path, bool compress = true)
	{
		try {
			using (Stream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
				ToStream(root, fs, compress);
		}
		catch (IOException e) {
			throw new IOException("Failed to read NBT file: " + path, e);
		}
	}

	public static void ToStream(TagCompound root, Stream stream, bool compress = true)
	{
		if (compress) stream = new GZipStream(stream, CompressionMode.Compress, true);
		Write(root, new BigEndianWriter(stream));
		if (compress) stream.Close();
	}

	/// <summary>
	/// Writes the TagCompound to the writer. Please don't use this to send TagCompound over the network if you can avoid it. If you have to, consider using <see cref="ToStream(TagCompound, Stream, bool)"/>/<see cref="FromStream(Stream, bool)"/> with <c>compress: true</c>.
	/// </summary>
	public static void Write(TagCompound root, BinaryWriter writer) => WriteTag("", root, writer);
}
