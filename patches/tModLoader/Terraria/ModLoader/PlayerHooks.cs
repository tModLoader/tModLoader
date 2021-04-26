using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.IO;
using HookList = Terraria.ModLoader.Core.HookList<Terraria.ModLoader.ModPlayer>;

namespace Terraria.ModLoader
{
	//todo: further documentation
	/// <summary>
	/// This is where all ModPlayer hooks are gathered and called.
	/// </summary>
	public static class PlayerHooks
	{
		private static readonly IList<ModPlayer> players = new List<ModPlayer>();
		private static Instanced<ModPlayer>[] playersArray = Array.Empty<Instanced<ModPlayer>>();

		private static List<HookList> hooks = new List<HookList>();

		private static HookList AddHook<F>(Expression<Func<ModPlayer, F>> func) {
			var hook = new HookList(ModLoader.Method(func));
			hooks.Add(hook);
			return hook;
		}

		internal static void Add(ModPlayer player) {
			player.index = (ushort)players.Count;
			players.Add(player);
		}

		internal static void RebuildHooks() {
			foreach (var hook in hooks) {
				hook.Update(players);
			}
		}

		internal static void Unload() {
			players.Clear();
		}

		internal static void SetupPlayer(Player player) {
			player.modPlayers = players
				.Select(modPlayer => modPlayer.CreateFor(player))
				.Select(modPlayer => new Instanced<ModPlayer>(modPlayer.index, modPlayer))
				.ToArray();
		}

		private static HookList HookResetEffects = AddHook<Action>(p => p.ResetEffects);

		public static void ResetEffects(Player player) {
			foreach (var g in HookResetEffects.Enumerate(player)) {
				g.ResetEffects();
			}
		}

		private static HookList HookUpdateDead = AddHook<Action>(p => p.UpdateDead);

		public static void UpdateDead(Player player) {
			foreach (var g in HookUpdateDead.Enumerate(player)) {
				g.UpdateDead();
			}
		}

