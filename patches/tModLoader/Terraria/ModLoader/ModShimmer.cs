using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace Terraria.ModLoader;
// TML: #AdvancedShimmerTransformations

/// <summary>
/// Represents the behaviour and output of a shimmer transformation, the Entity(s) that can use it are stored via <see
/// cref="ModShimmerTransformations"/> which is updated via <see cref="Register()"/> and its overloads
/// </summary>
public record class ModShimmer : IComparable<ModShimmer>
{
	/// <summary>
	/// Dictionary containing every <see cref="ModShimmer"/> registered to tMod indexed by <see cref="ModShimmerTypeID"/> and the entities type, automatically done in <see cref="Register()"/> and its overloads
	/// </summary>
	public static Dictionary<(ModShimmerTypeID, int), List<ModShimmer>> ModShimmerTransformations { get; } = new();

	public static List<(CustomShimmerableSpawner, ModShimmer)> CustomShimmerables { get; } = new();

	#region Constructors

	/// <inheritdoc cref="ModShimmer(ValueTuple{ModShimmerTypeID, int})"/>
	/// <param name="npc"><inheritdoc cref="ModShimmer(ValueTuple{ModShimmerTypeID, int})"/></param>

	public ModShimmer(NPC npc) : this((ModShimmerTypeID.NPC, npc.type))
	{ }

	/// <inheritdoc cref="ModShimmer(ValueTuple{ModShimmerTypeID, int})"/>
	/// <param name="item"><inheritdoc cref="ModShimmer(ValueTuple{ModShimmerTypeID, int})"/></param>
	public ModShimmer(Item item) : this((ModShimmerTypeID.Item, item.type))
	{ }

	/// <inheritdoc cref="ModShimmer(ValueTuple{ModShimmerTypeID, int})"/>
	public ModShimmer()
	{ }

	/// <inheritdoc cref="ModShimmer"/>
	/// <param name="entityIdentification"> Assigned to <see cref="InstantiationEntity"/> for use with the parameterless <see cref="Register()"/> </param>
	/// <exception cref="ArgumentException"> Thrown when <paramref name="entityIdentification"/> is not <see cref="ModShimmerTypeID.NPC"/> or <see cref="ModShimmerTypeID.Item"/> </exception>
	private ModShimmer((ModShimmerTypeID, int) entityIdentification)
	{
		if (!entityIdentification.Item1.IsValidSourceType())
			throw new ArgumentException("ModShimmerTypeID must be a valid source type, use parameterless constructor if an instantiation entity was not required here", nameof(entityIdentification));
		InstantiationEntity = entityIdentification;
	}

	#endregion Constructors

	#region FunctionalityVariables

	/// <summary>
	/// The entity that was used to create this transformation, does not have to be used when registering
	/// </summary>
	public (ModShimmerTypeID, int)? InstantiationEntity { get; init; }

	/// <summary>
	/// Every condition must be true for the transformation to occur
	/// </summary>
	public List<Condition> Conditions { get; init; } = new();

	/// <summary>
	/// The results that the transformation produces.
	/// </summary>
	public List<ModShimmerResult> Results { get; init; } = new();

	public List<CustomShimmerableSpawned> CustomResults { get; init; } = new();
	public CustomShimmerableSpawner CustomSource{ get; init; }

	/// <summary>
	/// Vanilla disallows a transformation if the result includes either a bone or a lihzahrd brick when skeletron or golem are undefeated respectively
	/// </summary>
	public bool IgnoreVanillaItemConstraints { get; private set; }

	/// <summary>
	/// Gives a priority to the shimmer operation, lower numbers are sorted lower, higher numbers are sorted higher, caps at 10
	/// </summary>
	public int Priority { get; private set; } = 0;

	/// <summary>
	/// Called in addition to conditions to check if the entity shimmers
	/// </summary>
	/// <param name="transformation"> The transformation </param>
	/// <param name="source"> The entity to be shimmered, either an <see cref="Item"/> or an <see cref="NPC"/> </param>
	public delegate bool CanShimmerCallBack(ModShimmer transformation, Entity source);

	/// <inheritdoc cref="CanShimmerCallBack"/>
	public CanShimmerCallBack CanShimmerCallBacks { get; private set; }

