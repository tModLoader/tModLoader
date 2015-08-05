using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.Map;

namespace Terraria.ModLoader {
public static class WallLoader
{
    private static int nextWall = WallID.Count;
    internal static readonly IDictionary<int, ModWall> walls = new Dictionary<int, ModWall>();
    internal static readonly IList<GlobalWall> globalWalls = new List<GlobalWall>();

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

    internal static void ResizeArrays()
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
    }

    internal static void Unload()
    {
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
}}
