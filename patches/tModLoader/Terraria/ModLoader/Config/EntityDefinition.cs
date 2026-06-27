using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Config;

/// <summary>
/// Classes implementing EntityDefinition serve to function as a way to save and load the identities of various Terraria objects. Only the identity is preserved, no other data such as stack size, damage, etc. These classes are well suited for ModConfig, but can be saved and loaded in a TagCompound as well.
/// <para/> An EntityDefinition can potentially refer to content removed from a mod or content from a mod not currently enabled. Use <see cref="IsUnloaded"/> to check if this is the case, especially before using <see cref="Type"/> as an index into an array. It is usually desirable to preserve all unloaded EntityDefinition entries so user data is not lost, so structure your code accordingly.
/// </summary>
public abstract class EntityDefinition : TagSerializable
{
	[DefaultValue("Terraria")]
	public string Mod;
	[DefaultValue("None")]
	public string Name;

	// Check if the type is invalid and the Mod/Name pair is NOT vanilla
	public virtual bool IsUnloaded
		=> Type <= 0 && !(Mod == "Terraria" && Name == "None" || Mod == "" && Name == "");

	/// <summary>
	/// The content ID of the content this EntityDefinition represents. Will be -1 for <see cref="IsUnloaded"/> EntityDefinition.
	/// </summary>
	[JsonIgnore]
	public abstract int Type { get; }

	public EntityDefinition()
	{
		Mod = "Terraria";
		Name = "None";
	}

	public EntityDefinition(string mod, string name)
	{
		Mod = mod;
		Name = name;
	}

	public EntityDefinition(string key)
	{
		Mod = "Terraria";
		Name = key;

		string[] parts = key.Split('/', 2);

		if (parts.Length == 2) {
			Mod = parts[0];
			Name = parts[1];
		}
	}

	public override bool Equals(object obj)
	{
		if (obj is not EntityDefinition p) {
			return false;
		}

		return (Mod == p.Mod) && (Name == p.Name);
	}

	public override string ToString()
		=> $"{Mod}/{Name}";

	public virtual string DisplayName => ToString();

	public override int GetHashCode()
		=> new { Mod, Name }.GetHashCode();

	public TagCompound SerializeData()
	{
		return new TagCompound {
			["mod"] = Mod,
			["name"] = Name,
		};
	}
}

/// <summary>
/// ItemDefinition represents an Item identity. A typical use for this class is usage in ModConfig, perhaps to facilitate an Item tweaking mod.
/// <para/> <inheritdoc/>
/// </summary>
// JSONItemConverter should allow this to be used as a dictionary key.
[TypeConverter(typeof(ToFromStringConverter<ItemDefinition>))]
//[CustomModConfigItem(typeof(UIModConfigItemDefinitionItem))]
public class ItemDefinition : EntityDefinition
{
	public static readonly Func<TagCompound, ItemDefinition> DESERIALIZER = Load;

	public override int Type => ItemID.Search.TryGetId(Mod != "Terraria" ? $"{Mod}/{Name}" : Name, out int id) ? id : -1;

	public ItemDefinition() : base() { }
	/// <summary><b>Note: </b>As ModConfig loads before other content, make sure to only use <see cref="ItemDefinition(string, string)"/> for modded content in ModConfig classes. </summary>
	public ItemDefinition(int type) : base(ItemID.Search.GetName(type)) { }
	public ItemDefinition(string key) : base(key) { }
	public ItemDefinition(string mod, string name) : base(mod, name) { }

	public static ItemDefinition FromString(string s)
		=> new(s);

	public static ItemDefinition Load(TagCompound tag)
		=> new(tag.GetString("mod"), tag.GetString("name"));

	public override string DisplayName => IsUnloaded ? Language.GetTextValue("Mods.ModLoader.Items.UnloadedItem.DisplayName") : Lang.GetItemNameValue(Type);
}

/// <summary>
/// ProjectileDefinition represents a Projectile identity.
/// <para/> <inheritdoc/>
/// </summary>
[TypeConverter(typeof(ToFromStringConverter<ProjectileDefinition>))]
public class ProjectileDefinition : EntityDefinition
{
	public static readonly Func<TagCompound, ProjectileDefinition> DESERIALIZER = Load;

