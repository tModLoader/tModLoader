using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.Utilities;

namespace Terraria.ModLoader;
// TML: #AdvancedShimmerTransformations

/// <summary>
/// Represents the behavior and output of a shimmer transformation, the <see cref="IModShimmerable"/>(s) that can use it are stored via
/// <see cref="ShimmerTransformation{TShimmeredType}.Transformations"/> which is updated via <see cref="Register()"/> and its overloads. Uses a similar syntax to
/// <see cref="Recipe"/>, usually starting with <see cref="ModNPC.CreateShimmerTransformation"/> or <see cref="ModItem.CreateShimmerTransformation"/>
/// </summary>
public abstract class ShimmerTransformation : ICloneable
{
	#region Management

	/// <summary> See <see cref="AddAsKnownType{TModShimmerable}"/> </summary>
	private static Action extraKnownTypeResets;

	/// <summary> See <see cref="AddAsKnownType{TModShimmerable}"/> </summary>
	private static Action extraKnownTypeOrders;

	/// <summary> Called on the first item added to the dictionary for a given type </summary>
	protected static void AddAsKnownType<TModShimmerable>() where TModShimmerable : IModShimmerable
	{
		extraKnownTypeResets += ShimmerTransformation<TModShimmerable>.Reset;
		extraKnownTypeOrders += ShimmerTransformation<TModShimmerable>.Order;
	}

	/// <summary> Called during unloading </summary>
	internal static void ResetKnown()
	{
		extraKnownTypeResets?.Invoke();
		extraKnownTypeResets = null; // Clear for the next load
		extraKnownTypeOrders = null;
	}

	/// <summary> Called near recipe ordering when loading </summary>
	internal static void OrderKnown()
	{
		extraKnownTypeOrders?.Invoke();
	}

	/// <inheritdoc cref="Disabled"/>
	public void Disable()
		=> Disabled = true;

	/// <summary> Prevents the transformation from being used. </summary>
	public bool Disabled { get; set; }

	#endregion Management

	#region Variables

	/// <summary> Every condition must be true for the transformation to occur </summary>
	public List<Condition> Conditions { get; init; } = new();

	/// <summary> The results that the transformation produces. </summary>
	public List<ModShimmerResult> Results { get; init; } = new();

	/// <summary> Vanilla disallows a transformation if the result includes either a bone or a lihzahrd brick when skeletron or golem are undefeated respectively </summary>
	public bool IgnoreVanillaItemConstraints { get; private protected set; }

	/// <summary>
	/// In Vanilla if an entity shimmers, if the new entity has a transformation then it will transform immediately on the next frame, setting this means that the new
	/// entity will have to leave shimmer for a period of time before transforming again.
	/// </summary>
	public bool AllowChainedShimmers { get; private protected set; } = true;

	/// <summary> Called in addition to conditions to check if the <see cref="IModShimmerable"/> shimmers </summary>
	/// <param name="transformation"> The transformation </param>
	/// <param name="source"> The <see cref="IModShimmerable"/> to be shimmered </param>
	public delegate bool CanShimmerCallBack(ShimmerTransformation transformation, IModShimmerable source);

	/// <inheritdoc cref="CanShimmerCallBack"/>
	public CanShimmerCallBack CanShimmerCallBacks { get; private protected set; }

	/// <summary> Called once before a transformation, use this to edit the transformation or type beforehand </summary>
	/// <param name="transformation"> The transformation, editing this does not change the stored transformation, only this time </param>
	/// <param name="source"> The <see cref="IModShimmerable"/> to be shimmered </param>
	public delegate void ModifyShimmerCallBack(ShimmerTransformation transformation, IModShimmerable source);

	/// <inheritdoc cref="ModifyShimmerCallBack"/>
	public ModifyShimmerCallBack ModifyShimmerCallBacks { get; private protected set; }

	/// <summary> Called after <see cref="IModShimmerable"/> shimmers </summary>
	/// <param name="transformation"> The transformation </param>
	/// <param name="spawnedEntities"> A list of the spawned Entities </param>
	/// <param name="source"> The <see cref="IModShimmerable"/> that was shimmered </param>
	public delegate void OnShimmerCallBack(ShimmerTransformation transformation, IModShimmerable source, IEnumerable<IModShimmerable> spawnedEntities);

