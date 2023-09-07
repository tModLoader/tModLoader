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
public abstract class ShimmerTransformation : ICloneable, IOrderable<ShimmerTransformation>
{
	private static Action extraKnownTypeResets;
	private static Action extraKnownTypeOrders;
	public static void AddAsKnownType<TModShimmerable>() where TModShimmerable : IModShimmerable
	{
		extraKnownTypeResets += ShimmerTransformation<TModShimmerable>.Reset;
		extraKnownTypeOrders += ShimmerTransformation<TModShimmerable>.Order;
	}
	internal static void ResetKnown()
	{
		ShimmerTransformation<NPC>.Reset();
		ShimmerTransformation<Item>.Reset();
		extraKnownTypeResets.Invoke();
		extraKnownTypeResets = null;
	}

	internal static void OrderKnown()
	{
		ShimmerTransformation<NPC>.Order();
		ShimmerTransformation<Item>.Order();
		extraKnownTypeOrders.Invoke();
	}
	private protected abstract ShimmerTransformation SetOrdering(ShimmerTransformation transformation, bool after);
	public abstract ShimmerTransformation SortBefore(ShimmerTransformation transformation);
	public abstract ShimmerTransformation SortAfter(ShimmerTransformation transformation);
	public void Disable()
		=> Disabled = true;

	public bool Disabled { get; set; }

	#region FunctionalityVariables

	public (ShimmerTransformation target, bool after) Ordering { get; set; }

	/// <summary> Every condition must be true for the transformation to occur </summary>
	public List<Condition> Conditions { get; init; } = new();

	/// <summary> The results that the transformation produces. </summary>
	public List<ModShimmerResult> Results { get; init; } = new();

	/// <summary> Vanilla disallows a transformation if the result includes either a bone or a lihzahrd brick when skeletron or golem are undefeated respectively </summary>
	public bool IgnoreVanillaItemConstraints { get; private protected set; }

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
	public delegate void OnShimmerCallBack(ShimmerTransformation transformation, IModShimmerable source, List<IModShimmerable> spawnedEntities);

	/// <inheritdoc cref="OnShimmerCallBack"/>
	public OnShimmerCallBack OnShimmerCallBacks { get; private protected set; }

	#endregion FunctionalityVariables

	#region ControllerMethods

	/// <summary> Adds a condition to <see cref="Conditions"/>. <inheritdoc cref="Conditions"/> </summary>
	/// <param name="condition"> The condition to be added </param>
	public ShimmerTransformation AddCondition(Condition condition)
	{
		Conditions.Add(condition);
		return this;
	}

	#region AddResultMethods

	/// <summary> Adds a result to <see cref="Results"/>, this will be spawned when the <see cref="IModShimmerable"/> successfully shimmers </summary>
	/// <param name="result"> The result to be added </param>
	/// <exception cref="ArgumentException"> thrown when <paramref name="result"/> has a <see cref="ModShimmerResult.Count"/> that is less than or equal to zero </exception>
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

	/// <summary> Adds a delegate to <see cref="CanShimmerCallBacks"/> that will be called if the shimmer transformation succeeds </summary>
	public ShimmerTransformation AddCanShimmerCallBack(CanShimmerCallBack callBack)
	{
		CanShimmerCallBacks += callBack;
		return this;
	}

	/// <summary> Adds a delegate to <see cref="ModifyShimmerCallBacks"/> that will be called before the transformation </summary>
	public ShimmerTransformation AddModifyShimmerCallBack(ModifyShimmerCallBack callBack)
	{
		ModifyShimmerCallBacks += callBack;
		return this;
	}

	/// <summary> Adds a delegate to <see cref="OnShimmerCallBacks"/> that will be called if the shimmer transformation succeeds </summary>
	public ShimmerTransformation AddOnShimmerCallBack(OnShimmerCallBack callBack)
	{
		OnShimmerCallBacks += callBack;
		return this;
	}

	/// <inheritdoc cref="Register(int)"/>
	/// <exception cref="InvalidOperationException"> Thrown if this instance was not created from an Entity </exception>
	public abstract void Register();

	/// <inheritdoc cref="Register(int)"/>
	public abstract void Register(IEnumerable<int> types);

	/// <summary> Finalizes transformation </summary>
	public abstract void Register(int type);

	#endregion ControllerMethods

	#region Shimmering

	/// <summary> Checks if the <see cref="IModShimmerable"/> supplied can undergo a shimmer transformation, should not alter game state / read only </summary>
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

