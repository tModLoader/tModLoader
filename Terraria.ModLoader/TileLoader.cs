using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ObjectData;

namespace Terraria.ModLoader {
public class TileLoader
{
    //in Terraria.IO.WorldFile.SaveFileFormatHeader set initial num to TileLoader.TileCount
    private static int nextTile = TileID.Count;
    internal static readonly IDictionary<int, ModTile> tiles = new Dictionary<int, ModTile>();
    private static int vanillaChairCount = TileID.Sets.RoomNeeds.CountsAsChair.Length;
    private static int vanillaTableCount = TileID.Sets.RoomNeeds.CountsAsTable.Length;
    private static int vanillaTorchCount = TileID.Sets.RoomNeeds.CountsAsTorch.Length;
    private static int vanillaDoorCount = TileID.Sets.RoomNeeds.CountsAsDoor.Length;

    internal static int ReserveTileID()
    {
        int reserveID = nextTile;
        nextTile++;
        return reserveID;
    }

    internal static int TileCount()
    {
        return nextTile;
    }

    public static ModTile GetTile(int type)
    {
        if(tiles.ContainsKey(type))
        {
            return tiles[type];
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
        for(int j = 0; j < newSize && j < dim1; j++)
        {
            for(int k = 0; k < dim2; k++)
            {
                newArray[j, k] = array[j, k];
            }
        }
        array = newArray;
    }

    internal static void ResizeArrays()
    {
        Array.Resize(ref Main.tileSetsLoaded, nextTile);
        for(int k = TileID.Count; k < nextTile; k++)
        {
            Main.tileSetsLoaded[k] = true;
        }
        Resize2DArray(ref Main.tileAltTexture, nextTile);
        Resize2DArray(ref Main.tileAltTextureInit, nextTile);
        Resize2DArray(ref Main.tileAltTextureDrawn, nextTile);
        Array.Resize(ref Main.tileTexture, nextTile);
        Array.Resize(ref Main.tileLighted, nextTile);
        Array.Resize(ref Main.tileMergeDirt, nextTile);
        Array.Resize(ref Main.tileCut, nextTile);
        Array.Resize(ref Main.tileAlch, nextTile);
        Array.Resize(ref Main.tileShine, nextTile);
        Array.Resize(ref Main.tileShine2, nextTile);
        Array.Resize(ref Main.tileStone, nextTile);
        Array.Resize(ref Main.tileAxe, nextTile);
        Array.Resize(ref Main.tileHammer, nextTile);
        Array.Resize(ref Main.tileWaterDeath, nextTile);
        Array.Resize(ref Main.tileLavaDeath, nextTile);
        Array.Resize(ref Main.tileTable, nextTile);
        Array.Resize(ref Main.tileBlockLight, nextTile);
        Array.Resize(ref Main.tileNoSunLight, nextTile);
        Array.Resize(ref Main.tileDungeon, nextTile);
        Array.Resize(ref Main.tileSpelunker, nextTile);
        Array.Resize(ref Main.tileSolidTop, nextTile);
        Array.Resize(ref Main.tileSolid, nextTile);
        Array.Resize(ref Main.tileBouncy, nextTile);
        Array.Resize(ref Main.tileValue, nextTile);
        Array.Resize(ref Main.tileLargeFrames, nextTile);
        Array.Resize(ref Main.tileRope, nextTile);
        Array.Resize(ref Main.tileBrick, nextTile);
        Array.Resize(ref Main.tileMoss, nextTile);
        Array.Resize(ref Main.tileNoAttach, nextTile);
        Array.Resize(ref Main.tileNoFail, nextTile);
        Array.Resize(ref Main.tileObsidianKill, nextTile);
        Array.Resize(ref Main.tileFrameImportant, nextTile);
        Array.Resize(ref Main.tilePile, nextTile);
        Array.Resize(ref Main.tileBlendAll, nextTile);
        Array.Resize(ref Main.tileGlowMask, nextTile);
        Array.Resize(ref Main.tileContainer, nextTile);
        Array.Resize(ref Main.tileSign, nextTile);
        Array.Resize(ref Main.tileMerge, nextTile);
        for(int k = 0; k < nextTile; k++) //oh dear
        {
            Array.Resize(ref Main.tileMerge[k], nextTile);
        }
        Array.Resize(ref Main.tileSand, nextTile);
        Array.Resize(ref Main.tileFlame, nextTile);
        Array.Resize(ref Main.tileFrame, nextTile);
        Array.Resize(ref Main.tileFrameCounter, nextTile);
        Array.Resize(ref TileID.Sets.Conversion.Grass, nextTile);
        Array.Resize(ref TileID.Sets.Conversion.Stone, nextTile);
        Array.Resize(ref TileID.Sets.Conversion.Ice, nextTile);
        Array.Resize(ref TileID.Sets.Conversion.Sand, nextTile);
        Array.Resize(ref TileID.Sets.Conversion.HardenedSand, nextTile);
        Array.Resize(ref TileID.Sets.Conversion.Sandstone, nextTile);
        Array.Resize(ref TileID.Sets.Conversion.Thorn, nextTile);
        Array.Resize(ref TileID.Sets.Conversion.Moss, nextTile);
        Array.Resize(ref TileID.Sets.AllTiles, nextTile);
        for(int k = TileID.Count; k < nextTile; k++)
        {
            TileID.Sets.AllTiles[k] = true;
        }
        Array.Resize(ref TileID.Sets.Mud, nextTile);
        Array.Resize(ref TileID.Sets.Snow, nextTile);
        Array.Resize(ref TileID.Sets.Ices, nextTile);
        Array.Resize(ref TileID.Sets.IcesSlush, nextTile);
        Array.Resize(ref TileID.Sets.IcesSnow, nextTile);
        Array.Resize(ref TileID.Sets.GrassSpecial, nextTile);
        Array.Resize(ref TileID.Sets.JungleSpecial, nextTile);
        Array.Resize(ref TileID.Sets.HellSpecial, nextTile);
        Array.Resize(ref TileID.Sets.Leaves, nextTile);
        Array.Resize(ref TileID.Sets.GeneralPlacementTiles, nextTile);
        Array.Resize(ref TileID.Sets.CanBeClearedDuringGeneration, nextTile);
        Array.Resize(ref TileID.Sets.Corrupt, nextTile);
        Array.Resize(ref TileID.Sets.Hallow, nextTile);
        Array.Resize(ref TileID.Sets.Crimson, nextTile);
        Array.Resize(ref TileID.Sets.BlocksStairs, nextTile);
        Array.Resize(ref TileID.Sets.BlocksStairsAbove, nextTile);
        Array.Resize(ref TileID.Sets.NotReallySolid, nextTile);
        Array.Resize(ref TileID.Sets.ChecksForMerge, nextTile);
        Array.Resize(ref TileID.Sets.FramesOnKillWall, nextTile);
        Array.Resize(ref TileID.Sets.AvoidedByNPCs, nextTile);
        Array.Resize(ref TileID.Sets.InteractibleByNPCs, nextTile);
        Array.Resize(ref TileID.Sets.HousingWalls, nextTile);
        Array.Resize(ref TileID.Sets.BreakableWhenPlacing, nextTile);
        Array.Resize(ref TileID.Sets.TouchDamageVines, nextTile);
        Array.Resize(ref TileID.Sets.TouchDamageSands, nextTile);
        Array.Resize(ref TileID.Sets.TouchDamageHot, nextTile);
        Array.Resize(ref TileID.Sets.TouchDamageOther, nextTile);
        Array.Resize(ref TileID.Sets.Falling, nextTile);
        Array.Resize(ref TileID.Sets.Ore, nextTile);
    }

    internal static void Unload()
    {
        tiles.Clear();
        nextTile = TileID.Count;
        Array.Resize(ref TileID.Sets.RoomNeeds.CountsAsChair, vanillaChairCount);
        Array.Resize(ref TileID.Sets.RoomNeeds.CountsAsTable, vanillaTableCount);
        Array.Resize(ref TileID.Sets.RoomNeeds.CountsAsTorch, vanillaTorchCount);
        Array.Resize(ref TileID.Sets.RoomNeeds.CountsAsDoor, vanillaDoorCount);
    }

    //add to beginning of Terraria.IO.WorldFile.SaveWorldTiles
    internal static void WriteTable(BinaryWriter writer)
    {
        HashSet<ushort> tiles = new HashSet<ushort>();
        for(int x = 0; x < Main.maxTilesX; x++)
        {
            for(int y = 0; y < Main.maxTilesY; y++)
            {
                ushort type = Main.tile[x, y].type;
                if(type >= TileID.Count)
                {
                    tiles.Add(type);
                }
            }
        }
        if(tiles.Count == 0)
        {
            return;
        }
        for(int k = 0; k < 13; k++)
        {
            writer.Write((byte)255);
        }
        writer.Write((ushort)tiles.Count);
        foreach(ushort type in tiles)
        {
            writer.Write(type);
            ModTile tile = GetTile(type);
            writer.Write(tile.mod.Name);
            writer.Write(tile.Name);
        }
    }

    //add to beginning of Terraria.IO.WorldFile.LoadWorldTiles
    //  IDictionary<int, int> modTiles = TileLoader.ReadTable(reader);
    internal static IDictionary<int, int> ReadTable(BinaryReader reader)
    {
        IDictionary<int, int> table = new Dictionary<int, int>();
        long startPos = reader.BaseStream.Position;
        for(int k = 0; k < 13; k++)
        {
            if(reader.ReadByte() != (byte)255)
            {
                reader.BaseStream.Seek(startPos, SeekOrigin.Begin);
                return table;
            }
        }
        ushort count = reader.ReadUInt16();
        for(ushort k = 0; k < count; k++)
        {
            ushort type = reader.ReadUInt16();
            string modName = reader.ReadString();
            string tileName = reader.ReadString();
            Mod mod = ModLoader.GetMod(modName);
            if (mod == null)
            {
                table[(int)type] = 0;
            }
            else
            {
                table[(int)type] = mod.TileType(tileName);
            }
        }
        return table;
    }

    //in Terraria.IO.WorldFile.LoadWorldTiles replace tile.type = (ushort)num2; with
    //  tile.type = TileLoader.ReadTileType(ref num2, modTiles);
    internal static ushort ReadTileType(ref int type, IDictionary<int, int> table)
    {
        if(table.ContainsKey(type))
        {
            type = table[type];
        }
        return (ushort)type;
    }
}}