	/// <summary>
	/// Called when the entity shimmers
	/// </summary>
	/// <param name="transformation"> The transformation </param>
	/// <param name="spawnedEntities"> A list of the spawned Entities </param>
	/// <param name="source"> The entity that was shimmered </param>
	public delegate void PostShimmerCallBack(ModShimmer transformation, Entity source, List<Entity> spawnedEntities);

	/// <inheritdoc cref="PostShimmerCallBack"/>
	public PostShimmerCallBack OnShimmerCallBacks { get; private set; }

	#endregion FunctionalityVariables

	#region ControllerMethods

	/// <summary>
	/// Adds a condition to <see cref="Conditions"/>. <inheritdoc cref="Conditions"/>
	/// </summary>
	/// <param name="condition"> The condition to be added </param>
	public ModShimmer AddCondition(Condition condition)
	{
		Conditions.Add(condition);
		return this;
	}

	#region AddResultMethods

	/// <summary>
	/// Adds a result to <see cref="Results"/>, this will be spawned when the entity successfully shimmers
	/// </summary>
	/// <param name="result"> The result to be added </param>
	/// <exception cref="ArgumentException">
	/// thrown when <paramref name="result"/> does not have a valid spawn <see cref="ModShimmerTypeID"/> or has a <see cref="ModShimmerResult.Count"/>
	/// that is not greater than 0
	/// </exception>
	public ModShimmer AddResult(ModShimmerResult result)
	{
		if (!result.ResultType.IsValidSpawnedType())
			throw new ArgumentException("ModShimmerTypeID must be a valid spawn type, check Example Mod for details", nameof(result));
		if (result.Count <= 0)
			throw new ArgumentException("A Count greater than 0 is required", nameof(result));

		Results.Add(result);
		return this;
	}

	/// <inheritdoc cref=" AddItemResult(int, int)"/>
	public ModShimmer AddModItemResult<T>(int stack) where T : ModItem
		=> AddItemResult(ModContent.ItemType<T>(), stack);

	/// <inheritdoc cref=" AddResult(ModShimmerResult)"/>
	/// <param name="type"> The <see cref="Item.type"/> of the <see cref="Item"/> </param>
	/// <param name="stack"> The amount of Item to be spawned </param>
	public ModShimmer AddItemResult(int type, int stack)
		=> AddResult(new(ModShimmerTypeID.Item, type, stack));

	/// <inheritdoc cref="AddNPCResult(int, int)"/>
	public ModShimmer AddModNPCResult<T>(int count) where T : ModNPC
		=> AddNPCResult(ModContent.NPCType<T>(), count);

	/// <inheritdoc cref=" AddResult(ModShimmerResult)"/>
	/// <param name="type"> The <see cref="NPC.type"/> of the <see cref="NPC"/> </param>
	/// <param name="count"> The amount of NPC to be spawned </param>
	public ModShimmer AddNPCResult(int type, int count)
		=> AddResult(new(ModShimmerTypeID.NPC, type, count));

	/// <inheritdoc cref=" AddResult(ModShimmerResult)"/>
	/// <param name="coinLuck"> The amount of coin luck to be added </param>
	public ModShimmer AddCoinLuckResult(int coinLuck)
		=> AddResult(new(ModShimmerTypeID.CoinLuck, -1, coinLuck));

	#endregion AddResultMethods

	/// <inheritdoc cref="IgnoreVanillaItemConstraints"/>
	public ModShimmer DisableVanillaItemConstraints()
	{
		IgnoreVanillaItemConstraints = true;
		return this;
	}

	/// <inheritdoc cref="Priority"/>
	public ModShimmer SetPriority(int priority)
	{
		Priority = Math.Min(priority, 10);
		return this;
	}

	/// <summary>
	/// Adds a delegate to <see cref="CanShimmerCallBacks"/> that will be called if the shimmer transformation succeeds
	/// </summary>
	public ModShimmer AddCanShimmerCallBack(CanShimmerCallBack callBack)
	{
		CanShimmerCallBacks += callBack;
		return this;
	}

	/// <summary>
	/// Adds a delegate to <see cref="OnShimmerCallBacks"/> that will be called if the shimmer transformation succeeds
	/// </summary>
	public ModShimmer AddOnShimmerCallBack(PostShimmerCallBack callBack)
	{
		OnShimmerCallBacks += callBack;
		return this;
	}

