using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;

namespace Terraria.ModLoader
{
	public static class TileLoader
	{
		//make Terraria.ObjectData.TileObjectData._data internal
		//make all static Terraria.ObjectData.TileObjectData.StyleName fields public
		//make Terraria.ObjectData.TileObjectData.LinkedAlternates public
		//make Terraria.ObjectData.TileObjectData.SubTiles and Alternates internal
		//at end of Terraria.ObjectData.TileObjectData.Initialize remove TileObjectData.readOnlyData = true;
		//at beginning of Terraria.WorldGen.PlaceTile remove type too high check
		//at beginning of Terraria.WorldGen.PlaceObject remove type too high check
		//in Terraria.WorldGen.Convert remove type too high checks
		//in Terraria.WorldGen.StartRoomCheck change 419 to WorldGen.houseTile.Length
		//at end of Terraria.WorldGen.KillWall remove type too high check
		//in Terraria.Player change adjTile and oldAdjTile size to TileLoader.TileCount()
		//in Terraria.Player.AdjTiles change 419 to adjTile.Length
		//in Terraria.Lighting for accOreFinder replace 419 with Main.tileValue.Length
		//make Terraria.WorldGen public
		//in Terraria.IO.WorldFile.SaveFileFormatHeader set initial num to TileLoader.TileCount
		private static int nextTile = TileID.Count;
		internal static readonly IDictionary<int, ModTile> tiles = new Dictionary<int, ModTile>();
		internal static readonly IList<GlobalTile> globalTiles = new List<GlobalTile>();
		private static bool loaded = false;
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
			if (tiles.ContainsKey(type))
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
			Array.Resize(ref Main.tileSetsLoaded, nextTile);
			for (int k = TileID.Count; k < nextTile; k++)
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
			for (int k = 0; k < nextTile; k++) //oh dear
			{
				Array.Resize(ref Main.tileMerge[k], nextTile);
			}
			Array.Resize(ref Main.tileSand, nextTile);
			Array.Resize(ref Main.tileFlame, nextTile);
			Array.Resize(ref Main.tileFrame, nextTile);
			Array.Resize(ref Main.tileFrameCounter, nextTile);
			Array.Resize(ref WorldGen.tileCounts, nextTile);
			Array.Resize(ref WorldGen.houseTile, nextTile);
			Array.Resize(ref TileID.Sets.Conversion.Grass, nextTile);
			Array.Resize(ref TileID.Sets.Conversion.Stone, nextTile);
			Array.Resize(ref TileID.Sets.Conversion.Ice, nextTile);
			Array.Resize(ref TileID.Sets.Conversion.Sand, nextTile);
			Array.Resize(ref TileID.Sets.Conversion.HardenedSand, nextTile);
			Array.Resize(ref TileID.Sets.Conversion.Sandstone, nextTile);
			Array.Resize(ref TileID.Sets.Conversion.Thorn, nextTile);
			Array.Resize(ref TileID.Sets.Conversion.Moss, nextTile);
			Array.Resize(ref TileID.Sets.AllTiles, nextTile);
			for (int k = TileID.Count; k < nextTile; k++)
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
			while (TileObjectData._data.Count < nextTile)
			{
				TileObjectData._data.Add(null);
			}
			if (!unloading)
			{
				loaded = true;
			}
		}

