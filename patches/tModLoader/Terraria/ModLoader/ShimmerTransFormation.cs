using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.ID;

namespace Terraria.ModLoader;
// TML: #AdvancedShimmerTransformations

/// <summary>
/// Represents the behavior and output of a shimmer transformation, the <see cref="IModShimmerable"/>(s) that can use it are stored via <see cref="Transformations"/>
/// which is updated via <see cref="Register()"/> and its overloads. Uses a similar syntax to <see cref="Recipe"/>, usually starting with
/// <see cref="ModNPC.CreateShimmerTransformation"/> or <see cref="ModItem.CreateShimmerTransformation"/>
/// </summary>
public sealed class ShimmerTransformation : IComparable<ShimmerTransformation>, ICloneable
{
	#region Managers

	/// <summary>
	/// Dictionary containing every <see cref="ShimmerTransformation"/> registered to tMod indexed by <see cref="ModShimmerTypeID"/> and the entities type, automatically done in
	/// <see cref="Register()"/> and its overloads
	/// </summary>
	public static Dictionary<(ModShimmerTypeID, int), List<ShimmerTransformation>> Transformations { get; } = new();

	/// <summary>
	/// Incremented for every transformation with type <see cref="ModShimmerTypeID.Custom"/>
	/// </summary>
	private static int customShimmerTypeCounter = -1;

	/// <summary>
	/// Use this to get the id type for use with <see cref="ModShimmerTypeID.Custom"/>. Only for custom type implementations of <see cref="IModShimmerable"/>, where an
	/// integer maps to a specific set of <see cref="ShimmerTransformation"/> transformations
	/// </summary>
	public static int GetNextCustomShimmerID()
	{
		customShimmerTypeCounter++;
		return customShimmerTypeCounter;
	}

	internal static void Unload()
	{
		Transformations.Clear();
		customShimmerTypeCounter = -1;
	}

	#endregion Managers

	#region Redirects

	public static Dictionary<(ModShimmerTypeID, int), (ModShimmerTypeID, int)> Redirects { get; } = new();

	public static void AddRedirect(IModShimmerable redirectFrom, (ModShimmerTypeID, int) redirectTo)
		=> AddRedirect(redirectFrom.StorageKey, redirectTo);

	public static void AddRedirect((ModShimmerTypeID, int) redirectFrom, (ModShimmerTypeID, int) redirectTo)
		=> Redirects.Add(redirectFrom, redirectTo);

	/// <summary>
	/// First <see cref="Redirects"/> is checked for an entry, if one exists it is applied over <paramref name="source"/>, then if <paramref name="source"/> is
	/// <see cref="ModShimmerTypeID.Item"/>, it checks <see cref="ItemID.Sets.ShimmerCountsAsItem"/><br/> Is not recursive
	/// </summary>
	public static (ModShimmerTypeID, int) GetRedirectedKey((ModShimmerTypeID, int) source)
	{
		if (Redirects.TryGetValue(source, out (ModShimmerTypeID, int) value))
			source = value;
		if (source.Item1 == ModShimmerTypeID.Item && ItemID.Sets.ShimmerCountsAsItem[source.Item2] > 0)
			source = source with { Item2 = ItemID.Sets.ShimmerCountsAsItem[source.Item2] };
		return source;
	}

	/// <summary>
	/// First <see cref="Redirects"/> is checked for an entry, if one exists and it has the same <see cref="ModShimmerTypeID"/> it is applied over
	/// <paramref name="source"/>, then if <paramref name="source"/> is <see cref="ModShimmerTypeID.Item"/>, it checks <see cref="ItemID.Sets.ShimmerCountsAsItem"/><br/>
	/// Is not recursive
	/// </summary>
	public static int GetRedirectedKeySameShimmerID((ModShimmerTypeID, int) source)
	{
		if (Redirects.TryGetValue(source, out (ModShimmerTypeID, int) value) && value.Item1 == source.Item1)
			source = value;
		if (source.Item1 == ModShimmerTypeID.Item && ItemID.Sets.ShimmerCountsAsItem[source.Item2] > 0)
			source = source with { Item2 = ItemID.Sets.ShimmerCountsAsItem[source.Item2] };
		return source.Item2;
	}

