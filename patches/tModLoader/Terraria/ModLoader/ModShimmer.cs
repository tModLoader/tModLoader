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
	public bool TreatAsVanilla { get; private set; }

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

	/// <summary>
	/// Makes this shimmer operation use the vanilla code for shimmering, restricts shimmer operations to the following constraints:
	/// <list type="bullet">
	/// <item/> Requires the type of the transformation origin to be set via <see cref="SetTransformationSourceType"/> this is already done if instantiated from an instance of <see cref="ModNPC"/>, <see cref="ModItem"/>, or <see cref="ModProjectile"/>
	/// <item/> NPCs can only transform into either one npc or one item
	/// <item/> Items may transform into any combination of other items but can only transform into single npcs and will use release logic to do so
	/// </list>
	/// If these are not met <see cref="Register()"/> will throw an error
	/// </summary>
	public ShimmerTransformation SetTreatAsVanilla()
	{
		TreatAsVanilla = true;
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
		if (TreatAsVanilla) {
			switch (modShimmerType) {
				case ModShimmerTypeID.NPC:
					if (Results.Count != 1)
						throw new InvalidOperationException("Using TreatAsVannila with an NPC requires there to be exactly 1 result");
					if (Results[0].Count != 1)
						throw new InvalidOperationException("Using TreatAsVannila with an NPC requires the result to have a count of exactly 1");
					if (Results[0].ResultType == ModShimmerTypeID.Custom || Results[0].ResultType == ModShimmerTypeID.Projectile || Results[0].ResultType == ModShimmerTypeID.Null)
						throw new InvalidOperationException("Using TreatAsVannila with an NPC requires its result not be custom, null or projectile");
					break;

				case ModShimmerTypeID.Item:
					if (Results.Any(result => result.ResultType != ModShimmerTypeID.Item))
						throw new InvalidOperationException("Using TreatAsVannila with an Item requires all results have a type of Item");
					break;

				default:
					throw new InvalidOperationException("Using TreatAsVannila requires the type of the transformation origin to be set via SetAsNPCTransformation() or SetAsItemTransformation()");
			};
		}

		if (!ModShimmerTransformations.TryAdd((modShimmerType, type), new() { this })) //Try add a new entry for the tuple
			ModShimmerTransformations[(modShimmerType, type)].Add(this); // If it fails, entry exists, therefore add to list
	}

	public static bool DoModShimmer(NPC npc) => DoModShimmer(npc, (ModShimmerTypeID.NPC, npc.type));

	public static bool DoModShimmer(Item item) => DoModShimmer(item, (ModShimmerTypeID.Item, item.type));

	public static bool DoModShimmer(Projectile projectile) => DoModShimmer(projectile, (ModShimmerTypeID.Projectile, projectile.type));

	private static bool DoModShimmer(Entity entity, (ModShimmerTypeID, int) entityIdentification)
	{
		List<ShimmerTransformation> transformations = ModShimmerTransformations.GetValueOrDefault(entityIdentification);
		if (transformations.Count  == 0)
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
		List<Entity> spawnedEntities = new();

		for (int resultIndex = 0; resultIndex < transformation.Results.Count; resultIndex++)
			SpawnModShimmerResult(entity, transformation.Results[resultIndex], resultIndex);

		transformation.OnShimmerCallBacks?.Invoke(entity, spawnedEntities);

		if (Main.netMode == 0)
			Item.ShimmerEffect(entity.Center);
		else
			NetMessage.SendData(146, -1, -1, null, 0, (int)entity.Center.X, (int)entity.Center.Y); //Shimmer effect net side
	}

	private static void SpawnModShimmerResult(Entity entity, ModShimmerResult shimmerResult, int resultIndex)
	{
		switch (shimmerResult.ResultType) {
			case ModShimmerTypeID.Item: {
				int num = Item.NewItem(entity.GetSource_Misc(context: ItemSourceID.ToContextString(ItemSourceID.Shimmer)), (int)entity.position.X, (int)entity.position.Y, entity.width, entity.height, shimmerResult.Type);
				Main.item[num].stack = shimmerResult.Count;
				Main.item[num].shimmerTime = 1f;
				Main.item[num].shimmered = true;
				Main.item[num].shimmerWet = true;
				Main.item[num].wet = true;
				Main.item[num].velocity *= 0.1f;
				Main.item[num].playerIndexTheItemIsReservedFor = Main.myPlayer;
				NetMessage.SendData(145, -1, -1, null, num, 1f);// net sync spawning the item

				//spawnedItems.Add(Main.item[num]); TODO fix with delegate
				break;
			}

			case ModShimmerTypeID.NPC: {
				for (int i = 0; i < shimmerResult.Count; i++) { // Loop spawn NPCs, will more than likely always be only one, but for the sake of completeness this is here
					if (Main.netMode != NetmodeID.MultiplayerClient) { // Else use the custom stuff and avoid spawning on multiplayer
						NPC newNPC = NPC.NewNPCDirect(entity.GetSource_Misc(context: ItemSourceID.ToContextString(ItemSourceID.Shimmer)), (int)entity.position.X, (int)entity.position.Y, shimmerResult.Type); //Should cause net update stuff

						//syncing up some values that vanilla intentionally sets after SetDefaults() is NPC transformations, mostly self explanatory
						if (entity is NPC && shimmerResult.KeepVanillaTransformationConventions) {
							newNPC.extraValue = (entity as NPC).extraValue;
							newNPC.CopyInteractions((entity as NPC));
							newNPC.SpawnedFromStatue = (entity as NPC).SpawnedFromStatue;
							newNPC.spriteDirection = (entity as NPC).spriteDirection;
							newNPC.shimmerTransparency = (entity as NPC).shimmerTransparency;

							if ((entity as NPC).value == 0f)
								newNPC.value = 0f;
							for (int j = 0; j < NPC.maxBuffs; j++) {
								newNPC.buffType[j] = (entity as NPC).buffType[j];
								newNPC.buffTime[j] = (entity as NPC).buffTime[j];
							}
						}

						newNPC.Center = entity.Center + new Vector2(25f * ((1 - shimmerResult.Count) / 2 + i), 0); //Vanilla does not do centering like this, but otherwise multiple NPCs stack
						newNPC.velocity = entity.velocity;

						//spawnedNPCs.Add(newNPC); fix with delegate
						newNPC.TargetClosest();
					}
				}

				break;
			}

			case ModShimmerTypeID.Projectile:
				throw new NotImplementedException();

			case ModShimmerTypeID.Custom:
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
			NetMessage.SendData(23, -1, -1, null, npc.whoAmI);
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
	Custom,
	Null,
}

/// <summary>
/// A record representing the information to spawn and entity during a shimmer transformation
/// </summary>
/// <param name="ResultType"> The type of entity to spawn </param>
/// <param name="Type"> The type of the entity to spawn </param>
/// <param name="Count"> The number of this entity to spawn, if an item it will spawn in a stack </param>
/// <param name="KeepVanillaTransformationConventions"> Keeps <see cref="ShimmerTransformation"/> roughly in line with vanilla as far as base functionality goes, e.g. NPC's spawned via statues stay spawned from a statue when shimmered </param>
public sealed record ModShimmerResult(ModShimmerTypeID ResultType, int Type, int Count, bool KeepVanillaTransformationConventions)
{
	public ModShimmerResult(ModShimmerTypeID ResultType, int Type, int Count) : this(ResultType, Type, Count, true)
	{ }
}