	/// <inheritdoc cref="OnShimmerCallBack"/>
	public OnShimmerCallBack OnShimmerCallBacks { get; private protected set; }

	#endregion Variables

	#region AbstractControllerMethods

	/// <summary> Adds a condition to <see cref="Conditions"/>. <inheritdoc cref="Conditions"/> </summary>
	/// <param name="condition"> The condition to be added </param>
	public abstract ShimmerTransformation AddCondition(Condition condition);

	#region AddResultMethods

	/// <summary> Adds a result to <see cref="Results"/>, this will be spawned when the <see cref="IModShimmerable"/> successfully shimmers </summary>
	/// <param name="result"> The result to be added </param>
	/// <exception cref="ArgumentException"> thrown when <paramref name="result"/> has a <see cref="ModShimmerResult.Count"/> that is less than or equal to zero </exception>
	public abstract ShimmerTransformation AddResult(ModShimmerResult result);

	/// <inheritdoc cref=" AddItemResult(int, int)"/>
	public abstract ShimmerTransformation AddModItemResult<T>(int stack = 1) where T : ModItem;

	/// <inheritdoc cref=" AddResult(ModShimmerResult)"/>
	/// <param name="type"> The <see cref="Item.type"/> of the <see cref="Item"/> </param>
	/// <param name="stack"> The amount of Item to be spawned </param>
	public abstract ShimmerTransformation AddItemResult(int type, int stack = 1);

	/// <inheritdoc cref="AddNPCResult(int, int)"/>
	public abstract ShimmerTransformation AddModNPCResult<T>(int count = 1) where T : ModNPC;

	/// <inheritdoc cref=" AddResult(ModShimmerResult)"/>
	/// <param name="type"> The <see cref="NPC.type"/> of the <see cref="NPC"/> </param>
	/// <param name="count"> The amount of NPC to be spawned </param>
	public abstract ShimmerTransformation AddNPCResult(int type, int count = 1);

	/// <inheritdoc cref=" AddResult(ModShimmerResult)"/>
	/// <param name="coinLuck"> The amount of coin luck to be added </param>
	public abstract ShimmerTransformation AddCoinLuckResult(int coinLuck);

	#endregion AddResultMethods

	/// <inheritdoc cref="IgnoreVanillaItemConstraints"/>
	public abstract ShimmerTransformation DisableVanillaItemConstraints();

	/// <inheritdoc cref="AllowChainedShimmers"/>
	public abstract ShimmerTransformation DisableChainedShimmers();

	/// <summary> Adds a delegate to <see cref="CanShimmerCallBacks"/> that will be called if the shimmer transformation succeeds </summary>
	public abstract ShimmerTransformation AddCanShimmerCallBack(CanShimmerCallBack callBack);

	/// <summary> Adds a delegate to <see cref="ModifyShimmerCallBacks"/> that will be called before the transformation </summary>
	public abstract ShimmerTransformation AddModifyShimmerCallBack(ModifyShimmerCallBack callBack);

	/// <summary> Adds a delegate to <see cref="OnShimmerCallBacks"/> that will be called if the shimmer transformation succeeds </summary>
	public abstract ShimmerTransformation AddOnShimmerCallBack(OnShimmerCallBack callBack);

	/// <inheritdoc cref="Register(int)"/>
	/// <exception cref="InvalidOperationException"> Thrown if this instance was not created from an Entity </exception>
	public abstract void Register();

	/// <inheritdoc cref="Register(IEnumerable{int})"/>
	public abstract void Register(params int[] types);

	/// <inheritdoc cref="Register(int)"/>
	public abstract void Register(IEnumerable<int> types);

	/// <summary> Finalizes transformation </summary>
	public abstract void Register(int type);

	/// <summary> Sorts the <see cref="ShimmerTransformation"/> before the one given as parameter. Both <see cref="ShimmerTransformation"/> must already be registered. </summary>
	public abstract ShimmerTransformation SortBefore(ShimmerTransformation transformation, int forType);

	/// <summary> Sorts the <see cref="ShimmerTransformation"/> after the one given as parameter. Both <see cref="ShimmerTransformation"/> must already be registered. </summary>
	public abstract ShimmerTransformation SortAfter(ShimmerTransformation transformation, int forType);

	#endregion AbstractControllerMethods

	#region Shimmering

