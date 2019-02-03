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
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;

namespace ExampleMod
{
	// This file contains 2 real ModConfigs (and also a bunch of fake ModConfigs showcasing various ideas). One is set to MultiplayerSyncMode.ServerDictates and the other MultiplayerSyncMode.UniquePerPlayer
	// ModConfigs contain Public Fields and Properties that represent the choices available to the user. 
	// Those Fields or Properties will be presented to users in the Config menu.
	// DONT use static members anywhere in this class, tModLoader maintains several instances of ModConfig classes which will not work well with static properties or fields.

	/// <summary>
	/// ExampleConfigServer has Server-wide effects. Things that happen on the server, on the world, or influence autoload go here
	/// MultiplayerSyncMode.ServerDictates ModConfigs are SHARED from the server to all clients connecting in MP.
	/// </summary>
	public class ExampleConfigServer : ModConfig
	{
		// You MUST specify a MultiplayerSyncMode.
		public override MultiplayerSyncMode Mode => MultiplayerSyncMode.ServerDictates;

		// We will use attributes to annotate our fields or properties so tModLoader can properly handle them.

		// First, we will learn about DefaultValue. You might assume "public bool BoolExample = true;" to work, 
		// but because tModLoader is overwriting with JSON, that value will be overwritten when the mod loads.
		// We must use the DefaultValue attribute instead of setting the value normally:
		[DefaultValue(true)]
		public bool UselessBoolExample;

		// This is private. You'll notice that it doesn't show up in the config menu. Don't set something private.
		private bool PrivateFieldBoolExample;

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
		
		[Label("Example Person free gift list")]
		[Tooltip("Each player can claim one free item from this list from Example Person")]
		// TODO: Fix PostSave to make this simpler.
		public List<ItemDefinition> ExamplePersonFreeGiftList { get; set; }

		// Clone logic is required. See ModConfigShowcaseDataTypes.Clone for more info.
		public override ModConfig Clone()
		{
			var clone = (ExampleConfigServer)base.Clone();

			clone.ExamplePersonFreeGiftList = ExamplePersonFreeGiftList?.Select(item=> new ItemDefinition(item.mod, item.name)).ToList();

			return clone;
		}

		// Here I use PostAutoLoad to assign a static variable in ExampleMod to make it a little easier to access config values.
		// This reduces code from "mod.GetConfig<ExampleConfigServer>().DisableExampleWings" to "ExampleMod.exampleServerConfig.DisableExampleWings". It's just a style choice.
		// Note that PostAutoLoad happens before AutoLoad and Mod.Load.
		public override void PostAutoLoad()
		{
			ExampleMod.exampleServerConfig = this;
		}

		// AcceptClientChanges is called on the server when a Client player attempts to change ServerDictates settings in-game. By default, client changes are accepted. (As long as they don't necessitate a Reload)
		// With more effort, a mod could implement more control over changing mod settings.
		public override bool AcceptClientChanges(ModConfig currentConfig, int whoAmI, ref string message)
		{
			if (Main.player[whoAmI].name == "jopojelly")
			{
				message = "Sorry, players named jopojelly aren't allowed to change settings.";
				return false;
			}
			return true;
		}

		// While ReloadRequired is sufficient for most, some may require more logic in deciding if a reload is required. Here is an incomplete example
		/*public override bool NeedsReload(ModConfig old)
		{
			bool defaultDecision = base.NeedsReload(old);
			bool otherLogic = IntExample > (old as ExampleConfigServer).IntExample; // This is just a random example. Your logic depends on your mod.
			return defaultDecision || otherLogic; // reload needed if either condition is met.
		}*/
	}

	/// <summary>
	/// This config operates on a per-client basis. 
	/// These parameters are local to this computer and are NOT synced from the server.
	/// </summary>
	public class ExampleConfigClient : ModConfig
	{
		public override MultiplayerSyncMode Mode => MultiplayerSyncMode.UniquePerPlayer;

		[Label("Show the coin rate UI")]
		public bool ShowCoinUI;