	public override int Type => ProjectileID.Search.TryGetId(Mod != "Terraria" ? $"{Mod}/{Name}" : Name, out int id) ? id : -1;

	public ProjectileDefinition() : base() { }
	/// <summary><b>Note: </b>As ModConfig loads before other content, make sure to only use <see cref="ProjectileDefinition(string, string)"/> for modded content in ModConfig classes. </summary>
	public ProjectileDefinition(int type) : base(ProjectileID.Search.GetName(type)) { }
	public ProjectileDefinition(string key) : base(key) { }
	public ProjectileDefinition(string mod, string name) : base(mod, name) { }

	public static ProjectileDefinition FromString(string s)
		=> new(s);

	public static ProjectileDefinition Load(TagCompound tag)
		=> new(tag.GetString("mod"), tag.GetString("name"));

	// Projectile display names sometimes don't exist or sometimes are shared, it might be better to use Name for ModConfig in those situations?
	public override string DisplayName => IsUnloaded ? Language.GetTextValue("Mods.ModLoader.Unloaded") : Lang.GetProjectileName(Type).Value;
}

/// <summary>
/// NPCDefinition represents an NPC identity.
/// <para/> <inheritdoc/>
/// </summary>
[TypeConverter(typeof(ToFromStringConverter<NPCDefinition>))]
public class NPCDefinition : EntityDefinition
{
	public static readonly Func<TagCompound, NPCDefinition> DESERIALIZER = Load;

	public override bool IsUnloaded
		=> Type <= NPCID.NegativeIDCount && !(Mod == "Terraria" && Name == "None" || Mod == "" && Name == "");

	/// <summary>
	/// The NPCID of the NPC this NPCDefinition represents. Will be -66 (<see cref="NPCID.NegativeIDCount"/>) for <see cref="IsUnloaded"/> NPCDefinition.
	/// </summary>
	public override int Type => NPCID.Search.TryGetId(Mod != "Terraria" ? $"{Mod}/{Name}" : Name, out int id) ? id : NPCID.NegativeIDCount;

	public NPCDefinition() : base() { }
	/// <summary><b>Note: </b>As ModConfig loads before other content, make sure to only use <see cref="NPCDefinition(string, string)"/> for modded content in ModConfig classes. </summary>
	public NPCDefinition(int type) : base(NPCID.Search.GetName(type)) { }
	public NPCDefinition(string key) : base(key) { }
	public NPCDefinition(string mod, string name) : base(mod, name) { }

	public static NPCDefinition FromString(string s)
		=> new(s);

	public static NPCDefinition Load(TagCompound tag)
		=> new(tag.GetString("mod"), tag.GetString("name"));

	public override string DisplayName => IsUnloaded ? Language.GetTextValue("Mods.ModLoader.Unloaded") : Lang.GetNPCNameValue(Type);
}

/// <summary>
/// PrefixDefinition represents a Prefix identity.
/// <para/> <inheritdoc/>
/// </summary>
[TypeConverter(typeof(ToFromStringConverter<PrefixDefinition>))]
public class PrefixDefinition : EntityDefinition
{
	public static readonly Func<TagCompound, PrefixDefinition> DESERIALIZER = Load;

	public override int Type {
		get {
			if (Mod == "Terraria" && Name == "None")
				return 0;
			return PrefixID.Search.TryGetId(Mod != "Terraria" ? $"{Mod}/{Name}" : Name, out int id) ? id : -1;
		}
	}

	public PrefixDefinition() : base() { }
	/// <summary><b>Note: </b>As ModConfig loads before other content, make sure to only use <see cref="PrefixDefinition(string, string)"/> for modded content in ModConfig classes. </summary>
	public PrefixDefinition(int type) : base(PrefixID.Search.GetName(type)) { }
	public PrefixDefinition(string key) : base(key) { }
	public PrefixDefinition(string mod, string name) : base(mod, name) { }

	public static PrefixDefinition FromString(string s)
		=> new(s);

