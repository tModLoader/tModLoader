using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace Terraria.ModLoader;

public sealed class SafeIndexDictionary
{
	public static SafeIndexDictionary Instance = new();

	public bool this[int type] {
		get => ModShimmer.UseNPCSpawnedFromStatue.TryGetValue(type, out bool val) && !val;
		set {
			if (!ModShimmer.UseNPCSpawnedFromStatue.TryAdd(type, value)) //Try add a new entry
				ModShimmer.UseNPCSpawnedFromStatue[type] = value; // If it fails, entry exists, therefore set
		}
	}
}

// TML: #AdvancedShimmerTransformations
public sealed class ModShimmer
{
	/// <summary>
	/// Dictionary containing every <see cref="ModShimmer"/> registered to tMod indexed by <see cref="ModShimmerTypeID"/> and the entities type
	/// </summary>
	public static Dictionary<(ModShimmerTypeID, int), List<ModShimmer>> ModShimmerTransformations { get; } = new();

	/// <summary>
	/// False if the NPC should not despawn in shimmer if <see cref="NPC.SpawnedFromStatue"/> is true. Value is assumed to be true if there is no dictionary value
	/// </summary>
	public static Dictionary<int, bool> UseNPCSpawnedFromStatue { get; } = new();

	#region Constructors

	public ModShimmer(NPC npc) : this((ModShimmerTypeID.NPC, npc.type))
	{ }

	public ModShimmer(Item item) : this((ModShimmerTypeID.Item, item.type))
	{ }

	private ModShimmer((ModShimmerTypeID, int) entityIdentification)
	{
		if (!entityIdentification.Item1.IsValidSourceType())
			throw new ArgumentException("ModShimmerTypeID must be a valid source type, use parameterless constructor if an instantiation entity was not required here", nameof(entityIdentification));
		InstantiationEntity = entityIdentification;
	}

	public ModShimmer()
	{ }

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
	/// The entities that the transformation produces.
	/// </summary>
	public List<ModShimmerResult> Results { get; init; } = new();

	/// <summary>
	/// Vanilla disallows a transformation if the result includes either a bone or a lihzahrd brick, when skeletron or golem are undefeated respectively
	/// </summary>
	public bool IgnoreVanillaItemConstraints { get; private set; }

	/// <summary>
	/// Makes this transformation allow other transformation to also spawn results, automatically sets this transformation to the highest priority
	/// </summary>
	public bool Additive { get; private set; }

	/// <summary>
	///	Called in addition to conditions to check if the entity shimmers
	/// </summary>
	/// <param name="transformation"> The transformation </param>
	/// <param name="source"> The entity to be shimmered, either an <see cref="Item"/> or an <see cref="NPC"/></param>
	public delegate bool CanShimmerCallBack(ModShimmer transformation, Entity source);

	/// <inheritdoc cref="CanShimmerCallBack"/>
	public CanShimmerCallBack CanShimmerCallBacks { get; set; }

	/// <summary>
	///	Called when the entity shimmers
	/// </summary>
	/// <param name="transformation"> The transformation </param>
	/// <param name="spawnedEntities"> A list of the spawned Entities </param>
	/// <param name="source"> The entity that was shimmered </param>
	public delegate void OnShimmerCallBack(ModShimmer transformation, Entity source, List<Entity> spawnedEntities);

	/// <inheritdoc cref="OnShimmerCallBack"/>
	public OnShimmerCallBack OnShimmerCallBacks { get; set; }

	#endregion FunctionalityVariables

	#region ControllerMethods

	/// <summary>
	/// Adds a condition to <see cref="Conditions"/>
	/// </summary>
	/// <param name="condition"> The condition to be added </param>
	public ModShimmer AddCondition(Condition condition)
	{
		Conditions.Add(condition);
		return this;
	}

	#region AddResultMethods

	/// <summary>
	/// Adds a result to <see cref="Results"/>
	/// </summary>
	/// <param name="result"> The result to be added </param>
	/// <exception cref="ArgumentException"> thrown when <paramref name="result"/> does not have a valid spawn <see cref="ModShimmerTypeID"/> or has a <see cref="ModShimmerResult.Count"/> that is not greater than 0 </exception>
	public ModShimmer AddResult(ModShimmerResult result)
	{
		if (!result.ResultType.IsValidSpawnedType())
			throw new ArgumentException("ModShimmerTypeID must be a valid spawn type, check Example Mod for details", nameof(result));
		if (result.Count > 0)
			throw new ArgumentException("A Count greater than 0 is required", nameof(result));

		Results.Add(result);
		return this;
	}

