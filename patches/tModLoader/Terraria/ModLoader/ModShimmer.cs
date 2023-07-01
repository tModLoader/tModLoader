using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace Terraria.ModLoader;

// TML: #AdvancedShimmerTransformations
public sealed class ShimmerTransformation
{
	public static List<ShimmerTransformation[]> ModShimmerSet { get; private set; } = new();

	public ShimmerTransformation(NPC npc) : this(new ModShimmerSource(ModShimmerTypeID.NPC, npc.type))
	{ }

	public ShimmerTransformation(Item item) : this(new ModShimmerSource(ModShimmerTypeID.Item, item.type))
	{ }

	public ShimmerTransformation() : this(new ModShimmerSource(ModShimmerTypeID.Null, -1))
	{ }

	public ShimmerTransformation(ModShimmerSource shimmerSource)
	{
		Source = shimmerSource;
	}

	/// <summary>
	/// Every condition must be true for the transformation to occur
	/// </summary>
	public List<Condition> Conditions { get; private set; } = new();

	/// <summary>
	/// The entities that the transformation produces.
	/// </summary>
	public List<ModShimmerResult> Results { get; private set; } = new();

	public ModShimmerSource Source { get; private set; }
	public bool TreatAsVanilla { get; private set; }

	/// <summary>
	/// Disallows transformation if a transformation result includes either a bone or a solar tablet fragment, when skeletron or golem are undefeated
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
	/// <item/> Requires the type of the transformation origin to be set via <see cref="SetAsNPCTransformation"/> or <see cref="SetAsItemTransformation"/>, this is already done if instantiated from an instance of <see cref="ModNPC"/> or <see cref="ModItem"/>
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

	public ShimmerTransformation SetAsNPCTransformation()
	{
		Source = new(ModShimmerTypeID.NPC, Source.Type);
		return this;
	}

	public ShimmerTransformation SetAsItemTransformation()
	{
		Source = new(ModShimmerTypeID.Item, Source.Type);
		return this;
	}

	/// <summary>
	///	Called in addition to conditions to check if the entity shimmers
	/// </summary>
	/// <param name="origin"> The entity that was shimmered, either an <see cref="Item"/> or an <see cref="NPC"/></param>
	public delegate bool CanShimmerCallBack(Entity origin);

	internal CanShimmerCallBack CanShimmerCallBacks;

	public ShimmerTransformation AddCanShimmerCallBack(CanShimmerCallBack callBack)
	{
		CanShimmerCallBacks += callBack;
		return this;
	}

	/// <summary>
	/// By default just checks the conditions, when overriding in a derived class remember that you will have to check base.CanShimmer in order to preserve this behavior
	/// </summary>
	/// <returns>True if the entity should proceed to transform</returns>
	public bool CanShimmer(Entity entity) => Conditions.All((condition) => condition.IsMet()) && (CanShimmerCallBacks?.Invoke(entity) ?? true) && (!CheckVanillaConstraints || !Results.Any((result) => result.ResultType == ModShimmerTypeID.Item && (result.Type == 154 || result.Type == 1101)));

	/// <summary>
	///	Called when the entity shimmers
	/// </summary>
	/// <param name="spawnedEntities"> A list of the spawned Entities </param>
	/// <param name="origin"> The entity that was shimmered, either an <see cref="Item"/> or an <see cref="NPC"/></param>
	public delegate void OnShimmerCallBack(Entity origin, List<Entity> spawnedEntities);

	internal OnShimmerCallBack OnShimmerCallBacks;

	public ShimmerTransformation AddOnShimmerCallBack(OnShimmerCallBack callBack)
	{
		OnShimmerCallBacks += callBack;
		return this;
	}

