using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.Localization;
using Terraria.GameContent;
using Terraria.ModLoader.Core;
using Terraria.DataStructures;

namespace Terraria.ModLoader;

/// <summary>
/// This serves as the central class from which buff-related functions are supported and carried out.
/// </summary>
public static class BuffLoader
{
	internal static int extraPlayerBuffCount;
	private static int nextBuff = BuffID.Count;
	internal static readonly IList<ModBuff> buffs = new List<ModBuff>();
	internal static readonly IList<GlobalBuff> globalBuffs = new List<GlobalBuff>();

	private delegate void DelegateUpdatePlayer(int type, Player player, ref int buffIndex);
	private static DelegateUpdatePlayer[] HookUpdatePlayer;
	private delegate void DelegateUpdateNPC(int type, NPC npc, ref int buffIndex);
	private static DelegateUpdateNPC[] HookUpdateNPC;
	private static Func<int, Player, int, int, bool>[] HookReApplyPlayer;
	private static Func<int, NPC, int, int, bool>[] HookReApplyNPC;
	private delegate void DelegateModifyBuffText(int type, ref string buffName, ref string tip, ref int rare);
	private static DelegateModifyBuffText[] HookModifyBuffText;
	private static Action<string, List<Vector2>>[] HookCustomBuffTipSize;
	private static Action<string, SpriteBatch, int, int>[] HookDrawCustomBuffTip;
	private delegate bool DelegatePreDraw(SpriteBatch spriteBatch, int type, int buffIndex, ref BuffDrawParams drawParams);
	private static DelegatePreDraw[] HookPreDraw;
	private delegate void DelegatePostDraw(SpriteBatch spriteBatch, int type, int buffIndex, BuffDrawParams drawParams);
	private static DelegatePostDraw[] HookPostDraw;
	private delegate bool DelegateRightClick(int type, int buffIndex);
	private static DelegateRightClick[] HookRightClick;

	internal static int ReserveBuffID()
	{
		int reserveID = nextBuff;
		nextBuff++;
		return reserveID;
	}

	public static int BuffCount => nextBuff;

	/// <summary>
	/// Gets the ModBuff instance with the given type. If no ModBuff with the given type exists, returns null.
	/// </summary>
	public static ModBuff GetBuff(int type) => type >= BuffID.Count && type < BuffCount ? buffs[type - BuffID.Count] : null;

	internal static void ResizeArrays()
	{
		//Textures
		Array.Resize(ref TextureAssets.Buff, nextBuff);

		//Sets
		LoaderUtils.ResetStaticMembers(typeof(BuffID));

		//Etc
		Array.Resize(ref Main.pvpBuff, nextBuff);
		Array.Resize(ref Main.persistentBuff, nextBuff);
		Array.Resize(ref Main.vanityPet, nextBuff);
		Array.Resize(ref Main.lightPet, nextBuff);
		Array.Resize(ref Main.meleeBuff, nextBuff);
		Array.Resize(ref Main.debuff, nextBuff);
		Array.Resize(ref Main.buffNoSave, nextBuff);
		Array.Resize(ref Main.buffNoTimeDisplay, nextBuff);
		Array.Resize(ref Main.buffDoubleApply, nextBuff);
		Array.Resize(ref Main.buffAlpha, nextBuff);
		Array.Resize(ref Lang._buffNameCache, nextBuff);
		Array.Resize(ref Lang._buffDescriptionCache, nextBuff);

		for (int k = BuffID.Count; k < nextBuff; k++) {
			Lang._buffNameCache[k] = LocalizedText.Empty;
			Lang._buffDescriptionCache[k] = LocalizedText.Empty;
		}

		extraPlayerBuffCount = ModLoader.Mods.Any() ? ModLoader.Mods.Max(m => (int)m.ExtraPlayerBuffSlots) : 0;

		//Hooks

		// .NET 6 SDK bug: https://github.com/dotnet/roslyn/issues/57517
		// Remove generic arguments once fixed.
		ModLoader.BuildGlobalHook(ref HookUpdatePlayer, globalBuffs, g => g.Update);
		ModLoader.BuildGlobalHook(ref HookUpdateNPC, globalBuffs, g => g.Update);
		ModLoader.BuildGlobalHook(ref HookReApplyPlayer, globalBuffs, g => g.ReApply);
		ModLoader.BuildGlobalHook(ref HookReApplyNPC, globalBuffs, g => g.ReApply);
		ModLoader.BuildGlobalHook<GlobalBuff, DelegateModifyBuffText>(ref HookModifyBuffText, globalBuffs, g => g.ModifyBuffText);
		ModLoader.BuildGlobalHook(ref HookCustomBuffTipSize, globalBuffs, g => g.CustomBuffTipSize);
		ModLoader.BuildGlobalHook(ref HookDrawCustomBuffTip, globalBuffs, g => g.DrawCustomBuffTip);
		ModLoader.BuildGlobalHook<GlobalBuff, DelegatePreDraw>(ref HookPreDraw, globalBuffs, g => g.PreDraw);
		ModLoader.BuildGlobalHook<GlobalBuff, DelegatePostDraw>(ref HookPostDraw, globalBuffs, g => g.PostDraw);
		ModLoader.BuildGlobalHook<GlobalBuff, DelegateRightClick>(ref HookRightClick, globalBuffs, g => g.RightClick);
	}