	/// <inheritdoc cref=" AddResult(ModShimmerResult)"/>
	/// <param name="resultType"> The type of shimmer operation this represents </param>
	/// <param name="type"> The type of the entity to spawn, ignored when <paramref name="resultType"/> is <see cref="ModShimmerTypeID.CoinLuck"/> or <see cref="ModShimmerTypeID.Custom"/> </param>
	/// <param name="count"> The number of this entity to spawn, if <paramref name="resultType"/> is <see cref="ModShimmerTypeID.CoinLuck"/> this is the coin luck value, if <see cref="ModShimmerTypeID.Custom"/>, set this to the expected amount of physical entities spawned as it affects the way the spawned entities are spread </param>

	public ModShimmer AddResult(ModShimmerTypeID resultType, int type, int count)
		=> AddResult(new ModShimmerResult(resultType, type, count));

	/// <inheritdoc cref=" AddResult(ModShimmerResult)"/>
	/// <param name="stack"> The stack size to be added </param>
	public ModShimmer AddModItemResult<T>(int stack) where T : ModItem
		=> AddResult(ModShimmerTypeID.Item, ModContent.ItemType<T>(), stack);

	/// <inheritdoc cref=" AddResult(ModShimmerResult)"/>
	/// <param name="count"> The amount of NPCs to be added </param>
	public ModShimmer AddModNPCResult<T>(int count) where T : ModNPC
		=> AddResult(ModShimmerTypeID.NPC, ModContent.NPCType<T>(), count);

	/// <inheritdoc cref=" AddResult(ModShimmerResult)"/>
	/// <param name="coinLuck"> The amount of NPCs to be added </param>
	public ModShimmer AddCoinLuckResult(int coinLuck)
		=> AddResult(ModShimmerTypeID.CoinLuck, -1, coinLuck);

	#endregion AddResultMethods

	/// <inheritdoc cref="IgnoreVanillaItemConstraints"/>
	public ModShimmer DisableVanillaItemConstraints()
	{
		IgnoreVanillaItemConstraints = true;
		return this;
	}

	public ModShimmer SetAsAdditive()
	{
		Additive = true;
		return this;
	}

	/// <summary>
	/// Adds a delegate to <see cref="CanShimmerCallBacks"/> that will be called if the shimmer transformation succeeds
	/// </summary>
	/// <param name="callBack"> The delegate to call </param>
	public ModShimmer AddCanShimmerCallBack(CanShimmerCallBack callBack)
	{
		CanShimmerCallBacks += callBack;
		return this;
	}

