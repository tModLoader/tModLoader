using System;
using Terraria;
using Terraria.ID;

namespace Terraria.ModLoader{
public class ModTile
{
    public Mod mod
    {
        get;
        internal set;
    }
    public string Name
    {
        get;
        internal set;
    }
    public ushort Type
    {
        get;
        internal set;
    }
    internal string texture;

    public void AddToArray(ref int[] array)
    {
        Array.Resize(ref array, array.Length + 1);
        array[array.Length - 1] = Type;
    }

    public virtual bool Autoload(ref string name, ref string texture)
    {
        return mod.Properties.Autoload;
    }

    public virtual void SetDefaults() { }
}}
