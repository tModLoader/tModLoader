﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using Terraria.Graphics;
using Terraria.Localization;
using Terraria.UI;
using Terraria.WorldBuilding;

#pragma warning disable IDE0044 // Add readonly modifier

namespace Terraria.ModLoader
{
	partial class SystemLoader
	{
		private class HookList
		{
			public readonly MethodInfo method;

			public ModSystem[] arr = new ModSystem[0];

			public HookList(MethodInfo method) {
				this.method = method;
			}
		}

		private static readonly List<HookList> hooks = new List<HookList>();

		private static HookList AddHook<F>(Expression<Func<ModSystem, F>> func) {
			var hook = new HookList(ModLoader.Method(func));

			hooks.Add(hook);
			
			return hook;
		}

		private static void RebuildHooks() {
			foreach (var hook in hooks) {
				hook.arr = ModLoader.BuildGlobalHook(Systems, hook.method);
			}
		}

		//Delegates
		
		private delegate void DelegateModifyTransformMatrix(ref SpriteViewMatrix Transform);
		
		private delegate void DelegateModifySunLightColor(ref Color tileColor, ref Color backgroundColor);
		
		private delegate void DelegateModifyLightingBrightness(ref float scale);

		private delegate void DelegatePostDrawFullscreenMap(ref string mouseText);

		private delegate void DelegateModifyTimeRate(ref double timeRate, ref double tileUpdateRate, ref double eventUpdateRate);

		private delegate void DelegateModifyWorldGenTasks(List<GenPass> passes, ref float totalWeight);

		private delegate bool DelegateHijackGetData(ref byte messageType, ref BinaryReader reader, int playerNumber);

		private delegate void DelegateTileCountsAvailable(ReadOnlySpan<int> tileCounts);

		//HookLists

		private static HookList HookOnWorldLoad = AddHook<Action>(s => s.OnWorldLoad);

		private static HookList HookOnWorldUnload = AddHook<Action>(s => s.OnWorldUnload);

		private static HookList HookModifyScreenPosition = AddHook<Action>(s => s.ModifyScreenPosition);

		private static HookList HookModifyTransformMatrix = AddHook<DelegateModifyTransformMatrix>(s => s.ModifyTransformMatrix);

		private static HookList HookModifySunLightColor = AddHook<DelegateModifySunLightColor>(s => s.ModifySunLightColor);

		private static HookList HookModifyLightingBrightness = AddHook<DelegateModifyLightingBrightness>(s => s.ModifyLightingBrightness);

		private static HookList HookPostDrawFullscreenMap = AddHook<DelegatePostDrawFullscreenMap>(s => s.PostDrawFullscreenMap);

		private static HookList HookUpdateUI = AddHook<Action<GameTime>>(s => s.UpdateUI);

		private static HookList HookPreUpdateEntities = AddHook<Action>(s => s.PreUpdateEntities);

		private static HookList HookPreUpdatePlayers = AddHook<Action>(s => s.PreUpdatePlayers);

		private static HookList HookPostUpdatePlayers = AddHook<Action>(s => s.PostUpdatePlayers);

		private static HookList HookPreUpdateNPCs = AddHook<Action>(s => s.PreUpdateNPCs);

		private static HookList HookPostUpdateNPCs = AddHook<Action>(s => s.PostUpdateNPCs);

		private static HookList HookPreUpdateGores = AddHook<Action>(s => s.PreUpdateGores);

		private static HookList HookPostUpdateGores = AddHook<Action>(s => s.PostUpdateGores);

		private static HookList HookPreUpdateProjectiles = AddHook<Action>(s => s.PreUpdateProjectiles);

		private static HookList HookPostUpdateProjectiles = AddHook<Action>(s => s.PostUpdateProjectiles);

		private static HookList HookPreUpdateItems = AddHook<Action>(s => s.PreUpdateItems);

		private static HookList HookPostUpdateItems = AddHook<Action>(s => s.PostUpdateItems);

		private static HookList HookPreUpdateDusts = AddHook<Action>(s => s.PreUpdateDusts);

		private static HookList HookPostUpdateDusts = AddHook<Action>(s => s.PostUpdateDusts);

		private static HookList HookPreUpdateTime = AddHook<Action>(s => s.PreUpdateTime);

		private static HookList HookPostUpdateTime = AddHook<Action>(s => s.PostUpdateTime);

		private static HookList HookPreUpdateWorld = AddHook<Action>(s => s.PreUpdateWorld);

		private static HookList HookPostUpdateWorld = AddHook<Action>(s => s.PostUpdateWorld);

		private static HookList HookPreUpdateInvasions = AddHook<Action>(s => s.PreUpdateInvasions);

		private static HookList HookPostUpdateInvasions = AddHook<Action>(s => s.PostUpdateInvasions);

		private static HookList HookPostUpdateEverything = AddHook<Action>(s => s.PostUpdateEverything);

		private static HookList HookModifyInterfaceLayers = AddHook<Action<List<GameInterfaceLayer>>> (s => s.ModifyInterfaceLayers);

		private static HookList HookPostDrawInterface = AddHook<Action<SpriteBatch>>(s => s.PostDrawInterface);

		private static HookList HookPostUpdateInput = AddHook<Action>(s => s.PostUpdateInput);

		private static HookList HookPreSaveAndQuit = AddHook<Action>(s => s.PreSaveAndQuit);

		private static HookList HookPostDrawTiles = AddHook<Action>(s => s.PostDrawTiles);

		private static HookList HookModifyTimeRate = AddHook<DelegateModifyTimeRate>(s => s.ModifyTimeRate);

		private static HookList HookPreWorldGen = AddHook<Action>(s => s.PreWorldGen);

		private static HookList HookModifyWorldGenTasks = AddHook<DelegateModifyWorldGenTasks>(s => s.ModifyWorldGenTasks);

		private static HookList HookPostWorldGen = AddHook<Action>(s => s.PostWorldGen);

		private static HookList HookResetNearbyTileEffects = AddHook<Action>(s => s.ResetNearbyTileEffects);

		private static HookList HookTileCountsAvailable = AddHook<DelegateTileCountsAvailable>(s => s.TileCountsAvailable);

		private static HookList HookModifyHardmodeTasks = AddHook<Action<List<GenPass>>>(s => s.ModifyHardmodeTasks);

		private static HookList HookHijackGetData = AddHook<DelegateHijackGetData>(s => s.HijackGetData);

		private static HookList HookHijackSendData = AddHook<Func<int, int, int, int, NetworkText, int, float, float, float, int, int, int, bool>>(s => s.HijackSendData);
	}
}