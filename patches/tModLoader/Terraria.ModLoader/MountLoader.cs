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

	public static void UpdateEffects(Player mountedPlayer)
	{
	    if (IsModMountData(Mount.mounts[mountedPlayer.mount.Type]))
	    {
		GetMount(mountedPlayer.mount.Type).UpdateEffects(mountedPlayer);
	    }
	}

	public static bool UpdateFrame(Player mountedPlayer, int state, Vector2 velocity)
	{
	    if (IsModMountData(Mount.mounts[mountedPlayer.mount.Type]))
	    {
		return GetMount(mountedPlayer.mount.Type).UpdateFrame(mountedPlayer, state, velocity);
	    }
	    return true;
	}

	public static bool CustomBodyFrame(Mount.MountData mount)
	{
	    if (IsModMountData(mount) && mount.modMountData.CustomBodyFrame())
	    {
		return true;
	    }
	    return false;
	}
	public static bool UseAbility(Player mountedPlayer, bool toggleOn)
	{
	    float num = Main.screenPosition.X + (float)Main.mouseX;
	    float num2 = Main.screenPosition.Y + (float)Main.mouseY;
	    Vector2 mousePosition = new Vector2(num, num2);
	    if (IsModMountData(Mount.mounts[mountedPlayer.mount.Type]))
	    {
		if (GetMount(mountedPlayer.mount.Type).CanUseAbility(mountedPlayer, mousePosition, toggleOn))
		{
		    GetMount(mountedPlayer.mount.Type).UseAbility(mountedPlayer, mousePosition, toggleOn);
		    return true;
		}

	    }
	    return false;
	}
    }
}
