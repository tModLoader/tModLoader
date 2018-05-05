using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.Reflection;
using System.ComponentModel;
using Terraria.ModLoader.UI;

namespace Terraria.ModLoader
{
	// TODO: PostSave hook
	// TODO: Enforce no statics allowd.

	/// <summary>
	/// ModConfig provides a way for mods to be configurable. ModConfigs can either be Client specific or Server specific. 
	/// When joining a MP server, Client configs are kept but Server configs are synced from the server.
	/// Using serialization attributes such as [DefaultValue(5)] or [JsonIgnore] are critical for proper usage of ModConfig.
	/// tModLoader also provides its own attributes such as ReloadRequiredAttribute and LabelAttribute. 
	/// </summary>
	public abstract class ModConfig
	{
		[JsonIgnore]
		public Mod mod { get; internal set; }

		[JsonIgnore]
		public string Name { get; internal set; }

		[JsonIgnore]
		public abstract MultiplayerSyncMode Mode { get; }

		// TODO: Does non-autoloaded ModConfigs hava a usecase?
		public virtual bool Autoload(ref string name) => mod.Properties.Autoload;

		// PostAutoLoad is called right after
		public virtual void PostAutoLoad()
		{
			// TODO: is this name misleading?
		}

		// Called after changes have been made. Useful for informing the mod of changes to config.
		public virtual void PostSave()
		{
		}

		// Called on the Server for ServerDictates configs to determine if the changes asked for by the Client will be accepted. Useful for enforcing permissions.
		public virtual bool AcceptClientChanges(ModConfig currentConfig, int whoAmI, ref string message)
		{
			return true;
		}

		// TODO: Can we get rid of Clone and just load from disk? Don't think so yet.
		public virtual ModConfig Clone() => (ModConfig)MemberwiseClone();