		[Label("Show mod origin in tooltip")]
		public bool ShowModOriginTooltip;

		public override void PostAutoLoad()
		{
			ExampleMod.exampleClientConfig = this;
			UI.ExampleUI.Visible = ShowCoinUI;
		}

		public override void PostSave()
		{
			// Here we use the PostSave hook to initialize ExampleUI.visible with the new values.
			// We maintain both ExampleUI.visible and ShowCoinUI as separate values so ShowCoinUI can act as a default while ExampleUI.visible can change within a play session.
			UI.ExampleUI.Visible = ShowCoinUI;
		}
	}

	[BackgroundColor(144, 252, 249)]
	[Label("ModConfig Showcase A: Data Types")]
	public class ModConfigShowcaseDataTypes : ModConfig
	{
		public override MultiplayerSyncMode Mode => MultiplayerSyncMode.UniquePerPlayer;

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
		private static int[] SomeArrayDefaults = new int[] { 25, 70, 12 };
		public int[] SomeArray; // Arrays have a specific length and need a default value specified.
		public List<int> SomeList;
		public Dictionary<string, int> SomeDictionary;
		public HashSet<string> SomeSet;

		// Classes (Reference Types
		public ItemDefinition SomeClassA; // ItemDefinition is a new class that can store the identity of a Item added by a mod or vanilla. Only the identity is preserved, not other mod data or stack. Has a unique UI
		public SimpleData SomeClassB; // Classes are automatically implemented in the UI.
		// See ??? for an example of a class with a CustomUI.

		// Proper cloning of reference types is required because behind the scenes many instances of ModConfig classes co-exist.
		public override ModConfig Clone()
		{
			// Since ListOfInts is a reference type, we need to clone it manually so our config works properly.
			var clone = (ModConfigShowcaseDataTypes)base.Clone();

			// We use ?. and ?: here because many of these fields can be null.
			// clone.SomeList = SomeList != null ? new List<int>(SomeList) : null;
			clone.SomeArray = (int[])SomeArray.Clone();
			clone.SomeList = SomeList?.ToList();
			clone.SomeDictionary = SomeDictionary?.ToDictionary(i => i.Key, i => i.Value);
			clone.SomeSet = SomeSet != null ? new HashSet<string>(SomeSet) : null;

			clone.SomeClassA = SomeClassA == null ? null : new ItemDefinition(SomeClassA.mod, SomeClassA.name);
			clone.SomeClassB = SomeClassB?.Clone();

			return clone;
		}

		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext context)
		{
			// constructors and field initializers don't work with the json deserialization process. We need to use a method annotated with [OnDeserialized] to handle default values. 
			// tModLoader will crash if you don't initialize the Array. If you need dynamic size collection, use List
			SomeArray = SomeArray?.Length == SomeArrayDefaults.Length ? SomeArray : SomeArrayDefaults.ToArray();
		}

