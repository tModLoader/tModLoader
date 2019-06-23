using System;
using System.ComponentModel;
using System.Globalization;
using Terraria.ID;

namespace Terraria.ModLoader.Config
{
	// JSONItemConverter should allow this to be used as a dictionary key.
	[TypeConverter(typeof(ItemDefinitionConverter))]
	//[CustomModConfigItem(typeof(UIModConfigItemDefinitionItem))]
	public class ItemDefinition
	{
		public string mod;
		public string name;

		public ItemDefinition()
		{
			mod = "";
			name = "";
		}

		public ItemDefinition(string mod, string name)
		{
			this.mod = mod;
			this.name = name;
		}

		public bool IsUnloaded => GetID() == 0 && !(name == "" && mod == "");

		public override bool Equals(object obj)
		{
			ItemDefinition p = obj as ItemDefinition;
			if (p == null)
			{
				return false;
			}
			return (mod == p.mod) && (name == p.name);
		}

		public override int GetHashCode()
		{
			return new { mod, name }.GetHashCode();
		}

		public int GetID()
		{
			if (mod == "Terraria")
			{
				if (!ItemID.Search.ContainsName(name))
					return 0;
				return ItemID.Search.GetId(name);
			}
			return ModLoader.GetMod(this.mod)?.GetItem(this.name)?.item.type ?? 0;
		}
	}

	internal class ItemDefinitionConverter : TypeConverter
	{
		// Overrides the CanConvertFrom method of TypeConverter.
		// The ITypeDescriptorContext interface provides the context for the
		// conversion. Typically, this interface is used at design time to
		// provide information about the design-time container.
		public override bool CanConvertFrom(ITypeDescriptorContext context,
		   Type sourceType)
		{
			if (sourceType == typeof(string))
			{
				return true;
			}
			return base.CanConvertFrom(context, sourceType);
		}

		// Overrides the ConvertFrom method of TypeConverter.
		public override object ConvertFrom(ITypeDescriptorContext context,
		   CultureInfo culture, object value)
		{
			if (value is string)
			{
				// ModNames can't have spaces, but ItemNames can I think.
				string[] v = ((string)value).Split(new char[] { ' ' }, 2);
				return new ItemDefinition(v[0], v[1]);
			}
			return base.ConvertFrom(context, culture, value);
		}

		// Overrides the ConvertTo method of TypeConverter.
		public override object ConvertTo(ITypeDescriptorContext context,
		   CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				ItemDefinition item = (ItemDefinition)value;
				return $"{item.mod} {item.name}";
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
