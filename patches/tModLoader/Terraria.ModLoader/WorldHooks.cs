using System;
using System.IO;
using System.Collections.Generic;
using Terraria.World.Generation;

namespace Terraria.ModLoader
{
	//todo: further documentation
	/// <summary>
	/// This is where all ModWorld hooks are gathered and called.
	/// </summary>
	public static class WorldHooks
	{
		internal static readonly IList<ModWorld> worlds = new List<ModWorld>();
		internal static ModWorld[] NetWorlds;

		internal static void Add(ModWorld modWorld)
		{
			worlds.Add(modWorld);
		}

		internal static void ResizeArrays()
		{
			NetWorlds = ModLoader.BuildGlobalHook<ModWorld, Action<BinaryWriter>>(worlds, w => w.NetSend);
		}

		internal static void Unload()
		{
			worlds.Clear();
		}

		internal static void SetupWorld()
		{
			foreach (ModWorld world in worlds)
			{
				world.Initialize();
			}
		}

		internal static void WriteNetWorldOrder(BinaryWriter w)
		{
			w.Write((short)NetWorlds.Length);
			foreach (var netWorld in NetWorlds)
			{
				w.Write(netWorld.mod.netID);
				w.Write(netWorld.Name);
			}
		}

		internal static void ReadNetWorldOrder(BinaryReader r)
		{
			short n = r.ReadInt16();
			NetWorlds = new ModWorld[n];
			for (short i = 0; i < n; i++)
				NetWorlds[i] = ModNet.GetMod(r.ReadInt16()).GetModWorld(r.ReadString());
		}

		public static void PreWorldGen()
		{
			foreach (ModWorld modWorld in worlds)
			{
				modWorld.PreWorldGen();
			}
		}

		public static void ModifyWorldGenTasks(List<GenPass> passes, ref float totalWeight)
		{
			foreach (ModWorld modWorld in worlds)
			{
				modWorld.ModifyWorldGenTasks(passes, ref totalWeight);
			}
		}

		public static void PostWorldGen()
		{
			foreach (ModWorld modWorld in worlds)
			{
				modWorld.PostWorldGen();
			}
		}

		public static void ResetNearbyTileEffects()
		{
			foreach (ModWorld modWorld in worlds)
			{
				modWorld.ResetNearbyTileEffects();
			}
		}

		public static void PreUpdateEntities()
		{
			foreach (ModWorld modWorld in worlds)
			{
				modWorld.PreUpdateEntities();
			}
		}

		public static void MidUpdatePlayerNPC()
		{
			foreach (ModWorld modWorld in worlds)
			{
				modWorld.MidUpdatePlayerNPC();
			}
		}

		public static void MidUpdateNPCGore()
		{
			foreach (ModWorld modWorld in worlds)
			{
				modWorld.MidUpdateNPCGore();
			}
		}

		public static void MidUpdateGoreProjectile()
		{
			foreach (ModWorld modWorld in worlds)
			{
				modWorld.MidUpdateGoreProjectile();
			}
		}

		public static void MidUpdateProjectileItem()
		{
			foreach (ModWorld modWorld in worlds)
			{
				modWorld.MidUpdateProjectileItem();
			}
		}

		public static void MidUpdateItemDust()
		{
			foreach (ModWorld modWorld in worlds)
			{
				modWorld.MidUpdateItemDust();
			}
		}

		public static void MidUpdateDustTime()
		{
			foreach (ModWorld modWorld in worlds)
			{
				modWorld.MidUpdateDustTime();
			}
		}

		public static void MidUpdateTimeWorld()
		{
			foreach (ModWorld modWorld in worlds)
			{
				modWorld.MidUpdateTimeWorld();
			}
		}

		public static void PreUpdate()
		{
			foreach (ModWorld modWorld in worlds)
			{
				modWorld.PreUpdate();
			}
		}

		public static void PostUpdate()
		{
			foreach (ModWorld modWorld in worlds)
			{
				modWorld.PostUpdate();
			}
		}

		public static void MidUpdateInvasionNet()
		{
			foreach (ModWorld modWorld in worlds)
			{
				modWorld.MidUpdateInvasionNet();
			}
		}

		public static void PostUpdateEverything()
		{
			foreach (ModWorld modWorld in worlds)
			{
				modWorld.PostUpdateEverything();
			}
		}

		public static void TileCountsAvailable(int[] tileCounts)
		{
			foreach (ModWorld modWorld in worlds)
			{
				modWorld.TileCountsAvailable(tileCounts);
			}
		}

		public static void ChooseWaterStyle(ref int style)
		{
			foreach (ModWorld modWorld in worlds)
			{
				modWorld.ChooseWaterStyle(ref style);
			}
		}

		public static void ModifyHardmodeTasks(List<GenPass> passes)
		{
			foreach (ModWorld modWorld in worlds)
			{
				modWorld.ModifyHardmodeTasks(passes);
			}
		}

		public static void PostDrawTiles()
		{
			foreach (ModWorld modWorld in worlds)
			{
				modWorld.PostDrawTiles();
			}
		}
	}
}