	#endregion Redirects

	#region Constructors

	/// <inheritdoc cref="ShimmerTransformation"/>
	/// <param name="source"> Assigned to <see cref="SourceStorageKey"/> for use with the parameterless <see cref="Register()"/> </param>
	public ShimmerTransformation(IModShimmerable source)
	{
		SourceStorageKey = source.StorageKey;
	}

	/// <inheritdoc cref="ShimmerTransformation"/>
	public ShimmerTransformation()
	{ }

	#endregion Constructors

	#region FunctionalityVariables

	/// <summary>
	/// The <see cref="IModShimmerable"/> that was used to create this transformation, does not have to be used when registering
	/// </summary>
	private (ModShimmerTypeID, int)? SourceStorageKey { get; init; } // Private as it has no Modder use

	/// <summary>
	/// Every condition must be true for the transformation to occur
	/// </summary>
	public List<Condition> Conditions { get; init; } = new();

	/// <summary>
	/// The results that the transformation produces.
	/// </summary>
	public List<ModShimmerResult> Results { get; init; } = new();

	/// <summary>
	/// Vanilla disallows a transformation if the result includes either a bone or a lihzahrd brick when skeletron or golem are undefeated respectively
	/// </summary>
	public bool IgnoreVanillaItemConstraints { get; private set; }

	/// <summary>
	/// Gives a priority to the shimmer operation, lower numbers are sorted lower, higher numbers are sorted higher, clamps between -10 and 10
	/// </summary>
	public int Priority { get; private set; } = 0;

	/// <summary>
	/// Called in addition to conditions to check if the <see cref="IModShimmerable"/> shimmers
	/// </summary>
	/// <param name="transformation"> The transformation </param>
	/// <param name="source"> The <see cref="IModShimmerable"/> to be shimmered </param>
	public delegate bool CanShimmerCallBack(ShimmerTransformation transformation, IModShimmerable source);

	/// <inheritdoc cref="CanShimmerCallBack"/>
	public CanShimmerCallBack CanShimmerCallBacks { get; private set; }

	/// <summary>
	/// Called once before a transformation, use this to edit the transformation or source beforehand
	/// </summary>
	/// <param name="transformation"> The transformation, editing this does not change the stored transformation, only this time </param>
	/// <param name="source"> The <see cref="IModShimmerable"/> to be shimmered </param>
	public delegate void ModifyShimmerCallBack(ShimmerTransformation transformation, IModShimmerable source);

	/// <inheritdoc cref="ModifyShimmerCallBack"/>
	public ModifyShimmerCallBack ModifyShimmerCallBacks { get; private set; }

	/// <summary>
	/// Called after <see cref="IModShimmerable"/> shimmers
	/// </summary>
	/// <param name="transformation"> The transformation </param>
	/// <param name="spawnedEntities"> A list of the spawned Entities </param>
	/// <param name="source"> The <see cref="IModShimmerable"/> that was shimmered </param>
	public delegate void OnShimmerCallBack(ShimmerTransformation transformation, IModShimmerable source, List<IModShimmerable> spawnedEntities);

	/// <inheritdoc cref="OnShimmerCallBack"/>
	public OnShimmerCallBack OnShimmerCallBacks { get; private set; }

	#endregion FunctionalityVariables

	#region ControllerMethods

	/// <summary>
	/// Adds a condition to <see cref="Conditions"/>. <inheritdoc cref="Conditions"/>
	/// </summary>
	/// <param name="condition"> The condition to be added </param>
	public ShimmerTransformation AddCondition(Condition condition)
	{
		Conditions.Add(condition);
		return this;
	}

	#region AddResultMethods