	/// <inheritdoc cref="Register(ModShimmerTypeID, int)"/>
	/// <exception cref="InvalidOperationException"> Thrown if this <see cref="ModShimmer"/> instance was not created from an Entity </exception>
	public void Register()
	{
		if (InstantiationEntity == null)
			throw new InvalidOperationException("The transformation must be created from an entity for the parameterless Register() to be used.");
		Register(InstantiationEntity.Value);
	}

	/// <inheritdoc cref="Register(ValueTuple{ModShimmerTypeID, int})"/>
	public void Register(ModShimmerTypeID modShimmerType, int type)
		=> Register((modShimmerType, type));

	/// <summary>
	/// Finalizes transformation, adds to <see cref="ModShimmerTransformations"/>
	/// </summary>
	/// <exception cref="ArgumentException">
	/// Thrown if <paramref name="entityIdentifier"/> field Item1 of type <see cref="ModShimmerTypeID"/> is an invalid source type
	/// </exception>
	public void Register((ModShimmerTypeID, int) entityIdentifier)
	{
		if (!entityIdentifier.Item1.IsValidSourceType())
			throw new ArgumentException("A valid source type for ModShimmerTypeID must be passed here", nameof(entityIdentifier));
		if (!ModShimmerTransformations.TryAdd(entityIdentifier, new() { this })) //Try add a new entry for the tuple
			ModShimmerTransformations[entityIdentifier].Add(this); // If it fails, entry exists, therefore add to list

		ModShimmerTransformations[entityIdentifier].Sort();
	}

	/// <inheritdoc cref="Register(ModShimmerTypeID, int)"/>
	public void Register(IEnumerable<(ModShimmerTypeID, int)> identifiers)
	{
		foreach ((ModShimmerTypeID, int) ID in identifiers)
			Register(ID);
	}

	#endregion ControllerMethods

	#region Operation

	/// <summary>
	/// Checks if the entity supplied can undergo a shimmer transformation, should not alter game state / read only
	/// </summary>
	/// <param name="entity"> The entity being shimmered </param>
	/// <returns>
	/// true if the following are all true in order
	/// <list type="number">
	/// <item/>
	/// All <see cref="Conditions"/> return true
	/// <item/>
	/// All added <see cref="CanShimmerCallBack"/> return true
	/// <item/>
	/// <see cref="IShimmerableEntityGlobal{TEntity}.CanShimmer"/> returns true
	/// <item/>
	/// <see cref="ModNPC.CanShimmer"/> or <see cref="ModItem.CanShimmer"/> returns true or unused
	/// <item/>
	/// None of the results contain bone or lihzahrd brick while skeletron or golem are undefeated if <see cref="IgnoreVanillaItemConstraints"/> is
	/// false (default)
	/// </list>
	/// </returns>
	public bool CanModShimmer<TEntity>(TEntity entity) where TEntity : Entity, IShimmerableEntity
		=> CanModShimmer_Transformation(entity)
		&& entity.CanShimmer();

	/// <summary>
	/// Checks the conditions for this transformation
	/// </summary>
	/// <returns>
	/// true if the following are all true in order
	/// <list type="number">
	/// <item/>
	/// All <see cref="Conditions"/> return true
	/// <item/>
	/// All added <see cref="CanShimmerCallBack"/> return true
	/// <item/>
	/// None of the results contain bone or lihzahrd brick while skeletron or golem are undefeated if <see cref="IgnoreVanillaItemConstraints"/> is
	/// false (default)
	/// </list>
	/// </returns>
	private bool CanModShimmer_Transformation<TEntity>(TEntity entity) where TEntity : Entity, IShimmerableEntity
		=> Conditions.All((condition) => condition.IsMet())
		&& (CheckCanShimmerCallBacks(entity))
		&& (IgnoreVanillaItemConstraints || !Results.Any((result) => result.ResultType == ModShimmerTypeID.Item && (result.Type == ItemID.Bone && !NPC.downedBoss3 || result.Type == ItemID.LihzahrdBrick && !NPC.downedGolemBoss)));

	private bool CheckCanShimmerCallBacks(Entity entity)
	{
		if (CanShimmerCallBacks == null)
			return true;
		foreach (CanShimmerCallBack callBack in CanShimmerCallBacks.GetInvocationList().Cast<CanShimmerCallBack>()) {
			if (!callBack.Invoke(this, entity))
				return false;
		}
		return true;
	}

