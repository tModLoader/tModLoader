using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Achievements;
using Terraria.GameContent.Achievements;
using Terraria.Localization;

namespace Terraria.ModLoader;

/// <summary>
/// Define a custom achievement and implement how it should act upon completion
/// </summary>
public abstract class ModAchievement : ModType<Achievement, ModAchievement>, ILocalizedModType
{
    public Achievement Achievement => Entity;

    private string TextureName => (GetType().Namespace + "." + Name).Replace('.', '/');//GetType().FullName.Replace('.', '/');
    public Asset<Texture2D> Texture { get; private set; }
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
	    SetStaticDefaults();
    }

    public override void Load()
    {
    }

    public override void Unload()
    {
        Main.Achievements.Unregister(Achievement);
        Achievement.OnCompleted -= OnCompleted;
    }

    /// <summary>
    /// Called when the achievement is completed.
    /// Override this to add custom behavior when the achievement is achieved.
    /// </summary>
    /// <param name="achievement">The achievement that was completed.</param>
    public virtual void OnCompleted(Achievement achievement)
    {
        // Override in derived classes to add custom behavior.
    }

    public override sealed void SetupContent()
    {
		if (Achievement.ModAchievement != null) {
			if (Achievement.ModAchievement.Texture == null) {
				throw new Exception($"{Achievement.Name}.png was not found, add it in the same directory as your source file.");
			}
		}

		Main.Achievements.Register(Achievement);
    }

    /// <summary>
    /// Should the achievement be hidden, defaults to false.
    /// </summary>
    public bool AchievementHidden = false;

    protected override sealed Achievement CreateTemplateEntity()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new InvalidOperationException("Achievement name cannot be null or empty during template creation.");
        }

		return new Achievement($"{Mod.Name.ToUpper()}_{FullName.ToUpper()}", this);
    }
}
