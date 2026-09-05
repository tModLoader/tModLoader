using Microsoft.Xna.Framework;
using Terraria.Graphics.Effects;

namespace Terraria.ModLoader;

/// <summary>
/// This class allows you to create a modded sky effect without any boilerplate.<para/>
/// Skies inheriting this are automatically loaded into <see cref="SkyManager"/> and handle common things like an <see cref="Enabled"/> check and a <see cref="FadeOpacity"/> value for fading sky visuals in and out.<para/>
/// The simplest implementation only requires overriding <see cref="CustomSky.Draw"/>
/// </summary>
/// <seealso cref="CustomSky" />
public abstract class ModSky : CustomSky, IModType, ILoadable
{
    ///<summary>
    /// The mod this sky belongs to.
    /// </summary>
    public Mod Mod { get; internal set;  }

    /// <summary>
    /// The internal name of this sky.
    /// </summary>
    public virtual string Name => GetType().Name;

    /// <summary>
    /// The internal name of this sky, including the mod it is from. This is used as the key for the <see cref="SkyManager"/> instance for this sky.
    /// </summary>
    public string FullName => $"{Mod.Name}/{Name}";

    /// <summary>
    /// The <see cref="SkyManager"/> instance of this sky, you may still retrieve the modded instance through <see cref="ModContent.GetInstance"/>.
    /// </summary>
    public CustomSky Instance => SkyManager.Instance[FullName];

    /// <summary>
    /// Whether this sky is enabled or not.<para/>
    /// This is automatically set in <see cref="Activate"/> and <see cref="Deactivate"/> and checked in <see cref="IsActive"/>
    /// </summary>
    public bool Enabled { get; private set; }

    /// <summary>
    /// An automatically handled value between 0-1 for fading the sky's visuals in and out.<para/>
    /// You may override <see cref="FadeRate"/> to alter the speed at which this value is modified, you may also return false in <see cref="ShouldFade"/> to skip the automatic linear fading behavior and replace it with your own. 
    /// </summary>
    public float FadeOpacity { get; set; }

    /// <summary>
    /// The rate at which <see cref="FadeOpacity"/> is modified.
    /// </summary>
    public virtual float FadeRate => 0.01f;

    /// <summary>
    /// Whether <see cref="FadeOpacity"/> should be updated as normal or not. You may return false to provide your own fading logic. 
    /// </summary>
    public virtual bool ShouldFade => true;

    /// <summary>
    /// Called when this sky's logic is updated.
    /// </summary>
    public virtual void OnUpdate(GameTime gameTime)
    {
        
    }

    /// <summary>
    /// Called the moment the sky is activated.
    /// </summary>
    public virtual void OnActivate(Vector2 position, params object[] args)
    {
        
    }
    
    /// <summary>
    /// Called the moment the sky is deactivated.
    /// </summary>
    public virtual void OnDeactivate(params object[] args)
    {
        
    }
    
    /// <summary>
    /// The conditions dictating whether this sky is properly active or not.<para/>
    /// This is not to be used for checking the conditions for activating the sky, instead, use <see cref="SkyManager.Toggle"/> somewhere suitable (such as a <see cref="ModSceneEffect"/>).<para/>
    /// In most cases you don't need to override this.
    /// </summary>
    public override bool IsActive() => Enabled || FadeOpacity > 0f;
    
    public sealed override void Update(GameTime gameTime)
    {
        if (ShouldFade)
            FadeOpacity = MathHelper.Clamp(FadeOpacity + Enabled.ToDirectionInt() * FadeRate, 0, 1);
        
        OnUpdate(gameTime);
    }

    public sealed override void Activate(Vector2 position, params object[] args)
    {
        Enabled = true;
        OnActivate(position, args);
    }

    public sealed override void Deactivate(params object[] args)
    {
        Enabled = false;
        OnDeactivate(args);
    }

    // SkyManager.Reset seems to be unused?
    public sealed override void Reset()
    {
        Enabled = false;
        FadeOpacity = 0;
    }

    public virtual bool IsLoadingEnabled(Mod mod) => true;
    void ILoadable.Load(Mod mod)
    {
        Mod = mod;
        SkyManager.Instance[FullName] = this;
        ModTypeLookup<ModSky>.Register(this);
        
        Load();
    }
    
    public new virtual void Load() { }
    
    public virtual void Unload() { }
}