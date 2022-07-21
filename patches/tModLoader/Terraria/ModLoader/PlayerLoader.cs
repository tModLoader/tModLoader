using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader.Default;
using HookList = Terraria.ModLoader.Core.HookList<Terraria.ModLoader.ModPlayer>;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This is where all ModPlayer hooks are gathered and called.
	/// </summary>
	public static class PlayerLoader
	{
		private static readonly List<ModPlayer> players = new();
		private static readonly List<HookList> hooks = new();
		private static readonly List<HookList> modHooks = new();

		private static HookList AddHook<F>(Expression<Func<ModPlayer, F>> func) where F : Delegate {
			var hook = HookList.Create(func);

			hooks.Add(hook);

			return hook;
		}

		public static T AddModHook<T>(T hook) where T : HookList {
			hook.Update(players);

			modHooks.Add(hook);

			return hook;
		}

		internal static void Add(ModPlayer player) {
			player.Index = (ushort)players.Count;
			players.Add(player);
		}

		internal static void RebuildHooks() {
			foreach (var hook in hooks.Union(modHooks)) {
				hook.Update(players);
			}
		}

		internal static void Unload() {
			players.Clear();
			modHooks.Clear();
		}

		private static HookList HookInitialize = AddHook<Action>(p => p.Initialize);

		internal static void SetupPlayer(Player player) {
			player.modPlayers = NewInstances(player, CollectionsMarshal.AsSpan(players));

			foreach (var modPlayer in HookInitialize.Enumerate(player.modPlayers)) {
				modPlayer.Initialize();
			}
		}

		private static ModPlayer[] NewInstances(Player player, Span<ModPlayer> modPlayers) {
			var arr = new ModPlayer[modPlayers.Length];
			for (int i = 0; i < modPlayers.Length; i++)
				arr[i] = modPlayers[i].NewInstance(player);

			return arr;
		}

		private static HookList HookResetEffects = AddHook<Action>(p => p.ResetEffects);

		public static void ResetEffects(Player player) {
			foreach (var modPlayer in HookResetEffects.Enumerate(player.modPlayers)) {
				modPlayer.ResetEffects();
			}
		}

		private static HookList HookUpdateDead = AddHook<Action>(p => p.UpdateDead);

		public static void UpdateDead(Player player) {
			foreach (var modPlayer in HookUpdateDead.Enumerate(player.modPlayers)) {
				modPlayer.UpdateDead();
			}
		}

		public static void SetStartInventory(Player player, IList<Item> items) {
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

		public static void PreSavePlayer(Player player) {
			foreach (var modPlayer in HookPreSavePlayer.Enumerate(player.modPlayers)) {
				modPlayer.PreSavePlayer();
			}
		}

		private static HookList HookPostSavePlayer = AddHook<Action>(p => p.PostSavePlayer);

		public static void PostSavePlayer(Player player) {
			foreach (var modPlayer in HookPostSavePlayer.Enumerate(player.modPlayers)) {
				modPlayer.PostSavePlayer();
			}
		}

		private static HookList HookClientClone = AddHook<Action<ModPlayer>>(p => p.clientClone);

		public static void clientClone(Player player, Player clientClone) {
			foreach (var modPlayer in HookClientClone.Enumerate(player.modPlayers)) {
				modPlayer.clientClone(clientClone.modPlayers[modPlayer.Index]);
			}
		}

		private static HookList HookSyncPlayer = AddHook<Action<int, int, bool>>(p => p.SyncPlayer);

		public static void SyncPlayer(Player player, int toWho, int fromWho, bool newPlayer) {
			foreach (var modPlayer in HookSyncPlayer.Enumerate(player.modPlayers)) {
				modPlayer.SyncPlayer(toWho, fromWho, newPlayer);
			}
		}

		private static HookList HookSendClientChanges = AddHook<Action<ModPlayer>>(p => p.SendClientChanges);

		public static void SendClientChanges(Player player, Player clientPlayer) {
			foreach (var modPlayer in HookSendClientChanges.Enumerate(player.modPlayers)) {
				modPlayer.SendClientChanges(clientPlayer.modPlayers[modPlayer.Index]);
			}
		}

		private static HookList HookUpdateBadLifeRegen = AddHook<Action>(p => p.UpdateBadLifeRegen);

		public static void UpdateBadLifeRegen(Player player) {
			foreach (var modPlayer in HookUpdateBadLifeRegen.Enumerate(player.modPlayers)) {
				modPlayer.UpdateBadLifeRegen();
			}
		}

		private static HookList HookUpdateLifeRegen = AddHook<Action>(p => p.UpdateLifeRegen);

		public static void UpdateLifeRegen(Player player) {
			foreach (var modPlayer in HookUpdateLifeRegen.Enumerate(player.modPlayers)) {
				modPlayer.UpdateLifeRegen();
			}
		}

		private delegate void DelegateNaturalLifeRegen(ref float regen);
		private static HookList HookNaturalLifeRegen = AddHook<DelegateNaturalLifeRegen>(p => p.NaturalLifeRegen);

		public static void NaturalLifeRegen(Player player, ref float regen) {
			foreach (var modPlayer in HookNaturalLifeRegen.Enumerate(player.modPlayers)) {
				modPlayer.NaturalLifeRegen(ref regen);
			}
		}

		private static HookList HookUpdateAutopause = AddHook<Action>(p => p.UpdateAutopause);

		public static void UpdateAutopause(Player player) {
			foreach (var modPlayer in HookUpdateAutopause.Enumerate(player.modPlayers)) {
				modPlayer.UpdateAutopause();
			}
		}

		private static HookList HookPreUpdate = AddHook<Action>(p => p.PreUpdate);

		public static void PreUpdate(Player player) {
			foreach (var modPlayer in HookPreUpdate.Enumerate(player.modPlayers)) {
				modPlayer.PreUpdate();
			}
		}

		private static HookList HookSetControls = AddHook<Action>(p => p.SetControls);

		public static void SetControls(Player player) {
			foreach (var modPlayer in HookSetControls.Enumerate(player.modPlayers)) {
				modPlayer.SetControls();
			}
		}

		private static HookList HookPreUpdateBuffs = AddHook<Action>(p => p.PreUpdateBuffs);

		public static void PreUpdateBuffs(Player player) {
			foreach (var modPlayer in HookPreUpdateBuffs.Enumerate(player.modPlayers)) {
				modPlayer.PreUpdateBuffs();
			}
		}

		private static HookList HookPostUpdateBuffs = AddHook<Action>(p => p.PostUpdateBuffs);

		public static void PostUpdateBuffs(Player player) {
			foreach (var modPlayer in HookPostUpdateBuffs.Enumerate(player.modPlayers)) {
				modPlayer.PostUpdateBuffs();
			}
		}

		private delegate void DelegateUpdateEquips();
		private static HookList HookUpdateEquips = AddHook<DelegateUpdateEquips>(p => p.UpdateEquips);

		public static void UpdateEquips(Player player) {
			foreach (var modPlayer in HookUpdateEquips.Enumerate(player.modPlayers)) {
				modPlayer.UpdateEquips();
			}
		}

		private static HookList HookPostUpdateEquips = AddHook<Action>(p => p.PostUpdateEquips);

		public static void PostUpdateEquips(Player player) {
			foreach (var modPlayer in HookPostUpdateEquips.Enumerate(player.modPlayers)) {
				modPlayer.PostUpdateEquips();
			}
		}

		private static HookList HookUpdateVisibleAccessories = AddHook<Action>(p => p.UpdateVisibleAccessories);

		public static void UpdateVisibleAccessories(Player player) {
			foreach (var modPlayer in HookUpdateVisibleAccessories.Enumerate(player.modPlayers)) {
				modPlayer.UpdateVisibleAccessories();
			}
		}

		private static HookList HookUpdateVisibleVanityAccessories = AddHook<Action>(p => p.UpdateVisibleVanityAccessories);

		public static void UpdateVisibleVanityAccessories(Player player) {
			foreach (var modPlayer in HookUpdateVisibleVanityAccessories.Enumerate(player.modPlayers)) {
				modPlayer.UpdateVisibleVanityAccessories();
			}
		}

		private static HookList HookUpdateDyes = AddHook<Action>(p => p.UpdateDyes);

		public static void UpdateDyes(Player player) {
			foreach (var modPlayer in HookUpdateDyes.Enumerate(player.modPlayers)) {
				modPlayer.UpdateDyes();
			}
		}

		private static HookList HookPostUpdateMiscEffects = AddHook<Action>(p => p.PostUpdateMiscEffects);

		public static void PostUpdateMiscEffects(Player player) {
			foreach (var modPlayer in HookPostUpdateMiscEffects.Enumerate(player.modPlayers)) {
				modPlayer.PostUpdateMiscEffects();
			}
		}

		private static HookList HookPostUpdateRunSpeeds = AddHook<Action>(p => p.PostUpdateRunSpeeds);

		public static void PostUpdateRunSpeeds(Player player) {
			foreach (var modPlayer in HookPostUpdateRunSpeeds.Enumerate(player.modPlayers)) {
				modPlayer.PostUpdateRunSpeeds();
			}
		}

		private static HookList HookPreUpdateMovement = AddHook<Action>(p => p.PreUpdateMovement);

		public static void PreUpdateMovement(Player player) {
			foreach (var modPlayer in HookPreUpdateMovement.Enumerate(player.modPlayers)) {
				modPlayer.PreUpdateMovement();
			}
		}

		private static HookList HookPostUpdate = AddHook<Action>(p => p.PostUpdate);

		public static void PostUpdate(Player player) {
			foreach (var modPlayer in HookPostUpdate.Enumerate(player.modPlayers)) {
				modPlayer.PostUpdate();
			}
		}

		private static HookList HookFrameEffects = AddHook<Action>(p => p.FrameEffects);

		public static void FrameEffects(Player player) {
			foreach (var modPlayer in HookFrameEffects.Enumerate(player.modPlayers)) {
				modPlayer.FrameEffects();
			}
		}

		private delegate bool DelegatePreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection,
			ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter);
		private static HookList HookPreHurt = AddHook<DelegatePreHurt>(p => p.PreHurt);

		public static bool PreHurt(Player player, bool pvp, bool quiet, ref int damage, ref int hitDirection,
			ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter) {
			PreHurt_Obsolete(player, pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource);
			bool flag = true;
			foreach (var modPlayer in HookPreHurt.Enumerate(player.modPlayers)) {
				if (!modPlayer.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage,
						ref playSound, ref genGore, ref damageSource, ref cooldownCounter)) {
					flag = false;
				}
			}
			return flag;
		}

		private static HookList HookHurt = AddHook<Action<bool, bool, double, int, bool, int>>(p => p.Hurt);

		public static void Hurt(Player player, bool pvp, bool quiet, double damage, int hitDirection, bool crit, int cooldownCounter) {
			Hurt_Obsolete(player, pvp, quiet, damage, hitDirection, crit);
			foreach (var modPlayer in HookHurt.Enumerate(player.modPlayers)) {
				modPlayer.Hurt(pvp, quiet, damage, hitDirection, crit, cooldownCounter);
			}
		}

		private static HookList HookPostHurt = AddHook<Action<bool, bool, double, int, bool, int>>(p => p.PostHurt);

		public static void PostHurt(Player player, bool pvp, bool quiet, double damage, int hitDirection, bool crit, int cooldownCounter) {
			PostHurt_Obsolete(player, pvp, quiet, damage, hitDirection, crit);
			foreach (var modPlayer in HookPostHurt.Enumerate(player.modPlayers)) {
				modPlayer.PostHurt(pvp, quiet, damage, hitDirection, crit, cooldownCounter);
			}
		}