	/// <summary>
	/// Adds a result to <see cref="Results"/>, this will be spawned when the <see cref="IModShimmerable"/> successfully shimmers
	/// </summary>
	/// <param name="result"> The result to be added </param>
	/// <exception cref="ArgumentException">
	/// thrown when <paramref name="result"/> does not have a valid spawn <see cref="ModShimmerTypeID"/> or has a <see cref="ModShimmerResult.Count"/> that is not greater
	/// than 0
	/// </exception>
	public ShimmerTransformation AddResult(ModShimmerResult result)
	{
		if (result.Count <= 0)
			throw new ArgumentException("A Count greater than 0 is required", nameof(result));

		Results.Add(result);
		return this;
	}

	/// <inheritdoc cref=" AddItemResult(int, int)"/>
	public ShimmerTransformation AddModItemResult<T>(int stack) where T : ModItem
		=> AddItemResult(ModContent.ItemType<T>(), stack);

	/// <inheritdoc cref=" AddResult(ModShimmerResult)"/>
	/// <param name="type"> The <see cref="Item.type"/> of the <see cref="Item"/> </param>
	/// <param name="stack"> The amount of Item to be spawned </param>
	public ShimmerTransformation AddItemResult(int type, int stack)
		=> AddResult(new ItemShimmerResult(type, stack));

	/// <inheritdoc cref="AddNPCResult(int, int)"/>
	public ShimmerTransformation AddModNPCResult<T>(int count) where T : ModNPC
		=> AddNPCResult(ModContent.NPCType<T>(), count);

	/// <inheritdoc cref=" AddResult(ModShimmerResult)"/>
	/// <param name="type"> The <see cref="NPC.type"/> of the <see cref="NPC"/> </param>
	/// <param name="count"> The amount of NPC to be spawned </param>
	public ShimmerTransformation AddNPCResult(int type, int count)
		=> AddResult(new NPCShimmerResult(type, count));

	/// <inheritdoc cref=" AddResult(ModShimmerResult)"/>
	/// <param name="coinLuck"> The amount of coin luck to be added </param>
	public ShimmerTransformation AddCoinLuckResult(int coinLuck)
		=> AddResult(new CoinLuckShimmerResult(coinLuck));

	#endregion AddResultMethods

	/// <inheritdoc cref="IgnoreVanillaItemConstraints"/>
	public ShimmerTransformation DisableVanillaItemConstraints()
	{
		IgnoreVanillaItemConstraints = true;
		return this;
	}

	/// <inheritdoc cref="Priority"/>
	public ShimmerTransformation SetPriority(int priority)
	{
		Priority = Math.Clamp(priority, -10, 10);
		return this;
	}

	/// <summary>
	/// Adds a delegate to <see cref="CanShimmerCallBacks"/> that will be called if the shimmer transformation succeeds
	/// </summary>
	public ShimmerTransformation AddCanShimmerCallBack(CanShimmerCallBack callBack)
	{
		CanShimmerCallBacks += callBack;
		return this;
	}

	/// <summary>
	/// Adds a delegate to <see cref="ModifyShimmerCallBacks"/> that will be called before the transformation
	/// </summary>
	public ShimmerTransformation AddModifyShimmerCallBack(ModifyShimmerCallBack callBack)
	{
		ModifyShimmerCallBacks += callBack;
		return this;
	}

	/// <summary>
	/// Adds a delegate to <see cref="OnShimmerCallBacks"/> that will be called if the shimmer transformation succeeds
	/// </summary>
	public ShimmerTransformation AddOnShimmerCallBack(OnShimmerCallBack callBack)
	{
		OnShimmerCallBacks += callBack;
		return this;
	}

	/// <inheritdoc cref="Register(ModShimmerTypeID, int)"/>
	/// <exception cref="InvalidOperationException"> Thrown if this <see cref="ShimmerTransformation"/> instance was not created from an Entity </exception>
	public void Register()
	{
		if (SourceStorageKey == null)
			throw new InvalidOperationException("The transformation must be created from an entity for the parameterless Register() to be used.");
		Register(SourceStorageKey.Value);
	}

	/// <inheritdoc cref="Register(ValueTuple{ModShimmerTypeID, int})"/>
	public void Register(ModShimmerTypeID modShimmerTypeID, int type)
		=> Register((modShimmerTypeID, type));

