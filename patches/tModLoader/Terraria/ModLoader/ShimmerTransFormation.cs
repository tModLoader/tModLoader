using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.Utilities;

namespace Terraria.ModLoader;
// TML: #AdvancedShimmerTransformations
public static class ShimmerManager<TShimmerable> where TShimmerable : IModShimmerable
{

	/// <summary> Tries to complete a shimmer operation on the <typeparamref name="TShimmerable"/> passed, should not be called on multiplayer clients </summary>
	/// <param name="source"> The <typeparamref name="TShimmerable"/> to be shimmered </param>
	/// <returns> True if the transformation is successful, false if it is should fall through to vanilla as normal </returns>
	public static bool TryModShimmer(TShimmerable source)
	{
		foreach (IShimmerTransformation<TShimmerable> transformation in GetValidTransformations(source.ShimmerRedirectedType)) { // Loops possible transformations
			if (transformation.TryShimmer(source)) {
				return true;
			}
		}
		return false;
	}
	public static Dictionary<int, List<IShimmerTransformation<TShimmerable>>> Transformations { get; } = new();

	public static void AddTransformation(int type, IShimmerTransformation<TShimmerable> transformation)
	{
		if (Transformations.Count == 0)
			ShimmerManager.AddAsKnownType<TShimmerable>();
		if (!Transformations.TryAdd(type, new() { transformation })) //Try add a new entry for the tuple
			Transformations[type].Add(transformation); // If it fails, entry exists, therefore add to list
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

	#region Helpers

	public static IEnumerable<IShimmerTransformation<TShimmerable>> GetValidTransformations(int type)
		=> (Transformations.GetValueOrDefault(type) ?? Array.Empty<IShimmerTransformation<TShimmerable>>().AsEnumerable()).Where(transformation => !transformation.Disabled);



	// These return an IEnumerable to make clear the list should not be edited here.
	private static Dictionary<int, IEnumerable<IShimmerTransformation<TShimmerable>>> CreateNewFilteredDictionary(IEnumerable<KeyValuePair<int, IEnumerable<IShimmerTransformation<TShimmerable>>>> keyValuePairs)
		=> new(keyValuePairs.Where(pair => pair.Value?.Count() > 0));

	/// <summary> Gets every entry that isn't disabled </summary>
	public static Dictionary<int, IEnumerable<IShimmerTransformation<TShimmerable>>> GetAllTransformationsNotDisabled()
		=> CreateNewFilteredDictionary(Transformations.Select(keyValuePair
			=> new KeyValuePair<int, IEnumerable<IShimmerTransformation<TShimmerable>>>(keyValuePair.Key, keyValuePair.Value.Where(transformation
				=> !transformation.Disabled))));

	/// <summary> Gets every entry that isn't disabled and where every result is of type <typeparamref name="TModShimmerResult"/> </summary>
	public static Dictionary<int, IEnumerable<IShimmerTransformation<TShimmerable>>> GetAllTransformationsWithOnly<TModShimmerResult>() where TModShimmerResult : GeneralShimmerResult
		=> CreateNewFilteredDictionary(Transformations.Select(keyValuePair
			=> new KeyValuePair<int, IEnumerable<IShimmerTransformation<TShimmerable>>>(keyValuePair.Key, keyValuePair.Value.Where(transformation
				=> !transformation.Disabled && transformation.Results.All(result // This transformation is not disabled, and for every result
					=> result is TModShimmerResult))))); // The result is of the TModShimmerResult passed

	/// <summary> Gets every entry that isn't disabled and where there is any result of type <typeparamref name="TModShimmerResult"/> </summary>
	public static Dictionary<int, IEnumerable<IShimmerTransformation<TShimmerable>>> GetAllTransformationsWithAny<TModShimmerResult>() where TModShimmerResult : GeneralShimmerResult
		=> CreateNewFilteredDictionary(Transformations.Select(keyValuePair
			=> new KeyValuePair<int, IEnumerable<IShimmerTransformation<TShimmerable>>>(keyValuePair.Key, keyValuePair.Value.Where(transformation
				=> !transformation.Disabled && transformation.Results.Any(result // This transformation is not disabled, and for any result
					=> result is TModShimmerResult))))); // The result is of the TModShimmerResult passed

	#endregion Helpers

}
public static class ShimmerManager
{
	#region Management

	/// <summary> See <see cref="AddAsKnownType{TShimmerable}"/> </summary>
	private static Action extraKnownTypeResets;

