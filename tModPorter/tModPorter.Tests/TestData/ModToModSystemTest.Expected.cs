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
	public override void UpdateMusic(ref int music)/* Suggestion: Use ModSceneEffect.Music and .Priority, aswell as ModSceneEffect.IsSceneEffectActive */ { /* Empty */ }

	public override void UpdateMusic(ref int music, ref MusicPriority priority)/* Suggestion: Suggestion: Use ModSceneEffect.Music and .Priority, aswell as ModSceneEffect.IsSceneEffectActive */ { /* Empty */ }

	public override void ModifyTransformMatrix(ref SpriteViewMatrix Transform)/* Suggestion: Use ModSystem.ModifyTransformMatrix */ { /* Empty */ }

	public override void UpdateUI(GameTime gameTime)/* Suggestion: Use ModSystem.UpdateUI */ { /* Empty */ }

	public override void PreUpdateEntities()/* Suggestion: Use ModSystem.PreUpdateEntities */ { /* Empty */ }

	public override void MidUpdateDustTime()/* Suggestion: Use ModSystem.PostUpdateDusts or ModSystem.PreUpdateTime */ { /* Empty */ }

	public override void MidUpdateGoreProjectile()/* Suggestion: Use ModSystem.PostUpdateGores or ModSystem.PreUpdateProjectiles */ { /* Empty */ }

	public override void MidUpdateInvasionNet()/* Suggestion: Use ModSystem.PostUpdateInvasions */ { /* Empty */ }

	public override void MidUpdateItemDust()/* Suggestion: Use ModSystem.PostUpdateItems or ModSystem.PreUpdateDusts */ { /* Empty */ }

	public override void MidUpdateNPCGore()/* Suggestion: Use ModSystem.PostUpdateNPCs or ModSystem.PreUpdateGores */ { /* Empty */ }

	public override void MidUpdatePlayerNPC()/* Suggestion: Use ModSystem.PostUpdatePlayers or ModSystem.PreUpdateNPCs */ { /* Empty */ }

	public override void MidUpdateProjectileItem()/* Suggestion: Use ModSystem.PostUpdateProjectiles or ModSystem.PreUpdateItems */ { /* Empty */ }

	public override void MidUpdateTimeWorld()/* Suggestion: Use ModSystem.PostUpdateTime */ { /* Empty */ }

	public override void PostUpdateEverything()/* Suggestion: Use ModSystem.PostUpdateEverything */ { /* Empty */ }

	public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)/* Suggestion: Use ModSystem.ModifyInterfaceLayers */ { /* Empty */ }

	public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)/* Suggestion: Use ModSystem.ModifySunLightColor */ { /* Empty */ }

	public override void ModifyLightingBrightness(ref float scale)/* Suggestion: Use ModSystem.ModifyLightingBrightness */ { /* Empty */ }

	public override void PostDrawInterface(SpriteBatch spriteBatch)/* Suggestion: Use ModSystem.PostDrawInterface */ { /* Empty */ }

	public override void PostDrawFullscreenMap(ref string mouseText)/* Suggestion: Use ModSystem.PostDrawFullscreenMap or a ModMapLayer */ { /* Empty */ }

	public override void PostUpdateInput()/* Suggestion: Use ModSystem.PostUpdateInput */ { /* Empty */ }

	public override void PreSaveAndQuit()/* Suggestion: Use ModSystem.PreSaveAndQuit */ { /* Empty */ }
#endif
}