	/// <summary>
	/// Checks every <see cref="ModShimmer"/> for this entity and returns true when if finds one that passes <see cref="CanModShimmer_Transformation{TEntity}(TEntity)"/>.
	/// <br/> Does not check <see cref="IShimmerableEntity.CanShimmer"/>
	/// </summary>
	/// <returns> True if there is a mod transformation this entity could undergo </returns>
	public static bool AnyValidModShimmer<TEntity>(TEntity entity) where TEntity : Entity, IShimmerableEntity
	{
		ArgumentNullException.ThrowIfNull(entity, nameof(entity));
		if (!ModShimmerTransformations.ContainsKey((entity.ModShimmerTypeID, entity.ShimmerableEntityTypePassthrough)))
			return false;

		foreach (ModShimmer modShimmer in ModShimmerTransformations[(entity.ModShimmerTypeID, entity.ShimmerableEntityTypePassthrough)]) {
			if (modShimmer.CanModShimmer_Transformation(entity))
				return true;
		}

		return false;
	}

	/// <summary>
	/// Tries to complete a shimmer operation on the entity passed, should not be called on multiplayer clients
	/// </summary>
	/// <param name="entity"> The <see cref="Entity"/> to be shimmered </param>
	/// <returns>
	/// True if the transformation is successful, false if it is should fall through to vanilla as normal
	/// </returns>
	public static bool TryModShimmer<TEntity>(TEntity entity) where TEntity : Entity, IShimmerableEntity
	{
		List<ModShimmer> transformations = ModShimmerTransformations.GetValueOrDefault((entity.ModShimmerTypeID, entity.ShimmerableEntityTypePassthrough));
		if (!(transformations?.Count > 0)) // Invers to catch null
			return false;

		foreach (ModShimmer transformation in transformations) { // Loops possible transformations
			if (transformation.Results.Count > 0 && transformation.CanModShimmer(entity)) { // Checks conditions and callback in CanShimmer
				DoModShimmer(entity, transformation);
				return true;
			}
		}
		return false;
	}

	public static void CheckShimmers()
	{
		foreach ((CustomShimmerableSpawner, ModShimmer) spawnerTuple in CustomShimmerables) {
			if (spawnerTuple.Item1.CanShimmer()) {
				spawnerTuple.Item1.ShimmerDespawnSelf();
			}
				
		}
	}

	public static void DoModShimmer<TEntity>(TEntity entity, ModShimmer transformation) where TEntity : Entity, IShimmerableEntity
	{
		SpawnModShimmerResults(entity, transformation);
		CleanupShimmerSource(entity);
		ShimmerEffect(entity.Center);
	}

	private static void SpawnModShimmerResults<TEntity>(TEntity entity, ModShimmer transformation) where TEntity : Entity, IShimmerableEntity
	{
		List<Entity> spawnedEntities = new(); // List to be passed for onShimmerCallBacks
		int resultSpawnCounter = 0; //Used to offset spawned stuff

		foreach (ModShimmerResult result in transformation.Results)
			SpawnModShimmerResult(entity, result, ref resultSpawnCounter, ref spawnedEntities); //Spawns the individual result, adds it to the list

		transformation.OnShimmerCallBacks?.Invoke(transformation, entity, spawnedEntities);
	}

