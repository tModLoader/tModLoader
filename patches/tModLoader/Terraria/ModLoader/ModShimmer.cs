using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace Terraria.ModLoader;

// TML: #AdvancedShimmerTransformations
public sealed class ShimmerTransformation
{
	public static Dictionary<(ModShimmerTypeID, int), List<ShimmerTransformation>> ModShimmerTransformations { get; private set; } = new();

	public ShimmerTransformation(NPC npc) : this((ModShimmerTypeID.NPC, npc.type))
	{ }

	public ShimmerTransformation(Item item) : this((ModShimmerTypeID.Item, item.type))
	{ }

	private ShimmerTransformation((ModShimmerTypeID, int) entityIdentification)
	{
		instantiatorEntity = entityIdentification;
	}

	public ShimmerTransformation()
	{
	}

	/// <summary>
	/// Every condition must be true for the transformation to occur
	/// </summary>
	public List<Condition> Conditions { get; private set; } = new();

	/// <summary>
	/// The entities that the transformation produces.
	/// </summary>
	public List<ModShimmerResult> Results { get; private set; } = new();

	private (ModShimmerTypeID, int)? instantiatorEntity;

	/// <summary>
	/// Disallows if a transformation result includes either a bone or a solar tablet fragment, when skeletron or golem are undefeated respectively
	/// </summary>
	public bool CheckVanillaConstraints { get; private set; } = true;

	/// <summary>
	/// Adds a condition to <see cref="Conditions"/>
	/// </summary>
	/// <param name="condition"> The condition to be added </param>
	public ShimmerTransformation AddCondition(Condition condition)
	{
		Conditions.Add(condition);
		return this;
	}

	/// <summary>
	/// Adds a result to <see cref="Results"/>
	/// </summary>
	/// <param name="result"> The result to be added </param>
	public ShimmerTransformation AddResult(ModShimmerResult result)
	{
		Results.Add(result);
		return this;
	}

	//public ShimmerTransformation SetTransformationSourceType(ModShimmerTypeID modShimmerTypeID)
	//{
	//	Source = new(modShimmerTypeID, Source.Type);
	//	return this;
	//}

	/// <summary>
	///	Called in addition to conditions to check if the entity shimmers
	/// </summary>
	/// <param name="source"> The entity that was shimmered, either an <see cref="Item"/> or an <see cref="NPC"/></param>
	public delegate bool CanShimmerCallBack(Entity source);

	internal CanShimmerCallBack CanShimmerCallBacks;

	public ShimmerTransformation AddCanShimmerCallBack(CanShimmerCallBack callBack)
	{
		CanShimmerCallBacks += callBack;
		return this;
	}

	/// <summary>
	/// Checks if the entity supplied can undergo a shimmer transformation
	/// </summary>
	/// <param name="entity">The entity being shimmered</param>
	/// <returns> true if:
	/// <list type="bullet">
	/// <item/> All <see cref="Conditions"/> return true
	/// <item/> All <see cref="CanShimmerCallBacks"/> return true
	/// <item/> All of the results do not contain bone or solar tablet fragments if <see cref="CheckVanillaConstraints"/> is true
	/// </list>
	/// </returns>
	private bool CanShimmer(Entity entity)
		=> Conditions.All((condition) => condition.IsMet())
		&& (CanShimmerCallBacks?.Invoke(entity) ?? true)
		&& (!CheckVanillaConstraints || !Results.Any((result) => result.ResultType == ModShimmerTypeID.Item && (result.Type == 154 || result.Type == 1101)));

	/// <summary>
	///	Called when the entity shimmers
	/// </summary>
	/// <param name="spawnedEntities"> A list of the spawned Entities </param>
	/// <param name="source"> The entity that was shimmered </param>
	public delegate void OnShimmerCallBack(Entity source, List<Entity> spawnedEntities);

	internal OnShimmerCallBack OnShimmerCallBacks;

	public ShimmerTransformation AddOnShimmerCallBack(OnShimmerCallBack callBack)
	{
		OnShimmerCallBacks += callBack;
		return this;
	}

	public void Register()
	{
		if (instantiatorEntity == null)
			throw new InvalidOperationException("The Register() function must be passed a ModShimmerTypeID and an integer type for an entity if the transformation is not instantiated from a ModNPC/ModItem/ModProjectile type.");
		Register(instantiatorEntity.Value.Item1, instantiatorEntity.Value.Item2);
	}