	/// <inheritdoc cref="Register(ModShimmerTypeID, int)"/>
	public void Register(IEnumerable<(ModShimmerTypeID, int)> sourceKeys)
	{
		foreach ((ModShimmerTypeID, int) ID in sourceKeys)
			Register(ID);
	}

	/// <summary>
	/// Finalizes transformation, adds to <see cref="Transformations"/>
	/// </summary>
	/// <exception cref="ArgumentException"> Thrown if <paramref name="sourceKey"/> field Item1 of type <see cref="ModShimmerTypeID"/> is an invalid source type </exception>
	public void Register((ModShimmerTypeID, int) sourceKey)
	{
		if (!sourceKey.Item1.IsValidSourceType())
			throw new ArgumentException("A valid source key for ModShimmerTypeID must be passed here", nameof(sourceKey));

		if (!Transformations.TryAdd(sourceKey, new() { this })) //Try add a new entry for the tuple
			Transformations[sourceKey].Add(this); // If it fails, entry exists, therefore add to list

		Transformations[sourceKey].Sort();
	}

	#endregion ControllerMethods

	#region Shimmering

	/// <summary>
	/// Checks if the <see cref="IModShimmerable"/> supplied can undergo a shimmer transformation, should not alter game state / read only
	/// </summary>
	/// <param name="shimmerable"> The <see cref="IModShimmerable"/> being shimmered </param>
	/// <returns>
	/// true if the following are all true in order
	/// <list type="number">
	/// <item/> All <see cref="Conditions"/> return true
	/// <item/> All added <see cref="CanShimmerCallBack"/> return true
	/// <item/> <see cref="IModShimmerable.CanShimmer"/> returns true for <paramref name="shimmerable"/>
	/// <item/> None of the results contain bone or lihzahrd brick while skeletron or golem are undefeated if <see cref="IgnoreVanillaItemConstraints"/> is false (default)
	/// <item/> The amount of empty NPC slots under slot 200 is less than the number of NPCs this transformation spawns
	/// </list>
	/// </returns>
	public bool CanModShimmer(IModShimmerable shimmerable)
		=> CanModShimmer_Transformation(shimmerable)
		&& shimmerable.CanShimmer();

	/// <summary>
	/// Checks the conditions for this transformation
	/// </summary>
	/// <returns>
	/// true if the following are all true in order
	/// <list type="number">
	/// <item/> All <see cref="Conditions"/> return true
	/// <item/>  All added <see cref="CanShimmerCallBack"/> return true
	/// <item/> None of the results contain bone or lihzahrd brick while skeletron or golem are undefeated if <see cref="IgnoreVanillaItemConstraints"/> is false (default)
	/// <item/> The amount of empty NPC slots under slot 200 is less than the number of NPCs this transformation spawns
	/// </list>
	/// </returns>
	public bool CanModShimmer_Transformation(IModShimmerable shimmerable)
		=> Conditions.All((condition) => condition.IsMet())
		&& (CheckCanShimmerCallBacks(shimmerable))
		&& (IgnoreVanillaItemConstraints || !Results.Any((result) => result is ItemShimmerResult item && (item.Type == ItemID.Bone && !NPC.downedBoss3 || item.Type == ItemID.LihzahrdBrick && !NPC.downedGolemBoss)))
		&& (GetCurrentAvailableNPCSlots() >= GetNPCSpawnCount());

	/// <summary>
	/// Checks all <see cref="CanShimmerCallBacks"/> for <paramref name="shimmerable"/>
	/// </summary>
	/// <returns> Returns true if all delegates in <see cref="CanShimmerCallBacks"/> return true </returns>
	public bool CheckCanShimmerCallBacks(IModShimmerable shimmerable)
	{
		foreach (CanShimmerCallBack callBack in CanShimmerCallBacks?.GetInvocationList()?.Cast<CanShimmerCallBack>() ?? Array.Empty<CanShimmerCallBack>()) {
			if (!callBack.Invoke(this, shimmerable))
				return false;
		}
		return true;
	}

