using Microsoft.Xna.Framework;
using System;
using Terraria.DataStructures;
using Terraria.Map;
using static Terraria.ModLoader.BackupIO;

namespace Terraria.ModLoader;

public static class CombinedHooks
{
	public static void ModifyWeaponDamage(Player player, Item item, ref StatModifier damage)
	{
		ItemLoader.ModifyWeaponDamage(item, player, ref damage);
		PlayerLoader.ModifyWeaponDamage(player, item, ref damage);
	}

	public static void ModifyWeaponCrit(Player player, Item item, ref float crit)
	{
		AddableFloat critAdd = AddableFloat.Zero;
		ItemLoader.ModifyWeaponCrit(item, player, ref critAdd);
		PlayerLoader.ModifyWeaponCrit(player, item, ref critAdd);
		crit += critAdd.Value;
	}

	public static void ModifyWeaponKnockback(Player player, Item item, ref StatModifier knockback)
	{
		ItemLoader.ModifyWeaponKnockback(item, player, ref knockback);
		PlayerLoader.ModifyWeaponKnockback(player, item, ref knockback);
	}

	public static void ModifyManaCost(Player player, Item item, ref float reduce, ref float mult)
	{
		ItemLoader.ModifyManaCost(item, player, ref reduce, ref mult);
		PlayerLoader.ModifyManaCost(player, item, ref reduce, ref mult);
	}

	public static void OnConsumeMana(Player player, Item item, int manaConsumed)
	{
		ItemLoader.OnConsumeMana(item, player, manaConsumed);
		PlayerLoader.OnConsumeMana(player, item, manaConsumed);
	}

	public static void OnMissingMana(Player player, Item item, int neededMana)
	{
		ItemLoader.OnMissingMana(item, player, neededMana);
		PlayerLoader.OnMissingMana(player, item, neededMana);
	}

	public static bool CanConsumeAmmo(Player player, Item weapon, Item ammo)
	{
		return PlayerLoader.CanConsumeAmmo(player, weapon, ammo) && ItemLoader.CanConsumeAmmo(weapon, ammo, player);
	}

	public static void OnConsumeAmmo(Player player, Item weapon, Item ammo)
	{
		PlayerLoader.OnConsumeAmmo(player, weapon, ammo);
		ItemLoader.OnConsumeAmmo(weapon, ammo, player);
	}

	//TODO: Fix various inconsistencies with calls of UseItem
	public static bool CanUseItem(Player player, Item item)
	{
		return PlayerLoader.CanUseItem(player, item) && ItemLoader.CanUseItem(item, player);
	}

	// In Player.TryAllowingItemReuse_Inner
	public static bool? CanAutoReuseItem(Player player, Item item)
	{
		bool? ret = null;
		bool Update(bool? b) => (ret ??= b) is not false;

		_ = Update(PlayerLoader.CanAutoReuseItem(player, item))
			&& Update(ItemLoader.CanAutoReuseItem(item, player));
		return ret;
	}

	public static bool CanShoot(Player player, Item item)
	{
		return PlayerLoader.CanShoot(player, item) && ItemLoader.CanShoot(item, player);
	}

