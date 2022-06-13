using System;
using System.ComponentModel;
using Terraria.ModLoader.Config;

// This file defines a custom data type that can be used as a key in dictionary.
namespace ExampleMod.Common.Configs.CustomDataTypes
{
	[TypeConverter(typeof(ToFromStringConverter<ClassUsedAsKey>))]
	public class ClassUsedAsKey
	{
		// When you save data from a dictionary into a file (json), you need to represent the key as a string
		// But to get the object back, you need a TypeConverter, and this example shows how to implement one

		// You start with the [TypeConverter(typeof(ToFromStringConverter<NameOfClassHere>))] attribute above the class
		// For this to work, you need the usual Equals and GetHashCode overrides as explained in the other examples,
		// plus ToString and FromString, which are used to transform your object into a string and back

		public bool SomeBool { get; set; }
		public int SomeNumber { get; set; }

		public override bool Equals(object obj) {
			if (obj is ClassUsedAsKey other)
				return SomeBool == other.SomeBool && SomeNumber == other.SomeNumber;
			return base.Equals(obj);
		}

		public override int GetHashCode() {
			return new { SomeBool, SomeNumber }.GetHashCode();
		}

		// Here you need to write how the string representation of your object will look like so it is easy to reconstruct again
		// Inside the json file, it will look something like this: "True, 5"
		public override string ToString() {
			return $"{SomeBool}, {SomeNumber}";
		}

		// Here you need to create an object from the given string (reverting ToString basically)
		// This has to be static and it must be named FromString
		public static ClassUsedAsKey FromString(string s) {
			// This following code depends on your implementation of ToString, here we just have two values separated by a ','
			string[] vars = s.Split(new char[] { ',' }, 2, StringSplitOptions.RemoveEmptyEntries);
			// The System.Convert class provides methods to transform data types between each other, here using the string overload
			return new ClassUsedAsKey {
				SomeBool = Convert.ToBoolean(vars[0]),
				SomeNumber = Convert.ToInt32(vars[1])
			};
		}
	}
}
