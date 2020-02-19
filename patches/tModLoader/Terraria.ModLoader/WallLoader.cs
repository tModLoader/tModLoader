using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria.ID;

namespace Terraria.ModLoader
{
	//todo: further documentation
	/// <summary>
	/// This serves as the central class from which wall-related functions are supported and carried out.
	/// </summary>
	public static class WallLoader
	{
		private static int nextWall = WallID.Count;
		internal static readonly IList<ModWall> walls = new List<ModWall>();
		internal static readonly IList<GlobalWall> globalWalls = new List<GlobalWall>();
		private static bool loaded = false;

		private static Func<int, int, int, bool>[] HookKillSound;
		private delegate void DelegateNumDust(int i, int j, int type, bool fail, ref int num);
		private static DelegateNumDust[] HookNumDust;
		private delegate bool DelegateCreateDust(int i, int j, int type, ref int dustType);
		private static DelegateCreateDust[] HookCreateDust;
		private delegate bool DelegateDrop(int i, int j, int type, ref int dropType);
		private static DelegateDrop[] HookDrop;
		private delegate void DelegateKillWall(int i, int j, int type, ref bool fail);
		private static DelegateKillWall[] HookKillWall;
		private static Func<int, int, int, bool>[] HookCanExplode;
		private delegate void DelegateModifyLight(int i, int j, int type, ref float r, ref float g, ref float b);
		private static DelegateModifyLight[] HookModifyLight;
		private static Action<int, int, int>[] HookRandomUpdate;
		private static Func<int, int, int, SpriteBatch, bool>[] HookPreDraw;
		private static Action<int, int, int, SpriteBatch>[] HookPostDraw;
		private static Action<int, int, int, Item>[] HookPlaceInWorld;

		internal static int ReserveWallID() {
			if (ModNet.AllowVanillaClients) throw new Exception("Adding walls breaks vanilla client compatibility");

			int reserveID = nextWall;
			nextWall++;
			return reserveID;
		}

		public static int WallCount => nextWall;

		/// <summary>
		/// Gets the ModWall instance with the given type. If no ModWall with the given type exists, returns null.
		/// </summary>
		public static ModWall GetWall(int type) {
			return type >= WallID.Count && type < WallCount ? walls[type - WallID.Count] : null;
		}

		private static void Resize2DArray<T>(ref T[,] array, int newSize) {
			int dim1 = array.GetLength(0);
			int dim2 = array.GetLength(1);
			T[,] newArray = new T[newSize, dim2];
			for (int j = 0; j < newSize && j < dim1; j++) {
				for (int k = 0; k < dim2; k++) {
					newArray[j, k] = array[j, k];
				}
			}
			array = newArray;
		}