		// ShouldSerialize{FieldNameHere}
		public bool ShouldSerializeSomeArray()
		{
			// This allows an update to a mod to change array defaults. Without ignoring default values, default values will populate the json file and prevent updated defaults from being loaded without extra logic.
			return !SomeArray.SequenceEqual(SomeArrayDefaults);
		}
	}

	// The rest of this file are many other ModConfig classes that show off various UI capabilities. They have no effect on the mod and are purely teaching examples.

	[BackgroundColor(99, 180, 209)]
	[Label("ModConfig Showcase B: Ranges")]
	public class ModConfigShowcaseRanges : ModConfig
	{
		public override MultiplayerSyncMode Mode => MultiplayerSyncMode.UniquePerPlayer;

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

		// With no annotations on an int, a range from 0 to 100 with ticks of 1 is the default. Without a Range attribute, there will be no slider.
		public int NormalInt;

		[Increment(5)]
		[Range(60, 250)]
		[DefaultValue(100)]
		public int RangedInteger;

		// We can annotate a List<int> and the range, ticks, and increment will be used by all elements of the List
		[Range(10, 20)]
		[Increment(2)]
		[DrawTicks]
		public List<int> ListOfInts;

		[Range(-20f, 20f)]
		[Increment(5f)]
		[DrawTicks]
		public Vector2 RangedWithIncrementVector2;

		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext context)
		{
			// RangeAttribute is just a suggestion to the UI. If we want to enforce constraints, we need to validate the data here. Users can edit config files manually with values outside the RangeAttribute, so we fix here if necessary.
			// Both enforcing ranges and not enforcing ranges have uses in mods.
			RangedFloat = Utils.Clamp(RangedFloat, 2f, 5f);
		}

		public override ModConfig Clone()
		{
			var clone = (ModConfigShowcaseRanges)base.Clone();
			clone.ListOfInts = ListOfInts?.ToList();
			return clone;
		}
	}

	[BackgroundColor(154, 152, 181)]
	[Label("ModConfig Showcase C: Labels")]
	public class ModConfigShowcaseLabels : ModConfig
	{
		public override MultiplayerSyncMode Mode => MultiplayerSyncMode.UniquePerPlayer;

		// Without a Label attribute, config items will display as field identifiers. Using Label will make your Config appealing.
		// Use Tooltip to convey additional information about the config item.
		[Label("This is a float")]
		[Tooltip("This text will show when hovered")]
		public float SomeFloat;

		// Using localization keys will help make your config readable in multiple languages. See ExampleMod/Localization/en-US.lang
		[Label("$Mods.ExampleMod.Common.LocalizedLabel")]
		[Tooltip("$Mods.ExampleMod.Common.LocalizedTooltip")]
		public int LocalizedLabel;

		// The color of the config entry can be customized. R, G, B
		[BackgroundColor(255, 0, 255)]
		public Pair pairExample;

		// List elements also inherit BackgroundColor
		[BackgroundColor(255, 0, 0)]
		public List<Pair> ListOfPair;

		// The class declaration of SimpleData specifies [BackgroundColor(255, 7, 7)]. Field and data structure field annotations override class annotations.
		[BackgroundColor(85, 107, 47)]
		[Label("OverridenColor SimpleData")]
		public SimpleData simpleDataExample2;
	}

	[BackgroundColor(164, 153, 190)]
	[Label("ModConfig Showcase D: Default Values")]
	public class ModConfigShowcaseDefaultValues : ModConfig
	{
		public override MultiplayerSyncMode Mode => MultiplayerSyncMode.UniquePerPlayer;

		// Using DefaultValue, we can specify a default value.
		[DefaultValue(99)]
		public int SimpleDefaultInt;

		[DefaultValue(typeof(Color), "73, 94, 171, 255")] // needs 4 comma separated bytes. The Color struct has [TypeConverter(typeof(ColorConverter))] annotating it supplying a way to convert a text constant to a runtime default value.
		public Color SomeColor;

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

		// does TypeConverter work on a field and override a class annotation? Can we get a default value here?
		// Can I get jsondefault value working?
		// Can I get default value working and not saving when data is same but reference different?

		// DefaultListValue provides the default value to be added when the user clicks add in the UI.
		[DefaultListValue(123)]
		public List<int> ListOfInts;

		public override ModConfig Clone()
		{
			var clone = (ModConfigShowcaseDefaultValues)base.Clone();
			clone.ListOfInts = ListOfInts?.ToList();
			return clone;
		}
	}

	[BackgroundColor(148, 72, 188)]
	[Label("ModConfig Showcase E: Subpages")]
	public class ModConfigShowcaseSubpages : ModConfig
	{
		public override MultiplayerSyncMode Mode => MultiplayerSyncMode.UniquePerPlayer;

		// Using SeparatePage, an object will be presented to the user as a button. That button will lead to a separate page where the usual UI will be presented. Useful for organization.
		[SeparatePage]
		public Gradient gradient;

		// This example has multiple levels of subpages, check it out. In this example, the SubConfigExample class itself is annotated with [SeparatePage]
		[Label("Sub Config Example - Click me")]
		public SubConfigExample subConfigExample;

		[SeparatePage]
		public Dictionary<ItemDefinition, SubConfigExample> DictionaryofSubConfigExample;

		// These 2 examples show how [SeparatePage] works on annotating both a field for a class and annotating a List of a class
		[SeparatePage]
		[Label("Subpage List Of Pairs")]
		public List<Pair> SeparateListOfPairs;

		[SeparatePage]
		[Label("Special Pair")]
		public Pair pair;

		public override ModConfig Clone()
		{
			var clone = (ModConfigShowcaseSubpages)base.Clone();
			clone.gradient = gradient?.Clone();
			clone.subConfigExample = subConfigExample?.Clone();
			clone.DictionaryofSubConfigExample = DictionaryofSubConfigExample?.ToDictionary(i => i.Key == null ? null : new ItemDefinition(i.Key.mod, i.Key.name), i => i.Value?.Clone());
			clone.SeparateListOfPairs = SeparateListOfPairs?.ToList();
			clone.pair = pair?.Clone();
			return clone;
		}

		// C# allows inner classes (used here), which might be useful for organization if you want.
		[SeparatePage]
		public class SubConfigExample
		{
			public int boost;
			public float percent;
			public bool enabled;

			[SeparatePage]
			[BackgroundColor(50, 200, 100)]
			public SubSubConfigExample SubA;

			[SeparatePage]
			[Label("Even More Sub")]
			public SubSubConfigExample SubB;

			public SubConfigExample Clone()
			{
				var clone = (SubConfigExample)MemberwiseClone();
				clone.SubA = SubA?.Clone();
				clone.SubB = SubB?.Clone();
				return clone;
			}
		}

		public class SubSubConfigExample
		{
			public int whoa;

			public SubSubConfigExample Clone()
			{
				return (SubSubConfigExample)MemberwiseClone();
			}
		}
	}

	[BackgroundColor(164, 153, 190)]
	[Label("ModConfig Showcase F: Accessibility")]
	public class ModConfigShowcaseAccessibility : ModConfig
	{
		public override MultiplayerSyncMode Mode => MultiplayerSyncMode.UniquePerPlayer;

		// Private and Internal fields and properties will not be shown. 
		// Note that private and internal values will not be replaced by the deserialization, so initializer and ctor work.
		// You should avoid private and internal values in 
		private float Private = 144;
		internal float Internal;

		// Public fields are most common. Use public for most items.
		public float Public;

		// Will not show. Avoid static. Due to how ModConfig works, static fields will not work correctly. Use PostAutoLoad in the manner used in ExampleConfigServer for accessing ModConfig fields in the rest of your mod.
		public static float Static;

		// Get only properties will show up, but will be grayed out to show that they can't be changed. 
		public float Getter => Main.rand.NextFloat(1f); // This is just an example, please don't do this.

		// AutoProperies work the same as fields.
		public float AutoProperty { get; set; }

		// Properties work as well. The backing field will be ignored when writing the json out.
		private float propertyBackingField;
		public float Property
		{
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

		public ModConfigShowcaseAccessibility()
		{
			Internal = 0.2f;
		}
	}

	// Custom UI
	// more JSON attributes
	//[NonNull]
	// Show off CanChange, ReloadRequired, AcceptClientChanges
	// TODO: does List<List> work?? NOPE
	//public List<List<int>> ListOfLists;
	//clone.ListOfLists = ListOfLists?.Select(x => x?.ToList()).ToList();

	/// <summary>
	/// This config is just a showcase of various attributes and their effects in the UI window.
	/// </summary>
	[Label("ModConfig Showcase G: Misc")]
	public class ModConfigShowcaseMisc : ModConfig
	{
		public override MultiplayerSyncMode Mode => MultiplayerSyncMode.UniquePerPlayer;

		[Label("Custom UI Element")]
		[Tooltip("This UI Element is modder defined")]
		[CustomModConfigItem(typeof(UIModConfigGradientItem))]
		public Gradient gradient;

		public Dictionary<string, Pair> StringPairDictionary;
		public Dictionary<ItemDefinition, float> JsonItemFloatDictionary;

		public HashSet<ItemDefinition> itemSet;

		[Label("ListOfPair2 label")]
		public List<Pair> ListOfPair2;
		public Pair pairExample2;

		public SimpleData simpleDataExample;
		public SimpleData simpleDataExample2;

		[Label("Really Complex Data")]
		public ComplexData complexData;

		[JsonExtensionData]
		private IDictionary<string, JToken> _additionalData = new Dictionary<string, JToken>();

		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext context)
		{
			// We use a method marked OnDeserialized to initialize default values of reference types since we can't do that with the DefaultValue attribute.
			if (StringPairDictionary == null)
				StringPairDictionary = new Dictionary<string, Pair>();
			if (JsonItemFloatDictionary == null)
				JsonItemFloatDictionary = new Dictionary<ItemDefinition, float>();
			itemSet = itemSet ?? new HashSet<ItemDefinition>();
			simpleDataExample = simpleDataExample ?? new SimpleData();
			if (simpleDataExample2 == null) // This won't auto initialize in UI if set to null via UI
			{
				simpleDataExample2 = new SimpleData();
				simpleDataExample2.boost = 32;
				simpleDataExample2.percent = 2f;
			}

			complexData = complexData ?? new ComplexData();
			// It is possible for nestedSimple to be null
			//complexData.nestedSimple = complexData.nestedSimple ?? new SimpleData();


			// If you change ModConfig fields between versions, your users might notice their configuration is lost when they update their mod.
			// We can use [JsonExtensionData] to capture un-de-serialized data and manually restore them to new fields.
			// Imagine in a previous version of this mod, we had a field "OldListOfInts" and we want to preserve that data in "ListOfInts".
			// To test this, insert the following into ExampleMod_ModConfigShowcase.json: "OldListOfInts": [ 99, 999],
			/*if (_additionalData.TryGetValue("OldListOfInts", out var token))
			{
				var OldListOfInts = token.ToObject<List<int>>();
				if (ListOfInts == null) ListOfInts = new List<int>();
				ListOfInts.AddRange(OldListOfInts);
			}*/
			_additionalData.Clear(); // make sure to clear this or it'll crash.
		}

		public override ModConfig Clone()
		{
			// Since ListOfInts is a reference type, we need to clone it manually so our config works properly.
			var clone = (ModConfigShowcaseMisc)base.Clone();

			// We use ?. and ?: here because many of these fields can be null.
			// clone.ListOfInts = ListOfInts != null ? new List<int>(ListOfInts) : null;
			clone.gradient = gradient == null ? null : gradient.Clone();
			clone.StringPairDictionary = StringPairDictionary.ToDictionary(i => i.Key, i => i.Value?.Clone());
			clone.JsonItemFloatDictionary = JsonItemFloatDictionary.ToDictionary(i => new ItemDefinition(i.Key.mod, i.Key.name), i => i.Value);
			clone.itemSet = new HashSet<ItemDefinition>(itemSet);
			clone.ListOfPair2 = ListOfPair2?.ConvertAll(pair => pair.Clone());
			clone.pairExample2 = pairExample2 == null ? null : pairExample2.Clone();
			clone.simpleDataExample = simpleDataExample == null ? null : simpleDataExample.Clone();
			clone.simpleDataExample2 = simpleDataExample2 == null ? null : simpleDataExample2.Clone();
			clone.complexData = complexData == null ? null : complexData.Clone();
			return clone;
		}

		// ShouldSerialize is useful. Here we use it so ListOfInts doesn't show up as a useless empty entry in the config file. https://www.newtonsoft.com/json/help/html/ConditionalProperties.htm
		// It's up to you if the distinction between a null List and an empty List is important.
		//public bool ShouldSerializeListOfInts()
		//{
		//	return ListOfInts?.Count > 0;
		//}
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
		public Color start;
		[Tooltip("The color the gradient ends at")]
		[DefaultValue(typeof(Color), "255, 0, 0, 255")]
		public Color end;

		public Gradient() // TODO: do I need an empty constructor for some reason?
		{
		}

		internal Gradient Clone()
		{
			return (Gradient)MemberwiseClone();
		}
	}

	[BackgroundColor(0, 255, 255)]
	[Label("Pair label")]
	public class Pair
	{
		public bool enabled;
		public int boost;

		public Pair Clone()
		{
			return (Pair)MemberwiseClone();
		}

		// If you override ToString, it will show up appended to the Label in the ModConfig UI.
		public override string ToString()
		{
			return $"Boost: {(enabled ? "" + boost : "disabled")}";
		}
	}

	[BackgroundColor(255, 7, 7)]
	public class SimpleData
	{
		public int boost;
		public float percent;
		public bool enabled;

		[DrawTicks]
		[OptionStrings(new string[] { "Pikachu", "Charmander", "Bulbasor", "Squirtle" })]
		[DefaultValue("Bulbasor")]
		public string FavoritePokemon;

		public SimpleData()
		{
			//FavoritePokemon = "Bulbasor";
		}

		public SimpleData Clone()
		{
			return (SimpleData)MemberwiseClone();
		}
	}

	public class ComplexData
	{
		public List<int> ListOfInts;

		public SimpleData nestedSimple;

		[Range(2f, 3f)]
		[Increment(.25f)]
		[DrawTicks]
		[DefaultValue(2f)]
		public float IncrementalFloat;

		public ComplexData()
		{
			//ListOfInts = new List<int>();
			//nestedSimple = new SimpleData();
		}

		public ComplexData Clone()
		{
			var clone = (ComplexData)MemberwiseClone();
			clone.ListOfInts = ListOfInts?.ToList();
			clone.nestedSimple = nestedSimple?.Clone();
			return clone;
		}

		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext context)
		{
			// NonNull annotation for UI and post Deserialize?
			nestedSimple = nestedSimple ?? new SimpleData();
		}
	}

	class UIModConfigGradientItem : ConfigElement
	{
		//public UIModConfigVector2Item(PropertyFieldWrapper memberInfo, object item, ref int i, IList<Vector2> array = null, int index = -1) : base(memberInfo, item, (IList)array)
		public UIModConfigGradientItem(PropertyFieldWrapper memberInfo, object item, int orderIgnore, IList array2 = null, int index = -1) : base(memberInfo, item, (IList)array2)
		{
			object subitem = memberInfo.GetValue(item);
			if (subitem == null)
			{
				subitem = Activator.CreateInstance(memberInfo.Type);
				JsonConvert.PopulateObject("{}", subitem, ConfigManager.serializerSettings);
				memberInfo.SetValue(item, subitem);
			}

			// item is the owner object instance, memberinfo is the Info about this field in item

			int height = 30;
			IList<Vector2> array = (IList<Vector2>)array2;
			//object subitem = memberInfo.GetValue(item
			int order = 0;
			foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(subitem))
			{
				var wrapped = ConfigManager.WrapIt(this, ref height, variable, subitem, order++);

				if (array != null)
				{
					wrapped.Item1.Left.Pixels -= 20;
					wrapped.Item1.Width.Pixels += 20;
				}
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);
			Rectangle hitbox = GetInnerDimensions().ToRectangle();
			Gradient g = (memberInfo.GetValue(item) as Gradient);
			if (g != null)
			{
				int left = (hitbox.Left + hitbox.Right) / 2;
				int right = hitbox.Right;
				int steps = right - left;
				for (int i = 0; i < steps; i += 1)
				{
					float percent = (float)i / steps;
					spriteBatch.Draw(Main.magicPixel, new Rectangle(left + i, hitbox.Y, 1, 30), Color.Lerp(g.start, g.end, percent));
				}

				//Main.spriteBatch.Draw(Main.magicPixel, new Rectangle(hitbox.X + hitbox.Width / 2, hitbox.Y, hitbox.Width / 4, 30), g.start);
				//Main.spriteBatch.Draw(Main.magicPixel, new Rectangle(hitbox.X + 3 * hitbox.Width / 4, hitbox.Y, hitbox.Width / 4, 30), g.end);
			}
		}

	}
}