		public static void SetStartInventory(Player player, IList<Item> items) {
			if (items.Count <= 50) {
				for (int k = 0; k < items.Count && k < 49; k++) {
					player.inventory[k] = items[k];
				}

				return;
			}

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

		private static HookList HookPreSavePlayer = AddHook<Action>(p => p.PreSavePlayer);

		public static void PreSavePlayer(Player player) {
			foreach (var g in HookPreSavePlayer.Enumerate(player)) {
				g.PreSavePlayer();
			}
		}

		private static HookList HookPostSavePlayer = AddHook<Action>(p => p.PostSavePlayer);

		public static void PostSavePlayer(Player player) {
			foreach (var g in HookPostSavePlayer.Enumerate(player)) {
				g.PostSavePlayer();
			}
		}

		private static HookList HookUpdateBiomes = AddHook<Action>(p => p.UpdateBiomes);

		public static void UpdateBiomes(Player player) {
			foreach (var g in HookUpdateBiomes.Enumerate(player)) {
				g.UpdateBiomes();
			}
		}

		private static HookList HookCustomBiomesMatch = AddHook<Func<Player, bool>>(p => p.CustomBiomesMatch);

		public static bool CustomBiomesMatch(Player player, Player other) {
			foreach (var g in HookCustomBiomesMatch.Enumerate(player)) {
				if (!g.CustomBiomesMatch(other)) {
					return false;
				}
			}

			return true;
		}

		private static HookList HookCopyCustomBiomesTo = AddHook<Action<Player>>(p => p.CopyCustomBiomesTo);

		public static void CopyCustomBiomesTo(Player player, Player other) {
			foreach (var g in HookCopyCustomBiomesTo.Enumerate(player)) {
				g.CopyCustomBiomesTo(other);
			}
		}

		private static HookList HookSendCustomBiomes = AddHook<Action<BinaryWriter>>(p => p.SendCustomBiomes);

		public static void SendCustomBiomes(Player player, BinaryWriter writer) {
			ushort count = 0;
			byte[] data;

			using MemoryStream stream = new MemoryStream();
			using BinaryWriter customWriter = new BinaryWriter(stream);

			foreach (var g in HookSendCustomBiomes.Enumerate(player)) {
				if (SendCustomBiomes(g, customWriter)) {
					count++;
				}
			}

			customWriter.Flush();
			data = stream.ToArray();

			writer.Write(count);
			writer.Write(data);
		}

		private static bool SendCustomBiomes(ModPlayer modPlayer, BinaryWriter writer) {
			byte[] data;

			using MemoryStream stream = new MemoryStream();
			using BinaryWriter customWriter = new BinaryWriter(stream);

			modPlayer.SendCustomBiomes(customWriter);
			customWriter.Flush();
			data = stream.ToArray();

			if (data.Length > 0) {
				writer.Write(modPlayer.Mod.Name);
				writer.Write(modPlayer.Name);
				writer.Write((byte)data.Length);
				writer.Write(data);

				return true;
			}

			return false;
		}

		public static void ReceiveCustomBiomes(Player player, BinaryReader reader) {
			int count = reader.ReadUInt16();

			for (int k = 0; k < count; k++) {
				string modName = reader.ReadString();
				string name = reader.ReadString();
				byte[] data = reader.ReadBytes(reader.ReadByte());

				if (ModContent.TryFind<ModPlayer>(modName, name, out var modPlayerBase)) {
					var modPlayer = player.GetModPlayer(modPlayerBase);

					using MemoryStream stream = new MemoryStream(data);
					using BinaryReader customReader = new BinaryReader(stream);

					try {
						modPlayer.ReceiveCustomBiomes(customReader);
					}
					catch {
					}
				}
			}
		}

		private static HookList HookUpdateBiomeVisuals = AddHook<Action>(p => p.UpdateBiomeVisuals);

		public static void UpdateBiomeVisuals(Player player) {
			foreach (var g in HookUpdateBiomeVisuals.Enumerate(player)) {
				g.UpdateBiomeVisuals();
			}
		}

		private static HookList HookClientClone = AddHook<Action<ModPlayer>>(p => p.clientClone);

		public static void clientClone(Player player, Player clientClone) {
			foreach (var g in HookClientClone.Enumerate(player)) {
				g.clientClone(clientClone.modPlayers[g.index].instance);
			}
		}

		private static HookList HookSyncPlayer = AddHook<Action<int, int, bool>>(p => p.SyncPlayer);

		public static void SyncPlayer(Player player, int toWho, int fromWho, bool newPlayer) {
			foreach (var g in HookSyncPlayer.Enumerate(player)) {
				g.SyncPlayer(toWho, fromWho, newPlayer);
			}
		}

		private static HookList HookSendClientChanges = AddHook<Action<ModPlayer>>(p => p.SendClientChanges);

		public static void SendClientChanges(Player player, Player clientPlayer) {
			foreach (var g in HookSendClientChanges.Enumerate(player)) {
				g.SendClientChanges(clientPlayer.modPlayers[g.index].instance);
			}
		}

		private static HookList HookGetMapBackgroundImage = AddHook<Func<Texture2D>>(p => p.GetMapBackgroundImage);

		public static Texture2D GetMapBackgroundImage(Player player) {
			Texture2D texture = null;

			foreach (var g in HookGetMapBackgroundImage.Enumerate(player)) {
				texture = g.GetMapBackgroundImage();

				if (texture != null) {
					return texture;
				}
			}

			return texture;
		}

		private static HookList HookUpdateBadLifeRegen = AddHook<Action>(p => p.UpdateBadLifeRegen);

		public static void UpdateBadLifeRegen(Player player) {
			foreach (var g in HookUpdateBadLifeRegen.Enumerate(player)) {
				g.UpdateBadLifeRegen();
			}
		}

		private static HookList HookUpdateLifeRegen = AddHook<Action>(p => p.UpdateLifeRegen);

		public static void UpdateLifeRegen(Player player) {
			foreach (var g in HookUpdateLifeRegen.Enumerate(player)) {
				g.UpdateLifeRegen();
			}
		}

		private delegate void DelegateNaturalLifeRegen(ref float regen);
		private static HookList HookNaturalLifeRegen = AddHook<DelegateNaturalLifeRegen>(p => p.NaturalLifeRegen);

		public static void NaturalLifeRegen(Player player, ref float regen) {
			foreach (var g in HookNaturalLifeRegen.Enumerate(player)) {
				g.NaturalLifeRegen(ref regen);
			}
		}

		private static HookList HookUpdateAutopause = AddHook<Action>(p => p.UpdateAutopause);

		public static void UpdateAutopause(Player player) {
			foreach (var g in HookUpdateAutopause.Enumerate(player)) {
				g.UpdateAutopause();
			}
		}

		private static HookList HookPreUpdate = AddHook<Action>(p => p.PreUpdate);

		public static void PreUpdate(Player player) {
			foreach (var g in HookPreUpdate.Enumerate(player)) {
				g.PreUpdate();
			}
		}

		private static HookList HookSetControls = AddHook<Action>(p => p.SetControls);

		public static void SetControls(Player player) {
			foreach (var g in HookSetControls.Enumerate(player)) {
				g.SetControls();
			}
		}

		private static HookList HookPreUpdateBuffs = AddHook<Action>(p => p.PreUpdateBuffs);

		public static void PreUpdateBuffs(Player player) {
			foreach (var g in HookPreUpdateBuffs.Enumerate(player)) {
				g.PreUpdateBuffs();
			}
		}

		private static HookList HookPostUpdateBuffs = AddHook<Action>(p => p.PostUpdateBuffs);

		public static void PostUpdateBuffs(Player player) {
			foreach (var g in HookPostUpdateBuffs.Enumerate(player)) {
				g.PostUpdateBuffs();
			}
		}

		private delegate void DelegateUpdateEquips();
		private static HookList HookUpdateEquips = AddHook<DelegateUpdateEquips>(p => p.UpdateEquips);

		public static void UpdateEquips(Player player) {
			foreach (var g in HookUpdateEquips.Enumerate(player)) {
				g.UpdateEquips();
			}
		}

		private static HookList HookUpdateVanityAccessories = AddHook<Action>(p => p.UpdateVanityAccessories);

		public static void UpdateVanityAccessories(Player player) {
			foreach (var g in HookUpdateVanityAccessories.Enumerate(player)) {
				g.UpdateVanityAccessories();
			}
		}

		private static HookList HookPostUpdateEquips = AddHook<Action>(p => p.PostUpdateEquips);

		public static void PostUpdateEquips(Player player) {
			foreach (var g in HookPostUpdateEquips.Enumerate(player)) {
				g.PostUpdateEquips();
			}
		}

		private static HookList HookPostUpdateMiscEffects = AddHook<Action>(p => p.PostUpdateMiscEffects);

		public static void PostUpdateMiscEffects(Player player) {
			foreach (var g in HookPostUpdateMiscEffects.Enumerate(player)) {
				g.PostUpdateMiscEffects();
			}
		}

		private static HookList HookPostUpdateRunSpeeds = AddHook<Action>(p => p.PostUpdateRunSpeeds);

		public static void PostUpdateRunSpeeds(Player player) {
			foreach (var g in HookPostUpdateRunSpeeds.Enumerate(player)) {
				g.PostUpdateRunSpeeds();
			}
		}

		private static HookList HookPreUpdateMovement = AddHook<Action>(p => p.PreUpdateMovement);

		public static void PreUpdateMovement(Player player) {
			foreach (var g in HookPreUpdateMovement.Enumerate(player)) {
				g.PreUpdateMovement();
			}
		}

		private static HookList HookPostUpdate = AddHook<Action>(p => p.PostUpdate);

		public static void PostUpdate(Player player) {
			foreach (var g in HookPostUpdate.Enumerate(player)) {
				g.PostUpdate();
			}
		}

		private static HookList HookFrameEffects = AddHook<Action>(p => p.FrameEffects);

		public static void FrameEffects(Player player) {
			foreach (var g in HookFrameEffects.Enumerate(player)) {
				g.FrameEffects();
			}
		}

		private delegate bool DelegatePreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource);

