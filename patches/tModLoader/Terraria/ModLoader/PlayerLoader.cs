using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.Graphics;
using Terraria.ModLoader.Default;
using HookList = Terraria.ModLoader.Core.HookList<Terraria.ModLoader.ModPlayer>;

namespace Terraria.ModLoader;

/// <summary>
/// This is where all ModPlayer hooks are gathered and called.
/// </summary>
public static class PlayerLoader
{
	private static readonly List<ModPlayer> players = new();
	private static readonly List<HookList> hooks = new();
	private static readonly List<HookList> modHooks = new();

	private static HookList AddHook<F>(Expression<Func<ModPlayer, F>> func) where F : Delegate
	{
		var hook = HookList.Create(func);
		hooks.Add(hook);
		return hook;
	}

	public static T AddModHook<T>(T hook) where T : HookList
	{
		hook.Update(players);
		modHooks.Add(hook);
		return hook;
	}

	internal static void Add(ModPlayer player)
	{
		player.Index = (ushort)players.Count;
		players.Add(player);
	}

	internal static void ResizeArrays()
	{
		foreach (var hook in hooks.Union(modHooks)) {
			hook.Update(players);
		}
	}

	internal static void Unload()
	{
		players.Clear();
		modHooks.Clear();
	}

	private static HookList HookInitialize = AddHook<Action>(p => p.Initialize);

	internal static void SetupPlayer(Player player)
	{
		player.modPlayers = NewInstances(player, CollectionsMarshal.AsSpan(players));

		foreach (var modPlayer in HookInitialize.Enumerate(player)) {
			modPlayer.Initialize();
		}
	}

	private static ModPlayer[] NewInstances(Player player, Span<ModPlayer> modPlayers)
	{
		var arr = new ModPlayer[modPlayers.Length];
		for (int i = 0; i < modPlayers.Length; i++)
			arr[i] = modPlayers[i].NewInstance(player);

		return arr;
	}

	private static HookList HookResetEffects = AddHook<Action>(p => p.ResetEffects);

	public static void ResetEffects(Player player)
	{
		foreach (var modPlayer in HookResetEffects.Enumerate(player)) {
			modPlayer.ResetEffects();
		}
	}

	private static HookList HookResetInfoAccessories = AddHook<Action>(p => p.ResetInfoAccessories);

	public static void ResetInfoAccessories(Player player)
	{
		foreach (var modPlayer in HookResetInfoAccessories.Enumerate(player)) {
			modPlayer.ResetInfoAccessories();
		}
	}

	private static HookList HookRefreshInfoAccessoriesFromTeamPlayers = AddHook<Action<Player>>(p => p.RefreshInfoAccessoriesFromTeamPlayers);

	public static void RefreshInfoAccessoriesFromTeamPlayers(Player player, Player otherPlayer)
	{
		foreach (var modPlayer in HookRefreshInfoAccessoriesFromTeamPlayers.Enumerate(player)) {
			modPlayer.RefreshInfoAccessoriesFromTeamPlayers(otherPlayer);
		}
	}

	/// <summary>
	/// Resets <see cref="Player.statLifeMax"/> and <see cref="Player.statManaMax"/> to their expected values by vanilla
	/// </summary>
	/// <param name="player"></param>
	public static void ResetMaxStatsToVanilla(Player player)
	{
		player.statLifeMax = 100 + player.ConsumedLifeCrystals * 20 + player.ConsumedLifeFruit * 5;
		player.statManaMax = 20 + player.ConsumedManaCrystals * 20;
	}

	private delegate void DelegateModifyMaxStats(out StatModifier health, out StatModifier mana);
	private static HookList HookModifyMaxStats = AddHook<DelegateModifyMaxStats>(p => p.ModifyMaxStats);

	/// <summary>
	/// Reset this player's <see cref="Player.statLifeMax"/> and <see cref="Player.statManaMax"/> to their vanilla defaults,
	/// applies <see cref="ModPlayer.ModifyMaxStats(out StatModifier, out StatModifier)"/> to them,
	/// then modifies <see cref="Player.statLifeMax2"/> and <see cref="Player.statManaMax2"/>
	/// </summary>
	/// <param name="player"></param>
	public static void ModifyMaxStats(Player player)
	{
		ResetMaxStatsToVanilla(player);

		StatModifier cumulativeHealth = StatModifier.Default;
		StatModifier cumulativeMana = StatModifier.Default;

		foreach (var modPlayer in HookModifyMaxStats.Enumerate(player)) {
			modPlayer.ModifyMaxStats(out StatModifier health, out StatModifier mana);

			cumulativeHealth = cumulativeHealth.CombineWith(health);
			cumulativeMana = cumulativeMana.CombineWith(mana);
		}

		player.statLifeMax = (int)cumulativeHealth.ApplyTo(player.statLifeMax);
		player.statManaMax = (int)cumulativeMana.ApplyTo(player.statManaMax);
	}

	private static HookList HookUpdateDead = AddHook<Action>(p => p.UpdateDead);

	public static void UpdateDead(Player player)
	{
		foreach (var modPlayer in HookUpdateDead.Enumerate(player)) {
			try { modPlayer.UpdateDead(); } catch { }
		}
	}

	public static void SetStartInventory(Player player, IList<Item> items)
	{
		if (items.Count <= 50) {
			for (int k = 0; k < 50; k++)
				if (k < items.Count)
					player.inventory[k] = items[k];
				else
					player.inventory[k].SetDefaults();
		}
		else {
			for (int k = 0; k < 49; k++) {
				player.inventory[k] = items[k];
			}
			Item bag = new Item();
			bag.SetDefaults(ModContent.ItemType<StartBag>());
			for (int k = 49; k < items.Count; k++) {
				((StartBag)bag.ModItem).AddItem(items[k]);
			}
			player.inventory[49] = bag;
		}
	}

	private static HookList HookPreSavePlayer = AddHook<Action>(p => p.PreSavePlayer);

	public static void PreSavePlayer(Player player)
	{
		foreach (var modPlayer in HookPreSavePlayer.Enumerate(player)) {
			modPlayer.PreSavePlayer();
		}
	}

	private static HookList HookPostSavePlayer = AddHook<Action>(p => p.PostSavePlayer);

	public static void PostSavePlayer(Player player)
	{
		foreach (var modPlayer in HookPostSavePlayer.Enumerate(player)) {
			modPlayer.PostSavePlayer();
		}
	}

	private static HookList HookCopyClientState = AddHook<Action<ModPlayer>>(p => p.CopyClientState);

	public static void CopyClientState(Player player, Player targetCopy)
	{
		foreach (var modPlayer in HookCopyClientState.Enumerate(player)) {
			try { modPlayer.CopyClientState(targetCopy.modPlayers[modPlayer.Index]); } catch { }
		}
	}

	private static HookList HookSyncPlayer = AddHook<Action<int, int, bool>>(p => p.SyncPlayer);

	public static void SyncPlayer(Player player, int toWho, int fromWho, bool newPlayer)
	{
		foreach (var modPlayer in HookSyncPlayer.Enumerate(player)) {
			try { modPlayer.SyncPlayer(toWho, fromWho, newPlayer); } catch { }
		}
	}

	private static HookList HookSendClientChanges = AddHook<Action<ModPlayer>>(p => p.SendClientChanges);

	public static void SendClientChanges(Player player, Player clientPlayer)
	{
		foreach (var modPlayer in HookSendClientChanges.Enumerate(player)) {
			try { modPlayer.SendClientChanges(clientPlayer.modPlayers[modPlayer.Index]); } catch { }
		}
	}

