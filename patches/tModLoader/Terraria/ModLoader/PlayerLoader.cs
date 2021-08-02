using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader
{
	//todo: further documentation
	/// <summary>
	/// This is where all ModPlayer hooks are gathered and called.
	/// </summary>
	public static class PlayerLoader
	{
		private static readonly IList<ModPlayer> players = new List<ModPlayer>();

		private class HookList
		{
			public int[] arr = new int[0];
			public readonly MethodInfo method;

			public HookList(MethodInfo method) {
				this.method = method;
			}
		}

		private static List<HookList> hooks = new List<HookList>();

		private static HookList AddHook<F>(Expression<Func<ModPlayer, F>> func) {
			var hook = new HookList(ModLoader.Method(func));
			hooks.Add(hook);
			return hook;
		}

		internal static void Add(ModPlayer player) {
			player.index = players.Count;
			players.Add(player);
		}

		internal static void RebuildHooks() {
			foreach (var hook in hooks) {
				hook.arr = ModLoader.BuildGlobalHook(players, hook.method).Select(p => p.index).ToArray();
			}
		}

		internal static void Unload() {
			players.Clear();
		}

		internal static void SetupPlayer(Player player) {
			player.modPlayers = players.Select(modPlayer => modPlayer.CreateFor(player)).ToArray();
		}

		private static HookList HookResetEffects = AddHook<Action>(p => p.ResetEffects);

		public static void ResetEffects(Player player) {
			foreach (int index in HookResetEffects.arr) {
				player.modPlayers[index].ResetEffects();
			}
		}

		private static HookList HookUpdateDead = AddHook<Action>(p => p.UpdateDead);

		public static void UpdateDead(Player player) {
			foreach (int index in HookUpdateDead.arr) {
				player.modPlayers[index].UpdateDead();
			}
		}

		public static void SetStartInventory(Player player, IList<Item> items) {
			if (items.Count <= 50) {
				for (int k = 0; k < items.Count && k < 49; k++)
					player.inventory[k] = items[k];
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
			foreach (int index in HookPreSavePlayer.arr) {
				player.modPlayers[index].PreSavePlayer();
			}
		}

		private static HookList HookPostSavePlayer = AddHook<Action>(p => p.PostSavePlayer);

		public static void PostSavePlayer(Player player) {
			foreach (int index in HookPostSavePlayer.arr) {
				player.modPlayers[index].PostSavePlayer();
			}
		}

		private static HookList HookClientClone = AddHook<Action<ModPlayer>>(p => p.clientClone);

		public static void clientClone(Player player, Player clientClone) {
			foreach (int index in HookClientClone.arr) {
				player.modPlayers[index].clientClone(clientClone.modPlayers[index]);
			}
		}

		private static HookList HookSyncPlayer = AddHook<Action<int, int, bool>>(p => p.SyncPlayer);

		public static void SyncPlayer(Player player, int toWho, int fromWho, bool newPlayer) {
			foreach (int index in HookSyncPlayer.arr) {
				player.modPlayers[index].SyncPlayer(toWho, fromWho, newPlayer);
			}
		}

		private static HookList HookSendClientChanges = AddHook<Action<ModPlayer>>(p => p.SendClientChanges);

		public static void SendClientChanges(Player player, Player clientPlayer) {
			foreach (int index in HookSendClientChanges.arr) {
				player.modPlayers[index].SendClientChanges(clientPlayer.modPlayers[index]);
			}
		}

		private static HookList HookGetMapBackgroundImage = AddHook<Func<Texture2D>>(p => p.GetMapBackgroundImage);

		public static Texture2D GetMapBackgroundImage(Player player) {
			Texture2D texture = null;
			foreach (int index in HookGetMapBackgroundImage.arr) {
				texture = player.modPlayers[index].GetMapBackgroundImage();
				if (texture != null) {
					return texture;
				}
			}
			return texture;
		}

		private static HookList HookUpdateBadLifeRegen = AddHook<Action>(p => p.UpdateBadLifeRegen);

		public static void UpdateBadLifeRegen(Player player) {
			foreach (int index in HookUpdateBadLifeRegen.arr) {
				player.modPlayers[index].UpdateBadLifeRegen();
			}
		}

		private static HookList HookUpdateLifeRegen = AddHook<Action>(p => p.UpdateLifeRegen);

		public static void UpdateLifeRegen(Player player) {
			foreach (int index in HookUpdateLifeRegen.arr) {
				player.modPlayers[index].UpdateLifeRegen();
			}
		}

		private delegate void DelegateNaturalLifeRegen(ref float regen);
		private static HookList HookNaturalLifeRegen = AddHook<DelegateNaturalLifeRegen>(p => p.NaturalLifeRegen);

		public static void NaturalLifeRegen(Player player, ref float regen) {
			foreach (int index in HookNaturalLifeRegen.arr) {
				player.modPlayers[index].NaturalLifeRegen(ref regen);
			}
		}

		private static HookList HookUpdateAutopause = AddHook<Action>(p => p.UpdateAutopause);

		public static void UpdateAutopause(Player player) {
			foreach (int index in HookUpdateAutopause.arr) {
				player.modPlayers[index].UpdateAutopause();
			}
		}

		private static HookList HookPreUpdate = AddHook<Action>(p => p.PreUpdate);

		public static void PreUpdate(Player player) {
			foreach (int index in HookPreUpdate.arr) {
				player.modPlayers[index].PreUpdate();
			}
		}

		private static HookList HookSetControls = AddHook<Action>(p => p.SetControls);

		public static void SetControls(Player player) {
			foreach (int index in HookSetControls.arr) {
				player.modPlayers[index].SetControls();
			}
		}

		private static HookList HookPreUpdateBuffs = AddHook<Action>(p => p.PreUpdateBuffs);

		public static void PreUpdateBuffs(Player player) {
			foreach (int index in HookPreUpdateBuffs.arr) {
				player.modPlayers[index].PreUpdateBuffs();
			}
		}

		private static HookList HookPostUpdateBuffs = AddHook<Action>(p => p.PostUpdateBuffs);

		public static void PostUpdateBuffs(Player player) {
			foreach (int index in HookPostUpdateBuffs.arr) {
				player.modPlayers[index].PostUpdateBuffs();
			}
		}

		private delegate void DelegateUpdateEquips();
		private static HookList HookUpdateEquips = AddHook<DelegateUpdateEquips>(p => p.UpdateEquips);

		public static void UpdateEquips(Player player) {
			foreach (int index in HookUpdateEquips.arr) {
				player.modPlayers[index].UpdateEquips();
			}
		}

		private static HookList HookUpdateVanityAccessories = AddHook<Action>(p => p.UpdateVanityAccessories);

		public static void UpdateVanityAccessories(Player player) {
			foreach (int index in HookUpdateVanityAccessories.arr) {
				player.modPlayers[index].UpdateVanityAccessories();
			}
		}

		private static HookList HookPostUpdateEquips = AddHook<Action>(p => p.PostUpdateEquips);

		public static void PostUpdateEquips(Player player) {
			foreach (int index in HookPostUpdateEquips.arr) {
				player.modPlayers[index].PostUpdateEquips();
			}
		}

		private static HookList HookPostUpdateMiscEffects = AddHook<Action>(p => p.PostUpdateMiscEffects);

		public static void PostUpdateMiscEffects(Player player) {
			foreach (int index in HookPostUpdateMiscEffects.arr) {
				player.modPlayers[index].PostUpdateMiscEffects();
			}
		}

		private static HookList HookPostUpdateRunSpeeds = AddHook<Action>(p => p.PostUpdateRunSpeeds);

		public static void PostUpdateRunSpeeds(Player player) {
			foreach (int index in HookPostUpdateRunSpeeds.arr) {
				player.modPlayers[index].PostUpdateRunSpeeds();
			}
		}

		private static HookList HookPreUpdateMovement = AddHook<Action>(p => p.PreUpdateMovement);

		public static void PreUpdateMovement(Player player) {
			foreach (int index in HookPreUpdateMovement.arr) {
				player.modPlayers[index].PreUpdateMovement();
			}
		}

		private static HookList HookPostUpdate = AddHook<Action>(p => p.PostUpdate);

		public static void PostUpdate(Player player) {
			foreach (int index in HookPostUpdate.arr) {
				player.modPlayers[index].PostUpdate();
			}
		}

		private static HookList HookFrameEffects = AddHook<Action>(p => p.FrameEffects);

		public static void FrameEffects(Player player) {
			foreach (int index in HookFrameEffects.arr) {
				player.modPlayers[index].FrameEffects();
			}
		}

		private delegate bool DelegatePreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection,
			ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource);
		private static HookList HookPreHurt = AddHook<DelegatePreHurt>(p => p.PreHurt);

		public static bool PreHurt(Player player, bool pvp, bool quiet, ref int damage, ref int hitDirection,
			ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) {
			bool flag = true;
			foreach (int index in HookPreHurt.arr) {
				if (!player.modPlayers[index].PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage,
						ref playSound, ref genGore, ref damageSource)) {
					flag = false;
				}
			}
			return flag;
		}

		private static HookList HookHurt = AddHook<Action<bool, bool, double, int, bool>>(p => p.Hurt);

		public static void Hurt(Player player, bool pvp, bool quiet, double damage, int hitDirection, bool crit) {
			foreach (int index in HookHurt.arr) {
				player.modPlayers[index].Hurt(pvp, quiet, damage, hitDirection, crit);
			}
		}

		private static HookList HookPostHurt = AddHook<Action<bool, bool, double, int, bool>>(p => p.PostHurt);

		public static void PostHurt(Player player, bool pvp, bool quiet, double damage, int hitDirection, bool crit) {
			foreach (int index in HookPostHurt.arr) {
				player.modPlayers[index].PostHurt(pvp, quiet, damage, hitDirection, crit);
			}
		}

		private delegate bool DelegatePreKill(double damage, int hitDirection, bool pvp, ref bool playSound,
			ref bool genGore, ref PlayerDeathReason damageSource);
		private static HookList HookPreKill = AddHook<DelegatePreKill>(p => p.PreKill);

		public static bool PreKill(Player player, double damage, int hitDirection, bool pvp, ref bool playSound,
			ref bool genGore, ref PlayerDeathReason damageSource) {
			bool flag = true;
			foreach (int index in HookPreKill.arr) {
				if (!player.modPlayers[index].PreKill(damage, hitDirection, pvp, ref playSound, ref genGore, ref damageSource)) {
					flag = false;
				}
			}
			return flag;
		}

		private static HookList HookKill = AddHook<Action<double, int, bool, PlayerDeathReason>>(p => p.Kill);

		public static void Kill(Player player, double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
			foreach (int index in HookKill.arr) {
				player.modPlayers[index].Kill(damage, hitDirection, pvp, damageSource);
			}
		}

		private delegate bool DelegatePreModifyLuck(ref float luck);
		private static HookList HookPreModifyLuck = AddHook<DelegatePreModifyLuck>(p => p.PreModifyLuck);

		public static bool PreModifyLuck(Player player, ref float luck) {
			bool flag = true;
			foreach (int index in HookPreModifyLuck.arr) {
				if (!player.modPlayers[index].PreModifyLuck(ref luck)) {
					flag = false;
				}
			}
			return flag;
		}

		private delegate void DelegateModifyLuck(ref float luck);
		private static HookList HookModifyLuck = AddHook<DelegateModifyLuck>(p => p.ModifyLuck);

		public static void ModifyLuck(Player player, ref float luck) {
			foreach (int index in HookModifyLuck.arr) {
				player.modPlayers[index].ModifyLuck(ref luck);
			}
		}

		private static HookList HookPreItemCheck = AddHook<Func<bool>>(p => p.PreItemCheck);

		public static bool PreItemCheck(Player player) {
			bool result = true;
			foreach (int index in HookPreItemCheck.arr) {
				result &= player.modPlayers[index].PreItemCheck();
			}
			return result;
		}

		private static HookList HookPostItemCheck = AddHook<Action>(p => p.PostItemCheck);

		public static void PostItemCheck(Player player) {
			foreach (int index in HookPostItemCheck.arr) {
				player.modPlayers[index].PostItemCheck();
			}
		}

		private static HookList HookUseTimeMultiplier = AddHook<Func<Item, float>>(p => p.UseTimeMultiplier);

		public static float UseTimeMultiplier(Player player, Item item) {
			float multiplier = 1f;

			if (item.IsAir)
				return multiplier;

			foreach (int index in HookUseTimeMultiplier.arr) {
				multiplier *= player.modPlayers[index].UseTimeMultiplier(item);
			}

			return multiplier;
		}

		private static HookList HookUseAnimationMultiplier = AddHook<Func<Item, float>>(p => p.UseAnimationMultiplier);

		public static float UseAnimationMultiplier(Player player, Item item) {
			float multiplier = 1f;

			if (item.IsAir)
				return multiplier;

			foreach (int index in HookUseAnimationMultiplier.arr) {
				multiplier *= player.modPlayers[index].UseAnimationMultiplier(item);
			}

			return multiplier;
		}

		private static HookList HookUseSpeedMultiplier = AddHook<Func<Item, float>>(p => p.UseSpeedMultiplier);

		public static float UseSpeedMultiplier(Player player, Item item) {
			float multiplier = 1f;

			if (item.IsAir)
				return multiplier;

			foreach (int index in HookUseSpeedMultiplier.arr) {
				multiplier *= player.modPlayers[index].UseSpeedMultiplier(item);
			}

			return multiplier;
		}

		private delegate void DelegateGetHealLife(Item item, bool quickHeal, ref int healValue);
		private static HookList HookGetHealLife = AddHook<DelegateGetHealLife>(p => p.GetHealLife);

		public static void GetHealLife(Player player, Item item, bool quickHeal, ref int healValue) {
			if (item.IsAir)
				return;

			foreach (int index in HookGetHealLife.arr) {
				player.modPlayers[index].GetHealLife(item, quickHeal, ref healValue);
			}
		}

		private delegate void DelegateGetHealMana(Item item, bool quickHeal, ref int healValue);
		private static HookList HookGetHealMana = AddHook<DelegateGetHealMana>(p => p.GetHealMana);

		public static void GetHealMana(Player player, Item item, bool quickHeal, ref int healValue) {
			if (item.IsAir)
				return;

			foreach (int index in HookGetHealMana.arr) {
				player.modPlayers[index].GetHealMana(item, quickHeal, ref healValue);
			}
		}

		private delegate void DelegateModifyManaCost(Item item, ref float reduce, ref float mult);
		private static HookList HookModifyManaCost = AddHook<DelegateModifyManaCost>(p => p.ModifyManaCost);

		public static void ModifyManaCost(Player player, Item item, ref float reduce, ref float mult) {
			if (item.IsAir)
				return;
			
			foreach (int index in HookModifyManaCost.arr) {
				player.modPlayers[index].ModifyManaCost(item, ref reduce, ref mult);
			}
		}

		private static HookList HookOnMissingMana = AddHook<Action<Item, int>>(p => p.OnMissingMana);

		public static void OnMissingMana(Player player, Item item, int manaNeeded) {
			if (item.IsAir)
				return;
			
			foreach (int index in HookOnMissingMana.arr) {
				player.modPlayers[index].OnMissingMana(item, manaNeeded);
			}
		}

		private static HookList HookOnConsumeMana = AddHook<Action<Item, int>>(p => p.OnConsumeMana);

		public static void OnConsumeMana(Player player, Item item, int manaConsumed) {
			if (item.IsAir)
				return;
			
			foreach (int index in HookOnConsumeMana.arr) {
				player.modPlayers[index].OnConsumeMana(item, manaConsumed);
			}
		}

		private delegate void DelegateModifyWeaponDamage(Item item, ref StatModifier damage, ref float flat);
		private static HookList HookModifyWeaponDamage = AddHook<DelegateModifyWeaponDamage>(p => p.ModifyWeaponDamage);
		/// <summary>
		/// Calls ModItem.HookModifyWeaponDamage, then all GlobalItem.HookModifyWeaponDamage hooks.
		/// </summary>
		public static void ModifyWeaponDamage(Player player, Item item, ref StatModifier damage, ref float flat) {
			if (item.IsAir)
				return;

			foreach (int index in HookModifyWeaponDamage.arr) {
				player.modPlayers[index].ModifyWeaponDamage(item, ref damage, ref flat);
			}
		}

		private static HookList HookProcessTriggers = AddHook<Action<TriggersSet>>(p => p.ProcessTriggers);

		public static void ProcessTriggers(Player player, TriggersSet triggersSet) {
			foreach (int index in HookProcessTriggers.arr) {
				player.modPlayers[index].ProcessTriggers(triggersSet);
			}
		}

		private delegate void DelegateModifyWeaponKnockback(Item item, ref StatModifier knockback, ref float flat);
		private static HookList HookModifyWeaponKnockback = AddHook<DelegateModifyWeaponKnockback>(p => p.ModifyWeaponKnockback);

		public static void ModifyWeaponKnockback(Player player, Item item, ref StatModifier knockback, ref float flat) {
			if (item.IsAir)
				return;

			foreach (int index in HookModifyWeaponKnockback.arr) {
				player.modPlayers[index].ModifyWeaponKnockback(item, ref knockback, ref flat);
			}
		}

		private delegate void DelegateModifyWeaponCrit(Item item, ref int crit);
		private static HookList HookModifyWeaponCrit = AddHook<DelegateModifyWeaponCrit>(p => p.ModifyWeaponCrit);

		public static void ModifyWeaponCrit(Player player, Item item, ref int crit) {
			if (item.IsAir) return;
			foreach (int index in HookModifyWeaponCrit.arr) {
				player.modPlayers[index].ModifyWeaponCrit(item, ref crit);
			}
		}

		private static HookList HookConsumeAmmo = AddHook<Func<Item, Item, bool>>(p => p.ConsumeAmmo);

		public static bool ConsumeAmmo(Player player, Item weapon, Item ammo) {
			foreach (int index in HookConsumeAmmo.arr) {
				if (!player.modPlayers[index].ConsumeAmmo(weapon, ammo)) {
					return false;
				}
			}
			return true;
		}

		private static HookList HookOnConsumeAmmo = AddHook<Action<Item, Item>>(p => p.OnConsumeAmmo);

		public static void OnConsumeAmmo(Player player, Item weapon, Item ammo) {
			foreach (int index in HookOnConsumeAmmo.arr)
				player.modPlayers[index].OnConsumeAmmo(weapon, ammo);
		}

		private static HookList HookCanShoot = AddHook<Func<Item, bool>>(p => p.CanShoot);

		public static bool CanShoot(Player player, Item item) {
			foreach (int index in HookCanShoot.arr) {
				if (!player.modPlayers[index].CanShoot(item))
					return false;
			}

			return true;
		}

		private delegate void DelegateModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback);
		private static HookList HookModifyShootStats = AddHook<DelegateModifyShootStats>(p => p.ModifyShootStats);

		public static void ModifyShootStats(Player player, Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			foreach (int index in HookModifyShootStats.arr) {
				player.modPlayers[index].ModifyShootStats(item, ref position, ref velocity, ref type, ref damage, ref knockback);
			}
		}

		private static HookList HookShoot = AddHook<Func<Item, ProjectileSource_Item_WithAmmo, Vector2, Vector2, int, int, float, bool>>(p => p.Shoot);

		public static bool Shoot(Player player, Item item, ProjectileSource_Item_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			bool defaultResult = true;

			foreach (int index in HookShoot.arr) {
				defaultResult &= player.modPlayers[index].Shoot(item, source, position, velocity, type, damage, knockback);
			}

			return defaultResult;
		}

		private static HookList HookMeleeEffects = AddHook<Action<Item, Rectangle>>(p => p.MeleeEffects);

		public static void MeleeEffects(Player player, Item item, Rectangle hitbox) {
			foreach (int index in HookMeleeEffects.arr) {
				player.modPlayers[index].MeleeEffects(item, hitbox);
			}
		}

		private static HookList HookOnHitAnything = AddHook<Action<float, float, Entity>>(p => p.OnHitAnything);

		public static void OnHitAnything(Player player, float x, float y, Entity victim) {
			foreach (int index in HookOnHitAnything.arr) {
				player.modPlayers[index].OnHitAnything(x, y, victim);
			}
		}

		private static HookList HookCanHitNPC = AddHook<Func<Item, NPC, bool?>>(p => p.CanHitNPC);

		public static bool? CanHitNPC(Player player, Item item, NPC target) {
			bool? flag = null;

			foreach (int index in HookCanHitNPC.arr) {
				bool? canHit = player.modPlayers[index].CanHitNPC(item, target);

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
			foreach (int index in HookModifyHitNPC.arr) {
				player.modPlayers[index].ModifyHitNPC(item, target, ref damage, ref knockback, ref crit);
			}
		}

		private static HookList HookOnHitNPC = AddHook<Action<Item, NPC, int, float, bool>>(p => p.OnHitNPC);

		public static void OnHitNPC(Player player, Item item, NPC target, int damage, float knockback, bool crit) {
			foreach (int index in HookOnHitNPC.arr) {
				player.modPlayers[index].OnHitNPC(item, target, damage, knockback, crit);
			}
		}

		private static HookList HookCanHitNPCWithProj = AddHook<Func<Projectile, NPC, bool?>>(p => p.CanHitNPCWithProj);

		public static bool? CanHitNPCWithProj(Projectile proj, NPC target) {
			if (proj.npcProj || proj.trap) {
				return null;
			}
			Player player = Main.player[proj.owner];
			bool? flag = null;
			foreach (int index in HookCanHitNPCWithProj.arr) {
				bool? canHit = player.modPlayers[index].CanHitNPCWithProj(proj, target);
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
			foreach (int index in HookModifyHitNPCWithProj.arr) {
				player.modPlayers[index].ModifyHitNPCWithProj(proj, target, ref damage, ref knockback, ref crit, ref hitDirection);
			}
		}

		private static HookList HookOnHitNPCWithProj = AddHook<Action<Projectile, NPC, int, float, bool>>(p => p.OnHitNPCWithProj);

		public static void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit) {
			if (proj.npcProj || proj.trap) {
				return;
			}
			Player player = Main.player[proj.owner];
			foreach (int index in HookOnHitNPCWithProj.arr) {
				player.modPlayers[index].OnHitNPCWithProj(proj, target, damage, knockback, crit);
			}
		}

		private static HookList HookCanHitPvp = AddHook<Func<Item, Player, bool>>(p => p.CanHitPvp);

		public static bool CanHitPvp(Player player, Item item, Player target) {
			foreach (int index in HookCanHitPvp.arr) {
				if (!player.modPlayers[index].CanHitPvp(item, target)) {
					return false;
				}
			}
			return true;
		}

		private delegate void DelegateModifyHitPvp(Item item, Player target, ref int damage, ref bool crit);
		private static HookList HookModifyHitPvp = AddHook<DelegateModifyHitPvp>(p => p.ModifyHitPvp);

		public static void ModifyHitPvp(Player player, Item item, Player target, ref int damage, ref bool crit) {
			foreach (int index in HookModifyHitPvp.arr) {
				player.modPlayers[index].ModifyHitPvp(item, target, ref damage, ref crit);
			}
		}

		private static HookList HookOnHitPvp = AddHook<Action<Item, Player, int, bool>>(p => p.OnHitPvp);

		public static void OnHitPvp(Player player, Item item, Player target, int damage, bool crit) {
			foreach (int index in HookOnHitPvp.arr) {
				player.modPlayers[index].OnHitPvp(item, target, damage, crit);
			}
		}

		private static HookList HookCanHitPvpWithProj = AddHook<Func<Projectile, Player, bool>>(p => p.CanHitPvpWithProj);

		public static bool CanHitPvpWithProj(Projectile proj, Player target) {
			Player player = Main.player[proj.owner];
			foreach (int index in HookCanHitPvpWithProj.arr) {
				if (!player.modPlayers[index].CanHitPvpWithProj(proj, target)) {
					return false;
				}
			}
			return true;
		}

		private delegate void DelegateModifyHitPvpWithProj(Projectile proj, Player target, ref int damage, ref bool crit);
		private static HookList HookModifyHitPvpWithProj = AddHook<DelegateModifyHitPvpWithProj>(p => p.ModifyHitPvpWithProj);

		public static void ModifyHitPvpWithProj(Projectile proj, Player target, ref int damage, ref bool crit) {
			Player player = Main.player[proj.owner];
			foreach (int index in HookModifyHitPvpWithProj.arr) {
				player.modPlayers[index].ModifyHitPvpWithProj(proj, target, ref damage, ref crit);
			}
		}

		private static HookList HookOnHitPvpWithProj = AddHook<Action<Projectile, Player, int, bool>>(p => p.OnHitPvpWithProj);

		public static void OnHitPvpWithProj(Projectile proj, Player target, int damage, bool crit) {
			Player player = Main.player[proj.owner];
			foreach (int index in HookOnHitPvpWithProj.arr) {
				player.modPlayers[index].OnHitPvpWithProj(proj, target, damage, crit);
			}
		}

		private delegate bool DelegateCanBeHitByNPC(NPC npc, ref int cooldownSlot);
		private static HookList HookCanBeHitByNPC = AddHook<DelegateCanBeHitByNPC>(p => p.CanBeHitByNPC);

		public static bool CanBeHitByNPC(Player player, NPC npc, ref int cooldownSlot) {
			foreach (int index in HookCanBeHitByNPC.arr) {
				if (!player.modPlayers[index].CanBeHitByNPC(npc, ref cooldownSlot)) {
					return false;
				}
			}
			return true;
		}

		private delegate void DelegateModifyHitByNPC(NPC npc, ref int damage, ref bool crit);
		private static HookList HookModifyHitByNPC = AddHook<DelegateModifyHitByNPC>(p => p.ModifyHitByNPC);

		public static void ModifyHitByNPC(Player player, NPC npc, ref int damage, ref bool crit) {
			foreach (int index in HookModifyHitByNPC.arr) {
				player.modPlayers[index].ModifyHitByNPC(npc, ref damage, ref crit);
			}
		}

		private static HookList HookOnHitByNPC = AddHook<Action<NPC, int, bool>>(p => p.OnHitByNPC);

		public static void OnHitByNPC(Player player, NPC npc, int damage, bool crit) {
			foreach (int index in HookOnHitByNPC.arr) {
				player.modPlayers[index].OnHitByNPC(npc, damage, crit);
			}
		}

		private static HookList HookCanBeHitByProjectile = AddHook<Func<Projectile, bool>>(p => p.CanBeHitByProjectile);

		public static bool CanBeHitByProjectile(Player player, Projectile proj) {
			foreach (int index in HookCanBeHitByProjectile.arr) {
				if (!player.modPlayers[index].CanBeHitByProjectile(proj)) {
					return false;
				}
			}
			return true;
		}

		private delegate void DelegateModifyHitByProjectile(Projectile proj, ref int damage, ref bool crit);
		private static HookList HookModifyHitByProjectile = AddHook<DelegateModifyHitByProjectile>(p => p.ModifyHitByProjectile);

		public static void ModifyHitByProjectile(Player player, Projectile proj, ref int damage, ref bool crit) {
			foreach (int index in HookModifyHitByProjectile.arr) {
				player.modPlayers[index].ModifyHitByProjectile(proj, ref damage, ref crit);
			}
		}

		private static HookList HookOnHitByProjectile = AddHook<Action<Projectile, int, bool>>(p => p.OnHitByProjectile);

		public static void OnHitByProjectile(Player player, Projectile proj, int damage, bool crit) {
			foreach (int index in HookOnHitByProjectile.arr) {
				player.modPlayers[index].OnHitByProjectile(proj, damage, crit);
			}
		}

		private delegate void DelegateCatchFish(Item fishingRod, Item bait, int power, int liquidType, int poolSize, int worldLayer, int questFish, ref int caughtType);
		private static HookList HookCatchFish = AddHook<DelegateCatchFish>(p => p.CatchFish);

		public static void CatchFish(Player player, Item fishingRod, int power, int liquidType, int poolSize, int worldLayer, int questFish, ref int caughtType) {
			int i = 0;
			while (i < 58) {
				if (player.inventory[i].stack > 0 && player.inventory[i].bait > 0) {
					break;
				}
				i++;
			}
			foreach (int index in HookCatchFish.arr) {
				player.modPlayers[index].CatchFish(fishingRod, player.inventory[i], power, liquidType, poolSize, worldLayer, questFish, ref caughtType);
			}
		}

		private delegate void DelegateGetFishingLevel(Item fishingRod, Item bait, ref float fishingLevel);
		private static HookList HookGetFishingLevel = AddHook<DelegateGetFishingLevel>(p => p.GetFishingLevel);

		public static void GetFishingLevel(Player player, Item fishingRod, Item bait, ref float fishingLevel) {
			foreach (int index in HookGetFishingLevel.arr) {
				player.modPlayers[index].GetFishingLevel(fishingRod, bait, ref fishingLevel);
			}
		}

		private static HookList HookAnglerQuestReward = AddHook<Action<float, List<Item>>>(p => p.AnglerQuestReward);

		public static void AnglerQuestReward(Player player, float rareMultiplier, List<Item> rewardItems) {
			foreach (int index in HookAnglerQuestReward.arr) {
				player.modPlayers[index].AnglerQuestReward(rareMultiplier, rewardItems);
			}
		}

		private static HookList HookGetDyeTraderReward = AddHook<Action<List<int>>>(p => p.GetDyeTraderReward);

		public static void GetDyeTraderReward(Player player, List<int> rewardPool) {
			foreach (int index in HookGetDyeTraderReward.arr) {
				player.modPlayers[index].GetDyeTraderReward(rewardPool);
			}
		}

		private delegate void DelegateDrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright);
		private static HookList HookDrawEffects = AddHook<DelegateDrawEffects>(p => p.DrawEffects);

		public static void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
			ModPlayer[] modPlayers = drawInfo.drawPlayer.modPlayers;
			foreach (int index in HookDrawEffects.arr) {
				modPlayers[index].DrawEffects(drawInfo, ref r, ref g, ref b, ref a, ref fullBright);
			}
		}

		private delegate void DelegateModifyDrawInfo(ref PlayerDrawSet drawInfo);
		private static HookList HookModifyDrawInfo = AddHook<DelegateModifyDrawInfo>(p => p.ModifyDrawInfo);

		public static void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
			ModPlayer[] modPlayers = drawInfo.drawPlayer.modPlayers;
			foreach (int index in HookModifyDrawInfo.arr) {
				modPlayers[index].ModifyDrawInfo(ref drawInfo);
			}
		}

		private static HookList HookModifyDrawLayerOrdering = AddHook<Action<IDictionary<PlayerDrawLayer, PlayerDrawLayer.Position>>>(p => p.ModifyDrawLayerOrdering);

		public static void ModifyDrawLayerOrdering(IDictionary<PlayerDrawLayer, PlayerDrawLayer.Position> positions) {
			foreach (int index in HookModifyDrawLayerOrdering.arr) {
				players[index].ModifyDrawLayerOrdering(positions);
			}
		}

		private static HookList HookModifyDrawLayers = AddHook<Action<PlayerDrawSet>>(p => p.HideDrawLayers);

		public static void HideDrawLayers(PlayerDrawSet drawInfo) {
			foreach (int index in HookModifyDrawLayers.arr) {
				drawInfo.drawPlayer.modPlayers[index].HideDrawLayers(drawInfo);
			}
		}

		private static HookList HookModifyScreenPosition = AddHook<Action>(p => p.ModifyScreenPosition);

		public static void ModifyScreenPosition(Player player) {
			foreach (int index in HookModifyScreenPosition.arr) {
				player.modPlayers[index].ModifyScreenPosition();
			}
		}

		private delegate void DelegateModifyZoom(ref float zoom);
		private static HookList HookModifyZoom = AddHook<DelegateModifyZoom>(p => p.ModifyZoom);

		public static void ModifyZoom(Player player, ref float zoom) {
			foreach (int index in HookModifyZoom.arr) {
				player.modPlayers[index].ModifyZoom(ref zoom);
			}
		}

		private static HookList HookPlayerConnect = AddHook<Action<Player>>(p => p.PlayerConnect);

		public static void PlayerConnect(int playerIndex) {
			var player = Main.player[playerIndex];
			foreach (int index in HookPlayerConnect.arr) {
				player.modPlayers[index].PlayerConnect(player);
			}
		}

		private static HookList HookPlayerDisconnect = AddHook<Action<Player>>(p => p.PlayerDisconnect);

		public static void PlayerDisconnect(int playerIndex) {
			var player = Main.player[playerIndex];
			foreach (int index in HookPlayerDisconnect.arr) {
				player.modPlayers[index].PlayerDisconnect(player);
			}
		}

		private static HookList HookOnEnterWorld = AddHook<Action<Player>>(p => p.OnEnterWorld);

		// Do NOT hook into the Player.Hooks.OnEnterWorld event
		public static void OnEnterWorld(int playerIndex) {
			var player = Main.player[playerIndex];
			foreach (int index in HookOnEnterWorld.arr) {
				player.modPlayers[index].OnEnterWorld(player);
			}
		}

		private static HookList HookOnRespawn = AddHook<Action<Player>>(p => p.OnRespawn);

		public static void OnRespawn(Player player) {
			foreach (int index in HookOnRespawn.arr) {
				player.modPlayers[index].OnRespawn(player);
			}
		}

		private static HookList HookShiftClickSlot = AddHook<Func<Item[], int, int, bool>>(p => p.ShiftClickSlot);

		public static bool ShiftClickSlot(Player player, Item[] inventory, int context, int slot) {
			foreach (int index in HookShiftClickSlot.arr) {
				if (player.modPlayers[index].ShiftClickSlot(inventory, context, slot)) {
					return true;
				}
			}
			return false;
		}

		private static bool HasMethod(Type t, string method, params Type[] args) {
			return t.GetMethod(method, args).DeclaringType != typeof(ModPlayer);
		}

		internal static void VerifyGlobalItem(ModPlayer player) {
			var type = player.GetType();

			int netClientMethods = 0;
			if (HasMethod(type, "clientClone", typeof(ModPlayer))) netClientMethods++;
			if (HasMethod(type, "SyncPlayer", typeof(int), typeof(int), typeof(bool))) netClientMethods++;
			if (HasMethod(type, "SendClientChanges", typeof(ModPlayer))) netClientMethods++;
			if (netClientMethods > 0 && netClientMethods < 3)
				throw new Exception(type + " must override all of (clientClone/SyncPlayer/SendClientChanges) or none");

			int saveMethods = 0;
			if (HasMethod(type, "Save")) saveMethods++;
			if (HasMethod(type, "Load", typeof(TagCompound))) saveMethods++;
			if (saveMethods == 1)
				throw new Exception(type + " must override all of (Save/Load) or none");

			int netMethods = 0;
			if (HasMethod(type, "NetSend", typeof(BinaryWriter))) netMethods++;
			if (HasMethod(type, "NetReceive", typeof(BinaryReader))) netMethods++;
			if (netMethods == 1)
				throw new Exception(type + " must override both of (NetSend/NetReceive) or none");
		}

		private static HookList HookPostSellItem = AddHook<Action<NPC, Item[], Item>>(p => p.PostSellItem);

		public static void PostSellItem(Player player, NPC npc, Item[] shopInventory, Item item) {
			foreach (int index in HookPostSellItem.arr) {
				player.modPlayers[index].PostSellItem(npc, shopInventory, item);
			}
		}

		private static HookList HookCanSellItem = AddHook<Func<NPC, Item[], Item, bool>>(p => p.CanSellItem);

		// TODO: GlobalNPC and ModNPC hooks for Buy/Sell hooks as well.
		public static bool CanSellItem(Player player, NPC npc, Item[] shopInventory, Item item) {
			foreach (int index in HookCanSellItem.arr) {
				if (!player.modPlayers[index].CanSellItem(npc, shopInventory, item))
					return false;
			}
			return true;
		}

		private static HookList HookPostBuyItem = AddHook<Action<NPC, Item[], Item>>(p => p.PostBuyItem);

		public static void PostBuyItem(Player player, NPC npc, Item[] shopInventory, Item item) {
			foreach (int index in HookPostBuyItem.arr) {
				player.modPlayers[index].PostBuyItem(npc, shopInventory, item);
			}
		}

		private static HookList HookCanBuyItem = AddHook<Func<NPC, Item[], Item, bool>>(p => p.CanBuyItem);

		public static bool CanBuyItem(Player player, NPC npc, Item[] shopInventory, Item item) {
			foreach (int index in HookCanBuyItem.arr) {
				if (!player.modPlayers[index].CanBuyItem(npc, shopInventory, item))
					return false;
			}
			return true;
		}

		private static HookList HookCanUseItem = AddHook<Func<Item, bool>>(p => p.CanUseItem);

		public static bool CanUseItem(Player player, Item item) {
			bool result = true;

			foreach (int index in HookCanUseItem.arr) {
				result &= player.modPlayers[index].CanUseItem(item);
			}

			return result;
		}

		private delegate bool DelegateModifyNurseHeal(NPC npc, ref int health, ref bool removeDebuffs, ref string chatText);
		private static HookList HookModifyNurseHeal = AddHook<DelegateModifyNurseHeal>(p => p.ModifyNurseHeal);

		public static bool ModifyNurseHeal(Player p, NPC npc, ref int health, ref bool removeDebuffs, ref string chat) {
			foreach (int index in HookModifyNurseHeal.arr) {
				if (!p.modPlayers[index].ModifyNurseHeal(npc, ref health, ref removeDebuffs, ref chat))
					return false;
			}
			return true;
		}

		private delegate void DelegateModifyNursePrice(NPC npc, int health, bool removeDebuffs, ref int price);
		private static HookList HookModifyNursePrice = AddHook<DelegateModifyNursePrice>(p => p.ModifyNursePrice);

		public static void ModifyNursePrice(Player p, NPC npc, int health, bool removeDebuffs, ref int price) {
			foreach (int index in HookModifyNursePrice.arr) {
				p.modPlayers[index].ModifyNursePrice(npc, health, removeDebuffs, ref price);
			}
		}

		private static HookList HookPostNurseHeal = AddHook<Action<NPC, int, bool, int>>(p => p.PostNurseHeal);

		public static void PostNurseHeal(Player player, NPC npc, int health, bool removeDebuffs, int price) {
			foreach (int index in HookPostNurseHeal.arr) {
				player.modPlayers[index].PostNurseHeal(npc, health, removeDebuffs, price);
			}
		}

		private static HookList HookAddStartingItems = AddHook<Func<bool, IEnumerable<Item>>>(p => p.AddStartingItems);
		private static HookList HookModifyStartingInventory = AddHook<Action<IReadOnlyDictionary<string, List<Item>>, bool>>(p => p.ModifyStartingInventory);

		public static List<Item> GetStartingItems(Player player, IEnumerable<Item> vanillaItems, bool mediumCoreDeath = false) {
			var itemsByMod = new Dictionary<string, List<Item>>();

			itemsByMod["Terraria"] = vanillaItems.ToList();

			foreach (int index in HookAddStartingItems.arr) {
				ModPlayer modPlayer = player.modPlayers[index];
				itemsByMod[modPlayer.Mod.Name] = modPlayer.AddStartingItems(mediumCoreDeath).ToList();
			}

			foreach (int index in HookModifyStartingInventory.arr) {
				player.modPlayers[index].ModifyStartingInventory(itemsByMod, mediumCoreDeath);
			}

			return itemsByMod
				.OrderBy(kv => kv.Key == "Terraria" ? "" : kv.Key)
				.SelectMany(kv => kv.Value)
				.ToList();
		}
	}
}