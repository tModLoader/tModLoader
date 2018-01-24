using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.Reflection;
using System.ComponentModel;

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
			PropertyInfo[] props = GetType().GetProperties(
			//	BindingFlags.DeclaredOnly |
				BindingFlags.Public |
				BindingFlags.Instance);

			foreach (PropertyInfo property in props)
			{
				ReloadRequiredAttribute reloadRequired = (ReloadRequiredAttribute)Attribute.GetCustomAttribute(property, typeof(ReloadRequiredAttribute));
				if (reloadRequired != null)
				{
					if (!property.GetValue(this, null).Equals(property.GetValue(old, null)))
					{
						return true;
					}
				}
			}

			FieldInfo[] fields = GetType().GetFields(
			//	BindingFlags.DeclaredOnly |
				BindingFlags.Public |
				BindingFlags.Instance);

			foreach (FieldInfo field in fields)
			{
				ReloadRequiredAttribute reloadRequired = (ReloadRequiredAttribute)Attribute.GetCustomAttribute(field, typeof(ReloadRequiredAttribute));
				if (reloadRequired != null)
				{
					if (!field.GetValue(this).Equals(field.GetValue(old)))
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
		UniquePerPlayer
	}

	// Attributes:
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class ReloadRequiredAttribute : Attribute
	{
	}

	// TODO: Let this be a ModTranslation key?
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class LabelAttribute : Attribute
	{
		readonly string label;
		public LabelAttribute(string label)
		{
			this.label = label;
		}
		public string Label => label;
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class TooltipAttribute : Attribute
	{
		public readonly string tooltip;
		public TooltipAttribute(string tooltip)
		{
			this.tooltip = tooltip;
		}
	}

	// Hide or Disable this item while in game.
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class HideInGameAttribute : Attribute { }

	// Hide or Disable this item while a client?
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class HideForClientAttribute : Attribute { }

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
	public class BackgroundColorAttribute : Attribute
	{
		public Color color;
		public BackgroundColorAttribute(int r, int g, int b, int a = 255)
		{
			this.color = new Color(r, g, b, a);
		}
	}

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
	//public class JsonDefaultValueAttribute: Attribute
	//{
	//	public string json;
	//	public JsonDefaultValueAttribute(string json)
	//	{
	//		this.json = json;
	//	}

	//}

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
	//			MissingMemberHandling = MissingMemberHandling.Error,
	//			NullValueHandling = NullValueHandling.Include,
	//			DefaultValueHandling = DefaultValueHandling.Populate
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

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class MyCustomAttribute : Attribute
	{
		public string SomeProperty { get; set; }
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class OptionStringsAttribute : Attribute
	{
		public string[] optionLabels { get; set; }
		public OptionStringsAttribute(string[] optionLabels)
		{
			this.optionLabels = optionLabels;
		}
	}

	//[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	//public class MultiplayerMode : Attribute
	//{
	//	public MultiplayerSyncMode Mode { get; set; }
	//}

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
	}

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
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class DrawTicksAttribute : Attribute
	{
	}
}
