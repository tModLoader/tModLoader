using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public static class MountLoader
	{
		//private static int nextMount = MountID.Count;
		internal static readonly IDictionary<int, ModMountData> mountDatas = new Dictionary<int, ModMountData>();

		////internal static readonly IDictionary<string, string> textures = new Dictionary<string, string>();
		//internal static int ReserveMountID()
		//{
		//    int result = nextMount;
		//    nextMount++;
		//    return result;
		//}
		//internal static int MountCount()
		//{
		//    return nextMount;
		//}
		public static ModMountData GetMount(int type)
		{
			if (mountDatas.ContainsKey(type))
			{
				return mountDatas[type];
			}
			return null;
		}

		internal static void ResizeArrays()
		{
			//Array.Resize(ref Main.itemTexture, nextItem);
			ErrorLogger.Log("Array.Resize( " + Mount.mounts.Length);
			//ErrorLogger.Log(MountID.Sets.Cart[0].ToString());
			//ErrorLogger.Log(Mount.mounts[0].ToString());
			Array.Resize(ref MountID.Sets.Cart, 30); // nextMount
			Array.Resize(ref Mount.mounts, 30);
			ErrorLogger.Log("Array.Resize( " + Mount.mounts.Length);
			//ErrorLogger.Log(MountID.Sets.Cart[0].ToString());
			//ErrorLogger.Log(Mount.mounts[0].ToString());
		}
		//internal static void Unload()
		//{
		//    mountDatas.Clear();
		//    nextMount = MountID.Count;
		//}
		internal static bool IsModMountData(Mount.MountData mountData)
		{
			return mountData.modMountData != null;
			//return Mount._type >= MountID.Count;
		}

		internal static void SetupMount(Mount.MountData mount)
		{
			if (IsModMountData(mount))
			{
				GetMount(mount.type).SetupMount(mount);
			}
		}
	}
}
