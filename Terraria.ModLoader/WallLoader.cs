using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Map;

namespace Terraria.ModLoader {
public static class WallLoader
{
    private static int nextWall = WallID.Count;
    internal static readonly IDictionary<int, ModWall> walls = new Dictionary<int, ModWall>();
    internal static readonly IList<GlobalWall> globalWalls = new List<GlobalWall>();
    private static bool loaded = false;

    internal static int ReserveWallID()
    {
        int reserveID = nextWall;
        nextWall++;
        return reserveID;
    }

    internal static int WallCount()
    {
        return nextWall;
    }

    public static ModWall GetWall(int type)
    {
        if(walls.ContainsKey(type))
        {
            return walls[type];
        }
        else
        {
            return null;
        }
    }

    private static void Resize2DArray<T>(ref T[,] array, int newSize)
    {
        int dim1 = array.GetLength(0);
        int dim2 = array.GetLength(1);
        T[,] newArray = new T[newSize, dim2];
        for (int j = 0; j < newSize && j < dim1; j++)
        {
            for (int k = 0; k < dim2; k++)
            {
                newArray[j, k] = array[j, k];
            }
        }
        array = newArray;
    }

    internal static void ResizeArrays(bool unloading = false)
    {
        Array.Resize(ref Main.wallLoaded, nextWall);
        for (int k = WallID.Count; k < nextWall; k++)
        {
            Main.wallLoaded[k] = true;
        }
        Resize2DArray(ref Main.wallAltTexture, nextWall);
        Resize2DArray(ref Main.wallAltTextureInit, nextWall);
        Resize2DArray(ref Main.wallAltTextureDrawn, nextWall);
        Array.Resize(ref Main.wallTexture, nextWall);
        Array.Resize(ref Main.wallHouse, nextWall);
        Array.Resize(ref Main.wallDungeon, nextWall);
        Array.Resize(ref Main.wallLight, nextWall);
        Array.Resize(ref Main.wallBlend, nextWall);
        Array.Resize(ref Main.wallLargeFrames, nextWall);
        Array.Resize(ref Main.wallFrame, nextWall);
        Array.Resize(ref Main.wallFrameCounter, nextWall);
        Array.Resize(ref MapHelper.wallLookup, nextWall);
        Array.Resize(ref WallID.Sets.Conversion.Grass, nextWall);
        Array.Resize(ref WallID.Sets.Conversion.Stone, nextWall);
        Array.Resize(ref WallID.Sets.Conversion.Sandstone, nextWall);
        Array.Resize(ref WallID.Sets.Conversion.HardenedSand, nextWall);
        Array.Resize(ref WallID.Sets.Transparent, nextWall);
        Array.Resize(ref WallID.Sets.Corrupt, nextWall);
        Array.Resize(ref WallID.Sets.Crimson, nextWall);
        Array.Resize(ref WallID.Sets.Hallow, nextWall);
        if(!unloading)
        {
            loaded = true;
        }
    }

    internal static void Unload()
    {
        loaded = false;
        walls.Clear();
        nextWall = WallID.Count;
        globalWalls.Clear();
    }
    
    //change type of Terraria.Tile.wall to ushort and fix associated compile errors
    //in Terraria.IO.WorldFile.SaveWorldTiles increase length of array by 1 from 13 to 14
    //in Terraria.IO.WorldFile.SaveWorldTiles inside block if (tile.wall != 0) after incrementing num2
    //  call WallLoader.WriteType(tile.wall, array, ref num2, ref b3);
    internal static void WriteType(ushort wall, byte[] data, ref int index, ref byte flags)
    {
        if(wall > 255)
        {
            data[index] = (byte)(wall >> 8);
            index++;
            flags |= 32;
        }
    }

    //in Terraria.IO.WorldFile.LoadWorldTiles after setting tile.wall call
    //  WallLoader.ReadType(ref tile.wall, reader, b, modWalls);
    //in Terraria.IO.WorldFile.ValidateWorld before if ((b2 & 16) == 16)
    //  replace fileIO.ReadByte(); with ushort wall = fileIO.ReadByte();
    //  ushort _ = 0; WallLoader.ReadType(ref wall, fileIO, b2, new Dictionary<int, int>());
    internal static void ReadType(ref ushort wall, BinaryReader reader, byte flags, IDictionary<int, int> wallTable)
    {
        if((flags & 32) == 32)
        {
            wall |= (ushort)(reader.ReadByte() << 8);
        }
        if(wallTable.ContainsKey(wall))
        {
            wall = (ushort)wallTable[wall];
        }
    }

    //in Terraria.WorldGen.KillWall add if(!WallLoader.KillSound(i, j, tile.wall)) { } to beginning of
    //  if/else chain for playing sounds, and turn first if into else if
    internal static bool KillSound(int i, int j, int type)
    {
        foreach(GlobalWall globalWall in globalWalls)
        {
            if(!globalWall.KillSound(i, j, type))
            {
                return false;
            }
        }
        ModWall modWall = GetWall(type);
        if(modWall != null)
        {
            if(!modWall.KillSound(i, j))
            {
                return false;
            }
            Main.PlaySound(modWall.soundType, i * 16, j * 16, modWall.soundStyle);
            return false;
        }
        return true;
    }

