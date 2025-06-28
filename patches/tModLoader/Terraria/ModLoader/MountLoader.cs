using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.Core;

namespace Terraria.ModLoader;

/// <summary>
/// This serves as the central place from which mounts are stored and mount-related functions are carried out.
/// </summary>
public static class MountLoader
{
	internal static readonly IDictionary<int, ModMount> mountDatas = new Dictionary<int, ModMount>();

	public static int MountCount { get; private set; } = MountID.Count;

	/// <summary>
	/// Gets the ModMount instance corresponding to the given type. Returns null if no ModMount has the given type.
	/// </summary>
	/// <param name="type">The type of the mount.</param>
	/// <returns>Null if not found, otherwise the ModMount associated with the mount.</returns>
	public static ModMount GetMount(int type)
	{
		if (mountDatas.ContainsKey(type)) {
			return mountDatas[type];
		}
		return null;
	}

	internal static int ReserveMountID()
	{
		return MountCount++;
	}

	internal static void ResizeArrays()
	{
		//Sets
		LoaderUtils.ResetStaticMembers(typeof(MountID));

		//Etc
		Array.Resize(ref Mount.mounts, MountCount);
	}

	internal static void Unload()
	{
		mountDatas.Clear();
		MountCount = MountID.Count;
	}

	internal static bool IsModMount(Mount.MountData mountData)
	{
		return mountData.ModMount != null;
	}

	public static void JumpHeight(Player mountedPlayer, Mount.MountData mount, ref int jumpHeight, float xVelocity)
	{
		if (IsModMount(mount)) {
			mount.ModMount.JumpHeight(mountedPlayer, ref jumpHeight, xVelocity);
		}
	}

	public static void JumpSpeed(Player mountedPlayer, Mount.MountData mount, ref float jumpSpeed, float xVelocity)
	{
		if (IsModMount(mount)) {
			mount.ModMount.JumpSpeed(mountedPlayer, ref jumpSpeed, xVelocity);
		}
	}

	internal static void UpdateEffects(Player mountedPlayer)
	{
		if (IsModMount(Mount.mounts[mountedPlayer.mount.Type])) {
			GetMount(mountedPlayer.mount.Type).UpdateEffects(mountedPlayer);
		}
	}

	internal static bool UpdateFrame(Player mountedPlayer, int state, Vector2 velocity)
	{
		if (IsModMount(Mount.mounts[mountedPlayer.mount.Type])) {
			return GetMount(mountedPlayer.mount.Type).UpdateFrame(mountedPlayer, state, velocity);
		}
		return true;
	}

	//todo: this is never called, why is this in here?
	internal static bool CustomBodyFrame(Mount.MountData mount)
	{
		if (IsModMount(mount) && mount.ModMount.CustomBodyFrame()) {
			return true;
		}
		return false;
	}
	/// <summary>
	/// Allows you to make things happen while the mouse is pressed while the mount is active. Called each tick the mouse is pressed.
	/// </summary>
	/// <param name="player"></param>
	/// <param name="mousePosition"></param>
	/// <param name="toggleOn">Does nothing yet</param>
	public static void UseAbility(Player player, Vector2 mousePosition, bool toggleOn)
	{
		if (IsModMount(player.mount._data)) {
			player.mount._data.ModMount.UseAbility(player, mousePosition, toggleOn);
		}
	}
	/// <summary>
	/// Allows you to make things happen when the mount ability is aiming (while charging).
	/// </summary>
	/// <param name="mount"></param>
	/// <param name="player"></param>
	/// <param name="mousePosition"></param>
	public static void AimAbility(Mount mount, Player player, Vector2 mousePosition)
	{
		if (IsModMount(mount._data)) {
			mount._data.ModMount.AimAbility(player, mousePosition);
		}
	}

	/// <summary>
	/// Allows you to make things happen when this mount is spawned in. Useful for player-specific initialization, utilizing player.mount._mountSpecificData or a ModPlayer class since ModMount is shared between all players.
	/// Custom dust spawning logic is also possible via the skipDust parameter.
	/// </summary>
	/// <param name="mount"></param>
	/// <param name="player"></param>
	/// <param name="skipDust">Set to true to skip the vanilla dust spawning logic</param>
	public static void SetMount(Mount mount, Player player, ref bool skipDust)
	{
		if (IsModMount(mount._data)) {
			mount._data.ModMount.SetMount(player, ref skipDust);
		}
	}

	/// <summary>
	/// Allows you to make things happen when this mount is de-spawned. Useful for player-specific cleanup, see SetMount.
	/// Custom dust spawning logic is also possible via the skipDust parameter.
	/// </summary>
	/// <param name="mount"></param>
	/// <param name="player"></param>
	/// <param name="skipDust">Set to true to skip the vanilla dust spawning logic</param>
	public static void Dismount(Mount mount, Player player, ref bool skipDust)
	{
		if (IsModMount(mount._data)) {
			mount._data.ModMount.Dismount(player, ref skipDust);
		}
	}

	/// <summary>
	/// See <see cref="ModMount.Draw(List{DrawData}, int, Player, ref Texture2D, ref Texture2D, ref Vector2, ref Rectangle, ref Color, ref Color, ref float, ref SpriteEffects, ref Vector2, ref float, float)"/>
	/// </summary>
	public static bool Draw(Mount mount, List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow)
	{
		if (IsModMount(mount._data)) {
			return mount._data.ModMount.Draw(playerDrawData, drawType, drawPlayer, ref texture, ref glowTexture, ref drawPosition, ref frame, ref drawColor, ref glowColor, ref rotation, ref spriteEffects, ref drawOrigin, ref drawScale, shadow);
		}
		return true;
	}
}
