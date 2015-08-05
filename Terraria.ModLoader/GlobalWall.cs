using System;
using Terraria;

namespace Terraria.ModLoader {
public class GlobalWall
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

    public virtual bool Autoload(ref string name)
    {
        return mod.Properties.Autoload;
    }

    public virtual void SetDefaults() { }

    public virtual bool KillSound(int i, int j, int type)
    {
        return true;
    }

    public virtual void NumDust(int i, int j, int type, bool fail, ref int num) { }

    public virtual bool CreateDust(int i, int j, int type, ref int dustType)
    {
        return true;
    }

    public virtual bool Drop(int i, int j, int type, ref int dropType)
    {
        return true;
    }

    public virtual void KillWall(int i, int j, int type, ref bool fail) { }
}}
