using Microsoft.Xna.Framework;
using System;
using Terraria.DataStructures;

namespace Terraria.ModLoader
{
	public static class CombinedHooks
	{
		public static void ModifyWeaponDamage(Player player, Item item, ref StatModifier damage, ref float flat) {
			ItemLoader.ModifyWeaponDamage(item, player, ref damage, ref flat);
			PlayerLoader.ModifyWeaponDamage(player, item, ref damage, ref flat);
		}

		public static void ModifyWeaponCrit(Player player, Item item, ref int crit) {
			ItemLoader.ModifyWeaponCrit(item, player, ref crit);
			PlayerLoader.ModifyWeaponCrit(player, item, ref crit);
		}

		public static void ModifyWeaponKnockback(Player player, Item item, ref StatModifier knockback, ref float flat) {
			ItemLoader.ModifyWeaponKnockback(item, player, ref knockback, ref flat);
			PlayerLoader.ModifyWeaponKnockback(player, item, ref knockback, ref flat);
		}

		public static void ModifyManaCost(Player player, Item item, ref float reduce, ref float mult) {
			ItemLoader.ModifyManaCost(item, player, ref reduce, ref mult);
			PlayerLoader.ModifyManaCost(player, item, ref reduce, ref mult);
		}

		public static void OnConsumeMana(Player player, Item item, int manaConsumed) {
			ItemLoader.OnConsumeMana(item, player, manaConsumed);
			PlayerLoader.OnConsumeMana(player, item, manaConsumed);
		}

		public static void OnMissingMana(Player player, Item item, int neededMana) {
			ItemLoader.OnMissingMana(item, player, neededMana);
			PlayerLoader.OnMissingMana(player, item, neededMana);
		}

		//TODO: Fix various inconsistencies with calls of UseItem, and then make this and its inner methods use short-circuiting.
		public static bool CanUseItem(Player player, Item item) {
			return PlayerLoader.CanUseItem(player, item) & ItemLoader.CanUseItem(item, player);
		}

		public static bool CanShoot(Player player, Item item) {
			return PlayerLoader.CanShoot(player, item) && ItemLoader.CanShoot(item, player);
		}

		public static void ModifyShootStats(Player player, Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			ItemLoader.ModifyShootStats(item, player, ref position, ref velocity, ref type, ref damage, ref knockback);
			PlayerLoader.ModifyShootStats(player, item, ref position, ref velocity, ref type, ref damage, ref knockback);
		}

		public static bool Shoot(Player player, Item item, ProjectileSource_Item_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			bool defaultResult = PlayerLoader.Shoot(player, item, source, position, velocity, type, damage, knockback);
			return ItemLoader.Shoot(item, player, source, position, velocity, type, damage, knockback, defaultResult); 
		}

		public static bool? CanPlayerHitNPCWithItem(Player player, Item item, NPC npc) {
			bool? result = null;

			bool ModifyResult(bool? nbool) {
				if (nbool.HasValue) {
					result = nbool.Value;
				}

				return result != false;
			}

			if (!ModifyResult(PlayerLoader.CanHitNPC(player, item, npc))) {
				return false;
			}

			if (!ModifyResult(ItemLoader.CanHitNPC(item, player, npc))) {
				return false;
			}

			if (!ModifyResult(NPCLoader.CanBeHitByItem(npc, player, item))) {
				return false;
			}

			return result;
		}

		public static float TotalUseSpeedMultiplier(Player player, Item item) {
			return PlayerLoader.UseSpeedMultiplier(player, item) * ItemLoader.UseSpeedMultiplier(item, player);
		}

		public static float TotalUseTimeMultiplier(Player player, Item item) {
			return PlayerLoader.UseTimeMultiplier(player, item) * ItemLoader.UseTimeMultiplier(item, player) / TotalUseSpeedMultiplier(player, item);
		}

		public static int TotalUseTime(float useTime, Player player, Item item) {
			int result = Math.Max(1, (int)(useTime * TotalUseTimeMultiplier(player, item)));

			return result;
		}

		public static float TotalUseAnimationMultiplier(Player player, Item item) {
			float result = PlayerLoader.UseAnimationMultiplier(player, item) * ItemLoader.UseAnimationMultiplier(item, player);

			// UseSpeedMultiplier tries to affect both useTime and useAnimation in a way that doesn't break their relativity.
			// The code below multiplies useAnimation based on the difference that UseSpeedMultiplier makes on the item's useTime.
			float timeAnimationFactor = item.useAnimation / (float)item.useTime;
			int multipliedUseTime = Math.Max(1, (int)(item.useTime / TotalUseSpeedMultiplier(player, item)));
			int relativeUseAnimation = Math.Max(1, (int)(multipliedUseTime * timeAnimationFactor));

			result *= relativeUseAnimation / (float)item.useAnimation;

			return result;
		}

		public static int TotalAnimationTime(float useAnimation, Player player, Item item) {
			int result = Math.Max(1, (int)(useAnimation * TotalUseAnimationMultiplier(player, item)));

			return result;
		}
	}
}
