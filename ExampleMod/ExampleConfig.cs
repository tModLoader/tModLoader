using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;

namespace ExampleMod
{
	// This file contains 2 real ModConfigs (and also a bunch of fake ModConfigs showcasing various ideas). One is set to ConfigScope.ServerSide and the other ConfigScope.ClientSide
	// ModConfigs contain Public Fields and Properties that represent the choices available to the user. 
	// Those Fields or Properties will be presented to users in the Config menu.
	// DONT use static members anywhere in this class (except for a variable named Instance, see below), tModLoader maintains several instances of ModConfig classes which will not work well with static properties or fields.

	/// <summary>
	/// ExampleConfigServer has Server-wide effects. Things that happen on the server, on the world, or influence autoload go here
	/// ConfigScope.ServerSide ModConfigs are SHARED from the server to all clients connecting in MP.
	/// </summary>
	public class ExampleConfigServer : ModConfig
	{
		// You MUST specify a ConfigScope.
		public override ConfigScope Mode => ConfigScope.ServerSide;

		// tModLoader will automatically populate a public static field named Instance with the active instance of this ModConfig. (It will unload it too.)
		// This reduces code from "mod.GetConfig<ExampleConfigServer>().DisableExampleWings" to "ExampleConfigServer.Instance.DisableExampleWings". It's just a style choice.
		public static ExampleConfigServer Instance;

		// We will use attributes to annotate our fields or properties so tModLoader can properly handle them.

		// First, we will learn about DefaultValue. You might assume "public bool BoolExample = true;" to work, 
		// but because tModLoader is overwriting with JSON, that value will be overwritten when the mod loads.
		// We must use the DefaultValue attribute instead of setting the value normally:
		[DefaultValue(true)]
		public bool UselessBoolExample;

		// This is private. You'll notice that it doesn't show up in the config menu. Don't set something private.
#pragma warning disable CS0169 // Unused field
		private bool PrivateFieldBoolExample;
#pragma warning restore CS0169

		// This is ignored, it also shouldn't show up in the config menu despite being public.
		[JsonIgnore]
		public bool IgnoreExample;

		// You'll notice this next one is a Property instead of a field. That works too.
		// Here we see an attribute added by tModLoader: LabelAttribute. This one allows us to add a label so the user knows more about the setting they are changing. Without a label, the name of the field or property is displayed.
		[Label("Disable Example Wings Item")]
		// Similar to Label, this sets the tooltip. Tooltips are useful for slightly longer and more detailed explanations of config options.
		[Tooltip("Prevents Loading the ExampleWings item. Requires a Reload")]
		// ReloadRequired hints that if this value is changed, a reload is required for the mod to properly work. 
		// Here we use it so if we disable ExampleWings from being loaded, we can properly prevent autoload in ExampleWings.cs
		// Failure to properly use ReloadRequired will cause many, many problems including ID desync.
		[ReloadRequired]
		public bool DisableExampleWings { get; set; }

		[Label("Disable Volcanos")]
		// Our game logic can handle toggling this setting in-game, so you'll notice we do NOT decorate this property with ReloadRequired
		public bool DisableVolcanos { get; set; }

		// Watch in action: https://gfycat.com/SickTerribleHoatzin
		[Label("Example Person free gift list")]
		[Tooltip("Each player can claim one free item from this list from Example Person\nSell the item back to Example Person to take a new item")]
		public List<ItemDefinition> ExamplePersonFreeGiftList { get; set; } = new List<ItemDefinition>();

		// AcceptClientChanges is called on the server when a Client player attempts to change ServerSide settings in-game. By default, client changes are accepted. (As long as they don't necessitate a Reload)
		// With more effort, a mod could implement more control over changing mod settings.
		public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref string message) {
			if (Main.player[whoAmI].name == "jopojelly") {
				message = "Sorry, players named jopojelly aren't allowed to change settings.";
				return false;
			}
			return true;
		}