    //in Terraria.WorldGen.KillWall after if statement setting num to 3 add
    //  WallLoader.NumDust(i, j, tile.wall, fail, ref num);
    internal static void NumDust(int i, int j, int type, bool fail, ref int numDust)
    {
        ModWall modWall = GetWall(type);
        if(modWall != null)
        {
            modWall.NumDust(i, j, fail, ref numDust);
        }
        foreach(GlobalWall globalWall in globalWalls)
        {
            globalWall.NumDust(i, j, type, fail, ref numDust);
        }
    }

    //in Terraria.WorldGen.KillWall before if statements creating dust add
    //  if(!WallLoader.CreateDust(i, j, tile.wall, ref int num2)) { continue; }
    internal static bool CreateDust(int i, int j, int type, ref int dustType)
    {
        foreach(GlobalWall globalWall in globalWalls)
        {
            if(!globalWall.CreateDust(i, j, type, ref dustType))
            {
                return false;
            }
        }
        ModWall modWall = GetWall(type);
        if(modWall != null)
        {
            return modWall.CreateDust(i, j, ref dustType);
        }
        return true;
    }

    //in Terraria.WorldGen.KillWall replace if (num4 > 0) with
    //  if (WallLoader.Drop(i, j, tile.wall, ref num4) && num4 > 0)
    internal static bool Drop(int i, int j, int type, ref int dropType)
    {
        foreach(GlobalWall globalWall in globalWalls)
        {
            if(!globalWall.Drop(i, j, type, ref dropType))
            {
                return false;
            }
        }
        ModWall modWall = GetWall(type);
        if(modWall != null)
        {
            return modWall.Drop(i, j, ref dropType);
        }
        return true;
    }

    //in Terraria.WorldGen.KillWall after if statements setting fail to true call
    //  WallLoader.KillWall(i, j, tile.wall, ref fail);
    internal static void KillWall(int i, int j, int type, ref bool fail)
    {
        ModWall modWall = GetWall(type);
        if(modWall != null)
        {
            modWall.KillWall(i, j, ref fail);
        }
        foreach(GlobalWall globalWall in globalWalls)
        {
            globalWall.KillWall(i, j, type, ref fail);
        }
    }

    //in Terraria.Lighting.PreRenderPhase after wall modifies light call
    //  WallLoader.ModifyLight(n, num17, wall, ref num18, ref num19, ref num20);
    internal static void ModifyLight(int i, int j, int type, ref float r, ref float g, ref float b)
    {
        ModWall modWall = GetWall(type);
        if(modWall != null)
        {
            modWall.ModifyLight(i, j, ref r, ref g, ref b);
        }
        foreach(GlobalWall globalWall in globalWalls)
        {
            globalWall.ModifyLight(i, j, type, ref r, ref g, ref b);
        }
    }

    //in Terraria.WorldGen.UpdateWorld after each call to TileLoader.RandomUpdate call
    //  WallLoader.RandomUpdate(num7, num8, Main.tile[num7, num8].wall);
    //  WallLoader.RandomUpdate(num64, num65, Main.tile[num64, num65].wall);
    internal static void RandomUpdate(int i, int j, int type)
    {
        ModWall modWall = GetWall(type);
        if(modWall != null)
        {
            modWall.RandomUpdate(i, j);
        }
        foreach(GlobalWall globalWall in globalWalls)
        {
            globalWall.RandomUpdate(i, j, type);
        }
    }

    //in Terraria.Main.Update after vanilla wall animations call WallLoader.AnimateWalls();
    internal static void AnimateWalls()
    {
        if(loaded)
        {
            foreach(ModWall modWall in walls.Values)
            {
                modWall.AnimateWall(ref Main.wallFrame[modWall.Type], ref Main.wallFrameCounter[modWall.Type]);
            }
        }
    }

    //in Terraria.Main.DrawWalls before if statements that do the drawing add
    //  if(!WallLoader.PreDraw(j, i, wall, Main.spriteBatch))
    //  { WallLoader.PostDraw(j, i, wall, Main.spriteBatch); continue; }
    internal static bool PreDraw(int i, int j, int type, SpriteBatch spriteBatch)
    {
        foreach(GlobalWall globalWall in globalWalls)
        {
            if(!globalWall.PreDraw(i, j, type, spriteBatch))
            {
                return false;
            }
        }
        ModWall modWall = GetWall(type);
        if(modWall != null)
        {
            return modWall.PreDraw(i, j, spriteBatch);
        }
        return true;
    }

    //in Terraria.Main.DrawWalls after wall outlines are drawn call
    //  WallLoader.PostDraw(j, i, wall, Main.spriteBatch);
    internal static void PostDraw(int i, int j, int type, SpriteBatch spriteBatch)
    {
        ModWall modWall = GetWall(type);
        if(modWall != null)
        {
            modWall.PostDraw(i, j, spriteBatch);
        }
        foreach(GlobalWall globalWall in globalWalls)
        {
            globalWall.PostDraw(i, j, type, spriteBatch);
        }
    }
}}