		/// <summary>
		/// Whether or not a reload is required. The default implementation compares properties and fields annotated with the ReloadRequiredAttribute.
		/// </summary>
		/// <param name="old">The other instance of ModConfig to compare against</param>
		/// <returns></returns>
		public virtual bool NeedsReload(ModConfig old)
		{
			foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(this))
			{
				ReloadRequiredAttribute reloadRequired = ConfigManager.GetCustomAttribute<ReloadRequiredAttribute>(variable, this, null);
				if (reloadRequired != null)
				{
					if (!variable.GetValue(this).Equals(variable.GetValue(old)))
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public enum MultiplayerSyncMode
	{

		ServerDictates,
		UniquePerPlayer,
		// Player,
		// World
	}

	/// <summary>
	/// This attribute hints that changing the value of the annotated property or field will put the config in a state that requires a reload. An overriden ModConfig.NeedsReload can further validate if more complex logic is needed.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class ReloadRequiredAttribute : Attribute
	{
	}

	/// <summary>
	/// This attibute sets a label for the property, field, or class for use in the ModConfig UI. 
	/// Starting the label with $ means the label should be interpreted as a Localization key.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
	public class LabelAttribute : Attribute
	{
		readonly string label;
		public LabelAttribute(string label)
		{
			this.label = label;
		}
		public string Label => label.StartsWith("$") ? Localization.Language.GetTextValue(label.Substring(1)) : label;
	}

	// [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	// public class StringRepresentationAttribute : Attribute
	// TODO

	/// <summary>
	/// This attibute sets a hover tooltip for the annotated property or field to be shown in the ModConfig UI. This can be longer and more descriptive than Label.
	/// Starting the tooltip with $ means the tooltip should be interpreted as a Localization key.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class TooltipAttribute : Attribute
	{
		readonly string tooltip;
		public TooltipAttribute(string tooltip)
		{
			this.tooltip = tooltip;
		}
		public string Tooltip => tooltip.StartsWith("$") ? Localization.Language.GetTextValue(tooltip.Substring(1)) : tooltip;
	}

	/// <summary>
	/// Specifies a background color to be used for the property, field, or class in the ModConfig UI. 
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
	public class BackgroundColorAttribute : Attribute
	{
		public Color color;
		public BackgroundColorAttribute(int r, int g, int b, int a = 255)
		{
			this.color = new Color(r, g, b, a);
		}
	}

	/// <summary>
	/// Use this attribute to specify a custom UI element to be used for the annotated property, field, or class in the ModConfig UI. 
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
	public class CustomModConfigItemAttribute : Attribute
	{
		public Type t;
		public CustomModConfigItemAttribute(Type t)
		{
			this.t = t;
		}
	}

	/// <summary>
	/// Defines the default value to be added when using the ModConfig UI to add elements to a List
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class DefaultListValueAttribute : Attribute
	{
		public object defaultValue;
		public DefaultListValueAttribute(int defaultValue)
		{
			this.defaultValue = defaultValue;
		}

		public DefaultListValueAttribute(float defaultValue)
		{
			this.defaultValue = defaultValue;
		}

		public DefaultListValueAttribute(object defaultValue)
		{
			this.defaultValue = defaultValue;
		}
	}

	/// <summary>
	/// By default, string fields will provide the user with a text input field. Use this attribute to restrict strings to a selection of options.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class OptionStringsAttribute : Attribute
	{
		public string[] optionLabels { get; set; }
		public OptionStringsAttribute(string[] optionLabels)
		{
			this.optionLabels = optionLabels;
		}
	}

	/// <summary>
	/// Use this to set an increment for sliders. The slider will move by the amount assigned. Remember that this is just a UI suggestion and manual editing of config files can specify other values, so validate your values.
	/// Defaults are: float: 0.01f - byte/int/uint: 1
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class IncrementAttribute : Attribute
	{
		public object increment;
		public IncrementAttribute(int increment)
		{
			this.increment = increment;
		}
		public IncrementAttribute(float increment)
		{
			this.increment = increment;
		}
		public IncrementAttribute(uint increment)
		{
			this.increment = increment;
		}
		public IncrementAttribute(byte increment)
		{
			this.increment = increment;
		}
	}

	/// <summary>
	/// Specifies a range for primative data values. Without this, default min and max are as follows: float: 0, 1 - int/uint: 0, 100 - byte: 0, 255
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class RangeAttribute : Attribute
	{
		public object min;
		public object max;

		public RangeAttribute(int min, int max)
		{
			this.min = min;
			this.max = max;
		}

		public RangeAttribute(float min, float max)
		{
			this.min = min;
			this.max = max;
		}
		public RangeAttribute(uint min, uint max)
		{
			this.min = min;
			this.max = max;
		}
		public RangeAttribute(byte min, byte max)
		{
			this.min = min;
			this.max = max;
		}
	}

	/// <summary>
	/// Add this attribute and the sliders will show white tick marks at each increment.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class DrawTicksAttribute : Attribute
	{
	}

	/// <summary>
	/// This specifies that the annotated item will appear as a button that leads to a separate page in the UI. Use this to organize hierarchies.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class SeparatePageAttribute : Attribute
	{
	}

	// Unimplemented below:

	// Hide or Disable this item while in game.
	//[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	//public class HideInGameAttribute : Attribute { }

	// Hide or Disable this item while a client?
	//[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	//public class HideForClientAttribute : Attribute { }

	//public class JsonDefaultValueAttribute : Attribute
	//{
	//	public string json;
	//	public JsonDefaultValueAttribute(string json)
	//	{
	//		this.json = json;
	//	}
	//}

	// Problem: shared reference.
	//public class JsonDefaultValueAttribute : DefaultValueAttribute
	//{
	//	public string json;
	//	public JsonDefaultValueAttribute(string json, Type type) : base(ConvertFromJson(json, type))
	//	{
	//		this.json = json;
	//	}

	//	private static object ConvertFromJson(string json, Type type)
	//	{
	//		//var value = Activator.CreateInstance(type);
	//		//JsonConvert.PopulateObject(json, value, ConfigManager.serializerSettings);
	//		//return value;
	//		var value = JsonConvert.DeserializeObject(json, type, new JsonSerializerSettings
	//		{
	//			//MissingMemberHandling = MissingMemberHandling.Error,
	//			//NullValueHandling = NullValueHandling.Include,
	//			//DefaultValueHandling = DefaultValueHandling.Populate
	//		});
	//		return value;
	//	}
	//}

	//[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
	//public class StringRepresentationAttribute : Attribute
	//{
	//	public Func<string> StringRepresentation { get; set; }

	//	public StringRepresentationAttribute(Type delegateType, string delegateName)
	//	{
	//		StringRepresentation = (Func<string>)Delegate.CreateDelegate(delegateType, delegateType.GetMethod(delegateName));
	//	}
	//}

	//[StringRepresentation(typeof(TestDelegate), "GetConnection")]
	//public class Test
	//{
	//}

	//[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	//public class StringRepresentationAttribute : Attribute
	//{
	//	public Func<string> SomeProperty { get; set; }
	//}
}