	/// <summary> See <see cref="AddAsKnownType{TShimmerable}"/> </summary>
	private static Action extraKnownTypeOrders;

	/// <summary> Called on the first item added to the dictionary for a given type </summary>
	internal static void AddAsKnownType<TShimmerable>() where TShimmerable : IModShimmerable
	{
		extraKnownTypeResets += ShimmerManager<TShimmerable>.Reset;
		extraKnownTypeOrders += ShimmerManager<TShimmerable>.Order;
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

	#endregion Management

	#region Helpers

	/// <summary> Added to the the velocity of the <see cref="IModShimmerable"/> to prevent stacking </summary>
	public static Vector2 GetShimmerSpawnVelocityModifier()
		=> new Vector2(Main.rand.Next(-30, 31), Main.rand.Next(-40, -15)) * 0.1f;

	// => new(count * (1f + count * 0.05f) * ((count % 2 == 0) ? -1 : 1), 0);
	// What vanilla does for items with more than one ingredient, flings stuff everywhere as it's never supposed to do more than 15
	//So we're using the random spawn values from shimmered items instead, items push each other away when in the shimmer state anyway, so this is more for NPCs

	public const int SingleShimmerNPCSpawnCap = 50;

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

	internal static int GetCurrentAvailableNPCSlots()
		=> NPC.GetAvailableAmountOfNPCsToSpawnUpToSlot(SingleShimmerNPCSpawnCap, 200);

	#endregion Helpers
}

public interface IShimmerTransformationWriter<out TShimmerable> where TShimmerable : IModShimmerable
{
	public void AddResult(IShimmerResult<TShimmerable> result);
}

public interface IShimmerTransformation<in TShimmerable> : IOrderable<IShimmerTransformation<TShimmerable>> where TShimmerable : IModShimmerable
{
	public bool Disabled { get; set; }

	public void Disable()
		=> Disabled = true;
	public IEnumerable<IShimmerResult<TShimmerable>> Results { get; }
	public bool CanShimmer(TShimmerable shimmerable);
	public bool CanShimmer_Transformation(TShimmerable shimmerable);

	public void Shimmer(TShimmerable shimmerable);

	public bool TryShimmer(TShimmerable shimmerable)
	{
		if (CanShimmer(shimmerable)) {
			Shimmer(shimmerable);
			return true;
		}
		else
			return false;
	}
}

public abstract record class VanillaTransformation<TShimmerable>(int Type) : IShimmerResult<TShimmerable>, IShimmerTransformation<TShimmerable> where TShimmerable : IModShimmerable
{
	public IShimmerTransformation<TShimmerable> Target { get; set; }
	public bool After { get; set; }
	public bool Disabled { get; set; }
	public IEnumerable<IShimmerResult<TShimmerable>> Results {
		get {
			yield return this;
		}
	}

	public bool HandlesCleanup => true;
	public int Count => 1;

	public virtual bool CanShimmer(TShimmerable shimmerable) => true;
	public bool CanShimmer_Transformation(TShimmerable shimmerable) => throw new NotImplementedException();
	public virtual void Shimmer(TShimmerable shimmerable) => SpawnFrom(shimmerable, new(1));
	public abstract IEnumerable<IModShimmerable> SpawnFrom(TShimmerable shimmerable, ShimmerInfo shimmerInfo);
}


public record class VanillaItemTransformation(int Type) : VanillaTransformation<Item>(Type)
{
	public override IEnumerable<IModShimmerable> SpawnFrom(Item shimmerable, ShimmerInfo shimmerInfo)
	{
		// Identical to vanilla behaviour for ItemID.Sets.ShimmerTransformToItem
		int num8 = shimmerable.stack;
		shimmerable.SetDefaults(Type);
		shimmerable.stack = num8;
		shimmerable.shimmered = true;
		shimmerable.shimmerTime = 1f;
		shimmerable.shimmerWet = true;
		shimmerable.wet = true;
		shimmerable.velocity *= 0.1f;

		if (Main.netMode != 0) {
			NetMessage.SendData(146, -1, -1, null, 0, (int)shimmerable.Center.X, (int)shimmerable.Center.Y);
			NetMessage.SendData(145, -1, -1, null, shimmerable.whoAmI, 1f);
		}
		yield return shimmerable;
	}
}


/// <summary>
/// Represents the behavior and output of a shimmer transformation, the <see cref="IModShimmerable"/>(s) that can use it are stored via
/// <see cref="ShimmerTransformation{TShimmeredType}.Transformations"/> which is updated via <see cref="Register()"/> and its overloads. Uses a similar syntax to
/// <see cref="Recipe"/>, usually starting with <see cref="ModNPC.CreateShimmerTransformation"/> or <see cref="ModItem.CreateShimmerTransformation"/>
/// </summary>
/// <typeparam name="TShimmerable"> The type that will be shimmered from, mainly used it tandem with <see cref="Transformations"/> for type separation </typeparam>
public class ShimmerTransformation<TShimmerable> : IShimmerTransformation<TShimmerable>, ICloneable where TShimmerable : IModShimmerable
{
	#region Management

