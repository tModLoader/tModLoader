using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Skies;

// This example shows a simple modded sky.
// ExampleSkyScene.cs shows how this can be used.

// ModSky is a class that wraps over CustomSky and handles the boilerplate automatically.
// For the simplest implementation you only have to override Draw.

// It is important to note that you don't miss out on any freedom by using ModSky over CustomSky, the range of capabilities is identical.
public class ExampleSky : ModSky
{
    // FadeOpacity is a property that automatically shifts between 0 and 1 when a sky is activated / deactivated.
    // This property dictates how much FadeOpacity is modified per frame. By default, it's 0.01f;
    public override float FadeRate => 0.05f;

    // minDepth and maxDepth are somewhat arbitrary values, but they can be used to specify where an element is to be drawn.
    public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
    {
        float opacity = FadeOpacity * 0.5f;
        
        // A minDepth value of 0 or below implies the front-most layer, whatever drawn here will be drawn in-front of all background elements.
        if (minDepth <= 0f)
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth / 2, Main.screenHeight), Color.CornflowerBlue * opacity);
        
        // A maxDepth value this high implies the back-most layer, whatever drawn here will be drawn behind all background elements.
        if (maxDepth >= float.MaxValue)
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Red * opacity);
    }

    // The following hooks can be quite useful for logic unrelated to rendering the sky.
    public override void OnActivate(Vector2 position, params object[] args)
    {
        SoundEngine.PlaySound(SoundID.AbigailUpgrade);
        Main.NewLightning();
    }
    
    public override void OnUpdate(GameTime gameTime)
    {
        if (Main.rand.NextBool(2000))
            Main.NewLightning();
    }

    public override void OnDeactivate(params object[] args)
    {
        SoundEngine.PlaySound(SoundID.AbigailCry);
    }
}