	/// <summary>
	/// Full checks to see if the <see cref="IModShimmerable"/> can undergo <b> this transformation. </b><br/> calls
	/// <see cref="CanModShimmer_Transformation(IModShimmerable)"/> and <see cref="IModShimmerable.CanShimmer"/>. <br/> Should not alter game state / treat as read only
	/// </summary>
	/// <param name="shimmerable"> The <see cref="IModShimmerable"/> being shimmered </param>
	/// <returns>
	/// <inheritdoc cref="CanModShimmer_Transformation(IModShimmerable)"/>
	/// <list type="bullet">
	/// <item/>
	/// <see cref="IModShimmerable.CanShimmer"/> returns true for <paramref name="shimmerable"/>
	/// </list>
	/// </returns>
	public bool CanModShimmer(IModShimmerable shimmerable)
		=> CanModShimmer_Transformation(shimmerable)
		&& shimmerable.CanShimmer();

	/// <summary>
	/// Checks that the <see cref="IModShimmerable"/> passed meets all the conditions for <b> this transformation. </b><br/> Should not alter game state / treat as read only
	/// </summary>
	/// <returns>
	/// true if the following are all true in order
	/// <list type="bullet">
	/// <item/>
	/// The transformation is not disabled
	/// <item/>
	/// All <see cref="Conditions"/> return true
	/// <item/>
	/// All added <see cref="CanShimmerCallBack"/> return true
	/// <item/>
	/// None of the results contain bone or lihzahrd brick while skeletron or golem are undefeated if <see cref="IgnoreVanillaItemConstraints"/> is false (Used by vanilla
	/// for progression protection)
	/// <item/>
	/// The amount of empty NPC slots under slot 200 is less than the number of NPCs this transformation spawns
	/// </list>
	/// </returns>
	public bool CanModShimmer_Transformation(IModShimmerable shimmerable)
		=> !Disabled
		&& Conditions.All((condition) => condition.IsMet())
		&& CheckCanShimmerCallBacks(shimmerable)
		&& (IgnoreVanillaItemConstraints || !Results.Any((result) => result.IsItemResult(ItemID.Bone) && !NPC.downedBoss3 || result.IsItemResult(ItemID.LihzahrdBrick) && !NPC.downedGolemBoss))
		&& (GetCurrentAvailableNPCSlots() >= GetSpawnCount<NPCShimmerResult>());

	/// <summary> Checks all <see cref="CanShimmerCallBacks"/> for <paramref name="shimmerable"/> </summary>
	/// <returns> Returns true if all delegates in <see cref="CanShimmerCallBacks"/> return true or there are no callbacks </returns>
	public bool CheckCanShimmerCallBacks(IModShimmerable shimmerable)
	{
		foreach (CanShimmerCallBack callBack in CanShimmerCallBacks?.GetInvocationList()?.Cast<CanShimmerCallBack>() ?? Array.Empty<CanShimmerCallBack>()) {
			if (!callBack.Invoke(this, shimmerable))
				return false;
		}
		return true;
	}

	/// <summary> Called by <see cref="ShimmerTransformation{TShimmeredType}.TryModShimmer(TShimmeredType)"/> once it finds a valid transformation </summary>
	public void DoModShimmer(IModShimmerable source)
	{
		int usableStack = GetUsableStack(source);
		IEnumerable<IModShimmerable> spawned = SpawnModShimmerResults(source, usableStack); // Spawn results, output stack amount used
		source.ShimmerRemoveStacked(usableStack); // Removed amount used
		OnShimmerCallBacks?.Invoke(this, source, spawned);

		ShimmerEffect(source.Center);
	}

	/// <summary>
	/// Spawns every item in <see cref="Results"/> <paramref name="stackUsed"/> times for this transformation using <paramref name="source"/>
	/// </summary>
	public IEnumerable<IModShimmerable> SpawnModShimmerResults(IModShimmerable source, int stackUsed)
	{
		foreach (IModShimmerable shimmerable in Results.SelectMany(result => result.SpawnFrom(source, stackUsed)).Where(shimmerable => shimmerable != null)) { //Spawns the individual result, adds it to the list
			if (!AllowChainedShimmers)
				shimmerable.PreventingChainedShimmers = true;

			if (shimmerable is Entity entity) {
				entity.shimmerWet = true;
				entity.wet = true;
			}

			yield return shimmerable;
		}
	}