		internal static void Unload()
		{
			loaded = false;
			tiles.Clear();
			nextTile = TileID.Count;
			globalTiles.Clear();
			Array.Resize(ref TileID.Sets.RoomNeeds.CountsAsChair, vanillaChairCount);
			Array.Resize(ref TileID.Sets.RoomNeeds.CountsAsTable, vanillaTableCount);
			Array.Resize(ref TileID.Sets.RoomNeeds.CountsAsTorch, vanillaTorchCount);
			Array.Resize(ref TileID.Sets.RoomNeeds.CountsAsDoor, vanillaDoorCount);
			while (TileObjectData._data.Count > TileID.Count)
			{
				TileObjectData._data.RemoveAt(TileObjectData._data.Count - 1);
			}
		}
		//in Terraria.WorldGen.TileFrame after if else chain inside frameImportant if statement before return add
		//  else { TileLoader.CheckModTile(i, j, num); }
		//in Terraria.TileObject.CanPlace add optional checkStay parameter as false to end
		//  and add && !checkStay to if statement that sets flag4
		public static void CheckModTile(int i, int j, int type)
		{
			if (WorldGen.destroyObject)
			{
				return;
			}
			TileObjectData tileData = TileObjectData.GetTileData(type, 0, 0);
			if (tileData == null)
			{
				return;
			}
			int frameX = Main.tile[i, j].frameX;
			int frameY = Main.tile[i, j].frameY;
			int subX = frameX / tileData.CoordinateFullWidth;
			int subY = frameY / tileData.CoordinateFullHeight;
			int wrap = tileData.StyleWrapLimit;
			if (wrap == 0)
			{
				wrap = 1;
			}
			int subTile = tileData.StyleHorizontal ? subY * wrap + subX : subX * wrap + subY;
			int style = subTile / tileData.StyleMultiplier;
			int alternate = subTile % tileData.StyleMultiplier;
			for (int k = 0; k < tileData.AlternatesCount; k++)
			{
				if (alternate >= tileData.Alternates[k].Style && alternate <= tileData.Alternates[k].Style + tileData.RandomStyleRange)
				{
					alternate = k;
					break;
				}
			}
			tileData = TileObjectData.GetTileData(type, style, alternate + 1);
			int partFrameX = frameX % tileData.CoordinateFullWidth;
			int partFrameY = frameY % tileData.CoordinateFullHeight;
			int partX = partFrameX / (tileData.CoordinateWidth + tileData.CoordinatePadding);
			int partY = 0;
			int remainingFrameY = partFrameY;
			while (remainingFrameY > 0)
			{
				remainingFrameY -= tileData.CoordinateHeights[partY] + tileData.CoordinatePadding;
				partY++;
			}
			i -= partX;
			j -= partY;
			int originX = i + tileData.Origin.X;
			int originY = j + tileData.Origin.Y;
			TileObject objectData;
			bool partiallyDestroyed = false;
			for (int x = i; x < i + tileData.Width; x++)
			{
				for (int y = j; y < j + tileData.Height; y++)
				{
					if (!Main.tile[x, y].active() || Main.tile[x, y].type != type)
					{
						partiallyDestroyed = true;
						break;
					}
				}
				if (partiallyDestroyed)
				{
					break;
				}
			}
			if (partiallyDestroyed || !TileObject.CanPlace(originX, originY, type, style, 0, out objectData, true, true))
			{
				WorldGen.destroyObject = true;
				for (int x = i; x < i + tileData.Width; x++)
				{
					for (int y = j; y < j + tileData.Height; y++)
					{
						if (Main.tile[x, y].type == type && Main.tile[x, y].active())
						{
							WorldGen.KillTile(x, y, false, false, false);
						}
					}
				}
				KillMultiTile(i, j, frameX - partFrameX, frameY - partFrameY, type);
				WorldGen.destroyObject = false;
				for (int x = i - 1; x < i + tileData.Width + 2; x++)
				{
					for (int y = j - 1; y < j + tileData.Height + 2; y++)
					{
						WorldGen.TileFrame(x, y, false, false);
					}
				}
			}
		}