	/// <inheritdoc cref="Disabled"/>
	public void Disable()
		=> Disabled = true;

	#endregion Management

	#region Variables

	/// <summary> Prevents the transformation from being used. </summary>
	public bool Disabled { get; set; }

	public IShimmerTransformation<TShimmerable> Target { get; internal set; }

	public bool After { get; internal set; }

	IEnumerable<IShimmerResult<TShimmerable>> IShimmerTransformation<TShimmerable>.Results => Results;

	/// <summary> The results that the transformation produces. </summary>
	public List<IShimmerResult<TShimmerable>> Results { get; private set; } = new();

	/// <summary> Every condition must be true for the transformation to occur </summary>
	public List<Condition> Conditions { get; init; } = new();

	/// <summary> Vanilla disallows a transformation if the result includes either a bone or a lihzahrd brick when skeletron or golem are undefeated respectively </summary>
	public bool IgnoreVanillaItemConstraints { get; internal set; }

	/// <summary>
	/// In Vanilla if an entity shimmers, if the new entity has a transformation then it will transform immediately on the next frame, setting this means that the new
	/// entity will have to leave shimmer for a period of time before transforming again.
	/// </summary>
	public bool AllowChainedShimmers { get; internal set; } = true;

	/// <summary> Called in addition to conditions to check if the <see cref="IModShimmerable"/> shimmers </summary>
	/// <param name="transformation"> The transformation </param>
	/// <param name="source"> The <see cref="IModShimmerable"/> to be shimmered </param>
	public delegate bool CanShimmerCallBack(ShimmerTransformation<TShimmerable> transformation, TShimmerable source);

	/// <inheritdoc cref="CanShimmerCallBack"/>
	public CanShimmerCallBack CanShimmerCallBacks { get; internal set; }

	/// <summary> Called once before a transformation, use this to edit the transformation or type beforehand </summary>
	/// <param name="transformation"> The transformation, editing this does not change the stored transformation, only this time </param>
	/// <param name="source"> The <see cref="IModShimmerable"/> to be shimmered </param>
	public delegate void ModifyShimmerCallBack(ShimmerTransformation<TShimmerable> transformation, TShimmerable source);

	/// <inheritdoc cref="ModifyShimmerCallBack"/>
	public ModifyShimmerCallBack ModifyShimmerCallBacks { get; internal set; }

	/// <summary> Called after <see cref="IModShimmerable"/> shimmers </summary>
	/// <param name="transformation"> The transformation </param>
	/// <param name="spawnedEntities"> A list of the spawned Entities </param>
	/// <param name="source"> The <see cref="IModShimmerable"/> that was shimmered </param>
	public delegate void OnShimmerCallBack(ShimmerTransformation<TShimmerable> transformation, TShimmerable source, IEnumerable<IModShimmerable> spawnedEntities);

	/// <inheritdoc cref="OnShimmerCallBack"/>
	public OnShimmerCallBack OnShimmerCallBacks { get; internal set; }

	#endregion Variables

	#region Shimmering

