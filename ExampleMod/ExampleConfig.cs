using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod
{
	// This file contains 2 ModConfigs. One is set to MultiplayerSyncMode.ServerDictates and the other MultiplayerSyncMode.UniquePerPlayer
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
		public override MultiplayerSyncMode Mode
		{
			get
			{
				return MultiplayerSyncMode.ServerDictates;
			}
		}

		// We will use attributes to annotate our fields or properties so tModLoader can properly handle them.

		// First, we will learn about DefaultValue. You might assume "public bool BoolExample = true;" to work, 
		// but because tModLoader is overwriting with JSON, that value will be overwritten when the mod loads.
		// We must use the DefaultValue Attribute instead of setting the value normally:
		[DefaultValue(true)]
		public bool UselessBoolExample;

		// This is private. You'll notice that it doesn't show up in the config menu. Don't set something private.
		private bool PrivateFieldBoolExample;

		// This is ignored, it also shouldn't show up in the config menu
		[JsonIgnore]
		public bool IgnoreExample;

		// You'll notice this next one is a Property instead of a field. That works too.
		// Here we see an attribute added by tModLoader: LabelAttribute. This one allows us to add a label so the user knows more about the setting they are changing.
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

		// While ReloadRequired is sufficient for most, some may require more logic in deciding if a reload is required. Here is an incomplete example
		/*public override bool NeedsReload(ModConfig old)
		{
			bool defaultDecision = base.NeedsReload(old);
			bool otherLogic = IntExample > (old as ExampleConfigServer).IntExample; // This is just a random example. Your logic depends on your mod.
			return defaultDecision || otherLogic; // reload needed if either condition is met.
		}*/

		// Here I use PostAutoLoad to assign a static variable in ExampleMod to make it a little easier to access config values.
		// This reduces code from "mod.GetConfig<ExampleConfigServer>().DisableExampleWings" to "ExampleMod.exampleServerConfig.DisableExampleWings". It's just a style choice.
		// Note that PostAutoLoad happens before AutoLoad and Mod.Load.
		public override void PostAutoLoad()
		{
			ExampleMod.exampleServerConfig = this;
		}

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
	}

	/// <summary>
	/// This config operates on a per-client basis. 
	/// These parameters are local to this computer and are NOT synced from the server.
	/// </summary>
	public class ExampleConfigClient : ModConfig
	{
		public override MultiplayerSyncMode Mode
		{
			get
			{
				return MultiplayerSyncMode.UniquePerPlayer;
			}
		}

		[Label("Show the coin rate UI")]
		public bool ShowCoinUI;

		[Label("Show mod origin in tooltip")]
		public bool ShowModOriginTooltip;

		public override void PostAutoLoad()
		{
			ExampleMod.exampleClientConfig = this;
			UI.ExampleUI.visible = ShowCoinUI;
		}

		public override void PostSave()
		{
			// Here we use the PostSave hook to initialize ExampleUI.visible with the new values.
			// We maintain both ExampleUI.visible and ShowCoinUI as separate values so ShowCoinUI can act as a default while ExampleUI.visible can change within a play session.
			UI.ExampleUI.visible = ShowCoinUI;
		}
	}

	/// <summary>
	/// This config is just a showcase of various attributes and their effects in the UI window.
	/// </summary>
	[Label("ModConfig Showcase")]
	public class ModConfigShowcase : ModConfig
	{
		public override MultiplayerSyncMode Mode
		{
			get
			{
				return MultiplayerSyncMode.UniquePerPlayer;
			}
		}

		[Label("$Mods.ExampleMod.Common.LocalizedLabel")]
		public int LocalizedLabel;

		[Label("This is a float")]
		public float SomeFLoat;

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

		[JsonConverter(typeof(StringEnumConverter))]
		public SampleEnum EnumExample1 { get; set; }

		[DefaultValue(SampleEnum.Strange)]
		[DrawTicks]
		public SampleEnum EnumExample2;

		[Increment(5)]
		[Range(60, 250)]
		[DefaultValue(100)]
		public int IntegerExample;

		[DrawTicks]
		[OptionStrings(new string[] { "Pikachu", "Charmander", "Bulbasor", "Squirtle" })]
		[DefaultValue("Bulbasor")]
		public string FavoritePokemon;

		public string InputString;
		public string InputString2;

		public List<string> ListOfString;

		[SeparatePage]
		public SubConfigExample subConfigExample;

		[Range(10, 20)]
		[Increment(2)]
		[DrawTicks]
		public List<int> ListOfInts;

		public Dictionary<int, float> IntFloatDictionary;
		public Dictionary<string, Pair> StringPairDictionary;

		[BackgroundColor(255, 0, 0)]
		public List<Pair> ListOfPair;

		[BackgroundColor(255, 0, 255)]
		public Pair pairExample;

		public Pair pairExample2;

		[OptionStrings(new string[] { "Win", "Lose", "Give Up" })]
		[DefaultValue(new string[] { "Give Up", "Give Up" })]
		public string[] ArrayOfString;

		[DefaultValue(new int[] { 4, 6, 12 })]
		public int[] ArrayOfInts;

		public SimpleData simpleDataExample;

		[BackgroundColor(85, 107, 47)]
		[Label("OverridenColor SimpleData")]
		public SimpleData simpleDataExample2;

		[Label("Really Complex Data")]
		public ComplexData complexData;

		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext context)
		{
			// We use a method marked OnDeserialized to initialize default values of reference types since we can't do that with the DefaultValue attribute.
			if (ListOfInts == null)
				ListOfInts = new List<int>() { };
			if (ListOfString == null)
				ListOfString = new List<string>() { };
			if (ListOfPair == null)
				ListOfPair = new List<Pair>() { };
			if (IntFloatDictionary == null)
				IntFloatDictionary = new Dictionary<int, float>();
			if (StringPairDictionary == null)
				StringPairDictionary = new Dictionary<string, Pair>();
			//if (simpleDataExample == null)
			//	simpleDataExample = new SimpleData();
			if (simpleDataExample2 == null)
			{
				simpleDataExample2 = new SimpleData();
				simpleDataExample2.boost = 32;
				simpleDataExample2.percent = 2f;
			}
			if (complexData == null)
			{
				complexData = new ComplexData();
				complexData.ListOfInts = new List<int>();
			}
			//ArrayOfInts = (int[])ArrayOfInts.Clone();
			//ArrayOfString = (string[])ArrayOfString.Clone();

			// RangeAttribute is just a suggestion to the UI. If we want to enforce constraints, we need to validate the data here.
			RangedFloat = Utils.Clamp(RangedFloat, 2f, 5f);
		}

		public override ModConfig Clone()
		{
			// Since ListOfInts is a reference type, we need to clone it manually so our config works properly.
			var clone = (ModConfigShowcase)base.Clone();
			clone.ListOfInts = new List<int>(ListOfInts);
			clone.ListOfString = new List<string>(ListOfString);
			clone.subConfigExample = subConfigExample == null ? null : subConfigExample.Clone();
			clone.simpleDataExample = simpleDataExample == null ? null : simpleDataExample.Clone();
			clone.pairExample = pairExample == null ? null : pairExample.Clone();
			clone.simpleDataExample2 = simpleDataExample2.Clone();
			clone.ArrayOfInts = (int[])ArrayOfInts.Clone();
			clone.ArrayOfString = (string[])ArrayOfString.Clone();
			clone.ListOfPair = ListOfPair.ConvertAll(pair => pair.Clone());
			clone.IntFloatDictionary = IntFloatDictionary.ToDictionary(i => i.Key, i => i.Value);
			clone.StringPairDictionary = StringPairDictionary.ToDictionary(i => i.Key, i => i.Value.Clone());
			return clone;
		}

		// ShouldSerialize is useful. Here we use it so ListOfInts doesn't show up as a useless empty entry in the config file. https://www.newtonsoft.com/json/help/html/ConditionalProperties.htm
		public bool ShouldSerializeListOfInts()
		{
			return ListOfInts.Count > 0;
		}
	}

	public enum SampleEnum
	{
		Weird,
		Odd,
		Strange,
		Peculiar
	}

	[BackgroundColor(0, 255, 255)]
	public class Pair
	{
		public int boost;
		public bool enabled;

		public Pair Clone()
		{
			return (Pair)MemberwiseClone();
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

		public SubConfigExample()
		{
		}

		public SubConfigExample Clone()
		{
			var clone =  (SubConfigExample)MemberwiseClone();
			clone.SubA = SubA.Clone();
			clone.SubB = SubB.Clone();
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

	public class ComplexData
	{
		public List<int> ListOfInts;

		public SimpleData nestedSimple;

		[Range(2f, 3f)]
		[Increment(.25f)]
		[DrawTicks]
		[DefaultValue(2f)]
		public float IncrementalFloat;

		public ComplexData Clone()
		{
			var clone = (ComplexData)MemberwiseClone();
			clone.ListOfInts = new List<int>(ListOfInts);
			clone.nestedSimple = nestedSimple.Clone();
			return clone;
		}
	}
}
