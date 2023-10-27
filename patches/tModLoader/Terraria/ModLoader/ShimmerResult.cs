using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace Terraria.ModLoader;
public record class ShimmerInfo(int AllowedSpawnStack);
public interface IShimmerResult<in TShimmerable> where TShimmerable : IModShimmerable
{
	public abstract bool HandlesCleanup { get; }
	public abstract int Count { get; }

	/// <summary> Used to check if this is an <see cref="Item"/> of <paramref name="type"/> </summary>
	public bool IsItemResult(int type);

	/// <summary> Used to check if this is an <see cref="NPC"/> of <paramref name="type"/> </summary>
	public bool IsNPCResult(int type);

	/// <summary> Used to check if this is an <see cref="Projectile"/> of <paramref name="type"/> </summary>
	public bool IsProjectileResult(int type);

	/// <summary>
	/// Spawns <see cref="IShimmerResult{T}.Count"/> * <paramref name="shimmerInfo"/> amount of the intended type <br/> Does not despawn <paramref name="shimmerable"/> or decrement
	/// <see cref="IModShimmerable.Stack"/>, use <see cref="IModShimmerable.ShimmerRemoveStacked(int)"/>
	/// </summary>
	/// <param name="shimmerable"> The <see cref="IModShimmerable"/> that is shimmering, does not affect this </param>
	/// <param name="shimmerInfo"> The amount of the <see cref="IModShimmerable"/> that is used, actual spawned amount will be <paramref name="shimmerInfo"/> * <see cref="Count"/> </param>
	/// <returns> yield returns an <see cref="IModShimmerable"/> or in the case of <see cref="CoinLuckShimmerResult"/> yield returns null. Will not return a null instance itself </returns>
	public abstract IEnumerable<IModShimmerable> SpawnFrom(TShimmerable shimmerable, ShimmerInfo shimmerInfo);
}
/// <summary> A record representing the information to spawn an <see cref="IModShimmerable"/> during a shimmer transformation </summary>
/// <param name="Count"> The number of this result to spawn, true count will be multiplied by the stack size of the <see cref="IModShimmerable"/> source </param>
public abstract record class GeneralShimmerResult(int Count = 1) : IShimmerResult<IModShimmerable>
{
	public virtual bool HandlesCleanup
		=> false;

	public virtual bool IsItemResult(int type)
		=> false;
	public virtual bool IsNPCResult(int type)
		=> false;
	public virtual bool IsProjectileResult(int type)
		=> false;
	public abstract IEnumerable<IModShimmerable> SpawnFrom(IModShimmerable shimmerable, ShimmerInfo shimmerInfo);
}

public record class CoinLuckShimmerResult(int Count = 1) : GeneralShimmerResult(Count)
{
	public override IEnumerable<IModShimmerable> SpawnFrom(IModShimmerable shimmerable, ShimmerInfo shimmerInfo)
	{
		Main.player[Main.myPlayer].AddCoinLuck(shimmerable.Center, shimmerInfo.AllowedSpawnStack * Count);
		NetMessage.SendData(MessageID.ShimmerActions, -1, -1, null, 1, shimmerable.Center.X, shimmerable.Center.Y, shimmerInfo.AllowedSpawnStack * Count);
		yield return null;
	}
}

/// <param name="Type"> The type of the to spawn </param>
/// <param name="Count"> <inheritdoc cref="GeneralShimmerResult(int)"/> </param>
/// <summary>
/// <inheritdoc cref="GeneralShimmerResult(int)"/><br/> Using <see cref="IShimmerResult{T}.IsItemResult(int)"/> and its relatives, this type can be used for inter-mod compatibility when implementing
/// custom spawning behaviour across known types
/// </summary>
public abstract record class TypedShimmerResult(int Type, int Count = 1) : GeneralShimmerResult(Count);