	/// <summary>
	/// Full checks to see if the <see cref="IModShimmerable"/> can undergo <b> this transformation. </b><br/> calls
	/// <see cref="CanShimmer_Transformation(TShimmerable)"/> and <see cref="IModShimmerable.CanShimmer"/>. <br/> Should not alter game state / treat as read only
	/// </summary>
	/// <param name="shimmerable"> The <see cref="IModShimmerable"/> being shimmered </param>
	/// <returns>
	/// <inheritdoc cref="CanShimmer_Transformation(TShimmerable)"/>
	/// <list type="bullet">
	/// <item/>
	/// <see cref="IModShimmerable.CanShimmer"/> returns true for <paramref name="shimmerable"/>
	/// </list>
	/// </returns>
	public bool CanShimmer(TShimmerable shimmerable)
		=> CanShimmer_Transformation(shimmerable)
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
	/// None of the results contain bone or lihzahrd brick while skeletron or golem are undefeated if <see cref="IgnoreVanillaItemConstraints"/> is false (Used by vanilla
	/// for progression protection)
	/// <item/>
	/// All added <see cref="CanShimmerCallBack"/> return true
	/// <item/>
	/// All <see cref="SystemLoader.CanModShimmer(ShimmerTransformation, IModShimmerable)"/>
	///	<item/>
	/// The amount of empty NPC slots under slot 200 is less than the number of NPCs this transformation spawns
	/// </list>
	/// </returns>
	public bool CanShimmer_Transformation(TShimmerable shimmerable)
		=> !Disabled
		&& Conditions.All((condition) => condition.IsMet())
		&& (IgnoreVanillaItemConstraints || !Results.Any((result) => result.IsItemResult(ItemID.Bone) && !NPC.downedBoss3 || result.IsItemResult(ItemID.LihzahrdBrick) && !NPC.downedGolemBoss))
		&& CheckCanShimmerCallBacks(shimmerable)
		//&& SystemLoader.CanModShimmer(this, shimmerable)
		&& (ShimmerManager.GetCurrentAvailableNPCSlots() >= GetSpawnCount<NPCShimmerResult>());

	/// <summary> Checks all <see cref="CanShimmerCallBacks"/> for <paramref name="shimmerable"/> </summary>
	/// <returns> Returns true if all delegates in <see cref="CanShimmerCallBacks"/> return true or there are no callbacks </returns>
	public bool CheckCanShimmerCallBacks(TShimmerable shimmerable)
	{
		foreach (CanShimmerCallBack callBack in CanShimmerCallBacks?.GetInvocationList()?.Cast<CanShimmerCallBack>() ?? Array.Empty<CanShimmerCallBack>()) {
			if (!callBack.Invoke(this, shimmerable))
				return false;
		}
		return true;
	}

	/// <summary> Called by <see cref="ShimmerTransformation{TShimmeredType}.TryModShimmer(TShimmeredType)"/> once it finds a valid transformation </summary>
	public void Shimmer(TShimmerable source)
	{
		int usableStack = GetUsableStack(source);
		IEnumerable<IModShimmerable> spawned = SpawnModShimmerResults(source, usableStack); // Spawn results, output stack amount used
		source.ShimmerRemoveStacked(usableStack); // Removed amount used

		OnShimmerCallBacks?.Invoke(this, source, spawned);
		//SystemLoader.OnModShimmer(this, source, spawned);

		ShimmerManager.ShimmerEffect(source.Center);
	}


	#endregion Shimmering

	#region Helpers

	/// <inheritdoc cref="Clone"/>
	object ICloneable.Clone() => Clone();

	/// <summary> Gets the number of <typeparamref name="TResultType"/> in this transformation by <see cref="IShimmerResult{T}.Count"/> </summary>
	public int GetSpawnCount<TResultType>() where TResultType : GeneralShimmerResult
		=> Results.Sum((IShimmerResult<TShimmerable> result) => result is TResultType ? result.Count : 0);

	/// <summary> Gets the total number of spawns in this transformation by <see cref="IShimmerResult{T}.Count"/> </summary>
	public int GetSpawnCount()
		=> Results.Sum((IShimmerResult<TShimmerable> result) => result.Count);

	/// <summary> Gets the current amount of this <see cref="IModShimmerable"/> of <see cref="IModShimmerable.Stack"/> that can currently be used for shimmer </summary>
	public int GetUsableStack(IModShimmerable source)
	{
		// 200 and 50 are the values vanilla uses for the highest slot to count with and the maximum NPCs to spawn in one transformation set
		int npcSpawnCount = GetSpawnCount<NPCShimmerResult>();
		return npcSpawnCount != 0 ? Math.Min((int)MathF.Floor(ShimmerManager.GetCurrentAvailableNPCSlots() / (float)npcSpawnCount), source.Stack) : source.Stack;
	}

	#endregion Helpers

	#region Management
	public ShimmerTransformation()
	{
	}

	public ShimmerTransformation(TShimmerable CreateFrom)
	{
		SourceType = CreateFrom.Type;
	}
	#endregion Management

	#region Variables

	public int? SourceType { get; init; }