	#endregion Shimmering

	#region Helpers

	/// <summary> Creates the shimmer effect checking either single player or server </summary>
	/// <param name="position"> The position to create the effect </param>
	public static void ShimmerEffect(Vector2 position)
	{
		switch (Main.netMode) {
			case NetmodeID.SinglePlayer:
				Item.ShimmerEffect(position);
				break;

			case NetmodeID.Server:
				NetMessage.SendData(MessageID.ShimmerActions, -1, -1, null, 0, (int)position.X, (int)position.Y);
				break;

			default:
				break;
		}
	}

	/// <inheritdoc cref="Clone"/>
	object ICloneable.Clone() => Clone();

	/// <summary> Creates a deep clone of <see cref="ShimmerTransformation"/>. </summary>
	public abstract ShimmerTransformation Clone();

	/// <summary> Gets the number of <typeparamref name="TResultType"/> in this transformation by <see cref="ModShimmerResult.Count"/> </summary>
	public int GetSpawnCount<TResultType>() where TResultType : ModShimmerResult
		=> Results.Sum((ModShimmerResult result) => result is TResultType ? result.Count : 0);

	/// <summary> Gets the total number of spawns in this transformation by <see cref="ModShimmerResult.Count"/> </summary>
	public int GetSpawnCount()
		=> Results.Sum((ModShimmerResult result) => result.Count);

	public const int SingleShimmerNPCSpawnCap = 50;

	private static int GetCurrentAvailableNPCSlots() => NPC.GetAvailableAmountOfNPCsToSpawnUpToSlot(SingleShimmerNPCSpawnCap, 200);

	/// <summary> Gets the current amount of this <see cref="IModShimmerable"/> of <see cref="IModShimmerable.Stack"/> that can currently be used for shimmer </summary>
	public int GetUsableStack(IModShimmerable source)
	{
		// 200 and 50 are the values vanilla uses for the highest slot to count with and the maximum NPCs to spawn in one transformation set
		int npcSpawnCount = GetSpawnCount<NPCShimmerResult>();
		return npcSpawnCount != 0 ? Math.Min((int)MathF.Floor(GetCurrentAvailableNPCSlots() / (float)npcSpawnCount), source.Stack) : source.Stack;
	}

	#endregion Helpers
}

/// <inheritdoc/>
/// <typeparam name="TModShimmerable"> The type that will be shimmered from, mainly used it tandem with <see cref="Transformations"/> for type separation </typeparam>
public sealed class ShimmerTransformation<TModShimmerable> : ShimmerTransformation, IOrderable<ShimmerTransformation<TModShimmerable>> where TModShimmerable : IModShimmerable
{
	#region Management

	public static Dictionary<int, List<ShimmerTransformation<TModShimmerable>>> Transformations { get; } = new();

	public ShimmerTransformation()
	{
	}

	public ShimmerTransformation(TModShimmerable CreateFrom)
	{
		SourceType = CreateFrom.Type;
	}

	public static void Reset()
	{
		Transformations.Clear();
	}

	public static void Order()
	{
		foreach (int type in Transformations.Keys)
			Transformations[type] = Transformations[type].GetOrdered().ToList();
	}

	#endregion Management

	#region Variables

	public int? SourceType { get; init; }
	public (ShimmerTransformation<TModShimmerable> target, bool after) Ordering { get; private set; }

	#endregion Variables

	#region ControllerMethods

	public override ShimmerTransformation<TModShimmerable> AddCondition(Condition condition)
	{
		Conditions.Add(condition);
		return this;
	}

	#region AddResultMethods

	public override ShimmerTransformation<TModShimmerable> AddResult(ModShimmerResult result)
	{
		if (result.Count <= 0)
			throw new ArgumentException("A Count greater than 0 is required", nameof(result));

		Results.Add(result);
		return this;
	}

	public override ShimmerTransformation<TModShimmerable> AddModItemResult<T>(int stack = 1)
		=> AddItemResult(ModContent.ItemType<T>(), stack);

	public override ShimmerTransformation<TModShimmerable> AddItemResult(int type, int stack = 1)
		=> AddResult(new ItemShimmerResult(type, stack));

