using System;
using Terraria;

namespace Terraria.ModLoader {
public class ModWall
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

    public virtual bool Autoload(ref string name, ref string texture)
    {
        return mod.Properties.Autoload;
    }

    public virtual void SetDefaults() { }
}}