	/// <summary> Checks the conditions for this transformation </summary>
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
	/// None of the results contain bone or lihzahrd brick while skeletron or golem are undefeated if <see cref="IgnoreVanillaItemConstraints"/> is false (default)
	/// <item/>
	/// The amount of empty NPC slots under slot 200 is less than the number of NPCs this transformation spawns
	/// </list>
	/// </returns>
	public bool CanModShimmer_Transformation(IModShimmerable shimmerable)
		=> !Disabled
		&& Conditions.All((condition) => condition.IsMet())
		&& (CheckCanShimmerCallBacks(shimmerable))
		&& (IgnoreVanillaItemConstraints || !Results.Any((result) => result is ItemShimmerResult item && (item.Type == ItemID.Bone && !NPC.downedBoss3 || item.Type == ItemID.LihzahrdBrick && !NPC.downedGolemBoss)))
		&& (GetCurrentAvailableNPCSlots() >= GetSpawnCount<NPCShimmerResult>());

	/// <summary> Checks all <see cref="CanShimmerCallBacks"/> for <paramref name="shimmerable"/> </summary>
	/// <returns> Returns true if all delegates in <see cref="CanShimmerCallBacks"/> return true </returns>
	public bool CheckCanShimmerCallBacks(IModShimmerable shimmerable)
	{
		foreach (CanShimmerCallBack callBack in CanShimmerCallBacks?.GetInvocationList()?.Cast<CanShimmerCallBack>() ?? Array.Empty<CanShimmerCallBack>()) {
			if (!callBack.Invoke(this, shimmerable))
				return false;
		}
		return true;
	}

	public const int SingleShimmerNPCSpawnCap = 50;

	public int GetSpawnCount<TResultType>()
		=> Results.Sum((ModShimmerResult result) => result is TResultType ? result.Count : 0);

	private static int GetCurrentAvailableNPCSlots() => NPC.GetAvailableAmountOfNPCsToSpawnUpToSlot(SingleShimmerNPCSpawnCap, 200);

	/// <summary> Called by <see cref="ShimmerTransformation{TShimmeredType}.TryModShimmer(TShimmeredType)"/> once it finds a valid transformation </summary>
	public static void DoModShimmer(IModShimmerable source, ShimmerTransformation transformation)
	{
		// 200 and 50 are the values vanilla uses for the highest slot to count with and the maximum NPCs to spawn in one transformation set
		int npcSpawnCount = transformation.GetSpawnCount<NPCShimmerResult>();
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

	/// <summary> Creates the shimmer effect checking either single player or server </summary>
	/// <param name="position"> The position to create the effect </param>
	public static void ShimmerEffect(Vector2 position)
	{
		if (Main.netMode == NetmodeID.SinglePlayer)
			Item.ShimmerEffect(position);
		else if (Main.netMode == NetmodeID.Server)
			NetMessage.SendData(MessageID.ShimmerActions, -1, -1, null, 0, (int)position.X, (int)position.Y);
	}

	/// <inheritdoc cref="Clone"/>
	object ICloneable.Clone() => Clone();

	/// <summary> Creates a deep clone of <see cref="ShimmerTransformation"/>. </summary>
	public abstract ShimmerTransformation Clone();

	#endregion Shimmering
}

public sealed class ShimmerTransformation<TModShimmerable> : ShimmerTransformation where TModShimmerable : IModShimmerable
{
	public int? SourceType { get; init; }

	public ShimmerTransformation()
	{
	}

	public ShimmerTransformation(TModShimmerable CreateFrom)
	{
		SourceType = CreateFrom.Type;
	}

	public static Dictionary<int, List<ShimmerTransformation>> Transformations { get; } = new();

	public static void Reset()
	{
		Transformations.Clear();
	}

	public static void Order()
	{
		foreach (int type in Transformations.Keys)
			Transformations[type] = Transformations[type].GetOrdered().ToList();
	}

	public override ShimmerTransformation<TModShimmerable> Clone()
		=> new() {
			Conditions = new List<Condition>(Conditions), // Technically I think the localization for the conditions can be changed
			Results = new List<ModShimmerResult>(Results), // List is new, ModShimmerResult is a readonly struct
			IgnoreVanillaItemConstraints = IgnoreVanillaItemConstraints, // Assigns by value
			CanShimmerCallBacks = (CanShimmerCallBack)CanShimmerCallBacks?.Clone(), // Stored values are immutable
			ModifyShimmerCallBacks = (ModifyShimmerCallBack)ModifyShimmerCallBacks?.Clone(),
			OnShimmerCallBacks = (OnShimmerCallBack)OnShimmerCallBacks?.Clone(),
		};

	/// <inheritdoc/>
	public override void Register()
	{
		if (SourceType == null)
			throw new InvalidOperationException("The transformation must be created from an entity for the parameterless Register() to be used.");
		Register(SourceType.Value);
	}

	/// <inheritdoc/>
	public override void Register(IEnumerable<int> types)
	{
		foreach (int ID in types)
			Register(ID);
	}