	/// <summary>
	/// Checks every <see cref="ShimmerTransformation"/> for this <see cref="IModShimmerable"/> and returns true when if finds one that passes
	/// <see cref="CanModShimmer_Transformation(IModShimmerable)"/>. <br/> Does not check <see cref="IModShimmerable.CanShimmer"/>
	/// </summary>
	/// <returns> True if there is a mod transformation this <see cref="IModShimmerable"/> could undergo </returns>
	public static bool AnyValidModShimmer(IModShimmerable shimmerable)
	{
		if (!Transformations.ContainsKey(shimmerable.RedirectedStorageKey))
			return false;

		foreach (ShimmerTransformation modShimmer in Transformations[shimmerable.RedirectedStorageKey]) {
			if (modShimmer.CanModShimmer_Transformation(shimmerable))
				return true;
		}

		return false;
	}

	/// <summary>
	/// Tries to complete a shimmer operation on the <see cref="IModShimmerable"/> passed, should not be called on multiplayer clients
	/// </summary>
	/// <param name="source"> The <see cref="IModShimmerable"/> to be shimmered </param>
	/// <returns> True if the transformation is successful, false if it is should fall through to vanilla as normal </returns>
	public static bool TryModShimmer(IModShimmerable source)
	{
		List<ShimmerTransformation> transformations = Transformations.GetValueOrDefault(source.RedirectedStorageKey);
		if (!(transformations?.Count > 0)) // Invers to catch null
			return false;

		foreach (ShimmerTransformation transformation in transformations) { // Loops possible transformations
			if (transformation.Results.Count > 0 && transformation.CanModShimmer(source)) { // Checks conditions and callback in CanShimmer
				ShimmerTransformation copy = (ShimmerTransformation)transformation.Clone(); // Make a copy
				copy.ModifyShimmerCallBacks?.Invoke(copy, source); // As to not be effected by any changes made here
				DoModShimmer(source, copy);
				return true;
			}
		}
		return false;
	}

	public const int SingleShimmerNPCSpawnCap = 50;

	public int GetNPCSpawnCount()
		=> Results.Sum((ModShimmerResult result) => result is NPCShimmerResult ? result.Count : 0);

	private static int GetCurrentAvailableNPCSlots() => NPC.GetAvailableAmountOfNPCsToSpawnUpToSlot(SingleShimmerNPCSpawnCap, 200);

	/// <summary>
	/// Called by <see cref="TryModShimmer(IModShimmerable)"/> once it finds a valid transformation
	/// </summary>
	public static void DoModShimmer(IModShimmerable source, ShimmerTransformation transformation)
	{
		// 200 and 50 are the values vanilla uses for the highest slot to count with and the maximum NPCs to spawn in one transformation set
		int npcSpawnCount = transformation.GetNPCSpawnCount();
		int usableStack = npcSpawnCount != 0 ? Math.Min((int)MathF.Floor(GetCurrentAvailableNPCSlots() / (float)npcSpawnCount), source.Stack) : source.Stack;

		SpawnModShimmerResults(source, transformation, usableStack, out List<IModShimmerable> spawned); // Spawn results, output stack amount used
		source.Remove(usableStack); // Removed amount used
		transformation.OnShimmerCallBacks?.Invoke(transformation, source, spawned);

		ShimmerEffect(source.Center);
	}

	public static void SpawnModShimmerResults(IModShimmerable source, ShimmerTransformation transformation, int stackUsed, out List<IModShimmerable> spawned)
	{
		spawned = new(); // List to be passed for onShimmerCallBacks
		foreach (ModShimmerResult result in transformation.Results)
			result.Spawn(source, stackUsed, spawned); //Spawns the individual result, adds it to the list
	}