#region Legacy
		private delegate bool DelegatePreHurt_Obsolete(bool pvp, bool quiet, ref int damage, ref int hitDirection,
			ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource);
		[Obsolete]
		private static HookList HookPreHurt_Obsolete = AddHook<DelegatePreHurt_Obsolete>(p => p.PreHurt);

		[Obsolete]
		private static bool PreHurt_Obsolete(Player player, bool pvp, bool quiet, ref int damage, ref int hitDirection,
			ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) {
			bool flag = true;
			foreach (var modPlayer in HookPreHurt_Obsolete.Enumerate(player.modPlayers)) {
				if (!modPlayer.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage,
						ref playSound, ref genGore, ref damageSource)) {
					flag = false;
				}
			}
			return flag;
		}

		[Obsolete]
		private static HookList HookHurt_Obsolete = AddHook<Action<bool, bool, double, int, bool>>(p => p.Hurt);

		[Obsolete]
		private static void Hurt_Obsolete(Player player, bool pvp, bool quiet, double damage, int hitDirection, bool crit) {
			foreach (var modPlayer in HookHurt_Obsolete.Enumerate(player.modPlayers)) {
				modPlayer.Hurt(pvp, quiet, damage, hitDirection, crit);
			}
		}

		[Obsolete]
		private static HookList HookPostHurt_Obsolete = AddHook<Action<bool, bool, double, int, bool>>(p => p.PostHurt);

		[Obsolete]
		private static void PostHurt_Obsolete(Player player, bool pvp, bool quiet, double damage, int hitDirection, bool crit) {
			foreach (var modPlayer in HookPostHurt_Obsolete.Enumerate(player.modPlayers)) {
				modPlayer.PostHurt(pvp, quiet, damage, hitDirection, crit);
			}
		}
