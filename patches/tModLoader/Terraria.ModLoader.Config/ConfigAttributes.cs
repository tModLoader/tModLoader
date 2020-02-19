using Microsoft.Xna.Framework;
using System;
using System.ComponentModel;

namespace Terraria.ModLoader.Config
{
	/// <summary>
	/// Specifies a background color to be used for the property, field, or class in the ModConfig UI. 
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
	public class BackgroundColorAttribute : Attribute
	{
		public Color color;
		public BackgroundColorAttribute(int r, int g, int b, int a = 255) {
			this.color = new Color(r, g, b, a);
		}
	}

	/// <summary>
	/// Specifies a slider color for ModConfig elements that use a slider. The default color is white.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
	public class SliderColorAttribute : Attribute
	{
		public Color color;
		public SliderColorAttribute(int r, int g, int b, int a = 255) {
			this.color = new Color(r, g, b, a);
		}
	}

	/// <summary>
	/// This attribute hints that changing the value of the annotated property or field will put the config in a state that requires a reload. An overridden ModConfig.NeedsReload can further validate if more complex logic is needed.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class ReloadRequiredAttribute : Attribute
	{
	}

	/// <summary>
	/// This attribute sets a label for the property, field, or class for use in the ModConfig UI. 
	/// Starting the label with $ means the label should be interpreted as a Localization key.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
	public class LabelAttribute : Attribute
	{
		readonly string label;
		public LabelAttribute(string label) {
			this.label = label;
		}
		public string Label => label.StartsWith("$") ? Localization.Language.GetTextValue(label.Substring(1)) : label;
	}

	/// <summary>
	/// This attribute sets a hover tooltip for the annotated property or field to be shown in the ModConfig UI. This can be longer and more descriptive than Label.
	/// Starting the tooltip with $ means the tooltip should be interpreted as a Localization key.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class TooltipAttribute : Attribute
	{
		readonly string tooltip;
		public TooltipAttribute(string tooltip) {
			this.tooltip = tooltip;
		}
		public string Tooltip => tooltip.StartsWith("$") ? Localization.Language.GetTextValue(tooltip.Substring(1)) : tooltip;
	}

	/// <summary>
	/// This attribute adds a label above this property or field in the ModConfig UI that acts as a header. Use this to delineate sections within your config.
	/// Note that fields will be in order, and properties will be in order, but fields and properties will not be interleaved together in the source code order.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class HeaderAttribute : Attribute
	{
		readonly string header;
		public HeaderAttribute(string header) {
			this.header = header;
		}
		public string Header => header.StartsWith("$") ? Localization.Language.GetTextValue(header.Substring(1)) : header;
	}

