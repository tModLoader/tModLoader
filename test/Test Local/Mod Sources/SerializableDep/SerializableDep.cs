using Terraria.ModLoader;
using System;
using Terraria.ModLoader.IO;

namespace SerializableDep;
public abstract class Parent : TagSerializable
{
	public TagCompound SerializeData() => [];
}

public class Child : Parent
{
	public static readonly Func<TagCompound, Child> DESERIALIZER = _ => new();
}

class SerializableDep : Mod
{
}
