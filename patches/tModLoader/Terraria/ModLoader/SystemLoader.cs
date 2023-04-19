using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Terraria.GameInput;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.Map;
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

	internal static void ResizeArrays()
	{
		RebuildHooks();
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
			system.OnWorldLoad();
		}
	}

	public static void OnWorldUnload()
	{
		foreach (var system in HookOnWorldUnload.Enumerate()) {
			system.OnWorldUnload();
		}
	}

	public static void ClearWorld() {
		foreach (var system in HookClearWorld.Enumerate()) {
			system.ClearWorld();
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

	// Based on LocalizedText._substitutionRegex
	private static readonly Regex _validKeysForDialogueSubstitutions = new("[a-zA-Z][\\w\\.]*", RegexOptions.Compiled);

	public static void PopulateDialogueSubstitutions(IDictionary<string, object> substitutions, NPC npc)
	{
		AddDefaultDialogueSubstitutions(substitutions, npc);

		// Mod substitutions
		foreach (var system in HookPopulateDialogueSubstitutions.Enumerate()) {
			var newSubstitutions = system.PopulateDialogueSubstitutions(npc);
			if (newSubstitutions is null) {
				continue; // Null is ignored, mentioned in PopulateDialogueSubstitutions' docs.
			}

			foreach (var pair in newSubstitutions) {
				if (!_validKeysForDialogueSubstitutions.IsMatch(pair.Key)) {
					continue; // Invalid keys are ignored, mentioned in PopulateDialogueSubstitutions' docs.
				}

				substitutions.Add($"{system.Mod.Name}.{pair.Key}", pair.Value);
			}
		}
	}

	private static void AddDefaultDialogueSubstitutions(IDictionary<string, object> substitutions, NPC npc)
	{
		var newSubstitutions = new Dictionary<string, object>() {
			// Missing town NPCs. All keys are NPCID entries / persistent IDs.
			{ "SantaClaus", NPC.GetFirstNPCNameOrNull(142) },
			{ "SkeletonMerchant", NPC.GetFirstNPCNameOrNull(453) },
			{ "BestiaryGirl", NPC.GetFirstNPCNameOrNull(633) },
			{ "TownCat", NPC.GetFirstNPCNameOrNull(637) },
			{ "TownDog", NPC.GetFirstNPCNameOrNull(638) },
			{ "TownBunny", NPC.GetFirstNPCNameOrNull(656) },
			{ "Princess", NPC.GetFirstNPCNameOrNull(663) },
			{ "TownSlimeBlue", NPC.GetFirstNPCNameOrNull(670) },
			{ "TownSlimeGreen", NPC.GetFirstNPCNameOrNull(678) },
			{ "TownSlimeOld", NPC.GetFirstNPCNameOrNull(679) },
			{ "TownSlimePurple", NPC.GetFirstNPCNameOrNull(680) },
			{ "TownSlimeRainbow", NPC.GetFirstNPCNameOrNull(681) },
			{ "TownSlimeRed", NPC.GetFirstNPCNameOrNull(682) },
			{ "TownSlimeYellow", NPC.GetFirstNPCNameOrNull(683) },
			{ "TownSlimeCopper", NPC.GetFirstNPCNameOrNull(684) },
			// Player info
			{ "PlayerMale", Main.LocalPlayer.Male },
			// Most flags below here are from Condition.
			// Downed flags
			{ "KingSlimeDefeated", Condition.DownedKingSlime.IsMet() },
			{ "EyeOfCthulhuDefeated", Condition.DownedEyeOfCthulhu.IsMet() },
			{ "Boss2Defeated", Condition.DownedEowOrBoc.IsMet() },
			{ "QueenBeeDefeated", Condition.DownedQueenBee.IsMet() },
			{ "SkeletronDefeated", Condition.DownedSkeletron.IsMet() },
			{ "DeerclopsDefeated", Condition.DownedDeerclops.IsMet() },
			{ "QueenSlimeDefeated", Condition.DownedQueenSlime.IsMet() },
			{ "EarlygameBossDefeated", Condition.DownedEarlygameBoss.IsMet() },
			{ "MechBossAnyDefeated", Condition.DownedMechBossAny.IsMet() },
			{ "TwinsDefeated", Condition.DownedTwins.IsMet() },
			{ "DestroyerDefeated", Condition.DownedDestroyer.IsMet() },
			{ "SkeletronPrimeDefeated", Condition.DownedSkeletronPrime.IsMet() },
			{ "MechBossAllDefeated", Condition.DownedMechBossAll.IsMet() },
			{ "PlanteraDefeated", Condition.DownedPlantera.IsMet() },
			{ "EmpressOfLightDefeated", Condition.DownedEmpressOfLight.IsMet() },
			{ "MourningWoodDefeated", Condition.DownedMourningWood.IsMet() },
			{ "EverscreamDefeated", Condition.DownedEverscream.IsMet() },
			{ "SantaNK1Defeated", Condition.DownedSantaNK1.IsMet() },
			{ "CultistDefeated", Condition.DownedCultist.IsMet() },
			{ "GoblinArmyDefeated", Condition.DownedGoblinArmy.IsMet() },
			{ "PiratesDefeated", Condition.DownedPirates.IsMet() },
			{ "MartiansDefeated", Condition.DownedMartians.IsMet() },
			{ "SolarPillarDefeated", Condition.DownedSolarPillar.IsMet() },
			{ "VortexPillarDefeated", Condition.DownedVortexPillar.IsMet() },
			{ "NebulaPillarDefeated", Condition.DownedNebulaPillar.IsMet() },
			{ "StardustPillarDefeated", Condition.DownedStardustPillar.IsMet() },
			{ "OldOnesArmyAnyDefeated", Condition.DownedOldOnesArmyAny.IsMet() },
			{ "OldOnesArmyT1Defeated", Condition.DownedOldOnesArmyT1.IsMet() },
			{ "OldOnesArmyT2Defeated", Condition.DownedOldOnesArmyT2.IsMet() },
			{ "OldOnesArmyT3Defeated", Condition.DownedOldOnesArmyT3.IsMet() },
			// Biome/event flags
			{ "Dungeon", Condition.InDungeon.IsMet() },
			{ "Corrupt", Condition.InCorrupt.IsMet() },
			{ "Hallow", Condition.InHallow.IsMet() },
			{ "Meteor", Condition.InMeteor.IsMet() },
			{ "Jungle", Condition.InJungle.IsMet() },
			{ "Snow", Condition.InSnow.IsMet() },
			{ "Crimson", Condition.InCrimson.IsMet() },
			{ "WaterCandle", Condition.InWaterCandle.IsMet() },
			{ "PeaceCandle", Condition.InPeaceCandle.IsMet() },
			{ "TowerSolar", Condition.InTowerSolar.IsMet() },
			{ "TowerVortex", Condition.InTowerVortex.IsMet() },
			{ "TowerNebula", Condition.InTowerNebula.IsMet() },
			{ "TowerStardust", Condition.InTowerStardust.IsMet() },
			{ "Desert", Condition.InDesert.IsMet() },
			{ "Glowshroom", Condition.InGlowshroom.IsMet() },
			{ "UndergroundDesert", Condition.InUndergroundDesert.IsMet() },
			{ "SkyHeight", Condition.InSkyHeight.IsMet() },
			{ "Space", Condition.InSpace.IsMet() },
			{ "OverworldHeight", Condition.InOverworldHeight.IsMet() },
			{ "DirtLayerHeight", Condition.InDirtLayerHeight.IsMet() },
			{ "RockLayerHeight", Condition.InRockLayerHeight.IsMet() },
			{ "UnderworldHeight", Condition.InUnderworldHeight.IsMet() },
			{ "Underworld", Condition.InUnderworld.IsMet() },
			{ "Beach", Condition.InBeach.IsMet() },
			{ "Rain", Condition.InRain.IsMet() },
			{ "Sandstorm", Condition.InSandstorm.IsMet() },
			{ "OldOneArmy", Condition.InOldOneArmy.IsMet() },
			{ "Granite", Condition.InGranite.IsMet() },
			{ "Marble", Condition.InMarble.IsMet() },
			{ "Hive", Condition.InHive.IsMet() },
			{ "GemCave", Condition.InGemCave.IsMet() },
			{ "LihzhardTemple", Condition.InLihzhardTemple.IsMet() },
			{ "Aether", Condition.InAether.IsMet() },
			{ "Thunderstorm", Condition.Thunderstorm.IsMet() },
			{ "BirthdayParty", Condition.BirthdayParty.IsMet() },
			{ "LanternNight", Condition.LanternNight.IsMet() },
			{ "HappyWindyDay", Condition.HappyWindyDay.IsMet() },
			// World info
			{ "ClassicMode", Condition.InClassicMode.IsMet() },
			{ "ExpertMode", Condition.InExpertMode.IsMet() },
			{ "MasterMode", Condition.InMasterMode.IsMet() },
			{ "JourneyMode", Condition.InJourneyMode.IsMet() },
			{ "CrimsonWorld", Condition.CrimsonWorld.IsMet() },
			{ "DrunkWorld", Condition.DrunkWorld.IsMet() },
			{ "RemixWorld", Condition.RemixWorld.IsMet() },
			{ "NotTheBeesWorld", Condition.NotTheBeesWorld.IsMet() },
			{ "ForTheWorthyWorld", Condition.ForTheWorthyWorld.IsMet() },
			{ "TenthAnniversaryWorld", Condition.TenthAnniversaryWorld.IsMet() },
			{ "DontStarveWorld", Condition.DontStarveWorld.IsMet() },
			{ "NoTrapsWorld", Condition.NoTrapsWorld.IsMet() },
			{ "ZenithWorld", Condition.ZenithWorld.IsMet() },
			{ "Christmas", Condition.Christmas.IsMet() },
			{ "Halloween", Condition.Halloween.IsMet() },
			// Misc.
			{ "Multiplayer", Condition.Multiplayer.IsMet() },
			{ "HappyEnough", Condition.HappyEnough.IsMet() },
			{ "HappyEnoughToSellPylons", Condition.HappyEnoughToSellPylons.IsMet() },
			{ "AnotherTownNPCNearby", Condition.AnotherTownNPCNearby.IsMet() },
			{ "IsNpcShimmered", npc?.IsShimmerVariant ?? false }
		};
		
		foreach (var pair in newSubstitutions) {
			// Future-proof if Terraria adds these keys.
			if (!substitutions.ContainsKey(pair.Key)) {
				substitutions[pair.Key] = pair.Value;
			}
		}
		
		// Autoloaded substitutions for keybinds and town NPCs.
		foreach (ModKeybind keybind in KeybindLoader.modKeybinds.Values) {
			substitutions.Add($"{keybind.Mod.Name}.InputTrigger_{keybind.Name}", PlayerInput.GenerateInputTag_ForCurrentGamemode(tagForGameplay: true, keybind.FullName));
		}

		for (int i = NPCID.Count; i < NPCLoader.NPCCount; i++) {
			NPC townNPC = ContentSamples.NpcsByNetId[i];
			if (townNPC.isLikeATownNPC || NPCID.Sets.SpawnsWithCustomName[i]) {
				substitutions.Add($"{townNPC.ModNPC.Mod.Name}.{townNPC.ModNPC.Name}", NPC.GetFirstNPCNameOrNull(i));
			}
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