	public static PrefixDefinition Load(TagCompound tag)
		=> new(tag.GetString("mod"), tag.GetString("name"));

	public override string DisplayName {
		get {
			if (IsUnloaded)
				return Language.GetTextValue("Mods.ModLoader.Unloaded");
			if(Type == 0)
				return Lang.inter[23].Value;
			return Lang.prefix[Type].Value;
		}
	}
}

/// <summary>
/// BuffDefinition represents an Buff identity.
/// <para/> <inheritdoc/>
/// </summary>
[TypeConverter(typeof(ToFromStringConverter<BuffDefinition>))]
public class BuffDefinition : EntityDefinition
{
	public static readonly Func<TagCompound, BuffDefinition> DESERIALIZER = Load;

	public override int Type {
		get {
			if (Mod == "Terraria" && Name == "None")
				return 0;
			return BuffID.Search.TryGetId(Mod != "Terraria" ? $"{Mod}/{Name}" : Name, out int id) ? id : -1;
		}
	}

	public BuffDefinition() : base() { }
	/// <summary><b>Note: </b>As ModConfig loads before other content, make sure to only use <see cref="BuffDefinition(string, string)"/> for modded content in ModConfig classes. </summary>
	public BuffDefinition(int type) : base(BuffID.Search.GetName(type)) { }
	public BuffDefinition(string key) : base(key) { }
	public BuffDefinition(string mod, string name) : base(mod, name) { }

	public static BuffDefinition FromString(string s)
		=> new(s);

	public static BuffDefinition Load(TagCompound tag)
		=> new(tag.GetString("mod"), tag.GetString("name"));

	public override string DisplayName => IsUnloaded ? Language.GetTextValue("Mods.ModLoader.Unloaded") : Lang.GetBuffName(Type);
}

/// <summary>
/// TileDefinition represents a Tile identity.
/// <para/> <inheritdoc/>
/// </summary>
[TypeConverter(typeof(ToFromStringConverter<TileDefinition>))]
public class TileDefinition : EntityDefinition
{
	public static readonly Func<TagCompound, TileDefinition> DESERIALIZER = Load;

	// Tile ids start from 0 for some reason
	public override bool IsUnloaded
		=> Type < 0 && !(Mod == "Terraria" && Name == "None" || Mod == "" && Name == "");

	// TODO: doesn't handle tile styles, should implement when NPCs get negative id support
	public override int Type => TileID.Search.TryGetId(Mod != "Terraria" ? $"{Mod}/{Name}" : Name, out int id) ? id : -1;

	public TileDefinition() : base() { }
	/// <summary><b>Note: </b>As ModConfig loads before other content, make sure to only use <see cref="TileDefinition(string, string)"/> for modded content in ModConfig classes. </summary>
	public TileDefinition(int type) : base(TileID.Search.GetName(type)) { }
	public TileDefinition(string key) : base(key) { }
	public TileDefinition(string mod, string name) : base(mod, name) { }

	public static TileDefinition FromString(string s)
		=> new(s);

	public static TileDefinition Load(TagCompound tag)
		=> new(tag.GetString("mod"), tag.GetString("name"));

	public override string DisplayName
		=> IsUnloaded || Type == -1
		? Language.GetTextValue("Mods.ModLoader.Unloaded")
		: (Main.dedServ || (string.IsNullOrEmpty(Lang.GetMapObjectName(MapHelper.TileToLookup(Type, 0))))
		? Name
		: $"{Name} \"{Lang.GetMapObjectName(MapHelper.TileToLookup(Type, 0))}\"");
}

/// <summary>
/// This TypeConverter facilitates converting to and from the string Type. This is necessary for Objects that are to be used as Dictionary keys, since the JSON for keys needs to be a string. Classes annotated with this TypeConverter need to implement a static FromString method that returns T.
/// </summary>
/// <typeparam name="T">The Type that implements the static FromString method that returns Type T.</typeparam>
public class ToFromStringConverter<T> : TypeConverter
{
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		return destinationType != typeof(string); // Critical for populating from json string. Does prevent compact json values though.
	}

	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string)) {
			return true;
		}

		return base.CanConvertFrom(context, sourceType);
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
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