		public static void DisableSmartCursor(Tile tile, ref bool disable)
		{
			if (tile.active())
			{
				ModTile modTile = GetTile(tile.type);
				if (modTile != null)
				{
					disable = modTile.disableSmartCursor;
				}
			}
		}
		//in Terraria.WorldGen.OpenDoor replace bad type check with TileLoader.OpenDoorID(Main.tile[i, j]) < 0
		//in Terraria.WorldGen.OpenDoor replace 11 with (ushort)TileLoader.OpenDoorID
		//replace all type checks before WorldGen.OpenDoor
		public static int OpenDoorID(Tile tile)
		{
			ModTile modTile = GetTile(tile.type);
			if (modTile != null)
			{
				return modTile.openDoorID;
			}
			if (tile.type == TileID.ClosedDoor && (tile.frameY < 594 || tile.frameY > 646))
			{
				return TileID.OpenDoor;
			}
			return -1;
		}
		//in Terraria.WorldGen.CloseDoor replace bad type check with TileLoader.CloseDoorID(Main.tile[i, j]) < 0
		//in Terraria.WorldGen.CloseDoor replace 10 with (ushort)TileLoader.CloseDoorID
		//replace all type checks before WorldGen.CloseDoor
		//replace type check in WorldGen.CheckRoom
		public static int CloseDoorID(Tile tile)
		{
			ModTile modTile = GetTile(tile.type);
			if (modTile != null)
			{
				return modTile.closeDoorID;
			}
			if (tile.type == TileID.OpenDoor)
			{
				return TileID.ClosedDoor;
			}
			return -1;
		}
		//replace chest checks (type == 21) with this
		public static bool IsChest(int type)
		{
			if (type == TileID.Containers)
			{
				return true;
			}
			return ModChestName(type).Length > 0;
		}
		//in Terraria.UI.ChestUI add this to Lang lookups
		public static string ModChestName(int type)
		{
			ModTile modTile = GetTile(type);
			if (modTile != null)
			{
				return modTile.chest;
			}
			return "";
		}
		//in Terraria.Player.CheckSpawn add this to bed type check
		public static bool IsModBed(int type)
		{
			ModTile modTile = GetTile(type);
			if (modTile == null)
			{
				return false;
			}
			return modTile.bed;
		}
		//replace tile type checks for 19 in Terraria.Player, some places in Terraria.WorldGen,
		//  Terraria.Main.DrawTiles, and Terraria.Collision
		public static bool IsPlatform(int type)
		{
			ModTile modTile = GetTile(type);
			if (modTile == null)
			{
				return type == TileID.Platforms;
			}
			return modTile.platform;
		}

		public static bool IsTorch(int type)
		{
			ModTile modTile = GetTile(type);
			if (modTile == null)
			{
				return type == TileID.Torches;
			}
			return modTile.torch;
		}

