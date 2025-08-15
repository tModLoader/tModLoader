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
/// </summary> 
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

	public override sealed bool IsCloneable => false;

	/// <summary>
	/// Gets the localized friendly name of the achievement.
	/// </summary>
	public virtual LocalizedText FriendlyName => this.GetLocalization(nameof(FriendlyName), PrettyPrintName);

	/// <summary>
	/// Gets the localized description of the achievement.
	/// </summary>
	public virtual LocalizedText Description => this.GetLocalization(nameof(Description));

	public CustomFlagCondition AddCondition(string key = "Condition") => AddCondition(new CustomFlagCondition(key));

	public CustomIntCondition AddIntCondition(int maxValue) => AddCondition(new CustomIntCondition("Condition", maxValue));

	public CustomIntCondition AddIntCondition(string key, int maxValue) => AddCondition(new CustomIntCondition(key, maxValue));

	public CustomFloatCondition AddFloatCondition(float maxValue) => AddCondition(new CustomFloatCondition("Condition", maxValue));

	public CustomFloatCondition AddFloatCondition(string key, float maxValue) => AddCondition(new CustomFloatCondition(key, maxValue));

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
	/// Called when the achievement is completed.
	/// Override this to add custom behavior when the achievement is achieved.
	/// </summary>
	/// <param name="achievement">The achievement that was completed.</param>
	public virtual void OnCompleted(Achievement achievement)
	{
	}

	public override sealed void SetupContent()
	{
		SetStaticDefaults();
		if (Achievement._conditions.Count == 0)
			throw new Exception($"The ModAchievement '{Name}' has no conditions, achievements must have at least one condition.");
		AutoStaticDefaults();
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
}
