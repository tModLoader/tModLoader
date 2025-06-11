using ExampleMod.Content.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
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
		// Here you can define where to put this layer in the vanilla map layer order.
		// In this case we go before the pings layer since all other vanilla map layers are ordered before pings,
		public override Position GetDefaultPosition() => new Before(IMapLayer.Pings);

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

	// The game doesn't send Main.dungeonX or Main.dungeonY to multiplayer clients.
	// This ModSystem will ensure that they are synced allowing ExampleMapLayer to work in multiplayer.
	public class ExampleMapLayerSystem : ModSystem
	{
		public override void NetSend(BinaryWriter writer) {
			writer.Write(Main.dungeonX);
			writer.Write(Main.dungeonY);
		}
		public override void NetReceive(BinaryReader reader) {
			Main.dungeonX = reader.ReadInt32();
			Main.dungeonY = reader.ReadInt32();
		}
	}

	// This example makes any ExampleItems that are on the ground in the world show up on the map.
	public class ExampleItemMapLayer : ModMapLayer
	{
		public override Position GetDefaultPosition() => new Before(IMapLayer.Pings);

		public override IEnumerable<Position> GetModdedConstraints() {
			// By default, modded map layers are positioned between two vanilla layers (via After and Before) and are ordered in load order.
			// This hook allows you to organize where this map layer is located relative to other modded map layers that are also
			// placed between the same two vanilla map layers.
			// In this case ExampleItems will always show up underneath the dungeon icon.
			yield return new Before(ModContent.GetInstance<ExampleMapLayer>());
		}

		public override void Draw(ref MapOverlayDrawContext context, ref string text) {
			// We can check Main.mapStyle or Main.mapFullscreen to limit drawing to specific map modes.
			// This example doesn't draw on the overlay map, but draws on the minimap and fullscreen map.
			if (Main.mapStyle == 2) {
				return;
			}

			foreach (Item item in Main.ActiveItems) {
				if (item.type == ModContent.ItemType<ExampleItem>()) {
					Texture2D itemTexture = TextureAssets.Item[item.type].Value;

					// item.position is in world coordinates, so divide by 16 to convert it to tile coordinates.
					Vector2 tilePosition = item.position / 16f;

					context.Draw(itemTexture, tilePosition, Alignment.Center);
				}
			}
		}
	}
}