	public void Register(ModShimmerTypeID modShimmerType, int type)
	{
		if (!ModShimmerTransformations.TryAdd((modShimmerType, type), new() { this })) //Try add a new entry for the tuple
			ModShimmerTransformations[(modShimmerType, type)].Add(this); // If it fails, entry exists, therefore add to list
	}

	/// <inheritdoc cref="DoModShimmer(Entity, ValueTuple{ModShimmerTypeID, int})"/>
	/// <param name="npc"> The <see cref="NPC"/> to be shimmered </param>
	public static bool DoModShimmer(NPC npc) => DoModShimmer(npc, (ModShimmerTypeID.NPC, npc.type));

	/// <inheritdoc cref="DoModShimmer(Entity, ValueTuple{ModShimmerTypeID, int})"/>
	/// <param name="item"> The <see cref="Item"/> to be shimmered </param>
	public static bool DoModShimmer(Item item) => DoModShimmer(item, (ModShimmerTypeID.Item, item.type));

	/// <inheritdoc cref="DoModShimmer(Entity, ValueTuple{ModShimmerTypeID, int})"/>
	/// <param name="projectile"> The <see cref="Projectile"/> to be shimmered </param>
	public static bool DoModShimmer(Projectile projectile) => DoModShimmer(projectile, (ModShimmerTypeID.Projectile, projectile.type));

	/// <summary>
	/// Shimmers the entity passed, do not call on multiplayer clients
	/// </summary>
	/// <param name="entity"> The <see cref="Entity"/> to be shimmered </param>
	/// <param name="entityIdentification"> tag required information not included in <see cref="Entity"/> </param>
	/// <returns> True if the transformation is successful</returns>
	private static bool DoModShimmer(Entity entity, (ModShimmerTypeID, int) entityIdentification)
	{
		List<ShimmerTransformation> transformations = ModShimmerTransformations.GetValueOrDefault(entityIdentification);
		if (transformations.Count == 0)
			return false;

		foreach (ShimmerTransformation transformation in transformations) { // Loops possible transformations
			if (transformation.Results.Count > 0 && transformation.CanShimmer(entity)) { // Checks conditions and callback in CanShimmer
				SpawnModShimmerResults(entity, transformation);
				CleanupShimmerSource(entityIdentification.Item1, entity);
				return true;
			}
		}
		return false;
	}

	private static void SpawnModShimmerResults(Entity entity, ShimmerTransformation transformation)
	{
		List<Entity> spawnedEntities = new(); // List to be passed for onShimmerCallBacks
		int resultSpawnCounter = 0; //Used to offset spawned stuff

		foreach (ModShimmerResult result in transformation.Results)
			SpawnModShimmerResult(entity, result, ref resultSpawnCounter, spawnedEntities); //Spawns the individual result, adds it to the list

		transformation.OnShimmerCallBacks?.Invoke(entity, spawnedEntities);

		if (Main.netMode == 0)
			Item.ShimmerEffect(entity.Center);
		else
			NetMessage.SendData(MessageID.ShimmerActions, -1, -1, null, 0, (int)entity.Center.X, (int)entity.Center.Y);
	}