		internal static void ResizeArrays(bool unloading = false) {
			Array.Resize(ref Main.wallLoaded, nextWall);
			for (int k = WallID.Count; k < nextWall; k++) {
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
			Array.Resize(ref WallID.Sets.Conversion.Grass, nextWall);
			Array.Resize(ref WallID.Sets.Conversion.Stone, nextWall);
			Array.Resize(ref WallID.Sets.Conversion.Sandstone, nextWall);
			Array.Resize(ref WallID.Sets.Conversion.HardenedSand, nextWall);
			Array.Resize(ref WallID.Sets.Transparent, nextWall);
			Array.Resize(ref WallID.Sets.Corrupt, nextWall);
			Array.Resize(ref WallID.Sets.Crimson, nextWall);
			Array.Resize(ref WallID.Sets.Hallow, nextWall);

			ModLoader.BuildGlobalHook(ref HookKillSound, globalWalls, g => g.KillSound);
			ModLoader.BuildGlobalHook(ref HookNumDust, globalWalls, g => g.NumDust);
			ModLoader.BuildGlobalHook(ref HookCreateDust, globalWalls, g => g.CreateDust);
			ModLoader.BuildGlobalHook(ref HookDrop, globalWalls, g => g.Drop);
			ModLoader.BuildGlobalHook(ref HookKillWall, globalWalls, g => g.KillWall);
			ModLoader.BuildGlobalHook(ref HookCanExplode, globalWalls, g => g.CanExplode);
			ModLoader.BuildGlobalHook(ref HookModifyLight, globalWalls, g => g.ModifyLight);
			ModLoader.BuildGlobalHook(ref HookRandomUpdate, globalWalls, g => g.RandomUpdate);
			ModLoader.BuildGlobalHook(ref HookPreDraw, globalWalls, g => g.PreDraw);
			ModLoader.BuildGlobalHook(ref HookPostDraw, globalWalls, g => g.PostDraw);
			ModLoader.BuildGlobalHook(ref HookPlaceInWorld, globalWalls, g => g.PlaceInWorld);

			if (!unloading) {
				loaded = true;
			}
		}

		internal static void Unload() {
			loaded = false;
			walls.Clear();
			nextWall = WallID.Count;
			globalWalls.Clear();
		}

		//change type of Terraria.Tile.wall to ushort and fix associated compile errors
		//in Terraria.IO.WorldFile.SaveWorldTiles increase length of array by 1 from 13 to 14
		//in Terraria.IO.WorldFile.SaveWorldTiles inside block if (tile.wall != 0) after incrementing num2
		//  call WallLoader.WriteType(tile.wall, array, ref num2, ref b3);
		internal static void WriteType(ushort wall, byte[] data, ref int index, ref byte flags) {
			if (wall > 255) {
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
		internal static void ReadType(ref ushort wall, BinaryReader reader, byte flags, IDictionary<int, int> wallTable) {
			if ((flags & 32) == 32) {
				wall |= (ushort)(reader.ReadByte() << 8);
			}
			if (wallTable.ContainsKey(wall)) {
				wall = (ushort)wallTable[wall];
			}
		}
		//in Terraria.WorldGen.KillWall add if(!WallLoader.KillSound(i, j, tile.wall)) { } to beginning of
		//  if/else chain for playing sounds, and turn first if into else if
		public static bool KillSound(int i, int j, int type) {
			foreach (var hook in HookKillSound) {
				if (!hook(i, j, type)) {
					return false;
				}
			}
			ModWall modWall = GetWall(type);
			if (modWall != null) {
				if (!modWall.KillSound(i, j)) {
					return false;
				}
				Main.PlaySound(modWall.soundType, i * 16, j * 16, modWall.soundStyle);
				return false;
			}
			return true;
		}
		//in Terraria.WorldGen.KillWall after if statement setting num to 3 add
		//  WallLoader.NumDust(i, j, tile.wall, fail, ref num);
		public static void NumDust(int i, int j, int type, bool fail, ref int numDust) {
			GetWall(type)?.NumDust(i, j, fail, ref numDust);

			foreach (var hook in HookNumDust) {
				hook(i, j, type, fail, ref numDust);
			}
		}
		//in Terraria.WorldGen.KillWall before if statements creating dust add
		//  if(!WallLoader.CreateDust(i, j, tile.wall, ref int num2)) { continue; }
		public static bool CreateDust(int i, int j, int type, ref int dustType) {
			foreach (var hook in HookCreateDust) {
				if (!hook(i, j, type, ref dustType)) {
					return false;
				}
			}
			return GetWall(type)?.CreateDust(i, j, ref dustType) ?? true;
		}
		//in Terraria.WorldGen.KillWall replace if (num4 > 0) with
		//  if (WallLoader.Drop(i, j, tile.wall, ref num4) && num4 > 0)
		public static bool Drop(int i, int j, int type, ref int dropType) {
			foreach (var hook in HookDrop) {
				if (!hook(i, j, type, ref dropType)) {
					return false;
				}
			}
			return GetWall(type)?.Drop(i, j, ref dropType) ?? true;
		}
		//in Terraria.WorldGen.KillWall after if statements setting fail to true call
		//  WallLoader.KillWall(i, j, tile.wall, ref fail);
		public static void KillWall(int i, int j, int type, ref bool fail) {
			GetWall(type)?.KillWall(i, j, ref fail);

			foreach (var hook in HookKillWall) {
				hook(i, j, type, ref fail);
			}
		}

		public static bool CanExplode(int i, int j, int type) {
			foreach (var hook in HookCanExplode) {
				if (!hook(i, j, type)) {
					return false;
				}
			}
			return GetWall(type)?.CanExplode(i, j) ?? true;
		}
		//in Terraria.Lighting.PreRenderPhase after wall modifies light call
		//  WallLoader.ModifyLight(n, num17, wall, ref num18, ref num19, ref num20);
		public static void ModifyLight(int i, int j, int type, ref float r, ref float g, ref float b) {
			GetWall(type)?.ModifyLight(i, j, ref r, ref g, ref b);

			foreach (var hook in HookModifyLight) {
				hook(i, j, type, ref r, ref g, ref b);
			}
		}
		//in Terraria.WorldGen.UpdateWorld after each call to TileLoader.RandomUpdate call
		//  WallLoader.RandomUpdate(num7, num8, Main.tile[num7, num8].wall);
		//  WallLoader.RandomUpdate(num64, num65, Main.tile[num64, num65].wall);
		public static void RandomUpdate(int i, int j, int type) {
			GetWall(type)?.RandomUpdate(i, j);

			foreach (var hook in HookRandomUpdate) {
				hook(i, j, type);
			}
		}
		//in Terraria.Main.Update after vanilla wall animations call WallLoader.AnimateWalls();
		public static void AnimateWalls() {
			if (loaded) {
				for (int i = 0; i < walls.Count; i++) {
					ModWall modWall = walls[i];
					modWall.AnimateWall(ref Main.wallFrame[modWall.Type], ref Main.wallFrameCounter[modWall.Type]);
				}
			}
		}
		//in Terraria.Main.DrawWalls before if statements that do the drawing add
		//  if(!WallLoader.PreDraw(j, i, wall, Main.spriteBatch))
		//  { WallLoader.PostDraw(j, i, wall, Main.spriteBatch); continue; }
		public static bool PreDraw(int i, int j, int type, SpriteBatch spriteBatch) {
			foreach (var hook in HookPreDraw) {
				if (!hook(i, j, type, spriteBatch)) {
					return false;
				}
			}
			return GetWall(type)?.PreDraw(i, j, spriteBatch) ?? true;
		}
		//in Terraria.Main.DrawWalls after wall outlines are drawn call
		//  WallLoader.PostDraw(j, i, wall, Main.spriteBatch);
		public static void PostDraw(int i, int j, int type, SpriteBatch spriteBatch) {
			GetWall(type)?.PostDraw(i, j, spriteBatch);

			foreach (var hook in HookPostDraw) {
				hook(i, j, type, spriteBatch);
			}
		}

		public static void PlaceInWorld(int i, int j, Item item) {
			int type = item.createWall;
			if (type < 0)
				return;

			foreach (var hook in HookPlaceInWorld) {
				hook(i, j, type, item);
			}

			GetWall(type)?.PlaceInWorld(i, j, item);
		}
	}
}