	private static HookList HookUpdateBadLifeRegen = AddHook<Action>(p => p.UpdateBadLifeRegen);

	public static void UpdateBadLifeRegen(Player player)
	{
		foreach (var modPlayer in HookUpdateBadLifeRegen.Enumerate(player)) {
			try { modPlayer.UpdateBadLifeRegen(); } catch { }
		}
	}

	private static HookList HookUpdateLifeRegen = AddHook<Action>(p => p.UpdateLifeRegen);

	public static void UpdateLifeRegen(Player player)
	{
		foreach (var modPlayer in HookUpdateLifeRegen.Enumerate(player)) {
			try { modPlayer.UpdateLifeRegen(); } catch { }
		}
	}

	private delegate void DelegateNaturalLifeRegen(ref float regen);
	private static HookList HookNaturalLifeRegen = AddHook<DelegateNaturalLifeRegen>(p => p.NaturalLifeRegen);

	public static void NaturalLifeRegen(Player player, ref float regen)
	{
		foreach (var modPlayer in HookNaturalLifeRegen.Enumerate(player)) {
			try { modPlayer.NaturalLifeRegen(ref regen); } catch { }
		}
	}

	private static HookList HookUpdateAutopause = AddHook<Action>(p => p.UpdateAutopause);

	public static void UpdateAutopause(Player player)
	{
		foreach (var modPlayer in HookUpdateAutopause.Enumerate(player)) {
			modPlayer.UpdateAutopause();
		}
	}

	private static HookList HookPreUpdate = AddHook<Action>(p => p.PreUpdate);

	public static void PreUpdate(Player player)
	{
		foreach (var modPlayer in HookPreUpdate.Enumerate(player)) {
			try { modPlayer.PreUpdate(); } catch { }
		}
	}

	private static HookList HookSetControls = AddHook<Action>(p => p.SetControls);

	public static void SetControls(Player player)
	{
		foreach (var modPlayer in HookSetControls.Enumerate(player)) {
			try { modPlayer.SetControls(); } catch { }
		}
	}

	private static HookList HookPreUpdateBuffs = AddHook<Action>(p => p.PreUpdateBuffs);

	public static void PreUpdateBuffs(Player player)
	{
		foreach (var modPlayer in HookPreUpdateBuffs.Enumerate(player)) {
			try { modPlayer.PreUpdateBuffs(); } catch { }
		}
	}

	private static HookList HookPostUpdateBuffs = AddHook<Action>(p => p.PostUpdateBuffs);

	public static void PostUpdateBuffs(Player player)
	{
		foreach (var modPlayer in HookPostUpdateBuffs.Enumerate(player)) {
			try { modPlayer.PostUpdateBuffs(); } catch { }
		}
	}

	private delegate void DelegateUpdateEquips();
	private static HookList HookUpdateEquips = AddHook<DelegateUpdateEquips>(p => p.UpdateEquips);

	public static void UpdateEquips(Player player)
	{
		foreach (var modPlayer in HookUpdateEquips.Enumerate(player)) {
			try { modPlayer.UpdateEquips(); } catch { }
		}
	}

	private static HookList HookPostUpdateEquips = AddHook<Action>(p => p.PostUpdateEquips);

	public static void PostUpdateEquips(Player player)
	{
		foreach (var modPlayer in HookPostUpdateEquips.Enumerate(player)) {
			try { modPlayer.PostUpdateEquips(); } catch { }
		}
	}

	private static HookList HookUpdateVisibleAccessories = AddHook<Action>(p => p.UpdateVisibleAccessories);

	public static void UpdateVisibleAccessories(Player player)
	{
		foreach (var modPlayer in HookUpdateVisibleAccessories.Enumerate(player)) {
			try { modPlayer.UpdateVisibleAccessories(); } catch { }
		}
	}

	private static HookList HookUpdateVisibleVanityAccessories = AddHook<Action>(p => p.UpdateVisibleVanityAccessories);

	public static void UpdateVisibleVanityAccessories(Player player)
	{
		foreach (var modPlayer in HookUpdateVisibleVanityAccessories.Enumerate(player)) {
			try { modPlayer.UpdateVisibleVanityAccessories(); } catch { }
		}
	}

	private static HookList HookUpdateDyes = AddHook<Action>(p => p.UpdateDyes);

	public static void UpdateDyes(Player player)
	{
		foreach (var modPlayer in HookUpdateDyes.Enumerate(player)) {
			try { modPlayer.UpdateDyes(); } catch { }
		}
	}

	private static HookList HookPostUpdateMiscEffects = AddHook<Action>(p => p.PostUpdateMiscEffects);

	public static void PostUpdateMiscEffects(Player player)
	{
		foreach (var modPlayer in HookPostUpdateMiscEffects.Enumerate(player)) {
			try { modPlayer.PostUpdateMiscEffects(); } catch { }
		}
	}

	private static HookList HookPostUpdateRunSpeeds = AddHook<Action>(p => p.PostUpdateRunSpeeds);

	public static void PostUpdateRunSpeeds(Player player)
	{
		foreach (var modPlayer in HookPostUpdateRunSpeeds.Enumerate(player)) {
			try { modPlayer.PostUpdateRunSpeeds(); } catch { }
		}
	}

	private static HookList HookPreUpdateMovement = AddHook<Action>(p => p.PreUpdateMovement);

	public static void PreUpdateMovement(Player player)
	{
		foreach (var modPlayer in HookPreUpdateMovement.Enumerate(player)) {
			try { modPlayer.PreUpdateMovement(); } catch { }
		}
	}

	private static HookList HookPostUpdate = AddHook<Action>(p => p.PostUpdate);

	public static void PostUpdate(Player player)
	{
		foreach (var modPlayer in HookPostUpdate.Enumerate(player)) {
			try { modPlayer.PostUpdate(); } catch { }
		}
	}

	private delegate void DelegateModifyExtraJumpDuration(ExtraJump jump, ref float duration);
	private static HookList HookModifyExtraJumpDurationMultiplier = AddHook<DelegateModifyExtraJumpDuration>(p => p.ModifyExtraJumpDurationMultiplier);

	public static void ModifyExtraJumpDurationMultiplier(ExtraJump jump, Player player, ref float duration)
	{
		foreach (var modPlayer in HookModifyExtraJumpDurationMultiplier.Enumerate(player)) {
			try { modPlayer.ModifyExtraJumpDurationMultiplier(jump, ref duration); } catch { }
		}
	}

	private static HookList HookCanStartExtraJump = AddHook<Func<ExtraJump, bool>>(p => p.CanStartExtraJump);

	public static bool CanStartExtraJump(ExtraJump jump, Player player)
	{
		foreach (var modPlayer in HookCanStartExtraJump.Enumerate(player)) {
			try {
				if (!modPlayer.CanStartExtraJump(jump))
					return false;
			} catch { }
		}

		return true;
	}

	private delegate void DelegateOnExtraJumpStarted(ExtraJump jump, ref bool playSound);
	private static HookList HookOnExtraJumpStarted = AddHook<DelegateOnExtraJumpStarted>(p => p.OnExtraJumpStarted);

	public static void OnExtraJumpStarted(ExtraJump jump, Player player, ref bool playSound)
	{
		foreach (var modPlayer in HookOnExtraJumpStarted.Enumerate(player)) {
			try { modPlayer.OnExtraJumpStarted(jump, ref playSound); } catch { }
		}
	}

