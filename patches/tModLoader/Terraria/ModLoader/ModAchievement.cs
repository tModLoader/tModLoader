using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Achievements;
using Terraria.GameContent.Achievements;
using Terraria.Localization;

namespace Terraria.ModLoader;

/// <summary>
/// Defines a custom achievement and implements how it should act upon completion. An achievement consists of several parts:
/// <br/><br/> The ModAchievement represents a single achievement, it provides the image (<see cref="Texture"/>), display name (<see cref="FriendlyName"/>), description (<see cref="Description"/>), and category (<see cref="Achievement.SetCategory(AchievementCategory)"/>).
/// <br/><br/> Each achievement also has one or more conditions (<see cref="AchievementCondition"/>). Conditions can be existing conditions, like crafting specific items or killing specific NPCs, or they can be completely custom. If an achievement has multiple conditions, each must use a unique identifier. Once every condition is completed, the achievement itself is completed and <see cref="OnCompleted(Achievement)"/> is called.
/// <br/><br/> And finally, each achievement can have an optional tracker. The tracker is responsible for consolidating all of the conditions and reporting a completion progress value shown in the achievements menu. A tracker will be assigned automatically if not assigned in SetStaticDefaults and if there are multiple conditions or if the sole condition has an associated tracker.
/// <br/><br/> Achievements are not loaded on the server.
/// </summary>
[Autoload(Side = ModSide.Client)]
public abstract class ModAchievement : ModType<Achievement, ModAchievement>, ILocalizedModType
{
	/// <summary> The Achievement object that this ModAchievement controls. </summary>
	public Achievement Achievement => Entity;

	/// <inheritdoc cref="ModTexturedType.Texture"/>
	public virtual string TextureName => (GetType().Namespace + "." + Name).Replace('.', '/');

	/// <summary> The texture loaded from <see cref="TextureName"/>. </summary>
	public Asset<Texture2D> Texture { get; private set; }

	/// <summary>
	/// The index of this achievement within the texture. Additional achievements are placed below on new rows. Can be used to share a achievement texture among multiple achievements. Defaults to 0.
	/// </summary>
	public virtual int Index => 0;

	public string LocalizationCategory => "Achievements";

	/// <summary> Should the achievement be hidden, meaning its name and description will both appear as "???" in the achievements menu. <br/><br/> Defaults to <see langword="false"/>. </summary>
	public virtual bool Hidden => false;

	public override sealed bool IsCloneable => false;

	/// <summary>
	/// Gets the localized friendly name of the achievement.
	/// </summary>
	public virtual LocalizedText FriendlyName => this.GetLocalization(nameof(FriendlyName), PrettyPrintName);

	/// <summary>
	/// Gets the localized description of the achievement.
	/// </summary>
	public virtual LocalizedText Description => this.GetLocalization(nameof(Description));

	/// <inheritdoc cref="CustomFlagCondition"/>
	public CustomFlagCondition AddCondition(string key = "Condition") => AddCondition(new CustomFlagCondition(key));

	/// <inheritdoc cref="CustomIntCondition"/>
	public CustomIntCondition AddIntCondition(int maxValue) => AddCondition(new CustomIntCondition("Condition", maxValue));

	/// <inheritdoc cref="CustomIntCondition"/>
	public CustomIntCondition AddIntCondition(string key, int maxValue) => AddCondition(new CustomIntCondition(key, maxValue));

	/// <inheritdoc cref="CustomFloatCondition"/>
	public CustomFloatCondition AddFloatCondition(float maxValue) => AddCondition(new CustomFloatCondition("Condition", maxValue));

	/// <inheritdoc cref="CustomFloatCondition"/>
	public CustomFloatCondition AddFloatCondition(string key, float maxValue) => AddCondition(new CustomFloatCondition(key, maxValue));

	/// <inheritdoc cref="ItemCraftCondition"/>
	public ItemCraftCondition AddItemCraftCondition(int itemId) => AddCondition(new ItemCraftCondition((short)itemId));

	/// <inheritdoc cref="ItemCraftCondition"/>
	public ItemCraftCondition AddItemCraftCondition(string key, int itemId) => AddCondition(new ItemCraftCondition((short)itemId, key));

	/// <inheritdoc cref="ItemCraftCondition"/>
	public ItemCraftCondition AddItemCraftCondition(int[] itemIds) => AddCondition(new ItemCraftCondition(itemIds.Select(x => (short)x).ToArray()));

	/// <inheritdoc cref="ItemCraftCondition"/>
	public ItemCraftCondition AddItemCraftCondition(string key, int[] itemIds) => AddCondition(new ItemCraftCondition(itemIds.Select(x => (short)x).ToArray(), key));

