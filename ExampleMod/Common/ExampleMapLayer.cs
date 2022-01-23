using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;

namespace ExampleMod.Common
{
	// ModMapLayers are used to draw icons and other things over the map. Pylons and spawn/bed icons are examples of vanilla map layers. This example adds an icon over the dungeon.
	public class ExampleMapLayer : ModMapLayer
	{
		// In the Draw method, we draw everything. Consulting vanilla examples in the source code is a good resource for properly using this Draw method.
		public override void Draw(ref MapOverlayDrawContext context, ref string text) {
			// Here we define the scale that we wish to draw the icon when hovered and not hovered.
			const float scaleIfNotSelected = 1f;
			const float scaleIfSelected = scaleIfNotSelected * 2f;

			// Here we retrieve the texture of the Skeletron boss head so that we can draw it. Remember that not all textures are loaded by default, so you might need to do something like `Main.instance.LoadItem(ItemID.BoneKey);` in your code to ensure the texture is loaded.
			var dungeonTexture = TextureAssets.NpcHeadBoss[19].Value;

			// The MapOverlayDrawContext.Draw method used here handles many of the small details for drawing an icon and should be used if possible. It'll handle scaling, alignment, culling, framing, and accounting for map zoom. Handling these manually is a lot of work.
			// Note that the `position` argument expects tile coordinates expressed as a Vector2. Don't scale tile coordinates to world coordinates by multiplying by 16.
			// The return of MapOverlayDrawContext.Draw has a field that indicates if the mouse is currently over our icon.
			if (context.Draw(dungeonTexture, new Vector2(Main.dungeonX, Main.dungeonY), Color.White, new SpriteFrame(1, 1, 0, 0), scaleIfNotSelected, scaleIfSelected, Alignment.Center).IsMouseOver) {
				// When the icon is being hovered by the users mouse, we set the mouse text to the localized text for "The Dungeon"
				text = Language.GetTextValue("Bestiary_Biomes.TheDungeon");
			}
		}
	}
}