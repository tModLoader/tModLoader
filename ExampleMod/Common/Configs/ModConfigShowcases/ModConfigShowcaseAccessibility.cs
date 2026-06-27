using ExampleMod.Common.Configs.CustomDataTypes;
using Newtonsoft.Json;
using Terraria;
using Terraria.ModLoader.Config;

// This file contains fake ModConfig class that showcase using
// access modifiers (to control which fields should be visible and have their value saved to file)
// and properties (to implement simple "presets" system).

// Because this config was designed to show off various UI capabilities,
// this config have no effect on the mod and provides purely teaching example.
namespace ExampleMod.Common.Configs.ModConfigShowcases
{
	[BackgroundColor(164, 153, 190)]
	public class ModConfigShowcaseAccessibility : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		// Private and Internal fields and properties will not be shown.
		// Note that private and internal values will not be replaced by the deserialization, so initializer and ctor work.
		// You should avoid private and internal values in ModConfigs
#pragma warning disable CS0414
		private float Private = 144;
#pragma warning restore CS0414
		internal float Internal;

		// Public fields are most common. Use public for most items.
		public float Public;

		// Will not show. Avoid static. Due to how ModConfig works, static fields will not work correctly. Use a static field named Instance in the manner used in ExampleConfigServer for accessing ModConfig fields in the rest of your mod.
		public static float Static;

		// Get only properties will show up, but will be grayed out to show that they can't be changed.
		public float Getter => Main.rand?.NextFloat(1f) ?? 0; // This is just an example, please don't do this.

		// AutoProperties work the same as fields.
		public float AutoProperty { get; set; }

		// Properties work as well. The backing field will be ignored when writing the json out.
		private float propertyBackingField;
		public float Property {
			get { return propertyBackingField; }
			set { propertyBackingField = value + 0.2f; } // + 0.2f is just to mess with the user.
		}

		// Using JsonIgnore on a public field means the field won't show up in the json or UI. Not really useful.
		[JsonIgnore]
		public float Ignore;

		// Using ShowDespiteJsonIgnore overrides JsonIgnore for the UI. Use this to display info to the user if needed. The value won't be saved since it is derived from other fields.
		// Useful for things like displaying sums or calculated relationships.
		[JsonIgnore]
		[ShowDespiteJsonIgnore]
		public float IgnoreWithLabelGetter => AutoProperty + Public;

		// Reference type getters kind of work with the UI. You can experiment with this if you want.
		[JsonIgnore]
		public Pair pair2 => pair;
		public Pair pair;

		// Set only properties will crash tModLoader.
		// public float Setter { set { Public = value; } }

		// The following shows how you can use properties to implement a preset system
		public bool PresetA {
			get => Data1 == 23 && Data2 == 63;
			set {
				if (value) {
					Data1 = 23;
					Data2 = 63;
				}
			}
		}

		public bool PresetB {
			get => Data1 == 93 && Data2 == 13;
			set {
				if (value) {
					Data1 = 93;
					Data2 = 13;
				}
			}
		}

		[Slider]
		public int Data1 { get; set; }
		[Slider]
		public int Data2 { get; set; }

		public ModConfigShowcaseAccessibility() {
			Internal = 0.2f;
		}

		// ShouldSerialize{FieldNameHere}. ShouldSerialize can be useful, but this example is simply replicating the behavior of JSONIgnore and is just an example for examples sake. https://www.newtonsoft.com/json/help/html/ConditionalProperties.htm
		public bool ShouldSerializeGetter() {
			// We can have some logic in here to determine if the value is worth saving, but this is just a trivial example
			return false;
		}
	}
}
