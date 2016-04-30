using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public static class MountLoader
	{
		private static int nextMount = MountID.Count;
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
			if (ModNet.AllowVanillaClients) throw new Exception("Adding mounts breaks vanilla client compatiblity");

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

		internal static void UpdateEffects(Player mountedPlayer)
		{
			if (IsModMountData(Mount.mounts[mountedPlayer.mount.Type]))
			{
				GetMount(mountedPlayer.mount.Type).UpdateEffects(mountedPlayer);
			}
		}

		internal static bool UpdateFrame(Player mountedPlayer, int state, Vector2 velocity)
		{
			if (IsModMountData(Mount.mounts[mountedPlayer.mount.Type]))
			{
				return GetMount(mountedPlayer.mount.Type).UpdateFrame(mountedPlayer, state, velocity);
			}
			return true;
		}

		internal static bool CustomBodyFrame(Mount.MountData mount)
		{
			if (IsModMountData(mount) && mount.modMountData.CustomBodyFrame())
			{
				return true;
			}
			return false;
		}
	}
}