	#endregion Variables

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
		if (typeof(TShimmerable) == typeof(Item) && ItemID.Sets.ShimmerCountsAsItem[type] > 0)
			type = ItemID.Sets.ShimmerCountsAsItem[type];
		return type;
	}

	#endregion Redirects
	#region ControllerMethods

	public ShimmerTransformation<TShimmerable> AddCondition(Condition condition)
	{
		Conditions.Add(condition);
		return this;
	}

	#region AddResultMethods

	public ShimmerTransformation<TShimmerable> AddResult(IShimmerResult<TShimmerable> result)
	{
		if (result.Count <= 0)
			throw new ArgumentException("A Count greater than 0 is required", nameof(result));
		Results.Add(result);
		return this;
	}

	public ShimmerTransformation<TShimmerable> AddResult(IShimmerResult<IModShimmerable> result)
		=> AddResult((IShimmerResult<TShimmerable>)result);

	public ShimmerTransformation<TShimmerable> AddItemResult<T>(int stack = 1) where T : ModItem
		=> AddItemResult(ModContent.ItemType<T>(), stack);

	public ShimmerTransformation<TShimmerable> AddItemResult(int type, int stack = 1)
		=> AddResult(new ItemShimmerResult(type, stack));

	public ShimmerTransformation<TShimmerable> AddNPCResult<T>(int count = 1) where T : ModNPC
		=> AddNPCResult(ModContent.NPCType<T>(), count);

	public ShimmerTransformation<TShimmerable> AddNPCResult(int type, int count = 1)
		=> AddResult(new NPCShimmerResult(type, count));

	public ShimmerTransformation<TShimmerable> AddCoinLuckResult(int coinLuck)
		=> AddResult(new CoinLuckShimmerResult(coinLuck));

	#endregion AddResultMethods

	public ShimmerTransformation<TShimmerable> DisableVanillaItemConstraints()
	{
		IgnoreVanillaItemConstraints = true;
		return this;
	}

	public ShimmerTransformation<TShimmerable> DisableChainedShimmers()
	{
		AllowChainedShimmers = false;
		return this;
	}

	public ShimmerTransformation<TShimmerable> AddCanShimmerCallBack(CanShimmerCallBack callBack)
	{
		CanShimmerCallBacks += callBack;
		return this;
	}

	public ShimmerTransformation<TShimmerable> AddModifyShimmerCallBack(ModifyShimmerCallBack callBack)
	{
		ModifyShimmerCallBacks += callBack;
		return this;
	}

	public ShimmerTransformation<TShimmerable> AddOnShimmerCallBack(OnShimmerCallBack callBack)
	{
		OnShimmerCallBacks += callBack;
		return this;
	}
	#endregion ControllerMethods

	#region Ordering

	/// <summary> Sets the Ordering of this <see cref="ShimmerTransformation"/>. This <see cref="ShimmerTransformation"/> can't already have one. </summary>
	private ShimmerTransformation<TShimmerable> SetOrdering(IShimmerTransformation<TShimmerable> transformation, bool after, int forType)
	{
		if (Target != null)
			throw new Exception("This transformation already has an ordering.");
		if (!ShimmerManager<TShimmerable>.Transformations[forType].Contains(transformation))
			throw new ArgumentException("This passed transformation must be registered.", nameof(transformation));

		Target = transformation;
		After = after;
		IShimmerTransformation<TShimmerable> target = transformation;
		do {
			if (target == this)
				throw new Exception("Shimmer ordering loop!");
			target = target.Target;
		} while (target != null);

		return this;
	}

	public ShimmerTransformation<TShimmerable> SortBefore(IShimmerTransformation<TShimmerable> transformation, int forType) => SetOrdering(transformation, false, forType);

	public ShimmerTransformation<TShimmerable> SortAfter(IShimmerTransformation<TShimmerable> transformation, int forType) => SetOrdering(transformation, true, forType);


	#endregion Ordering


	#region Registering

	/// <inheritdoc cref="Register(int)"/>
	/// <exception cref="InvalidOperationException"> Thrown if this instance was not created from an Entity </exception>
	public void Register()
	{
		if (SourceType == null)
			throw new InvalidOperationException("The transformation must be created from an entity for the parameterless Register() to be used.");
		Register(SourceType.Value);
	}

	/// <inheritdoc cref="Register(IEnumerable{int})"/>
	public void Register(params int[] types)
		=> Register(types.AsEnumerable());

	/// <inheritdoc cref="Register(int)"/>
	public void Register(IEnumerable<int> types)
	{
		foreach (int ID in types)
			Register(ID);
	}

