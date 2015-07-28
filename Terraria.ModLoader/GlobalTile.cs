using System;
using Terraria;

namespace Terraria.ModLoader {
public class GlobalTile
{
    public Mod mod
    {
        get;
        internal set;
    }

    public void AddToArray(ref int[] array, int type)
    {
        Array.Resize(ref array, array.Length + 1);
        array[array.Length - 1] = type;
    }

    public virtual void SetDefaults() { }
}}