	/// <summary>
	/// Acts on the <paramref name="shimmerResult"/>. Result depends on <see cref="ModShimmerResult.ModShimmerTypeID"/>, usually spawns an <see cref="Item"/> or
	/// <see cref="NPC"/>. <br/> Does not despawn <paramref name="shimmerResult"/> or decrement <see cref="IModShimmerable.Stack"/>, use <see cref="IModShimmerable.Remove(int)"/>
	/// </summary>
	/// <param name="source"> The <see cref="IModShimmerable"/> that is shimmering, does not affect this </param>
	/// <param name="shimmerResult"> The Result to be spawned </param>
	/// <param name="stackUsed"> The amount of the <see cref="IModShimmerable"/> that is used, actual spawned amount will be <paramref name="stackUsed"/> * <see cref="ModShimmerResult.Count"/> </param>
	/// <param name="spawned"> A list of <see cref="IModShimmerable"/> passed to <see cref="OnShimmerCallBacks"/> </param>
	public static void ToRemove(IModShimmerable source, ModShimmerResult shimmerResult, int stackUsed, ref List<IModShimmerable> spawned)
	{
			//case ModShimmerTypeID.Custom:
			//	for (int i = 0; i < shimmerResult.Count; i++) {
			//		shimmerResult.CustomSpawner.Invoke(source, shimmerResult, source.ShimmerVelocity + GetShimmerSpawnVelocityModifier(), ref spawned);
			//	}
	}

	/// <summary>
	/// Creates the shimmer effect checking either single player or server
	/// </summary>
	/// <param name="position"> The position to create the effect </param>
	public static void ShimmerEffect(Vector2 position)
	{
		if (Main.netMode == NetmodeID.SinglePlayer)
			Item.ShimmerEffect(position);
		else if (Main.netMode == NetmodeID.Server)
			NetMessage.SendData(MessageID.ShimmerActions, -1, -1, null, 0, (int)position.X, (int)position.Y);
	}

	/// <summary>
	/// Creates a deep clone of <see cref="ShimmerTransformation"/>.
	/// </summary>
	public object Clone()
		=> new ShimmerTransformation() {
			SourceStorageKey = SourceStorageKey,
			Priority = Priority,
			Conditions = new List<Condition>(Conditions), // Technically I think the localization for the conditions can be changed
			Results = new List<ModShimmerResult>(Results), // List is new, ModShimmerResult is a readonly struct
			IgnoreVanillaItemConstraints = IgnoreVanillaItemConstraints, // Assigns by value
			CanShimmerCallBacks = (CanShimmerCallBack)CanShimmerCallBacks?.Clone(), // Stored values are immutable
			ModifyShimmerCallBacks = (ModifyShimmerCallBack)ModifyShimmerCallBacks?.Clone(),
			OnShimmerCallBacks = (OnShimmerCallBack)OnShimmerCallBacks?.Clone(),
		};

	public int CompareTo(ShimmerTransformation other)
		=> other.Priority - Priority;

	#endregion Shimmering

	#region Helpers

	//TODO: this.
	//public bool ContainsResult((ModShimmerTypeID, int) type)
	//	=> Results.Any((result) => result.IsSameResultType(type));

	//public bool ContainsAnyResult((ModShimmerTypeID, int)[] types)
	//	=> types.Any((type) => ContainsResult(type));

	//public bool ContainsResults((ModShimmerTypeID, int)[] types)
	//	=> types.All((type) => ContainsResult(type));

	#endregion Helpers
}

/// <summary>
/// Value used by <see cref="ModShimmerResult"/> to identify what type of <see cref="IModShimmerable"/> to spawn. <br/> When <see cref="Custom"/> it will try a null
/// checked call to the delegate <see cref="ModShimmerResult.CustomSpawner"/>
/// </summary>
public enum ModShimmerTypeID
{
	NPC,
	Item,
	CoinLuck,
	Custom,
}

/// <summary>
/// Extensions for <see cref="ModShimmerTypeID"/>
/// </summary>
public static class ModShimmerTypeIDExtensions
{
	/// <summary>
	/// <see cref="ModShimmerTypeID.NPC"/>, <see cref="ModShimmerTypeID.Item"/>, and <see cref="ModShimmerTypeID.Custom"/>
	/// </summary>
	public static bool IsValidSourceType(this ModShimmerTypeID id)
		=> id == ModShimmerTypeID.NPC || id == ModShimmerTypeID.Item || id == ModShimmerTypeID.Custom;