	/// <summary>
	/// Adds a delegate to <see cref="OnShimmerCallBacks"/> that will be called if the shimmer transformation succeeds
	/// </summary>
	/// <param name="callBack"> The delegate to call </param>
	public ModShimmer AddOnShimmerCallBack(OnShimmerCallBack callBack)
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
		Register(InstantiationEntity.Value.Item1, InstantiationEntity.Value.Item2);
	}

	/// <summary>
	/// Finalizes transformation, adds to <see cref="ModShimmerTransformations"/>
	/// </summary>
	public void Register(ModShimmerTypeID modShimmerType, int type)
	{
		if (!modShimmerType.IsValidSourceType())
			throw new ArgumentException("A valid source type for ModShimmerTypeID must be passed here", nameof(modShimmerType));
		if (!ModShimmerTransformations.TryAdd((modShimmerType, type), new() { this })) //Try add a new entry for the tuple
			ModShimmerTransformations[(modShimmerType, type)].Add(this); // If it fails, entry exists, therefore add to list
	}

	#endregion ControllerMethods

	#region Operation

	/// <summary>
	/// Checks if the entity supplied can undergo a shimmer transformation, should not alter game state / read only
	/// </summary>
	/// <param name="entity">The entity being shimmered</param>
	/// <returns> true if the following are all true in order
	/// <list type="number">
	/// <item/> All <see cref="Conditions"/> return true
	/// <item/> All added <see cref="CanShimmerCallBack"/> return true
	/// <item/> None of the results contain bone or lihzahrd brick if <see cref="IgnoreVanillaItemConstraints"/> is false (default)
	/// </list>
	/// </returns>
	public bool CanShimmer(Entity entity)
		=> Conditions.All((condition) => condition.IsMet())
		&& (CanShimmerCallBacks?.Invoke(this, entity) ?? true)
		&& (IgnoreVanillaItemConstraints || !Results.Any((result) => result.ResultType == ModShimmerTypeID.Item && (result.Type == ItemID.Bone || result.Type == ItemID.LihzahrdBrick)));

	/// <inheritdoc cref="DoModShimmer(Entity, ValueTuple{ModShimmerTypeID, int})"/>
	/// <param name="npc"> The <see cref="NPC"/> to be shimmered </param>
	public static bool? DoModShimmer(NPC npc) => npc.SpawnedFromStatue
			? SafeIndexDictionary.Instance[npc.type] ? null : false
			: DoModShimmer(npc, (ModShimmerTypeID.NPC, npc.type));

	/// <inheritdoc cref="DoModShimmer(Entity, ValueTuple{ModShimmerTypeID, int})"/>
	/// <param name="item"> The <see cref="Item"/> to be shimmered </param>
	public static bool? DoModShimmer(Item item) => DoModShimmer(item, (ModShimmerTypeID.Item, item.type));

	/// <inheritdoc cref="DoModShimmer(Entity, ValueTuple{ModShimmerTypeID, int})"/>
	/// <param name="projectile"> The <see cref="Projectile"/> to be shimmered </param>
	public static bool? DoModShimmer(Projectile projectile) => DoModShimmer(projectile, (ModShimmerTypeID.Projectile, projectile.type));

	/// <summary>
	/// Shimmers the entity passed, do not call on multiplayer clients
	/// </summary>
	/// <param name="entity"> The <see cref="Entity"/> to be shimmered </param>
	/// <param name="entityIdentification"> tag required information not included in <see cref="Entity"/> </param>
	/// <returns> True if the transformation is successful, false if it is should fall through to vanilla as normal, and null if it should fall through ignoring if the NPC is a statue </returns>
	private static bool? DoModShimmer(Entity entity, (ModShimmerTypeID, int) entityIdentification)
	{
		List<ModShimmer> transformations = ModShimmerTransformations.GetValueOrDefault(entityIdentification);
		if (!(transformations?.Count > 0))
			return false;

		foreach (ModShimmer transformation in transformations) { // Loops possible transformations
			if (transformation.Results.Count > 0 && transformation.CanShimmer(entity)) { // Checks conditions and callback in CanShimmer
				SpawnModShimmerResults(entity, transformation);
				CleanupShimmerSource(entityIdentification.Item1, entity);
				ShimmerEffect(entity.Center);
				return true;
			}
		}
		return false;
	}

	private static void SpawnModShimmerResults(Entity entity, ModShimmer transformation)
	{
		List<Entity> spawnedEntities = new(); // List to be passed for onShimmerCallBacks
		int resultSpawnCounter = 0; //Used to offset spawned stuff

		foreach (ModShimmerResult result in transformation.Results)
			SpawnModShimmerResult(entity, result, ref resultSpawnCounter, ref spawnedEntities); //Spawns the individual result, adds it to the list

		transformation.OnShimmerCallBacks?.Invoke(transformation, entity, spawnedEntities);
	}

	private static void SpawnModShimmerResult(Entity entity, ModShimmerResult shimmerResult, ref int resultIndex, ref List<Entity> spawned)
	{
		int stackCounter = shimmerResult.Count;

		switch (shimmerResult.ResultType) {
			case ModShimmerTypeID.Item: {
				while (stackCounter > 0) { // Since DoShimmer excludes multiplayer, we're in server or single player code here
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
					if (Main.netMode != NetmodeID.MultiplayerClient) { // Else use the custom stuff and avoid spawning on multiplayer
						NPC newNPC = NPC.NewNPCDirect(entity.GetSource_Misc(ItemSourceID.ToContextString(ItemSourceID.Shimmer)), (int)entity.position.X, (int)entity.position.Y, shimmerResult.Type); //Should cause net update stuff

						//syncing up some values that vanilla intentionally sets after SetDefaults() is NPC transformations, mostly self explanatory
						if (entity is NPC && shimmerResult.KeepVanillaTransformationConventions) {
							newNPC.extraValue = (entity as NPC).extraValue;
							newNPC.CopyInteractions((entity as NPC));
							newNPC.spriteDirection = (entity as NPC).spriteDirection;
							newNPC.shimmerTransparency = (entity as NPC).shimmerTransparency;

							if ((entity as NPC).value == 0f)
								newNPC.value = 0f;
							for (int j = 0; j < NPC.maxBuffs; j++) {
								newNPC.buffType[j] = (entity as NPC).buffType[j];
								newNPC.buffTime[j] = (entity as NPC).buffTime[j];
							}
						}
						else {
							newNPC.shimmerTransparency = 1f;
						}
						newNPC.velocity = entity.velocity;
						newNPC.TargetClosest();
						spawned.Add(newNPC);

						if (Main.netMode == 2) {
							NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, newNPC.whoAmI);
							NetMessage.SendData(MessageID.NPCBuffs, -1, -1, null, newNPC.whoAmI);
							newNPC.netUpdate = true;
						}
						resultIndex++;
					}
				}

				break;
			}

			case ModShimmerTypeID.CoinLuck: // Make sure to check this works right, if you're reading this while reviewing please remind me bc I def will forget
				Main.player[Main.myPlayer].AddCoinLuck(entity.Center, stackCounter * shimmerResult.Count);
				NetMessage.SendData(MessageID.ShimmerActions, -1, -1, null, 1, (int)entity.Center.X, (int)entity.Center.Y, stackCounter * shimmerResult.Count);
				break;

			case ModShimmerTypeID.Custom:
				resultIndex += shimmerResult.Count;
				break;
		}
	}

	private static void CleanupShimmerSource(ModShimmerTypeID modShimmerTypeID, Entity entity)
	{
		switch (modShimmerTypeID) {
			case ModShimmerTypeID.NPC:
				CleanupShimmerSource(entity as NPC);
				break;

			case ModShimmerTypeID.Item:
				CleanupShimmerSource(entity as Item);
				break;

			case ModShimmerTypeID.Projectile:
				CleanupShimmerSource(entity as Projectile);
				break;
		}
	}

	private static void CleanupShimmerSource(NPC npc)
	{
		npc.active = false; // despawn this NPC
		if (Main.netMode == 2) {
			npc.netSkip = -1;
			npc.life = 0;
			NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
		}
	}

	private static void CleanupShimmerSource(Item item)
	{
		item.shimmerTime = 0f;
		if (Main.netMode == NetmodeID.Server)
			NetMessage.SendData(MessageID.SyncItemsWithShimmer, -1, -1, null, item.whoAmI, 1f);
		item.makeNPC = -1;
		item.active = false;
	}

	private static void CleanupShimmerSource(Projectile projectile)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Creates the shimmer effect, net syncs
	/// </summary>
	/// <param name="position"> The position to create the effect </param>
	public static void ShimmerEffect(Vector2 position)
	{
		if (Main.netMode == 0)
			Item.ShimmerEffect(position);
		else if (Main.netMode == NetmodeID.Server)
			NetMessage.SendData(MessageID.ShimmerActions, -1, -1, null, 0, (int)position.X, (int)position.Y);
	}

	#endregion Operation
}

