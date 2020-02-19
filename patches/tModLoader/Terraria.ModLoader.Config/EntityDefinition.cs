using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Config
{
	/// <summary>
	/// Classes implementing EntityDefinition serve to function as a way to save and load the identities of various Terraria objects. Only the identity is preserved, no other data such as stack size, damage, etc. These classes are well suited for ModConfig, but can be saved and loaded in a TagCompound as well. 
	/// </summary>
	public abstract class EntityDefinition : TagSerializable
	{
		[DefaultValue("Terraria")]
		public string mod;
		[DefaultValue("None")]
		public string name;

		public EntityDefinition() {
			mod = "Terraria";
			name = "None";
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

		[JsonIgnore]
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
	[TypeConverter(typeof(ToFromStringConverter<ItemDefinition>))]
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

		public static ItemDefinition FromString(string s) => new ItemDefinition(s);

		public static readonly Func<TagCompound, ItemDefinition> DESERIALIZER = Load;
		public static ItemDefinition Load(TagCompound tag) => new ItemDefinition(tag.GetString("mod"), tag.GetString("name"));
	}

	[TypeConverter(typeof(ToFromStringConverter<ProjectileDefinition>))]
	public class ProjectileDefinition : EntityDefinition
	{
		public ProjectileDefinition() : base() { }
		public ProjectileDefinition(int type) : base(ProjectileID.GetUniqueKey(type)) { }
		public ProjectileDefinition(string key) : base(key) { }
		public ProjectileDefinition(string mod, string name) : base(mod, name) { }

		public override int Type => ProjectileID.TypeFromUniqueKey(mod, name);

		public static ProjectileDefinition FromString(string s) => new ProjectileDefinition(s);

		public static readonly Func<TagCompound, ProjectileDefinition> DESERIALIZER = Load;
		public static ProjectileDefinition Load(TagCompound tag) => new ProjectileDefinition(tag.GetString("mod"), tag.GetString("name"));
	}

	[TypeConverter(typeof(ToFromStringConverter<NPCDefinition>))]
	public class NPCDefinition : EntityDefinition
	{
		public NPCDefinition() : base() { }
		public NPCDefinition(int type) : base(NPCID.GetUniqueKey(type)) { }
		public NPCDefinition(string key) : base(key) { }
		public NPCDefinition(string mod, string name) : base(mod, name) { }

		public override int Type => NPCID.TypeFromUniqueKey(mod, name);

		public static NPCDefinition FromString(string s) => new NPCDefinition(s);

		public static readonly Func<TagCompound, NPCDefinition> DESERIALIZER = Load;
		public static NPCDefinition Load(TagCompound tag) => new NPCDefinition(tag.GetString("mod"), tag.GetString("name"));
	}

	[TypeConverter(typeof(ToFromStringConverter<PrefixDefinition>))]
	public class PrefixDefinition : EntityDefinition
	{
		public PrefixDefinition() : base() { }
		public PrefixDefinition(int type) : base(PrefixID.GetUniqueKey((byte)type)) { }
		public PrefixDefinition(string key) : base(key) { }
		public PrefixDefinition(string mod, string name) : base(mod, name) { }

		public override int Type => PrefixID.TypeFromUniqueKey(mod, name);

		public static PrefixDefinition FromString(string s) => new PrefixDefinition(s);

		public static readonly Func<TagCompound, PrefixDefinition> DESERIALIZER = Load;
		public static PrefixDefinition Load(TagCompound tag) => new PrefixDefinition(tag.GetString("mod"), tag.GetString("name"));
	}

	/// <summary>
	/// This TypeConverter facilitates converting to and from the string Type. This is necessary for Objects that are to be used as Dictionary keys, since the JSON for keys needs to be a string. Classes annotated with this TypeConverter need to implement a static FromString method that returns T.
	/// </summary>
	/// <typeparam name="T">The Type that implementes the static FromString method that returns Type T.</typeparam>
	public class ToFromStringConverter<T> : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
			return destinationType != typeof(string); // critical for populating from json string. Does prevent compact json values though.
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType) {
			if (sourceType == typeof(string)) {
				return true;
			}
			return base.CanConvertFrom(context, sourceType);
		}
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
			if (value is string) {
				MethodInfo parse = typeof(T).GetMethod("FromString", new Type[] { typeof(string) });
				if (parse != null && parse.IsStatic && parse.ReturnType == typeof(T)) {
					return parse.Invoke(null, new object[] { value });
				}

				throw new JsonException(string.Format(
					"The {0} type does not have a public static FromString(string) method that returns a {0}.",
					typeof(T).Name));
			}
			return base.ConvertFrom(context, culture, value);
		}
	}
}
