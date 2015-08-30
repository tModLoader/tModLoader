using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public static class MountLoader
	{
		private static int nextMount = 14;
		internal static readonly IDictionary<int, ModMountData> mountDatas = new Dictionary<int, ModMountData>();

		public static ModMountData GetMount(int type)
		{
			if (mountDatas.ContainsKey(type))
			{
				return mountDatas[type];
			}
			return null;
		}

		internal static int ReserveMountID()
		{
			ErrorLogger.Log("reserve " + Mount.mounts.Length);
			int reserveID = nextMount;
			nextMount++;
			return reserveID;
		}

		internal static void ResizeArrays()
		{
			Array.Resize(ref MountID.Sets.Cart, nextMount);
			Array.Resize(ref Mount.mounts, nextMount);
		}

		internal static void Unload()
		{
			mountDatas.Clear();
			nextMount = MountID.Count;
		}

		internal static bool IsModMountData(Mount.MountData mountData)
		{
			return mountData.modMountData != null;
		}

		internal static void SetupMount(Mount.MountData mount)
		{
			if (IsModMountData(mount))
			{
				GetMount(mount.modMountData.Type).SetupMount(mount);
			}
		}
	}
}