	private static HookList HookOnExtraJumpEnded = AddHook<Action<ExtraJump>>(p => p.OnExtraJumpEnded);

	public static void OnExtraJumpEnded(ExtraJump jump, Player player)
	{
		foreach (var modPlayer in HookOnExtraJumpEnded.Enumerate(player)) {
			try { modPlayer.OnExtraJumpEnded(jump); } catch { }
		}
	}

	private static HookList HookOnExtraJumpRefreshed = AddHook<Action<ExtraJump>>(p => p.OnExtraJumpRefreshed);

	public static void OnExtraJumpRefreshed(ExtraJump jump, Player player)
	{
		foreach (var modPlayer in HookOnExtraJumpRefreshed.Enumerate(player)) {
			try { modPlayer.OnExtraJumpRefreshed(jump); } catch { }
		}
	}

	private static HookList HookExtraJumpVisuals = AddHook<Action<ExtraJump>>(p => p.ExtraJumpVisuals);

	public static void ExtraJumpVisuals(ExtraJump jump, Player player)
	{
		foreach (var modPlayer in HookExtraJumpVisuals.Enumerate(player)) {
			try { modPlayer.ExtraJumpVisuals(jump); } catch { }
		}
	}

	private static HookList HookCanShowExtraJumpVisuals = AddHook<Func<ExtraJump, bool>>(p => p.CanShowExtraJumpVisuals);

	public static bool CanShowExtraJumpVisuals(ExtraJump jump, Player player)
	{
		foreach (var modPlayer in HookCanShowExtraJumpVisuals.Enumerate(player)) {
			try {
				if (!modPlayer.CanShowExtraJumpVisuals(jump))
					return false;
			} catch { }
		}

		return true;
	}

	private static HookList HookFrameEffects = AddHook<Action>(p => p.FrameEffects);

	public static void FrameEffects(Player player)
	{
		foreach (var modPlayer in HookFrameEffects.Enumerate(player)) {
			try { modPlayer.FrameEffects(); } catch { }
		}
	}

	private static HookList HookImmuneTo = AddHook<Func<PlayerDeathReason, int, bool, bool>>(p => p.ImmuneTo);
	public static bool ImmuneTo(Player player, PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable)
	{
		foreach (var modPlayer in HookImmuneTo.Enumerate(player)) {
			if (modPlayer.ImmuneTo(damageSource, cooldownCounter, dodgeable))
				return true;
		}

		return false;
	}

	private static HookList HookFreeDodge = AddHook<Func<Player.HurtInfo, bool>>(p => p.FreeDodge);
	public static bool FreeDodge(Player player, in Player.HurtInfo info)
	{
		foreach (var modPlayer in HookFreeDodge.Enumerate(player)) {
			if (modPlayer.FreeDodge(info))
				return true;
		}

		return false;
	}

	private static HookList HookConsumableDodge = AddHook<Func<Player.HurtInfo, bool>>(p => p.ConsumableDodge);
	public static bool ConsumableDodge(Player player, in Player.HurtInfo info)
	{
		foreach (var modPlayer in HookConsumableDodge.Enumerate(player)) {
			if (modPlayer.ConsumableDodge(info))
				return true;
		}

		return false;
	}

	private delegate void DelegateModifyHurt(ref Player.HurtModifiers modifiers);
	private static HookList HookModifyHurt = AddHook<DelegateModifyHurt>(p => p.ModifyHurt);

	public static void ModifyHurt(Player player, ref Player.HurtModifiers modifiers)
	{
		// safe to get source entity, as hurt is not synchronized across the net
		if (modifiers.DamageSource.TryGetCausingEntity(out Entity sourceEntity)) {
			switch (sourceEntity) {
				case Projectile proj:
					CombinedHooks.ModifyHitByProjectile(player, proj, ref modifiers);
					break;
				case NPC npc:
					CombinedHooks.ModifyHitByNPC(player, npc, ref modifiers);
					break;
				case Player sourcePlayer when modifiers.DamageSource.SourceItem is Item item && modifiers.PvP:
					ItemLoader.ModifyHitPvp(item, sourcePlayer, player, ref modifiers);
					break;
			}
		}

		foreach (var modPlayer in HookModifyHurt.Enumerate(player)) {
			try { modPlayer.ModifyHurt(ref modifiers); } catch { }
		}
	}

	private static HookList HookHurt = AddHook<Action<Player.HurtInfo>>(p => p.OnHurt);

	public static void OnHurt(Player player, Player.HurtInfo info)
	{
		// source entity is only safe to retrieve if the hit is happening 'locally'
		if (info.DamageSource.TryGetCausingEntity(out Entity sourceEntity)) {
			switch (sourceEntity) {
				case Projectile proj when player == Main.LocalPlayer:
					CombinedHooks.OnHitByProjectile(player, proj, info);
					break;
				case NPC npc when player == Main.LocalPlayer:
					CombinedHooks.OnHitByNPC(player, npc, info);
					break;
				case Player sourcePlayer when info.DamageSource.SourceItem is Item item && info.PvP:
					ItemLoader.OnHitPvp(item, sourcePlayer, player, info);
					break;
			}
		}

		foreach (var modPlayer in HookHurt.Enumerate(player)) {
			modPlayer.OnHurt(info);
		}
	}

	private static HookList HookPostHurt = AddHook<Action<Player.HurtInfo>>(p => p.PostHurt);

	public static void PostHurt(Player player, Player.HurtInfo info)
	{
		foreach (var modPlayer in HookPostHurt.Enumerate(player)) {
			modPlayer.PostHurt(info);
		}
	}

	private delegate bool DelegatePreKill(double damage, int hitDirection, bool pvp, ref bool playSound,
		ref bool genGore, ref PlayerDeathReason damageSource);
	private static HookList HookPreKill = AddHook<DelegatePreKill>(p => p.PreKill);

	public static bool PreKill(Player player, double damage, int hitDirection, bool pvp, ref bool playSound,
		ref bool genGore, ref PlayerDeathReason damageSource)
	{
		bool ret = true;
		foreach (var modPlayer in HookPreKill.Enumerate(player)) {
			ret &= modPlayer.PreKill(damage, hitDirection, pvp, ref playSound, ref genGore, ref damageSource);
		}
		return ret;
	}

	private static HookList HookKill = AddHook<Action<double, int, bool, PlayerDeathReason>>(p => p.Kill);