	internal static void PostSetupContent()
	{
		Main.Initialize_BuffDataFromMountData();
	}

	internal static void FinishSetup()
	{
		foreach (ModBuff buff in buffs) {
			Lang._buffNameCache[buff.Type] = buff.DisplayName;
			Lang._buffDescriptionCache[buff.Type] = buff.Description;
		}
	}

	internal static void Unload()
	{
		buffs.Clear();
		nextBuff = BuffID.Count;
		globalBuffs.Clear();
	}

	internal static bool IsModBuff(int type) => type >= BuffID.Count;
	//in Terraria.Player.UpdateBuffs at end of if else chain add BuffLoader.Update(this.buffType[k], this, ref k);
	public static void Update(int buff, Player player, ref int buffIndex)
	{
		int originalIndex = buffIndex;
		if (IsModBuff(buff)) {
			GetBuff(buff).Update(player, ref buffIndex);
		}
		foreach (var hook in HookUpdatePlayer) {
			if (buffIndex != originalIndex)
				break;

			hook(buff, player, ref buffIndex);
		}
	}

	public static void Update(int buff, NPC npc, ref int buffIndex)
	{
		int originalIndex = buffIndex;
		if (IsModBuff(buff)) {
			GetBuff(buff).Update(npc, ref buffIndex);
		}
		foreach (var hook in HookUpdateNPC) {
			if (buffIndex != originalIndex)
				break;

			hook(buff, npc, ref buffIndex);
		}
	}

	public static bool ReApply(int buff, Player player, int time, int buffIndex)
	{
		foreach (var hook in HookReApplyPlayer) {
			if (hook(buff, player, time, buffIndex)) {
				return true;
			}
		}
		if (IsModBuff(buff)) {
			return GetBuff(buff).ReApply(player, time, buffIndex);
		}
		return false;
	}

	public static bool ReApply(int buff, NPC npc, int time, int buffIndex)
	{
		foreach (var hook in HookReApplyNPC) {
			if (hook(buff, npc, time, buffIndex)) {
				return true;
			}
		}
		if (IsModBuff(buff)) {
			return GetBuff(buff).ReApply(npc, time, buffIndex);
		}
		return false;
	}

	public static void ModifyBuffText(int buff, ref string buffName, ref string tip, ref int rare)
	{
		if (IsModBuff(buff)) {
			GetBuff(buff).ModifyBuffText(ref buffName, ref tip, ref rare);
		}
		foreach (var hook in HookModifyBuffText) {
			hook(buff, ref buffName, ref tip, ref rare);
		}
	}

	public static void CustomBuffTipSize(string buffTip, List<Vector2> sizes)
	{
		foreach (var hook in HookCustomBuffTipSize) {
			hook(buffTip, sizes);
		}
	}

	public static void DrawCustomBuffTip(string buffTip, SpriteBatch spriteBatch, int originX, int originY)
	{
		foreach (var hook in HookDrawCustomBuffTip) {
			hook(buffTip, spriteBatch, originX, originY);
		}
	}

	public static bool PreDraw(SpriteBatch spriteBatch, int type, int buffIndex, ref BuffDrawParams drawParams)
	{
		bool result = true;
		foreach (var hook in HookPreDraw) {
			result &= hook(spriteBatch, type, buffIndex, ref drawParams);
		}
		if (result && IsModBuff(type)) {
			return GetBuff(type).PreDraw(spriteBatch, buffIndex, ref drawParams);
		}
		return result;
	}

	public static void PostDraw(SpriteBatch spriteBatch, int type, int buffIndex, BuffDrawParams drawParams)
	{
		if (IsModBuff(type)) {
			GetBuff(type).PostDraw(spriteBatch, buffIndex, drawParams);
		}
		foreach (var hook in HookPostDraw) {
			hook(spriteBatch, type, buffIndex, drawParams);
		}
	}

	public static bool RightClick(int type, int buffIndex)
	{
		bool result = true;
		foreach (var hook in HookRightClick) {
			result &= hook(type, buffIndex);
		}
		if (IsModBuff(type)) {
			result &= GetBuff(type).RightClick(buffIndex);
		}
		return result;
	}
}