		private static HookList HookPreHurt = AddHook<DelegatePreHurt>(p => p.PreHurt);

		public static bool PreHurt(Player player, bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) {
			bool flag = true;

			foreach (var g in HookPreHurt.Enumerate(player)) {
				if (!g.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource)) {
					flag = false;
				}
			}

			return flag;
		}

		private static HookList HookHurt = AddHook<Action<bool, bool, double, int, bool>>(p => p.Hurt);

		public static void Hurt(Player player, bool pvp, bool quiet, double damage, int hitDirection, bool crit) {
			foreach (var g in HookHurt.Enumerate(player)) {
				g.Hurt(pvp, quiet, damage, hitDirection, crit);
			}
		}

		private static HookList HookPostHurt = AddHook<Action<bool, bool, double, int, bool>>(p => p.PostHurt);

		public static void PostHurt(Player player, bool pvp, bool quiet, double damage, int hitDirection, bool crit) {
			foreach (var g in HookPostHurt.Enumerate(player)) {
				g.PostHurt(pvp, quiet, damage, hitDirection, crit);
			}
		}

		private delegate bool DelegatePreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource);

		private static HookList HookPreKill = AddHook<DelegatePreKill>(p => p.PreKill);

		public static bool PreKill(Player player, double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) {
			bool flag = true;

			foreach (var g in HookPreKill.Enumerate(player)) {
				if (!g.PreKill(damage, hitDirection, pvp, ref playSound, ref genGore, ref damageSource)) {
					flag = false;
				}
			}

			return flag;
		}

		private static HookList HookKill = AddHook<Action<double, int, bool, PlayerDeathReason>>(p => p.Kill);

		public static void Kill(Player player, double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
			foreach (var g in HookKill.Enumerate(player)) {
				g.Kill(damage, hitDirection, pvp, damageSource);
			}
		}

		private delegate bool DelegatePreModifyLuck(ref float luck);
		private static HookList HookPreModifyLuck = AddHook<DelegatePreModifyLuck>(p => p.PreModifyLuck);

		public static bool PreModifyLuck(Player player, ref float luck) {
			bool flag = true;

			foreach (var g in HookPreModifyLuck.Enumerate(player)) {
				if (!g.PreModifyLuck(ref luck)) {
					flag = false;
				}
			}

			return flag;
		}

		private delegate void DelegateModifyLuck(ref float luck);
		private static HookList HookModifyLuck = AddHook<DelegateModifyLuck>(p => p.ModifyLuck);

		public static void ModifyLuck(Player player, ref float luck) {
			foreach (var g in HookModifyLuck.Enumerate(player)) {
				g.ModifyLuck(ref luck);
			}
		}

		private static HookList HookPreItemCheck = AddHook<Func<bool>>(p => p.PreItemCheck);

		public static bool PreItemCheck(Player player) {
			bool result = true;

			foreach (var g in HookPreItemCheck.Enumerate(player)) {
				result &= g.PreItemCheck();
			}

			return result;
		}

		private static HookList HookPostItemCheck = AddHook<Action>(p => p.PostItemCheck);

		public static void PostItemCheck(Player player) {
			foreach (var g in HookPostItemCheck.Enumerate(player)) {
				g.PostItemCheck();
			}
		}

		private static HookList HookUseTimeMultiplier = AddHook<Func<Item, float>>(p => p.UseTimeMultiplier);

		public static float UseTimeMultiplier(Player player, Item item) {
			float multiplier = 1f;

			if (item.IsAir)
				return multiplier;

			foreach (var g in HookUseTimeMultiplier.Enumerate(player)) {
				multiplier *= g.UseTimeMultiplier(item);
			}

			return multiplier;
		}

		public static float TotalUseTimeMultiplier(Player player, Item item) {
			return UseTimeMultiplier(player, item) * ItemLoader.UseTimeMultiplier(item, player);
		}

		public static int TotalUseTime(float useTime, Player player, Item item) {
			return Math.Max(2, (int)(useTime / TotalUseTimeMultiplier(player, item)));
		}

		private static HookList HookMeleeSpeedMultiplier = AddHook<Func<Item, float>>(p => p.MeleeSpeedMultiplier);

		public static float MeleeSpeedMultiplier(Player player, Item item) {
			float multiplier = 1f;

			if (item.IsAir)
				return multiplier;

			foreach (var g in HookMeleeSpeedMultiplier.Enumerate(player)) {
				multiplier *= g.MeleeSpeedMultiplier(item);
			}

			return multiplier;
		}

		public static float TotalMeleeSpeedMultiplier(Player player, Item item) {
			return TotalUseTimeMultiplier(player, item) * MeleeSpeedMultiplier(player, item)
				* ItemLoader.MeleeSpeedMultiplier(item, player);
		}

		public static int TotalMeleeTime(float useAnimation, Player player, Item item) {
			return Math.Max(2, (int)(useAnimation / TotalMeleeSpeedMultiplier(player, item)));
		}

		private delegate void DelegateGetHealLife(Item item, bool quickHeal, ref int healValue);
		private static HookList HookGetHealLife = AddHook<DelegateGetHealLife>(p => p.GetHealLife);

		public static void GetHealLife(Player player, Item item, bool quickHeal, ref int healValue) {
			if (item.IsAir)
				return;

			foreach (var g in HookGetHealLife.Enumerate(player)) {
				g.GetHealLife(item, quickHeal, ref healValue);
			}
		}

		private delegate void DelegateGetHealMana(Item item, bool quickHeal, ref int healValue);
		private static HookList HookGetHealMana = AddHook<DelegateGetHealMana>(p => p.GetHealMana);

		public static void GetHealMana(Player player, Item item, bool quickHeal, ref int healValue) {
			if (item.IsAir)
				return;

			foreach (var g in HookGetHealMana.Enumerate(player)) {
				g.GetHealMana(item, quickHeal, ref healValue);
			}
		}

		private delegate void DelegateModifyManaCost(Item item, ref float reduce, ref float mult);
		private static HookList HookModifyManaCost = AddHook<DelegateModifyManaCost>(p => p.ModifyManaCost);

		public static void ModifyManaCost(Player player, Item item, ref float reduce, ref float mult) {
			if (item.IsAir)
				return;
			
			foreach (var g in HookModifyManaCost.Enumerate(player)) {
				g.ModifyManaCost(item, ref reduce, ref mult);
			}
		}

		private static HookList HookOnMissingMana = AddHook<Action<Item, int>>(p => p.OnMissingMana);

		public static void OnMissingMana(Player player, Item item, int manaNeeded) {
			if (item.IsAir)
				return;
			
			foreach (var g in HookOnMissingMana.Enumerate(player)) {
				g.OnMissingMana(item, manaNeeded);
			}
		}

		private static HookList HookOnConsumeMana = AddHook<Action<Item, int>>(p => p.OnConsumeMana);

		public static void OnConsumeMana(Player player, Item item, int manaConsumed) {
			if (item.IsAir)
				return;
			
			foreach (var g in HookOnConsumeMana.Enumerate(player)) {
				g.OnConsumeMana(item, manaConsumed);
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

			foreach (var g in HookModifyWeaponDamage.Enumerate(player)) {
				g.ModifyWeaponDamage(item, ref damage, ref flat);
			}
		}

		private static HookList HookProcessTriggers = AddHook<Action<TriggersSet>>(p => p.ProcessTriggers);

		public static void ProcessTriggers(Player player, TriggersSet triggersSet) {
			foreach (var g in HookProcessTriggers.Enumerate(player)) {
				g.ProcessTriggers(triggersSet);
			}
		}

		private delegate void DelegateModifyWeaponKnockback(Item item, ref StatModifier knockback, ref float flat);
		private static HookList HookModifyWeaponKnockback = AddHook<DelegateModifyWeaponKnockback>(p => p.ModifyWeaponKnockback);

		public static void ModifyWeaponKnockback(Player player, Item item, ref StatModifier knockback, ref float flat) {
			if (item.IsAir)
				return;

			foreach (var g in HookModifyWeaponKnockback.Enumerate(player)) {
				g.ModifyWeaponKnockback(item, ref knockback, ref flat);
			}
		}

		private delegate void DelegateModifyWeaponCrit(Item item, ref int crit);
		private static HookList HookModifyWeaponCrit = AddHook<DelegateModifyWeaponCrit>(p => p.ModifyWeaponCrit);

		public static void ModifyWeaponCrit(Player player, Item item, ref int crit) {
			if (item.IsAir)
				return;

			foreach (var g in HookModifyWeaponCrit.Enumerate(player)) {
				g.ModifyWeaponCrit(item, ref crit);
			}
		}

		private static HookList HookConsumeAmmo = AddHook<Func<Item, Item, bool>>(p => p.ConsumeAmmo);

		public static bool ConsumeAmmo(Player player, Item weapon, Item ammo) {
			foreach (var g in HookConsumeAmmo.Enumerate(player)) {
				if (!g.ConsumeAmmo(weapon, ammo)) {
					return false;
				}
			}
			return true;
		}

		private static HookList HookOnConsumeAmmo = AddHook<Action<Item, Item>>(p => p.OnConsumeAmmo);

		public static void OnConsumeAmmo(Player player, Item weapon, Item ammo) {
			foreach (var g in HookOnConsumeAmmo.Enumerate(player))
				g.OnConsumeAmmo(weapon, ammo);
		}

		private static HookList HookCanShoot = AddHook<Func<Item, bool>>(p => p.CanShoot);

		public static bool CanShoot(Player player, Item item) {
			bool canShoot = true;

			foreach (var g in HookCanShoot.Enumerate(player)) {
				canShoot &= g.CanShoot(item);
			}

			return canShoot;
		}

		private delegate void DelegateModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback);
		private static HookList HookModifyShootStats = AddHook<DelegateModifyShootStats>(p => p.ModifyShootStats);

		public static void ModifyShootStats(Player player, Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			foreach (var g in HookModifyShootStats.Enumerate(player)) {
				g.ModifyShootStats(item, ref position, ref velocity, ref type, ref damage, ref knockback);
			}
		}

		private static HookList HookShoot = AddHook<Action<Item, ProjectileSource_Item_WithAmmo, Vector2, Vector2, int, int, float>>(p => p.Shoot);

		public static void Shoot(Player player, Item item, ProjectileSource_Item_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			foreach (var g in HookShoot.Enumerate(player)) {
				g.Shoot(item, source, position, velocity, type, damage, knockback);
			}
		}

		private static HookList HookMeleeEffects = AddHook<Action<Item, Rectangle>>(p => p.MeleeEffects);

		public static void MeleeEffects(Player player, Item item, Rectangle hitbox) {
			foreach (var g in HookMeleeEffects.Enumerate(player)) {
				g.MeleeEffects(item, hitbox);
			}
		}

		private static HookList HookOnHitAnything = AddHook<Action<float, float, Entity>>(p => p.OnHitAnything);

		public static void OnHitAnything(Player player, float x, float y, Entity victim) {
			foreach (var g in HookOnHitAnything.Enumerate(player)) {
				g.OnHitAnything(x, y, victim);
			}
		}

		private static HookList HookCanHitNPC = AddHook<Func<Item, NPC, bool?>>(p => p.CanHitNPC);

		public static bool? CanHitNPC(Player player, Item item, NPC target) {
			bool? flag = null;

			foreach (var g in HookCanHitNPC.Enumerate(player)) {
				bool? canHit = g.CanHitNPC(item, target);

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
			foreach (var g in HookModifyHitNPC.Enumerate(player)) {
				g.ModifyHitNPC(item, target, ref damage, ref knockback, ref crit);
			}
		}

		private static HookList HookOnHitNPC = AddHook<Action<Item, NPC, int, float, bool>>(p => p.OnHitNPC);

		public static void OnHitNPC(Player player, Item item, NPC target, int damage, float knockback, bool crit) {
			foreach (var g in HookOnHitNPC.Enumerate(player)) {
				g.OnHitNPC(item, target, damage, knockback, crit);
			}
		}

		private static HookList HookCanHitNPCWithProj = AddHook<Func<Projectile, NPC, bool?>>(p => p.CanHitNPCWithProj);

		public static bool? CanHitNPCWithProj(Projectile proj, NPC target) {
			if (proj.npcProj || proj.trap) {
				return null;
			}

			Player player = Main.player[proj.owner];
			bool? flag = null;

			foreach (var g in HookCanHitNPCWithProj.Enumerate(player)) {
				bool? canHit = g.CanHitNPCWithProj(proj, target);

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

			foreach (var g in HookModifyHitNPCWithProj.Enumerate(player)) {
				g.ModifyHitNPCWithProj(proj, target, ref damage, ref knockback, ref crit, ref hitDirection);
			}
		}

		private static HookList HookOnHitNPCWithProj = AddHook<Action<Projectile, NPC, int, float, bool>>(p => p.OnHitNPCWithProj);

		public static void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit) {
			if (proj.npcProj || proj.trap) {
				return;
			}

			Player player = Main.player[proj.owner];

			foreach (var g in HookOnHitNPCWithProj.Enumerate(player)) {
				g.OnHitNPCWithProj(proj, target, damage, knockback, crit);
			}
		}

		private static HookList HookCanHitPvp = AddHook<Func<Item, Player, bool>>(p => p.CanHitPvp);

		public static bool CanHitPvp(Player player, Item item, Player target) {
			foreach (var g in HookCanHitPvp.Enumerate(player)) {
				if (!g.CanHitPvp(item, target)) {
					return false;
				}
			}

			return true;
		}

		private delegate void DelegateModifyHitPvp(Item item, Player target, ref int damage, ref bool crit);
		private static HookList HookModifyHitPvp = AddHook<DelegateModifyHitPvp>(p => p.ModifyHitPvp);

		public static void ModifyHitPvp(Player player, Item item, Player target, ref int damage, ref bool crit) {
			foreach (var g in HookModifyHitPvp.Enumerate(player)) {
				g.ModifyHitPvp(item, target, ref damage, ref crit);
			}
		}

		private static HookList HookOnHitPvp = AddHook<Action<Item, Player, int, bool>>(p => p.OnHitPvp);

		public static void OnHitPvp(Player player, Item item, Player target, int damage, bool crit) {
			foreach (var g in HookOnHitPvp.Enumerate(player)) {
				g.OnHitPvp(item, target, damage, crit);
			}
		}

		private static HookList HookCanHitPvpWithProj = AddHook<Func<Projectile, Player, bool>>(p => p.CanHitPvpWithProj);

		public static bool CanHitPvpWithProj(Projectile proj, Player target) {
			Player player = Main.player[proj.owner];

			foreach (var g in HookCanHitPvpWithProj.Enumerate(player)) {
				if (!g.CanHitPvpWithProj(proj, target)) {
					return false;
				}
			}

			return true;
		}

		private delegate void DelegateModifyHitPvpWithProj(Projectile proj, Player target, ref int damage, ref bool crit);
		private static HookList HookModifyHitPvpWithProj = AddHook<DelegateModifyHitPvpWithProj>(p => p.ModifyHitPvpWithProj);

		public static void ModifyHitPvpWithProj(Projectile proj, Player target, ref int damage, ref bool crit) {
			Player player = Main.player[proj.owner];

			foreach (var g in HookModifyHitPvpWithProj.Enumerate(player)) {
				g.ModifyHitPvpWithProj(proj, target, ref damage, ref crit);
			}
		}

		private static HookList HookOnHitPvpWithProj = AddHook<Action<Projectile, Player, int, bool>>(p => p.OnHitPvpWithProj);

		public static void OnHitPvpWithProj(Projectile proj, Player target, int damage, bool crit) {
			Player player = Main.player[proj.owner];

			foreach (var g in HookOnHitPvpWithProj.Enumerate(player)) {
				g.OnHitPvpWithProj(proj, target, damage, crit);
			}
		}

		private delegate bool DelegateCanBeHitByNPC(NPC npc, ref int cooldownSlot);
		private static HookList HookCanBeHitByNPC = AddHook<DelegateCanBeHitByNPC>(p => p.CanBeHitByNPC);

		public static bool CanBeHitByNPC(Player player, NPC npc, ref int cooldownSlot) {
			foreach (var g in HookCanBeHitByNPC.Enumerate(player)) {
				if (!g.CanBeHitByNPC(npc, ref cooldownSlot)) {
					return false;
				}
			}

			return true;
		}

		private delegate void DelegateModifyHitByNPC(NPC npc, ref int damage, ref bool crit);
		private static HookList HookModifyHitByNPC = AddHook<DelegateModifyHitByNPC>(p => p.ModifyHitByNPC);

		public static void ModifyHitByNPC(Player player, NPC npc, ref int damage, ref bool crit) {
			foreach (var g in HookModifyHitByNPC.Enumerate(player)) {
				g.ModifyHitByNPC(npc, ref damage, ref crit);
			}
		}

		private static HookList HookOnHitByNPC = AddHook<Action<NPC, int, bool>>(p => p.OnHitByNPC);

		public static void OnHitByNPC(Player player, NPC npc, int damage, bool crit) {
			foreach (var g in HookOnHitByNPC.Enumerate(player)) {
				g.OnHitByNPC(npc, damage, crit);
			}
		}

		private static HookList HookCanBeHitByProjectile = AddHook<Func<Projectile, bool>>(p => p.CanBeHitByProjectile);

		public static bool CanBeHitByProjectile(Player player, Projectile proj) {
			foreach (var g in HookCanBeHitByProjectile.Enumerate(player)) {
				if (!g.CanBeHitByProjectile(proj)) {
					return false;
				}
			}

			return true;
		}

		private delegate void DelegateModifyHitByProjectile(Projectile proj, ref int damage, ref bool crit);
		private static HookList HookModifyHitByProjectile = AddHook<DelegateModifyHitByProjectile>(p => p.ModifyHitByProjectile);

		public static void ModifyHitByProjectile(Player player, Projectile proj, ref int damage, ref bool crit) {
			foreach (var g in HookModifyHitByProjectile.Enumerate(player)) {
				g.ModifyHitByProjectile(proj, ref damage, ref crit);
			}
		}

		private static HookList HookOnHitByProjectile = AddHook<Action<Projectile, int, bool>>(p => p.OnHitByProjectile);

		public static void OnHitByProjectile(Player player, Projectile proj, int damage, bool crit) {
			foreach (var g in HookOnHitByProjectile.Enumerate(player)) {
				g.OnHitByProjectile(proj, damage, crit);
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

			foreach (var g in HookCatchFish.Enumerate(player)) {
				g.CatchFish(fishingRod, player.inventory[i], power, liquidType, poolSize, worldLayer, questFish, ref caughtType);
			}
		}

		private delegate void DelegateGetFishingLevel(Item fishingRod, Item bait, ref float fishingLevel);
		private static HookList HookGetFishingLevel = AddHook<DelegateGetFishingLevel>(p => p.GetFishingLevel);

		public static void GetFishingLevel(Player player, Item fishingRod, Item bait, ref float fishingLevel) {
			foreach (var g in HookGetFishingLevel.Enumerate(player)) {
				g.GetFishingLevel(fishingRod, bait, ref fishingLevel);
			}
		}

		private static HookList HookAnglerQuestReward = AddHook<Action<float, List<Item>>>(p => p.AnglerQuestReward);

		public static void AnglerQuestReward(Player player, float rareMultiplier, List<Item> rewardItems) {
			foreach (var g in HookAnglerQuestReward.Enumerate(player)) {
				g.AnglerQuestReward(rareMultiplier, rewardItems);
			}
		}

		private static HookList HookGetDyeTraderReward = AddHook<Action<List<int>>>(p => p.GetDyeTraderReward);

		public static void GetDyeTraderReward(Player player, List<int> rewardPool) {
			foreach (var g in HookGetDyeTraderReward.Enumerate(player)) {
				g.GetDyeTraderReward(rewardPool);
			}
		}

		private delegate void DelegateDrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright);
		private static HookList HookDrawEffects = AddHook<DelegateDrawEffects>(p => p.DrawEffects);

		public static void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
			foreach (var modPlayer in HookDrawEffects.Enumerate(drawInfo.drawPlayer)) {
				modPlayer.DrawEffects(drawInfo, ref r, ref g, ref b, ref a, ref fullBright);
			}
		}

		private delegate void DelegateModifyDrawInfo(ref PlayerDrawSet drawInfo);
		private static HookList HookModifyDrawInfo = AddHook<DelegateModifyDrawInfo>(p => p.ModifyDrawInfo);

		public static void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
			foreach (var g in HookModifyDrawInfo.Enumerate(drawInfo.drawPlayer)) {
				g.ModifyDrawInfo(ref drawInfo);
			}
		}

		private static HookList HookModifyDrawLayerOrdering = AddHook<Action<IDictionary<PlayerDrawLayer, PlayerDrawLayer.Position>>>(p => p.ModifyDrawLayerOrdering);

		public static void ModifyDrawLayerOrdering(IDictionary<PlayerDrawLayer, PlayerDrawLayer.Position> positions) {
			foreach (var g in HookModifyDrawLayerOrdering.Enumerate(playersArray)) {
				g.ModifyDrawLayerOrdering(positions);
			}
		}

		private static HookList HookModifyDrawLayers = AddHook<Action<PlayerDrawSet>>(p => p.HideDrawLayers);

		public static void HideDrawLayers(PlayerDrawSet drawInfo) {
			foreach (var g in HookModifyDrawLayers.Enumerate(drawInfo.drawPlayer)) {
				g.HideDrawLayers(drawInfo);
			}
		}

		private static HookList HookModifyScreenPosition = AddHook<Action>(p => p.ModifyScreenPosition);

		public static void ModifyScreenPosition(Player player) {
			foreach (var g in HookModifyScreenPosition.Enumerate(player)) {
				g.ModifyScreenPosition();
			}
		}

		private delegate void DelegateModifyZoom(ref float zoom);
		private static HookList HookModifyZoom = AddHook<DelegateModifyZoom>(p => p.ModifyZoom);

		public static void ModifyZoom(Player player, ref float zoom) {
			foreach (var g in HookModifyZoom.Enumerate(player)) {
				g.ModifyZoom(ref zoom);
			}
		}

		private static HookList HookPlayerConnect = AddHook<Action<Player>>(p => p.PlayerConnect);

		public static void PlayerConnect(int playerIndex) {
			var player = Main.player[playerIndex];

			foreach (var g in HookPlayerConnect.Enumerate(player)) {
				g.PlayerConnect(player);
			}
		}

		private static HookList HookPlayerDisconnect = AddHook<Action<Player>>(p => p.PlayerDisconnect);

		public static void PlayerDisconnect(int playerIndex) {
			var player = Main.player[playerIndex];

			foreach (var g in HookPlayerDisconnect.Enumerate(player)) {
				g.PlayerDisconnect(player);
			}
		}

		private static HookList HookOnEnterWorld = AddHook<Action<Player>>(p => p.OnEnterWorld);

		// Do NOT hook into the Player.Hooks.OnEnterWorld event
		public static void OnEnterWorld(int playerIndex) {
			var player = Main.player[playerIndex];

			foreach (var g in HookOnEnterWorld.Enumerate(player)) {
				g.OnEnterWorld(player);
			}
		}

		private static HookList HookOnRespawn = AddHook<Action<Player>>(p => p.OnRespawn);

		public static void OnRespawn(Player player) {
			foreach (var g in HookOnRespawn.Enumerate(player)) {
				g.OnRespawn(player);
			}
		}

		private static HookList HookShiftClickSlot = AddHook<Func<Item[], int, int, bool>>(p => p.ShiftClickSlot);

		public static bool ShiftClickSlot(Player player, Item[] inventory, int context, int slot) {
			foreach (var g in HookShiftClickSlot.Enumerate(player)) {
				if (g.ShiftClickSlot(inventory, context, slot))
					return true;
			}

			return false;
		}

		private static bool HasMethod(Type t, string method, params Type[] args) 
			=> t.GetMethod(method, args).DeclaringType != typeof(ModPlayer);

		internal static void VerifyGlobalItem(ModPlayer player) {
			var type = player.GetType();

			// Net Custom Biome Methods

			int netCustomBiomeMethods = 0;

			if (HasMethod(type, "CustomBiomesMatch", typeof(Player)))
				netCustomBiomeMethods++;

			if (HasMethod(type, "CopyCustomBiomesTo", typeof(Player)))
				netCustomBiomeMethods++;

			if (HasMethod(type, "SendCustomBiomes", typeof(BinaryWriter)))
				netCustomBiomeMethods++;

			if (HasMethod(type, "ReceiveCustomBiomes", typeof(BinaryReader)))
				netCustomBiomeMethods++;

			if (netCustomBiomeMethods > 0 && netCustomBiomeMethods < 4)
				throw new Exception(type + " must override all of (CustomBiomesMatch/CopyCustomBiomesTo/SendCustomBiomes/ReceiveCustomBiomes) or none");

			// Net Client Methods

			int netClientMethods = 0;

			if (HasMethod(type, "clientClone", typeof(ModPlayer)))
				netClientMethods++;

			if (HasMethod(type, "SyncPlayer", typeof(int), typeof(int), typeof(bool)))
				netClientMethods++;

			if (HasMethod(type, "SendClientChanges", typeof(ModPlayer)))
				netClientMethods++;

			if (netClientMethods > 0 && netClientMethods < 3)
				throw new Exception(type + " must override all of (clientClone/SyncPlayer/SendClientChanges) or none");

			// Save Methods

			int saveMethods = 0;

			if (HasMethod(type, "Save"))
				saveMethods++;

			if (HasMethod(type, "Load", typeof(TagCompound)))
				saveMethods++;

			if (saveMethods == 1)
				throw new Exception(type + " must override all of (Save/Load) or none");

			// Net Methods

			int netMethods = 0;

			if (HasMethod(type, "NetSend", typeof(BinaryWriter)))
				netMethods++;

			if (HasMethod(type, "NetReceive", typeof(BinaryReader)))
				netMethods++;

			if (netMethods == 1)
				throw new Exception(type + " must override both of (NetSend/NetReceive) or none");
		}

		private static HookList HookPostSellItem = AddHook<Action<NPC, Item[], Item>>(p => p.PostSellItem);

		public static void PostSellItem(Player player, NPC npc, Item[] shopInventory, Item item) {
			foreach (var g in HookPostSellItem.Enumerate(player)) {
				g.PostSellItem(npc, shopInventory, item);
			}
		}

		private static HookList HookCanSellItem = AddHook<Func<NPC, Item[], Item, bool>>(p => p.CanSellItem);

		// TODO: GlobalNPC and ModNPC hooks for Buy/Sell hooks as well.
		public static bool CanSellItem(Player player, NPC npc, Item[] shopInventory, Item item) {
			foreach (var g in HookCanSellItem.Enumerate(player)) {
				if (!g.CanSellItem(npc, shopInventory, item))
					return false;
			}

			return true;
		}

		private static HookList HookPostBuyItem = AddHook<Action<NPC, Item[], Item>>(p => p.PostBuyItem);

		public static void PostBuyItem(Player player, NPC npc, Item[] shopInventory, Item item) {
			foreach (var g in HookPostBuyItem.Enumerate(player)) {
				g.PostBuyItem(npc, shopInventory, item);
			}
		}

		private static HookList HookCanBuyItem = AddHook<Func<NPC, Item[], Item, bool>>(p => p.CanBuyItem);

		public static bool CanBuyItem(Player player, NPC npc, Item[] shopInventory, Item item) {
			foreach (var g in HookCanBuyItem.Enumerate(player)) {
				if (!g.CanBuyItem(npc, shopInventory, item))
					return false;
			}

			return true;
		}

		private static HookList HookCanUseItem = AddHook<Func<Item, bool>>(p => p.CanUseItem);

		public static bool CanUseItem(Player player, Item item) {
			bool result = true;

			foreach (var g in HookCanUseItem.Enumerate(player)) {
				result &= g.CanUseItem(item);
			}

			return result;
		}

		private delegate bool DelegateModifyNurseHeal(NPC npc, ref int health, ref bool removeDebuffs, ref string chatText);
		private static HookList HookModifyNurseHeal = AddHook<DelegateModifyNurseHeal>(p => p.ModifyNurseHeal);

		public static bool ModifyNurseHeal(Player player, NPC npc, ref int health, ref bool removeDebuffs, ref string chat) {
			foreach (var g in HookModifyNurseHeal.Enumerate(player)) {
				if (!g.ModifyNurseHeal(npc, ref health, ref removeDebuffs, ref chat))
					return false;
			}

			return true;
		}

		private delegate void DelegateModifyNursePrice(NPC npc, int health, bool removeDebuffs, ref int price);
		private static HookList HookModifyNursePrice = AddHook<DelegateModifyNursePrice>(p => p.ModifyNursePrice);

		public static void ModifyNursePrice(Player player, NPC npc, int health, bool removeDebuffs, ref int price) {
			foreach (var g in HookModifyNursePrice.Enumerate(player)) {
				g.ModifyNursePrice(npc, health, removeDebuffs, ref price);
			}
		}

		private static HookList HookPostNurseHeal = AddHook<Action<NPC, int, bool, int>>(p => p.PostNurseHeal);

		public static void PostNurseHeal(Player player, NPC npc, int health, bool removeDebuffs, int price) {
			foreach (var g in HookPostNurseHeal.Enumerate(player)) {
				g.PostNurseHeal(npc, health, removeDebuffs, price);
			}
		}

		private static HookList HookAddStartingItems = AddHook<Func<bool, IEnumerable<Item>>>(p => p.AddStartingItems);
		private static HookList HookModifyStartingInventory = AddHook<Action<IReadOnlyDictionary<string, List<Item>>, bool>>(p => p.ModifyStartingInventory);

		public static List<Item> GetStartingItems(Player player, IEnumerable<Item> vanillaItems, bool mediumCoreDeath = false) {
			var itemsByMod = new Dictionary<string, List<Item>> {
				["Terraria"] = vanillaItems.ToList()
			};

			foreach (var g in HookAddStartingItems.Enumerate(player)) {
				itemsByMod[g.Mod.Name] = g.AddStartingItems(mediumCoreDeath).ToList();
			}

			foreach (var g in HookModifyStartingInventory.Enumerate(player)) {
				g.ModifyStartingInventory(itemsByMod, mediumCoreDeath);
			}

			return itemsByMod
				.OrderBy(kv => kv.Key == "Terraria" ? "" : kv.Key)
				.SelectMany(kv => kv.Value)
				.ToList();
		}
	}
}