		// While ReloadRequired is sufficient for most, some may require more logic in deciding if a reload is required. Here is an incomplete example
		/*public override bool NeedsReload(ModConfig pendingConfig)
		{
			bool defaultDecision = base.NeedsReload(pendingConfig);
			bool otherLogic = IntExample > (pendingConfig as ExampleConfigServer).IntExample; // This is just a random example. Your logic depends on your mod.
			return defaultDecision || otherLogic; // reload needed if either condition is met.
		}*/
	}

	/// <summary>
	/// This config operates on a per-client basis. 
	/// These parameters are local to this computer and are NOT synced from the server.
	/// </summary>
	public class ExampleConfigClient : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		public static ExampleConfigClient Instance; // See ExampleConfigServer.Instance for info.

		[Label("Show the coin rate UI")]
		public bool ShowCoinUI;

		[Label("Show mod origin in tooltip")]
		public bool ShowModOriginTooltip;

		public override void OnChanged() {
			// Here we use the OnChanged hook to initialize ExampleUI.visible with the new values.
			// We maintain both ExampleUI.visible and ShowCoinUI as separate values so ShowCoinUI can act as a default while ExampleUI.visible can change within a play session.
			UI.ExampleUI.Visible = ShowCoinUI;
		}
	}

	[BackgroundColor(144, 252, 249)]
	[Label("ModConfig Showcase A: Data Types")]
	public class ModConfigShowcaseDataTypes : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		// Value Types
		public bool SomeBool;
		public int SomeInt;
		public float SomeFloat;
		public string SomeString;
		public EquipType SomeEnum;
		public byte SomeByte;
		public uint SomeUInt;

		// Structs - These require special code. We've implemented Color and Vector2 so far.
		public Color SomeColor;
		public Vector2 SomeVector2;
		public Point SomePoint; // notice the not implemented message.

		// Data Structures (Reference Types)
		public int[] SomeArray = new int[] { 25, 70, 12 }; // Arrays have a specific length and need a default value specified.
		public List<int> SomeList = new List<int>() { 1, 3, 5 }; // Initializers can be used to declare defaults for data structures.
		public Dictionary<string, int> SomeDictionary = new Dictionary<string, int>();
		public HashSet<string> SomeSet = new HashSet<string>();

		// Classes (Reference Types) - Classes are automatically implemented in the UI.
		public SimpleData SomeClassA;
		// EntityDefinition classes store the identity of an Entity (Item, NPC, Projectile, etc) added by a mod or vanilla. Only the identity is preserved, not other mod data or stack.
		// When using XDefinition classes, you can the .Type property to get the ID of the item. You can use .IsUnloaded to check if the item in question is loaded.
		public ItemDefinition itemDefinitionExample; 
		public NPCDefinition npcDefinitionExample = new NPCDefinition(NPCID.Bunny);
		public ProjectileDefinition projectileDefinitionExample = new ProjectileDefinition("ExampleMod", nameof(Projectiles.Wisp));

		// Data Structures of reference types
		public Dictionary<PrefixDefinition, float> SomeClassE = new Dictionary<PrefixDefinition, float>() {
			[new PrefixDefinition("ExampleMod", "Awesome")] = 0.5f,
			[new PrefixDefinition("ExampleMod", "ReallyAwesome")] = 0.8f
		};

		public ModConfigShowcaseDataTypes() {
			// Doing the initialization of defaults for reference types in a constructor is also acceptable.
			SomeClassA = new SimpleData() {
				percent = .85f
			};
			itemDefinitionExample = new ItemDefinition("Terraria GoldOre"); // EntityDefinition uses ItemID field names rather than the numbers themselves for readability.
		}
	}

	// The rest of this file are many other ModConfig classes that show off various UI capabilities. They have no effect on the mod and are purely teaching examples.

	[BackgroundColor(99, 180, 209)]
	[Label("ModConfig Showcase B: Ranges")]
	public class ModConfigShowcaseRanges : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		// With no annotations on a float, a range from 0 to 1 with ticks of 0.01 is the default.
		public float NormalFloat;

		// We can specify range, increments, and even whether or not to draw guide ticks with annotations.
		[Range(2f, 3f)]
		[Increment(.25f)]
		[DrawTicks]
		[DefaultValue(2f)]
		public float IncrementalRangedFloat;

		[Range(0f, 5f)]
		[Increment(.11f)]
		public float IncrementByPoint11;

		[Range(2f, 5f)]
		[DefaultValue(2f)]
		public float RangedFloat;

		// With no annotations on an int, a range from 0 to 100 is the default. Ints will be displayed as a text input unless a Slider attribute is present.
		public int NormalInt;

		[Increment(5)]
		[Range(60, 250)]
		[DefaultValue(100)]
		[Slider] // The Slider attribute makes this field be presented with a slider rather than a text input. The default ticks is 1.
		public int RangedInteger;

		// We can annotate a List<int> and the range, ticks, increment, and slider attributes will be used by all elements of the List.
		// We can use DefaultListValue to set the default value for items added to the list. Using DefaultValue here will crash the game.
		[Range(10, 20)]
		[Increment(2)]
		[DrawTicks]
		[DefaultListValue(16)]
		[Slider]
		public List<int> ListOfInts = new List<int>();

		[Range(-20f, 20f)]
		[Increment(5f)]
		[DrawTicks]
		public Vector2 RangedWithIncrementVector2;

		// A method annotated with OnDeserialized will run after deserialization. You can use it for enforcing things like ranges, since Range and Increment are UI suggestions.
		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext context) {
			// RangeAttribute is just a suggestion to the UI. If we want to enforce constraints, we need to validate the data here. Users can edit config files manually with values outside the RangeAttribute, so we fix here if necessary.
			// Both enforcing ranges and not enforcing ranges have uses in mods. Make sure you fix config values if values outside the range will mess up your mod.
			RangedFloat = Utils.Clamp(RangedFloat, 2f, 5f);
		}
	}

	[BackgroundColor(154, 152, 181)]
	[Label("ModConfig Showcase C: Labels")]
	public class ModConfigShowcaseLabels : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		// Without a Label attribute, config items will display as field identifiers. Using Label will make your Config appealing.
		// Use Tooltip to convey additional information about the config item.
		[Label("This is a float")]
		[Tooltip("This text will show when hovered")]
		[SliderColor(255, 0, 127)]
		public float SomeFloat;

		// Using localization keys will help make your config readable in multiple languages. See ExampleMod/Localization/en-US.lang
		[Label("$Mods.ExampleMod.Common.LocalizedLabel")]
		[Tooltip("$Mods.ExampleMod.Common.LocalizedTooltip")]
		public int LocalizedLabel;

		// The color of the config entry can be customized. R, G, B
		[BackgroundColor(255, 0, 255)]
		public Pair pairExample = new Pair();

		// List elements also inherit BackgroundColor
		[BackgroundColor(255, 0, 0)]
		public List<Pair> ListOfPair = new List<Pair>();

		// We can also add section headers, separating fields for organization
		[Header("Headers Section")]
		public int Header;

		[Header("$Mods.ExampleMod.Common.LocalizedHeader")]
		public int LocalizedHeader;

		[Header("[i:19][c/00FF00:Green Text]")]
		public int CoolHeader;

		// The class declaration of SimpleData specifies [BackgroundColor(255, 7, 7)]. Field and data structure field annotations override class annotations.
		[BackgroundColor(85, 107, 47)]
		[Label("OverridenColor SimpleData")]
		public SimpleData simpleDataExample2 = new SimpleData();
	}

	[BackgroundColor(164, 153, 190)]
	[Label("ModConfig Showcase D: Default Values")]
	public class ModConfigShowcaseDefaultValues : ModConfig
	{
		// There are 2 approaches to default values. One is applicable only to value types (int, bool, float, string, structs, etc) and the other to reference types (classes).
		// For value types, annotate the field with the DefaultValue attribute. Some structs, like Color and Vector2, accept a string that will be converted to a default value.
		// For reference types (classes), simply assign the value in the field initializer or constructor as you would typically do.

		public override ConfigScope Mode => ConfigScope.ClientSide;

		// Using DefaultValue, we can specify a default value.
		[DefaultValue(99)]
		public int SimpleDefaultInt;

		[DefaultValue(typeof(Color), "73, 94, 171, 255")] // needs 4 comma separated bytes. The Color struct has [TypeConverter(typeof(ColorConverter))] annotating it supplying a way to convert a text constant to a runtime default value.
		public Color SomeColor;

		[DefaultValue(typeof(Vector2), "0.23, 0.77")]
		public Vector2 SomeVector2;

		[DefaultValue(SampleEnum.Strange)]
		[DrawTicks]
		public SampleEnum EnumExample2;

		// Using StringEnumConverter, Enums are read and written as strings rather than the numerical value of the Enum. This makes the config file more readable, but prone to errors if a player manually modifies the config file.
		[JsonConverter(typeof(StringEnumConverter))]
		public SampleEnum EnumExample1 { get; set; }

		// OptionStrings makes a string appear as a choice rather than an input field. Remember that users can manually edit json files, so be aware that a value other than the Options in OptionStrings might populate the field.
		[OptionStrings(new string[] { "Win", "Lose", "Give Up" })]
		[DefaultValue(new string[] { "Give Up", "Give Up" })]
		public string[] ArrayOfString;

		[DrawTicks]
		[OptionStrings(new string[] { "Pikachu", "Charmander", "Bulbasor", "Squirtle" })]
		[DefaultValue("Bulbasor")]
		public string FavoritePokemon;

		// DefaultListValue provides the default value to be added when the user clicks add in the UI.
		[DefaultListValue(123)]
		public List<int> ListOfInts = new List<int>();

		[DefaultListValue(typeof(Vector2), "0.1, 0.2")]
		public List<Vector2> ListOfVector2 = new List<Vector2>();

		// JsonDefaultListValue provides the default value for referece types/classes, expressed as JSON. If you are unsure of the JSON, you can copy from a saved config file itself.
		[JsonDefaultListValue("{\"name\": \"GoldBar\"}")]
		public List<ItemDefinition> ListOfItemDefinition = new List<ItemDefinition>();

		// For Dictionaries, additional attributes (DefaultDictionaryKeyValue or JsonDefaultDictionaryKeyValue) are used to specify a default value for the Key of the Dictionary entry. The Value uses the DefaultListValue or JsonDefaultListValue as List and HashSet do.
		[DefaultDictionaryKeyValue(0.3f)]
		[DefaultListValue(10)]
		public Dictionary<float, int> DictionaryDefaults = new Dictionary<float, int>();

		[JsonDefaultDictionaryKeyValue("{\"name\": \"GoldBar\"}")]
		[JsonDefaultListValue("{\"name\": \"SilverBar\"}")]
		public Dictionary<ItemDefinition, ItemDefinition> DictionaryDefaults2 = new Dictionary<ItemDefinition, ItemDefinition>();
	}

	[BackgroundColor(148, 72, 188)]
	[Label("ModConfig Showcase E: Subpages")]
	public class ModConfigShowcaseSubpages : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[Header("SeparatePage Examples")]
		// Using SeparatePage, an object will be presented to the user as a button. That button will lead to a separate page where the usual UI will be presented. Useful for organization.
		[SeparatePage]
		public Gradient gradient = new Gradient();

		// This example has multiple levels of subpages, check it out. In this example, the SubConfigExample class itself is annotated with [SeparatePage]
		[Label("Sub Config Example - Click me")]
		public SubConfigExample subConfigExample = new SubConfigExample();

		[SeparatePage]
		public Dictionary<ItemDefinition, SubConfigExample> DictionaryofSubConfigExample = new Dictionary<ItemDefinition, SubConfigExample>();

		// These 2 examples show how [SeparatePage] works on annotating both a field for a class and annotating a List of a class
		[SeparatePage]
		[Label("Subpage List Of Pairs")]
		public List<Pair> SeparateListOfPairs = new List<Pair>();

		[SeparatePage]
		[Label("Special Pair")]
		public Pair pair = new Pair();

		// C# allows inner classes (used here), which might be useful for organization if you want.
		[SeparatePage]
		public class SubConfigExample
		{
			[DefaultValue(99)]
			public int boost = 99;
			public float percent;
			public bool enabled;

			[SeparatePage]
			[BackgroundColor(50, 200, 100)]
			public SubSubConfigExample SubA = new SubSubConfigExample();

			[SeparatePage]
			[Label("Even More Sub")]
			public SubSubConfigExample SubB = new SubSubConfigExample();

			public override string ToString() {
				return $"{boost} {percent} {enabled} {SubA.whoa}/{SubB.whoa}";
			}

			public override bool Equals(object obj) {
				if (obj is SubConfigExample other)
					return boost == other.boost && percent == other.percent && enabled == other.enabled && SubA.Equals(other.SubA) && SubB.Equals(other.SubB);
				return base.Equals(obj);
			}

			public override int GetHashCode() {
				return new { boost, percent, enabled, SubA, SubB }.GetHashCode();
			}
		}

		public class SubSubConfigExample
		{
			public int whoa;
			public override bool Equals(object obj) {
				if (obj is SubSubConfigExample other)
					return whoa == other.whoa;
				return base.Equals(obj);
			}

			public override int GetHashCode() => whoa.GetHashCode();
		}
	}

	[BackgroundColor(164, 153, 190)]
	[Label("ModConfig Showcase F: Accessibility")]
	public class ModConfigShowcaseAccessibility : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		// Private and Internal fields and properties will not be shown. 
		// Note that private and internal values will not be replaced by the deserialization, so initializer and ctor work.
		// You should avoid private and internal values in 
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

		// AutoProperies work the same as fields.
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

		// Using Label overrides JsonIgnore for the UI. Use this to display info to the user if needed. The value won't be saved since it is derived from other fields.
		// Useful for things like displaying sums or calculated relationships.
		[JsonIgnore]
		[Label("Ignore With Label")]
		public float IgnoreWithLabelGetter => AutoProperty + Public;

		// Reference type getters kind of work with the UI. You can experiment with this if you want.
		[JsonIgnore]
		[Label("Pair Getter")]
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

	/// <summary>
	/// This config is just a showcase of various attributes and their effects in the UI window.
	/// </summary>
	[Label("ModConfig Showcase G: Misc")]
	public class ModConfigShowcaseMisc : ModConfig {
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

		// In this example we inherit from a tmodloader config UIElement to slightly customize the colors.
		[CustomModConfigItem(typeof(CustomFloatElement))]
		public float tint;

		public Dictionary<string, Pair> StringPairDictionary = new Dictionary<string, Pair>();
		public Dictionary<ItemDefinition, float> JsonItemFloatDictionary = new Dictionary<ItemDefinition, float>();

		public HashSet<ItemDefinition> itemSet = new HashSet<ItemDefinition>();

		[Label("ListOfPair2 label")]
		public List<Pair> ListOfPair2 = new List<Pair>();
		public Pair pairExample2 = new Pair();

		public SimpleData simpleDataExample; // you can also initialize in the constructor, see initailization in public ModConfigShowcaseMisc() below.

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
			if (_additionalData.TryGetValue("OldListOfInts", out var token))
			{
				var OldListOfInts = token.ToObject<List<int>>();
				ListOfInts.AddRange(OldListOfInts);
			}
			_additionalData.Clear(); // make sure to clear this or it'll crash.
		}
	}

	// These are some classes and enums used in the ModConfig above.
	public enum SampleEnum
	{
		Weird,
		Odd,
		Strange,
		Peculiar
	}

	public class Gradient
	{
		[Tooltip("The color the gradient starts at")]
		[DefaultValue(typeof(Color), "0, 0, 255, 255")]
		public Color start = Color.Blue; // For sub-objects, you'll want to make sure to set defaults in constructor or field initializer.
		[Tooltip("The color the gradient ends at")]
		[DefaultValue(typeof(Color), "255, 0, 0, 255")]
		public Color end = Color.Red;

		public override bool Equals(object obj) {
			if (obj is Gradient other)
				return start == other.start && end == other.end;
			return base.Equals(obj);
		}

		public override int GetHashCode() {
			return new { start, end }.GetHashCode();
		}
	}

	[BackgroundColor(0, 255, 255)]
	[Label("Pair label")]
	public class Pair
	{
		public bool enabled;
		public int boost;

		// If you override ToString, it will show up appended to the Label in the ModConfig UI.
		public override string ToString() {
			return $"Boost: {(enabled ? "" + boost : "disabled")}";
		}

		// Implementing Equals and GetHashCode are critical for any classes you use.
		public override bool Equals(object obj) {
			if (obj is Pair other)
				return enabled == other.enabled && boost == other.boost;
			return base.Equals(obj);
		}

		public override int GetHashCode() {
			return new { boost, enabled }.GetHashCode();
		}
	}

	[BackgroundColor(255, 7, 7)]
	public class SimpleData
	{
		[Header("Awesome")]
		public int boost;
		public float percent;

		[Header("Lame")]
		public bool enabled;

		[DrawTicks]
		[OptionStrings(new string[] { "Pikachu", "Charmander", "Bulbasor", "Squirtle" })]
		[DefaultValue("Bulbasor")]
		public string FavoritePokemon;

		public SimpleData() {
			FavoritePokemon = "Bulbasor";
		}

		public override bool Equals(object obj) {
			if (obj is SimpleData other)
				return boost == other.boost && percent == other.percent && enabled == other.enabled && FavoritePokemon == other.FavoritePokemon;
			return base.Equals(obj);
		}

		public override int GetHashCode() {
			return new { boost, percent, enabled, FavoritePokemon }.GetHashCode();
		}
	}

	public class ComplexData
	{
		public List<int> ListOfInts = new List<int>();

		public SimpleData nestedSimple = new SimpleData();

		[Range(2f, 3f)]
		[Increment(.25f)]
		[DrawTicks]
		[DefaultValue(2f)]
		public float IncrementalFloat = 2f;
		public override bool Equals(object obj) {
			if (obj is ComplexData other)
				return ListOfInts.SequenceEqual(other.ListOfInts) && IncrementalFloat == other.IncrementalFloat && nestedSimple.Equals(other.nestedSimple);
			return base.Equals(obj);
		}

		public override int GetHashCode() {
			return new { ListOfInts, nestedSimple, IncrementalFloat }.GetHashCode();
		}
	}

	// ATTENTION: Below this point are custom config UI elements. Be aware that mods using custom config elements will break with the next few tModLoader updates until their design is finalized.
	// You will need to be very active in updating your mod if you use these as they can break in any update.

	// This custom config UI element uses vanilla config elements paired with custom drawing.
	class GradientElement : ConfigElement
	{
		public override void OnBind() {
			base.OnBind();
			object subitem = memberInfo.GetValue(item);
			if (subitem == null) {
				subitem = Activator.CreateInstance(memberInfo.Type);
				JsonConvert.PopulateObject("{}", subitem, ConfigManager.serializerSettings);
				memberInfo.SetValue(item, subitem);
			}

			// item is the owner object instance, memberinfo is the Info about this field in item

			int height = 30;
			int order = 0;
			foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(subitem)) {
				var wrapped = ConfigManager.WrapIt(this, ref height, variable, subitem, order++);

				if (list != null) {
					wrapped.Item1.Left.Pixels -= 20;
					wrapped.Item1.Width.Pixels += 20;
				}
			}
		}

		public override void Draw(SpriteBatch spriteBatch) {
			base.Draw(spriteBatch);
			Rectangle hitbox = GetInnerDimensions().ToRectangle();
			Gradient g = (memberInfo.GetValue(item) as Gradient);
			if (g != null) {
				int left = (hitbox.Left + hitbox.Right) / 2;
				int right = hitbox.Right;
				int steps = right - left;
				for (int i = 0; i < steps; i += 1) {
					float percent = (float)i / steps;
					spriteBatch.Draw(Main.magicPixel, new Rectangle(left + i, hitbox.Y, 1, 30), Color.Lerp(g.start, g.end, percent));
				}

				//Main.spriteBatch.Draw(Main.magicPixel, new Rectangle(hitbox.X + hitbox.Width / 2, hitbox.Y, hitbox.Width / 4, 30), g.start);
				//Main.spriteBatch.Draw(Main.magicPixel, new Rectangle(hitbox.X + 3 * hitbox.Width / 4, hitbox.Y, hitbox.Width / 4, 30), g.end);
			}
		}
	}

	[JsonConverter(typeof(StringEnumConverter))]
	[CustomModConfigItem(typeof(CornerElement))]
	public enum Corner
	{
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight
	}

	// This custom config UI element shows a completely custom config element that handles setting and getting the values in addition to custom drawing.
	class CornerElement : ConfigElement
	{
		Texture2D circleTexture;
		string[] valueStrings;

		public override void OnBind() {
			base.OnBind();
			circleTexture = Terraria.Graphics.TextureManager.Load("Images/UI/Settings_Toggle");
			valueStrings = Enum.GetNames(memberInfo.Type);
			TextDisplayFunction = () => memberInfo.Name + ": " + GetStringValue();
			if (labelAttribute != null) {
				TextDisplayFunction = () => labelAttribute.Label + ": " + GetStringValue();
			}
		}

		void SetValue(Corner value) => SetObject(value);

		Corner GetValue() => (Corner)GetObject();

		string GetStringValue() {
			return valueStrings[(int)GetValue()];
		}

		public override void Click(UIMouseEvent evt) {
			base.Click(evt);
			SetValue(GetValue().NextEnum());
		}

		public override void RightClick(UIMouseEvent evt) {
			base.RightClick(evt);
			SetValue(GetValue().PreviousEnum());
		}

		public override void Draw(SpriteBatch spriteBatch) {
			base.Draw(spriteBatch);
			CalculatedStyle dimensions = base.GetDimensions();
			Rectangle circleSourceRectangle = new Rectangle(0, 0, (circleTexture.Width - 2) / 2, circleTexture.Height);
			spriteBatch.Draw(Main.magicPixel, new Rectangle((int)(dimensions.X + dimensions.Width - 25), (int)(dimensions.Y + 4), 22, 22), Color.LightGreen);
			Corner corner = GetValue();
			Vector2 circlePositionOffset = new Vector2((int)corner % 2 * 8, (int)corner / 2 * 8);
			spriteBatch.Draw(circleTexture, new Vector2(dimensions.X + dimensions.Width - 25, dimensions.Y + 4) + circlePositionOffset, circleSourceRectangle, Color.White);
		}
	}

	class CustomFloatElement : FloatElement
	{
		public CustomFloatElement() {
			colorMethod = new Utils.ColorLerpMethod((percent) => Color.Lerp(Color.BlueViolet, Color.Aquamarine, percent));
		}
	}
}
