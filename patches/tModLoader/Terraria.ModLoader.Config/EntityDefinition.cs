using System;
using System.ComponentModel;
using System.Globalization;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Config
{
	/// <summary>
	/// Classes implementing EntityDefinition serve to function as a way to save and load the identities of various Terraria objects. Only the identity is preserved, no other data such as stack size, damage, etc. These classes are well suited for ModConfig, but can be saved and loaded in a TagCompound as well. 
	/// </summary>
	public abstract class EntityDefinition : TagSerializable
	{
		public string mod;
		public string name;

		public EntityDefinition() {
			mod = "";
			name = "";
		}

		public EntityDefinition(string mod, string name) {
			this.mod = mod;
			this.name = name;
		}

		public EntityDefinition(string key) {
			this.mod = "";
			this.name = "";
			string[] parts = key.Split(new char[] { ' ' }, 2);
			if (parts.Length == 2) {
				mod = parts[0];
				name = parts[1];
			}
		}

		public override bool Equals(object obj) {
			EntityDefinition p = obj as EntityDefinition;
			if (p == null) {
				return false;
			}
			return (mod == p.mod) && (name == p.name);
		}

		public override string ToString() => $"{mod} {name}";

		public override int GetHashCode() {
			return new { mod, name }.GetHashCode();
		}

		public bool IsUnloaded => Type == 0 && !(mod == "Terraria" && name == "None" || mod == "" && name == "");

		public abstract int Type { get; }

		public TagCompound SerializeData() {
			return new TagCompound {
				["mod"] = mod,
				["name"] = name,
			};
		}
	}

	/// <summary>
	/// ItemDefinition represents an Item identity. A typical use for this class is usage in ModConfig, perhapse to facilitate an Item tweaking mod.
	/// </summary>
	// JSONItemConverter should allow this to be used as a dictionary key.
	[TypeConverter(typeof(EntityDefinitionConverter<ItemDefinition>))]
	//[CustomModConfigItem(typeof(UIModConfigItemDefinitionItem))]
	public class ItemDefinition : EntityDefinition
	{
		public ItemDefinition() : base() {
		}
		public ItemDefinition(int type) : base(ItemID.GetUniqueKey(type)) {
		}
		public ItemDefinition(string key) : base(key) {
		}
		public ItemDefinition(string mod, string name) : base(mod, name) {
		}

		public override int Type => ItemID.TypeFromUniqueKey(base.mod, base.name);

		public static readonly Func<TagCompound, ItemDefinition> DESERIALIZER = Load;
		public static ItemDefinition Load(TagCompound tag) => new ItemDefinition(tag.GetString("mod"), tag.GetString("name"));
	}

	[TypeConverter(typeof(EntityDefinitionConverter<ProjectileDefinition>))]
	public class ProjectileDefinition : EntityDefinition
	{
		public ProjectileDefinition() : base() { }
		public ProjectileDefinition(int type) : base(ProjectileID.GetUniqueKey(type)) { }
		public ProjectileDefinition(string key) : base(key) { }
		public ProjectileDefinition(string mod, string name) : base(mod, name) { }

		public override int Type => ProjectileID.TypeFromUniqueKey(mod, name);

		public static readonly Func<TagCompound, ProjectileDefinition> DESERIALIZER = Load;
		public static ProjectileDefinition Load(TagCompound tag) => new ProjectileDefinition(tag.GetString("mod"), tag.GetString("name"));
	}

	[TypeConverter(typeof(EntityDefinitionConverter<NPCDefinition>))]
	public class NPCDefinition : EntityDefinition
	{
		public NPCDefinition() : base() { }
		public NPCDefinition(int type) : base(NPCID.GetUniqueKey(type)) { }
		public NPCDefinition(string key) : base(key) { }
		public NPCDefinition(string mod, string name) : base(mod, name) { }

		public override int Type => NPCID.TypeFromUniqueKey(mod, name);

		public static readonly Func<TagCompound, NPCDefinition> DESERIALIZER = Load;
		public static NPCDefinition Load(TagCompound tag) => new NPCDefinition(tag.GetString("mod"), tag.GetString("name"));
	}

	internal class EntityDefinitionConverter<T> : TypeConverter where T : EntityDefinition, new()
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
				var definition = new T {
					mod = v[0],
					name = v[1]
				};
				return definition;
			}
			return base.ConvertFrom(context, culture, value);
		}

		// Overrides the ConvertTo method of TypeConverter.
		public override object ConvertTo(ITypeDescriptorContext context,
		   CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				EntityDefinition item = (EntityDefinition)value;
				return $"{item.mod} {item.name}";
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
