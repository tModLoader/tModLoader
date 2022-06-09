using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Terraria.ModLoader.Config;
using ExampleMod.Common.Configs.CustomDataTypes;
using ExampleMod.Common.Configs.CustomUI;

// This file contains fake ModConfig class that showcase various attributes
// that can be used to customize behavior config fields.

// Because this config was designed to show off various UI capabilities,
// this config have no effect on the mod and provides purely teaching example.
namespace ExampleMod.Common.Configs.ModConfigShowcases
{
	/// <summary>
	/// This config is just a showcase of various attributes and their effects in the UI window.
	/// </summary>
	[Label("ModConfig Showcase G: Misc")]
	public class ModConfigShowcaseMisc : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[Label("Custom UI Element")]
		[Tooltip("This UI Element is modder defined")]
		[CustomModConfigItem(typeof(GradientElement))]
		public Gradient gradient = new Gradient();

		/*
		// Here are some more examples, showing a complex JsonDefaultListValue and a initializer overriding the defaults of the constructor.
		[CustomModConfigItem(typeof(GradientElement))]
		public Gradient gradient2 = new Gradient() {
			start = Color.AliceBlue,
			end = Color.DeepSkyBlue
		};

		[JsonDefaultListValue("{\"start\": \"238, 248, 255, 255\", \"end\": \"0, 191, 255, 255\"}")]
		public List<Gradient> gradients = new List<Gradient>();
		*/

		[Label("Custom UI Element 2")]
		// In this case, CustomModConfigItem is annotating the Enum instead of the Field. Either is acceptable and can be used for different situations.
		public Corner corner;

		// You can put multiple attributes in the same [] if you like.
		// ColorHueSliderAttribute displays Hue Saturation Lightness. Passing in false means only Hue is shown.
		[DefaultValue(typeof(Color), "255, 0, 0, 255"), ColorHSLSlider(false), ColorNoAlpha]
		public Color hsl;

		// In this example we inherit from a tmodloader config UIElement to slightly customize the colors.
		[CustomModConfigItem(typeof(CustomFloatElement))]
		public float tint;

		public Dictionary<string, Pair> StringPairDictionary = new Dictionary<string, Pair>();
		public Dictionary<ItemDefinition, float> JsonItemFloatDictionary = new Dictionary<ItemDefinition, float>();

		public HashSet<ItemDefinition> itemSet = new HashSet<ItemDefinition>();

		[Label("ListOfPair2 label")]
		public List<Pair> ListOfPair2 = new List<Pair>();
		public Pair pairExample2 = new Pair();

		public SimpleData simpleDataExample; // you can also initialize in the constructor, see initialization in public ModConfigShowcaseMisc() below.

		// This annotation allows the UI to null out this class. You need to make sure to initialize fields without the NullAllowed annotation in constructor or initializer or you might have issues. Of course, if you allow nulls, you'll need to make sure the rest of your mod will handle them correctly. Try to avoid null unless you have a good reason to use them, as null objects will only complicate the rest of your code.
		[NullAllowed]
		[JsonDefaultValue("{\"boost\": 777}")] // With NullAllowed, you can specify a default value like this.
		public SimpleData simpleDataExample2;

		[Label("Really Complex Data")]
		public ComplexData complexData = new ComplexData();

		[JsonExtensionData]
		private IDictionary<string, JToken> _additionalData = new Dictionary<string, JToken>();

		// See _additionalData usage in OnDeserializedMethod to see how this ListOfInts can be populated from old versions of this mod.
		public List<int> ListOfInts = new List<int>();

		public ModConfigShowcaseMisc() {
			simpleDataExample = new SimpleData();
			simpleDataExample.boost = 32;
			simpleDataExample.percent = 0.7f;
		}

		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext context) {
			// If you change ModConfig fields between versions, your users might notice their configuration is lost when they update their mod.
			// We can use [JsonExtensionData] to capture un-de-serialized data and manually restore them to new fields.
			// Imagine in a previous version of this mod, we had a field "OldListOfInts" and we want to preserve that data in "ListOfInts".
			// To test this, insert the following into ExampleMod_ModConfigShowcase.json: "OldListOfInts": [ 99, 999],
			if (_additionalData.TryGetValue("OldListOfInts", out var token)) {
				var OldListOfInts = token.ToObject<List<int>>();
				ListOfInts.AddRange(OldListOfInts);
			}
			_additionalData.Clear(); // make sure to clear this or it'll crash.
		}
	}
}