	public void Register()
	{
		if (TreatAsVanilla) {
			switch (Source.EntityType) {
				case ModShimmerTypeID.NPC:
					if (Results.Count != 1)
						throw new InvalidOperationException("Using TreatAsVannila with an NPC requires there to be exactly 1 result");
					if (Results[0].Count != 1)
						throw new InvalidOperationException("Using TreatAsVannila with an NPC requires the result to have a count of exactly 1");
					if (Results[0].ResultType == ModShimmerTypeID.Custom)
						throw new InvalidOperationException("Using TreatAsVannila with an NPC requires its result not be Custom");
					break;

				case ModShimmerTypeID.Item:
					if (Results.Any(result => result.ResultType != ModShimmerTypeID.Item))
						throw new InvalidOperationException("Using TreatAsVannila with an Item requires all results have a type of Item");
					break;

				default:
					throw new InvalidOperationException("Using TreatAsVannila requires the type of the transformation origin to be set via SetAsNPCTransformation() or SetAsItemTransformation()");
			};
		}

		if (ShimmerAdvancedTransformIndex()[Source.Type] >= 0) {
			ModShimmerSet[ShimmerAdvancedTransformIndex()[Source.Type]] = ModShimmerSet[ShimmerAdvancedTransformIndex()[Source.Type]].Append(this).ToArray();
		}
		else {
			ModShimmerSet.Add(new ShimmerTransformation[] { this });
			ShimmerAdvancedTransformIndex()[Source.Type] = ModShimmerSet.Count - 1;
		}
	}

	private int[] ShimmerAdvancedTransformIndex() => ShimmerAdvancedTransformIndex(Source.EntityType);

	private static int[] ShimmerAdvancedTransformIndex(ModShimmerTypeID shimmerTypeID)
	{
		return shimmerTypeID switch {
			ModShimmerTypeID.NPC => NPCID.Sets.ShimmerAdvancedTransformIndex,
			ModShimmerTypeID.Item => ItemID.Sets.ShimmerAdvancedTransformIndex,
			_ => throw new NotImplementedException()
		};
	}

	public static bool DoModShimmer(NPC npc) => DoModShimmer(npc, new(ModShimmerTypeID.NPC, npc.type));

	public static bool DoModShimmer(Item item) => DoModShimmer(item, new(ModShimmerTypeID.Item, item.type));

	public static bool DoModShimmer(Projectile projectile) => DoModShimmer(projectile, new(ModShimmerTypeID.Projectile, projectile.type));

	private static bool DoModShimmer(Entity entity, ModShimmerSource shimmerSource)
	{
		if (ShimmerAdvancedTransformIndex(shimmerSource.EntityType)[shimmerSource.Type] < 0)
			return false;

		foreach (ShimmerTransformation transformation in ModShimmerSet[ShimmerAdvancedTransformIndex(ModShimmerTypeID.NPC)[shimmerSource.Type]]) { // Loops possible transformations
			if (TryModShimmerTransformation(entity, transformation)) {
				CleanupShimmerSource(shimmerSource.EntityType, entity);
				return true;
			}
		}
		return false;
	}

	private static bool TryModShimmerTransformation(Entity entity, ShimmerTransformation transformation)
	{
		if (transformation.Results.Count > 0 && transformation.CanShimmer(entity)) { // Checks conditions and callback in CanShimmer
			List<Entity> spawnedEntities = new();

			for (int resultIndex = 0; resultIndex < transformation.Results.Count; resultIndex++)
				SpawnModShimmerResult(entity, transformation.Results[resultIndex], resultIndex);

			transformation.OnShimmerCallBacks?.Invoke(entity, spawnedEntities);

			if (Main.netMode == 0)
				Item.ShimmerEffect(entity.Center);
			else
				NetMessage.SendData(146, -1, -1, null, 0, (int)entity.Center.X, (int)entity.Center.Y); //Shimmer effect net side

			return true;
		}
		return false;
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

/// <summary>
/// A record representing the type information of an entity that will transform in shimmer
/// </summary>
/// <param name="EntityType"> The type of entity to shimmer </param>
/// <param name="Type"> The type of the entity to shimmer </param>
public sealed record ModShimmerSource(ModShimmerTypeID EntityType, int Type);