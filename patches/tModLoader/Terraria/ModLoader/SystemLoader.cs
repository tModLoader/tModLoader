using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.IO;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.Exceptions;
using Terraria.UI;
using Terraria.WorldBuilding;

namespace Terraria.ModLoader;

/// <summary>
/// This is where all <see cref="ModSystem"/> hooks are gathered and called.
/// </summary>
public static partial class SystemLoader
{
	internal static readonly List<ModSystem> Systems = new();
	internal static readonly Dictionary<Mod, List<ModSystem>> SystemsByMod = new();

	internal static void Add(ModSystem modSystem)
	{
		if (!SystemsByMod.TryGetValue(modSystem.Mod, out var modsSystems)) {
			SystemsByMod[modSystem.Mod] = modsSystems = new();
		}

		Systems.Add(modSystem);
		modsSystems.Add(modSystem);
	}

	internal static void Unload()
	{
		Systems.Clear();
		SystemsByMod.Clear();
	}

	internal static IEnumerable<Type> TypesWithResizeArraysAttribute(Assembly assembly) => AssemblyManager.GetLoadableTypes(assembly)
				.Where(t => t.GetAttribute<ReinitializeDuringResizeArraysAttribute>() != null)
				.OrderBy(type => type.FullName, StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// LoaderUtils.ResetStaticMembers does not mark the class constructor as having run via normal means
	/// and running the class constructor after ResizeArrays can cause issues due to duplicate registration.
	/// </summary>
	internal static void EnsureResizeArraysAttributeStaticCtorsRun(Mod mod)
	{
		void RunStaticCtorIfNotAlreadyRun(Type type)
		{
			RuntimeHelpers.RunClassConstructor(type.TypeHandle);
			foreach (var nestedType in type.GetNestedTypes())
				RunStaticCtorIfNotAlreadyRun(type);
		}

		foreach (var typesToReinitialize in TypesWithResizeArraysAttribute(mod.Code))
			RunStaticCtorIfNotAlreadyRun(typesToReinitialize);
	}

	internal static void ResizeArrays(bool unloading)
	{
		RebuildHooks();

		if (unloading)
			return;

		foreach (var mod in ModLoader.Mods) {
			using var _ = new ModContent.TrackCurrentlyLoadingMod(mod.Name);
			foreach (var typesToReinitialize in TypesWithResizeArraysAttribute(mod.Code))
				LoaderUtils.ResetStaticMembers(typesToReinitialize);
		}

		foreach (var system in HookResizeArrays.Enumerate()) {
			using var _ = new ModContent.TrackCurrentlyLoadingMod(system.Mod.Name);
			system.ResizeArrays();
		}
	}

	internal static void OnModLoad(Mod mod)
	{
		if (SystemsByMod.TryGetValue(mod, out var systems)) {
			foreach (var system in systems) {
				system.OnModLoad();
			}
		}
	}

	internal static void OnModUnload(Mod mod)
	{
		if (SystemsByMod.TryGetValue(mod, out var systems)) {
			foreach (var system in systems) {
				system.OnModUnload();
			}
		}
	}

	internal static void PostSetupContent(Mod mod)
	{
		if (SystemsByMod.TryGetValue(mod, out var systems)) {
			foreach (var system in systems) {
				system.PostSetupContent();
			}
		}
	}

	internal static void OnLocalizationsLoaded()
	{
		foreach (var system in HookOnLocalizationsLoaded.Enumerate()) {
			system.OnLocalizationsLoaded();
		}
	}

	internal static void AddRecipes(Mod mod)
	{
		if (SystemsByMod.TryGetValue(mod, out var systems)) {
			foreach (var system in systems) {
				system.AddRecipes();
			}
		}
	}

	internal static void PostAddRecipes(Mod mod)
	{
		if (SystemsByMod.TryGetValue(mod, out var systems)) {
			foreach (var system in systems) {
				system.PostAddRecipes();
			}
		}
	}

	internal static void PostSetupRecipes(Mod mod)
	{
		if (SystemsByMod.TryGetValue(mod, out var systems)) {
			foreach (var system in systems) {
				system.PostSetupRecipes();
			}
		}
	}

	internal static void AddRecipeGroups(Mod mod)
	{
		if (SystemsByMod.TryGetValue(mod, out var systems)) {
			foreach (var system in systems) {
				system.AddRecipeGroups();
			}
		}
	}

	public static void OnWorldLoad()
	{
		foreach (var system in HookOnWorldLoad.Enumerate()) {
			try {
				system.OnWorldLoad();
			}
			catch (Exception e) {
				throw new CustomModDataException(system.Mod, e.Message, e);
			}
		}
	}

	public static void PostWorldLoad()
	{
		foreach (var system in HookPostWorldLoad.Enumerate()) {
			try {
				system.PostWorldLoad();
			}
			catch (Exception e) {
				throw new CustomModDataException(system.Mod, e.Message, e);
			}
		}
	}

	public static void OnWorldUnload()
	{
		foreach (var system in HookOnWorldUnload.Enumerate()) {
			try {
				system.OnWorldUnload();
			}
			catch {
				Logging.tML.Error($"Encountered an error while running the \"{system.Name}.OnWorldUnload\" method from the \"{system.Mod.Name}\" mod. The game, world, or mod might be in an unstable state.");
			}
		}
	}

	public static void ClearWorld()
	{
		foreach (var system in HookClearWorld.Enumerate()) {
			try {
				system.ClearWorld();
			}
			catch (Exception e) {
				throw new CustomModDataException(system.Mod, e.Message, e);
			}
		}
	}

	public static bool CanWorldBePlayed(PlayerFileData playerData, WorldFileData worldData, out ModSystem rejector)
	{
		foreach (var system in HookCanWorldBePlayed.Enumerate()) {
			if (!system.CanWorldBePlayed(playerData, worldData)) {
				rejector = system;
				return false;
			}
		}

		rejector = null;
		return true;
	}

	public static void ModifyScreenPosition()
	{
		foreach (var system in HookModifyScreenPosition.Enumerate()) {
			system.ModifyScreenPosition();
		}
	}

	public static void ModifyTransformMatrix(ref SpriteViewMatrix Transform)
	{
		foreach (var system in HookModifyTransformMatrix.Enumerate()) {
			system.ModifyTransformMatrix(ref Transform);
		}
	}

	public static void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
	{
		if (Main.gameMenu)
			return;

		foreach (var system in HookModifySunLightColor.Enumerate()) {
			system.ModifySunLightColor(ref tileColor, ref backgroundColor);
		}
	}

	public static void ModifyLightingBrightness(ref float negLight, ref float negLight2)
	{
		float scale = 1f;

		foreach (var system in HookModifyLightingBrightness.Enumerate()) {
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

	public static void PreDrawMapIconOverlay(IReadOnlyList<IMapLayer> layers, MapOverlayDrawContext mapOverlayDrawContext)
	{
		foreach (var system in HookPreDrawMapIconOverlay.Enumerate()) {
			system.PreDrawMapIconOverlay(layers, mapOverlayDrawContext);
		}
	}

	public static void PostDrawFullscreenMap(ref string mouseText)
	{
		foreach (var system in HookPostDrawFullscreenMap.Enumerate()) {
			system.PostDrawFullscreenMap(ref mouseText);
		}
	}

	public static void UpdateUI(GameTime gameTime)
	{
		if (Main.gameMenu)
			return;

		foreach (var system in HookUpdateUI.Enumerate()) {
			system.UpdateUI(gameTime);
		}
	}

	public static void PreUpdateEntities()
	{
		foreach (var system in HookPreUpdateEntities.Enumerate()) {
			system.PreUpdateEntities();
		}
	}

	public static void PreUpdatePlayers()
	{
		foreach (var system in HookPreUpdatePlayers.Enumerate()) {
			system.PreUpdatePlayers();
		}
	}

	public static void PostUpdatePlayers()
	{
		foreach (var system in HookPostUpdatePlayers.Enumerate()) {
			system.PostUpdatePlayers();
		}
	}

	public static void PreUpdateNPCs()
	{
		foreach (var system in HookPreUpdateNPCs.Enumerate()) {
			system.PreUpdateNPCs();
		}
	}

	public static void PostUpdateNPCs()
	{
		foreach (var system in HookPostUpdateNPCs.Enumerate()) {
			system.PostUpdateNPCs();
		}
	}

	public static void PreUpdateGores()
	{
		foreach (var system in HookPreUpdateGores.Enumerate()) {
			system.PreUpdateGores();
		}
	}

	public static void PostUpdateGores()
	{
		foreach (var system in HookPostUpdateGores.Enumerate()) {
			system.PostUpdateGores();
		}
	}

	public static void PreUpdateProjectiles()
	{
		foreach (var system in HookPreUpdateProjectiles.Enumerate()) {
			system.PreUpdateProjectiles();
		}
	}

	public static void PostUpdateProjectiles()
	{
		foreach (var system in HookPostUpdateProjectiles.Enumerate()) {
			system.PostUpdateProjectiles();
		}
	}

	public static void PreUpdateItems()
	{
		foreach (var system in HookPreUpdateItems.Enumerate()) {
			system.PreUpdateItems();
		}
	}

	public static void PostUpdateItems()
	{
		foreach (var system in HookPostUpdateItems.Enumerate()) {
			system.PostUpdateItems();
		}
	}

	public static void PreUpdateDusts()
	{
		foreach (var system in HookPreUpdateDusts.Enumerate()) {
			system.PreUpdateDusts();
		}
	}

	public static void PostUpdateDusts()
	{
		foreach (var system in HookPostUpdateDusts.Enumerate()) {
			system.PostUpdateDusts();
		}
	}

	public static void PreUpdateTime()
	{
		foreach (var system in HookPreUpdateTime.Enumerate()) {
			system.PreUpdateTime();
		}
	}

	public static void PostUpdateTime()
	{
		foreach (var system in HookPostUpdateTime.Enumerate()) {
			system.PostUpdateTime();
		}
	}

	public static void PreUpdateWorld()
	{
		foreach (var system in HookPreUpdateWorld.Enumerate()) {
			system.PreUpdateWorld();
		}
	}

	public static void PostUpdateWorld()
	{
		foreach (var system in HookPostUpdateWorld.Enumerate()) {
			system.PostUpdateWorld();
		}
	}

	public static void PreUpdateInvasions()
	{
		foreach (var system in HookPreUpdateInvasions.Enumerate()) {
			system.PreUpdateInvasions();
		}
	}

	public static void PostUpdateInvasions()
	{
		foreach (var system in HookPostUpdateInvasions.Enumerate()) {
			system.PostUpdateInvasions();
		}
	}

	public static void PostUpdateEverything()
	{
		foreach (var system in HookPostUpdateEverything.Enumerate()) {
			system.PostUpdateEverything();
		}
	}

	public static void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
	{
		foreach (GameInterfaceLayer layer in layers) {
			layer.Active = true;
		}

		foreach (var system in HookModifyInterfaceLayers.Enumerate()) {
			system.ModifyInterfaceLayers(layers);
		}
	}

	public static void ModifyGameTipVisibility(IReadOnlyList<GameTipData> tips)
	{
		foreach (var system in HookModifyGameTipVisibility.Enumerate()) {
			system.ModifyGameTipVisibility(tips);
		}
	}

	public static void PostDrawInterface(SpriteBatch spriteBatch)
	{
		foreach (var system in HookPostDrawInterface.Enumerate()) {
			system.PostDrawInterface(spriteBatch);
		}
	}

	public static void PostUpdateInput()
	{
		foreach (var system in HookPostUpdateInput.Enumerate()) {
			system.PostUpdateInput();
		}
	}

	public static void PreSaveAndQuit()
	{
		foreach (var system in HookPreSaveAndQuit.Enumerate()) {
			system.PreSaveAndQuit();
		}
	}

	public static void PostDrawTiles()
	{
		foreach (var system in HookPostDrawTiles.Enumerate()) {
			system.PostDrawTiles();
		}
	}

	public static void ModifyTimeRate(ref double timeRate, ref double tileUpdateRate, ref double eventUpdateRate)
	{
		foreach (var system in HookModifyTimeRate.Enumerate()) {
			system.ModifyTimeRate(ref timeRate, ref tileUpdateRate, ref eventUpdateRate);
		}
	}

	public static void PreWorldGen()
	{
		foreach (var system in HookPreWorldGen.Enumerate()) {
			system.PreWorldGen();
		}
	}

	public static void ModifyWorldGenTasks(List<GenPass> passes, ref double totalWeight)
	{
		foreach (var system in HookModifyWorldGenTasks.Enumerate()) {
			try {
				system.ModifyWorldGenTasks(passes, ref totalWeight);
			}
			catch (Exception e) {
				string message = string.Join(
					"\n",
					system.FullName + " : " + Language.GetTextValue("tModLoader.WorldGenError"),
					e
				);
				Utils.ShowFancyErrorMessage(message, 0);

				throw;
			}
		}

		passes.RemoveAll(x => !x.Enabled);
	}

	public static void PostWorldGen()
	{
		foreach (var system in HookPostWorldGen.Enumerate()) {
			system.PostWorldGen();
		}
	}

	public static void ResetNearbyTileEffects()
	{
		foreach (var system in HookResetNearbyTileEffects.Enumerate()) {
			system.ResetNearbyTileEffects();
		}
	}

	public static void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
	{
		foreach (var system in HookTileCountsAvailable.Enumerate()) {
			system.TileCountsAvailable(tileCounts);
		}
	}

	public static void ModifyHardmodeTasks(List<GenPass> passes)
	{
		foreach (var system in HookModifyHardmodeTasks.Enumerate()) {
			system.ModifyHardmodeTasks(passes);
		}

		passes.RemoveAll(x => !x.Enabled);
	}

	internal static bool HijackGetData(ref byte messageType, ref BinaryReader reader, int playerNumber)
	{
		bool hijacked = false;
		long readerPos = reader.BaseStream.Position;
		long biggestReaderPos = readerPos;

		foreach (var system in HookHijackGetData.Enumerate()) {
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

	internal static bool HijackSendData(int whoAmI, int msgType, int remoteClient, int ignoreClient, NetworkText text, int number, float number2, float number3, float number4, int number5, int number6, int number7)
	{
		bool result = false;

		foreach (var system in HookHijackSendData.Enumerate()) {
			result |= system.HijackSendData(whoAmI, msgType, remoteClient, ignoreClient, text, number, number2, number3, number4, number5, number6, number7);
		}

		return result;
	}
}