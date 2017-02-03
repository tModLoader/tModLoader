using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace tModLoaderTests
{
	[TestClass]
	public class TagIOTests
	{
		[ClassInitialize]
		public static void ClassInit(TestContext context) {
			Main.dedServ = true;
			var main = new Main();
			typeof(Main).GetMethod("Initialize", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(main, new object[0]);
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

				var eType1 = list1.GetType().GetGenericArguments()[0];
				var eType2 = list2.GetType().GetGenericArguments()[0];
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
			try {
				var tag = new TagCompound();
				tag.Get<char>("key1");
				Assert.Fail("Test method did not throw expected exception System.IO.IOException.");
			}
			catch (IOException e) {
				Assert.AreEqual(e.Message, "NBT Deserialization (type=System.Char,entry=\"key1\" = null)");
				Assert.AreEqual(e.InnerException.Message, "Invalid NBT payload type 'System.Char'");
			}

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
  float ""floatMin"" = -3.402823E+38,
  float ""floatMax"" = 3.402823E+38,
  float ""floatNaN"" = NaN,
  float ""float"" = 1.525,
  double ""doubleMin"" = -1.79769313486232E+308,
  double ""doubleMax"" = 1.79769313486232E+308,
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
}");
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
}");

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
}");

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
}");
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
}");
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
			
			var item1 = new Item();
			item1.SetDefaults(ItemID.LastPrism, true);
			item1.favorited = true;
			var item2 = new Item();
			item2.SetDefaults(ItemID.IronBar, true);
			item2.stack = 25;
			var item3 = new Item();
			item3.SetDefaults(ItemID.Meowmere, true);
			item3.prefix = 4;

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
})");
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
	}
}
