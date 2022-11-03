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
	public override void UpdateMusic(ref int music)/* tModPorter Note: Removed. Use ModSceneEffect.Music and .Priority, aswell as ModSceneEffect.IsSceneEffectActive */ { /* Empty */ }

	public override void UpdateMusic(ref int music, ref SceneEffectPriority priority)/* tModPorter Note: Removed. Use ModSceneEffect.Music and .Priority, aswell as ModSceneEffect.IsSceneEffectActive */ { /* Empty */ }

	public override void ModifyTransformMatrix(ref SpriteViewMatrix Transform)/* tModPorter Note: Removed. Use ModSystem.ModifyTransformMatrix */ { /* Empty */ }

	public override void UpdateUI(GameTime gameTime)/* tModPorter Note: Removed. Use ModSystem.UpdateUI */ { /* Empty */ }

	public override void PreUpdateEntities()/* tModPorter Note: Removed. Use ModSystem.PreUpdateEntities */ { /* Empty */ }

	public override void MidUpdateDustTime()/* tModPorter Note: Removed. Use ModSystem.PostUpdateDusts or ModSystem.PreUpdateTime */ { /* Empty */ }

	public override void MidUpdateGoreProjectile()/* tModPorter Note: Removed. Use ModSystem.PostUpdateGores or ModSystem.PreUpdateProjectiles */ { /* Empty */ }

	public override void MidUpdateInvasionNet()/* tModPorter Note: Removed. Use ModSystem.PostUpdateInvasions */ { /* Empty */ }

	public override void MidUpdateItemDust()/* tModPorter Note: Removed. Use ModSystem.PostUpdateItems or ModSystem.PreUpdateDusts */ { /* Empty */ }

	public override void MidUpdateNPCGore()/* tModPorter Note: Removed. Use ModSystem.PostUpdateNPCs or ModSystem.PreUpdateGores */ { /* Empty */ }

	public override void MidUpdatePlayerNPC()/* tModPorter Note: Removed. Use ModSystem.PostUpdatePlayers or ModSystem.PreUpdateNPCs */ { /* Empty */ }

	public override void MidUpdateProjectileItem()/* tModPorter Note: Removed. Use ModSystem.PostUpdateProjectiles or ModSystem.PreUpdateItems */ { /* Empty */ }

	public override void MidUpdateTimeWorld()/* tModPorter Note: Removed. Use ModSystem.PostUpdateTime */ { /* Empty */ }

	public override void PostUpdateEverything()/* tModPorter Note: Removed. Use ModSystem.PostUpdateEverything */ { /* Empty */ }

	public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)/* tModPorter Note: Removed. Use ModSystem.ModifyInterfaceLayers */ { /* Empty */ }

	public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)/* tModPorter Note: Removed. Use ModSystem.ModifySunLightColor */ { /* Empty */ }

	public override void ModifyLightingBrightness(ref float scale)/* tModPorter Note: Removed. Use ModSystem.ModifyLightingBrightness */ { /* Empty */ }

	public override void PostDrawInterface(SpriteBatch spriteBatch)/* tModPorter Note: Removed. Use ModSystem.PostDrawInterface */ { /* Empty */ }

	public override void PostDrawFullscreenMap(ref string mouseText)/* tModPorter Note: Removed. Use ModSystem.PostDrawFullscreenMap or a ModMapLayer */ { /* Empty */ }

	public override void PostUpdateInput()/* tModPorter Note: Removed. Use ModSystem.PostUpdateInput */ { /* Empty */ }

	public override void PreSaveAndQuit()/* tModPorter Note: Removed. Use ModSystem.PreSaveAndQuit */ { /* Empty */ }
#endif
}