	private static void SpawnModShimmerResult<TEntity>(TEntity entity, ModShimmerResult shimmerResult, ref int resultIndex, ref List<Entity> spawned) where TEntity : Entity, IShimmerableEntity
	{// TODO: Implement spawn offsets from item
		int stackCounter = shimmerResult.Count;

		switch (shimmerResult.ResultType) {
			case ModShimmerTypeID.Item: {
				while (stackCounter > 0) {
					Item item = Main.item[Item.NewItem(entity.GetSource_Misc(ItemSourceID.ToContextString(ItemSourceID.Shimmer)), (int)entity.position.X, (int)entity.position.Y, entity.width, entity.height, shimmerResult.Type)];
					item.stack = Math.Min(item.maxStack, stackCounter);
					stackCounter -= item.stack;
					item.shimmerTime = 1f;
					item.shimmered = true;
					item.shimmerWet = true;
					item.wet = true;
					item.velocity *= 0.1f;
					item.playerIndexTheItemIsReservedFor = Main.myPlayer;
					NetMessage.SendData(MessageID.SyncItemsWithShimmer, -1, -1, null, item.whoAmI, 1f); // net sync spawning the item

					spawned.Add(item);
					resultIndex++;
				}
				break;
			}

			case ModShimmerTypeID.NPC: {
				int spawnCount = Math.Min(NPC.GetAvailableAmountOfNPCsToSpawnUpToSlot(stackCounter, 200), 50);
				// 200 and 50 are the values vanilla uses for the highest slot to count with and the maximum NPCs to spawn in one transformation set,
				// technically can be violated because multiple NPCs can be put into the same transformation

				for (int i = 0; i < spawnCount; i++) { // Loop spawn NPCs
					NPC newNPC = NPC.NewNPCDirect(entity.GetSource_Misc(ItemSourceID.ToContextString(ItemSourceID.Shimmer)), (int)entity.position.X, (int)entity.position.Y, shimmerResult.Type); //Should cause net update stuff

					//syncing up some values that vanilla intentionally sets after SetDefaults() is NPC transformations, mostly self explanatory

					if (entity is NPC nPC && shimmerResult.KeepVanillaTransformationConventions) {
						newNPC.extraValue = nPC.extraValue;
						newNPC.CopyInteractions(nPC);
						newNPC.spriteDirection = nPC.spriteDirection;
						newNPC.shimmerTransparency = nPC.shimmerTransparency;

						if (nPC.value == 0f) // I think this is just for statues
							newNPC.value = 0f;

						newNPC.SpawnedFromStatue = nPC.SpawnedFromStatue;

						newNPC.buffType = nPC.buffType[..]; // Pretty sure the manual way vanilla does it is actually the slowest way that isn't LINQ
						newNPC.buffTime = nPC.buffTime[..];
					}
					else {
						newNPC.shimmerTransparency = 1f;
					}
					newNPC.velocity = entity.velocity;
					newNPC.TargetClosest();
					spawned.Add(newNPC);

					if (Main.netMode == NetmodeID.Server) {
						NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, newNPC.whoAmI);
						NetMessage.SendData(MessageID.NPCBuffs, -1, -1, null, newNPC.whoAmI);
						newNPC.netUpdate = true;
					}
					resultIndex++;
				}

				break;
			}

			case ModShimmerTypeID.CoinLuck: // TODO: Test
				Main.player[Main.myPlayer].AddCoinLuck(entity.Center, shimmerResult.Count);
				NetMessage.SendData(MessageID.ShimmerActions, -1, -1, null, 1, (int)entity.Center.X, (int)entity.Center.Y, shimmerResult.Count);
				break;

			case ModShimmerTypeID.Empty:
				resultIndex += shimmerResult.Count;
				break;
		}
	}

	private static void CleanupShimmerSource<TEntity>(TEntity entity) where TEntity : Entity, IShimmerableEntity
	{
		switch (entity.ModShimmerTypeID) {
			case ModShimmerTypeID.NPC:
				CleanupShimmerSource(entity as NPC);
				break;

			case ModShimmerTypeID.Item:
				CleanupShimmerSource(entity as Item);
				break;
		}
	}

	private static void CleanupShimmerSource(NPC npc)
	{
		npc.active = false; // despawn this NPC
		if (Main.netMode == NetmodeID.Server) {
			npc.netSkip = -1;
			npc.life = 0;
			NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
		}
	}

	private static void CleanupShimmerSource(Item item)
	{
		item.makeNPC = -1;
		item.active = false;
		item.shimmerTime = 0f;
		if (Main.netMode == NetmodeID.Server)
			NetMessage.SendData(MessageID.SyncItemsWithShimmer, -1, -1, null, item.whoAmI, 1f);
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
	/// Creates a new instance of <see cref="ModShimmer"/>. Not a true deep clone as the new instance will share some values with the old instance but any values
	/// still shared cannot be edited. (e.g. A new <see cref="List{T}"/> of <see cref="Condition"/> that shares the immutable conditions inside it)
	/// </summary>
	public ModShimmer DeepClone()
		=> new() {
			InstantiationEntity = InstantiationEntity, // Assigns by value
			Priority = Priority,
			Conditions = new List<Condition>(Conditions), // List is new, Condition is a record type
			Results = new List<ModShimmerResult>(Results), // List is new, ModShimmerResult is a structure type
			IgnoreVanillaItemConstraints = IgnoreVanillaItemConstraints, // Assigns by value
			CanShimmerCallBacks = (CanShimmerCallBack)CanShimmerCallBacks.Clone(), // Stored values are immutable
			OnShimmerCallBacks = (PostShimmerCallBack)OnShimmerCallBacks.Clone(),
		};

	/// <summary>
	/// Compares using <see cref="Priority"/>
	/// </summary>
	public int CompareTo(ModShimmer other)
	{
		return Priority - other.Priority;
	}

	#endregion Operation

}

