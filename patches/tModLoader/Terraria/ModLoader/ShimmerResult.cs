using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace Terraria.ModLoader;

/// <summary> A record representing the information to spawn an <see cref="IModShimmerable"/> during a shimmer transformation </summary>
/// <param name="Count"> The number of this result to spawn, true count will be multiplied by the stack size of the <see cref="IModShimmerable"/> source </param>
public abstract record class ModShimmerResult(int Count = 1)
{
	public abstract bool IsSameResultType(ModShimmerResult result);

	public virtual bool IsItemResult(int type)
		=> false;
	public virtual bool IsNPCResult(int type)
		=> false;
	public virtual bool IsProjectileResult(int type)
		=> false;

	/// <summary>
	/// Spawns <see cref="Count"/> * <paramref name="allowedStack"/> amount of the intended type <br/> Does not despawn <paramref name="shimmerable"/> or decrement
	/// <see cref="IModShimmerable.Stack"/>, use <see cref="IModShimmerable.ShimmerRemoveStacked(int)"/>
	/// </summary>
	/// <param name="shimmerable"> The <see cref="IModShimmerable"/> that is shimmering, does not affect this </param>
	/// <param name="allowedStack"> The amount of the <see cref="IModShimmerable"/> that is used, actual spawned amount will be <paramref name="allowedStack"/> * <see cref="Count"/> </param>
	/// <returns>
	/// yield returns an <see cref="IModShimmerable"/> or in the case of <see cref="CoinLuckShimmerResult"/> yield returns null. Will not return a null instance itself
	/// </returns>
	public abstract IEnumerable<IModShimmerable> SpawnFrom(IModShimmerable shimmerable, int allowedStack);

	/// <summary> Added to the the velocity of the <see cref="IModShimmerable"/> to prevent stacking </summary>
	public static Vector2 GetShimmerSpawnVelocityModifier()
		=> new Vector2(Main.rand.Next(-30, 31), Main.rand.Next(-40, -15)) * 0.1f;
	// => new(count * (1f + count * 0.05f) * ((count % 2 == 0) ? -1 : 1), 0);
	// What vanilla does for items with more than one ingredient, flings stuff everywhere as it's never supposed to do more than 15
	//So we're using the random spawn values from shimmered items instead, items push each other away when in the shimmer state anyway, so this is more for NPCs
}

public record class CoinLuckShimmerResult(int Count = 1) : ModShimmerResult(Count)
{
	public override bool IsSameResultType(ModShimmerResult result)
		=> result is CoinLuckShimmerResult;
	public override IEnumerable<IModShimmerable> SpawnFrom(IModShimmerable shimmerable, int allowedStack)
	{
		Main.player[Main.myPlayer].AddCoinLuck(shimmerable.Center, allowedStack * Count);
		NetMessage.SendData(MessageID.ShimmerActions, -1, -1, null, 1, shimmerable.Center.X, shimmerable.Center.Y, allowedStack * Count);
		yield return null;
	}
}

/// <param name="Type"> The type of the to spawn </param>
/// <param name="Count"> <inheritdoc/> </param>
/// <inheritdoc cref="ModShimmerResult(int)"/>
public abstract record class TypedShimmerResult(int Type, int Count = 1) : ModShimmerResult(Count);

/// <inheritdoc cref="TypedShimmerResult(int, int)"/>
public record class ItemShimmerResult(int Type, int Count = 1) : TypedShimmerResult(Type, Count)
{
	public override bool IsItemResult(int type)
		=> Type == type;
	public override bool IsSameResultType(ModShimmerResult result)
		=> result is ItemShimmerResult r && r.Type == Type;

	public override IEnumerable<IModShimmerable> SpawnFrom(IModShimmerable shimmerable, int allowedStack)
	{
		int spawnTotal = Count * allowedStack;
		while (spawnTotal > 0) {
			Item item = Main.item[Item.NewItem(shimmerable.GetSource_Misc("Shimmer"), shimmerable.Center, Type)]; //TODO: Get player bits here
			item.stack = Math.Min(item.maxStack, spawnTotal);
			item.shimmerTime = 1f;
			item.shimmered = true;
			item.velocity = shimmerable.Velocity + GetShimmerSpawnVelocityModifier();
			item.playerIndexTheItemIsReservedFor = Main.myPlayer;
			NetMessage.SendData(MessageID.SyncItemsWithShimmer, -1, -1, null, item.whoAmI, 1f); // net sync spawning the item

			yield return item;
			spawnTotal -= item.stack;
		}
	}
}

