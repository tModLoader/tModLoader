using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Terraria.DataStructures;
using Terraria.ID;

namespace Terraria.ModLoader;

/// <summary>
/// A record representing the information to spawn an <see cref="IModShimmerable"/> during a shimmer transformation
/// </summary>
/// <param name="ModShimmerTypeID"> The type of shimmer operation this represents </param>
/// <param name="Type">
/// The type of the <see cref="IModShimmerable"/> to spawn, ignored when <paramref name="ModShimmerTypeID"/> is <see cref="ModShimmerTypeID.CoinLuck"/> or
/// <see cref="ModShimmerTypeID.Custom"/> although it is passed into <see cref="CustomSpawner"/> so can be used for custom logic
/// </param>
/// <param name="Count">
/// The number of this <see cref="IModShimmerable"/> to spawn, if <paramref name="ModShimmerTypeID"/> is <see cref="ModShimmerTypeID.CoinLuck"/> this is the coin luck
/// value, if <see cref="ModShimmerTypeID.Custom"/>, this is the amount of times <see cref="CustomSpawner"/> will be called
/// </param>
/// <param name="KeepVanillaTransformationConventions">
/// Keeps <see cref="ShimmerTransformation"/> roughly in line with vanilla as far as base functionality goes, e.g. NPC's spawned via statues stay keep their spawned NPCs from a
/// statue when shimmered, if you have no reason to disable, don't
/// </param>
public abstract record class ModShimmerResult(/*ModShimmerTypeID ModShimmerTypeID, int Type, */int Count/*, bool KeepVanillaTransformationConventions*/)
{
	///// <inheritdoc cref="ModShimmerResult(ModShimmerTypeID, int, int, bool)"/>
	//public ModShimmerResult() : this(default, default, default, default) { }

	///// <inheritdoc cref="ModShimmerResult(ModShimmerTypeID, int, int, bool)"/>
	//public ModShimmerResult(ModShimmerTypeID ResultType, int Type, int Count) : this(ResultType, Type, Count, true) { }

	//public (ModShimmerTypeID, int) CompleteResultType => (ModShimmerTypeID, Type);

	public abstract bool IsSameResultType(ModShimmerResult result);

	public virtual bool IsSameResult(ModShimmerResult result)
		=> result == this;
	public abstract void Spawn(IModShimmerable shimmerable, int allowedStack, List<IModShimmerable> spawned);

	/// <summary>
	/// Added the the velocity of the <see cref="IModShimmerable"/> to prevent stacking
	/// </summary>
	public static Vector2 GetShimmerSpawnVelocityModifier()
		// What vanilla does for items with more than one ingredient, flings stuff everywhere as it's never supposed to do more than 15
		// => new(count * (1f + count * 0.05f) * ((count % 2 == 0) ? -1 : 1), 0);
		=> new Vector2(Main.rand.Next(-30, 31), Main.rand.Next(-40, -15)) * 0.1f; //So we're using the random spawn values from shimmered items instead, items push each other away when in the shimmer state anyway, so this is more for NPCs

}
public record class CoinLuckShimmerResult(int Count) : ModShimmerResult(Count)
{
	public override bool IsSameResultType(ModShimmerResult result)
		=> result is CoinLuckShimmerResult;
	public override void Spawn(IModShimmerable shimmerable, int allowedStack, List<IModShimmerable> spawned)
	{
		Main.player[Main.myPlayer].AddCoinLuck(shimmerable.Center, allowedStack * Count);
		NetMessage.SendData(MessageID.ShimmerActions, -1, -1, null, 1, shimmerable.Center.X, shimmerable.Center.Y, allowedStack * Count);
	}
}
public abstract record class TypedShimmerResult(int Type, int Count) : ModShimmerResult(Count);
public record class ItemShimmerResult(int Type, int Count) : TypedShimmerResult(Type, Count)
{
	public override bool IsSameResultType(ModShimmerResult result)
		=> result is ItemShimmerResult r && r.Type == Type;

	public override void Spawn(IModShimmerable shimmerable, int allowedStack, List<IModShimmerable> spawned)
	{
		int spawnTotal = Count * allowedStack;
		while (spawnTotal > 0)
		{
			Item item = Main.item[Item.NewItem(shimmerable.GetSource_ForShimmer(), shimmerable.Center, Type)];
			item.stack = Math.Min(item.maxStack, spawnTotal);
			item.shimmerTime = 1f;
			item.shimmered = item.shimmerWet = item.wet = true;
			item.velocity = shimmerable.ShimmerVelocity + GetShimmerSpawnVelocityModifier();
			item.playerIndexTheItemIsReservedFor = Main.myPlayer;
			NetMessage.SendData(MessageID.SyncItemsWithShimmer, -1, -1, null, item.whoAmI, 1f); // net sync spawning the item

			spawned.Add(item);
			spawnTotal -= item.stack;
		}
	}
}

public record class NPCShimmerResult(int Type, int Count) : TypedShimmerResult(Type, Count)
{
	public override bool IsSameResultType(ModShimmerResult result)
		=> result is NPCShimmerResult r && r.Type == Type;

	public override void Spawn(IModShimmerable shimmerable, int allowedStack, List<IModShimmerable> spawned)
	{
		int spawnTotal = Count * allowedStack;
		while (spawnTotal > 0)
		{
			NPC newNPC = NPC.NewNPCDirect(shimmerable.GetSource_ForShimmer(), shimmerable.Center, Type);

			//TODO: This.
			//syncing up some values that vanilla intentionally sets after SetDefaults() is NPC transformations, mostly self explanatory
			//if (source is NPC nPC && shimmerResult.KeepVanillaTransformationConventions)
			//{
			//	newNPC.extraValue = nPC.extraValue;
			//	newNPC.CopyInteractions(nPC);
			//	newNPC.spriteDirection = nPC.spriteDirection;

			//	if (nPC.value == 0f) // Statue stuff
			//		newNPC.value = 0f;
			//	newNPC.SpawnedFromStatue = nPC.SpawnedFromStatue;
			//	newNPC.shimmerTransparency = nPC.shimmerTransparency;
			//	newNPC.buffType = nPC.buffType[..]; // Pretty sure the manual way vanilla does it is actually the slowest way that isn't LINQ
			//	newNPC.buffTime = nPC.buffTime[..];
			//}
			//else
			{
				newNPC.shimmerTransparency = 1f;
			}
			newNPC.velocity = shimmerable.ShimmerVelocity + GetShimmerSpawnVelocityModifier();
			newNPC.TargetClosest();

			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, newNPC.whoAmI);
				NetMessage.SendData(MessageID.NPCBuffs, -1, -1, null, newNPC.whoAmI);
				newNPC.netUpdate = true;
			}

			spawned.Add(newNPC);
			spawnTotal--;
		}
	}
}

//public record class ShimmerDataPacket(IEntitySource Source, Rectangle HitBox, Vector2 Velocity, int Stack)
//{
//	public Vector2 Center => HitBox.Center();
//}