	public static void Kill(Player player, double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
	{
		foreach (var modPlayer in HookKill.Enumerate(player)) {
			try { modPlayer.Kill(damage, hitDirection, pvp, damageSource); } catch { }
		}
	}

	private delegate bool DelegatePreModifyLuck(ref float luck);
	private static HookList HookPreModifyLuck = AddHook<DelegatePreModifyLuck>(p => p.PreModifyLuck);

	public static bool PreModifyLuck(Player player, ref float luck)
	{
		bool ret = true;
		foreach (var modPlayer in HookPreModifyLuck.Enumerate(player)) {
			ret &= modPlayer.PreModifyLuck(ref luck);
		}
		return ret;
	}

	private delegate void DelegateModifyLuck(ref float luck);
	private static HookList HookModifyLuck = AddHook<DelegateModifyLuck>(p => p.ModifyLuck);

	public static void ModifyLuck(Player player, ref float luck)
	{
		foreach (var modPlayer in HookModifyLuck.Enumerate(player)) {
			modPlayer.ModifyLuck(ref luck);
		}
	}

	private static HookList HookPreItemCheck = AddHook<Func<bool>>(p => p.PreItemCheck);

	public static bool PreItemCheck(Player player)
	{
		bool ret = true;
		foreach (var modPlayer in HookPreItemCheck.Enumerate(player)) {
			try { ret &= modPlayer.PreItemCheck(); } catch { }
		}
		return ret;
	}

	private static HookList HookPostItemCheck = AddHook<Action>(p => p.PostItemCheck);

	public static void PostItemCheck(Player player)
	{
		foreach (var modPlayer in HookPostItemCheck.Enumerate(player)) {
			try { modPlayer.PostItemCheck(); } catch { }
		}
	}

	private static HookList HookUseTimeMultiplier = AddHook<Func<Item, float>>(p => p.UseTimeMultiplier);

	public static float UseTimeMultiplier(Player player, Item item)
	{
		float multiplier = 1f;

		if (item.IsAir)
			return multiplier;

		foreach (var modPlayer in HookUseTimeMultiplier.Enumerate(player)) {
			multiplier *= modPlayer.UseTimeMultiplier(item);
		}

		return multiplier;
	}

	private static HookList HookUseAnimationMultiplier = AddHook<Func<Item, float>>(p => p.UseAnimationMultiplier);

	public static float UseAnimationMultiplier(Player player, Item item)
	{
		float multiplier = 1f;

		if (item.IsAir)
			return multiplier;

		foreach (var modPlayer in HookUseAnimationMultiplier.Enumerate(player)) {
			multiplier *= modPlayer.UseAnimationMultiplier(item);
		}

		return multiplier;
	}

	private static HookList HookUseSpeedMultiplier = AddHook<Func<Item, float>>(p => p.UseSpeedMultiplier);

	public static float UseSpeedMultiplier(Player player, Item item)
	{
		float multiplier = 1f;

		if (item.IsAir)
			return multiplier;

		foreach (var modPlayer in HookUseSpeedMultiplier.Enumerate(player)) {
			multiplier *= modPlayer.UseSpeedMultiplier(item);
		}

		return multiplier;
	}

	private delegate void DelegateGetHealLife(Item item, bool quickHeal, ref int healValue);
	private static HookList HookGetHealLife = AddHook<DelegateGetHealLife>(p => p.GetHealLife);

	public static void GetHealLife(Player player, Item item, bool quickHeal, ref int healValue)
	{
		if (item.IsAir)
			return;

		foreach (var modPlayer in HookGetHealLife.Enumerate(player)) {
			modPlayer.GetHealLife(item, quickHeal, ref healValue);
		}
	}

	private delegate void DelegateGetHealMana(Item item, bool quickHeal, ref int healValue);
	private static HookList HookGetHealMana = AddHook<DelegateGetHealMana>(p => p.GetHealMana);

	public static void GetHealMana(Player player, Item item, bool quickHeal, ref int healValue)
	{
		if (item.IsAir)
			return;

		foreach (var modPlayer in HookGetHealMana.Enumerate(player)) {
			modPlayer.GetHealMana(item, quickHeal, ref healValue);
		}
	}

	private delegate void DelegateModifyManaCost(Item item, ref float reduce, ref float mult);
	private static HookList HookModifyManaCost = AddHook<DelegateModifyManaCost>(p => p.ModifyManaCost);

	public static void ModifyManaCost(Player player, Item item, ref float reduce, ref float mult)
	{
		if (item.IsAir)
			return;

		foreach (var modPlayer in HookModifyManaCost.Enumerate(player)) {
			modPlayer.ModifyManaCost(item, ref reduce, ref mult);
		}
	}

	private static HookList HookOnMissingMana = AddHook<Action<Item, int>>(p => p.OnMissingMana);

	public static void OnMissingMana(Player player, Item item, int manaNeeded)
	{
		if (item.IsAir)
			return;

		foreach (var modPlayer in HookOnMissingMana.Enumerate(player)) {
			modPlayer.OnMissingMana(item, manaNeeded);
		}
	}

	private static HookList HookOnConsumeMana = AddHook<Action<Item, int>>(p => p.OnConsumeMana);

	public static void OnConsumeMana(Player player, Item item, int manaConsumed)
	{
		if (item.IsAir)
			return;

		foreach (var modPlayer in HookOnConsumeMana.Enumerate(player)) {
			modPlayer.OnConsumeMana(item, manaConsumed);
		}
	}

	private delegate bool DelegateApplyPotionDelay(Item item, int potionDelay);
	private static HookList HookApplyPotionDelay = AddHook<DelegateApplyPotionDelay>(p => p.ApplyPotionDelay);

	public static bool ApplyPotionDelay(Item item, Player player, int potionDelay)
	{
		bool flag = true;

		foreach (var modPlayer in HookApplyPotionDelay.Enumerate(player)) {
			flag &= modPlayer.ApplyPotionDelay(item, potionDelay);
		}

		return flag;
	}

	private delegate void DelegateModifyWeaponDamage(Item item, ref StatModifier damage);
	private static HookList HookModifyWeaponDamage = AddHook<DelegateModifyWeaponDamage>(p => p.ModifyWeaponDamage);
	/// <summary>
	/// Calls ModItem.HookModifyWeaponDamage, then all GlobalItem.HookModifyWeaponDamage hooks.
	/// </summary>
	public static void ModifyWeaponDamage(Player player, Item item, ref StatModifier damage)
	{
		if (item.IsAir)
			return;

		foreach (var modPlayer in HookModifyWeaponDamage.Enumerate(player)) {
			modPlayer.ModifyWeaponDamage(item, ref damage);
		}
	}

	private static HookList HookProcessTriggers = AddHook<Action<TriggersSet>>(p => p.ProcessTriggers);

	public static void ProcessTriggers(Player player, TriggersSet triggersSet)
	{
		foreach (var modPlayer in HookProcessTriggers.Enumerate(player)) {
			try { modPlayer.ProcessTriggers(triggersSet); }	catch { }
		}
	}

	private delegate void DelegateModifyWeaponKnockback(Item item, ref StatModifier knockback);
	private static HookList HookModifyWeaponKnockback = AddHook<DelegateModifyWeaponKnockback>(p => p.ModifyWeaponKnockback);

	public static void ModifyWeaponKnockback(Player player, Item item, ref StatModifier knockback)
	{
		if (item.IsAir)
			return;

		foreach (var modPlayer in HookModifyWeaponKnockback.Enumerate(player)) {
			modPlayer.ModifyWeaponKnockback(item, ref knockback);
		}
	}

	private delegate void DelegateModifyWeaponCrit(Item item, ref float crit);
	private static HookList HookModifyWeaponCrit = AddHook<DelegateModifyWeaponCrit>(p => p.ModifyWeaponCrit);

	public static void ModifyWeaponCrit(Player player, Item item, ref float crit)
	{
		if (item.IsAir) return;
		foreach (var modPlayer in HookModifyWeaponCrit.Enumerate(player)) {
			modPlayer.ModifyWeaponCrit(item, ref crit);
		}
	}

	private static HookList HookCanConsumeAmmo = AddHook<Func<Item, Item, bool>>(p => p.CanConsumeAmmo);

	public static bool CanConsumeAmmo(Player player, Item weapon, Item ammo)
	{
		foreach (var modPlayer in HookCanConsumeAmmo.Enumerate(player)) {
			if (!modPlayer.CanConsumeAmmo(weapon, ammo)) {
				return false;
			}
		}
		return true;
	}

	private static HookList HookOnConsumeAmmo = AddHook<Action<Item, Item>>(p => p.OnConsumeAmmo);

	public static void OnConsumeAmmo(Player player, Item weapon, Item ammo)
	{
		foreach (var modPlayer in HookOnConsumeAmmo.Enumerate(player))
			modPlayer.OnConsumeAmmo(weapon, ammo);
	}

	private static HookList HookCanShoot = AddHook<Func<Item, bool>>(p => p.CanShoot);

	public static bool CanShoot(Player player, Item item)
	{
		foreach (var modPlayer in HookCanShoot.Enumerate(player)) {
			if (!modPlayer.CanShoot(item))
				return false;
		}

		return true;
	}

	private delegate void DelegateModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback);
	private static HookList HookModifyShootStats = AddHook<DelegateModifyShootStats>(p => p.ModifyShootStats);