	public override ShimmerTransformation<TModShimmerable> AddModNPCResult<T>(int count = 1)
		=> AddNPCResult(ModContent.NPCType<T>(), count);

	public override ShimmerTransformation<TModShimmerable> AddNPCResult(int type, int count = 1)
		=> AddResult(new NPCShimmerResult(type, count));

	public override ShimmerTransformation<TModShimmerable> AddCoinLuckResult(int coinLuck)
		=> AddResult(new CoinLuckShimmerResult(coinLuck));

	#endregion AddResultMethods

	public override ShimmerTransformation<TModShimmerable> DisableVanillaItemConstraints()
	{
		IgnoreVanillaItemConstraints = true;
		return this;
	}

	public override ShimmerTransformation<TModShimmerable> DisableChainedShimmers()
	{
		AllowChainedShimmers = false;
		return this;
	}

	public override ShimmerTransformation<TModShimmerable> AddCanShimmerCallBack(CanShimmerCallBack callBack)
	{
		CanShimmerCallBacks += callBack;
		return this;
	}

	public override ShimmerTransformation<TModShimmerable> AddModifyShimmerCallBack(ModifyShimmerCallBack callBack)
	{
		ModifyShimmerCallBacks += callBack;
		return this;
	}

	public override ShimmerTransformation<TModShimmerable> AddOnShimmerCallBack(OnShimmerCallBack callBack)
	{
		OnShimmerCallBacks += callBack;
		return this;
	}

	#region Registering

	/// <inheritdoc/>
	public override void Register()
	{
		if (SourceType == null)
			throw new InvalidOperationException("The transformation must be created from an entity for the parameterless Register() to be used.");
		Register(SourceType.Value);
	}

	/// <inheritdoc/>
	public override void Register(params int[] types)
		=> Register(types.AsEnumerable());

	/// <inheritdoc/>
	public override void Register(IEnumerable<int> types)
	{
		foreach (int ID in types)
			Register(ID);
	}

	/// <inheritdoc/>
	public override void Register(int type)
	{
		if (Transformations.Count == 0)
			AddAsKnownType<TModShimmerable>();
		if (!Transformations.TryAdd(type, new() { this })) //Try add a new entry for the tuple
			Transformations[type].Add(this); // If it fails, entry exists, therefore add to list
	}

	#endregion Registering

	#endregion ControllerMethods

	#region Redirects

	public static Dictionary<int, int> Redirects { get; } = new();

	public static void AddRedirect(int typeFrom, int typeTo)
		=> Redirects.Add(typeFrom, typeTo);

	/// <summary>
	/// First <see cref="Redirects"/> is checked for an entry, if one exists it is applied over <paramref name="type"/>, then if this is
	/// <see cref="ShimmerTransformation"/>&lt; <see cref="Item"/>&gt;, it checks <see cref="ItemID.Sets.ShimmerCountsAsItem"/><br/>. Is not recursive
	/// </summary>
	public static int GetRedirectedType(int type)
	{
		if (Redirects.TryGetValue(type, out int value))
			type = value;
		if (typeof(TModShimmerable) == typeof(Item) && ItemID.Sets.ShimmerCountsAsItem[type] > 0)
			type = ItemID.Sets.ShimmerCountsAsItem[type];
		return type;
	}

	#endregion Redirects

	#region Shimmering

	public static IEnumerable<ShimmerTransformation<TModShimmerable>> GetTransformationsOrEmpty(int type)
		=> Transformations.GetValueOrDefault(type) ?? Array.Empty<ShimmerTransformation<TModShimmerable>>().AsEnumerable();

	/// <summary>
	/// Checks every <see cref="ShimmerTransformation"/> for this <typeparamref name="TModShimmerable"/> and returns true when if finds one that passes
	/// <see cref="ShimmerTransformation.CanModShimmer_Transformation(IModShimmerable)"/>. <br/> Does not check <see cref="IModShimmerable.CanShimmer"/>
	/// </summary>
	/// <returns> True if there is a mod transformation this <typeparamref name="TModShimmerable"/> could undergo </returns>
	public static bool AnyValidModShimmer(TModShimmerable source)
		=> GetTransformationsOrEmpty(source.ShimmerRedirectedType).Any(transformation => transformation.CanModShimmer_Transformation(source));