/// <summary>
/// Value used by <see cref="ModShimmerResult"/> to identify what type of entity to spawn. <br/>
/// The <see cref="Custom"/> value simply sets the shimmer as successful, spawns nothing, for if you desire entirely custom behavior to be defined in <see cref="ModShimmer.AddOnShimmerCallBack(ModShimmer.OnShimmerCallBack)"/> but do not want to include the item or NPC spawn that would usually count as a "successful" transformation
/// </summary>
public enum ModShimmerTypeID
{
	NPC, //Spawner Spawned
	Item, // Spawner Spawned
	Projectile, // None, might be added later
	CoinLuck, // Spawned Type
	Custom, // Spawned Type
	Null, // None
}

public static class ModShimmerTypeIDExtensions
{
	public static bool IsValidSourceType(this ModShimmerTypeID id)
	=> id == ModShimmerTypeID.NPC || id == ModShimmerTypeID.Item;

	public static bool IsValidSpawnedType(this ModShimmerTypeID id)
		=> id == ModShimmerTypeID.NPC || id == ModShimmerTypeID.Item || id == ModShimmerTypeID.CoinLuck || id == ModShimmerTypeID.Custom;
}

/// <summary>
/// A record representing the information to spawn and entity during a shimmer transformation
/// </summary>
/// <param name="ResultType"> The type of shimmer operation this represents </param>
/// <param name="Type"> The type of the entity to spawn, ignored when <paramref name="ResultType"/> is <see cref="ModShimmerTypeID.CoinLuck"/> or <see cref="ModShimmerTypeID.Custom"/> </param>
/// <param name="Count"> The number of this entity to spawn, if <paramref name="ResultType"/> is <see cref="ModShimmerTypeID.CoinLuck"/> this is the coin luck value, if custom, set this to the expected amount of physical entities spawned </param>
/// <param name="KeepVanillaTransformationConventions"> Keeps <see cref="ModShimmer"/> roughly in line with vanilla as far as base functionality goes, e.g. NPC's spawned via statues stay spawned from a statue when shimmered, if you don't know you need it, leave it true </param>
public record struct ModShimmerResult(ModShimmerTypeID ResultType, int Type, int Count, bool KeepVanillaTransformationConventions)
{
	/// <inheritdoc cref="ModShimmerResult"/>
	/// <param name="ResultType"> The type of shimmer operation this represents </param>
	/// <param name="Type"> The type of the entity to spawn, ignored when <paramref name="ResultType"/> is <see cref="ModShimmerTypeID.CoinLuck"/> or <see cref="ModShimmerTypeID.Custom"/> </param>
	/// <param name="Count"> The number of this entity to spawn, if <paramref name="ResultType"/> is <see cref="ModShimmerTypeID.CoinLuck"/> this is the coin luck value, if <see cref="ModShimmerTypeID.Custom"/>, set this to the expected amount of physical entities spawned as it affects the way the spawned entities are spread </param>
	public ModShimmerResult(ModShimmerTypeID ResultType, int Type, int Count) : this(ResultType, Type, Count, true) { }
}