	/// <summary>
	/// Adds a <see cref="ItemCraftCondition"/> for each <paramref name="itemIds"/> entry. Unlike with passing in several Item IDs to <see cref="AddItemCraftCondition(int[])"/>, this method will require that each Item be crafted rather than just any one of them.
	/// </summary>
	public ItemCraftCondition[] AddManyItemCraftCondition(int[] itemIds) => itemIds.Select(x => AddCondition(new ItemCraftCondition((short)x))).ToArray();

	/// <inheritdoc cref="ItemPickupCondition"/>
	public ItemPickupCondition AddItemPickupCondition(int itemId) => AddCondition(new ItemPickupCondition((short)itemId));

	/// <inheritdoc cref="ItemPickupCondition"/>
	public ItemPickupCondition AddItemPickupCondition(string key, int itemId) => AddCondition(new ItemPickupCondition((short)itemId, key));

	/// <inheritdoc cref="ItemPickupCondition"/>
	public ItemPickupCondition AddItemPickupCondition(int[] itemIds) => AddCondition(new ItemPickupCondition(itemIds.Select(x => (short)x).ToArray()));

	/// <inheritdoc cref="ItemPickupCondition"/>
	public ItemPickupCondition AddItemPickupCondition(string key, int[] itemIds) => AddCondition(new ItemPickupCondition(itemIds.Select(x => (short)x).ToArray(), key));

	/// <summary>
	/// Adds a <see cref="ItemPickupCondition"/> for each <paramref name="itemIds"/> entry. Unlike with passing in several Item IDs to <see cref="AddItemPickupCondition(int[])"/>, this method will require that each Item be picked up rather than just any one of them.
	/// </summary>
	public ItemPickupCondition[] AddManyItemPickupCondition(int[] itemIds) => itemIds.Select(x => AddCondition(new ItemPickupCondition((short)x))).ToArray();

	/// <inheritdoc cref="NPCKilledCondition"/>
	public NPCKilledCondition AddNPCKilledCondition(int npcID) => AddCondition(new NPCKilledCondition((short)npcID));

	/// <inheritdoc cref="NPCKilledCondition"/>
	public NPCKilledCondition AddNPCKilledCondition(string key, int npcID) => AddCondition(new NPCKilledCondition((short)npcID, key));

	/// <inheritdoc cref="NPCKilledCondition"/>
	public NPCKilledCondition AddNPCKilledCondition(int[] npcIDs) => AddCondition(new NPCKilledCondition(npcIDs.Select(x=>(short)x).ToArray()));

	/// <inheritdoc cref="NPCKilledCondition"/>
	public NPCKilledCondition AddNPCKilledCondition(string key, int[] npcIDs) => AddCondition(new NPCKilledCondition(npcIDs.Select(x => (short)x).ToArray(), key));

	/// <summary>
	/// Adds a <see cref="NPCKilledCondition"/> for each <paramref name="npcIDs"/> entry. Unlike with passing in several NPC IDs to <see cref="AddNPCKilledCondition(int[])"/>, this method will require that each NPC be killed rather than just any one of them.
	/// </summary>
	public NPCKilledCondition[] AddManyNPCKilledCondition(int[] npcIDs) => npcIDs.Select(x => AddCondition(new NPCKilledCondition((short)x))).ToArray();

	/// <inheritdoc cref="TileDestroyedCondition"/>
	public TileDestroyedCondition AddTileDestroyedCondition(int[] tileIds) => AddCondition(new TileDestroyedCondition(tileIds.Select(x => (ushort)x).ToArray()));

	/// <inheritdoc cref="TileDestroyedCondition"/>
	public TileDestroyedCondition AddTileDestroyedCondition(string key, int[] tileIds) => AddCondition(new TileDestroyedCondition(tileIds.Select(x => (ushort)x).ToArray(), key));

	/// <summary> Used to add any custom <see cref="AchievementCondition"/>. </summary>
	public T AddCondition<T>(T condition) where T : AchievementCondition
	{
		Achievement.AddCondition(condition);
		return condition;
	}

	protected override sealed void Register()
	{
		if (string.IsNullOrWhiteSpace(Name))
			throw new InvalidOperationException("Achievement name cannot be null or empty.");

		if (FriendlyName == null)
			throw new ArgumentNullException(nameof(FriendlyName));

		if (Description == null)
			throw new InvalidOperationException($"Description for achievement '{Name}' could not be found.");

		ModTypeLookup<ModAchievement>.Register(this);

		Achievement.FriendlyName = FriendlyName;
		Achievement.Description = Description;
		Achievement.ModAchievement = this;
		Texture = ModContent.Request<Texture2D>(TextureName);
	}