	public static void ModifyShootStats(Player player, Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
	{
		foreach (var modPlayer in HookModifyShootStats.Enumerate(player)) {
			modPlayer.ModifyShootStats(item, ref position, ref velocity, ref type, ref damage, ref knockback);
		}
	}

	private static HookList HookShoot = AddHook<Func<Item, EntitySource_ItemUse_WithAmmo, Vector2, Vector2, int, int, float, bool>>(p => p.Shoot);

	public static bool Shoot(Player player, Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		bool defaultResult = true;

		foreach (var modPlayer in HookShoot.Enumerate(player)) {
			defaultResult &= modPlayer.Shoot(item, source, position, velocity, type, damage, knockback);
		}

		return defaultResult;
	}

	private static HookList HookMeleeEffects = AddHook<Action<Item, Rectangle>>(p => p.MeleeEffects);

	public static void MeleeEffects(Player player, Item item, Rectangle hitbox)
	{
		foreach (var modPlayer in HookMeleeEffects.Enumerate(player)) {
			modPlayer.MeleeEffects(item, hitbox);
		}
	}

	private static HookList HookEmitEnchantmentVisualsAt = AddHook<Action<Projectile, Vector2, int, int>>(p => p.EmitEnchantmentVisualsAt);

	public static void EmitEnchantmentVisualsAt(Player player, Projectile projectile, Vector2 boxPosition, int boxWidth, int boxHeight)
	{
		foreach (var modPlayer in HookEmitEnchantmentVisualsAt.Enumerate(player)) {
			modPlayer.EmitEnchantmentVisualsAt(projectile, boxPosition, boxWidth, boxHeight);
		}
	}

	private static HookList HookCanCatchNPC = AddHook<Func<NPC, Item, bool?>>(p => p.CanCatchNPC);

	public static bool? CanCatchNPC(Player player, NPC target, Item item)
	{
		bool? returnValue = null;
		foreach (var modPlayer in HookCanCatchNPC.Enumerate(player)) {
			bool? canCatch = modPlayer.CanCatchNPC(target, item);
			if (canCatch.HasValue) {
				if (!canCatch.Value)
					return false;

				returnValue = true;
			}
		}
		return returnValue;
	}

	private static HookList HookOnCatchNPC = AddHook<Action<NPC, Item, bool>>(p => p.OnCatchNPC);

	public static void OnCatchNPC(Player player, NPC target, Item item, bool failed)
	{
		foreach (var modPlayer in HookOnCatchNPC.Enumerate(player)) {
			modPlayer.OnCatchNPC(target, item, failed);
		}
	}

	private delegate void DelegateModifyItemScale(Item item, ref float scale);
	private static HookList HookModifyItemScale = AddHook<DelegateModifyItemScale>(p => p.ModifyItemScale);

	public static void ModifyItemScale(Player player, Item item, ref float scale)
	{
		foreach (var modPlayer in HookModifyItemScale.Enumerate(player)) {
			modPlayer.ModifyItemScale(item, ref scale);
		}
	}

	private static HookList HookOnHitAnything = AddHook<Action<float, float, Entity>>(p => p.OnHitAnything);

	public static void OnHitAnything(Player player, float x, float y, Entity victim)
	{
		foreach (var modPlayer in HookOnHitAnything.Enumerate(player)) {
			modPlayer.OnHitAnything(x, y, victim);
		}
	}

	private static HookList HookCanHitNPC = AddHook<Func<NPC, bool>>(p => p.CanHitNPC);
	public static bool CanHitNPC(Player player, NPC target)
	{
		foreach (var modPlayer in HookCanHitNPC.Enumerate(player))
			if (!modPlayer.CanHitNPC(target))
				return false;

		return true;
	}

	private static HookList HookCanCollideNPCWithItem = AddHook<Func<Item, Rectangle, NPC, bool?>>(p => p.CanMeleeAttackCollideWithNPC);

	public static bool? CanMeleeAttackCollideWithNPC(Player player, Item item, Rectangle meleeAttackHitbox, NPC target)
	{
		bool? flag = null;

		foreach (var modPlayer in HookCanCollideNPCWithItem.Enumerate(player)) {
			bool? canHit = modPlayer.CanMeleeAttackCollideWithNPC(item, meleeAttackHitbox, target);

			if (canHit.HasValue) {
				if (!canHit.Value) {
					return false;
				}

				flag = true;
			}
		}

		return flag;
	}

	private delegate void DelegateModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers);
	private static HookList HookModifyHitNPC = AddHook<DelegateModifyHitNPC>(p => p.ModifyHitNPC);