/// <inheritdoc cref="TypedShimmerResult(int, int)"/>
public record class ItemShimmerResult(int Type, int Count = 1) : TypedShimmerResult(Type, Count), IShimmerResult<IModShimmerable>
{
	public override bool IsItemResult(int type)
		=> Type == type;

	public override IEnumerable<Item> SpawnFrom(IModShimmerable shimmerable, ShimmerInfo shimmerInfo)
	{
		int spawnTotal = Count * shimmerInfo.AllowedSpawnStack;
		while (spawnTotal > 0) {
			Item item = Main.item[Item.NewItem(shimmerable.GetSource_Misc("Shimmer"), shimmerable.Center, Type)];
			item.position -= item.Size / 2; // Centre
			item.stack = Math.Min(item.maxStack, spawnTotal);
			item.shimmerTime = 1f;
			item.shimmered = true;
			item.velocity = shimmerable.Velocity + ShimmerManager.GetShimmerSpawnVelocityModifier();
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
	/// Keeps <see cref="ShimmerTransformation"/> roughly in line with vanilla as far as base functionality goes when shimmering NPCs. Defaults to true. If you have no reason to disable,
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
	public bool KeepVanillaTransformationConventions { get; init; } = true;
	/// <summary> Passed to <see cref="NPC.NewNPC(DataStructures.IEntitySource, int, int, int, int, float, float, float, float, int)"/> </summary>
	public int Start { get; init; }
	/// <summary> Passed to <see cref="NPC.NewNPC(DataStructures.IEntitySource, int, int, int, int, float, float, float, float, int)"/> </summary>
	public float AI0 { get; init; }
	/// <summary> Passed to <see cref="NPC.NewNPC(DataStructures.IEntitySource, int, int, int, int, float, float, float, float, int)"/> </summary>
	public float AI1 { get; init; }
	/// <summary> Passed to <see cref="NPC.NewNPC(DataStructures.IEntitySource, int, int, int, int, float, float, float, float, int)"/> </summary>
	public float AI2 { get; init; }
	/// <summary> Passed to <see cref="NPC.NewNPC(DataStructures.IEntitySource, int, int, int, int, float, float, float, float, int)"/> </summary>
	public float AI3 { get; init; }

	/// <summary> Wraps <see cref="AI0"/>, <see cref="AI1"/>, <see cref="AI2"/>, and <see cref="AI3"/> </summary>
	public float[] AI { get => new float[4] { AI0, AI1, AI2, AI3 }; init { AI0 = value[0]; AI1 = value[1]; AI2 = value[2]; AI3 = value[3]; } }
	/// <summary> Assigned to <see cref="NPC.scale"/> </summary>
	public float? Scale { get; init; }
	public override bool IsNPCResult(int type)
		=> Type == type;

	public override IEnumerable<NPC> SpawnFrom(IModShimmerable shimmerable, ShimmerInfo shimmerInfo)
	{
		int spawnTotal = Count * shimmerInfo.AllowedSpawnStack;
		while (spawnTotal > 0) {
			NPC newNPC = NPC.NewNPCDirect(shimmerable.GetSource_Misc("Shimmer"), shimmerable.Center, Type, Start, AI0, AI1, AI2, AI3);
			newNPC.position -= newNPC.Size / 2; // Centre

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
			if (shimmerable is Projectile projectile && KeepVanillaTransformationConventions) {
				newNPC.shimmerTransparency = projectile.shimmerTransformTime;
			}
			else {
				newNPC.shimmerTransparency = 1f;
			}
			newNPC.velocity = shimmerable.Velocity + ShimmerManager.GetShimmerSpawnVelocityModifier();
			if (Scale != null)
				newNPC.scale = Scale.Value;
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

/// <summary>
/// <inheritdoc/><br/> Projectiles are rarely both autonomous and do not require an owner, due to this there is no <see cref="ShimmerManager.AddItemResult{TSelf}(TSelf, int, int)"/> for projectiles.
/// <see cref="ShimmerManager.AddResult{TSelf}(TSelf, GeneralShimmerResult)"/> must be used instead. This class is more as a framework to derive from rather than a working class since for the large
/// majority of projectiles spawn behaviour or a target will need to be defined.
/// </summary>
/// <inheritdoc cref="TypedShimmerResult(int, int)"/>
/// <param name="Type"> <inheritdoc/> </param>
/// <param name="Count"> <inheritdoc/> </param>
/// <param name="Damage"> Passed to <see cref="Projectile.NewProjectile(DataStructures.IEntitySource, Vector2, Vector2, int, int, float, int, float, float, float)"/> </param>
/// <param name="Knockback"> Passed to <see cref="Projectile.NewProjectile(DataStructures.IEntitySource, Vector2, Vector2, int, int, float, int, float, float, float)"/> </param>
/// <param name="Friendly"> Assigned to <see cref="Projectile.friendly"/> </param>
/// <param name="Hostile"> Assigned to <see cref="Projectile.hostile"/> </param>
public record class ProjectileShimmerResult(int Type, int Damage, int Knockback, bool Friendly = false, bool Hostile = false, int Count = 1) : TypedShimmerResult(Type, Count)
{
	/// <summary> Passed to <see cref="Projectile.NewProjectile(DataStructures.IEntitySource, Vector2, Vector2, int, int, float, int, float, float, float)"/> </summary>
	public float AI0 { get; init; }
	/// <summary> Passed to <see cref="Projectile.NewProjectile(DataStructures.IEntitySource, Vector2, Vector2, int, int, float, int, float, float, float)"/> </summary>
	public float AI1 { get; init; }
	/// <summary> Passed to <see cref="Projectile.NewProjectile(DataStructures.IEntitySource, Vector2, Vector2, int, int, float, int, float, float, float)"/> </summary>
	public float AI2 { get; init; }
	/// <summary> Wraps <see cref="AI0"/>, <see cref="AI1"/>, and <see cref="AI2"/> </summary>
	public float[] AI { get => new float[3] { AI0, AI1, AI2 }; init { AI0 = value[0]; AI1 = value[1]; AI2 = value[2]; } }
	/// <summary> Assigned to <see cref="Projectile.scale"/> if a value is present </summary>
	public float? Scale { get; init; }

	/// <summary>
	/// If true, when spawning a projectile this will check if the entity shimmering into this was a projectile, and if it was, will set <see cref="Projectile.friendly"/> and
	/// <see cref="Projectile.hostile"/> to its values. <br/> Overrides <see cref="Hostile"/> and <see cref="Friendly"/>.
	/// </summary>
	public bool TryCopyHostileFriendly;
	public override bool IsProjectileResult(int type)
		=> Type == type;

	public override IEnumerable<Projectile> SpawnFrom(IModShimmerable shimmerable, ShimmerInfo shimmerInfo)
	{
		int spawnTotal = Count * shimmerInfo.AllowedSpawnStack;
		while (spawnTotal > 0) {
			Projectile projectile = Projectile.NewProjectileDirect(shimmerable.GetSource_Misc("Shimmer"), shimmerable.Center, shimmerable.Velocity + ShimmerManager.GetShimmerSpawnVelocityModifier(), Type, Damage, Knockback, ai0: AI0, ai1: AI1, ai2: AI2);
			projectile.position -= projectile.Size / 2;
			if (Scale != null)
				projectile.scale = Scale.Value;

			if (TryCopyHostileFriendly && shimmerable is Projectile oldProjectile) {
				projectile.hostile = oldProjectile.hostile;
				projectile.friendly = oldProjectile.friendly;
			}
			else {
				projectile.hostile = Hostile;
				projectile.friendly = Friendly;
			}

			if (Main.netMode == NetmodeID.Server) {
				NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projectile.whoAmI);
				projectile.netUpdate = true;
			}

			yield return projectile;

			spawnTotal--;
		}
	}
}