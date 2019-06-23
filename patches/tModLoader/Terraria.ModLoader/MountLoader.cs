using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria.ID;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This serves as the central place from which mounts are stored and mount-related functions are carried out.
	/// </summary>
	public static class MountLoader
	{
		private static int nextMount = MountID.Count;
		internal static readonly IDictionary<int, ModMountData> mountDatas = new Dictionary<int, ModMountData>();

		/// <summary>
		/// Gets the ModMountData instance corresponding to the given type. Returns null if no ModMountData has the given type.
		/// </summary>
		/// <param name="type">The type of the mount.</param>
		/// <returns>Null if not found, otherwise the ModMountData associated with the mount.</returns>
		public static ModMountData GetMount(int type) {
			if (mountDatas.ContainsKey(type)) {
				return mountDatas[type];
			}
			return null;
		}

		internal static int ReserveMountID() {
			if (ModNet.AllowVanillaClients) throw new Exception("Adding mounts breaks vanilla client compatibility");

			int reserveID = nextMount;
			nextMount++;
			return reserveID;
		}

		internal static void ResizeArrays() {
			Array.Resize(ref MountID.Sets.Cart, nextMount);
			Array.Resize(ref Mount.mounts, nextMount);
		}

		internal static void Unload() {
			mountDatas.Clear();
			nextMount = MountID.Count;
		}

		internal static bool IsModMountData(Mount.MountData mountData) {
			return mountData.modMountData != null;
		}

		internal static void SetupMount(Mount.MountData mount) {
			if (IsModMountData(mount)) {
				GetMount(mount.modMountData.Type).SetupMount(mount);
			}
		}

		public static void JumpHeight(Mount.MountData mount, ref int jumpHeight, float xVelocity) {
			if (IsModMountData(mount)) {
				mount.modMountData.JumpHeight(ref jumpHeight, xVelocity);
			}
		}

		public static void JumpSpeed(Mount.MountData mount, ref float jumpSpeed, float xVelocity) {
			if (IsModMountData(mount)) {
				mount.modMountData.JumpSpeed(ref jumpSpeed, xVelocity);
			}
		}

		internal static void UpdateEffects(Player mountedPlayer) {
			if (IsModMountData(Mount.mounts[mountedPlayer.mount.Type])) {
				GetMount(mountedPlayer.mount.Type).UpdateEffects(mountedPlayer);
			}
		}

		internal static bool UpdateFrame(Player mountedPlayer, int state, Vector2 velocity) {
			if (IsModMountData(Mount.mounts[mountedPlayer.mount.Type])) {
				return GetMount(mountedPlayer.mount.Type).UpdateFrame(mountedPlayer, state, velocity);
			}
			return true;
		}

		//todo: this is never called, why is this in here?
		internal static bool CustomBodyFrame(Mount.MountData mount) {
			if (IsModMountData(mount) && mount.modMountData.CustomBodyFrame()) {
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
		public static void UseAbility(Player player, Vector2 mousePosition, bool toggleOn) {
			if (IsModMountData(player.mount._data)) {
				player.mount._data.modMountData.UseAbility(player, mousePosition, toggleOn);
			}
		}
		/// <summary>
		/// Allows you to make things happen when the mount ability is aiming (while charging).
		/// </summary>
		/// <param name="mount"></param>
		/// <param name="player"></param>
		/// <param name="mousePosition"></param>
		public static void AimAbility(Mount mount, Player player, Vector2 mousePosition) {
			if (IsModMountData(mount._data)) {
				mount._data.modMountData.AimAbility(player, mousePosition);
			}
		}
	}
}
