using Microsoft.Xna.Framework;
using System;
using Terraria.DataStructures;

namespace Terraria.ModLoader
{
	public static class CombinedHooks
	{
		public static void ModifyWeaponDamage(Player player, Item item, ref StatModifier damage, ref float flat) {
			ItemLoader.ModifyWeaponDamage(item, player, ref damage, ref flat);
			PlayerHooks.ModifyWeaponDamage(player, item, ref damage, ref flat);
		}

		public static void ModifyWeaponCrit(Player player, Item item, ref int crit) {
			ItemLoader.ModifyWeaponCrit(item, player, ref crit);
			PlayerHooks.ModifyWeaponCrit(player, item, ref crit);
		}

		public static void ModifyWeaponKnockback(Player player, Item item, ref StatModifier knockback, ref float flat) {
			ItemLoader.ModifyWeaponKnockback(item, player, ref knockback, ref flat);
			PlayerHooks.ModifyWeaponKnockback(player, item, ref knockback, ref flat);
		}

		public static void ModifyManaCost(Player player, Item item, ref float reduce, ref float mult) {
			ItemLoader.ModifyManaCost(item, player, ref reduce, ref mult);
			PlayerHooks.ModifyManaCost(player, item, ref reduce, ref mult);
		}

		public static void OnConsumeMana(Player player, Item item, int manaConsumed) {
			ItemLoader.OnConsumeMana(item, player, manaConsumed);
			PlayerHooks.OnConsumeMana(player, item, manaConsumed);
		}

		public static void OnMissingMana(Player player, Item item, int neededMana) {
			ItemLoader.OnMissingMana(item, player, neededMana);
			PlayerHooks.OnMissingMana(player, item, neededMana);
		}

		//TODO: Fix various inconsistencies with calls of UseItem, and then make this and its inner methods use short-circuiting.
		public static bool CanUseItem(Player player, Item item) {
			return PlayerHooks.CanUseItem(player, item) & ItemLoader.CanUseItem(item, player);
		}

		public static bool CanShoot(Player player, Item item) {
			return PlayerHooks.CanShoot(player, item) & ItemLoader.CanShoot(item, player);
		}

		public static void ModifyShootStats(Player player, Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			ItemLoader.ModifyShootStats(item, player, ref position, ref velocity, ref type, ref damage, ref knockback);
			PlayerHooks.ModifyShootStats(player, item, ref position, ref velocity, ref type, ref damage, ref knockback);
		}

		public static bool Shoot(Player player, Item item, ProjectileSource_Item_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			PlayerHooks.Shoot(player, item, source, position, velocity, type, damage, knockback);
			return ItemLoader.Shoot(item, player, source, position, velocity, type, damage, knockback);
		}

		public static bool? CanPlayerHitNPCWithItem(Player player, Item item, NPC npc) {
			bool? result = null;

			bool ModifyResult(bool? nbool) {
				if (nbool.HasValue) {
					result = nbool.Value;
				}

				return result != false;
			}

			if (!ModifyResult(PlayerHooks.CanHitNPC(player, item, npc))) {
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

		public static bool? CanUseTool(Player player, Item item, ToolType toolType, Tile tile, int x, int y) {
			bool? result = null;

			bool ModifyResult(bool? nbool) {
				if (nbool.HasValue) {
					result = nbool.Value;
				}

				return result != false;
			}

			if (toolType.AffectsBlocks && (!tile.active() || !ModifyResult(TileLoader.CanUseTool(x, y, tile.type, item, toolType)))) {
				return false;
			}

			if (toolType.AffectsWalls && (tile.wall <= 0 || !ModifyResult(WallLoader.CanUseTool(x, y, tile.wall, item, toolType)))) {
				return false;
			}

			if (!ModifyResult(ItemLoader.CanUseTool(item, player, toolType, tile, x, y))) {
				return false;
			}

			return result;
		}
	}
}