	public static void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers)
	{
		foreach (var modPlayer in HookModifyHitNPC.Enumerate(player)) {
			modPlayer.ModifyHitNPC(target, ref modifiers);
		}
	}

	private static HookList HookOnHitNPC = AddHook<Action<NPC, NPC.HitInfo, int>>(p => p.OnHitNPC);

	public static void OnHitNPC(Player player, NPC target, in NPC.HitInfo hit, int damageDone)
	{
		foreach (var modPlayer in HookOnHitNPC.Enumerate(player)) {
			modPlayer.OnHitNPC(target, hit, damageDone);
		}
	}

	private static HookList HookCanHitNPCWithItem = AddHook<Func<Item, NPC, bool?>>(p => p.CanHitNPCWithItem);
	public static bool? CanHitNPCWithItem(Player player, Item item, NPC target)
	{
		if (!CanHitNPC(player, target))
			return false;

		bool? ret = null;
		foreach (var modPlayer in HookCanHitNPCWithItem.Enumerate(player)) {
			if (modPlayer.CanHitNPCWithItem(item, target) is bool b) {
				if (!b)
					return false;

				ret = true;
			}
		}

		return ret;
	}

	private delegate void DelegateModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers);
	private static HookList HookModifyHitNPCWithItem = AddHook<DelegateModifyHitNPCWithItem>(p => p.ModifyHitNPCWithItem);

	public static void ModifyHitNPCWithItem(Player player, Item item, NPC target, ref NPC.HitModifiers modifiers)
	{
		ModifyHitNPC(player, target, ref modifiers);
		foreach (var modPlayer in HookModifyHitNPCWithItem.Enumerate(player)) {
			modPlayer.ModifyHitNPCWithItem(item, target, ref modifiers);
		}
	}

	private static HookList HookOnHitNPCWithItem = AddHook<Action<Item, NPC, NPC.HitInfo, int>>(p => p.OnHitNPCWithItem);

	public static void OnHitNPCWithItem(Player player, Item item, NPC target, in NPC.HitInfo hit, int damageDone)
	{
		OnHitNPC(player, target, hit, damageDone);
		foreach (var modPlayer in HookOnHitNPCWithItem.Enumerate(player)) {
			modPlayer.OnHitNPCWithItem(item, target, hit, damageDone);
		}
	}

	private static HookList HookCanHitNPCWithProj = AddHook<Func<Projectile, NPC, bool?>>(p => p.CanHitNPCWithProj);

	public static bool? CanHitNPCWithProj(Player player, Projectile proj, NPC target)
	{
		if (!CanHitNPC(player, target))
			return false;

		bool? ret = null;
		foreach (var modPlayer in HookCanHitNPCWithProj.Enumerate(player)) {
			if (modPlayer.CanHitNPCWithProj(proj, target) is bool b) {
				if (!b)
					return false;

				ret = true;
			}
		}
		return ret;
	}

	private delegate void DelegateModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers);
	private static HookList HookModifyHitNPCWithProj = AddHook<DelegateModifyHitNPCWithProj>(p => p.ModifyHitNPCWithProj);

	public static void ModifyHitNPCWithProj(Player player, Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
	{
		ModifyHitNPC(player, target, ref modifiers);
		foreach (var modPlayer in HookModifyHitNPCWithProj.Enumerate(player)) {
			modPlayer.ModifyHitNPCWithProj(proj, target, ref modifiers);
		}
	}

	private static HookList HookOnHitNPCWithProj = AddHook<Action<Projectile, NPC, NPC.HitInfo, int>>(p => p.OnHitNPCWithProj);

	public static void OnHitNPCWithProj(Player player, Projectile proj, NPC target, in NPC.HitInfo hit, int damageDone)
	{
		OnHitNPC(player, target, hit, damageDone);
		foreach (var modPlayer in HookOnHitNPCWithProj.Enumerate(player)) {
			modPlayer.OnHitNPCWithProj(proj, target, hit, damageDone);
		}
	}

	private static HookList HookCanHitPvp = AddHook<Func<Item, Player, bool>>(p => p.CanHitPvp);

	public static bool CanHitPvp(Player player, Item item, Player target)
	{
		foreach (var modPlayer in HookCanHitPvp.Enumerate(player)) {
			if (!modPlayer.CanHitPvp(item, target)) {
				return false;
			}
		}
		return true;
	}

	private static HookList HookCanHitPvpWithProj = AddHook<Func<Projectile, Player, bool>>(p => p.CanHitPvpWithProj);

	public static bool CanHitPvpWithProj(Projectile proj, Player target)
	{
		Player player = Main.player[proj.owner];
		foreach (var modPlayer in HookCanHitPvpWithProj.Enumerate(player)) {
			if (!modPlayer.CanHitPvpWithProj(proj, target)) {
				return false;
			}
		}
		return true;
	}

	private delegate bool DelegateCanBeHitByNPC(NPC npc, ref int cooldownSlot);
	private static HookList HookCanBeHitByNPC = AddHook<DelegateCanBeHitByNPC>(p => p.CanBeHitByNPC);

	public static bool CanBeHitByNPC(Player player, NPC npc, ref int cooldownSlot)
	{
		foreach (var modPlayer in HookCanBeHitByNPC.Enumerate(player)) {
			if (!modPlayer.CanBeHitByNPC(npc, ref cooldownSlot)) {
				return false;
			}
		}
		return true;
	}

	private delegate void DelegateModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers);
	private static HookList HookModifyHitByNPC = AddHook<DelegateModifyHitByNPC>(p => p.ModifyHitByNPC);

	public static void ModifyHitByNPC(Player player, NPC npc, ref Player.HurtModifiers modifiers)
	{
		foreach (var modPlayer in HookModifyHitByNPC.Enumerate(player)) {
			modPlayer.ModifyHitByNPC(npc, ref modifiers);
		}
	}

	private static HookList HookOnHitByNPC = AddHook<Action<NPC, Player.HurtInfo>>(p => p.OnHitByNPC);

	public static void OnHitByNPC(Player player, NPC npc, in Player.HurtInfo hurtInfo)
	{
		foreach (var modPlayer in HookOnHitByNPC.Enumerate(player)) {
			modPlayer.OnHitByNPC(npc, hurtInfo);
		}
	}

	private static HookList HookCanBeHitByProjectile = AddHook<Func<Projectile, bool>>(p => p.CanBeHitByProjectile);

	public static bool CanBeHitByProjectile(Player player, Projectile proj)
	{
		foreach (var modPlayer in HookCanBeHitByProjectile.Enumerate(player)) {
			if (!modPlayer.CanBeHitByProjectile(proj)) {
				return false;
			}
		}
		return true;
	}

	private delegate void DelegateModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers);
	private static HookList HookModifyHitByProjectile = AddHook<DelegateModifyHitByProjectile>(p => p.ModifyHitByProjectile);

	public static void ModifyHitByProjectile(Player player, Projectile proj, ref Player.HurtModifiers modifiers)
	{
		foreach (var modPlayer in HookModifyHitByProjectile.Enumerate(player)) {
			modPlayer.ModifyHitByProjectile(proj, ref modifiers);
		}
	}

	private static HookList HookOnHitByProjectile = AddHook<Action<Projectile, Player.HurtInfo>>(p => p.OnHitByProjectile);

	public static void OnHitByProjectile(Player player, Projectile proj, in Player.HurtInfo hurtInfo)
	{
		foreach (var modPlayer in HookOnHitByProjectile.Enumerate(player)) {
			modPlayer.OnHitByProjectile(proj, hurtInfo);
		}
	}

	private delegate void DelegateModifyFishingAttempt(ref FishingAttempt attempt);
	private static HookList HookModifyFishingAttempt = AddHook<DelegateModifyFishingAttempt>(p => p.ModifyFishingAttempt);

	public static void ModifyFishingAttempt(Player player, ref FishingAttempt attempt)
	{
		foreach (var modPlayer in HookModifyFishingAttempt.Enumerate(player)) {
			modPlayer.ModifyFishingAttempt(ref attempt);
		}

		attempt.rolledItemDrop = attempt.rolledEnemySpawn = 0; // Reset, modders need to use CatchFish for this
	}

	private delegate void DelegateCatchFish(FishingAttempt attempt, ref int itemDrop, ref int enemySpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition);
	private static HookList HookCatchFish = AddHook<DelegateCatchFish>(p => p.CatchFish);

	public static void CatchFish(Player player, FishingAttempt attempt, ref int itemDrop, ref int enemySpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition)
	{
		foreach (var modPlayer in HookCatchFish.Enumerate(player)) {
			modPlayer.CatchFish(attempt, ref itemDrop, ref enemySpawn, ref sonar, ref sonarPosition);
		}
	}

	private delegate void DelegateModifyCaughtFish(Item fish);
	private static HookList HookCaughtFish = AddHook<DelegateModifyCaughtFish>(p => p.ModifyCaughtFish);

	public static void ModifyCaughtFish(Player player, Item fish)
	{
		foreach (var modPlayer in HookCaughtFish.Enumerate(player)) {
			modPlayer.ModifyCaughtFish(fish);
		}
	}

	private delegate bool? DelegateCanConsumeBait(Item bait);
	private static HookList HookCanConsumeBait = AddHook<DelegateCanConsumeBait>(p => p.CanConsumeBait);

	public static bool? CanConsumeBait(Player player, Item bait)
	{
		bool? ret = null;
		foreach (var modPlayer in HookCaughtFish.Enumerate(player)) {
			if (modPlayer.CanConsumeBait(bait) is bool b)
				ret = (ret ?? true) && b;
		}
		return ret;
	}

	private delegate void DelegateGetFishingLevel(Item fishingRod, Item bait, ref float fishingLevel);
	private static HookList HookGetFishingLevel = AddHook<DelegateGetFishingLevel>(p => p.GetFishingLevel);

	public static void GetFishingLevel(Player player, Item fishingRod, Item bait, ref float fishingLevel)
	{
		foreach (var modPlayer in HookGetFishingLevel.Enumerate(player)) {
			modPlayer.GetFishingLevel(fishingRod, bait, ref fishingLevel);
		}
	}

	private static HookList HookAnglerQuestReward = AddHook<Action<float, List<Item>>>(p => p.AnglerQuestReward);

	public static void AnglerQuestReward(Player player, float rareMultiplier, List<Item> rewardItems)
	{
		foreach (var modPlayer in HookAnglerQuestReward.Enumerate(player)) {
			modPlayer.AnglerQuestReward(rareMultiplier, rewardItems);
		}
	}

	private static HookList HookGetDyeTraderReward = AddHook<Action<List<int>>>(p => p.GetDyeTraderReward);

	public static void GetDyeTraderReward(Player player, List<int> rewardPool)
	{
		foreach (var modPlayer in HookGetDyeTraderReward.Enumerate(player)) {
			modPlayer.GetDyeTraderReward(rewardPool);
		}
	}

	private delegate void DelegateDrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright);
	private static HookList HookDrawEffects = AddHook<DelegateDrawEffects>(p => p.DrawEffects);

	public static void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
	{
		var player = drawInfo.drawPlayer;

		foreach (var modPlayer in HookDrawEffects.Enumerate(player)) {
			try { modPlayer.DrawEffects(drawInfo, ref r, ref g, ref b, ref a, ref fullBright); } catch { }
		}
	}

	private delegate void DelegateModifyDrawInfo(ref PlayerDrawSet drawInfo);
	private static HookList HookModifyDrawInfo = AddHook<DelegateModifyDrawInfo>(p => p.ModifyDrawInfo);

	public static void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
	{
		var player = drawInfo.drawPlayer;

		foreach (var modPlayer in HookModifyDrawInfo.Enumerate(player)) {
			try { modPlayer.ModifyDrawInfo(ref drawInfo); } catch { }
		}
	}

	private delegate void DelegateTransformDrawData(ref PlayerDrawSet drawInfo);
	private static HookList HookTransformDrawData = AddHook<DelegateTransformDrawData>(p => p.TransformDrawData);

	public static void TransformDrawData(ref PlayerDrawSet drawInfo)
	{
		var player = drawInfo.drawPlayer;

		foreach (var modPlayer in HookTransformDrawData.Enumerate(player)) {
			try { modPlayer.TransformDrawData(ref drawInfo); }
			catch { }
		}
	}

	private static HookList HookModifyDrawLayerOrdering = AddHook<Action<IDictionary<PlayerDrawLayer, PlayerDrawLayer.Position>>>(p => p.ModifyDrawLayerOrdering);

	public static void ModifyDrawLayerOrdering(IDictionary<PlayerDrawLayer, PlayerDrawLayer.Position> positions)
	{
		foreach (var modPlayer in HookModifyDrawLayerOrdering.Enumerate()) {
			try { modPlayer.ModifyDrawLayerOrdering(positions); } catch { }
		}
	}

	private static HookList HookModifyDrawLayers = AddHook<Action<PlayerDrawSet>>(p => p.HideDrawLayers);

	public static void HideDrawLayers(PlayerDrawSet drawInfo)
	{
		var player = drawInfo.drawPlayer;

		foreach (var modPlayer in HookModifyDrawLayers.Enumerate(player)) {
			try { modPlayer.HideDrawLayers(drawInfo); } catch { }
		}
	}

	private static HookList HookModifyScreenPosition = AddHook<Action>(p => p.ModifyScreenPosition);

	public static void ModifyScreenPosition(Player player)
	{
		foreach (var modPlayer in HookModifyScreenPosition.Enumerate(player)) {
			try { modPlayer.ModifyScreenPosition(); } catch { }
		}
	}

	private delegate void DelegateModifyZoom(ref float zoom);
	private static HookList HookModifyZoom = AddHook<DelegateModifyZoom>(p => p.ModifyZoom);

	public static void ModifyZoom(Player player, ref float zoom)
	{
		foreach (var modPlayer in HookModifyZoom.Enumerate(player)) {
			try { modPlayer.ModifyZoom(ref zoom); } catch { }
		}
	}

	private static HookList HookPlayerConnect = AddHook<Action>(p => p.PlayerConnect);

	public static void PlayerConnect(int playerIndex)
	{
		var player = Main.player[playerIndex];
		foreach (var modPlayer in HookPlayerConnect.Enumerate(player)) {
			modPlayer.PlayerConnect();
		}
	}

	private static HookList HookPlayerDisconnect = AddHook<Action>(p => p.PlayerDisconnect);

	public static void PlayerDisconnect(int playerIndex)
	{
		var player = Main.player[playerIndex];
		foreach (var modPlayer in HookPlayerDisconnect.Enumerate(player)) {
			modPlayer.PlayerDisconnect();
		}
	}

	private static HookList HookOnEnterWorld = AddHook<Action>(p => p.OnEnterWorld);

	// Do NOT hook into the Player.Hooks.OnEnterWorld event
	public static void OnEnterWorld(int playerIndex)
	{
		var player = Main.player[playerIndex];
		foreach (var modPlayer in HookOnEnterWorld.Enumerate(player)) {
			modPlayer.OnEnterWorld();
		}
	}

	private static HookList HookOnRespawn = AddHook<Action>(p => p.OnRespawn);

	public static void OnRespawn(Player player)
	{
		foreach (var modPlayer in HookOnRespawn.Enumerate(player)) {
			try { modPlayer.OnRespawn(); } catch { }
		}
	}

	private static HookList HookShiftClickSlot = AddHook<Func<Item[], int, int, bool>>(p => p.ShiftClickSlot);

	public static bool ShiftClickSlot(Player player, Item[] inventory, int context, int slot)
	{
		foreach (var modPlayer in HookShiftClickSlot.Enumerate(player)) {
			if (modPlayer.ShiftClickSlot(inventory, context, slot)) {
				return true;
			}
		}
		return false;
	}

	private static HookList HookHoverSlot = AddHook<Func<Item[], int, int, bool>>(p => p.HoverSlot);

	public static bool HoverSlot(Player player, Item[] inventory, int context, int slot)
	{
		foreach (var modPlayer in HookHoverSlot.Enumerate(player)) {
			if (modPlayer.HoverSlot(inventory, context, slot)) {
				return true;
			}
		}
		return false;
	}

	private static HookList HookPostSellItem = AddHook<Action<NPC, Item[], Item>>(p => p.PostSellItem);

	public static void PostSellItem(Player player, NPC npc, Item[] shopInventory, Item item)
	{
		foreach (var modPlayer in HookPostSellItem.Enumerate(player)) {
			modPlayer.PostSellItem(npc, shopInventory, item);
		}
	}

	private static HookList HookCanSellItem = AddHook<Func<NPC, Item[], Item, bool>>(p => p.CanSellItem);

	// TODO: GlobalNPC and ModNPC hooks for Buy/Sell hooks as well.
	public static bool CanSellItem(Player player, NPC npc, Item[] shopInventory, Item item)
	{
		foreach (var modPlayer in HookCanSellItem.Enumerate(player)) {
			if (!modPlayer.CanSellItem(npc, shopInventory, item))
				return false;
		}
		return true;
	}

	private static HookList HookPostBuyItem = AddHook<Action<NPC, Item[], Item>>(p => p.PostBuyItem);

	public static void PostBuyItem(Player player, NPC npc, Item[] shopInventory, Item item)
	{
		foreach (var modPlayer in HookPostBuyItem.Enumerate(player)) {
			modPlayer.PostBuyItem(npc, shopInventory, item);
		}
	}

	private static HookList HookCanBuyItem = AddHook<Func<NPC, Item[], Item, bool>>(p => p.CanBuyItem);

	public static bool CanBuyItem(Player player, NPC npc, Item[] shopInventory, Item item)
	{
		foreach (var modPlayer in HookCanBuyItem.Enumerate(player)) {
			if (!modPlayer.CanBuyItem(npc, shopInventory, item))
				return false;
		}
		return true;
	}

	private static HookList HookCanUseItem = AddHook<Func<Item, bool>>(p => p.CanUseItem);

	public static bool CanUseItem(Player player, Item item)
	{
		foreach (var modPlayer in HookCanUseItem.Enumerate(player)) {
			if (!modPlayer.CanUseItem(item))
				return false;
		}

		return true;
	}

	private static HookList HookCanAutoReuseItem = AddHook<Func<Item, bool?>>(p => p.CanAutoReuseItem);

	public static bool? CanAutoReuseItem(Player player, Item item)
	{
		bool? flag = null;

		foreach (var modPlayer in HookCanAutoReuseItem.Enumerate(player)) {
			bool? allow = modPlayer.CanAutoReuseItem(item);

			if (allow.HasValue) {
				if (!allow.Value) {
					return false;
				}

				flag = true;
			}
		}

		return flag;
	}

	private delegate bool DelegateModifyNurseHeal(NPC npc, ref int health, ref bool removeDebuffs, ref string chatText);

	private static readonly HookList HookModifyNurseHeal = AddHook<DelegateModifyNurseHeal>(p => p.ModifyNurseHeal);

	public static bool ModifyNurseHeal(Player player, NPC npc, ref int health, ref bool removeDebuffs, ref string chat)
	{
		foreach (var modPlayer in HookModifyNurseHeal.Enumerate(player)) {
			if (!modPlayer.ModifyNurseHeal(npc, ref health, ref removeDebuffs, ref chat))
				return false;
		}

		return true;
	}

	private delegate void DelegateModifyNursePrice(NPC npc, int health, bool removeDebuffs, ref int price);
	private static HookList HookModifyNursePrice = AddHook<DelegateModifyNursePrice>(p => p.ModifyNursePrice);

	public static void ModifyNursePrice(Player player, NPC npc, int health, bool removeDebuffs, ref int price)
	{
		foreach (var modPlayer in HookModifyNursePrice.Enumerate(player)) {
			modPlayer.ModifyNursePrice(npc, health, removeDebuffs, ref price);
		}
	}

	private static HookList HookPostNurseHeal = AddHook<Action<NPC, int, bool, int>>(p => p.PostNurseHeal);

	public static void PostNurseHeal(Player player, NPC npc, int health, bool removeDebuffs, int price)
	{
		foreach (var modPlayer in HookPostNurseHeal.Enumerate(player)) {
			modPlayer.PostNurseHeal(npc, health, removeDebuffs, price);
		}
	}

	private static HookList HookAddStartingItems = AddHook<Func<bool, IEnumerable<Item>>>(p => p.AddStartingItems);
	private static HookList HookModifyStartingInventory = AddHook<Action<IReadOnlyDictionary<string, List<Item>>, bool>>(p => p.ModifyStartingInventory);

	public static List<Item> GetStartingItems(Player player, IEnumerable<Item> vanillaItems, bool mediumCoreDeath = false)
	{
		var itemsByMod = new Dictionary<string, List<Item>> {
			["Terraria"] = vanillaItems.ToList()
		};

		foreach (var modPlayer in HookAddStartingItems.Enumerate(player)) {
			itemsByMod[modPlayer.Mod.Name] = modPlayer.AddStartingItems(mediumCoreDeath).ToList();
		}

		foreach (var modPlayer in HookModifyStartingInventory.Enumerate(player)) {
			modPlayer.ModifyStartingInventory(itemsByMod, mediumCoreDeath);
		}

		return itemsByMod
			.OrderBy(kv => kv.Key == "Terraria" ? "" : kv.Key)
			.SelectMany(kv => kv.Value)
			.ToList();
	}

	private delegate IEnumerable<Item> DelegateFindMaterialsFrom(out ModPlayer.ItemConsumedCallback onUsedForCrafting);
	private static HookList HookAddCraftingMaterials = AddHook<DelegateFindMaterialsFrom>(p => p.AddMaterialsForCrafting);

	public static IEnumerable<(IEnumerable<Item>, ModPlayer.ItemConsumedCallback)> GetModdedCraftingMaterials(Player player)
	{
		// unfortunately we can't use a lot of nice ref struct syntax with enumerators, so we have to enumerate on the slow path.
		foreach (var modPlayer in HookAddCraftingMaterials.EnumerateSlow(player.modPlayers)) {
			var items = modPlayer.AddMaterialsForCrafting(out var onUsedForCrafting);
			if (items != null)
				yield return (items, onUsedForCrafting);
		}
	}

	private static HookList HookOnPickup = AddHook<Func<Item, bool>>(p => p.OnPickup);

	public static bool OnPickup(Player player, Item item)
	{
		foreach (var modPlayer in HookOnPickup.Enumerate(player)) {
			if (!modPlayer.OnPickup(item)) {
				return false;
			}
		}

		return true;
	}

	private static HookList HookArmorSetBonusActivated = AddHook<Action>(p => p.ArmorSetBonusActivated);

	public static void ArmorSetBonusActivated(Player player)
	{
		foreach (var modPlayer in HookArmorSetBonusActivated.Enumerate(player)) {
			modPlayer.ArmorSetBonusActivated();
		}
	}

	private static HookList HookArmorSetBonusHeld = AddHook<Action<int>>(p => p.ArmorSetBonusHeld);

	public static void ArmorSetBonusHeld(Player player, int holdTime)
	{
		foreach (var modPlayer in HookArmorSetBonusHeld.Enumerate(player)) {
			modPlayer.ArmorSetBonusHeld(holdTime);
		}
	}

	private static HookList HookCanBeTeleportedTo = AddHook<Func<Vector2, string, bool>>(p => p.CanBeTeleportedTo);

	public static bool CanBeTeleportedTo(Player player, Vector2 teleportPosition, string context)
	{
		foreach (var modPlayer in HookCanBeTeleportedTo.Enumerate(player)) {
			if (!modPlayer.CanBeTeleportedTo(teleportPosition, context))
				return false;
		}
		return true;
	}

	private static HookList HookOnEquipmentLoadoutSwitched = AddHook<Action<int, int>>(p => p.OnEquipmentLoadoutSwitched);

	public static void OnEquipmentLoadoutSwitched(Player player, int oldLoadoutIndex, int loadoutIndex)
	{
		foreach (var modPlayer in HookOnEquipmentLoadoutSwitched.Enumerate(player)) {
			modPlayer.OnEquipmentLoadoutSwitched(oldLoadoutIndex, loadoutIndex);
		}
	}

	private static HookList HookDrawPlayer = AddHook<Action<Camera>>(p => p.DrawPlayer);

	public static void DrawPlayer(Player player, Camera camera) {
		foreach (var modPlayer in HookDrawPlayer.Enumerate(player)) {
			modPlayer.DrawPlayer(camera);
		}
	}
}