	/// <summary>
	/// Use this attribute to specify a custom UI element to be used for the annotated property, field, or class in the ModConfig UI. 
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Enum)]
	public class CustomModConfigItemAttribute : Attribute
	{
		public Type t;
		public CustomModConfigItemAttribute(Type t) {
			this.t = t;
		}
	}

	/// <summary>
	/// Similar to DefaultValueAttribute but for reference types. It uses a json string that will be used populate this element when initialized. Defines the default value, expressed as json, to be used to populate an object with the NullAllowed attribute. Modders should only use this in conjuction with NullAllowed, as simply initializing the field with a default value is preferred.
	/// </summary>
	public class JsonDefaultValueAttribute : Attribute
	{
		public string json;
		public JsonDefaultValueAttribute(string json) {
			this.json = json;
		}
	}

	/// <summary>
	/// Defines the default value to be added when using the ModConfig UI to add elements to a Collection (List, Set, or Dictionary value). Works the same as System.ComponentModel.DefaultValueAttribute, but can't inherit from it because it would break when deserializing any data structure annotated with it.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class DefaultListValueAttribute : Attribute
	{
		private object value;
		public DefaultListValueAttribute(Type type, string value) {
			try {
				this.value = TypeDescriptor.GetConverter(type).ConvertFromInvariantString(value);
			}
			catch {
				Logging.tML.Error("Default value attribute of type " + type.FullName + " threw converting from the string '" + value + "'.");
			}
		}

		public DefaultListValueAttribute(char value) => this.value = value;
		public DefaultListValueAttribute(byte value) => this.value = value;
		public DefaultListValueAttribute(short value) => this.value = value;
		public DefaultListValueAttribute(int value) => this.value = value;
		public DefaultListValueAttribute(long value) => this.value = value;
		public DefaultListValueAttribute(float value) => this.value = value;
		public DefaultListValueAttribute(double value) => this.value = value;
		public DefaultListValueAttribute(bool value) => this.value = value;
		public DefaultListValueAttribute(string value) => this.value = value;
		public DefaultListValueAttribute(object value) => this.value = value;

		public virtual object Value => value;

		public override bool Equals(object obj) {
			if (obj == this) {
				return true;
			}
			var other = obj as DefaultListValueAttribute;
			if (other != null) {
				if (Value != null) {
					return Value.Equals(other.Value);
				}
				else {
					return (other.Value == null);
				}
			}
			return false;
		}

		public override int GetHashCode() => base.GetHashCode();

		protected void SetValue(object value) {
			this.value = value;
		}
	}

	/// <summary>
	/// Defines the default key value to be added when using the ModConfig UI to add elements to a Dictionary. Works the same as System.ComponentModel.DefaultValueAttribute, but can't inherit from it because it would break when deserializing any data structure annotated with it. This attribute compliments DefaultListValueAttribute when used annotating a Dictionary.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class DefaultDictionaryKeyValueAttribute : Attribute
	{
		private object value;
		public DefaultDictionaryKeyValueAttribute(Type type, string value) {
			try {
				this.value = TypeDescriptor.GetConverter(type).ConvertFromInvariantString(value);
			}
			catch {
				Logging.tML.Error("Default value attribute of type " + type.FullName + " threw converting from the string '" + value + "'.");
			}
		}

		public DefaultDictionaryKeyValueAttribute(char value) => this.value = value;
		public DefaultDictionaryKeyValueAttribute(byte value) => this.value = value;
		public DefaultDictionaryKeyValueAttribute(short value) => this.value = value;
		public DefaultDictionaryKeyValueAttribute(int value) => this.value = value;
		public DefaultDictionaryKeyValueAttribute(long value) => this.value = value;
		public DefaultDictionaryKeyValueAttribute(float value) => this.value = value;
		public DefaultDictionaryKeyValueAttribute(double value) => this.value = value;
		public DefaultDictionaryKeyValueAttribute(bool value) => this.value = value;
		public DefaultDictionaryKeyValueAttribute(string value) => this.value = value;
		public DefaultDictionaryKeyValueAttribute(object value) => this.value = value;

		public virtual object Value => value;

		public override bool Equals(object obj) {
			if (obj == this) {
				return true;
			}
			var other = obj as DefaultDictionaryKeyValueAttribute;
			if (other != null) {
				if (Value != null) {
					return Value.Equals(other.Value);
				}
				else {
					return (other.Value == null);
				}
			}
			return false;
		}

		public override int GetHashCode() => base.GetHashCode();

		protected void SetValue(object value) {
			this.value = value;
		}
	}

	/// <summary>
	/// Similar to DefaultListValueAttribute but for reference types. It uses a json string that will be used populate new instances list elements. Defines the default value, expressed as json, to be added when using the ModConfig UI to add elements to a Collection (List, Set, or Dictionary value).
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class JsonDefaultListValueAttribute : Attribute
	{
		public string json;
		public JsonDefaultListValueAttribute(string json) {
			this.json = json;
		}
	}

	/* TODO: Implement this
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class CustomAddMethodAttribute : Attribute
	{
		public string methodName;
		public CustomAddMethodAttribute(string methodName) {
			this.methodName = methodName;
		}
	}
	*/

	/// <summary>
	/// Similar to JsonDefaultListValueAttribute, but for assigning to the Dictionary Key rather than the Value.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class JsonDefaultDictionaryKeyValueAttribute : Attribute
	{
		public string json;
		public JsonDefaultDictionaryKeyValueAttribute(string json) {
			this.json = json;
		}
	}

	/// <summary>
	/// By default, string fields will provide the user with a text input field. Use this attribute to restrict strings to a selection of options.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class OptionStringsAttribute : Attribute
	{
		public string[] optionLabels { get; set; }
		public OptionStringsAttribute(string[] optionLabels) {
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
		public IncrementAttribute(int increment) {
			this.increment = increment;
		}
		public IncrementAttribute(float increment) {
			this.increment = increment;
		}
		public IncrementAttribute(uint increment) {
			this.increment = increment;
		}
		public IncrementAttribute(byte increment) {
			this.increment = increment;
		}
	}

	/// <summary>
	/// Specifies a range for primitive data values. Without this, default min and max are as follows: float: 0, 1 - int/uint: 0, 100 - byte: 0, 255
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class RangeAttribute : Attribute
	{
		public object min;
		public object max;

		public RangeAttribute(int min, int max) {
			this.min = min;
			this.max = max;
		}

		public RangeAttribute(float min, float max) {
			this.min = min;
			this.max = max;
		}
		public RangeAttribute(uint min, uint max) {
			this.min = min;
			this.max = max;
		}
		public RangeAttribute(byte min, byte max) {
			this.min = min;
			this.max = max;
		}
	}

	/// <summary>
	/// Affects whether this data will be presented as a slider of an input field. Add this attribute to use a slider. Currently only affects data of type int.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class SliderAttribute : Attribute
	{
	}

	/// <summary>
	/// Add this attribute and the sliders will show white tick marks at each increment.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class DrawTicksAttribute : Attribute
	{
	}

	/// <summary>
	/// Add this attribute to a Color item and Alpha will not be presented in the UI and will remain as 255 unless manually edited.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class ColorNoAlphaAttribute : Attribute
	{
	}

	/// <summary>
	/// Add this attribute to a Color item and the UI will present a Hue, Saturation, and Lightness sliders rather than Red, Green, and Blue sliders. Pass in false to skip Saturation and Lightness.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class ColorHSLSliderAttribute : Attribute
	{
		public bool showSaturationAndLightness;
		public ColorHSLSliderAttribute(bool showSaturationAndLightness = true) {
			this.showSaturationAndLightness = showSaturationAndLightness;
		}
	}

	/// <summary>
	/// This specifies that the annotated item will appear as a button that leads to a separate page in the UI. Use this to organize hierarchies.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
	public class SeparatePageAttribute : Attribute
	{
	}

	/// <summary>
	/// This attribute means the annotated item can possibly be null. This will allow the UI to make the item null. It is up to the modder to make sure the item isn't null in the ModConfig constructor and nested classes.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
	public class NullAllowedAttribute : Attribute
	{
	}

	// Unimplemented ideas below:

	// Hide or Disable this item while in game.
	//[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	//public class HideInGameAttribute : Attribute { }

	// Hide or Disable this item while a client?
	//[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	//public class HideForClientAttribute : Attribute { }

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