#endregion

		private delegate bool DelegatePreKill(double damage, int hitDirection, bool pvp, ref bool playSound,
			ref bool genGore, ref PlayerDeathReason damageSource);
		private static HookList HookPreKill = AddHook<DelegatePreKill>(p => p.PreKill);

		public static bool PreKill(Player player, double damage, int hitDirection, bool pvp, ref bool playSound,
			ref bool genGore, ref PlayerDeathReason damageSource) {
			bool flag = true;
			foreach (var modPlayer in HookPreKill.Enumerate(player.modPlayers)) {
				if (!modPlayer.PreKill(damage, hitDirection, pvp, ref playSound, ref genGore, ref damageSource)) {
					flag = false;
				}
			}
			return flag;
		}

		private static HookList HookKill = AddHook<Action<double, int, bool, PlayerDeathReason>>(p => p.Kill);

		public static void Kill(Player player, double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
			foreach (var modPlayer in HookKill.Enumerate(player.modPlayers)) {
				modPlayer.Kill(damage, hitDirection, pvp, damageSource);
			}
		}

		private delegate bool DelegatePreModifyLuck(ref float luck);
		private static HookList HookPreModifyLuck = AddHook<DelegatePreModifyLuck>(p => p.PreModifyLuck);

		public static bool PreModifyLuck(Player player, ref float luck) {
			bool flag = true;
			foreach (var modPlayer in HookPreModifyLuck.Enumerate(player.modPlayers)) {
				if (!modPlayer.PreModifyLuck(ref luck)) {
					flag = false;
				}
			}
			return flag;
		}

		private delegate void DelegateModifyLuck(ref float luck);
		private static HookList HookModifyLuck = AddHook<DelegateModifyLuck>(p => p.ModifyLuck);

		public static void ModifyLuck(Player player, ref float luck) {
			foreach (var modPlayer in HookModifyLuck.Enumerate(player.modPlayers)) {
				modPlayer.ModifyLuck(ref luck);
			}
		}

		private static HookList HookPreItemCheck = AddHook<Func<bool>>(p => p.PreItemCheck);

		public static bool PreItemCheck(Player player) {
			bool result = true;
			foreach (var modPlayer in HookPreItemCheck.Enumerate(player.modPlayers)) {
				result &= modPlayer.PreItemCheck();
			}
			return result;
		}

		private static HookList HookPostItemCheck = AddHook<Action>(p => p.PostItemCheck);

		public static void PostItemCheck(Player player) {
			foreach (var modPlayer in HookPostItemCheck.Enumerate(player.modPlayers)) {
				modPlayer.PostItemCheck();
			}
		}

		private static HookList HookUseTimeMultiplier = AddHook<Func<Item, float>>(p => p.UseTimeMultiplier);

		public static float UseTimeMultiplier(Player player, Item item) {
			float multiplier = 1f;

			if (item.IsAir)
				return multiplier;

			foreach (var modPlayer in HookUseTimeMultiplier.Enumerate(player.modPlayers)) {
				multiplier *= modPlayer.UseTimeMultiplier(item);
			}

			return multiplier;
		}

		private static HookList HookUseAnimationMultiplier = AddHook<Func<Item, float>>(p => p.UseAnimationMultiplier);

		public static float UseAnimationMultiplier(Player player, Item item) {
			float multiplier = 1f;

			if (item.IsAir)
				return multiplier;

			foreach (var modPlayer in HookUseAnimationMultiplier.Enumerate(player.modPlayers)) {
				multiplier *= modPlayer.UseAnimationMultiplier(item);
			}

			return multiplier;
		}

		private static HookList HookUseSpeedMultiplier = AddHook<Func<Item, float>>(p => p.UseSpeedMultiplier);

		public static float UseSpeedMultiplier(Player player, Item item) {
			float multiplier = 1f;

			if (item.IsAir)
				return multiplier;

			foreach (var modPlayer in HookUseSpeedMultiplier.Enumerate(player.modPlayers)) {
				multiplier *= modPlayer.UseSpeedMultiplier(item);
			}

			return multiplier;
		}

		private delegate void DelegateGetHealLife(Item item, bool quickHeal, ref int healValue);
		private static HookList HookGetHealLife = AddHook<DelegateGetHealLife>(p => p.GetHealLife);

		public static void GetHealLife(Player player, Item item, bool quickHeal, ref int healValue) {
			if (item.IsAir)
				return;

			foreach (var modPlayer in HookGetHealLife.Enumerate(player.modPlayers)) {
				modPlayer.GetHealLife(item, quickHeal, ref healValue);
			}
		}

		private delegate void DelegateGetHealMana(Item item, bool quickHeal, ref int healValue);
		private static HookList HookGetHealMana = AddHook<DelegateGetHealMana>(p => p.GetHealMana);

		public static void GetHealMana(Player player, Item item, bool quickHeal, ref int healValue) {
			if (item.IsAir)
				return;

			foreach (var modPlayer in HookGetHealMana.Enumerate(player.modPlayers)) {
				modPlayer.GetHealMana(item, quickHeal, ref healValue);
			}
		}

		private delegate void DelegateModifyManaCost(Item item, ref float reduce, ref float mult);
		private static HookList HookModifyManaCost = AddHook<DelegateModifyManaCost>(p => p.ModifyManaCost);

		public static void ModifyManaCost(Player player, Item item, ref float reduce, ref float mult) {
			if (item.IsAir)
				return;

			foreach (var modPlayer in HookModifyManaCost.Enumerate(player.modPlayers)) {
				modPlayer.ModifyManaCost(item, ref reduce, ref mult);
			}
		}

		private static HookList HookOnMissingMana = AddHook<Action<Item, int>>(p => p.OnMissingMana);

		public static void OnMissingMana(Player player, Item item, int manaNeeded) {
			if (item.IsAir)
				return;

			foreach (var modPlayer in HookOnMissingMana.Enumerate(player.modPlayers)) {
				modPlayer.OnMissingMana(item, manaNeeded);
			}
		}

		private static HookList HookOnConsumeMana = AddHook<Action<Item, int>>(p => p.OnConsumeMana);

		public static void OnConsumeMana(Player player, Item item, int manaConsumed) {
			if (item.IsAir)
				return;

			foreach (var modPlayer in HookOnConsumeMana.Enumerate(player.modPlayers)) {
				modPlayer.OnConsumeMana(item, manaConsumed);
			}
		}

		private delegate void DelegateModifyWeaponDamage(Item item, ref StatModifier damage);
		private static HookList HookModifyWeaponDamage = AddHook<DelegateModifyWeaponDamage>(p => p.ModifyWeaponDamage);
		/// <summary>
		/// Calls ModItem.HookModifyWeaponDamage, then all GlobalItem.HookModifyWeaponDamage hooks.
		/// </summary>
		public static void ModifyWeaponDamage(Player player, Item item, ref StatModifier damage) {
			if (item.IsAir)
				return;

			foreach (var modPlayer in HookModifyWeaponDamage.Enumerate(player.modPlayers)) {
				modPlayer.ModifyWeaponDamage(item, ref damage);
			}
		}

		private static HookList HookProcessTriggers = AddHook<Action<TriggersSet>>(p => p.ProcessTriggers);

		public static void ProcessTriggers(Player player, TriggersSet triggersSet) {
			foreach (var modPlayer in HookProcessTriggers.Enumerate(player.modPlayers)) {
				modPlayer.ProcessTriggers(triggersSet);
			}
		}

		private delegate void DelegateModifyWeaponKnockback(Item item, ref StatModifier knockback);
		private static HookList HookModifyWeaponKnockback = AddHook<DelegateModifyWeaponKnockback>(p => p.ModifyWeaponKnockback);

		public static void ModifyWeaponKnockback(Player player, Item item, ref StatModifier knockback) {
			if (item.IsAir)
				return;

			foreach (var modPlayer in HookModifyWeaponKnockback.Enumerate(player.modPlayers)) {
				modPlayer.ModifyWeaponKnockback(item, ref knockback);
			}
		}

		private delegate void DelegateModifyWeaponCrit(Item item, ref float crit);
		private static HookList HookModifyWeaponCrit = AddHook<DelegateModifyWeaponCrit>(p => p.ModifyWeaponCrit);

		public static void ModifyWeaponCrit(Player player, Item item, ref float crit) {
			if (item.IsAir) return;
			foreach (var modPlayer in HookModifyWeaponCrit.Enumerate(player.modPlayers)) {
				modPlayer.ModifyWeaponCrit(item, ref crit);
			}
		}

		private static HookList HookCanConsumeAmmo = AddHook<Func<Item, Item, bool>>(p => p.CanConsumeAmmo);

		public static bool CanConsumeAmmo(Player player, Item weapon, Item ammo) {
			foreach (var modPlayer in HookCanConsumeAmmo.Enumerate(player.modPlayers)) {
				if (!modPlayer.CanConsumeAmmo(weapon, ammo)) {
					return false;
				}
			}
			return true;
		}

		private static HookList HookOnConsumeAmmo = AddHook<Action<Item, Item>>(p => p.OnConsumeAmmo);

		public static void OnConsumeAmmo(Player player, Item weapon, Item ammo) {
			foreach (var modPlayer in HookOnConsumeAmmo.Enumerate(player.modPlayers))
				modPlayer.OnConsumeAmmo(weapon, ammo);
		}

		private static HookList HookCanShoot = AddHook<Func<Item, bool>>(p => p.CanShoot);

		public static bool CanShoot(Player player, Item item) {
			foreach (var modPlayer in HookCanShoot.Enumerate(player.modPlayers)) {
				if (!modPlayer.CanShoot(item))
					return false;
			}

			return true;
		}

		private delegate void DelegateModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback);
		private static HookList HookModifyShootStats = AddHook<DelegateModifyShootStats>(p => p.ModifyShootStats);

		public static void ModifyShootStats(Player player, Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			foreach (var modPlayer in HookModifyShootStats.Enumerate(player.modPlayers)) {
				modPlayer.ModifyShootStats(item, ref position, ref velocity, ref type, ref damage, ref knockback);
			}
		}

		private static HookList HookShoot = AddHook<Func<Item, EntitySource_ItemUse_WithAmmo, Vector2, Vector2, int, int, float, bool>>(p => p.Shoot);

		public static bool Shoot(Player player, Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			bool defaultResult = true;

			foreach (var modPlayer in HookShoot.Enumerate(player.modPlayers)) {
				defaultResult &= modPlayer.Shoot(item, source, position, velocity, type, damage, knockback);
			}

			return defaultResult;
		}

		private static HookList HookMeleeEffects = AddHook<Action<Item, Rectangle>>(p => p.MeleeEffects);

		public static void MeleeEffects(Player player, Item item, Rectangle hitbox) {
			foreach (var modPlayer in HookMeleeEffects.Enumerate(player.modPlayers)) {
				modPlayer.MeleeEffects(item, hitbox);
			}
		}

		private static HookList HookCanCatchNPC = AddHook<Func<NPC, Item, bool?>>(p => p.CanCatchNPC);

		public static bool? CanCatchNPC(Player player, NPC target, Item item) {
			bool? returnValue = null;
			foreach (var modPlayer in HookCanCatchNPC.Enumerate(player.modPlayers)) {
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

		public static void OnCatchNPC(Player player, NPC target, Item item, bool failed) {
			foreach (var modPlayer in HookOnCatchNPC.Enumerate(player.modPlayers)) {
				modPlayer.OnCatchNPC(target, item, failed);
			}
		}

		private delegate void DelegateModifyItemScale(Item item, ref float scale);
		private static HookList HookModifyItemScale = AddHook<DelegateModifyItemScale>(p => p.ModifyItemScale);

		public static void ModifyItemScale(Player player, Item item, ref float scale) {
			foreach (var modPlayer in HookModifyItemScale.Enumerate(player.modPlayers)) {
				modPlayer.ModifyItemScale(item, ref scale);
			}
		}

		private static HookList HookOnHitAnything = AddHook<Action<float, float, Entity>>(p => p.OnHitAnything);

		public static void OnHitAnything(Player player, float x, float y, Entity victim) {
			foreach (var modPlayer in HookOnHitAnything.Enumerate(player.modPlayers)) {
				modPlayer.OnHitAnything(x, y, victim);
			}
		}

		private static HookList HookCanHitNPC = AddHook<Func<Item, NPC, bool?>>(p => p.CanHitNPC);

		public static bool? CanHitNPC(Player player, Item item, NPC target) {
			bool? flag = null;

			foreach (var modPlayer in HookCanHitNPC.Enumerate(player.modPlayers)) {
				bool? canHit = modPlayer.CanHitNPC(item, target);

				if (canHit.HasValue) {
					if (!canHit.Value) {
						return false;
					}

					flag = true;
				}
			}

			return flag;
		}

		private delegate void DelegateModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit);
		private static HookList HookModifyHitNPC = AddHook<DelegateModifyHitNPC>(p => p.ModifyHitNPC);

		public static void ModifyHitNPC(Player player, Item item, NPC target, ref int damage, ref float knockback, ref bool crit) {
			foreach (var modPlayer in HookModifyHitNPC.Enumerate(player.modPlayers)) {
				modPlayer.ModifyHitNPC(item, target, ref damage, ref knockback, ref crit);
			}
		}

		private static HookList HookOnHitNPC = AddHook<Action<Item, NPC, int, float, bool>>(p => p.OnHitNPC);

		public static void OnHitNPC(Player player, Item item, NPC target, int damage, float knockback, bool crit) {
			foreach (var modPlayer in HookOnHitNPC.Enumerate(player.modPlayers)) {
				modPlayer.OnHitNPC(item, target, damage, knockback, crit);
			}
		}

		private static HookList HookCanHitNPCWithProj = AddHook<Func<Projectile, NPC, bool?>>(p => p.CanHitNPCWithProj);

		public static bool? CanHitNPCWithProj(Projectile proj, NPC target) {
			if (proj.npcProj || proj.trap) {
				return null;
			}
			Player player = Main.player[proj.owner];
			bool? flag = null;
			foreach (var modPlayer in HookCanHitNPCWithProj.Enumerate(player.modPlayers)) {
				bool? canHit = modPlayer.CanHitNPCWithProj(proj, target);
				if (canHit.HasValue && !canHit.Value) {
					return false;
				}
				if (canHit.HasValue) {
					flag = canHit.Value;
				}
			}
			return flag;
		}

		private delegate void DelegateModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection);
		private static HookList HookModifyHitNPCWithProj = AddHook<DelegateModifyHitNPCWithProj>(p => p.ModifyHitNPCWithProj);

		public static void ModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			if (proj.npcProj || proj.trap) {
				return;
			}
			Player player = Main.player[proj.owner];
			foreach (var modPlayer in HookModifyHitNPCWithProj.Enumerate(player.modPlayers)) {
				modPlayer.ModifyHitNPCWithProj(proj, target, ref damage, ref knockback, ref crit, ref hitDirection);
			}
		}

		private static HookList HookOnHitNPCWithProj = AddHook<Action<Projectile, NPC, int, float, bool>>(p => p.OnHitNPCWithProj);

		public static void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit) {
			if (proj.npcProj || proj.trap) {
				return;
			}
			Player player = Main.player[proj.owner];
			foreach (var modPlayer in HookOnHitNPCWithProj.Enumerate(player.modPlayers)) {
				modPlayer.OnHitNPCWithProj(proj, target, damage, knockback, crit);
			}
		}

		private static HookList HookCanHitPvp = AddHook<Func<Item, Player, bool>>(p => p.CanHitPvp);

		public static bool CanHitPvp(Player player, Item item, Player target) {
			foreach (var modPlayer in HookCanHitPvp.Enumerate(player.modPlayers)) {
				if (!modPlayer.CanHitPvp(item, target)) {
					return false;
				}
			}
			return true;
		}

		private delegate void DelegateModifyHitPvp(Item item, Player target, ref int damage, ref bool crit);
		private static HookList HookModifyHitPvp = AddHook<DelegateModifyHitPvp>(p => p.ModifyHitPvp);

		public static void ModifyHitPvp(Player player, Item item, Player target, ref int damage, ref bool crit) {
			foreach (var modPlayer in HookModifyHitPvp.Enumerate(player.modPlayers)) {
				modPlayer.ModifyHitPvp(item, target, ref damage, ref crit);
			}
		}

		private static HookList HookOnHitPvp = AddHook<Action<Item, Player, int, bool>>(p => p.OnHitPvp);

		public static void OnHitPvp(Player player, Item item, Player target, int damage, bool crit) {
			foreach (var modPlayer in HookOnHitPvp.Enumerate(player.modPlayers)) {
				modPlayer.OnHitPvp(item, target, damage, crit);
			}
		}

		private static HookList HookCanHitPvpWithProj = AddHook<Func<Projectile, Player, bool>>(p => p.CanHitPvpWithProj);

		public static bool CanHitPvpWithProj(Projectile proj, Player target) {
			Player player = Main.player[proj.owner];
			foreach (var modPlayer in HookCanHitPvpWithProj.Enumerate(player.modPlayers)) {
				if (!modPlayer.CanHitPvpWithProj(proj, target)) {
					return false;
				}
			}
			return true;
		}

		private delegate void DelegateModifyHitPvpWithProj(Projectile proj, Player target, ref int damage, ref bool crit);
		private static HookList HookModifyHitPvpWithProj = AddHook<DelegateModifyHitPvpWithProj>(p => p.ModifyHitPvpWithProj);

		public static void ModifyHitPvpWithProj(Projectile proj, Player target, ref int damage, ref bool crit) {
			Player player = Main.player[proj.owner];
			foreach (var modPlayer in HookModifyHitPvpWithProj.Enumerate(player.modPlayers)) {
				modPlayer.ModifyHitPvpWithProj(proj, target, ref damage, ref crit);
			}
		}

		private static HookList HookOnHitPvpWithProj = AddHook<Action<Projectile, Player, int, bool>>(p => p.OnHitPvpWithProj);

		public static void OnHitPvpWithProj(Projectile proj, Player target, int damage, bool crit) {
			Player player = Main.player[proj.owner];
			foreach (var modPlayer in HookOnHitPvpWithProj.Enumerate(player.modPlayers)) {
				modPlayer.OnHitPvpWithProj(proj, target, damage, crit);
			}
		}

		private delegate bool DelegateCanBeHitByNPC(NPC npc, ref int cooldownSlot);
		private static HookList HookCanBeHitByNPC = AddHook<DelegateCanBeHitByNPC>(p => p.CanBeHitByNPC);

		public static bool CanBeHitByNPC(Player player, NPC npc, ref int cooldownSlot) {
			foreach (var modPlayer in HookCanBeHitByNPC.Enumerate(player.modPlayers)) {
				if (!modPlayer.CanBeHitByNPC(npc, ref cooldownSlot)) {
					return false;
				}
			}
			return true;
		}

		private delegate void DelegateModifyHitByNPC(NPC npc, ref int damage, ref bool crit);
		private static HookList HookModifyHitByNPC = AddHook<DelegateModifyHitByNPC>(p => p.ModifyHitByNPC);

		public static void ModifyHitByNPC(Player player, NPC npc, ref int damage, ref bool crit) {
			foreach (var modPlayer in HookModifyHitByNPC.Enumerate(player.modPlayers)) {
				modPlayer.ModifyHitByNPC(npc, ref damage, ref crit);
			}
		}

		private static HookList HookOnHitByNPC = AddHook<Action<NPC, int, bool>>(p => p.OnHitByNPC);

		public static void OnHitByNPC(Player player, NPC npc, int damage, bool crit) {
			foreach (var modPlayer in HookOnHitByNPC.Enumerate(player.modPlayers)) {
				modPlayer.OnHitByNPC(npc, damage, crit);
			}
		}

		private static HookList HookCanBeHitByProjectile = AddHook<Func<Projectile, bool>>(p => p.CanBeHitByProjectile);

		public static bool CanBeHitByProjectile(Player player, Projectile proj) {
			foreach (var modPlayer in HookCanBeHitByProjectile.Enumerate(player.modPlayers)) {
				if (!modPlayer.CanBeHitByProjectile(proj)) {
					return false;
				}
			}
			return true;
		}

		private delegate void DelegateModifyHitByProjectile(Projectile proj, ref int damage, ref bool crit);
		private static HookList HookModifyHitByProjectile = AddHook<DelegateModifyHitByProjectile>(p => p.ModifyHitByProjectile);

		public static void ModifyHitByProjectile(Player player, Projectile proj, ref int damage, ref bool crit) {
			foreach (var modPlayer in HookModifyHitByProjectile.Enumerate(player.modPlayers)) {
				modPlayer.ModifyHitByProjectile(proj, ref damage, ref crit);
			}
		}

		private static HookList HookOnHitByProjectile = AddHook<Action<Projectile, int, bool>>(p => p.OnHitByProjectile);

		public static void OnHitByProjectile(Player player, Projectile proj, int damage, bool crit) {
			foreach (var modPlayer in HookOnHitByProjectile.Enumerate(player.modPlayers)) {
				modPlayer.OnHitByProjectile(proj, damage, crit);
			}
		}

		private delegate void DelegateModifyFishingAttempt(ref FishingAttempt attempt);
		private static HookList HookModifyFishingAttempt = AddHook<DelegateModifyFishingAttempt>(p => p.ModifyFishingAttempt);

		public static void ModifyFishingAttempt(Player player, ref FishingAttempt attempt) {
			foreach (var modPlayer in HookModifyFishingAttempt.Enumerate(player.modPlayers)) {
				modPlayer.ModifyFishingAttempt(ref attempt);
			}

			attempt.rolledItemDrop = attempt.rolledEnemySpawn = 0; // Reset, modders need to use CatchFish for this 
		}

		private delegate void DelegateCatchFish(FishingAttempt attempt, ref int itemDrop, ref int enemySpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition);
		private static HookList HookCatchFish = AddHook<DelegateCatchFish>(p => p.CatchFish);

		public static void CatchFish(Player player, FishingAttempt attempt, ref int itemDrop, ref int enemySpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) {
			foreach (var modPlayer in HookCatchFish.Enumerate(player.modPlayers)) {
				modPlayer.CatchFish(attempt, ref itemDrop, ref enemySpawn, ref sonar, ref sonarPosition);
			}
		}

		private delegate void DelegateModifyCaughtFish(Item fish);
		private static HookList HookCaughtFish = AddHook<DelegateModifyCaughtFish>(p => p.ModifyCaughtFish);

		public static void ModifyCaughtFish(Player player, Item fish) {
			foreach (var modPlayer in HookCaughtFish.Enumerate(player.modPlayers)) {
				modPlayer.ModifyCaughtFish(fish);
			}
		}

		private delegate bool? DelegateCanConsumeBait(Item bait);
		private static HookList HookCanConsumeBait = AddHook<DelegateCanConsumeBait>(p => p.CanConsumeBait);

		public static bool? CanConsumeBait(Player player, Item bait) {
			bool? ret = null;
			foreach (var modPlayer in HookCaughtFish.Enumerate(player.modPlayers)) {
				if (modPlayer.CanConsumeBait(bait) is bool b)
					ret = (ret ?? true) && b;
			}
			return ret;
		}

		private delegate void DelegateGetFishingLevel(Item fishingRod, Item bait, ref float fishingLevel);
		private static HookList HookGetFishingLevel = AddHook<DelegateGetFishingLevel>(p => p.GetFishingLevel);

		public static void GetFishingLevel(Player player, Item fishingRod, Item bait, ref float fishingLevel) {
			foreach (var modPlayer in HookGetFishingLevel.Enumerate(player.modPlayers)) {
				modPlayer.GetFishingLevel(fishingRod, bait, ref fishingLevel);
			}
		}

		private static HookList HookAnglerQuestReward = AddHook<Action<float, List<Item>>>(p => p.AnglerQuestReward);

		public static void AnglerQuestReward(Player player, float rareMultiplier, List<Item> rewardItems) {
			foreach (var modPlayer in HookAnglerQuestReward.Enumerate(player.modPlayers)) {
				modPlayer.AnglerQuestReward(rareMultiplier, rewardItems);
			}
		}

		private static HookList HookGetDyeTraderReward = AddHook<Action<List<int>>>(p => p.GetDyeTraderReward);

		public static void GetDyeTraderReward(Player player, List<int> rewardPool) {
			foreach (var modPlayer in HookGetDyeTraderReward.Enumerate(player.modPlayers)) {
				modPlayer.GetDyeTraderReward(rewardPool);
			}
		}

		private delegate void DelegateDrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright);
		private static HookList HookDrawEffects = AddHook<DelegateDrawEffects>(p => p.DrawEffects);

		public static void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
			var player = drawInfo.drawPlayer;

			foreach (var modPlayer in HookDrawEffects.Enumerate(player.modPlayers)) {
				modPlayer.DrawEffects(drawInfo, ref r, ref g, ref b, ref a, ref fullBright);
			}
		}

		private delegate void DelegateModifyDrawInfo(ref PlayerDrawSet drawInfo);
		private static HookList HookModifyDrawInfo = AddHook<DelegateModifyDrawInfo>(p => p.ModifyDrawInfo);

		public static void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
			var player = drawInfo.drawPlayer;

			foreach (var modPlayer in HookModifyDrawInfo.Enumerate(player.modPlayers)) {
				modPlayer.ModifyDrawInfo(ref drawInfo);
			}
		}

		private static HookList HookModifyDrawLayerOrdering = AddHook<Action<IDictionary<PlayerDrawLayer, PlayerDrawLayer.Position>>>(p => p.ModifyDrawLayerOrdering);

		public static void ModifyDrawLayerOrdering(IDictionary<PlayerDrawLayer, PlayerDrawLayer.Position> positions) {
			foreach (var modPlayer in HookModifyDrawLayerOrdering.Enumerate(players)) {
				modPlayer.ModifyDrawLayerOrdering(positions);
			}
		}

		private static HookList HookModifyDrawLayers = AddHook<Action<PlayerDrawSet>>(p => p.HideDrawLayers);

		public static void HideDrawLayers(PlayerDrawSet drawInfo) {
			var player = drawInfo.drawPlayer;

			foreach (var modPlayer in HookModifyDrawLayers.Enumerate(player.modPlayers)) {
				modPlayer.HideDrawLayers(drawInfo);
			}
		}

		private static HookList HookModifyScreenPosition = AddHook<Action>(p => p.ModifyScreenPosition);

		public static void ModifyScreenPosition(Player player) {
			foreach (var modPlayer in HookModifyScreenPosition.Enumerate(player.modPlayers)) {
				modPlayer.ModifyScreenPosition();
			}
		}

		private delegate void DelegateModifyZoom(ref float zoom);
		private static HookList HookModifyZoom = AddHook<DelegateModifyZoom>(p => p.ModifyZoom);

		public static void ModifyZoom(Player player, ref float zoom) {
			foreach (var modPlayer in HookModifyZoom.Enumerate(player.modPlayers.ToList())) {
				modPlayer.ModifyZoom(ref zoom);
			}
		}

		private static HookList HookPlayerConnect = AddHook<Action<Player>>(p => p.PlayerConnect);

		public static void PlayerConnect(int playerIndex) {
			var player = Main.player[playerIndex];
			foreach (var modPlayer in HookPlayerConnect.Enumerate(player.modPlayers)) {
				modPlayer.PlayerConnect(player);
			}
		}

		private static HookList HookPlayerDisconnect = AddHook<Action<Player>>(p => p.PlayerDisconnect);

		public static void PlayerDisconnect(int playerIndex) {
			var player = Main.player[playerIndex];
			foreach (var modPlayer in HookPlayerDisconnect.Enumerate(player.modPlayers)) {
				modPlayer.PlayerDisconnect(player);
			}
		}

		private static HookList HookOnEnterWorld = AddHook<Action<Player>>(p => p.OnEnterWorld);

		// Do NOT hook into the Player.Hooks.OnEnterWorld event
		public static void OnEnterWorld(int playerIndex) {
			var player = Main.player[playerIndex];
			foreach (var modPlayer in HookOnEnterWorld.Enumerate(player.modPlayers)) {
				modPlayer.OnEnterWorld(player);
			}
		}

		private static HookList HookOnRespawn = AddHook<Action<Player>>(p => p.OnRespawn);

		public static void OnRespawn(Player player) {
			foreach (var modPlayer in HookOnRespawn.Enumerate(player.modPlayers)) {
				modPlayer.OnRespawn(player);
			}
		}

		private static HookList HookShiftClickSlot = AddHook<Func<Item[], int, int, bool>>(p => p.ShiftClickSlot);

		public static bool ShiftClickSlot(Player player, Item[] inventory, int context, int slot) {
			foreach (var modPlayer in HookShiftClickSlot.Enumerate(player.modPlayers)) {
				if (modPlayer.ShiftClickSlot(inventory, context, slot)) {
					return true;
				}
			}
			return false;
		}

		private static HookList HookHoverSlot = AddHook<Func<Item[], int, int, bool>>(p => p.HoverSlot);

		public static bool HoverSlot(Player player, Item[] inventory, int context, int slot) {
			foreach (var modPlayer in HookHoverSlot.Enumerate(player.ModPlayers)) {
				if (modPlayer.HoverSlot(inventory, context, slot)) {
					return true;
				}
			}
			return false;
		}

		private static HookList HookPostSellItem = AddHook<Action<NPC, Item[], Item>>(p => p.PostSellItem);

		public static void PostSellItem(Player player, NPC npc, Item[] shopInventory, Item item) {
			foreach (var modPlayer in HookPostSellItem.Enumerate(player.modPlayers)) {
				modPlayer.PostSellItem(npc, shopInventory, item);
			}
		}

		private static HookList HookCanSellItem = AddHook<Func<NPC, Item[], Item, bool>>(p => p.CanSellItem);

		// TODO: GlobalNPC and ModNPC hooks for Buy/Sell hooks as well.
		public static bool CanSellItem(Player player, NPC npc, Item[] shopInventory, Item item) {
			foreach (var modPlayer in HookCanSellItem.Enumerate(player.modPlayers)) {
				if (!modPlayer.CanSellItem(npc, shopInventory, item))
					return false;
			}
			return true;
		}

		private static HookList HookPostBuyItem = AddHook<Action<NPC, Item[], Item>>(p => p.PostBuyItem);

		public static void PostBuyItem(Player player, NPC npc, Item[] shopInventory, Item item) {
			foreach (var modPlayer in HookPostBuyItem.Enumerate(player.modPlayers)) {
				modPlayer.PostBuyItem(npc, shopInventory, item);
			}
		}

		private static HookList HookCanBuyItem = AddHook<Func<NPC, Item[], Item, bool>>(p => p.CanBuyItem);

		public static bool CanBuyItem(Player player, NPC npc, Item[] shopInventory, Item item) {
			foreach (var modPlayer in HookCanBuyItem.Enumerate(player.modPlayers)) {
				if (!modPlayer.CanBuyItem(npc, shopInventory, item))
					return false;
			}
			return true;
		}

		private static HookList HookCanUseItem = AddHook<Func<Item, bool>>(p => p.CanUseItem);

		public static bool CanUseItem(Player player, Item item) {
			foreach (var modPlayer in HookCanUseItem.Enumerate(player.modPlayers)) {
				if (!modPlayer.CanUseItem(item))
					return false;
			}

			return true;
		}

		private static HookList HookCanAutoReuseItem = AddHook<Func<Item, bool?>>(p => p.CanAutoReuseItem);

		public static bool? CanAutoReuseItem(Player player, Item item) {
			bool? flag = null;

			foreach (var modPlayer in HookCanAutoReuseItem.Enumerate(player.modPlayers)) {
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

		public static bool ModifyNurseHeal(Player player, NPC npc, ref int health, ref bool removeDebuffs, ref string chat) {
			foreach (var modPlayer in HookModifyNurseHeal.Enumerate(player.modPlayers)) {
				if (!modPlayer.ModifyNurseHeal(npc, ref health, ref removeDebuffs, ref chat))
					return false;
			}

			return true;
		}

		private delegate void DelegateModifyNursePrice(NPC npc, int health, bool removeDebuffs, ref int price);
		private static HookList HookModifyNursePrice = AddHook<DelegateModifyNursePrice>(p => p.ModifyNursePrice);

		public static void ModifyNursePrice(Player player, NPC npc, int health, bool removeDebuffs, ref int price) {
			foreach (var modPlayer in HookModifyNursePrice.Enumerate(player.modPlayers)) {
				modPlayer.ModifyNursePrice(npc, health, removeDebuffs, ref price);
			}
		}

		private static HookList HookPostNurseHeal = AddHook<Action<NPC, int, bool, int>>(p => p.PostNurseHeal);

		public static void PostNurseHeal(Player player, NPC npc, int health, bool removeDebuffs, int price) {
			foreach (var modPlayer in HookPostNurseHeal.Enumerate(player.modPlayers)) {
				modPlayer.PostNurseHeal(npc, health, removeDebuffs, price);
			}
		}

		private static HookList HookAddStartingItems = AddHook<Func<bool, IEnumerable<Item>>>(p => p.AddStartingItems);
		private static HookList HookModifyStartingInventory = AddHook<Action<IReadOnlyDictionary<string, List<Item>>, bool>>(p => p.ModifyStartingInventory);

		public static List<Item> GetStartingItems(Player player, IEnumerable<Item> vanillaItems, bool mediumCoreDeath = false) {
			var itemsByMod = new Dictionary<string, List<Item>> {
				["Terraria"] = vanillaItems.ToList()
			};

			foreach (var modPlayer in HookAddStartingItems.Enumerate(player.modPlayers)) {
				itemsByMod[modPlayer.Mod.Name] = modPlayer.AddStartingItems(mediumCoreDeath).ToList();
			}

			foreach (var modPlayer in HookModifyStartingInventory.Enumerate(player.modPlayers)) {
				modPlayer.ModifyStartingInventory(itemsByMod, mediumCoreDeath);
			}

			return itemsByMod
				.OrderBy(kv => kv.Key == "Terraria" ? "" : kv.Key)
				.SelectMany(kv => kv.Value)
				.ToList();
		}
	}
}