	/// <summary> Tries to complete a shimmer operation on the <typeparamref name="TModShimmerable"/> passed, should not be called on multiplayer clients </summary>
	/// <param name="source"> The <typeparamref name="TModShimmerable"/> to be shimmered </param>
	/// <returns> True if the transformation is successful, false if it is should fall through to vanilla as normal </returns>
	public static bool TryModShimmer(TModShimmerable source)
	{
		foreach (ShimmerTransformation transformation in GetTransformationsOrEmpty(source.ShimmerRedirectedType)) { // Loops possible transformations
			if (transformation.CanModShimmer(source)) { // Checks conditions and callback in CanShimmer
				ShimmerTransformation copy = transformation.Clone(); // Make a copy
				copy.ModifyShimmerCallBacks?.Invoke(copy, source); // As to not be effected by any changes made here
				copy.DoModShimmer(source);
				return true;
			}
		}
		return false;
	}

	#endregion Shimmering

	#region Ordering

	/// <summary> Sets the Ordering of this <see cref="ShimmerTransformation"/>. This <see cref="ShimmerTransformation"/> can't already have one. </summary>
	private ShimmerTransformation<TModShimmerable> SetOrdering(ShimmerTransformation<TModShimmerable> transformation, bool after, int forType)
	{
		if (Ordering.target != null)
			throw new Exception("This transformation already has an ordering.");
		if (!Transformations[forType].Contains(transformation))
			throw new ArgumentException("This passed transformation must be registered.", nameof(transformation));

		Ordering = (transformation, after);
		ShimmerTransformation<TModShimmerable> target = transformation;
		do {
			if (target == this)
				throw new Exception("Shimmer ordering loop!");
			target = target.Ordering.target;
		} while (target != null);

		return this;
	}

	public ShimmerTransformation<TModShimmerable> SortBefore(ShimmerTransformation<TModShimmerable> transformation, int forType) => SetOrdering(transformation, false, forType);

	public override ShimmerTransformation<TModShimmerable> SortBefore(ShimmerTransformation transformation, int forType)
	{
		if (transformation is ShimmerTransformation<TModShimmerable> typedTransformation)
			return SortBefore(typedTransformation, forType);
		throw new ArgumentException("Passed transformation must be the same type as self.", nameof(transformation));
	}

	public ShimmerTransformation<TModShimmerable> SortAfter(ShimmerTransformation<TModShimmerable> transformation, int forType) => SetOrdering(transformation, true, forType);

	public override ShimmerTransformation<TModShimmerable> SortAfter(ShimmerTransformation transformation, int forType)
	{
		if (transformation is ShimmerTransformation<TModShimmerable> typedTransformation)
			return SortBefore(typedTransformation, forType);
		throw new ArgumentException("Passed transformation must be the same type as self.", nameof(transformation));
	}

	#endregion Ordering

	#region Helpers

	public override ShimmerTransformation<TModShimmerable> Clone()
		=> new() {
			Disabled = Disabled,
			Ordering = Ordering,
			AllowChainedShimmers = AllowChainedShimmers,
			SourceType = SourceType,
			IgnoreVanillaItemConstraints = IgnoreVanillaItemConstraints,
			Conditions = new List<Condition>(Conditions), // Technically I think the localization for the conditions can be changed
			Results = new List<ModShimmerResult>(Results), // List is new, ModShimmerResult is a readonly struct
			CanShimmerCallBacks = (CanShimmerCallBack)CanShimmerCallBacks?.Clone(), // Stored values are immutable
			ModifyShimmerCallBacks = (ModifyShimmerCallBack)ModifyShimmerCallBacks?.Clone(),
			OnShimmerCallBacks = (OnShimmerCallBack)OnShimmerCallBacks?.Clone(),
		};

	// These return an IEnumerable to make clear the list should not be edited here.

	private static Dictionary<int, IEnumerable<ShimmerTransformation<TModShimmerable>>> FilterInnerEmpty(IEnumerable<KeyValuePair<int, IEnumerable<ShimmerTransformation<TModShimmerable>>>> keyValuePairs)
		=> new(keyValuePairs.Where(pair => pair.Value?.Count() > 0));