	public static void ModifyShootStats(Player player, Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
	{
		ItemLoader.ModifyShootStats(item, player, ref position, ref velocity, ref type, ref damage, ref knockback);
		PlayerLoader.ModifyShootStats(player, item, ref position, ref velocity, ref type, ref damage, ref knockback);
	}

	public static bool Shoot(Player player, Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		bool defaultResult = PlayerLoader.Shoot(player, item, source, position, velocity, type, damage, knockback);
		return ItemLoader.Shoot(item, player, source, position, velocity, type, damage, knockback, defaultResult);
	}

	public static bool? CanPlayerHitNPCWithItem(Player player, Item item, NPC npc)
	{
		bool? ret = null;
		bool Update(bool? b) => (ret ??= b) is not false;

		_ = Update(PlayerLoader.CanHitNPC(player, item, npc))
			&& Update(ItemLoader.CanHitNPC(item, player, npc))
			&& Update(NPCLoader.CanBeHitByItem(npc, player, item));
		return ret;
	}

	public static void ModifyPlayerHitNPCWithItem(Player player, Item sItem, NPC nPC, ref StrikeModifiers modifiers)
	{
		ItemLoader.ModifyHitNPC(sItem, player, nPC, ref modifiers);
		NPCLoader.ModifyHitByItem(nPC, player, sItem, ref modifiers);
		PlayerLoader.ModifyHitNPC(player, sItem, nPC, ref modifiers);
	}

	public static void OnPlayerHitNPCWithItem(Player player, Item sItem, NPC nPC, in Strike strike, int damageDone)
	{
		ItemLoader.OnHitNPC(sItem, player, nPC, strike, damageDone);
		NPCLoader.OnHitByItem(nPC, player, sItem, strike, damageDone);
		PlayerLoader.OnHitNPC(player, sItem, nPC, strike, damageDone);
	}

	public static bool CanHitPvp(Player player, Item sItem, Player target)
	{
		return ItemLoader.CanHitPvp(sItem, player, target) && PlayerLoader.CanHitPvp(player, sItem, target);
	}

	public static void ModifyHitPvp(Player player, Item sItem, Player target, ref int damage, ref bool crit)
	{
		ItemLoader.ModifyHitPvp(sItem, player, target, ref damage, ref crit);
		PlayerLoader.ModifyHitPvp(player, sItem, target, ref damage, ref crit);
	}

	public static void OnHitPvp(Player player, Item sItem, Player target, int damage, bool crit)
	{
		ItemLoader.OnHitPvp(sItem, player, target, damage, crit);
		PlayerLoader.OnHitPvp(player, sItem, target, damage, crit);
	}

	public static void MeleeEffects(Player player, Item sItem, Rectangle itemRectangle)
	{
		ItemLoader.MeleeEffects(sItem, player, itemRectangle);
		PlayerLoader.MeleeEffects(player, sItem, itemRectangle);
	}

	public static bool? CanHitNPCWithProj(Projectile proj, NPC npc)
	{
		bool? ret = null;
		bool Update(bool? b) => (ret ??= b) is not false;

		_ = Update(PlayerLoader.CanHitNPCWithProj(proj, npc))
			&& Update(ProjectileLoader.CanHitNPC(proj, npc))
			&& Update(NPCLoader.CanBeHitByProjectile(npc, proj));
		return ret;
	}

	public static void ModifyHitNPCWithProj(Projectile projectile, NPC nPC, ref StrikeModifiers modifiers)
	{
		ProjectileLoader.ModifyHitNPC(projectile, nPC, ref modifiers);
		NPCLoader.ModifyHitByProjectile(nPC, projectile, ref modifiers);
		PlayerLoader.ModifyHitNPCWithProj(projectile, nPC, ref modifiers);
	}

	public static void OnHitNPCWithProj(Projectile projectile, NPC nPC, in Strike strike, int damageDone)
	{
		ProjectileLoader.OnHitNPC(projectile, nPC, strike, damageDone);
		NPCLoader.OnHitByProjectile(nPC, projectile, strike, damageDone);
		PlayerLoader.OnHitNPCWithProj(projectile, nPC, strike, damageDone);
	}

	public static bool CanBeHitByProjectile(Player player, Projectile projectile)
	{
		return ProjectileLoader.CanHitPlayer(projectile, player) && PlayerLoader.CanBeHitByProjectile(player, projectile);
	}

	public static void ModifyHitByProjectile(Player player, Projectile projectile, ref int damage, ref bool crit)
	{
		ProjectileLoader.ModifyHitPlayer(projectile, player, ref damage, ref crit);
		PlayerLoader.ModifyHitByProjectile(player, projectile, ref damage, ref crit);
	}

	public static void OnHitByProjectile(Player player, Projectile projectile, int realDamage, bool crit)
	{
		ProjectileLoader.OnHitPlayer(projectile, player, realDamage, crit);
		PlayerLoader.OnHitByProjectile(player, projectile, realDamage, crit);
	}

	public static bool CanHitPvpWithProj(Projectile projectile, Player target)
	{
		return ProjectileLoader.CanHitPvp(projectile, target) && PlayerLoader.CanHitPvpWithProj(projectile, target);
	}

	public static void ModifyHitPvpWithProj(Projectile projectile, Player target, ref int damage, ref bool crit)
	{
		ProjectileLoader.ModifyHitPvp(projectile, target, ref damage, ref crit);
		PlayerLoader.ModifyHitPvpWithProj(projectile, target, ref damage, ref crit);
	}

	public static void OnHitPvpWithProj(Projectile projectile, Player target, int damage, bool crit)
	{
		ProjectileLoader.OnHitPvp(projectile, target, damage, crit);
		PlayerLoader.OnHitPvpWithProj(projectile, target, damage, crit);
	}

	public static void ModifyItemScale(Player player, Item item, ref float scale)
	{
		ItemLoader.ModifyItemScale(item, player, ref scale);
		PlayerLoader.ModifyItemScale(player, item, ref scale);
	}

	public static float TotalUseSpeedMultiplier(Player player, Item item)
	{
		return PlayerLoader.UseSpeedMultiplier(player, item) * ItemLoader.UseSpeedMultiplier(item, player) * player.GetWeaponAttackSpeed(item);
	}

	public static float TotalUseTimeMultiplier(Player player, Item item)
	{
		float useTimeMult = PlayerLoader.UseTimeMultiplier(player, item) * ItemLoader.UseTimeMultiplier(item, player);
		if (!item.attackSpeedOnlyAffectsWeaponAnimation)
			useTimeMult /= TotalUseSpeedMultiplier(player, item);

		return useTimeMult;
	}

	public static int TotalUseTime(float useTime, Player player, Item item)
	{
		int result = Math.Max(1, (int)(useTime * TotalUseTimeMultiplier(player, item)));

		return result;
	}

	// TO-DO: should this be affected by item.attackSpeedOnlyAffectsWeaponAnimation?
	public static float TotalUseAnimationMultiplier(Player player, Item item)
	{
		float result = PlayerLoader.UseAnimationMultiplier(player, item) * ItemLoader.UseAnimationMultiplier(item, player);

		// UseSpeedMultiplier tries to affect both useTime and useAnimation in a way that doesn't break their relativity.
		// The code below multiplies useAnimation based on the difference that UseSpeedMultiplier makes on the item's useTime.
		// NOTE #3179, the * (1 / useSpeedMultiplier) is important. Due to floating point rounding errors, the order of multiply and divide must match the code in TotalUseTime above.
		//   Test case is useSpeedMultiplier = 1.1f, useTime = 33.
		//   33/1.1f = 30
		//   33*(1/1.1f) = 29.9999
		int multipliedUseTime = Math.Max(1, (int)(item.useTime * (1 / TotalUseSpeedMultiplier(player, item))));
		int relativeUseAnimation = Math.Max(1, (int)(multipliedUseTime * item.useAnimation / item.useTime));

		result *= relativeUseAnimation / (float)item.useAnimation;

		return result;
	}

	public static int TotalAnimationTime(float useAnimation, Player player, Item item)
	{
		int result = Math.Max(1, (int)(useAnimation * TotalUseAnimationMultiplier(player, item)));

		return result;
	}

	public static bool? CanConsumeBait(Player player, Item item)
	{
		bool? ret = PlayerLoader.CanConsumeBait(player, item);
		if (ItemLoader.CanConsumeBait(player, item) is bool b) {
			ret = (ret ?? true) && b;
		}
		return ret;
	}

	public static bool? CanCatchNPC(Player player, NPC npc, Item item)
	{
		bool? ret = null;
		bool Update(bool? b) => (ret ??= b) is not false;

		_ = Update(PlayerLoader.CanCatchNPC(player, npc, item))
			&& Update(ItemLoader.CanCatchNPC(item, npc, player))
			&& Update(NPCLoader.CanBeCaughtBy(npc, item, player));
		return ret;
	}

	public static void OnCatchNPC(Player player, NPC npc, Item item, bool failed)
	{
		PlayerLoader.OnCatchNPC(player, npc, item, failed);
		ItemLoader.OnCatchNPC(item, npc, player, failed);
		NPCLoader.OnCaughtBy(npc, player, item, failed);
	}

	public static bool CanNPCHitPlayer(NPC nPC, Player player, ref int specialHitSetter)
	{
		return NPCLoader.CanHitPlayer(nPC, player, ref specialHitSetter) && PlayerLoader.CanBeHitByNPC(player, nPC, ref specialHitSetter);
	}

	public static void ModifyNPCHitPlayer(NPC nPC, Player player, ref int damage, ref bool crit)
	{
		NPCLoader.ModifyHitPlayer(nPC, player, ref damage, ref crit);
		PlayerLoader.ModifyHitByNPC(player, nPC, ref damage, ref crit);
	}

	public static void OnNPCHitPlayer(NPC nPC, Player player, int damage, bool crit)
	{
		NPCLoader.OnHitPlayer(nPC, player, damage, crit);
		PlayerLoader.OnHitByNPC(player, nPC, damage, crit);
	}

	public static void PlayerFrameEffects(Player player)
	{
		PlayerLoader.FrameEffects(player);
		EquipLoader.EquipFrameEffects(player);
	}
}
