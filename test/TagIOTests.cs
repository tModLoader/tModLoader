using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader
{
	[TestClass]
	public class TagIOTests
	{
		private class MockMod : Mod
		{
			public MockMod() {
				typeof(Mod).GetProperty("Code", BindingFlags.Instance | BindingFlags.Public)
					.SetValue(this, typeof(MockMod).Assembly);
			}
		}

		[ClassInitialize]
		public static void ClassInit(TestContext context) {
			//initialize a server context for ItemIO
			Program.SavePath = ".";
			Main.dedServ = true;

			// autoload the TagSerializers
			var loadableTypes = typeof(Main).Assembly.GetTypes()
				.Where(t => !t.IsAbstract && !t.ContainsGenericParameters)
				.Where(t => t.IsAssignableTo(typeof(TagSerializer)))
				.Where(t => t.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null) != null) // has default constructor
				.Where(t => AutoloadAttribute.GetValue(t).NeedsAutoloading)
				.OrderBy(type => type.FullName, StringComparer.InvariantCulture);

			foreach (var t in loadableTypes) {
				((ILoadable)Activator.CreateInstance(t, true)).Load(null);
			}

			// Add a mod for resolving TagSerializable types
			typeof(ModLoader).GetProperty(nameof(ModLoader.Mods), BindingFlags.Public | BindingFlags.Static).SetValue(null, new Mod[] {new MockMod()});
		}

		private void AssertEqual(object o1, object o2) {
			if (o1 is TagCompound) {
				var tag1 = (TagCompound) o1;
				var tag2 = (TagCompound) o2;
				Assert.AreEqual(tag1.Count, tag2.Count, "Different Tag Counts");
				foreach (var entry in tag1) {
					Assert.IsTrue(tag2.ContainsKey(entry.Key));
					var value1 = entry.Value;
					var value2 = tag2.Get<object>(entry.Key);

					Assert.AreEqual(value1.GetType(), value2.GetType());
					AssertEqual(value1, value2);
				}
			}
			else if (o1 is byte[]) {
				Assert.IsTrue(Enumerable.SequenceEqual((byte[]) o1, (byte[]) o2));
			}
			else if (o1 is int[]) {
				Assert.IsTrue(Enumerable.SequenceEqual((int[]) o1, (int[]) o2));
			}
			else if (o1 is IList) {
				var list1 = (IList) o1;
				var list2 = (IList) o2;

				var list1Type = list1.GetType();
				var list2Type = list2.GetType();

				var eType1 = list1Type.GetElementType() ?? list1Type.GetGenericArguments()[0];
				var eType2 = list2Type.GetElementType() ?? list2Type.GetGenericArguments()[0];
				Assert.AreEqual(eType1, eType2);

				Assert.AreEqual(list1.Count, list2.Count);
				for (int i = 0; i < list1.Count; i++)
					AssertEqual(list1[i], list2[i]);
			}
			else if (o1 is Item) {
				var item1 = (Item)o1;
				var item2 = (Item)o2;
				Assert.IsTrue(item1.netID == item2.netID);
				Assert.IsTrue(item1.stack == item2.stack);
				Assert.IsTrue(item1.prefix == item2.prefix);
				Assert.IsTrue(item1.favorited == item2.favorited);
			}
			else {
				Assert.AreEqual(o1, o2);
			}
		}

		private TagCompound AfterIO(TagCompound tag) {
			var stream = new MemoryStream();
			TagIO.ToStream(tag, stream);
			stream.Position = 0;
			return TagIO.FromStream(stream);
		}

		private TagCompound GetGeneralTestTag() {
			return new TagCompound {
				["byte2"] = (byte)2,
				["byte255"] = (byte)255,
				["short536"] = (short)536,
				["short-5"] = (short)-5,
				["intMin"] = int.MinValue,
				["intMax"] = int.MaxValue,
				["longMin"] = long.MinValue,
				["longMax"] = long.MaxValue,
				["floatMin"] = float.MinValue,
				["floatMax"] = float.MaxValue,
				["floatNaN"] = float.NaN,
				["float"] = 1.525f,
				["doubleMin"] = double.MinValue,
				["doubleMax"] = double.MaxValue,
				["doubleNaN"] = double.NaN,
				["double"] = 1.525,
				["string1"] = "",
				["string2"] = "Test string",
				["byteArr1"] = new byte[0],
				["byteArr2"] = new byte[] { 0, 5, 25, 125 },
				["intArr1"] = new int[0],
				["intArr2"] = new[] { -7, -5326, 32800 },
				["tag1"] = new TagCompound(),
				["tag2"] = new TagCompound { { "key", "value" } },
				["listByte1"] = new List<byte>(),
				["listByte2"] = new List<byte> { 1, 26 },
				["listShort1"] = new List<short>(),
				["listShort2"] = new List<short> { 2, 27 },
				["listInt1"] = new List<int>(),
				["listInt2"] = new List<int> { 3, 28 },
				["listLong1"] = new List<long>(),
				["listLong2"] = new List<long> { 4, 29 },
				["listFloat1"] = new List<float>(),
				["listFloat2"] = new List<float> { 5, 1.2345f },
				["listDouble1"] = new List<double>(),
				["listDouble2"] = new List<double> { 6, -23.25 },
				["listString1"] = new List<string>(),
				["listString2"] = new List<string> { "item1", "item2" },
				["listByteArr1"] = new List<byte[]>(),
				["listByteArr2"] = new List<byte[]> { new byte[0], new byte[] { 7, 15 } },
				["listIntArr1"] = new List<int[]>(),
				["listIntArr2"] = new List<int[]> { new int[0], new[] { 12, 3 } },
			};
		}

		private TagCompound GetNestedListTestTag() {
			return new TagCompound {
				["dynList"] = new List<IList> {
					new List<int> {1, 2, 3, 4, 5},
					new List<string> {"a", "ab", "abc"},
					new List<TagCompound> {
						new TagCompound {
							["key1"] = 2f,
							["key2"] = new byte[] {1, 2, 4},
							["key3"] = new TagCompound {
								["moarLists"] = new List<IList> {
									new List<long> {-1, -2, -3},
									new List<int[]> {
										new int[0],
										new[] {5, 6, 7, 8}
									}
								}
							}
						}
					},
					new List<List<TagCompound>> {
						new List<TagCompound>(),
						new List<TagCompound> {
							new TagCompound {
								["key"] = "value"
							}
						}
					}
				}
			};
		}

		//serialisation and deserialisation of a tag compound with base types
		[TestMethod]
		public void TestSerialisation() {
			var tag = GetGeneralTestTag();
			AssertEqual(tag, AfterIO(tag));
		}

		[TestMethod]
		[ExpectedException(typeof(System.ArgumentException))]
		public void TestDuplicate() {
			var tag = new TagCompound();
			tag.Add("key", "value");
			tag.Add("key", 5);
		}

		[TestMethod]
		public void TestRemoval() {
			var tag = new TagCompound {
				["key1"] = "value1",
				["key2"] = "value2"
			};
			tag.Remove("missing");
			tag.Remove("key1");
			tag.Add("key1", 5);
			tag.Remove("key2");
			AssertEqual(tag, new TagCompound {{"key1", 5}});
		}

		[TestMethod]
		public void TestClear() {
			var tag = new TagCompound {
				["key1"] = new List<byte>(),
				["key2"] = 5
			};
			tag.Clear();
			Assert.AreEqual(tag.Count, 0);
			tag.Add("key1", 5);
			AssertEqual(tag, new TagCompound {{"key1", 5}});
		}

		[TestMethod]
		public void TestAddNull() {
			var tag = new TagCompound {
				["key1"] = new List<byte>(),
				["key2"] = 5
			};
			tag.Add("key1", null);
			AssertEqual(tag, new TagCompound {{"key2", 5}});
		}

		[TestMethod]
		public void TestInvalidType() {
			try {
				new TagCompound { { "key1", 'v' } };
				Assert.Fail("Test method did not throw expected exception System.IO.IOException.");
			}
			catch (IOException e) {
				Assert.AreEqual(e.Message, "NBT Serialization (key=key1,type=System.Char,value=v)");
				Assert.AreEqual(e.InnerException.Message, "Invalid NBT payload type 'System.Char'");
			}

			try {
				new TagCompound { { "key1", new Guid() } };
				Assert.Fail("Test method did not throw expected exception System.IO.IOException.");
			}
			catch (IOException e) {
				Assert.AreEqual(e.Message, "NBT Serialization (key=key1,type=System.Guid,value=00000000-0000-0000-0000-000000000000)");
				Assert.AreEqual(e.InnerException.Message, "Invalid NBT payload type 'System.Guid'");
			}

			try {
				new TagCompound { { "key1", new List<char>() } };
				Assert.Fail("Test method did not throw expected exception System.IO.IOException.");
			}
			catch (IOException e) {
				Assert.AreEqual(e.Message, "NBT Serialization (key=key1,value=System.Collections.Generic.List`1[System.Char])");
				Assert.AreEqual(e.InnerException.Message, "Invalid NBT payload type 'System.Char'");
			}

			//not a valid NBT type, but it ends up as a List<IList>
			var tag = new TagCompound { { "key1", new List<List<char>>() } };
			tag.Get<List<List<char>>>("key1");
		}

		[TestMethod]
		public void TestWrongGetTagType() {
			//get a valid NBT type as a different valid NBT type
			try {
				var tag = new TagCompound { { "key1", 2 } };
				tag.Get<float>("key1");
				Assert.Fail("Test method did not throw expected exception System.IO.IOException.");
			}
			catch (IOException e) {
				Assert.AreEqual(e.Message, "NBT Deserialization (type=System.Single,entry=int \"key1\" = 2)");
				Assert.AreEqual(e.InnerException.Message, "Unable to cast object of type 'System.Int32' to type 'System.Single'");
			}

			//get a valid NBT type as an invalid NBT type
			try {
				var tag = new TagCompound { { "key1", 2 } };
				tag.Get<char>("key1");
				Assert.Fail("Test method did not throw expected exception System.IO.IOException.");
			}
			catch (IOException e) {
				Assert.AreEqual(e.Message, "NBT Deserialization (type=System.Char,entry=int \"key1\" = 2)");
				Assert.AreEqual(e.InnerException.Message, "Unable to cast object of type 'System.Int32' to type 'System.Char'");
			}

			//get a missing tag as an invalid NBT type
			// now permitted, returns default
			Assert.AreEqual(default, new TagCompound().Get<char>("key1"));

			//get a list tag as an invalid generic type
			try {
				var tag = new TagCompound { { "key1", new List<int>() } };
				tag.Get<IQueryable<int>>("key1");
				Assert.Fail("Test method did not throw expected exception System.IO.IOException.");
			}
			catch (IOException e) {
				Assert.AreEqual(e.Message, "NBT Deserialization (type=System.Linq.IQueryable`1[System.Int32],entry=int \"key1\" [])");
				Assert.AreEqual(e.InnerException.Message, "Unable to cast object of type 'System.Collections.Generic.List`1[System.Int32]' to type 'System.Linq.IQueryable`1[System.Int32]'");
			}

			//get a missing tag as an invalid generic type
			try {
				var tag = new TagCompound();
				tag.Get<IQueryable<int>>("key1");
				Assert.Fail("Test method did not throw expected exception System.IO.IOException.");
			}
			catch (IOException e) {
				Assert.AreEqual(e.Message, "NBT Deserialization (type=System.Linq.IQueryable`1[System.Int32],entry=\"key1\" = null)");
				Assert.AreEqual(e.InnerException.Message, "Invalid NBT payload type 'System.Linq.IQueryable`1[System.Int32]'");
			}
		}

		[TestMethod]
		public void TestBoolExtension() {
			var list = new List<bool> {true, false, true};
			var tag = new TagCompound {
				["true"] = true,
				["false"] = false,
				["list1"] = new List<bool>(),
				["list2"] = list,
				["list3"] = new List<List<bool>> {list}
			};
			AssertEqual(tag, new TagCompound {
				["true"] = (byte) 1,
				["false"] = (byte) 0,
				["list1"] = new List<byte>(),
				["list2"] = new List<byte> {1, 0, 1},
				["list3"] = new List<List<byte>> {new List<byte> {1, 0, 1}}
			});
			Assert.IsTrue(tag.Get<bool>("true"));
			Assert.IsFalse(tag.Get<bool>("false"));
			AssertEqual(tag.Get<bool>("default"), false);
			AssertEqual(tag.GetList<bool>("list1"), new List<bool>());
			AssertEqual(tag.GetList<bool>("list2"), list);
			AssertEqual(tag.GetList<IList<bool>>("list3"), new List<IList<bool>> {list});
			AssertEqual(tag.GetList<List<bool>>("list3"), new List<List<bool>> {list});
		}

		[TestMethod]
		public void TestDefaultValues() {
			var tag = new TagCompound();
			AssertEqual(tag.Get<byte>("key"), (byte)0);
			AssertEqual(tag.Get<short>("key"), (short)0);
			AssertEqual(tag.Get<int>("key"), 0);
			AssertEqual(tag.Get<long>("key"), 0L);
			AssertEqual(tag.Get<float>("key"), 0f);
			AssertEqual(tag.Get<double>("key"), 0d);
			AssertEqual(tag.Get<string>("key"), "");
			AssertEqual(tag.Get<byte[]>("key"), new byte[0]);
			AssertEqual(tag.Get<int[]>("key"), new int[0]);
			AssertEqual(tag.Get<TagCompound>("key"), new TagCompound());

			AssertEqual(tag.GetList<byte>("key"), new List<byte>());
			AssertEqual(tag.GetList<float>("key"), new List<float>());
			AssertEqual(tag.GetList<TagCompound>("key"), new List<TagCompound>());
			AssertEqual(tag.GetList<char>("key"), new List<char>());//not a valid NBT type, but can still create empty lists
			AssertEqual(tag.GetList<List<float>>("key"), new List<List<float>>());

			AssertEqual(tag.Get<IList<float>>("key"),  new List<float>());
			AssertEqual(tag.Get<IList<IList<float>>>("key"), new List<IList<float>>());

			AssertEqual(tag.Get<string[,]>("key"), new string[0, 0]);
			AssertEqual(tag.Get<string[][]>("key"), new string[0][]);
			AssertEqual(tag.Get<string[][,]>("key"), new string[0][,]);
			AssertEqual(tag.Get<string[,][]>("key"), new string[0, 0][]);
			AssertEqual(tag.Get<string[,][,]>("key"), new string[0, 0][,]);

			AssertEqual(tag.Get<Vector2[,]>("key"), new Vector2[0, 0]);
			AssertEqual(tag.Get<Vector2[][]>("key"), new Vector2[0][]);
			AssertEqual(tag.Get<Vector2[][,]>("key"), new Vector2[0][,]);
			AssertEqual(tag.Get<Vector2[,][]>("key"), new Vector2[0, 0][]);
			AssertEqual(tag.Get<Vector2[,][,]>("key"), new Vector2[0, 0][,]);

			AssertEqual(tag.Get<C[,]>("key"), new C[0, 0]);
			AssertEqual(tag.Get<C[][]>("key"), new C[0][]);
			AssertEqual(tag.Get<C[][,]>("key"), new C[0][,]);
			AssertEqual(tag.Get<C[,][]>("key"), new C[0, 0][]);
			AssertEqual(tag.Get<C[,][,]>("key"), new C[0, 0][,]);

			AssertEqual(tag.Get<List<string>[]>("key"), new List<string>[0]);
			AssertEqual(tag.Get<List<string[]>>("key"), new List<string[]>());
			AssertEqual(tag.Get<List<string[,]>>("key"), new List<string[,]>());
			AssertEqual(tag.Get<List<string>[,]>("key"), new List<string>[0, 0]);

			AssertEqual(tag.Get<List<Vector2>[]>("key"), new List<Vector2>[0]);
			AssertEqual(tag.Get<List<Vector2[]>>("key"), new List<Vector2[]>());
			AssertEqual(tag.Get<List<Vector2[,]>>("key"), new List<Vector2[,]>());
			AssertEqual(tag.Get<List<Vector2>[,]>("key"), new List<Vector2>[0, 0]);

			AssertEqual(tag.Get<List<C>[]>("key"), new List<C>[0]);
			AssertEqual(tag.Get<List<C[]>>("key"), new List<C[]>());
			AssertEqual(tag.Get<List<C[,]>>("key"), new List<C[,]>());
			AssertEqual(tag.Get<List<C>[,]>("key"), new List<C>[0, 0]);
		}

		[TestMethod]
		public void TestNullableGetters() {
			var tag = new TagCompound {
				["i"] = 5,
				["f"] = 3f
			};
			Assert.AreEqual(tag.Get<int?>("i"), 5);
			Assert.AreEqual(tag.Get<int?>(""), null);
			Assert.AreEqual(tag.Get<float?>("f"), 3f);
			Assert.AreEqual(tag.Get<float?>(""), null);

			try {
				tag.Get<byte?>("i");
				Assert.Fail("Test method did not throw expected exception System.IO.IOException.");
			}
			catch (IOException e) {
				Assert.AreEqual(e.Message, "NBT Deserialization (type=System.Nullable`1[System.Byte],entry=int \"i\" = 5)");
				Assert.AreEqual(e.InnerException.Message, "Unable to cast object of type 'System.Int32' to type 'System.Nullable`1[System.Byte]'");
			}
		}

		[TestMethod]
		public void TestTypePromotions() {
			var tag = new TagCompound {
				["byte1"] = (byte) 5,
				["byte2"] = (byte) 254,
				["short1"] = (short) 3200,
				["short2"] = (short) -3200,
				["int1"] = 0xFFEEFF,
				["long1"] = long.MaxValue,
				["float1"] = 1.25264f,
				["float2"] = float.MaxValue,
				["float3"] = float.NaN,
				["double1"] = 1.25264
			};
			Assert.AreEqual(tag.GetAsShort("byte1"), (short)5);
			Assert.AreEqual(tag.GetAsShort("byte2"), (short)254);
			Assert.AreEqual(tag.GetAsShort("short2"), -3200);
			Assert.AreEqual(tag.GetAsInt("byte1"), 5);
			Assert.AreEqual(tag.GetAsInt("byte2"), 254);
			Assert.AreEqual(tag.GetAsInt("short1"), 3200);
			Assert.AreEqual(tag.GetAsInt("short2"), -3200);
			Assert.AreEqual(tag.GetAsInt("int1"), 0xFFEEFF);
			Assert.AreEqual(tag.GetAsLong("byte1"), 5);
			Assert.AreEqual(tag.GetAsLong("byte2"), 254);
			Assert.AreEqual(tag.GetAsLong("short1"), 3200);
			Assert.AreEqual(tag.GetAsLong("short2"), -3200);
			Assert.AreEqual(tag.GetAsLong("int1"), 0xFFEEFF);
			Assert.AreEqual(tag.GetAsLong("long1"), long.MaxValue);
			Assert.AreEqual(tag.GetAsDouble("float1"), 1.25264f);
			Assert.AreEqual(tag.GetAsDouble("float2"), float.MaxValue);
			Assert.AreEqual(tag.GetAsDouble("float3"), double.NaN);
			Assert.AreEqual(tag.GetAsDouble("double1"), 1.25264);
		}

		[TestMethod]
		public void TestToString() {
			var tag = GetGeneralTestTag();
			Assert.AreEqual(tag.ToString(), @"{
  byte ""byte2"" = 2,
  byte ""byte255"" = 255,
  short ""short536"" = 536,
  short ""short-5"" = -5,
  int ""intMin"" = -2147483648,
  int ""intMax"" = 2147483647,
  long ""longMin"" = -9223372036854775808,
  long ""longMax"" = 9223372036854775807,
  float ""floatMin"" = -3.4028235E+38,
  float ""floatMax"" = 3.4028235E+38,
  float ""floatNaN"" = NaN,
  float ""float"" = 1.525,
  double ""doubleMin"" = -1.7976931348623157E+308,
  double ""doubleMax"" = 1.7976931348623157E+308,
  double ""doubleNaN"" = NaN,
  double ""double"" = 1.525,
  string ""string1"" = """",
  string ""string2"" = ""Test string"",
  byte[] ""byteArr1"" = [],
  byte[] ""byteArr2"" = [0, 5, 25, 125],
  int[] ""intArr1"" = [],
  int[] ""intArr2"" = [-7, -5326, 32800],
  object ""tag1"" {},
  object ""tag2"" {
    string ""key"" = ""value""
  },
  byte ""listByte1"" [],
  byte ""listByte2"" [1, 26],
  short ""listShort1"" [],
  short ""listShort2"" [2, 27],
  int ""listInt1"" [],
  int ""listInt2"" [3, 28],
  long ""listLong1"" [],
  long ""listLong2"" [4, 29],
  float ""listFloat1"" [],
  float ""listFloat2"" [5, 1.2345],
  double ""listDouble1"" [],
  double ""listDouble2"" [6, -23.25],
  string ""listString1"" [],
  string ""listString2"" [
    ""item1"",
    ""item2""
  ],
  byte[] ""listByteArr1"" [],
  byte[] ""listByteArr2"" [
    [],
    [7, 15]
  ],
  int[] ""listIntArr1"" [],
  int[] ""listIntArr2"" [
    [],
    [12, 3]
  ]
}".ReplaceLineEndings());
		}

		[TestMethod]
		public void TestNestedLists() {
			var tag = GetNestedListTestTag();
			AssertEqual(tag, AfterIO(tag));

			AssertEqual(tag.ToString(), @"{
  list ""dynList"" [
    int [1, 2, 3, 4, 5],
    string [
      ""a"",
      ""ab"",
      ""abc""
    ],
    object [
      {
        float ""key1"" = 2,
        byte[] ""key2"" = [1, 2, 4],
        object ""key3"" {
          list ""moarLists"" [
            long [-1, -2, -3],
            int[] [
              [],
              [5, 6, 7, 8]
            ]
          ]
        }
      }
    ],
    list [
      object [],
      object [
        {
          string ""key"" = ""value""
        }
      ]
    ]
  ]
}".ReplaceLineEndings());

			//verify that imposing an element type on a dynamic list throws the appropriate exception
			try {
				tag.GetList<TagCompound>("dynList");
				Assert.Fail("Test method did not throw expected exception System.IO.IOException.");
			}
			catch (IOException e) {
				Assert.IsTrue(e.Message.StartsWith("NBT Deserialization (type=System.Collections.Generic.List`1[Terraria.ModLoader.IO.TagCompound],entry=list \"dynList\" ["));
				Assert.AreEqual(e.InnerException.Message, "Unable to cast object of type 'System.Collections.Generic.List`1[System.Int32]' to type 'Terraria.ModLoader.IO.TagCompound'");
			}

			//verify that recovering a strongly typed list provides a copy with common content
			tag = new TagCompound {
				["list"] = new List<List<TagCompound>> {
					new List<TagCompound> {
						new TagCompound()
					}
				}
			};
			AssertEqual(tag.ToString(), @"{
  list ""list"" [
    object [
      {}
    ]
  ]
}".ReplaceLineEndings());

			//should modify the underlying tag
			var list1 = tag.GetList<IList>("list");
			list1.Add(new List<TagCompound>());
			list1[0].Add(new TagCompound());
			AssertEqual(tag.ToString(), @"{
  list ""list"" [
    object [
      {},
      {}
    ],
    object []
  ]
}".ReplaceLineEndings());
			var list2 = tag.GetList<List<TagCompound>>("list");
			list2.RemoveAt(1);//no effect on underlying tag
			list2[0][1]["key"] = "value";//effect on underlying tag
			AssertEqual(tag.ToString(), @"{
  list ""list"" [
    object [
      {},
      {
        string ""key"" = ""value""
      }
    ],
    object []
  ]
}".ReplaceLineEndings());
		}

		[TestMethod]
		public void TestColorExtension() {
			var list = new List<Color> { Color.Red, Color.Lime, Color.Blue };
			var tag = new TagCompound {
				["color1"] = Color.Aqua,
				["color2"] = Color.Violet,
				["list1"] = new List<Color>(),
				["list2"] = list,
			};
			AssertEqual(tag, new TagCompound {
				["color1"] = (int)Color.Aqua.PackedValue,
				["color2"] = (int)Color.Violet.PackedValue,
				["list1"] = new List<int>(),
				["list2"] = new List<uint> { 0xFF0000FF, 0xFF00FF00, 0xFFFF0000 }//use internal uint -> int conversion
			});
			Assert.AreEqual(tag.Get<Color>("color1"), Color.Aqua);
			Assert.AreEqual(tag.Get<Color>("color2"), Color.Violet);
			Assert.AreEqual(tag.Get<Color>("default"), new Color());
			AssertEqual(tag.GetList<Color>("list1"), new List<Color>());
			AssertEqual(tag.GetList<Color>("list2"), list);
		}

		[TestMethod]
		public void TestVector2Extension() {
			var list = new List<Vector2> { Vector2.UnitX, Vector2.UnitY };
			var tag = new TagCompound {
				["vec1"] = new Vector2(-3, 5),
				["vec2"] = new Vector2((float)Math.PI),
				["list"] = list
			};
			AssertEqual(tag, new TagCompound {
				["vec1"] = new TagCompound {
					["x"] = -3f,
					["y"] = 5f
				},
				["vec2"] = new TagCompound {
					["x"] = (float)Math.PI,
					["y"] = (float)Math.PI
				},
				["list"] = new List<TagCompound> {
					new TagCompound {
						["x"] = 1f,
						["y"] = 0f
					}, new TagCompound {
						["x"] = 0f,
						["y"] = 1f
					}
				}
			});
			Assert.AreEqual(tag.Get<Vector2>("vec1"), new Vector2(-3, 5));
			Assert.AreEqual(tag.Get<Vector2>("vec2"), new Vector2((float)Math.PI));
			Assert.AreEqual(tag.Get<Vector2>("default"), new Vector2());
			AssertEqual(tag.GetList<Vector2>("list"), list);
		}

		[TestMethod]
		public void TestItemSerializable() {
			//unfortunately can't test modded items or global data

			var item1 = new Item(ItemID.LastPrism) { favorited = true };
			var item2 = new Item(ItemID.IronBar) { stack = 25 };
			var item3 = new Item(ItemID.Meowmere) { prefix = 4 };

			var list = new List<Item> {item1, item2, item3};
			var tag = new TagCompound {
				["item1"] = item1,
				["item2"] = item2,
				["item3"] = item3,
				["list"] = list
			};

			var sTag = new TagCompound {
				["item1"] = new TagCompound {
					["<type>"] = typeof(Item).FullName,
					["mod"] = "Terraria",
					["id"] = item1.netID,
					["fav"] = true
				},
				["item2"] = new TagCompound {
					["<type>"] = typeof(Item).FullName,
					["mod"] = "Terraria",
					["id"] = item2.netID,
					["stack"] = 25
				},
				["item3"] = new TagCompound {
					["<type>"] = typeof(Item).FullName,
					["mod"] = "Terraria",
					["id"] = item3.netID,
					["prefix"] = (byte)4
				}
			};
			sTag["list"] = new[] {"item1", "item2", "item3"}.Select(sTag.GetCompound).ToList();
			AssertEqual(tag, sTag);

			AssertEqual(tag.Get<Item>("item1"), item1);
			AssertEqual(tag.Get<Item>("item2"), item2);
			AssertEqual(tag.Get<Item>("item3"), item3);
			AssertEqual(tag.Get<Item>("default"), new Item());

			AssertEqual(tag.GetList<Item>("list"), list);
		}

		[TestMethod]
		public void TestItemSerializerCompatibility() {
			var item = new Item();
			item.SetDefaults(ItemID.Meowmere, true);
			item.prefix = 4;
			item.stack = 25;
			item.favorited = true;

			var tag = new TagCompound {
				["item"] = ItemIO.Save(item)
			};

			AssertEqual(item, tag.Get<Item>("item"));
		}

		[TestMethod]
		public void TestDeserializerException() {
			try {
				var tag = new TagCompound { { "item", 0 } };
				tag.Get<Item>("item");
				Assert.Fail("Test method did not throw expected exception System.IO.IOException.");
			}
			catch (IOException e) {
				Assert.AreEqual(e.Message, "NBT Deserialization (type=Terraria.Item,entry=int \"item\" = 0)");
				Assert.AreEqual(e.InnerException.Message, "Unable to cast object of type 'System.Int32' to type 'Terraria.ModLoader.IO.TagCompound'.");
			}

			try {
				var item = new Item();
				item.SetDefaults(ItemID.IronShortsword);

				var tag = new TagCompound {
					["item"] = item
				};
				tag.GetCompound("item").Set("prefix", "string");
				tag.Get<Item>("item");
				Assert.Fail("Test method did not throw expected exception System.IO.IOException.");
			}
			catch (IOException e) {
				Assert.AreEqual(e.Message, @"NBT Deserialization (type=Terraria.Item,entry=object ""item"" {
  string ""mod"" = ""Terraria"",
  int ""id"" = 6,
  string ""<type>"" = ""Terraria.Item"",
  string ""prefix"" = ""string""
})".ReplaceLineEndings());
				Assert.AreEqual(e.InnerException.Message, "NBT Deserialization (type=System.Byte,entry=string \"prefix\" = \"string\")");
				Assert.AreEqual(e.InnerException.InnerException.Message, "Unable to cast object of type 'System.String' to type 'System.Byte'");
			}
		}

		[TestMethod]
		public void TestClone() {
			//clone a complex nested tag tree
			var tag1 = GetNestedListTestTag();
			var tag2 = (TagCompound)tag1.Clone();
			AssertEqual(tag1, tag2);

			//modify the root tag
			tag2["new"] = 0;
			Assert.AreNotEqual(tag1.Count, tag2.Count);

			//test a list in the root tag
			var list1 = tag1.GetList<IList>("dynList");
			var list2 = tag2.GetList<IList>("dynList");
			AssertEqual(list1, list2);
			list2.Add(new List<int>());
			Assert.AreNotEqual(list1.Count, list2.Count);

			//test a tag in a list
			var tag3 = (TagCompound)list1[2][0];
			var tag4 = (TagCompound)list2[2][0];
			AssertEqual(tag3, tag4);
			tag4["new"] = 0;
			Assert.AreNotEqual(tag3.Count, tag4.Count);

			//test a tag in a tag in a list
			var tag5 = tag3.GetCompound("key3");
			var tag6 = tag4.GetCompound("key3");
			AssertEqual(tag5, tag6);
			tag6["new"] = 0;
			Assert.AreNotEqual(tag5.Count, tag6.Count);

			//test a list in a list in a tag in a tag in a list
			var list3 = (List<int[]>)tag5.GetList<IList>("moarLists")[1];
			var list4 = (List<int[]>)tag6.GetList<IList>("moarLists")[1];
			AssertEqual(list3, list4);
			list3.Add(new int[0]);
			Assert.AreNotEqual(list3.Count, list4.Count);

			//test an array in above list
			var arr1 = list3[1];
			var arr2 = list4[1];
			AssertEqual(arr1, arr2);
			arr1[3] = 0;
			Assert.IsFalse(Enumerable.SequenceEqual(arr1, arr2));
		}

		class A : TagSerializable
		{
			public virtual TagCompound SerializeData() => new TagCompound();
		}

		class B : A
		{
			public static Func<TagCompound, B> DESERIALIZER = tag => new B();
		}

		class C : B
		{
			public new static Func<TagCompound, C> DESERIALIZER = tag => new C(tag.GetInt("value"));
			public int value;

			public C(int value) {
				this.value = value;
			}

			public override TagCompound SerializeData() => new TagCompound { { "value", value } };
		}

		class C2 : TagSerializable
		{
			public static Func<TagCompound, C2> DESERIALIZER = tag => new C2(tag.GetInt("value"));
			public int value;

			public C2(int value) {
				this.value = value;
			}

			public TagCompound SerializeData() => new TagCompound { { "value", value } };
		}

		[TestMethod]
		public void TestSerializableInheritance() {
			var tag = new TagCompound {
				["a"] = new A(),//no deserializer so unable to get it back
				["b"] = new B(),
				["c"] = new C(10),
				["list"] = new List<A> {new B(), new C(12)}
			};

			//can use Get<A> even though A does not have a deserializer provided that the instance has <type> that is subtype of A
			Assert.IsTrue(tag.Get<A>("b") is B);
			Assert.IsTrue(tag.Get<A>("c") is C);
			Assert.IsTrue(tag.Get<B>("c") is C);

			var list = tag.GetList<A>("list");
			Assert.IsTrue(list[0] is B);
			Assert.IsTrue(list[1] is C);

			var list2 = tag.GetList<B>("list");
			Assert.IsTrue(list2[1] is C);

			//missing deserializer for A
			try {
				tag.Get<A>("a");
				Assert.Fail("Test method did not throw expected exception System.IO.IOException.");
			}
			catch (IOException e) {
				Assert.AreEqual(e.Message, @"NBT Deserialization (type=Terraria.ModLoader.TagIOTests+A,entry=object ""a"" {
  string ""<type>"" = ""Terraria.ModLoader.TagIOTests+A""
})".ReplaceLineEndings());
				Assert.AreEqual(e.InnerException.Message, "Missing deserializer for type 'Terraria.ModLoader.TagIOTests+A'.");
			}
		}

		[TestMethod]
		public void TestSerializableRefactoring() {
			var tag = new TagCompound {
				["c"] = new C(15),
				["list"] = new List<C> { new(17) },
				["2darr"] = new C[1,1] { { new(19) } },
			};
			//can get C as C2 because their deserializers are compatible
			Assert.AreEqual(tag.Get<C2>("c").value, 15);
			Assert.AreEqual(tag.GetList<C2>("list")[0].value, 17);
			Assert.AreEqual(tag.Get<C2[]>("list")[0].value, 17);
			Assert.AreEqual(tag.Get<C2[,]>("2darr")[0, 0].value, 19);
			//note that inheritance won't work unless the C2 deserializer deliberately looks at the <type> field
		}

		[TestMethod]
		public void TestArrayListInterchangeability() {
			var stringArray = new string[]      { "one", "two", "three" };
			var stringList  = new List<string>  { "one", "two", "three" };
			var vectorArray = new Vector2[]     { new(1, 2), new(3, 4), new(5, 6) };
			var vectorList  = new List<Vector2> { new(1, 2), new(3, 4), new(5, 6) };
			var cArray      = new C[]           { new(1), new(2), new(3) };
			var cList       = new List<C>       { new(1), new(2), new(3) };
			var c2Array     = new C2[]          { new(1), new(2), new(3) };
			var c2List      = new List<C2>      { new(1), new(2), new(3) };

			TagCompound tag = new() {
				["stringArray"] = stringArray,
				["stringList"] = stringList,
				["vectorArray"] = vectorArray,
				["vectorList"] = vectorList,
				["cArray"] = cArray,
				["cList"] = cList,
				["c2Array"] = c2Array,
				["c2List"] = c2List,
			};

			void Check(TagCompound tag)
			{
				Assert.IsTrue(stringList .Zip(tag.Get<List<string>> ("stringArray")).All(t => t.First == t.Second));
				Assert.IsTrue(stringArray.Zip(tag.Get<string[]>     ("stringList")) .All(t => t.First == t.Second));
				Assert.IsTrue(vectorList .Zip(tag.Get<List<Vector2>>("vectorArray")).All(t => t.First == t.Second));
				Assert.IsTrue(vectorArray.Zip(tag.Get<Vector2[]>    ("vectorList")) .All(t => t.First == t.Second));
				Assert.IsTrue(cList      .Zip(tag.Get<List<C>>      ("cArray"))     .All(t => t.First.value == t.Second.value));
				Assert.IsTrue(cArray     .Zip(tag.Get<C[]>          ("cList"))      .All(t => t.First.value == t.Second.value));
				Assert.IsTrue(c2List     .Zip(tag.Get<List<C2>>     ("c2Array"))    .All(t => t.First.value == t.Second.value));
				Assert.IsTrue(c2Array    .Zip(tag.Get<C2[]>         ("c2List"))     .All(t => t.First.value == t.Second.value));
			}

			Check(tag);
			Check(AfterIO(tag));
			Check((TagCompound)tag.Clone());
		}

		[TestMethod]
		public void TestMultiDimensionalArrays() {
			string[,]     s1 = new string[1,1]    {                   { "one" } };
			string[][]    s2 = new string[1][]    {   new string[]    { "one" } };
			string[][,]   s3 = new string[1][,]   {   new string[,] { { "one" } } };
			string[,][]   s4 = new string[1,1][]  { { new string[]    { "one" } } };
			string[,][,]  s5 = new string[1,1][,] { { new string[,] { { "one" } } } };

			Vector2[,]    v1 = new Vector2[1,1]    {                    { Vector2.One } };
			Vector2[][]   v2 = new Vector2[1][]    {   new Vector2[]    { Vector2.One } };
			Vector2[][,]  v3 = new Vector2[1][,]   {   new Vector2[,] { { Vector2.One } } };
			Vector2[,][]  v4 = new Vector2[1,1][]  { { new Vector2[]    { Vector2.One } } };
			Vector2[,][,] v5 = new Vector2[1,1][,] { { new Vector2[,] { { Vector2.One } } } };

			C[,]          c1 = new C[1,1]    {            {   new C(1) } };
			C[][]         c2 = new C[1][]    {   new C[]  {   new C(1) } };
			C[][,]        c3 = new C[1][,]   {   new C[,] { { new C(1) } } };
			C[,][]        c4 = new C[1,1][]  { { new C[]    { new C(1) } } };
			C[,][,]       c5 = new C[1,1][,] { { new C[,] { { new C(1) } } } };

			TagCompound tag = new() {
				 ["s1"] = s1, ["v1"] = v1, ["c1"] = c1,
				 ["s2"] = s2, ["v2"] = v2, ["c2"] = c2,
				 ["s3"] = s3, ["v3"] = v3, ["c3"] = c3,
				 ["s4"] = s4, ["v4"] = v4, ["c4"] = c4,
				 ["s5"] = s5, ["v5"] = v5, ["c5"] = c5,
			};

			void Check(TagCompound tag)
			{
				Assert.IsTrue(s1[0, 0]       == tag.Get<string[,]>    ("s1")[0, 0]);
				Assert.IsTrue(s2[0][0]       == tag.Get<string[][]>   ("s2")[0][0]);
				Assert.IsTrue(s3[0][0, 0]    == tag.Get<string[][,]>  ("s3")[0][0, 0]);
				Assert.IsTrue(s4[0, 0][0]    == tag.Get<string[,][]>  ("s4")[0, 0][0]);
				Assert.IsTrue(s5[0, 0][0, 0] == tag.Get<string[,][,]> ("s5")[0, 0][0, 0]);

				Assert.IsTrue(v1[0, 0]       == tag.Get<Vector2[,]>   ("v1")[0, 0]);
				Assert.IsTrue(v2[0][0]       == tag.Get<Vector2[][]>  ("v2")[0][0]);
				Assert.IsTrue(v3[0][0, 0]    == tag.Get<Vector2[][,]> ("v3")[0][0, 0]);
				Assert.IsTrue(v4[0, 0][0]    == tag.Get<Vector2[,][]> ("v4")[0, 0][0]);
				Assert.IsTrue(v5[0, 0][0, 0] == tag.Get<Vector2[,][,]>("v5")[0, 0][0, 0]);

				Assert.IsTrue(c1[0, 0].value       == tag.Get<C[,]>   ("c1")[0, 0].value);
				Assert.IsTrue(c2[0][0].value       == tag.Get<C[][]>  ("c2")[0][0].value);
				Assert.IsTrue(c3[0][0, 0].value    == tag.Get<C[][,]> ("c3")[0][0, 0].value);
				Assert.IsTrue(c4[0, 0][0].value    == tag.Get<C[,][]> ("c4")[0, 0][0].value);
				Assert.IsTrue(c5[0, 0][0, 0].value == tag.Get<C[,][,]>("c5")[0, 0][0, 0].value);
			}

			Check(tag);
			Check(AfterIO(tag));
			Check((TagCompound)tag.Clone());
		}

		[TestMethod]
		public void TestMultiDimensionalArrays2() {
			List<string>[]   s1 = new List<string>[]    {   new List<string> { "one" } };
			List<string[]>   s2 = new List<string[]>    {   new string[]     { "one" } };
			List<string[,]>  s3 = new List<string[,]>   {   new string[,]  { { "one" } } };
			List<string>[,]  s4 = new List<string>[1,1] { { new List<string> { "one" } } };

			List<Vector2>[]  v1 = new List<Vector2>[]    {   new List<Vector2> { Vector2.One } };
			List<Vector2[]>  v2 = new List<Vector2[]>    {   new Vector2[]     { Vector2.One } };
			List<Vector2[,]> v3 = new List<Vector2[,]>   {   new Vector2[,]  { { Vector2.One } } };
			List<Vector2>[,] v4 = new List<Vector2>[1,1] { { new List<Vector2> { Vector2.One } } };

			List<C>[]        c1 = new List<C>[]    {   new List<C> { new C(1) } };
			List<C[]>        c2 = new List<C[]>    {   new C[]     { new C(1) } };
			List<C[,]>       c3 = new List<C[,]>   {   new C[,]  { { new C(1) } } };
			List<C>[,]       c4 = new List<C>[1,1] { { new List<C> { new C(1) } } };

			TagCompound tag = new() {
				 ["s1"] = s1, ["v1"] = v1, ["c1"] = c1,
				 ["s2"] = s2, ["v2"] = v2, ["c2"] = c2,
				 ["s3"] = s3, ["v3"] = v3, ["c3"] = c3,
				 ["s4"] = s4, ["v4"] = v4, ["c4"] = c4,
			};

			void Check(TagCompound tag)
			{
				Assert.IsTrue(s1[0][0]    == tag.Get<List<string>[]>  ("s1")[0][0]);
				Assert.IsTrue(s2[0][0]    == tag.Get<List<string[]>>  ("s2")[0][0]);
				Assert.IsTrue(s3[0][0, 0] == tag.Get<List<string[,]>> ("s3")[0][0, 0]);
				Assert.IsTrue(s4[0, 0][0] == tag.Get<List<string>[,]> ("s4")[0, 0][0]);

				Assert.IsTrue(v1[0][0]    == tag.Get<List<Vector2>[]> ("v1")[0][0]);
				Assert.IsTrue(v2[0][0]    == tag.Get<List<Vector2[]>> ("v2")[0][0]);
				Assert.IsTrue(v3[0][0, 0] == tag.Get<List<Vector2[,]>>("v3")[0][0, 0]);
				Assert.IsTrue(v4[0, 0][0] == tag.Get<List<Vector2>[,]>("v4")[0, 0][0]);

				Assert.IsTrue(c1[0][0].value    == tag.Get<List<C>[]> ("c1")[0][0].value);
				Assert.IsTrue(c2[0][0].value    == tag.Get<List<C[]>> ("c2")[0][0].value);
				Assert.IsTrue(c3[0][0, 0].value == tag.Get<List<C[,]>>("c3")[0][0, 0].value);
				Assert.IsTrue(c4[0, 0][0].value == tag.Get<List<C>[,]>("c4")[0, 0][0].value);
			}

			Check(tag);
			Check(AfterIO(tag));
			Check((TagCompound)tag.Clone());
		}

		[TestMethod]
		public void TestMultiDimensionalArrayDuckTyping() {

			C[,]       c1 = new C[1,1]       {             {   new C(1) } };
			C[][]      c2 = new C[1][]       {   new C[]   {   new C(1) } };
			C[][,]     c3 = new C[1][,]      {   new C[,]  { { new C(1) } } };
			C[,][]     c4 = new C[1,1][]     { { new C[]     { new C(1) } } };
			C[,][,]    c5 = new C[1,1][,]    { { new C[,]  { { new C(1) } } } };

			List<C>[]  c6 = new List<C>[]    {   new List<C> { new C(1) } };
			List<C[]>  c7 = new List<C[]>    {   new C[]     { new C(1) } };
			List<C[,]> c8 = new List<C[,]>   {   new C[,]  { { new C(1) } } };
			List<C>[,] c9 = new List<C>[1,1] { { new List<C> { new C(1) } } };

			Vector2[,]    v1 = new Vector2[1,1]    {                    { Vector2.One } };
			Vector2[][]   v2 = new Vector2[1][]    {   new Vector2[]    { Vector2.One } };
			Vector2[][,]  v3 = new Vector2[1][,]   {   new Vector2[,] { { Vector2.One } } };
			Vector2[,][]  v4 = new Vector2[1,1][]  { { new Vector2[]    { Vector2.One } } };
			Vector2[,][,] v5 = new Vector2[1,1][,] { { new Vector2[,] { { Vector2.One } } } };

			List<Vector2>[]  v6 = new List<Vector2>[]    {   new List<Vector2> { Vector2.One } };
			List<Vector2[]>  v7 = new List<Vector2[]>    {   new Vector2[]     { Vector2.One } };
			List<Vector2[,]> v8 = new List<Vector2[,]>   {   new Vector2[,]  { { Vector2.One } } };
			List<Vector2>[,] v9 = new List<Vector2>[1,1] { { new List<Vector2> { Vector2.One } } };

			bool[,]    b1 = new bool[,]          {                  { true } };
			bool[][]   b2 = new bool[][]         {   new bool[]     { true } };
			bool[][,]  b3 = new bool[][,]        {   new bool[,]  { { true } } };
			bool[,][]  b4 = new bool[,][]        { { new bool[]     { true } } };
			bool[,][,] b5 = new bool[,][,]       { { new bool[,]  { { true } } } };

			List<bool>[]  b6 = new List<bool>[]  {   new List<bool> { true } };
			List<bool[]>  b7 = new List<bool[]>  {   new bool[]     { true } };
			List<bool[,]> b8 = new List<bool[,]> {   new bool[,]  { { true } } };
			List<bool>[,] b9 = new List<bool>[,] { { new List<bool> { true } } };

			TagCompound tag = new() {
				["c1"] = c1, ["v1"] = v1, ["b1"] = b1,
				["c2"] = c2, ["v2"] = v2, ["b2"] = b2,
				["c3"] = c3, ["v3"] = v3, ["b3"] = b3,
				["c4"] = c4, ["v4"] = v4, ["b4"] = b4,
				["c5"] = c5, ["v5"] = v5, ["b5"] = b5,
				["c6"] = c6, ["v6"] = v6, ["b6"] = b6,
				["c7"] = c7, ["v7"] = v7, ["b7"] = b7,
				["c8"] = c8, ["v8"] = v8, ["b8"] = b8,
				["c9"] = c9, ["v9"] = v9, ["b9"] = b9,
			};

			void Check(TagCompound tag)
			{
				Assert.IsTrue(c1[0, 0].value       == tag.Get<C2[,]>       ("c1")[0, 0].value);
				Assert.IsTrue(c2[0][0].value       == tag.Get<C2[][]>      ("c2")[0][0].value);
				Assert.IsTrue(c3[0][0, 0].value    == tag.Get<C2[][,]>     ("c3")[0][0, 0].value);
				Assert.IsTrue(c4[0, 0][0].value    == tag.Get<C2[,][]>     ("c4")[0, 0][0].value);
				Assert.IsTrue(c5[0, 0][0, 0].value == tag.Get<C2[,][,]>    ("c5")[0, 0][0, 0].value);

				Assert.IsTrue(c6[0][0].value       == tag.Get<List<C2>[]>  ("c6")[0][0].value);
				Assert.IsTrue(c7[0][0].value       == tag.Get<List<C2[]>>  ("c7")[0][0].value);
				Assert.IsTrue(c8[0][0, 0].value    == tag.Get<List<C2[,]>> ("c8")[0][0, 0].value);
				Assert.IsTrue(c9[0, 0][0].value    == tag.Get<List<C2>[,]> ("c9")[0, 0][0].value);

				Assert.IsTrue(new Vector3(v1[0, 0], 0)       == tag.Get<Vector3[,]>      ("v1")[0, 0]);
				Assert.IsTrue(new Vector3(v2[0][0], 0)       == tag.Get<Vector3[][]>     ("v2")[0][0]);
				Assert.IsTrue(new Vector3(v3[0][0, 0], 0)    == tag.Get<Vector3[][,]>    ("v3")[0][0, 0]);
				Assert.IsTrue(new Vector3(v4[0, 0][0], 0)    == tag.Get<Vector3[,][]>    ("v4")[0, 0][0]);
				Assert.IsTrue(new Vector3(v5[0, 0][0, 0], 0) == tag.Get<Vector3[,][,]>   ("v5")[0, 0][0, 0]);

				Assert.IsTrue(new Vector3(v6[0][0], 0)       == tag.Get<List<Vector3>[]> ("v6")[0][0]);
				Assert.IsTrue(new Vector3(v7[0][0], 0)       == tag.Get<List<Vector3[]>> ("v7")[0][0]);
				Assert.IsTrue(new Vector3(v8[0][0, 0], 0)    == tag.Get<List<Vector3[,]>>("v8")[0][0, 0]);
				Assert.IsTrue(new Vector3(v9[0, 0][0], 0)    == tag.Get<List<Vector3>[,]>("v9")[0, 0][0]);

				Assert.IsTrue((byte)(b1[0, 0]       ? 1 : 0) == tag.Get<byte[,]>      ("b1")[0, 0]);
				Assert.IsTrue((byte)(b2[0][0]       ? 1 : 0) == tag.Get<byte[][]>     ("b2")[0][0]);
				Assert.IsTrue((byte)(b3[0][0, 0]    ? 1 : 0) == tag.Get<byte[][,]>    ("b3")[0][0, 0]);
				Assert.IsTrue((byte)(b4[0, 0][0]    ? 1 : 0) == tag.Get<byte[,][]>    ("b4")[0, 0][0]);
				Assert.IsTrue((byte)(b5[0, 0][0, 0] ? 1 : 0) == tag.Get<byte[,][,]>   ("b5")[0, 0][0, 0]);

				Assert.IsTrue((byte)(b6[0][0]       ? 1 : 0) == tag.Get<List<byte>[]> ("b6")[0][0]);
				Assert.IsTrue((byte)(b7[0][0]       ? 1 : 0) == tag.Get<List<byte[]>> ("b7")[0][0]);
				Assert.IsTrue((byte)(b8[0][0, 0]    ? 1 : 0) == tag.Get<List<byte[,]>>("b8")[0][0, 0]);
				Assert.IsTrue((byte)(b9[0, 0][0]    ? 1 : 0) == tag.Get<List<byte>[,]>("b9")[0, 0][0]);
			}

			Check(tag);
			Check(AfterIO(tag));
			Check((TagCompound)tag.Clone());
		}
	}
}
