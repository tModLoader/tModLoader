using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Skies;

// An example usage of ExampleSky through a ModSceneEffect
public class ExampleSkyScene : ModSceneEffect
{
    public override bool IsSceneEffectActive(Player player) => NPC.AnyNPCs(NPCID.SkeletronPrime) && NPC.AnyNPCs(NPCID.SkeletronHead);

    public override void SpecialVisuals(Player player, bool isActive)
    {
        // Toggle activates the sky if the given condition is true and deactivates it otherwise. 
        // It also checks if the sky is already active / inactive to not repeatedly call the activation / deactivation methods.
        SkyManager.Instance.Toggle<ExampleSky>(isActive);
    }
}