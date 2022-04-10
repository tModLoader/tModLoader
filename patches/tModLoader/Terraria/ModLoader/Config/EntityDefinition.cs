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
		public string Mod;
		[DefaultValue("None")]
		public string Name;

		// Check if the type is invalid and the Mod/Name pair is NOT vanilla
		public bool IsUnloaded
			=> Type <= 0 && !(Mod == "Terraria" && Name == "None" || Mod == "" && Name == "");

		[JsonIgnore]
		public abstract int Type { get; }

		public EntityDefinition() {
			Mod = "Terraria";
			Name = "None";
		}

		public EntityDefinition(string mod, string name) {
			Mod = mod;
			Name = name;
		}

		public EntityDefinition(string key) {
			Mod = "Terraria";
			Name = key;

			string[] parts = key.Split('/', 2);

			if (parts.Length == 2) {
				Mod = parts[0];
				Name = parts[1];
			}
		}

		public override bool Equals(object obj) {
			if (obj is not EntityDefinition p) {
				return false;
			}

			return (Mod == p.Mod) && (Name == p.Name);
		}

		public override string ToString()
			=> $"{Mod} {Name}";

		public override int GetHashCode()
			=> new { Mod, Name }.GetHashCode();

		public TagCompound SerializeData() {
			return new TagCompound {
				["mod"] = Mod,
				["name"] = Name,
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
		public static readonly Func<TagCompound, ItemDefinition> DESERIALIZER = Load;

		public override int Type => ItemID.Search.TryGetId(Mod != "Terraria" ? $"{Mod}/{Name}" : Name, out int id) ? id : -1;

		public ItemDefinition() : base() { }
		public ItemDefinition(int type) : base(ItemID.Search.GetName(type)) { }
		public ItemDefinition(string key) : base(key) { }
		public ItemDefinition(string mod, string name) : base(mod, name) { }

		public static ItemDefinition FromString(string s)
			=> new(s);

		public static ItemDefinition Load(TagCompound tag)
			=> new(tag.GetString("mod"), tag.GetString("name"));
	}

	[TypeConverter(typeof(ToFromStringConverter<ProjectileDefinition>))]
	public class ProjectileDefinition : EntityDefinition
	{
		public static readonly Func<TagCompound, ProjectileDefinition> DESERIALIZER = Load;

		public override int Type => ProjectileID.Search.TryGetId(Mod != "Terraria" ? $"{Mod}/{Name}" : Name, out int id) ? id : -1;

		public ProjectileDefinition() : base() { }
		public ProjectileDefinition(int type) : base(ProjectileID.Search.GetName(type)) { }
		public ProjectileDefinition(string key) : base(key) { }
		public ProjectileDefinition(string mod, string name) : base(mod, name) { }

		public static ProjectileDefinition FromString(string s)
			=> new(s);

		public static ProjectileDefinition Load(TagCompound tag)
			=> new(tag.GetString("mod"), tag.GetString("name"));
	}

	[TypeConverter(typeof(ToFromStringConverter<NPCDefinition>))]
	public class NPCDefinition : EntityDefinition
	{
		public static readonly Func<TagCompound, NPCDefinition> DESERIALIZER = Load;

		public override int Type => NPCID.Search.TryGetId(Mod != "Terraria" ? $"{Mod}/{Name}" : Name, out int id) ? id : -1;

		public NPCDefinition() : base() { }
		public NPCDefinition(int type) : base(NPCID.Search.GetName(type)) { }
		public NPCDefinition(string key) : base(key) { }
		public NPCDefinition(string mod, string name) : base(mod, name) { }

		public static NPCDefinition FromString(string s)
			=> new(s);

		public static NPCDefinition Load(TagCompound tag)
			=> new(tag.GetString("mod"), tag.GetString("name"));
	}

	[TypeConverter(typeof(ToFromStringConverter<PrefixDefinition>))]
	public class PrefixDefinition : EntityDefinition
	{
		public static readonly Func<TagCompound, PrefixDefinition> DESERIALIZER = Load;

		public override int Type => PrefixID.Search.TryGetId(Mod != "Terraria" ? $"{Mod}/{Name}" : Name, out int id) ? id : -1;

		public PrefixDefinition() : base() { }
		public PrefixDefinition(int type) : base(PrefixID.Search.GetName(type)) { }
		public PrefixDefinition(string key) : base(key) { }
		public PrefixDefinition(string mod, string name) : base(mod, name) { }

		public static PrefixDefinition FromString(string s)
			=> new(s);

		public static PrefixDefinition Load(TagCompound tag)
			=> new(tag.GetString("mod"), tag.GetString("name"));
	}

	/// <summary>
	/// This TypeConverter facilitates converting to and from the string Type. This is necessary for Objects that are to be used as Dictionary keys, since the JSON for keys needs to be a string. Classes annotated with this TypeConverter need to implement a static FromString method that returns T.
	/// </summary>
	/// <typeparam name="T">The Type that implementes the static FromString method that returns Type T.</typeparam>
	public class ToFromStringConverter<T> : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
			return destinationType != typeof(string); // Critical for populating from json string. Does prevent compact json values though.
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
			if (sourceType == typeof(string)) {
				return true;
			}

			return base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
			if (value is string) {
				MethodInfo parse = typeof(T).GetMethod("FromString", new Type[] { typeof(string) });

				if (parse != null && parse.IsStatic && parse.ReturnType == typeof(T)) {
					return parse.Invoke(null, new object[] { value });
				}

				throw new JsonException($"The {typeof(T).Name} type does not have a public static FromString(string) method that returns a {typeof(T).Name}.");
			}

			return base.ConvertFrom(context, culture, value);
		}
	}
}
