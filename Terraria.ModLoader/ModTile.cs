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

    public int soundType = 0;
    public int soundStyle = 1;
    public int numDust = 10;
    public int dustType = 0;
    public int drop = 0;

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

    public virtual bool KillSound(int i, int j)
    {
        return false;
    }

    public virtual void NumDust(int i, int j, ref int num) { }

    public virtual bool CreateDust(int i, int j, ref int type)
    {
        type = dustType;
        return false;
    }

    public virtual void DropCritterChance(int i, int j, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance) { }

    public virtual bool Drop(int i, int j)
    {
        return false;
    }

    public virtual void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) { }
}}