	private static void SpawnModShimmerResult(Entity entity, ModShimmerResult shimmerResult, ref int resultIndex, List<Entity> spawned)
	{
		int stackCounter = 1;
		if (entity is Item) {
			stackCounter = (entity as Item).stack;
		}

		switch (shimmerResult.ResultType) {
			case ModShimmerTypeID.Item: {
				while (stackCounter > 0) { // Since DoShimmer excludes multiplayer, we're in server or single player code here 
					Item item = Main.item[Item.NewItem(entity.GetSource_Misc(context: ItemSourceID.ToContextString(ItemSourceID.Shimmer)), (int)entity.position.X, (int)entity.position.Y, entity.width, entity.height, shimmerResult.Type)];
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
				int spawnCount = Math.Min(NPC.GetAvailableAmountOfNPCsToSpawnUpToSlot(shimmerResult.Count * stackCounter, 200), 50);
				// 200 and 50 are the values vanilla uses for the highest slot to count with and the maximum NPCs to spawn in one transformation set,
				// technically can be violated because multiple NPCs can be put into the same transformation

				for (int i = 0; i < spawnCount; i++) { // Loop spawn NPCs
					if (Main.netMode != NetmodeID.MultiplayerClient) { // Else use the custom stuff and avoid spawning on multiplayer
						NPC newNPC = NPC.NewNPCDirect(entity.GetSource_Misc(context: ItemSourceID.ToContextString(ItemSourceID.Shimmer)), (int)entity.position.X, (int)entity.position.Y, shimmerResult.Type); //Should cause net update stuff

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

			case ModShimmerTypeID.Projectile:
				throw new NotImplementedException();

			case ModShimmerTypeID.CoinLuck: // Make sure to check this works right, if you're reading this while reviewing please remind me bc I def will forget
				Main.player[Main.myPlayer].AddCoinLuck(entity.Center, stackCounter * shimmerResult.Count);
				NetMessage.SendData(MessageID.ShimmerActions, -1, -1, null, 1, (int)entity.Center.X, (int)entity.Center.Y, stackCounter * shimmerResult.Count);
				break;

			case ModShimmerTypeID.Custom:
				resultIndex += shimmerResult.Count;
				break;

			case ModShimmerTypeID.Null:
				throw new ArgumentException("The value for shimmerResult.ResultType should not be ShimmerTypeID.Null at this point, if behavior is being manually added please use ShimmerTypeID.Custom", nameof(shimmerResult));
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
		throw new NotImplementedException();
	}

	private static void CleanupShimmerSource(Projectile projectile)
	{
		throw new NotImplementedException();
	}
}

/// <summary>
/// Value used by <see cref="ModShimmerResult"/> to identify what type of entity to spawn. <br/>
/// The <see cref="Custom"/> value simply sets the shimmer as successful, spawns nothing, for if you desire entirely custom behavior to be defined in <see cref="ShimmerTransformation.AddOnShimmerCallBack(ShimmerTransformation.OnShimmerCallBack)"/> but do not want to include the item or NPC spawn that would usually count as a "successful" transformation
/// </summary>
public enum ModShimmerTypeID
{
	NPC,
	Item,
	Projectile,
	CoinLuck,
	Custom,
	Null,
}

/// <summary>
/// A record representing the information to spawn and entity during a shimmer transformation
/// </summary>
/// <param name="ResultType"> The type of shimmer operation this represents </param>
/// <param name="Type"> The type of the entity to spawn, ignored when <paramref name="ResultType"/> is <see cref="ModShimmerTypeID.CoinLuck"/> or <see cref="ModShimmerTypeID.Custom"/> </param>
/// <param name="Count"> The number of this entity to spawn, if <paramref name="ResultType"/> is <see cref="ModShimmerTypeID.CoinLuck"/> this is the coin luck value, if custom, set this to the expected amount of physical entities spawned </param>
/// <param name="KeepVanillaTransformationConventions"> Keeps <see cref="ShimmerTransformation"/> roughly in line with vanilla as far as base functionality goes, e.g. NPC's spawned via statues stay spawned from a statue when shimmered, if you don't know you need it, leave it true </param>
public record struct ModShimmerResult(ModShimmerTypeID ResultType, int Type, int Count, bool KeepVanillaTransformationConventions)
{
	/// <inheritdoc cref="ModShimmerResult"/>
	public ModShimmerResult() : this(ModShimmerTypeID.Null, -1, -1, false) { }

	/// <inheritdoc cref="ModShimmerResult"/>
	/// <param name="ResultType"> The type of shimmer operation this represents </param>
	/// <param name="Type"> The type of the entity to spawn, ignored when <paramref name="ResultType"/> is <see cref="ModShimmerTypeID.CoinLuck"/> or <see cref="ModShimmerTypeID.Custom"/> </param>
	/// <param name="Count"> The number of this entity to spawn, if <paramref name="ResultType"/> is <see cref="ModShimmerTypeID.CoinLuck"/> this is the coin luck value, if <see cref="ModShimmerTypeID.Custom"/>, set this to the expected amount of physical entities spawned as it affects the way the spawned entities are spread </param>
	public ModShimmerResult(ModShimmerTypeID ResultType, int Type, int Count) : this(ResultType, Type, Count, true) { }
}