		public static bool IsModMusicBox(Tile tile)
		{
			return SoundLoader.tileToMusic.ContainsKey(tile.type)
			&& SoundLoader.tileToMusic[tile.type].ContainsKey(tile.frameY / 36 * 36);
		}
		//in Terraria.ObjectData.TileObject data make the following public:
		//  newTile, newSubTile, newAlternate, addTile, addSubTile, addAlternate
		internal static void SetDefaults(ModTile tile)
		{
			tile.SetDefaults();
			if (Main.tileLavaDeath[tile.Type])
			{
				Main.tileObsidianKill[tile.Type] = true;
			}
			if (Main.tileSolid[tile.Type])
			{
				Main.tileNoSunLight[tile.Type] = true;
			}
			tile.PostSetDefaults();
		}
		//in Terraria.WorldGen.KillTile inside if (!effectOnly && !WorldGen.stopDrops) for playing sounds
		//  add if(!TileLoader.KillSound(i, j, tile.type)) { } to beginning of if/else chain and turn first if into else if
		public static bool KillSound(int i, int j, int type)
		{
			foreach (GlobalTile globalTile in globalTiles)
			{
				if (!globalTile.KillSound(i, j, type))
				{
					return false;
				}
			}
			ModTile modTile = GetTile(type);
			if (modTile != null)
			{
				if (!modTile.KillSound(i, j))
				{
					return false;
				}
				Main.PlaySound(modTile.soundType, i * 16, j * 16, modTile.soundStyle);
				return false;
			}
			return true;
		}
		//in Terraria.WorldGen.KillTile before num14 (num dust iteration) is declared, add
		//  TileLoader.NumDust(i, j, tile.type, ref num13);
		public static void NumDust(int i, int j, int type, bool fail, ref int numDust)
		{
			ModTile modTile = GetTile(type);
			if (modTile != null)
			{
				modTile.NumDust(i, j, fail, ref numDust);
			}
			foreach (GlobalTile globalTile in globalTiles)
			{
				globalTile.NumDust(i, j, type, fail, ref numDust);
			}
		}
		//in Terraria.WorldGen.KillTile replace if (num15 >= 0) with
		//  if(TileLoader.CreateDust(i, j, tile.type, ref num15) && num15 >= 0)
		public static bool CreateDust(int i, int j, int type, ref int dustType)
		{
			foreach (GlobalTile globalTile in globalTiles)
			{
				if (!globalTile.CreateDust(i, j, type, ref dustType))
				{
					return false;
				}
			}
			ModTile modTile = GetTile(type);
			if (modTile != null)
			{
				return modTile.CreateDust(i, j, ref dustType);
			}
			return true;
		}
		//in Terraria.WorldGen.KillTile before if statement checking num43 call
		//  TileLoader.DropCritterChance(i, j, tile.type, ref num43, ref num44, ref num45);
		public static void DropCritterChance(int i, int j, int type, ref int wormChance, ref int grassHopperChance, ref int jungleGrubChance)
		{
			ModTile modTile = GetTile(type);
			if (modTile != null)
			{
				modTile.DropCritterChance(i, j, ref wormChance, ref grassHopperChance, ref jungleGrubChance);
			}
			foreach (GlobalTile globalTile in globalTiles)
			{
				globalTile.DropCritterChance(i, j, type, ref wormChance, ref grassHopperChance, ref jungleGrubChance);
			}
		}
		//in Terraria.WorldGen.KillTile before if statements checking num49 and num50
		//  add bool vanillaDrop = TileLoader.Drop(i, j, tile.type);
		//  add "vanillaDrop && " to beginning of these if statements
		public static bool Drop(int i, int j, int type)
		{
			foreach (GlobalTile globalTile in globalTiles)
			{
				if (!globalTile.Drop(i, j, type))
				{
					return false;
				}
			}
			ModTile modTile = GetTile(type);
			if (modTile != null)
			{
				if (!modTile.Drop(i, j))
				{
					return false;
				}
				if (modTile.drop > 0)
				{
					Item.NewItem(i * 16, j * 16, 16, 16, modTile.drop, 1, false, -1, false, false);
				}
				return false;
			}
			return true;
		}
		//in Terraria.WorldGen.CanKillTile after check for tile.active() add
		//  if(!TileLoader.CanKillTile(i, j, tile.type, ref blockDamaged)) { return false; }
		public static bool CanKillTile(int i, int j, int type, ref bool blockDamaged)
		{
			foreach (GlobalTile globalTile in globalTiles)
			{
				if (!globalTile.CanKillTile(i, j, type, ref blockDamaged))
				{
					return false;
				}
			}
			ModTile modTile = GetTile(type);
			if (modTile != null)
			{
				return modTile.CanKillTile(i, j, ref blockDamaged);
			}
			return true;
		}
		//in Terraria.WorldGen.KillTile before if (!effectOnly && !WorldGen.stopDrops) add
		//  TileLoader.KillTile(i, j, tile.type, ref fail, ref effectOnly, ref noItem);
		public static void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			ModTile modTile = GetTile(type);
			if (modTile != null)
			{
				modTile.KillTile(i, j, ref fail, ref effectOnly, ref noItem);
			}
			foreach (GlobalTile globalTile in globalTiles)
			{
				globalTile.KillTile(i, j, type, ref fail, ref effectOnly, ref noItem);
			}
		}

		public static void KillMultiTile(int i, int j, int frameX, int frameY, int type)
		{
			ModTile modTile = GetTile(type);
			if (modTile != null)
			{
				modTile.KillMultiTile(i, j, frameX, frameY);
			}
		}
		//in Terraria.Lighting.PreRenderPhase add local closer variable and after setting music box
		//  call TileLoader.NearbyEffects(n, num17, type, closer);
		public static void NearbyEffects(int i, int j, int type, bool closer)
		{
			ModTile modTile = GetTile(type);
			if (modTile != null)
			{
				modTile.NearbyEffects(i, j, closer);
			}
			foreach (GlobalTile globalTile in globalTiles)
			{
				globalTile.NearbyEffects(i, j, type, closer);
			}
		}
		//in Terraria.Lighting.PreRenderPhase after label after if statement checking Main.tileLighted call
		//  TileLoader.ModifyLight(n, num17, tile.type, ref num18, ref num19, ref num20);
		public static void ModifyLight(int i, int j, int type, ref float r, ref float g, ref float b)
		{
			if (!Main.tileLighted[type])
			{
				return;
			}
			ModTile modTile = GetTile(type);
			if (modTile != null)
			{
				modTile.ModifyLight(i, j, ref r, ref g, ref b);
			}
			foreach (GlobalTile globalTile in globalTiles)
			{
				globalTile.ModifyLight(i, j, type, ref r, ref g, ref b);
			}
		}

		public static bool Dangersense(int i, int j, int type, Player player)
		{
			ModTile modTile = GetTile(type);
			if (modTile != null && modTile.Dangersense(i, j, player))
			{
				return true;
			}
			foreach (GlobalTile globalTile in globalTiles)
			{
				if (globalTile.Dangersense(i, j, type, player))
				{
					return true;
				}
			}
			return false;
		}
		//in Terraria.Main.DrawTiles after if statement setting effects call
		//  TileLoader.SetSpriteEffects(j, i, type, ref effects);
		public static void SetSpriteEffects(int i, int j, int type, ref SpriteEffects spriteEffects)
		{
			ModTile modTile = GetTile(type);
			if (modTile != null)
			{
				modTile.SetSpriteEffects(i, j, ref spriteEffects);
			}
			foreach (GlobalTile globalTile in globalTiles)
			{
				globalTile.SetSpriteEffects(i, j, type, ref spriteEffects);
			}
		}
		//in Terraria.Main.DrawTiles after if statements setting num11 and num12 call
		//  TileLoader.SetDrawPositions(j, i, ref num9, ref num11, ref num12);
		public static void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height)
		{
			Tile tile = Main.tile[i, j];
			if (tile.type >= TileID.Count)
			{
				TileObjectData tileData = TileObjectData.GetTileData(tile.type, 0, 0);
				if (tileData != null)
				{
					int partFrameY = tile.frameY % tileData.CoordinateFullHeight;
					int partY = 0;
					while (partFrameY > 0)
					{
						partFrameY -= tileData.CoordinateHeights[partY] + tileData.CoordinatePadding;
						partY++;
					}
					width = tileData.CoordinateWidth;
					offsetY = tileData.DrawYOffset;
					height = tileData.CoordinateHeights[partY];
				}
				GetTile(tile.type).SetDrawPositions(i, j, ref width, ref offsetY, ref height);
			}
		}
		//in Terraria.Main.Update after vanilla tile animations call TileLoader.AnimateTiles();
		public static void AnimateTiles()
		{
			if (loaded)
			{
				foreach (ModTile modTile in tiles.Values)
				{
					modTile.AnimateTile(ref Main.tileFrame[modTile.Type], ref Main.tileFrameCounter[modTile.Type]);
				}
				foreach (GlobalTile globalTile in globalTiles)
				{
					globalTile.AnimateTile();
				}
			}
		}
		//in Terraria.Main.Draw after small if statements setting num15 call
		//  TileLoader.SetAnimationFrame(type, ref num15);
		public static void SetAnimationFrame(int type, ref int frameY)
		{
			ModTile modTile = GetTile(type);
			if (modTile != null)
			{
				frameY = modTile.animationFrameHeight * Main.tileFrame[type];
			}
		}
		//in Terraria.Main.Draw after calling SetAnimationFrame call
		//  if(!TileLoader.PreDraw(j, i, type, Main.spriteBatch))
		//  { TileLoader.PostDraw(j, i, type, Main.spriteBatch); continue; }
		public static bool PreDraw(int i, int j, int type, SpriteBatch spriteBatch)
		{
			foreach (GlobalTile globalTile in globalTiles)
			{
				if (!globalTile.PreDraw(i, j, type, spriteBatch))
				{
					return false;
				}
			}
			ModTile modTile = GetTile(type);
			if (modTile != null && !modTile.PreDraw(i, j, spriteBatch))
			{
				return false;
			}
			return true;
		}
		//in Terraria.Main.Draw after if statement checking whether texture2D is null call
		//  TileLoader.PostDraw(j, i, type, Main.spriteBatch);
		public static void PostDraw(int i, int j, int type, SpriteBatch spriteBatch)
		{
			ModTile modTile = GetTile(type);
			if (modTile != null)
			{
				modTile.PostDraw(i, j, spriteBatch);
			}
			foreach (GlobalTile globalTile in globalTiles)
			{
				globalTile.PostDraw(i, j, type, spriteBatch);
			}
		}
		//in Terraria.WorldGen.UpdateWorld in the while loops updating certain numbers of tiles at end of null check if statements
		//  add TileLoader.RandomUpdate(num7, num8, Main.tile[num7, num8].type; for the first loop
		//  add TileLoader.RandomUpdate(num64, num65, Main.tile[num64, num65].type; for the second loop
		public static void RandomUpdate(int i, int j, int type)
		{
			if (!Main.tile[i, j].active())
			{
				return;
			}
			ModTile modTile = GetTile(type);
			if (modTile != null)
			{
				modTile.RandomUpdate(i, j);
			}
			foreach (GlobalTile globalTile in globalTiles)
			{
				globalTile.RandomUpdate(i, j, type);
			}
		}
		//in Terraria.WorldGen.TileFrame at beginning of block of if(tile.active()) add
		//  if(!TileLoader.TileFrame(i, j, tile.type, ref resetFrame, ref noBreak)) { return; }
		public static bool TileFrame(int i, int j, int type, ref bool resetFrame, ref bool noBreak)
		{
			ModTile modTile = GetTile(type);
			bool flag = true;
			if (modTile != null)
			{
				flag = modTile.TileFrame(i, j, ref resetFrame, ref noBreak);
			}
			foreach (GlobalTile globalTile in globalTiles)
			{
				flag &= globalTile.TileFrame(i, j, type, ref resetFrame, ref noBreak);
			}
			return flag;
		}
		//in Terraria.Player.ItemCheck in if statements for mining
		//  replace num222 += item.hammer; with TileLoader.MineDamage(item.hammer, ref num222);
		//  replace num222 += item.axe; with TileLoader.MineDamage(item.axe, ref num222);
		//in Terraria.Player.PickTile replace num += pickPower; with TileLoader.MineDamage(pickPower, ref num);
		public static void MineDamage(int minePower, ref int damage)
		{
			Tile target = Main.tile[Player.tileTargetX, Player.tileTargetY];
			ModTile modTile = GetTile(target.type);
			if (modTile != null)
			{
				damage = (int)(damage + minePower / modTile.mineResist);
			}
			else
			{
				damage += minePower;
			}
		}
		//in Terraria.Player.ItemCheck at end of else if chain setting num to 0 add
		//  else { TileLoader.PickPowerCheck(tile, pickPower, ref num); }
		public static void PickPowerCheck(Tile target, int pickPower, ref int damage)
		{
			ModTile modTile = GetTile(target.type);
			if (modTile != null && pickPower < modTile.minPick)
			{
				damage = 0;
			}
		}
		//in Terraria.Player.PlaceThing after tileObject is initalized add else to if statement and before add
		//  if(!TileLoader.CanPlace(Player.tileTargetX, Player.tileTargetY)) { }
		public static bool CanPlace(int i, int j)
		{
			int type = Main.tile[i, j].type;
			foreach (GlobalTile globalTile in globalTiles)
			{
				if (!globalTile.CanPlace(i, j, type))
				{
					return false;
				}
			}
			ModTile modTile = GetTile(type);
			if (modTile != null && !modTile.CanPlace(i, j))
			{
				return false;
			}
			return true;
		}
		//in Terraria.Player.AdjTiles in end of if statement checking for tile's active
		//  add TileLoader.AdjTiles(this, Main.tile[j, k].type);
		public static void AdjTiles(Player player, int type)
		{
			ModTile modTile = GetTile(type);
			if (modTile != null)
			{
				foreach (int k in modTile.adjTiles)
				{
					player.adjTile[k] = true;
				}
			}
			foreach (GlobalTile globalTile in globalTiles)
			{
				int[] adjTiles = globalTile.AdjTiles(type);
				foreach (int k in adjTiles)
				{
					player.adjTile[k] = true;
				}
			}
		}
		//in Terraria.Player.Update in if statements involving controluseTile and releaseUseTile
		//  at end of type-check if else chain add TileLoader.RightClick(Player.tileTargetX, Player.tileTargetY);
		public static void RightClick(int i, int j)
		{
			int type = Main.tile[i, j].type;
			ModTile modTile = GetTile(type);
			if (modTile != null)
			{
				modTile.RightClick(i, j);
			}
			foreach (GlobalTile globalTile in globalTiles)
			{
				globalTile.RightClick(i, j, type);
			}
		}
		//in Terraria.Player.Update after if statements setting showItemIcon call
		//  TileLoader.MouseOver(Player.tileTargetX, Player.tileTargetY);
		public static void MouseOver(int i, int j)
		{
			int type = Main.tile[i, j].type;
			ModTile modTile = GetTile(type);
			if (modTile != null)
			{
				modTile.MouseOver(i, j);
			}
			foreach (GlobalTile globalTile in globalTiles)
			{
				globalTile.MouseOver(i, j, type);
			}
		}

		public static void MouseOverFar(int i, int j)
		{
			int type = Main.tile[i, j].type;
			ModTile modTile = GetTile(type);
			if (modTile != null)
			{
				modTile.MouseOverFar(i, j);
			}
			foreach (GlobalTile globalTile in globalTiles)
			{
				globalTile.MouseOverFar(i, j, type);
			}
		}

		public static int AutoSelect(int i, int j, Player player)
		{
			if (!Main.tile[i, j].active())
			{
				return -1;
			}
			int type = Main.tile[i, j].type;
			ModTile modTile = GetTile(type);
			for (int k = 0; k < 50; k++)
			{
				Item item = player.inventory[k];
				if (item.type == 0 || item.stack == 0)
				{
					continue;
				}
				if (modTile != null && modTile.AutoSelect(i, j, item))
				{
					return k;
				}
				foreach (GlobalTile globalTile in globalTiles)
				{
					if (globalTile.AutoSelect(i, j, type, item))
					{
						return k;
					}
				}
			}
			return -1;
		}
		//in Terraria.Wiring make the following public:
		//  _wireList, _toProcess, _teleport, _inPumpX, _inPumpY, _numInPump, _outPumpX, _outPumpY, _numOutPump CheckMech, TripWire
		//at end of Terraria.Wiring.HitWireSingle inside if statement checking for tile active add
		//  TileLoader.HitWire(i, j, type);
		public static void HitWire(int i, int j, int type)
		{
			ModTile modTile = GetTile(type);
			if (modTile != null)
			{
				modTile.HitWire(i, j);
			}
			foreach (GlobalTile globalTile in globalTiles)
			{
				globalTile.HitWire(i, j, type);
			}
		}
		//in Terraria.Player.ItemCheck in poundRelease if statement before sloping if statements add
		//  if(TileLoader.Slope(num223, num224, Main.tile[num223, num224].type)) { } else
		public static bool Slope(int i, int j, int type)
		{
			foreach (GlobalTile globalTile in globalTiles)
			{
				if (!globalTile.Slope(i, j, type))
				{
					return true;
				}
			}
			ModTile modTile = GetTile(type);
			if (modTile != null)
			{
				return !modTile.Slope(i, j);
			}
			return false;
		}

		public static bool HasWalkDust(int type)
		{
			ModTile modTile = GetTile(type);
			if (modTile != null)
			{
				return modTile.HasWalkDust();
			}
			return false;
		}

		public static void WalkDust(int type, ref int dustType, ref bool makeDust, ref Color color)
		{
			ModTile modTile = GetTile(type);
			if (modTile != null)
			{
				modTile.WalkDust(ref dustType, ref makeDust, ref color);
			}
		}
	}
}
