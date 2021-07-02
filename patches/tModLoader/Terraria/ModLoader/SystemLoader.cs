using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.Graphics;
using Terraria.Localization;
using Terraria.UI;
using Terraria.WorldBuilding;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This is where all <see cref="ModSystem"/> hooks are gathered and called.
	/// </summary>
	public static partial class SystemLoader
	{
		internal static readonly List<ModSystem> Systems = new List<ModSystem>();

		internal static ModSystem[] NetSystems { get; private set; }

		internal static void Add(ModSystem modSystem) => Systems.Add(modSystem);

		internal static void Unload() => Systems.Clear();

		internal static void ResizeArrays() {
			NetSystems = ModLoader.BuildGlobalHook<ModSystem, Action<BinaryWriter>>(Systems, s => s.NetSend);

			RebuildHooks();
		}

		internal static void WriteNetSystemOrder(BinaryWriter w) {
			w.Write((short)NetSystems.Length);

			foreach (var netWorld in NetSystems) {
				w.Write(netWorld.Mod.netID);
				w.Write(netWorld.Name);
			}
		}

		internal static void ReadNetSystemOrder(BinaryReader r) {
			short n = r.ReadInt16();

			NetSystems = new ModSystem[n];

			for (short i = 0; i < n; i++) {
				NetSystems[i] = ModContent.Find<ModSystem>(ModNet.GetMod(r.ReadInt16()).Name, r.ReadString());
			}
		}

		internal static void OnModLoad(Mod mod) {
			foreach (var system in Systems.Where(s => s.Mod == mod)) {
				system.OnModLoad();
			}
		}

		internal static void PostSetupContent(Mod mod) {
			foreach (var system in Systems.Where(s => s.Mod == mod)) {
				system.PostSetupContent();
			}
		}

		public static void OnWorldLoad() {
			foreach (var system in HookOnWorldLoad.arr) {
				system.OnWorldLoad();
			}
		}

		public static void OnWorldUnload() {
			foreach (var system in HookOnWorldUnload.arr) {
				system.OnWorldUnload();
			}
		}

		public static void ModifyScreenPosition() {
			foreach (var system in HookModifyScreenPosition.arr) {
				system.ModifyScreenPosition();
			}
		}

		public static void ModifyTransformMatrix(ref SpriteViewMatrix Transform) {
			foreach (var system in HookModifyTransformMatrix.arr) {
				system.ModifyTransformMatrix(ref Transform);
			}
		}

		public static void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor) {
			if (Main.gameMenu)
				return;

			foreach (var system in HookModifySunLightColor.arr) {
				system.ModifySunLightColor(ref tileColor, ref backgroundColor);
			}
		}

		public static void ModifyLightingBrightness(ref float negLight, ref float negLight2) {
			float scale = 1f;

			foreach (var system in HookModifyLightingBrightness.arr) {
				system.ModifyLightingBrightness(ref scale);
			}

			if (Lighting.NotRetro) {
				negLight *= scale;
				negLight2 *= scale;
			}
			else {
				negLight -= (scale - 1f) / 2.307692307692308f;
				negLight2 -= (scale - 1f) / 0.75f;
			}

			negLight = Math.Max(negLight, 0.001f);
			negLight2 = Math.Max(negLight2, 0.001f);
		}

		public static void PostDrawFullscreenMap(ref string mouseText) {
			foreach (var system in HookPostDrawFullscreenMap.arr) {
				system.PostDrawFullscreenMap(ref mouseText);
			}
		}

		public static void UpdateUI(GameTime gameTime) {
			if (Main.gameMenu)
				return;

			foreach (var system in HookUpdateUI.arr) {
				system.UpdateUI(gameTime);
			}
		}

		public static void PreUpdateEntities() {
			foreach (var system in HookPreUpdateEntities.arr) {
				system.PreUpdateEntities();
			}
		}

		public static void PreUpdatePlayers() {
			foreach (var system in HookPreUpdatePlayers.arr) {
				system.PreUpdatePlayers();
			}
		}

		public static void PostUpdatePlayers() {
			foreach (var system in HookPostUpdatePlayers.arr) {
				system.PostUpdatePlayers();
			}
		}

		public static void PreUpdateNPCs() {
			foreach (var system in HookPreUpdateNPCs.arr) {
				system.PreUpdateNPCs();
			}
		}

		public static void PostUpdateNPCs() {
			foreach (var system in HookPostUpdateNPCs.arr) {
				system.PostUpdateNPCs();
			}
		}

		public static void PreUpdateGores() {
			foreach (var system in HookPreUpdateGores.arr) {
				system.PreUpdateGores();
			}
		}

		public static void PostUpdateGores() {
			foreach (var system in HookPostUpdateGores.arr) {
				system.PostUpdateGores();
			}
		}

		public static void PreUpdateProjectiles() {
			foreach (var system in HookPreUpdateProjectiles.arr) {
				system.PreUpdateProjectiles();
			}
		}

		public static void PostUpdateProjectiles() {
			foreach (var system in HookPostUpdateProjectiles.arr) {
				system.PostUpdateProjectiles();
			}
		}

		public static void PreUpdateItems() {
			foreach (var system in HookPreUpdateItems.arr) {
				system.PreUpdateItems();
			}
		}

		public static void PostUpdateItems() {
			foreach (var system in HookPostUpdateItems.arr) {
				system.PostUpdateItems();
			}
		}

		public static void PreUpdateDusts() {
			foreach (var system in HookPreUpdateDusts.arr) {
				system.PreUpdateDusts();
			}
		}

		public static void PostUpdateDusts() {
			foreach (var system in HookPostUpdateDusts.arr) {
				system.PostUpdateDusts();
			}
		}

		public static void PreUpdateTime() {
			foreach (var system in HookPreUpdateTime.arr) {
				system.PreUpdateTime();
			}
		}

		public static void PostUpdateTime() {
			foreach (var system in HookPostUpdateTime.arr) {
				system.PostUpdateTime();
			}
		}

		public static void PreUpdateWorld() {
			foreach (var system in HookPreUpdateWorld.arr) {
				system.PreUpdateWorld();
			}
		}

		public static void PostUpdateWorld() {
			foreach (var system in HookPostUpdateWorld.arr) {
				system.PostUpdateWorld();
			}
		}

		public static void PreUpdateInvasions() {
			foreach (var system in HookPreUpdateInvasions.arr) {
				system.PreUpdateInvasions();
			}
		}

		public static void PostUpdateInvasions() {
			foreach (var system in HookPostUpdateInvasions.arr) {
				system.PostUpdateInvasions();
			}
		}

		public static void PostUpdateEverything() {
			foreach (var system in HookPostUpdateEverything.arr) {
				system.PostUpdateEverything();
			}
		}

		public static void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			foreach (GameInterfaceLayer layer in layers) {
				layer.Active = true;
			}

			foreach (var system in HookModifyInterfaceLayers.arr) {
				system.ModifyInterfaceLayers(layers);
			}
		}

		public static void PostDrawInterface(SpriteBatch spriteBatch) {
			foreach (var system in HookPostDrawInterface.arr) {
				system.PostDrawInterface(spriteBatch);
			}
		}

		public static void PostUpdateInput() {
			foreach (var system in HookPostUpdateInput.arr) {
				system.PostUpdateInput();
			}
		}

		public static void PreSaveAndQuit() {
			foreach (var system in HookPreSaveAndQuit.arr) {
				system.PreSaveAndQuit();
			}
		}

		public static void PostDrawTiles() {
			foreach (var system in HookPostDrawTiles.arr) {
				system.PostDrawTiles();
			}
		}

		public static void ModifyTimeRate(ref int timeRate, ref int tileUpdateRate) {
			foreach (var system in HookModifyTimeRate.arr) {
				system.ModifyTimeRate(ref timeRate, ref tileUpdateRate);
			}
		}

		public static void PreWorldGen() {
			foreach (var system in HookPreWorldGen.arr) {
				system.PreWorldGen();
			}
		}

		public static void ModifyWorldGenTasks(List<GenPass> passes, ref float totalWeight) {
			foreach (var system in HookModifyWorldGenTasks.arr) {
				system.ModifyWorldGenTasks(passes, ref totalWeight);
			}
		}

		public static void PostWorldGen() {
			foreach (var system in HookPostWorldGen.arr) {
				system.PostWorldGen();
			}
		}

		public static void ResetNearbyTileEffects() {
			foreach (var system in HookResetNearbyTileEffects.arr) {
				system.ResetNearbyTileEffects();
			}
		}

		public static void TileCountsAvailable(ReadOnlySpan<int> tileCounts) {
			foreach (var system in HookTileCountsAvailable.arr) {
				system.TileCountsAvailable(tileCounts);
			}
		}

		public static void ModifyHardmodeTasks(List<GenPass> passes) {
			foreach (var system in HookModifyHardmodeTasks.arr) {
				system.ModifyHardmodeTasks(passes);
			}
		}

		internal static bool HijackGetData(ref byte messageType, ref BinaryReader reader, int playerNumber) {
			bool hijacked = false;
			long readerPos = reader.BaseStream.Position;
			long biggestReaderPos = readerPos;

			foreach (var system in HookHijackGetData.arr) {
				if (system.HijackGetData(ref messageType, ref reader, playerNumber)) {
					hijacked = true;
					biggestReaderPos = Math.Max(reader.BaseStream.Position, biggestReaderPos);
				}

				reader.BaseStream.Position = readerPos;
			}

			if (hijacked) {
				reader.BaseStream.Position = biggestReaderPos;
			}

			return hijacked;
		}

		internal static bool HijackSendData(int whoAmI, int msgType, int remoteClient, int ignoreClient, NetworkText text, int number, float number2, float number3, float number4, int number5, int number6, int number7) {
			bool result = false;

			foreach (var system in HookHijackSendData.arr) {
				result |= system.HijackSendData(whoAmI, msgType, remoteClient, ignoreClient, text, number, number2, number3, number4, number5, number6, number7);
			}

			return result;
		}
	}
}