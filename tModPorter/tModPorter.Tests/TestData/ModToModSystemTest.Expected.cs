using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using Terraria.Graphics;
using Terraria.ModLoader;
using Terraria.UI;

// Do not rename Mod to ModSystem, that would cause 0 Mod classes in the mod. Instead, give suggestions
public class ModToModSystemTest : Mod
{
#if COMPILE_ERROR
	public override void UpdateMusic(ref int music)/* tModPorter Suggestion: Use ModSceneEffect.Music and .Priority, aswell as ModSceneEffect.IsSceneEffectActive */ { /* Empty */ }

	public override void UpdateMusic(ref int music, ref MusicPriority priority)/* tModPorter Suggestion: Suggestion: Use ModSceneEffect.Music and .Priority, aswell as ModSceneEffect.IsSceneEffectActive */ { /* Empty */ }

	public override void ModifyTransformMatrix(ref SpriteViewMatrix Transform)/* tModPorter Suggestion: Use ModSystem.ModifyTransformMatrix */ { /* Empty */ }

	public override void UpdateUI(GameTime gameTime)/* tModPorter Suggestion: Use ModSystem.UpdateUI */ { /* Empty */ }

	public override void PreUpdateEntities()/* tModPorter Suggestion: Use ModSystem.PreUpdateEntities */ { /* Empty */ }

	public override void MidUpdateDustTime()/* tModPorter Suggestion: Use ModSystem.PostUpdateDusts or ModSystem.PreUpdateTime */ { /* Empty */ }

	public override void MidUpdateGoreProjectile()/* tModPorter Suggestion: Use ModSystem.PostUpdateGores or ModSystem.PreUpdateProjectiles */ { /* Empty */ }

	public override void MidUpdateInvasionNet()/* tModPorter Suggestion: Use ModSystem.PostUpdateInvasions */ { /* Empty */ }

	public override void MidUpdateItemDust()/* tModPorter Suggestion: Use ModSystem.PostUpdateItems or ModSystem.PreUpdateDusts */ { /* Empty */ }

	public override void MidUpdateNPCGore()/* tModPorter Suggestion: Use ModSystem.PostUpdateNPCs or ModSystem.PreUpdateGores */ { /* Empty */ }

	public override void MidUpdatePlayerNPC()/* tModPorter Suggestion: Use ModSystem.PostUpdatePlayers or ModSystem.PreUpdateNPCs */ { /* Empty */ }

	public override void MidUpdateProjectileItem()/* tModPorter Suggestion: Use ModSystem.PostUpdateProjectiles or ModSystem.PreUpdateItems */ { /* Empty */ }

	public override void MidUpdateTimeWorld()/* tModPorter Suggestion: Use ModSystem.PostUpdateTime */ { /* Empty */ }

	public override void PostUpdateEverything()/* tModPorter Suggestion: Use ModSystem.PostUpdateEverything */ { /* Empty */ }

	public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)/* tModPorter Suggestion: Use ModSystem.ModifyInterfaceLayers */ { /* Empty */ }

	public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)/* tModPorter Suggestion: Use ModSystem.ModifySunLightColor */ { /* Empty */ }

	public override void ModifyLightingBrightness(ref float scale)/* tModPorter Suggestion: Use ModSystem.ModifyLightingBrightness */ { /* Empty */ }

	public override void PostDrawInterface(SpriteBatch spriteBatch)/* tModPorter Suggestion: Use ModSystem.PostDrawInterface */ { /* Empty */ }

	public override void PostDrawFullscreenMap(ref string mouseText)/* tModPorter Suggestion: Use ModSystem.PostDrawFullscreenMap or a ModMapLayer */ { /* Empty */ }

	public override void PostUpdateInput()/* tModPorter Suggestion: Use ModSystem.PostUpdateInput */ { /* Empty */ }

	public override void PreSaveAndQuit()/* tModPorter Suggestion: Use ModSystem.PreSaveAndQuit */ { /* Empty */ }
#endif
}