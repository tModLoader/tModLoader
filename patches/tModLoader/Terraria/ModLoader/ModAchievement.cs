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

    /// <summary>
    /// Achievement type, this is used for keeping track of modded achievements with ModContent.GetModAchievement(int type);
    /// </summary>
    public int Type => Achievement.Type;

    private string TextureName => (GetType().Namespace + "." + Name).Replace('.', '/');//GetType().FullName.Replace('.', '/');
    private Asset<Texture2D> Texture;
    public string LocalizationCategory => "Achievements";

    public override sealed bool IsCloneable => false;

    /// <summary>
    /// Gets the localized friendly name of the achievement.
    /// </summary>
    public virtual LocalizedText FriendlyName => Mod.GetLocalization($"{LocalizationCategory}.{Name}.Name");

    public AchievementCondition GetCondition(string condition) => Achievement.GetCondition(condition);

    /// <summary>
    /// Gets the localized description of the achievement.
    /// </summary>
    public virtual LocalizedText Description => Mod.GetLocalization($"{LocalizationCategory}.{Name}.Description");

    protected override sealed void Register()
    {
	    ModTypeLookup<ModAchievement>.Register(this);

	    if (string.IsNullOrWhiteSpace(Name))
	    {
		    throw new InvalidOperationException("Achievement name cannot be null or empty.");
	    }

	    if (FriendlyName == null)
	    {
		    throw new InvalidOperationException($"FriendlyName for achievement '{Name}' could not be found.");
	    }

	    if (Description == null)
	    {
		    throw new InvalidOperationException($"Description for achievement '{Name}' could not be found.");
	    }

	    Achievement.FriendlyName = FriendlyName;
	    Achievement.Description = Description;
	    Achievement.ModAchievement = this;
	    Texture = ModContent.Request<Texture2D>(TextureName);
	    SetStaticDefaults();
	    AchievementLoader.Register(this);
    }

    public override void Load()
    {
    }

    public override void Unload()
    {
        Main.Achievements.Unregister(Achievement);
        Achievement.OnCompleted -= OnCompleted;
        Texture = null;
	    AchievementLoader.Unregister(this);
        base.Unload();
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
        return new Achievement($"{Mod.Name.ToUpper()}_{Name.ToUpper()}", true);
    }

    public Asset<Texture2D> GetTexture()
    {
	    return Texture;
    }
}