/// <inheritdoc cref="TypedShimmerResult(int, int)"/>
public record class NPCShimmerResult(int Type, int Count = 1) : TypedShimmerResult(Type, Count)
{
	/// <summary>
	/// Keeps <see cref="ShimmerTransformation"/> roughly in line with vanilla as far as base functionality goes when shimmering NPCs. If you have no reason to disable,
	/// don't. Here are the effects: <code>
	/// newNPC.extraValue = nPC.extraValue;
	/// newNPC.CopyInteractions(nPC);
	/// newNPC.spriteDirection = nPC.spriteDirection;
	///
	/// if (nPC.value == 0f)
	///		newNPC.value = 0f;
	/// newNPC.SpawnedFromStatue = nPC.SpawnedFromStatue;
	/// newNPC.shimmerTransparency = nPC.shimmerTransparency;
	/// newNPC.buffType = nPC.buffType[..];
	/// newNPC.buffTime = nPC.buffTime[..];
	///</code>
	/// </summary>
	public bool KeepVanillaTransformationConventions { get; init; }
	public override bool IsSameResultType(ModShimmerResult result)
		=> result is NPCShimmerResult r && r.Type == Type;
	public override bool IsNPCResult(int type)
		=> Type == type;

	public override IEnumerable<IModShimmerable> SpawnFrom(IModShimmerable shimmerable, int allowedStack)
	{
		int spawnTotal = Count * allowedStack;
		while (spawnTotal > 0) {
			NPC newNPC = NPC.NewNPCDirect(shimmerable.GetSource_Misc("Shimmer"), shimmerable.Center, Type);//TODO: Get player bits here

			//syncing up some values that vanilla intentionally sets after SetDefaults() is NPC transformations, mostly self explanatory
			if (shimmerable is NPC nPC && KeepVanillaTransformationConventions) {
				newNPC.extraValue = nPC.extraValue;
				newNPC.CopyInteractions(nPC);
				newNPC.spriteDirection = nPC.spriteDirection;

				if (nPC.value == 0f) // Statue stuff
					newNPC.value = 0f;
				newNPC.SpawnedFromStatue = nPC.SpawnedFromStatue;
				newNPC.shimmerTransparency = nPC.shimmerTransparency;
				newNPC.buffType = nPC.buffType[..]; // Pretty sure the manual way vanilla does it is actually the slowest way that isn't LINQ
				newNPC.buffTime = nPC.buffTime[..];
			}
			else {
				newNPC.shimmerTransparency = 1f;
			}
			newNPC.velocity = shimmerable.Velocity + GetShimmerSpawnVelocityModifier();
			newNPC.TargetClosest();

			if (Main.netMode == NetmodeID.Server) {
				NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, newNPC.whoAmI);
				NetMessage.SendData(MessageID.NPCBuffs, -1, -1, null, newNPC.whoAmI);
				newNPC.netUpdate = true;
			}

			yield return newNPC;
			spawnTotal--;
		}
	}
}

/// <inheritdoc cref="TypedShimmerResult(int, int)"/>
/// <param name="Type"> <inheritdoc/> </param>
/// <param name="Count"> <inheritdoc/> </param>
/// <param name="Damage"> Passed to <see cref="Projectile.NewProjectile(DataStructures.IEntitySource, Vector2, Vector2, int, int, float, int, float, float, float)"/> </param>
/// <param name="Knockback"> Passed to <see cref="Projectile.NewProjectile(DataStructures.IEntitySource, Vector2, Vector2, int, int, float, int, float, float, float)"/> </param>
/// <param name="Friendly"> Assigned to <see cref="Projectile.friendly"/> </param>
/// <param name="Hostile"> Assigned to <see cref="Projectile.hostile"/> </param>
public record class ProjectileShimmerResult(int Type, int Damage, int Knockback, bool Friendly, bool Hostile, int Count = 1) : TypedShimmerResult(Type, Count)
{
	public override bool IsProjectileResult(int type)
		=> Type == type;
	public override bool IsSameResultType(ModShimmerResult result)
		=> result is ProjectileShimmerResult r && r.Type == Type;

	public override IEnumerable<IModShimmerable> SpawnFrom(IModShimmerable shimmerable, int allowedStack)
	{
		int spawnTotal = Count * allowedStack;
		while (spawnTotal > 0) {
			Projectile projectile = Projectile.NewProjectileDirect(shimmerable.GetSource_Misc("Shimmer"), shimmerable.Center, shimmerable.Velocity + GetShimmerSpawnVelocityModifier(), Type, Damage, Knockback);
			projectile.position -= projectile.Size / 2;
			projectile.hostile = Hostile;
			projectile.friendly = Friendly;
			projectile.netUpdate = true;
			yield return projectile;

			spawnTotal--;
		}
	}
}
