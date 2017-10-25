using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.Reflection;

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
				BindingFlags.DeclaredOnly |
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
				BindingFlags.DeclaredOnly |
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

	public class Test_Server : ModConfig
	{
		public override MultiplayerSyncMode Mode => MultiplayerSyncMode.ServerDictates;

		[MyCustomAttribute(SomeProperty = "hey is ")]
		bool hey;
	}


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

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class MultiplayerMode : Attribute
	{
		public MultiplayerSyncMode Mode { get; set; }
	}


	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class ValueRangeAttribute : Attribute
	{
		private int min;
		private int max;

		public ValueRangeAttribute(int min, int max)
		{
			this.min = min;
			this.max = max;
		}

		public int Max
		{
			get { return max; }
			set { max = value; }
		}

		public int Min
		{
			get { return min; }
			set { min = value; }
		}
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class FloatValueRangeAttribute : Attribute
	{
		private float min;
		private float max;

		public FloatValueRangeAttribute(float min, float max)
		{
			this.min = min;
			this.max = max;
		}

		public float Max
		{
			get { return max; }
			set { max = value; }
		}

		public float Min
		{
			get { return min; }
			set { min = value; }
		}
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class IntValueIncrementesAttribute : Attribute
	{
		public int increment;
		public IntValueIncrementesAttribute(int increment)
		{
			this.increment = increment;
		}
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class FloatValueIncrementesAttribute : Attribute
	{
		public float increment;
		public FloatValueIncrementesAttribute(float increment)
		{
			this.increment = increment;
		}
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class DrawTicksAttribute : Attribute
	{
	}
}