	/// <summary>
	/// Called when the achievement is completed. Use this to add custom behavior when the achievement is achieved.
	/// <br/><br/> Note that achievements will only be completed once per user, not per world or per player, so rewarding players with tangible rewards, like an Item, isn't recommended.
	/// </summary>
	/// <param name="achievement">The achievement that was completed.</param>
	public virtual void OnCompleted(Achievement achievement)
	{
	}

	public override sealed void SetupContent()
	{
		Main.Achievements.Register(Achievement);
		Main.Achievements.RegisterIconIndex(Achievement.Name, Index);
		Achievement.OnCompleted += OnCompleted;
	}

	protected override sealed Achievement CreateTemplateEntity()
	{
		if (string.IsNullOrWhiteSpace(Name)) {
			throw new InvalidOperationException("Achievement name cannot be null or empty during template creation.");
		}

		return new Achievement(FullName, this);
	}

	/// <summary>
	/// Automatically assigns an <see cref="IAchievementTracker"/> if not yet assigned. Override this if you need to skip this logic.
	/// </summary>
	public virtual void AutoStaticDefaults()
	{
		if (!Achievement.HasTracker) {
			// There are 3 trackers: ConditionsCompletedTracker, ConditionIntTracker, ConditionFloatTracker. CustomFlagCondition has no associated tracker
			if (Achievement._conditions.Count > 1) {
				Achievement.UseConditionsCompletedTracker();
			}
			else {
				var tracker = Achievement._conditions.First().Value.GetAchievementTracker();
				if (tracker != null)
					Achievement.UseTracker(tracker);
			}
		}
	}

	/// <summary>
	/// Returns the achievement's default position in regard to vanilla's achievement ordering. Make use of e.g. <see cref="Before"/>/<see cref="After"/>, and provide an achievement (for example <c>new After("EYE_ON_YOU")</c>). Consult the <see href="https://github.com/tModLoader/tModLoader/wiki/Vanilla-Content-IDs#achievement-identifiers">Achievement Identifiers section of the Vanilla Content IDs wiki page</see> to look up the string to use with GetAchievement. You can also use <see cref="BeforeFirstVanillaAchievement"/> or <see cref="AfterLastVanillaAchievement"/> to put your achievement at the start/end of the vanilla achievement order.
	/// <br/><br/> <b>NOTE:</b> The position must specify a vanilla <see cref="Achievements.Achievement"/> otherwise an exception will be thrown. Use <see cref="GetModdedConstraints"/> to order modded achievements.
	/// <br/><br/> By default, this hook positions this achievement after all vanilla achievements.
	/// </summary>
	public virtual Position GetDefaultPosition() => AfterLastVanillaAchievement;

	/// <summary>
	/// Modded achievements are placed between vanilla achievements via <see cref="GetDefaultPosition"/> and, by default, are sorted in load order.<br/>
	/// This hook allows you to sort this achievement before/after other modded achievements that were placed between the same two vanilla achievements.<br/>
	/// Example:
	/// <para>
	/// <c>yield return new After(ModContent.GetInstance&lt;MinionBossKilled&gt;());</c>
	/// </para>
	/// By default, this hook returns <see langword="null"/>, which indicates that this achievement has no modded ordering constraints.
	/// </summary>
	public virtual IEnumerable<Position> GetModdedConstraints() => null;

	#region Sort Positions

	public abstract class Position { }

	public static Position BeforeFirstVanillaAchievement => new Before(AchievementManager.FirstVanillaAchievement);
	public static Position AfterLastVanillaAchievement => new After(AchievementManager.LastVanillaAchievement);

	public sealed class Default : Position { }

	public sealed class Before : Position
	{
		public Achievement Achievement { get; }

		public Before(Achievement achievement)
		{
			Achievement = achievement;
		}

		public Before(ModAchievement modAchievement)
		{
			Achievement = modAchievement.Achievement;
		}

		/// <inheritdoc cref="After(string)"/>
		public Before(string achievementIdentifier)
		{
			Achievement = Main.Achievements.GetAchievement(achievementIdentifier);
		}
	}

	public sealed class After : Position
	{
		public Achievement Achievement { get; }

		public After(Achievement achievement)
		{
			Achievement = achievement;
		}

		public After(ModAchievement modAchievement)
		{
			Achievement = modAchievement.Achievement;
		}

		/// <summary>
		/// Consult the <see href="https://github.com/tModLoader/tModLoader/wiki/Vanilla-Content-IDs#achievement-identifiers">Achievement Identifiers section of the Vanilla Content IDs wiki page</see> to look up vanilla achievement identifiers. Modded achievements will be in the form of "ModName/Name".
		/// </summary>
		public After(string achievementIdentifier)
		{
			Achievement = Main.Achievements.GetAchievement(achievementIdentifier);
		}
	}

	#endregion
}