	/// <summary> Gets every entry that isn't disabled </summary>
	public static Dictionary<int, IEnumerable<ShimmerTransformation<TModShimmerable>>> GetAllTransformationsNotDisabled()
		=> FilterInnerEmpty(Transformations.Select(keyValuePair
			=> new KeyValuePair<int, IEnumerable<ShimmerTransformation<TModShimmerable>>>(keyValuePair.Key, keyValuePair.Value.Where(transformation
				=> !transformation.Disabled))));

	/// <summary> Gets every entry that isn't disabled and where every result is of type <typeparamref name="TModShimmerResult"/> </summary>
	public static Dictionary<int, IEnumerable<ShimmerTransformation<TModShimmerable>>> GetAllTransformationsWithOnly<TModShimmerResult>() where TModShimmerResult : ModShimmerResult
		=> FilterInnerEmpty(Transformations.Select(keyValuePair
			=> new KeyValuePair<int, IEnumerable<ShimmerTransformation<TModShimmerable>>>(keyValuePair.Key, keyValuePair.Value.Where(transformation
				=> !transformation.Disabled && transformation.Results.All(result // This transformation is not disabled, and for every result
					=> result is TModShimmerResult))))); // The result is of the TModShimmerResult passed

	/// <summary> Gets every entry that isn't disabled and where there is any result of type <typeparamref name="TModShimmerResult"/> </summary>
	public static Dictionary<int, IEnumerable<ShimmerTransformation<TModShimmerable>>> GetAllTransformationsWithAny<TModShimmerResult>() where TModShimmerResult : ModShimmerResult
		=> FilterInnerEmpty(Transformations.Select(keyValuePair
			=> new KeyValuePair<int, IEnumerable<ShimmerTransformation<TModShimmerable>>>(keyValuePair.Key, keyValuePair.Value.Where(transformation
				=> !transformation.Disabled && transformation.Results.Any(result // This transformation is not disabled, and for any result
					=> result is TModShimmerResult))))); // The result is of the TModShimmerResult passed

	#endregion Helpers
}

/// <summary>
/// Marks a class to be used with <see cref="ShimmerTransformation"/> as a type, most implementations for <see cref="NPC"/> and <see cref="Item"/> wrap normal values,
/// <see cref="ShimmerTransformation{T}.TryModShimmer(T)"/> must be called manually for implementing types
/// </summary>
public interface IModShimmerable
{
	/// <inheritdoc cref="Entity.Center"/>
	public abstract Vector2 Center { get; set; }

	/// <inheritdoc cref="Entity.Hitbox"/>
	public abstract Rectangle Hitbox { get; set; }

	/// <summary> Wraps <see cref="Entity.velocity"/> </summary>
	public abstract Vector2 Velocity { get; set; }

	/// <summary> <see cref="Type"/> passed through <see cref="ShimmerTransformation{TModShimmerable}.GetRedirectedType(int)"/> </summary>
	public abstract int ShimmerRedirectedType { get; }

	public abstract int Type { get; }

	/// <summary>
	/// When this undergoes a <see cref="ShimmerTransformation"/> this is the amount contained within one instance of the type, returns 1 for <see cref="NPC"/> and
	/// <see cref="Projectile"/>, <see cref="Item.stack"/> for <see cref="Item"/><br/>
	/// </summary>
	public virtual int Stack => 1;

	/// <summary> Used by <see cref="ShimmerTransformation.AllowChainedShimmers"/> to check if this instance has left shimmer long enough to go again </summary>
	public abstract bool PreventingChainedShimmers { get; set; }

	/// <summary>
	/// Checks if this <see cref="IModShimmerable"/> can currently undergo <b> any shimmer transformation. </b> This includes both <b> Vanilla and modded </b><br/> Should
	/// not alter game state / treat as read only
	/// </summary>
	/// <returns> True if the <see cref="IModShimmerable"/> currently has a valid shimmer operation it can use <b> Vanilla or modded </b>. </returns>
	public virtual bool CanShimmer() => true;

	/// <summary> Called at the end of shimmer </summary>
	public virtual void OnShimmer() { }

	/// <summary>
	/// Called once an entity Shimmers, for <see cref="Item"/> decrements <see cref="Item.stack"/> and handles despawning when <see cref="Stack"/> reaches zero For
	/// <see cref="NPC"/> and <see cref="Projectile"/> always despawns regardless of passed value
	/// </summary>
	public abstract void ShimmerRemoveStacked(int amount);

	public abstract IEntitySource GetSource_Misc(string context);
}