	/// <summary>
	/// <see cref="ModShimmerTypeID.NPC"/>, <see cref="ModShimmerTypeID.Item"/>, <see cref="ModShimmerTypeID.CoinLuck"/>, and <see cref="ModShimmerTypeID.Custom"/>
	/// </summary>
	public static bool IsValidSpawnedType(this ModShimmerTypeID id)
		=> id == ModShimmerTypeID.NPC || id == ModShimmerTypeID.Item || id == ModShimmerTypeID.CoinLuck || id == ModShimmerTypeID.Custom;
}

/// <summary>
/// Marks a class to be used with <see cref="ShimmerTransformation"/> as a source, most implementations for <see cref="NPC"/> and <see cref="Item"/> wrap normal values,
/// <see cref="ShimmerTransformation.TryModShimmer(IModShimmerable)"/> must be called manually for implementing types
/// </summary>
public interface IModShimmerable
{
	/// <inheritdoc cref="Entity.Center"/>
	public abstract Vector2 Center { get; set; }

	/// <inheritdoc cref="Entity.Hitbox"/>
	public abstract Rectangle Hitbox { get; set; }

	/// <summary>
	/// Wraps <see cref="Entity.velocity"/>
	/// </summary>
	public abstract Vector2 ShimmerVelocity { get; set; }

	/// <summary>
	/// Internal as types implementing outside tModLoader should always use <see cref="ModShimmerTypeID.Custom"/>, use <see cref="StorageKey"/> for modder access
	/// </summary>
	internal virtual ModShimmerTypeID ModShimmerTypeID => ModShimmerTypeID.Custom;

	/// <summary>
	/// Should return a value from <see cref="ShimmerTransformation.GetNextCustomShimmerID"/> when overriding that is constant for this type
	/// </summary>
	public abstract int ShimmerType { get; }

	/// <summary>
	/// Used as the key when both setting and retrieving for this <see cref="IModShimmerable"/> from <see cref="ShimmerTransformation.Transformations"/>
	/// </summary>
	public (ModShimmerTypeID, int) StorageKey => (ModShimmerTypeID, ShimmerType);

	/// <summary>
	/// <see cref="StorageKey"/> passed through <see cref="ShimmerTransformation.GetRedirectedKey(ValueTuple{ModShimmerTypeID, int})"/>
	/// </summary>
	public (ModShimmerTypeID, int) RedirectedStorageKey => ShimmerTransformation.GetRedirectedKey(StorageKey);

	/// <summary>
	/// When this undergoes a <see cref="ShimmerTransformation"/> this is the amount contained within one instance of the type, returns 1 for <see cref="NPC"/>, and
	/// <see cref="Item.stack"/> for <see cref="Item"/><br/> returns 1 by default
	/// </summary>
	public virtual int Stack => 1;

	/// <summary>
	/// Checks if this <see cref="IModShimmerable"/> can currently undergo a shimmer transformation. This includes both vanilla and <br/> Should not makes changes to game
	/// state. <br/> Treat as read only.
	/// </summary>
	/// <returns> True if the <see cref="IModShimmerable"/> currently has a valid shimmer operation it can use. </returns>
	public virtual bool CanShimmer() => true;

	/// <summary>
	/// Called at the end of shimmer
	/// </summary>
	public virtual void OnShimmer() { }

	/// <summary>
	/// Called once an entity Shimmers, int <see cref="Item"/> decrements <see cref="Item.stack"/>, handles despawning when <see cref="Stack"/> reaches 0
	/// </summary>
	public abstract void Remove(int amount);

	/// <summary>
	/// Returns <see cref="Entity.GetSource_Misc(string)"/> with passed value "shimmer" in for <see cref="NPC"/> and <see cref="Item"/>, used only for <see cref="ShimmerTransformation"/>
	/// </summary>
	public abstract IEntitySource GetSource_ForShimmer();
}