	/// <summary> Finalizes transformation </summary>
	public void Register(int type)
		=> ShimmerManager<TShimmerable>.AddTransformation(type, this);

	#endregion Registering

	#region Shimmering
	/// <summary>
	/// Spawns every item in <see cref="GetResults()"/> <paramref name="stackUsed"/> times for this transformation using <paramref name="source"/>
	/// </summary>
	public IEnumerable<IModShimmerable> SpawnModShimmerResults(TShimmerable source, int stackUsed)
	{
		List<IModShimmerable> results = new();
		foreach (IModShimmerable shimmerable in Results.SelectMany(result => result.SpawnFrom(source, new(stackUsed))).Where(shimmerable => shimmerable != null)) { //Spawns the individual result, adds it to the list
			if (!AllowChainedShimmers)
				shimmerable.PreventingChainedShimmers = true;

			if (shimmerable is Entity entity) {
				entity.shimmerWet = true;
				entity.wet = true;
			}

			results.Add(shimmerable);
		}
		return results;
	}
	/// <summary>
	/// Checks every <see cref="ShimmerTransformation"/> for this <typeparamref name="TShimmerable"/> and returns true when if finds one that passes
	/// <see cref="ShimmerTransformation.CanShimmer_Transformation(IModShimmerable)"/>. <br/> Does not check <see cref="IModShimmerable.CanShimmer"/>
	/// </summary>
	/// <returns> True if there is a mod transformation this <typeparamref name="TShimmerable"/> could undergo </returns>
	public static bool AnyValidModShimmer(TShimmerable source)
		=> ShimmerManager<TShimmerable>.GetValidTransformations(source.ShimmerRedirectedType).Any(transformation => transformation.CanShimmer_Transformation(source));

	/// <summary> Tries to complete a shimmer operation on the <typeparamref name="TShimmerable"/> passed, should not be called on multiplayer clients </summary>
	/// <param name="source"> The <typeparamref name="TShimmerable"/> to be shimmered </param>
	/// <returns> True if the transformation is successful, false if it is should fall through to vanilla as normal </returns>
	public bool TryModShimmer(TShimmerable source)
	{
		if (CanShimmer(source)) { // Checks conditions and callback in CanShimmer
			ShimmerTransformation<TShimmerable> copy = Clone(); // Make a copy
			copy.ModifyShimmerCallBacks?.Invoke(copy, source); // As to not be effected by any changes made here
															   //SystemLoader.ModifyModShimmer(copy, source);
			copy.Shimmer(source);
			return true;
		}
		return false;
	}

	#endregion Shimmering

	#region Helpers
	/// <summary> Creates a deep clone of this <see cref="ShimmerTransformation{TShimmerable}"/>. </summary>

	public ShimmerTransformation<TShimmerable> Clone()
		=> new() {
			Disabled = Disabled,
			After = After,
			Target = Target,
			AllowChainedShimmers = AllowChainedShimmers,
			SourceType = SourceType,
			IgnoreVanillaItemConstraints = IgnoreVanillaItemConstraints,
			Conditions = new List<Condition>(Conditions), // Technically I think the localization for the conditions can be changed
			Results = new List<IShimmerResult<TShimmerable>>(Results), // List is new, GeneralShimmerResult is a readonly struct
			CanShimmerCallBacks = (CanShimmerCallBack)CanShimmerCallBacks?.Clone(), // Stored values are immutable
			ModifyShimmerCallBacks = (ModifyShimmerCallBack)ModifyShimmerCallBacks?.Clone(),
			OnShimmerCallBacks = (OnShimmerCallBack)OnShimmerCallBacks?.Clone(),
		};

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

	/// <summary> <see cref="Type"/> passed through <see cref="ShimmerTransformation{TShimmerable}.GetRedirectedType(int)"/> </summary>
	public abstract int ShimmerRedirectedType { get; }

	public abstract int Type { get; }

	/// <summary>
	/// When this undergoes a <see cref="ShimmerTransformation"/> this is the amount contained within one instance of the type, returns 1 for <see cref="NPC"/> and
	/// <see cref="Projectile"/>, <see cref="Item.stack"/> for <see cref="Item"/><br/>
	/// </summary>
	public virtual int Stack => 1;

	/// <summary> Used by <see cref="ShimmerTransformation.AllowChainedShimmers"/> to check if this instance has left shimmer long enough to go again, entirely server or single-player </summary>
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