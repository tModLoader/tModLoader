using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zlib;

namespace Terraria.ModLoader.IO
{
	public static class TagIO
	{
		private abstract class PayloadHandler
		{
			public abstract Type PayloadType { get; }
			public abstract object Read(BinaryReader r);
			public abstract void Write(BinaryWriter w, object v);
			public abstract IList ReadList(BinaryReader r, int size);
			public abstract void WriteList(BinaryWriter w, IList list);
			public abstract object Clone(object o);
			public abstract IList CloneList(IList list);
		}

		private class PayloadHandler<T> : PayloadHandler
		{
			public Func<BinaryReader, T> reader;
			public Action<BinaryWriter, T> writer;
			public Func<T, T> clone;

			public PayloadHandler(Func<BinaryReader, T> reader, Action<BinaryWriter, T> writer, Func<T, T> clone = null) {
				this.reader = reader;
				this.writer = writer;
			}

			public override Type PayloadType => typeof(T);
			public override object Read(BinaryReader r) => reader(r);
			public override void Write(BinaryWriter w, object v) => writer(w, (T)v);

			public override IList ReadList(BinaryReader r, int size) {
				var list = new List<T>(size);
				for (int i = 0; i < size; i++)
					list.Add(reader(r));

				return list;
			}

			public override void WriteList(BinaryWriter w, IList list) => WriteList(w, (IList<T>) list);
			public void WriteList(BinaryWriter w, IList<T> list) {
				foreach (T t in list)
					writer(w, t);
			}

			public override object Clone(object o) => clone != null ? clone((T) o) : o;
			public override IList CloneList(IList list) => CloneList((IList<T>)list);
			public IList CloneList(IList<T> list) => new List<T>(clone != null ? list.Select(clone) : list);
		}

		private class ByteHandler : PayloadHandler<byte>
		{
			public ByteHandler(Func<BinaryReader, byte> reader, Action<BinaryWriter, byte> writer) : base(reader, writer)
			{}

			public override void Write(BinaryWriter w, object v)
			{
				if (v is bool)
					base.Write(w, (byte)((bool)v ? 1 : 0));
				else
					base.Write(w, v);
			}
		}

		private static readonly PayloadHandler[] PayloadHandlers = {
			null,
			new ByteHandler(r => r.ReadByte(), (w, v) => w.Write(v)), 
			new PayloadHandler<short>(r => r.ReadInt16(), (w, v) => w.Write(v)),
			new PayloadHandler<int>(r => r.ReadInt32(), (w, v) => w.Write(v)),
			new PayloadHandler<long>(r => r.ReadInt64(), (w, v) => w.Write(v)),
			new PayloadHandler<float>(r => r.ReadSingle(), (w, v) => w.Write(v)),
			new PayloadHandler<double>(r => r.ReadDouble(), (w, v) => w.Write(v)),
			new PayloadHandler<byte[]>(
				r => r.ReadBytes(r.ReadInt32()),
				(w, v) => {
					w.Write(v.Length);
					w.Write(v);
				},
				v => (byte[]) v.Clone()),
			new PayloadHandler<string>(
				r => Encoding.UTF8.GetString(r.ReadBytes(r.ReadInt16())),
				(w, v) => {
					var b = Encoding.UTF8.GetBytes(v);
					w.Write((short)b.Length);
					w.Write(b);
				}),
			new PayloadHandler<IList>(
				r => GetHandler(r.ReadByte()).ReadList(r, r.ReadInt32()),
				(w, v) => {
					int id;
					try {
						id = GetPayloadId(v.GetType().GetGenericArguments()[0]);
					}
					catch (IOException) {
						throw new IOException("Invalid NBT list type: "+v.GetType());
					}
					w.Write((byte)id);
					w.Write(v.Count);
					PayloadHandlers[id].WriteList(w, v);
				},
				v => {
					try {
						return GetHandler(GetPayloadId(v.GetType().GetGenericArguments()[0])).CloneList(v);
					}
					catch (IOException) {
						throw new ArgumentException("Invalid NBT list type: "+v.GetType());
					}
				}),
			new PayloadHandler<TagCompound>(
				r => {
					var compound = new TagCompound();
					string name;
					object tag;
					while ((tag = ReadTag(r, out name)) != null)
						compound.SetTag(name, tag);

					return compound;
				},
				(w, v) => {
					foreach (var entry in v)
						if (entry.Value != null)
							WriteTag(entry.Key, entry.Value, w);

					w.Write((byte)0);
				},
				v => (TagCompound) v.Clone()),
			new PayloadHandler<int[]>(
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
				})
		};

		private static readonly Dictionary<Type, int> PayloadIDs =
			Enumerable.Range(1, PayloadHandlers.Length - 1).ToDictionary(i => PayloadHandlers[i].PayloadType);

		private static PayloadHandler<string> StringHandler = (PayloadHandler<string>) PayloadHandlers[8];

		private static PayloadHandler GetHandler(int id) {
			if (id < 1 || id >= PayloadHandlers.Length)
				throw new IOException("Invalid NBT payload id: "+id);

			return PayloadHandlers[id];
		}

		private static int GetPayloadId(Type t) {

			int id;
			if (PayloadIDs.TryGetValue(t, out id))
				return id;

			if (t == typeof(bool))
				return 1;

			if (typeof(IList).IsAssignableFrom(t))
				return 9;

			throw new IOException("Invalid NBT payload type: "+t);
		}

		public static void TypeCheck(Type t)
		{
			if (GetPayloadId(t) == 9)
				TypeCheck(t.GetGenericArguments()[0]);
		}

		public static object ReadTag(BinaryReader r, out string name) {
			int id = r.ReadByte();
			if (id == 0) {
				name = null;
				return null;
			}

			name = StringHandler.reader(r);
			return PayloadHandlers[id].Read(r);
		}

		public static void WriteTag(string name, object tag, BinaryWriter w) {
			int id = GetPayloadId(tag.GetType());
			w.Write((byte)id);
			StringHandler.writer(w, name);
			PayloadHandlers[id].Write(w, tag);
		}

		public static TagCompound FromFile(string path, bool compressed = true) {
			try {
				using (Stream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
					return FromStream(fs, compressed);
			}
			catch (IOException e) {
				throw new IOException("Failed to read NBT file: "+path, e);
			}
		}

		public static TagCompound FromStream(Stream stream, bool compressed = true)
		{
			if (compressed) stream = new GZipStream(stream, CompressionMode.Decompress);
			return Read(new BigEndianReader(stream));
		}

		public static TagCompound Read(BinaryReader reader) {
			string name;
			var tag = ReadTag(reader, out name);
			if (!(tag is TagCompound))
				throw new IOException("Root tag not a TagCompound");

			return (TagCompound) tag;
		}

		public static void ToFile(TagCompound root, string path, bool compress = true) {
			try {
				using (Stream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
					ToStream(root, fs, compress);
			}
			catch (IOException e) {
				throw new IOException("Failed to read NBT file: "+path, e);
			}
		}

		public static void ToStream(TagCompound root, Stream stream, bool compress = true) {
			if (compress) stream = new GZipStream(stream, CompressionMode.Compress, true);
			Write(root, new BigEndianWriter(stream));
			if (compress) stream.Close();
		}

		public static void Write(TagCompound root, BinaryWriter writer) => WriteTag("", root, writer);

		public static T Clone<T>(T o) => (T)GetHandler(GetPayloadId(o.GetType())).Clone(o);
	}
}
