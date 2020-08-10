using ExampleMod.Content.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ExampleMod.Common
{
	public class ExamplePlayerDrawLayer : PlayerDrawLayer
	{
		public override bool IsHeadLayer => true; //Makes this layer appear on the minimap player head icon.

		//Setup is called every time a player is rendered, and lets you setup its depth, parent, and whether or not it should be drawn.
		public override bool Setup(Player drawPlayer, IReadOnlyList<PlayerDrawLayer> vanillaLayers) {
			depth = Head.depth - 0.5f; //Set the layer's depth. Layer depth determines the order that layers will get drawn in.
			Parent = Head; //Sets the layer's parent. This layer won't be drawn if its parent is hidden.

			//Return whether or not this layer should be added. In this example, the layer will only be drawn when the player holds an ExampleItem.
			return drawPlayer.HeldItem?.type == ModContent.ItemType<ExampleItem>();
		}

		public override void Draw(ref PlayerDrawSet drawInfo) {
			//The following code draws ExampleItem's texture behind the player's head.

			var exampleItemTexture = ModContent.GetTexture(ItemLoader.GetItem(ModContent.ItemType<ExampleItem>()).Texture).Value;

			drawInfo.DrawDataCache.Add(new DrawData(exampleItemTexture, drawInfo.Center + new Vector2(0f, -20f) - Main.screenPosition, null, Color.White, 0f, exampleItemTexture.Size() * 0.5f, 1f, SpriteEffects.None, 0));
		}
	}
}