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
	public override void UpdateMusic(ref int music) { /* Empty */ }

	public override void UpdateMusic(ref int music, ref MusicPriority priority) { /* Empty */ }

	public override void ModifyTransformMatrix(ref SpriteViewMatrix Transform) { /* Empty */ }

	public override void UpdateUI(GameTime gameTime) { /* Empty */ }

	public override void PreUpdateEntities() { /* Empty */ }

	public override void MidUpdateDustTime() { /* Empty */ }

	public override void MidUpdateGoreProjectile() { /* Empty */ }

	public override void MidUpdateInvasionNet() { /* Empty */ }

	public override void MidUpdateItemDust() { /* Empty */ }

	public override void MidUpdateNPCGore() { /* Empty */ }

	public override void MidUpdatePlayerNPC() { /* Empty */ }

	public override void MidUpdateProjectileItem() { /* Empty */ }

	public override void MidUpdateTimeWorld() { /* Empty */ }

	public override void PostUpdateEverything() { /* Empty */ }

	public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) { /* Empty */ }

	public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor) { /* Empty */ }

	public override void ModifyLightingBrightness(ref float scale) { /* Empty */ }

	public override void PostDrawInterface(SpriteBatch spriteBatch) { /* Empty */ }

	public override void PostDrawFullscreenMap(ref string mouseText) { /* Empty */ }

	public override void PostUpdateInput() { /* Empty */ }

	public override void PreSaveAndQuit() { /* Empty */ }
}