public abstract class CustomShimmerableSpawner
{
	public abstract void OnShimmer();

	public abstract void ShimmerDespawnSelf();

	public abstract bool CanShimmer();
}

public abstract class CustomShimmerableSpawned
{
	public abstract void ShimmerSpawnSelf();

	public abstract void OnShimmer();

	public const ModShimmerTypeID ModShimmerType = ModShimmerTypeID.Custom;
}

/// <summary>
/// Value used by <see cref="ModShimmerResult"/> to identify what type of entity to spawn. <br/> The <see cref="Empty"/> value simply sets the
/// shimmer as successful, spawns nothing, for if you desire entirely custom behavior to be defined in <see
/// cref="ModShimmer.AddOnShimmerCallBack(ModShimmer.PostShimmerCallBack)"/> but do not want to include the item or NPC spawn that would usually count
/// as a "successful" transformation
/// </summary>
[DefaultValue(Null)]
public enum ModShimmerTypeID
{
	NPC,
	Item,
	CoinLuck,
	Empty,
	Null,
	Custom,
}

/// <summary>
/// Extensions for <see cref="ModShimmerTypeID"/>
/// </summary>
public static class ModShimmerTypeIDExtensions
{
	public static bool IsValidSourceType(this ModShimmerTypeID id)
		=> id == ModShimmerTypeID.NPC || id == ModShimmerTypeID.Item;

	public static bool IsValidSpawnedType(this ModShimmerTypeID id)
		=> id == ModShimmerTypeID.NPC || id == ModShimmerTypeID.Item || id == ModShimmerTypeID.CoinLuck || id == ModShimmerTypeID.Empty;
}

/// <summary>
/// A record representing the information to spawn an entity during a shimmer transformation
/// </summary>
/// <param name="ResultType"> The type of shimmer operation this represents </param>
/// <param name="Type">
/// The type of the entity to spawn, ignored when <paramref name="ResultType"/> is <see cref="ModShimmerTypeID.CoinLuck"/> or <see cref="ModShimmerTypeID.Empty"/>
/// </param>
/// <param name="Count">
/// The number of this entity to spawn, if <paramref name="ResultType"/> is <see cref="ModShimmerTypeID.CoinLuck"/> this is the coin luck value, if
/// custom, set this to the expected amount of physical entities spawned
/// </param>
/// <param name="KeepVanillaTransformationConventions">
/// Keeps <see cref="ModShimmer"/> roughly in line with vanilla as far as base functionality goes, e.g. NPC's spawned via statues stay keep their
/// spawned NPCs from a statue when shimmered, if you have no reason to disable, don't
/// </param>
public record struct ModShimmerResult(ModShimmerTypeID ResultType, int Type, int Count, bool KeepVanillaTransformationConventions)
{
	/// <inheritdoc cref="ModShimmerResult(ModShimmerTypeID, int, int, bool)"/>
	public ModShimmerResult() : this(default, default, default, default) { }

	/// <inheritdoc cref="ModShimmerResult(ModShimmerTypeID, int, int, bool)"/>
	public ModShimmerResult(ModShimmerTypeID ResultType, int Type, int Count) : this(ResultType, Type, Count, true) { }
}

public interface IShimmerableEntityGlobal<TEntity> where TEntity : Entity, IShimmerableEntity
{
	/// <inheritdoc cref="IShimmerableEntity.CanShimmer"/>
	public abstract bool CanShimmer(TEntity entity);

	/// <inheritdoc cref="IShimmerableEntity.OnShimmer"/>
	public abstract void OnShimmer(TEntity entity);
}

public interface IShimmerableEntity
{
	/// <summary>
	/// Checks if this Entity can currently undergo a shimmer transformation. This includes both vanilla and <br/> Should not makes changes to game
	/// state. consider read only
	/// </summary>
	/// <returns> True if the entity currently has a valid shimmer operation it can use. </returns>
	public abstract bool CanShimmer();

	public abstract void OnShimmer();

	public abstract ModShimmerTypeID ModShimmerTypeID { get; }

	public abstract int ShimmerableEntityTypePassthrough { get; }
}