	/// <inheritdoc/>
	public override void Register(int type)
	{
		if (!Transformations.TryAdd(type, new() { this })) //Try add a new entry for the tuple
			Transformations[type].Add(this); // If it fails, entry exists, therefore add to list

		Transformations[type].Sort(); //TODO: with orderafter stuff
	}

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
		if (default(TModShimmerable) is Item && ItemID.Sets.ShimmerCountsAsItem[type] > 0)
			type = ItemID.Sets.ShimmerCountsAsItem[type];
		return type;
	}

	#endregion Redirects

	/// <summary>
	/// Checks every <see cref="ShimmerTransformation"/> for this <see cref="IModShimmerable"/> and returns true when if finds one that passes
	/// <see cref="ShimmerTransformation.CanModShimmer_Transformation(IModShimmerable)"/>. <br/> Does not check <see cref="IModShimmerable.CanShimmer"/>
	/// </summary>
	/// <returns> True if there is a mod transformation this <see cref="IModShimmerable"/> could undergo </returns>
	public static bool AnyValidModShimmer(TModShimmerable source)
	{
		if (!Transformations.ContainsKey(source.Type))
			return false;

		foreach (ShimmerTransformation modShimmer in Transformations[source.Type]) {
			if (modShimmer.CanModShimmer_Transformation(source))
				return true;
		}

		return false;
	}

	/// <summary> Tries to complete a shimmer operation on the <see cref="IModShimmerable"/> passed, should not be called on multiplayer clients </summary>
	/// <param name="source"> The <see cref="IModShimmerable"/> to be shimmered </param>
	/// <returns> True if the transformation is successful, false if it is should fall through to vanilla as normal </returns>
	public static bool TryModShimmer(TModShimmerable source)
	{
		List<ShimmerTransformation> transformations = Transformations.GetValueOrDefault(source.Type);
		if (!(transformations?.Count > 0)) // Inverse to catch null
			return false;

		foreach (ShimmerTransformation transformation in transformations) { // Loops possible transformations
			if (transformation.Results.Count > 0 && transformation.CanModShimmer(source)) { // Checks conditions and callback in CanShimmer
				ShimmerTransformation copy = transformation.Clone(); // Make a copy
				copy.ModifyShimmerCallBacks?.Invoke(copy, source); // As to not be effected by any changes made here
				DoModShimmer(source, copy);
				return true;
			}
		}
		return false;
	}

	#region Ordering

	/// <summary>
	/// Sets the Ordering of this <see cref="ShimmerTransformation"/>. This <see cref="ShimmerTransformation"/> can't already have one.
	/// </summary>
	private protected override ShimmerTransformation SetOrdering(ShimmerTransformation transformation, bool after)
	{
		if (Ordering.target != null)
			throw new Exception("This transformation already has an ordering.");

		Ordering = (transformation, after);
		ShimmerTransformation target = transformation;
		do
		{
			if (target == this)
				throw new Exception("Shimmer ordering loop!");

			target = target.Ordering.target;
		} while (target != null);


		return this;
	}

	/// <summary>
	/// Sorts the <see cref="ShimmerTransformation"/> before the one given as parameter. Both <see cref="ShimmerTransformation"/> must already be registered.
	/// </summary>
	public override ShimmerTransformation SortBefore(ShimmerTransformation recipe) => SetOrdering(recipe, false);

	/// <summary>
	/// Sorts the <see cref="ShimmerTransformation"/> after the one given as parameter. Both <see cref="ShimmerTransformation"/> must already be registered.
	/// </summary>
	public override ShimmerTransformation SortAfter(ShimmerTransformation recipe) => SetOrdering(recipe, true);

	#endregion
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

	public abstract int Type { get; }

	/// <summary>
	/// When this undergoes a <see cref="ShimmerTransformation"/> this is the amount contained within one instance of the type, returns 1 for <see cref="NPC"/>, and
	/// <see cref="Item.stack"/> for <see cref="Item"/><br/>
	/// </summary>
	public virtual int Stack => 1;

	/// <summary>
	/// Checks if this <see cref="IModShimmerable"/> can currently undergo a shimmer transformation. This includes both vanilla and modded <br/> Should not makes changes to game
	/// state. <br/> Treat as read only.
	/// </summary>
	/// <returns> True if the <see cref="IModShimmerable"/> currently has a valid shimmer operation it can use. </returns>
	public virtual bool CanShimmer() => true;

	/// <summary> Called at the end of shimmer </summary>
	public virtual void OnShimmer() { }

	/// <summary>
	/// Called once an entity Shimmers, for <see cref="Item"/> decrements <see cref="Item.stack"/>, handles despawning when <see cref="Stack"/> reaches zero
	/// </summary>
	public abstract void Remove(int amount);

	/// <summary>
	/// Returns <see cref="Entity.GetSource_Misc(string)"/> with passed value "shimmer" in <see cref="NPC"/> and <see cref="Item"/>, used only for <see cref="ShimmerTransformation"/>
	/// </summary>
	public abstract IEntitySource GetSource_ForShimmer();
}